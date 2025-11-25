using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// This editor script helps update meme prefabs to include the new pinch gesture functionality
public class MemePrefabUpdater : MonoBehaviour
{
    [MenuItem("Tools/NetworkARv2/Update Meme Prefabs with Pinch Gesture")]
    public static void UpdateMemePrefabs()
    {
        // Find all meme prefabs in Resources/Memes
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Memes" });
        
        int updatedCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            // Check if the prefab has the required components for pinch gesture
            var memeHandler = prefab.GetComponent<MemeObjectHandler>();
            if (memeHandler != null)
            {
                // Edit the prefab in-place
                GameObject prefabInstance = Object.Instantiate(prefab);
                prefabInstance.name = prefab.name;
                
                // Ensure the PinchGestureHandler component is properly configured
                var pinchHandler = prefabInstance.GetComponent<PinchGestureHandler>();
                if (pinchHandler == null)
                {
                    // Add the component if it doesn't exist
                    pinchHandler = prefabInstance.AddComponent<PinchGestureHandler>();
                }
                
                // Update the MemeObjectHandler settings to enable pinch gestures
                var handler = prefabInstance.GetComponent<MemeObjectHandler>();
                if (handler != null)
                {
                    handler.enablePinchScaling = true;
                    handler.enablePinchRotation = true;
                }
                
                // If the prefab has an XRGrabInteractable component, ensure it's configured properly
                var grabInteractable = prefabInstance.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
                if (grabInteractable == null)
                {
                    // Add XRGrabInteractable if it doesn't exist
                    grabInteractable = prefabInstance.AddComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
                    Debug.Log($"Added XRGrabInteractable to {prefab.name}");
                }
                
                // Save the updated prefab
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
                Object.DestroyImmediate(prefabInstance);
                
                Debug.Log($"Updated meme prefab: {path}");
                updatedCount++;
            }
        }
        
        Debug.Log($"Updated {updatedCount} meme prefabs with pinch gesture functionality.");
    }
    
    [MenuItem("Tools/NetworkARv2/Update Single Meme Prefab")]
    public static void UpdateSingleMemePrefab()
    {
        // Get the selected object in the project window
        GameObject selectedPrefab = Selection.activeObject as GameObject;
        
        if (selectedPrefab == null || !PrefabUtility.IsPartOfPrefabAsset(selectedPrefab))
        {
            Debug.LogWarning("Please select a Meme prefab in the Project window.");
            return;
        }
        
        // Check if the prefab has the required components for pinch gesture
        var memeHandler = selectedPrefab.GetComponent<MemeObjectHandler>();
        if (memeHandler != null)
        {
            // Edit the prefab in-place
            GameObject prefabInstance = Object.Instantiate(selectedPrefab);
            prefabInstance.name = selectedPrefab.name;
            
            // Ensure the PinchGestureHandler component is properly configured
            var pinchHandler = prefabInstance.GetComponent<PinchGestureHandler>();
            if (pinchHandler == null)
            {
                // Add the component if it doesn't exist
                pinchHandler = prefabInstance.AddComponent<PinchGestureHandler>();
            }
            
            // Update the MemeObjectHandler settings to enable pinch gestures
            var handler = prefabInstance.GetComponent<MemeObjectHandler>();
            if (handler != null)
            {
                handler.enablePinchScaling = true;
                handler.enablePinchRotation = true;
            }
            
            // If the prefab has an XRGrabInteractable component, ensure it's configured properly
            var grabInteractable = prefabInstance.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
            if (grabInteractable == null)
            {
                // Add XRGrabInteractable if it doesn't exist
                grabInteractable = prefabInstance.AddComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
                Debug.Log($"Added XRGrabInteractable to {selectedPrefab.name}");
            }
            
            // Save the updated prefab
            string path = AssetDatabase.GetAssetPath(selectedPrefab);
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
            Object.DestroyImmediate(prefabInstance);
            
            Debug.Log($"Updated meme prefab: {path}");
        }
        else
        {
            Debug.LogWarning($"Selected prefab does not have MemeObjectHandler component: {selectedPrefab.name}");
        }
    }
}