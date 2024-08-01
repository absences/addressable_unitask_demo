//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEditor;
//using UnityEngine;

//public class BatEditor 
//{
//    [MenuItem("tool/run")]
//    public static void Run()
//    {
//        var path = Application.dataPath + "/Build/atlas/planning";

//        var info = new DirectoryInfo(path);
//        var files = info.GetFiles("*.png", SearchOption.AllDirectories);

//        for (int i = 0; i < files.Length; i++)
//        {
//            var file = files[i];

//            var name = file.Name;

//            var index = name.IndexOf('_');

//            // UnityEngine.Debug.Log(name.Remove(0, index+1));

//            FileUtil.MoveFileOrDirectory(file.FullName,


//                path + "/" + name.Remove(0, index + 1));


//        }
//    }
//}
