using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;

public enum ESdkType
{
    Standard,
    //  GameIOS,
}
public enum EBuildType
{
    dev,
    release,
}

public enum EBuildTarget
{
    Android,
    StandaloneWindows64,
    iOS,
    WeChat
}

//打包时包内资源策略：
//Small：所有资源上传服务器
//Separatelocal：存本地，remote存服务器
//Whole：所有资源 存本地，

public enum MarksStatus
{
    Small,
    Separate,
    Whole,
}
public class AutoBuilder
{
    public const string WIN = "StandaloneWindows64";
    public const string AND = "Android";
    public const string iOS = "iOS";


    public static void WriteBuildinfo()//根据参数写入打包信息
    {

        var info = BuildParam.GetCustomerParam();
        var version = info.Get("CodeVersion", 1);
        string buildType = info.Get("BuildType", "dev_1");
        string buildTarget = info.Get("BuildPlatform", "StandaloneWindows64");
        string GameServerAddress = info.Get("GameServerAddress", "192.168.3.60:8899");
        string ResServerAddress = info.Get("ResUrl", "http://192.168.1.92/ServerData");
        bool enableLog = info.Get("EnableDebug", false);
        bool hotUpdate = info.Get("HOT_UPDATE", false);
        string SDKType = info.Get("SDKType", "");
        var pkgInfo = new PkgInfo
        {
            Language = info.Get("BuildLanguage", "ch"),
            BuildType = buildType,
            PkgVersion = version,
            ResUrl = ResServerAddress,
            GetPlatformStr = buildTarget,
            GameServerAddress = GameServerAddress,
            EnableDebug = enableLog,
            HOT_UPDATE = hotUpdate,
            SdkType = SDKType
        };
        var str = JsonTool.ToJson(pkgInfo, true);

        var dir = Application.dataPath + "/Resources";

        //保存构建信息
        if(!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(dir +"/pkginfo.json", str);

        dir = string.Format("{0}/../ServerData", Application.dataPath);

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(string.Format("{0}/{1}_{2}", dir, version, "pkginfo.json"), str);

        var scene = EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        var Reporter = GameObject.Find("Reporter");
        if (!enableLog && Reporter != null)
        {
        }
        else if (Reporter == null)
        {
            ReporterEditor .CreateReporter();
            EditorSceneManager.SaveScene(scene);
        }

        if (buildTarget == WIN)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        }
        else if (buildTarget == AND)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }
        else if (buildTarget == iOS)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        }

    }
    static string GetBundleVersion(string bundleVersion)
    {
        const int spt = 3;
        string a = "0", b = "0", c = bundleVersion;
        int length = bundleVersion.Length;
        if (length > spt)
        {
            c = bundleVersion.Remove(0, length - spt);
            b = bundleVersion.Remove(length - spt, spt);
        }
        if (length > 2 * spt)
        {
            a = bundleVersion.Remove(length - spt * 2, spt * 2);
            b = bundleVersion.Remove(0, length - spt * 2).Remove(spt, spt);
        }
        return string.Format("{0}.{1}.{2:D3}", a, b, int.Parse(c));
    }
    public static void BuildPlatformTarget()
    {
        var info = BuildParam.GetCustomerParam();
        string MarkStatus = info.Get("MarkStatus", "Separate");
        var version = info.Get("CodeVersion", 1);
        string buildType = info.Get("BuildType", "Release");
        string buildTarget = info.Get("BuildPlatform", "StandaloneWindows64");
        string ResUrl = info.Get("ResUrl", "http://192.168.1.92/ServerData");
        bool enableLog = info.Get("EnableDebug", false);
        bool hotUpdate = info.Get("HOT_UPDATE", false);
        string SDKType = info.Get("SDKType", "");


        MarksStatus status = MarksStatus.Whole;
        if (MarkStatus == "Small")
            status = MarksStatus.Small;
        else if (MarkStatus == "Separate")
            status = MarksStatus.Separate;

        string bundleVersion = version.ToString();

        PlayerSettings.bundleVersion = GetBundleVersion(bundleVersion) ;//package version x.x.x

        if (buildTarget == AND)
            PlayerSettings.Android.bundleVersionCode = int.Parse(bundleVersion);
        else if (buildTarget == iOS)
            PlayerSettings.iOS.buildNumber = bundleVersion;

        AddressableAssetSettings.CleanPlayerContent();//清理

        AddressableAssetSettings setting = AddressableAssetSettingsDefaultObject.Settings;

        #region  SetAddressableProfile

        var names = setting.profileSettings.GetAllProfileNames();
        if (!names.Contains(buildType))
        {
            setting.profileSettings.AddProfile(buildType, setting.activeProfileId);
        }

        var id = setting.profileSettings.GetProfileId(buildType);

        setting.activeProfileId = id;

        if (!hotUpdate)
        {
            setting.profileSettings.SetValue(setting.activeProfileId, AddressableAssetSettings.kRemoteBuildPath, "[UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget]");
            setting.profileSettings.SetValue(setting.activeProfileId, AddressableAssetSettings.kRemoteLoadPath, "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]");
            setting.BuildRemoteCatalog = false;
        }
        else
        {
            setting.BuildRemoteCatalog = true;

            string name = setting.profileSettings.GetProfileName(setting.activeProfileId);

            string buildPath = string.Format("ServerData/[BuildTarget]/{0}_{1}", version, name);
            //打包输出目录
            setting.profileSettings.SetValue(setting.activeProfileId, AddressableAssetSettings.kRemoteBuildPath, buildPath);
            //远程加载目录
            string loadPath = string.Format("{0}/[BuildTarget]/{1}_{2}", ResUrl, version, name);//加载目录

            setting.profileSettings.SetValue(setting.activeProfileId, AddressableAssetSettings.kRemoteLoadPath, loadPath);
        }
        #endregion

        #region MarkStatus 
        //打包时包内资源策略：
        //Small：所有资源上传服务器
        //Separatelocal：存本地，remote存服务器
        //Whole：所有资源 存本地，

        List<AddressableAssetGroup> deleteList = new List<AddressableAssetGroup>();

        for (int i = 0; i < setting.groups.Count; i++)
        {
            var group = setting.groups[i];

            if (group.Name != "Built In Data")//Default Local Group
            {
                if (group.entries.Count <= 0 || group.Name.StartsWith("UpdateGroup_"))
                {
                    ///删除没有资源的分组与 以往更新的
                    deleteList.Add(group);
                }
                else
                {
                    foreach (var schema in group.Schemas)//group上有多个模式 模块？
                    {
                        if (schema is BundledAssetGroupSchema bundledAssetGroupSchema)//content packing &loading
                        {
                            bool bundleCrc = true;//UseAssetBundleCrc  循环冗余校验 
                            string buildPath = AddressableAssetSettings.kLocalBuildPath;//LocalBuild
                            string loadPath = AddressableAssetSettings.kLocalLoadPath;//LocalLoad

                            if (group.name.StartsWith("Local_"))
                            {
                                bundleCrc = status == MarksStatus.Small;
                                buildPath = bundleCrc ? AddressableAssetSettings.kRemoteBuildPath : AddressableAssetSettings.kLocalBuildPath;
                                loadPath = bundleCrc ? AddressableAssetSettings.kRemoteLoadPath : AddressableAssetSettings.kLocalLoadPath;
                            }
                            else if (group.name.StartsWith("Remoted_"))
                            {
                                bundleCrc = status != MarksStatus.Whole;

                                buildPath = !bundleCrc ? AddressableAssetSettings.kLocalBuildPath : AddressableAssetSettings.kRemoteBuildPath;
                                loadPath = !bundleCrc ? AddressableAssetSettings.kLocalLoadPath : AddressableAssetSettings.kRemoteLoadPath;
                            }
                            else if (group.name.StartsWith("UpdateGroup_"))//始终保持远程的
                            {
                                bundleCrc = true;
                                buildPath = AddressableAssetSettings.kRemoteBuildPath;
                                loadPath = AddressableAssetSettings.kRemoteLoadPath;
                            }

                            bundledAssetGroupSchema.BuildPath.SetVariableByName(group.Settings, buildPath);
                            bundledAssetGroupSchema.LoadPath.SetVariableByName(group.Settings, loadPath);

                            bundledAssetGroupSchema.UseAssetBundleCrc = bundleCrc;
                            bundledAssetGroupSchema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;
                           // bundledAssetGroupSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel;

                        }
                        else if (schema is ContentUpdateGroupSchema updateGroupSchema)
                        {
                            //RemoteStatic:远程静态Group
                            if (group.name.StartsWith("Local_"))
                            {
                                updateGroupSchema.StaticContent = !(status == MarksStatus.Small);  //放服务器  StaticContent=false
                            }
                            else if (group.name.StartsWith("Remoted_"))//远程非静态Group
                            {
                                updateGroupSchema.StaticContent = (status == MarksStatus.Whole);
                            }
                            else if (group.name.StartsWith("UpdateGroup_"))
                            {
                                updateGroupSchema.StaticContent = false;
                            }
                        }
                    }
                }
            }
        }

        for (int i = 0; i < deleteList.Count; i++)
        {
            setting.RemoveGroup(deleteList[i]);
        }
        #endregion


        //buildPlayer

        var target = EditorUserBuildSettings.activeBuildTarget;
        var savePath = "";
        switch (target)
        {
            case BuildTarget.Android:
                {
                   // var config = ImportAndroidSdk(SDKType);
                    savePath = Application.dataPath.Replace("Assets", "AutoBuild") + "/AutoBuild.apk";
                  //  ReplaceCommon(config);
                }
                break;
            case BuildTarget.StandaloneWindows64:
                savePath = Application.dataPath.Replace("Assets", "AutoBuild") + "/clientPC/run game.exe";
                break;
            case BuildTarget.iOS:
                {
                    //var config = ImportIOSSDK(SDKType);
                    savePath = Application.dataPath.Replace("Assets", "AutoBuild") + "/clientIOS";
                   // ReplaceCommon(config);
                }
                break;
        }
        AssetDatabase.Refresh();

        HybridCLREditor.CompileDll();

        AssetDatabase.Refresh();

        //mabe import assets
        AddressableAssetSettings.BuildPlayerContent();//输出

        var levels = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

        BuildPipeline.BuildPlayer(levels, savePath, target, BuildOptions.None);

        if (hotUpdate)//备份content_state.bin
        {
            var bin = ContentUpdateScript.GetContentStateDataPath(false);//  Assets\AddressableAssetsData\Android\addressables_content_state.bin
            var name = Path.GetFileName(bin);
            var bintPath = Application.dataPath.Replace("Assets", "") + bin;

            var disPath = string.Format("{0}/../ServerData/{1}/{2}_{3}/{4}",
                Application.dataPath, buildTarget, version, buildType, name);

            if (File.Exists(disPath))
                File.Delete(disPath);

            FileUtil.CopyFileOrDirectory(bintPath, disPath);
        }
    }


    public static void PreloadForUpdate()
    {
        var info = BuildParam.GetCustomerParam();

        string CodeVersion = info.Get("CodeVersion", "150");
        var path = string.Format("{0}/../ServerData/{1}_{2}",
            Application.dataPath, CodeVersion, "pkginfo.json");

        if(File.Exists(path))
        {
            Debug.Log($"{CodeVersion} not find");
            return;
        }

        var txt = File.ReadAllText(path);
        PkgInfo pinfo = JsonTool.FromJson<PkgInfo>(txt);

        

    }

    public static void BuildHotUpdate()
    {
        //资源更新上传
        var info = BuildParam.GetCustomerParam();

        string CodeVersion = info.Get("CodeVersion", "150");
        //string SERVERDATA = info.Get("SERVERDATA", "");

        var disPath = string.Format("{0}/../ServerData/{1}_{2}",
            Application.dataPath, CodeVersion, "pkginfo.json");

        var txt = File.ReadAllText(disPath);
        PkgInfo pinfo = JsonTool.FromJson<PkgInfo>(txt);

        if (!pinfo.HOT_UPDATE)//no hot update
        {
            Debug.Log($"{CodeVersion} not hot update");
            return;
        }
        if (pinfo.GetPlatformStr == WIN)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        }
        else if (pinfo.GetPlatformStr == AND)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }
        else if (pinfo.GetPlatformStr == iOS)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        }

        //将备份的bin文件放回工程便于读取更新信息
        var bin = ContentUpdateScript.GetContentStateDataPath(false);
        // Assets\AddressableAssetsData\Android\addressables_content_state.bin

        var destFilePath = Application.dataPath.Replace("Assets", "") + bin;

        var sourceFilePath = string.Format("{0}/../ServerData/{1}/{2}_{3}/{4}",
              Application.dataPath, pinfo.GetPlatformStr, pinfo.PkgVersion, pinfo.BuildType, Path.GetFileName(bin));
        if (File.Exists(destFilePath))
            File.Delete(destFilePath);
        FileUtil.CopyFileOrDirectory(sourceFilePath, destFilePath);
        AssetDatabase.Refresh();

        //if (pinfo.GetPlatformStr == WIN)
        //{
        //    IFixEditor.GenPlatformPatch(IFixEditor.Platform.standalone);
        //}
        //else if (pinfo.GetPlatformStr == AND)
        //{
        //    IFixEditor.GenPlatformPatch(IFixEditor.Platform.android);
        //}
        //else if (pinfo.GetPlatformStr == iOS)
        //{
        //    IFixEditor.GenPlatformPatch(IFixEditor.Platform.ios);
        //}
        HybridCLREditor.CompileDll();

        AssetDatabase.Refresh();

        var path = ContentUpdateScript.GetContentStateDataPath(false);

        var setting = AddressableAssetSettingsDefaultObject.Settings;

        var id = setting.profileSettings.GetProfileId(pinfo.BuildType);
        setting.activeProfileId = id;

        {
            setting.BuildRemoteCatalog = true;

            string name = setting.profileSettings.GetProfileName(setting.activeProfileId);
            string buildPath = string.Format("ServerData/[BuildTarget]/{0}_{1}", pinfo.PkgVersion, name);
            //打包输出目录
            setting.profileSettings.SetValue(setting.activeProfileId, AddressableAssetSettings.kRemoteBuildPath, buildPath);
            //远程加载目录
            string loadPath = string.Format("{0}/[BuildTarget]/{1}_{2}", pinfo.ResUrl, pinfo.PkgVersion, name);//加载目录
            setting.profileSettings.SetValue(setting.activeProfileId, AddressableAssetSettings.kRemoteLoadPath, loadPath);
        }

        AddressableEditorTool.CheckForUpdateContent();

        ContentUpdateScript.BuildContentUpdate(setting, path);

        Debug.Log("BuildFinish path = " + setting.RemoteCatalogBuildPath.GetValue(setting));


      //  var original = string.Format("{0}/../ServerData/{1}/{2}_{3}",
       // Application.dataPath, pinfo.GetPlatformStr, pinfo.PkgVersion, pinfo.BuildType);

        // var target = string.Format(SERVERDATA + "/{1}/{2}_{3}",
        //Application.dataPath, pinfo.GetPlatformStr, pinfo.PkgVersion, pinfo.BuildType);
        // MoveFolderToFolder(original, target);
    }
}
