using System;
using UnityEditor;

public sealed class PitchBlackCreatureAssetPostprocessor : AssetPostprocessor
{
    private const string RakeRoot = "Assets/Resources/ThirdParty/TheRake/";
    private const string RakeModel = RakeRoot + "TheRake.fbx";
    private const string RakeNormal = RakeRoot + "lambert1_Normal_OpenGL.png";

    private void OnPreprocessModel()
    {
        if (!string.Equals(assetPath, RakeModel, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var importer = (ModelImporter)assetImporter;
        importer.importAnimation = true;
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.importBlendShapes = true;
        importer.importCameras = false;
        importer.importLights = false;
        importer.meshCompression = ModelImporterMeshCompression.Medium;
        importer.isReadable = false;
    }

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(RakeRoot, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var importer = (TextureImporter)assetImporter;
        importer.maxTextureSize = 1024;
        importer.mipmapEnabled = true;
        importer.streamingMipmaps = false;
        importer.textureCompression = TextureImporterCompression.Compressed;

        if (string.Equals(assetPath, RakeNormal, StringComparison.OrdinalIgnoreCase))
        {
            importer.textureType = TextureImporterType.NormalMap;
            importer.sRGBTexture = false;
        }
    }
}
