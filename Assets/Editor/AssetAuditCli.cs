#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class AssetAuditCli
{
    [MenuItem("Tools/Holstin/Audit/Dump Character Assets")]
    public static void DumpCharacterAssets()
    {
        string[] assetPaths =
        {
            "Assets/Game/Art/Characters/Source/Ch01_nonPBR@Double Dagger Stab.fbx",
            "Assets/Game/Art/Characters/Source/Ch02_nonPBR@Double Dagger Stab.fbx",
            "Assets/Game/Art/Characters/Source/Ch11_nonPBR@Double Dagger Stab.fbx",
            "Assets/Game/Art/Characters/CharMorph/2K/Antonia/Antonia_2K.fbx",
            "Assets/Game/Art/Characters/CharMorph/2K/MB-Lab_Male/MB-Lab_Male_2K.fbx",
            "Assets/Game/Art/Characters/CharMorph/2K/Vitruvian/Vitruvian_2K.fbx",
            "Assets/Game/Art/Animations/Locomotion/UAL1_Standard.fbx",
            "Assets/Game/Art/Animations/Locomotion/UAL2_Standard.fbx"
        };

        List<string> lines = new List<string>
        {
            "# Character Asset Audit",
            string.Empty
        };

        foreach (string assetPath in assetPaths)
        {
            if (!File.Exists(assetPath))
            {
                lines.Add($"- {assetPath} | missing");
                continue;
            }

            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            int meshCount = subAssets.OfType<Mesh>().Count();
            int animationCount = subAssets.OfType<AnimationClip>().Count(c => !string.Equals(c.name, "__preview__", System.StringComparison.OrdinalIgnoreCase));
            int materialCount = subAssets.OfType<Material>().Count();
            long bytes = new FileInfo(assetPath).Length;

            lines.Add($"- {assetPath}");
            lines.Add($"  - size_bytes: {bytes}");
            lines.Add($"  - meshes: {meshCount}");
            lines.Add($"  - materials: {materialCount}");
            lines.Add($"  - clips: {animationCount}");

            IEnumerable<string> clipNames = subAssets
                .OfType<AnimationClip>()
                .Where(c => !string.Equals(c.name, "__preview__", System.StringComparison.OrdinalIgnoreCase))
                .Select(c => c.name)
                .Distinct()
                .OrderBy(n => n);

            foreach (string clipName in clipNames)
            {
                lines.Add($"    - clip: {clipName}");
            }

            IEnumerable<Mesh> meshes = subAssets
                .OfType<Mesh>()
                .OrderBy(m => m.name);
            foreach (Mesh mesh in meshes)
            {
                lines.Add($"    - mesh: {mesh.name} | verts={mesh.vertexCount} tris={mesh.triangles.Length / 3}");
            }

            IEnumerable<Material> materials = subAssets
                .OfType<Material>()
                .OrderBy(m => m.name);
            foreach (Material material in materials)
            {
                bool hasBaseMap = (material.HasProperty("_BaseMap") && material.GetTexture("_BaseMap") != null) ||
                                  (material.HasProperty("_MainTex") && material.GetTexture("_MainTex") != null);
                lines.Add($"    - material: {material.name} | has_albedo={hasBaseMap}");
            }
        }

        string outputPath = Path.Combine("Assets", "Game", "Docs", "CharacterAssetSubassetAudit.md");
        string outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        File.WriteAllLines(outputPath, lines);
        AssetDatabase.Refresh();
        Debug.Log($"ASSET_AUDIT_WRITTEN: {outputPath}");
    }
}
#endif
