#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Import pipeline for the Kenney "Toon Characters" pack (CC0).
///
/// Everything under Assets/Art/KenneyToonCharacters is imported as a Sprite:
///   * PNG/Poses/*.png        -> Single sprite, pivot = Bottom Center (the feet sit
///                               on the bottom edge of the 96x128 canvas)
///   * Tilesheet/*_sheet.png  -> Multiple sprites, sliced from the sibling .xml atlas
///                               so the Sprite Editor shows the 45 named frames
///
/// Pixels Per Unit is 96 so one 96x128 frame is ~1 world unit tall, which lines up
/// with the 1-unit platforms used in the W3/W4 lab scenes.
/// </summary>
public class KenneyToonCharacterImporter : AssetPostprocessor
{
    private const string Root = "Assets/Art/KenneyToonCharacters";
    private const float PixelsPerUnit = 96f;

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Root))
        {
            return;
        }

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
        importer.SetTextureSettings(settings);

        if (assetPath.EndsWith("_sheet.png"))
        {
            SliceTilesheet(importer);
        }
    }

    /// <summary>Slice a *_sheet.png using the frame rectangles in the sibling .xml atlas.</summary>
    private void SliceTilesheet(TextureImporter importer)
    {
        string xmlPath = assetPath.Substring(0, assetPath.Length - 4) + ".xml";
        if (!File.Exists(xmlPath))
        {
            return;
        }

        // Unity sprite rects use a bottom-left origin; the Kenney atlas XML uses top-left,
        // so we need the texture height to flip every rect. The texture is still being
        // imported at this point, so read the dimensions straight from the PNG header.
        float textureHeight = ReadPngHeight(assetPath);
        if (textureHeight <= 0f)
        {
            return;
        }

        XmlDocument doc = new XmlDocument();
        doc.Load(xmlPath);

        List<SpriteMetaData> frames = new List<SpriteMetaData>();
        foreach (XmlNode node in doc.SelectNodes("//SubTexture"))
        {
            XmlAttributeCollection a = node.Attributes;
            if (a == null)
            {
                continue;
            }

            float x = float.Parse(a["x"].Value);
            float y = float.Parse(a["y"].Value);
            float w = float.Parse(a["width"].Value);
            float h = float.Parse(a["height"].Value);

            frames.Add(new SpriteMetaData
            {
                name = a["name"].Value,
                rect = new Rect(x, textureHeight - (y + h), w, h),
                alignment = (int)SpriteAlignment.BottomCenter,
                pivot = new Vector2(0.5f, 0f)
            });
        }

        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritesheet = frames.ToArray();
    }

    /// <summary>Read the height field out of a PNG IHDR chunk (bytes 20..24, big-endian).</summary>
    private static float ReadPngHeight(string path)
    {
        try
        {
            byte[] header = new byte[24];
            using (FileStream fs = File.OpenRead(path))
            {
                if (fs.Read(header, 0, header.Length) < header.Length)
                {
                    return 0f;
                }
            }

            return (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
        }
        catch (IOException)
        {
            return 0f;
        }
    }
}
#endif
