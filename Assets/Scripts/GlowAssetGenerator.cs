#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class GlowAssetGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Sci-Fi Glow Texture")]
    public static void GenerateGlowTexture()
    {
        int textureSize = 256;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        
        float innerRadius = 64f; 
        float maxRadius = textureSize / 2f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = 0f;

                if (distance <= innerRadius)
                {
                    alpha = 1f; 
                }
                else
                {
                    float falloff = 1f - ((distance - innerRadius) / (maxRadius - innerRadius));
                    alpha = Mathf.Pow(Mathf.Clamp01(falloff), 2f);
                }

                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        byte[] pngBytes = texture.EncodeToPNG();
        DestroyImmediate(texture);

        string localPath = "Assets/OuterGlowTexture.png";
        string fullPath = Path.Combine(Application.dataPath, "OuterGlowTexture.png");
        
        // 1. Write the raw file raw to the disk
        File.WriteAllBytes(fullPath, pngBytes);
        
        // 2. Force Unity to recognize the file exists before configuring it
        AssetDatabase.Refresh();

        // 3. Now target the asset's importer properties
        TextureImporter importer = AssetImporter.GetAtPath(localPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single; // Explicitly forces the purple sub-asset creation
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spriteBorder = new Vector4(64, 64, 64, 64); // Sets up the 9-slice cuts
            
            // 4. Force a hard reimport to commit changes safely
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        AssetDatabase.Refresh();
        Debug.Log("[GlowAssetGenerator] Solid purple-asset generated cleanly!");
    }
}
#endif