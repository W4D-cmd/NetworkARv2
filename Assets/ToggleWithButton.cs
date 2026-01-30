using UnityEngine;

public class ToggleWithButton : MonoBehaviour
{
    public OVRInput.Button toggleButton = OVRInput.Button.Two;
    public GameObject target;

    void Update()
    {
        if (OVRInput.GetDown(toggleButton) && target != null)
            target.SetActive(!target.activeSelf);
    }
}