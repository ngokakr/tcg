using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureImageImporter : AssetPostprocessor
{
	static readonly string[] targetExtensions = { ".tga" };

	void OnPreprocessTexture ()
	{
//		bool isValidExtension = false;
//		foreach (var extension in targetExtensions) {
//			if (Path.GetExtension (assetPath).ToLower ().Equals (extension)) {
//				isValidExtension = true;
//				break;
//			}
//		}
//
//		if (!isValidExtension) {
//			return;
//		}

		var importer = assetImporter as TextureImporter;
		importer.textureType = TextureImporterType.Sprite;
		importer.spriteImportMode = SpriteImportMode.Single;

//		importer.npotScale = TextureImporterNPOTScale.None;
//		importer.alphaIsTransparency = true;
//		importer.mipmapEnabled = false;
//		importer.lightmap = false;
//		importer.normalmap = false;
//		importer.linearTexture = false;
//		importer.wrapMode = TextureWrapMode.Repeat;
//		importer.generateCubemap = TextureImporterGenerateCubemap.None;
	}
}