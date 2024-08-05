using System;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildWindowEditor : EditorWindow
{/// <summary>
 /// 停靠窗口类型集合
 /// </summary>
    public static readonly Type[] DockedWindowTypes =
    {
       typeof(BuildWindowEditor),
    };

    [MenuItem("Tools/Addressable Build Window", false, 4)]
    public static void ShowWindow()
    {
        BuildWindowEditor window = GetWindow<BuildWindowEditor>("构建工具", true, DockedWindowTypes);
        window.minSize = new Vector2(600, 450);
        window.maxSize = new Vector2(600, 450);
    }

    private EnumField _buildTargetField;
    private EnumField _buildSdkType;
    private EnumField _buildVersion;
    private EnumField _buildMarkStatus;
    private TextField _buildResServerAddressField;
    private TextField _buildGameServerAddressField;
    private Toggle _showDebugToogle;
    private Toggle _showHOTUPDATE;

    private IntegerField _buildCodeVersionField;
    private TextField _buildOutputField;
    public void CreateGUI()
    {
        try
        {
            VisualElement root = this.rootVisualElement;

            var visualAsset = EditorHelper.LoadWindowUXML<BuildWindowEditor>();
            if (visualAsset == null)
                return;

            visualAsset.CloneTree(root);

            _buildTargetField = root.Q<EnumField>("BuildPlatform");

            _buildTargetField.Init(EBuildTarget.StandaloneWindows64);

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows64:
                    _buildTargetField.SetValueWithoutNotify(EBuildTarget.StandaloneWindows64);
                    break;
                case BuildTarget.Android:
                    _buildTargetField.SetValueWithoutNotify(EBuildTarget.Android);
                    break;
                case BuildTarget.iOS:
                    _buildTargetField.SetValueWithoutNotify(EBuildTarget.iOS);
                    break;
                case BuildTarget.WebGL:
                    _buildTargetField.SetValueWithoutNotify(EBuildTarget.WeChat);
                    break;
            }
            _buildTargetField.style.width = 350;
            _buildTargetField.RegisterValueChangedCallback(evt =>
            {
                var value = (EBuildTarget)_buildTargetField.value;
                switch (value)
                {
                    case EBuildTarget.Android:
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                        break;
                    case EBuildTarget.StandaloneWindows64:
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                        break;
                    case EBuildTarget.iOS:
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
                        break;
                }
                RefreshWindow();
            });

            _buildSdkType = root.Q<EnumField>("SDKType");
            _buildSdkType.Init(ESdkType.Standard);
            _buildSdkType.style.width = 350;
            _buildSdkType.SetValueWithoutNotify(ESdkType.Standard);
            _buildSdkType.RegisterValueChangedCallback((val) => {
                RefreshWindow();
            });

            _buildVersion = root.Q<EnumField>("BuildType");
            _buildVersion.Init(EBuildType.dev);
            _buildVersion.style.width = 350;
            _buildVersion.SetValueWithoutNotify(EBuildType.dev);
            _buildVersion.RegisterValueChangedCallback((val) => {
                RefreshWindow();
            });

            _buildMarkStatus = root.Q<EnumField>("MarksStatus");
            _buildMarkStatus.Init(MarksStatus.Separate);
            _buildMarkStatus.SetValueWithoutNotify(MarksStatus.Separate);
            _buildMarkStatus.style.width = 350;
            _buildMarkStatus.RegisterValueChangedCallback((val) => {
                RefreshWindow();
            });

            _buildResServerAddressField = root.Q<TextField>("ResUrl");
            _buildResServerAddressField.SetValueWithoutNotify("http://192.168.1.92/ServerData");
            _buildResServerAddressField.RegisterValueChangedCallback((val) => {
                RefreshWindow();
            });

            _buildGameServerAddressField = root.Q<TextField>("GameServerAddress");
            _buildGameServerAddressField.SetValueWithoutNotify("192.168.1.92:8899");
            _buildGameServerAddressField.RegisterValueChangedCallback((val) => {
                RefreshWindow();
            });

            _showDebugToogle = root.Q<Toggle>("EnableDebug");
            _showDebugToogle.SetValueWithoutNotify(true);
            _showDebugToogle.RegisterValueChangedCallback((val) => {
                RefreshWindow();
            });

            _showHOTUPDATE = root.Q<Toggle>("HOT_UPDATE");
            _showHOTUPDATE.SetValueWithoutNotify(true);
            _showHOTUPDATE.RegisterValueChangedCallback((val) => {
                RefreshWindow();
            });

            _buildCodeVersionField = root.Q<IntegerField>("CodeVersion");
            _buildCodeVersionField.SetValueWithoutNotify(100);
            _buildCodeVersionField.RegisterValueChangedCallback((val) => {
                RefreshWindow();
            });

            _buildOutputField = root.Q<TextField>("BuildParam");
            _buildOutputField.SetEnabled(false);

            var buildButton = root.Q<Button>("Build");
            buildButton.clicked += BuildButton_clicked;
            
            var button = root.Q<Button>("BuildHot");
            button.clicked += BuildHotUpdate;

            RefreshWindow();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
    void BuildButton_clicked()
    {
        BuildParam.Args = GetParam();

        AutoBuilder.WriteBuildinfo();
        AutoBuilder.BuildPlatformTarget();
    }
    void BuildHotUpdate()
    {
        BuildParam.Args = GetParam();
        AutoBuilder.BuildHotUpdate();
    }
    private void RefreshWindow()
    {
        //	_compressionField.SetEnabled(enableElement);  //使之不被选中
        //_copyBuildinFileTagsField.visible = tagsFiledVisible;

        _buildOutputField.SetValueWithoutNotify(GetParam());
    }
    public string GetParam()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("-customerParam:");
        sb.Append(_buildTargetField.name);
        sb.Append('^');
        sb.Append(_buildTargetField.value);
        sb.Append(',');

        sb.Append(_buildSdkType.name);
        sb.Append('^');
        sb.Append(_buildSdkType.value);
        sb.Append(',');

        sb.Append(_buildVersion.name);
        sb.Append('^');
        sb.Append(_buildVersion.value);
        sb.Append(',');

        sb.Append(_buildMarkStatus.name);
        sb.Append('^');
        sb.Append(_buildMarkStatus.value);
        sb.Append(',');
        
        sb.Append(_buildResServerAddressField.name);
        sb.Append('^');
        sb.Append(_buildResServerAddressField.value);
        sb.Append(',');
        
        sb.Append(_buildGameServerAddressField.name);
        sb.Append('^');
        sb.Append(_buildGameServerAddressField.value);
        sb.Append(','); 
        
        sb.Append(_showDebugToogle.name);
        sb.Append('^');
        sb.Append(_showDebugToogle.value);
        sb.Append(','); 
        
        sb.Append(_showHOTUPDATE.name);
        sb.Append('^');
        sb.Append(_showHOTUPDATE.value);
        sb.Append(',');
        
        sb.Append(_buildCodeVersionField.name);
        sb.Append('^');
        sb.Append(_buildCodeVersionField.value);
        sb.Append(',');
        return sb.ToString();
    }
}
