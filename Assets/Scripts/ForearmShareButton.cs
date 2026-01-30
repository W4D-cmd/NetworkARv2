using UnityEngine;
using UnityEngine.UI;

public class ForearmShareButton : MonoBehaviour
{
    public MemeMenu memeMenu;
    public int quickSlotIndex = 0;

    void Start()
    {
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnPressed);
        }
    }

    void OnPressed()
    {
        if (memeMenu == null) memeMenu = FindFirstObjectByType<MemeMenu>();
        if (memeMenu == null) return;

        // You need a public method in MemeMenu to spawn by quick slot with technique
        memeMenu.SpawnQuickSlotFromExternal(quickSlotIndex, "FOREARM_MENU");
    }
}