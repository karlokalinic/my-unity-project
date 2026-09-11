using System;
using UnityEditor;

public sealed class PlayerHumanoidAssetPostprocessor : AssetPostprocessor
{
    internal const string PlayerModelPath = "Assets/Resources/Player/Ch01_nonPBR@Double Dagger Stab.fbx";

    private void OnPreprocessModel()
    {
        if (!string.Equals(assetPath, PlayerModelPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var importer = (ModelImporter)assetImporter;

        // The player renderer is driven bone-by-bone at runtime. Keep the real Humanoid hierarchy
        // exposed, retain facial/body deformation data, and strip unrelated scene payload.
        importer.importAnimation = true;
        importer.animationType = ModelImporterAnimationType.Human;
        importer.importBlendShapes = true;
        importer.importCameras = false;
        importer.importLights = false;
        importer.meshCompression = ModelImporterMeshCompression.Off;
        importer.isReadable = false;
        importer.optimizeGameObjects = false;
    }
}
