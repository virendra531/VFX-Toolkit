using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

namespace FortyWorks.FGAImporter
{
[ScriptedImporter(1, "fga")]
public class FGAImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var fgaFileNameWithPath = ctx.assetPath; // directory + file name + extension // Assets/VFX Toolkit Example/FGA Importer/vel.200.fga
        var justDirectory = Path.GetDirectoryName(fgaFileNameWithPath);
        var fgaFileName = $"{justDirectory}\\{Path.GetFileNameWithoutExtension(fgaFileNameWithPath)}.asset"; //Assets/VFX Toolkit Example/FGA Importer/vel.200.fga

        var fgaContent = File.ReadAllText(fgaFileNameWithPath);

        if (fgaFileNameWithPath.Length == 0 || fgaFileName.Length == 0)
            return;

        var baker = new FGAParser(fgaContent, fgaFileName);
        baker.Parse();
    }
}
}