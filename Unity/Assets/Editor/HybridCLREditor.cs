using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Installer;
using System.IO;
using UnityEditor;
using UnityEngine;

public class HybridCLREditor
{
    [MenuItem("Tools/Compile Mono and AOT Dll")]
    public static void CompileDll()
    {
        var installer = new InstallerController();

        if (!installer.HasInstalledHybridCLR())
        {
            installer.InstallDefaultHybridCLR();
        }

        CompileDllCommand.CompileDll(EditorUserBuildSettings.activeBuildTarget);

        PrebuildCommand.GenerateAll();

        CopyAOTAssembliesTo();

        CopyHotUpdateAssembliesTo();

        //热更新dll需修改FixedSetAssemblyResolver 查找bytes
        //与 搜索路径 Assets/Builds/HotUpdate/Assembly

    }
    static void CopyAOTAssembliesTo()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
        string aotAssembliesDstDir = Application.dataPath + "/Builds/HotUpdate/AOTMetaAssembly";

        if (!Directory.Exists(aotAssembliesDstDir))
        {
            Directory.CreateDirectory(aotAssembliesDstDir);
        }

        foreach (var dll in SettingsUtil.AOTAssemblyNames)//patch aot 补充元数据文件
        {
            string srcDllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
            if (!File.Exists(srcDllPath))
            {
                continue;
            }
            string dllBytesPath = $"{aotAssembliesDstDir}/{dll}.bytes";
            File.Copy(srcDllPath, dllBytesPath, true);
            Debug.Log($"[CopyAOTAssembliesTo] copy AOT dll {srcDllPath} -> {dllBytesPath}");
        }
    }
    static void CopyHotUpdateAssembliesTo()//assembly 
    {
        var target = EditorUserBuildSettings.activeBuildTarget;

        string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        string hotfixAssembliesDstDir = Application.dataPath + "/Builds/HotUpdate/Assembly";
        if (!Directory.Exists(hotfixAssembliesDstDir))
        {
            Directory.CreateDirectory(hotfixAssembliesDstDir);
        }
        foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
        {
            string sourceFilePath = $"{hotfixDllSrcDir}/{dll}";

            if (File.Exists(sourceFilePath))
            {
                string destFilePath = $"{hotfixAssembliesDstDir}/{dll.Replace(".dll", ".bytes")}";

                File.Copy(sourceFilePath, destFilePath, true);
                Debug.Log($"[CopyHotUpdateAssembliesTo] copy hotfix dll {sourceFilePath} -> {destFilePath}");
            }
            string pdbSourcePath= $"{hotfixDllSrcDir}/{dll.Replace(".dll", ".pdb")}";

            if (File.Exists(pdbSourcePath))
            {
                string destFilePath = $"{hotfixAssembliesDstDir}/{dll.Replace(".dll", "_pdb.bytes")}";

                File.Copy(sourceFilePath, destFilePath, true);
            }
        }
    }


}
