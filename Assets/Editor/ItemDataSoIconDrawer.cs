using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ItemDataSoIconDrawer
{
    static ItemDataSoIconDrawer()
    {
        // Subscribe to the Project Window drawing event
        EditorApplication.projectWindowItemOnGUI += DrawItemIcon;
    }

    private static void DrawItemIcon(string guid, Rect rect)
    {
        // 1. Get the asset path from the GUID
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(assetPath)) return;

        // 2. Optimization: Only load the asset if it's actually an ItemDataSo
        if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) != typeof(ItemDataSo)) return;

        // 3. Load the scriptable object and check for an icon
        ItemDataSo itemData = AssetDatabase.LoadAssetAtPath<ItemDataSo>(assetPath);
        if (itemData == null || itemData.ItemIcon == null || itemData.ItemIcon.Length == 0) return;

        Sprite iconSprite = itemData.ItemIcon[0];
        if (iconSprite == null || iconSprite.texture == null) return;

        Texture2D texture = iconSprite.texture;

        // 4. Calculate the drawing rectangle depending on the Project Window view mode
        bool isListView = rect.height <= 20;
        Rect imageRect;

        if (isListView)
        {
            // List view mode: The icon is tiny and on the left
            imageRect = new Rect(rect.x, rect.y, 16, 16);
        }
        else
        {
            // Grid view mode: The icon is large and sits above the text
            imageRect = new Rect(rect.x, rect.y, rect.width, rect.width);
            
            // Draw a background block to hide the default Unity ScriptableObject icon poking out
            Color bgColor = EditorGUIUtility.isProSkin ? new Color32(51, 51, 51, 255) : new Color32(190, 190, 190, 255);
            EditorGUI.DrawRect(imageRect, bgColor);
        }

        // 5. Handle Sprite Sheets by calculating the exact UV coordinates (slice)
        Rect spriteRect = iconSprite.textureRect;
        Rect uv = new Rect(
            spriteRect.x / texture.width,
            spriteRect.y / texture.height,
            spriteRect.width / texture.width,
            spriteRect.height / texture.height
        );

        // 6. Draw the specific sprite slice directly into the Project window
        GUI.DrawTextureWithTexCoords(imageRect, texture, uv, true);
    }
}