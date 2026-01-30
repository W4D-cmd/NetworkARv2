#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// This editor script helps update meme prefabs to include the new pinch gesture functionality
public static class MemePrefabUpdater
{
    [MenuItem("Tools/NetworkARv2/Update Meme Prefabs with Pinch Gesture")]
    public static void UpdateMemePrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Memes" });
        int updatedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            if (!prefab.TryGetComponent(out MemeObjectHandler handler))
                continue;

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            if (!instance.TryGetComponent(out PinchGestureHandler pinch))
                instance.AddComponent<PinchGestureHandler>();

            handler = instance.GetComponent<MemeObjectHandler>();
            handler.enablePinchScaling = true;
            handler.enablePinchRotation = true;

            if (!instance.TryGetComponent(
                out UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _))
            {
                instance.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            }

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            updatedCount++;
        }

        Debug.Log($"Updated {updatedCount} meme prefabs with pinch gesture functionality.");
    }

    [MenuItem("Tools/NetworkARv2/Update Single Meme Prefab")]
    public static void UpdateSingleMemePrefab()
    {
        if (!(Selection.activeObject is GameObject prefab) ||
            !PrefabUtility.IsPartOfPrefabAsset(prefab))
        {
            Debug.LogWarning("Please select a Meme prefab in the Project window.");
            return;
        }

        if (!prefab.TryGetComponent(out MemeObjectHandler _))
        {
            Debug.LogWarning($"Selected prefab does not have MemeObjectHandler: {prefab.name}");
            return;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        if (!instance.TryGetComponent(out PinchGestureHandler _))
            instance.AddComponent<PinchGestureHandler>();

        var handler = instance.GetComponent<MemeObjectHandler>();
        handler.enablePinchScaling = true;
        handler.enablePinchRotation = true;

        if (!instance.TryGetComponent(
            out UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _))
        {
            instance.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        string path = AssetDatabase.GetAssetPath(prefab);
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);

        Debug.Log($"Updated meme prefab: {path}");
    }
}
#endif