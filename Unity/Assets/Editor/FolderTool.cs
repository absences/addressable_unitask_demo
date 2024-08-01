using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FolderTool
{
    [MenuItem("Tools/OpenFolder/persistentDataPath")]
    public static void OpenpersistentDataPath()
    {
        RevealInFinder(Application.persistentDataPath);
    }

    [MenuItem("Tools/OpenFolder/temporaryPath")]
    public static void OpentemporaryCachePathDataPath()
    {
        RevealInFinder(Application.temporaryCachePath);
    }


    private static void RevealInFinder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            UnityEngine.Debug.LogWarning(string.Format("Folder '{0}' is not Exists", folder));
            return;
        }

        EditorUtility.RevealInFinder(folder);
    }
}
