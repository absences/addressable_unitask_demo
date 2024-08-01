public class PkgInfo
{
    /// <summary>
    /// 参数
    /// </summary>
    public bool GMENABLE;
    /// <summary>
    /// 游戏服地址
    /// </summary>
    public string GameServerAddress;
    /// <summary>
    /// 启用热更
    /// </summary>
    public bool HOT_UPDATE;
    /// <summary>
    /// 资源更新地址
    /// </summary>
    public string ResUrl;
    /// <summary>
    /// 语言
    /// </summary>
    public string Language;
    /// <summary>
    /// 平台
    /// StandaloneWindows64
    /// Android
    /// </summary>
    public string GetPlatformStr;
    /// <summary>
    ///打包类型
    ///dev_1
    ///release_1 区分不同包类型
    /// </summary>
    public string BuildType;
    /// <summary>
    /// 包版本
    /// </summary>
    public int PkgVersion;
    /// <summary>
    /// debug模式
    /// </summary>
    public bool EnableDebug;

    public string SdkType;
}
/// <summary>
/// 新版本信息
/// </summary>
public class VersionInfo
{
    public int newVersionCode;

}