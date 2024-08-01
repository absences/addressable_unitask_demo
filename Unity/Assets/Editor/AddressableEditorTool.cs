using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AddressableEditorTool
{
    /// <summary>
    /// 出包前 打包资源
    /// </summary>
    [MenuItem("Tools/Addressable/New Build")]
    public static void BuildContent()
    {
        AddressableAssetSettings.BuildPlayerContent();

    }
    [MenuItem("Tools/Addressable/更新检测")]
    public static void CheckForUpdateContent()
    {
        //与上次打包做资源对比
        string buildPath = ContentUpdateScript.GetContentStateDataPath(false);
        var m_Settings = AddressableAssetSettingsDefaultObject.Settings;
        List<AddressableAssetEntry> entrys =ContentUpdateScript.GatherModifiedEntries(m_Settings, buildPath);
        if (entrys.Count == 0) return;

        StringBuilder sbuider = new StringBuilder();
        sbuider.AppendLine("Need Update Assets:");
        foreach (var _ in entrys)
        {
            sbuider.AppendLine(_.address);
        }
        Debug.Log(sbuider.ToString());

        //将被修改过的资源单独分组
        var groupName = string.Format("UpdateGroup_{0}", DateTime.Now.ToString("yyyyMMddhhmmss"));
        ContentUpdateScript.CreateContentUpdateGroup(m_Settings, entrys, groupName);
    }
    /// <summary>
    /// 输出当前配置信息
    /// </summary>
    [MenuItem("Tools/Addressable/Debug_Info")]

    public static void LogPathInfo()
    {
        //Project​Config​Data.PostProfilerEvents = true;
        Debug.Log("BuildPath = " + Addressables.BuildPath);
        Debug.Log("PlayerBuildDataPath = " + Addressables.PlayerBuildDataPath);

        Debug.Log("GetContentStateDataPath = " + ContentUpdateScript.GetContentStateDataPath(false));//  Assets\AddressableAssetsData\Android

        Debug.Log("RemoteCatalogBuildPath = " + AddressableAssetSettingsDefaultObject.Settings.RemoteCatalogBuildPath.GetValue(AddressableAssetSettingsDefaultObject.Settings));
    }
}
