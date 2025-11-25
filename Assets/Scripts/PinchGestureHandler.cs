using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using Unity.Netcode;

public class PinchGestureHandler : NetworkBehaviour
{
    [Header("Pinch Settings")]
    [Tooltip("Threshold distance for pinch detection")]
    public float pinchThreshold = 0.03f;
    
    [Tooltip("Threshold distance for unpinch detection")]
    public float unpinchThreshold = 0.05f;
    
    [Header("Scaling Settings")]
    [Tooltip("Speed multiplier for scaling operations")]
    public float scaleSpeed = 1.0f;
    
    [Tooltip("Minimum allowed scale")]
    public float minScale = 0.1f;
    
    [Tooltip("Maximum allowed scale")]
    public float maxScale = 5.0f;
    
    [Header("Rotation Settings")]
    [Tooltip("Speed multiplier for rotation operations")]
    public float rotationSpeed = 100.0f;

    private XRGrabInteractable grabInteractable;
    private Transform initialGrabPoint;
    private float initialGrabDistance;
    private float initialScaleFactor;
    private bool isPinchScaling = false;
    private bool isTwoHandRotating = false;
    private Transform initialTransform;
    private Vector3 initialLocalScale;
    private Vector3 initialGrabPosition1;
    private Vector3 initialGrabPosition2;
    private Vector3 initialObjectPosition;
    private Quaternion initialObjectRotation;
    private Quaternion initialGrabRotation1;
    private Quaternion initialGrabRotation2;
    private Quaternion initialObjectLocalRotation;
    private float initialPinchDistance;
    private float currentPinchStrength;
    private Logger logger;

    void Start()
    {
        if (!IsOwner) return; // Only owner handles input
        
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEntered);
            grabInteractable.selectExited.AddListener(OnSelectExited);
        }
        
        // Get reference to logger if available
        var memeHandler = GetComponent<MemeObjectHandler>();
        if (memeHandler != null)
        {
            logger = memeHandler.GetComponent<MemeNetworkManager>().logger;
        }
    }

    private void OnSelectEntered(SelectEnteredEventArgs args)
    {
        if (!IsOwner) return;
        
        logger?.LogDebug($"Pinch gesture started for object: {gameObject.name}", nameof(PinchGestureHandler));
        
        // Store initial transform state
        initialTransform = transform;
        initialLocalScale = transform.localScale;
        initialObjectPosition = transform.position;
        initialObjectRotation = transform.rotation;
        initialObjectLocalRotation = transform.localRotation;
        
        var interactors = grabInteractable.selectingInteractors;
        
        if (interactors.Count == 1)
        {
            // Single hand grab - potentially for pinch scaling
            var interactor = interactors[0];
            if (interactor is XRDirectInteractor directInteractor)
            {
                initialGrabPoint = directInteractor.transform;
                initialGrabPosition1 = directInteractor.transform.position;
                isPinchScaling = true;
            }
        }
        else if (interactors.Count >= 2)
        {
            // Two-handed grab - for rotation or scaling
            var interactor1 = interactors[0];
            var interactor2 = interactors[1];
            
            if (interactor1 is XRDirectInteractor directInteractor1 && 
                interactor2 is XRDirectInteractor directInteractor2)
            {
                initialGrabPosition1 = directInteractor1.transform.position;
                initialGrabPosition2 = directInteractor2.transform.position;
                initialGrabRotation1 = directInteractor1.transform.rotation;
                initialGrabRotation2 = directInteractor2.transform.rotation;
                
                initialGrabDistance = Vector3.Distance(initialGrabPosition1, initialGrabPosition2);
                initialScaleFactor = transform.localScale.x;  // Assuming uniform scaling
                
                // Determine if it's a rotation gesture (hands on opposite sides) or scaling gesture (hands close together)
                Vector3 objectToHand1 = initialGrabPosition1 - transform.position;
                Vector3 objectToHand2 = initialGrabPosition2 - transform.position;
                
                float dotProduct = Vector3.Dot(objectToHand1.normalized, objectToHand2.normalized);
                
                // If hands are on opposite sides of the object, it's likely a rotation gesture
                if (dotProduct < -0.5f)
                {
                    isTwoHandRotating = true;
                    isPinchScaling = false;
                }
                else
                {
                    isPinchScaling = true;
                    isTwoHandRotating = false;
                }
            }
        }
    }

    private void OnSelectExited(SelectExitedEventArgs args)
    {
        if (!IsOwner) return;
        
        logger?.LogDebug($"Pinch gesture ended for object: {gameObject.name}", nameof(PinchGestureHandler));
        
        isPinchScaling = false;
        isTwoHandRotating = false;
        
        // Save state when grab ends
        var memeHandler = GetComponent<MemeObjectHandler>();
        if (memeHandler != null)
        {
            memeHandler.SaveState();
        }
    }

    void Update()
    {
        if (!IsOwner || !grabInteractable || grabInteractable.selectingInteractors.Count == 0)
            return;

        var interactors = grabInteractable.selectingInteractors;

        if (interactors.Count >= 2 && isPinchScaling)
        {
            // Two-handed scale operation
            var interactor1 = interactors[0];
            var interactor2 = interactors[1];

            if (interactor1 is XRDirectInteractor directInteractor1 &&
                interactor2 is XRDirectInteractor directInteractor2)
            {
                Vector3 currentGrabPosition1 = directInteractor1.transform.position;
                Vector3 currentGrabPosition2 = directInteractor2.transform.position;

                float currentDistance = Vector3.Distance(currentGrabPosition1, currentGrabPosition2);
                float distanceRatio = currentDistance / initialGrabDistance;

                // Apply scaling with constraints
                float newScale = Mathf.Clamp(initialScaleFactor * distanceRatio, minScale, maxScale);

                if (transform.localScale.x != newScale)
                {
                    // Apply uniform scaling
                    Vector3 newScaleVec = Vector3.one * newScale;
                    transform.localScale = newScaleVec;

                    // Send the new scale to other clients via the network transform
                    if (IsServer)
                    {
                        ApplyTransformServerRpc(transform.position, transform.rotation, transform.localScale);
                    }
                    else
                    {
                        ApplyTransformServerRpc(transform.position, transform.rotation, transform.localScale);
                    }
                }
            }
        }
        else if (isTwoHandRotating && interactors.Count >= 2)
        {
            // Two-handed rotation operation
            var interactor1 = interactors[0];
            var interactor2 = interactors[1];

            if (interactor1 is XRDirectInteractor directInteractor1 &&
                interactor2 is XRDirectInteractor directInteractor2)
            {
                Vector3 currentGrabPosition1 = directInteractor1.transform.position;
                Vector3 currentGrabPosition2 = directInteractor2.transform.position;

                // Calculate rotation based on hand movement
                Vector3 initialVector = initialGrabPosition2 - initialGrabPosition1;
                Vector3 currentVector = currentGrabPosition2 - currentGrabPosition1;

                Quaternion rotationDelta = Quaternion.FromToRotation(initialVector, currentVector);

                // Apply rotation
                transform.rotation = rotationDelta * initialObjectRotation;

                // Send the new rotation to other clients via the network transform
                if (IsServer)
                {
                    ApplyTransformServerRpc(transform.position, transform.rotation, transform.localScale);
                }
                else
                {
                    ApplyTransformServerRpc(transform.position, transform.rotation, transform.localScale);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyTransformServerRpc(Vector3 position, Quaternion rotation, Vector3 scale, ServerRpcParams rpcParams = default)
    {
        // Validate the transform values to prevent cheating
        if (!IsValidTransform(position, rotation, scale))
        {
            logger?.LogWarning("Received invalid transform from client, ignoring.", nameof(PinchGestureHandler));
            return;
        }

        // Server updates the values and syncs to all clients
        ApplyTransformClientRpc(position, rotation, scale, rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void ApplyTransformClientRpc(Vector3 position, Quaternion rotation, Vector3 scale, ulong senderClientId)
    {
        // Apply the transform change to all clients
        // Only apply if this client doesn't own the object
        // or if the update came from the current owner
        if (!IsOwner || NetworkManager.Singleton.LocalClientId == senderClientId)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;
        }
    }

    // Validate transform values to prevent cheating or invalid states
    private bool IsValidTransform(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        // Check if any values are invalid (NaN or infinity)
        if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z) ||
            float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z) ||
            float.IsNaN(rotation.x) || float.IsNaN(rotation.y) || float.IsNaN(rotation.z) || float.IsNaN(rotation.w) ||
            float.IsInfinity(rotation.x) || float.IsInfinity(rotation.y) || float.IsInfinity(rotation.z) || float.IsInfinity(rotation.w) ||
            float.IsNaN(scale.x) || float.IsNaN(scale.y) || float.IsNaN(scale.z) ||
            float.IsInfinity(scale.x) || float.IsInfinity(scale.y) || float.IsInfinity(scale.z))
        {
            return false;
        }

        // Check if scale is within reasonable bounds
        if (scale.x < 0.01f || scale.x > 100f || scale.y < 0.01f || scale.y > 100f || scale.z < 0.01f || scale.z > 100f)
        {
            return false;
        }

        return true;
    }

    // Optional: Method to directly initiate pinch scaling from other components
    public void StartPinchScaling()
    {
        if (!IsOwner) return;
        
        var memeHandler = GetComponent<MemeObjectHandler>();
        if (memeHandler != null && memeHandler.IsOwner)
        {
            logger?.LogDebug($"Programmatically starting pinch scaling for: {gameObject.name}", nameof(PinchGestureHandler));
            isPinchScaling = true;
            initialLocalScale = transform.localScale;
        }
    }

    // Optional: Method to check if currently in pinch scaling mode
    public bool IsCurrentlyScaling()
    {
        return isPinchScaling;
    }
}