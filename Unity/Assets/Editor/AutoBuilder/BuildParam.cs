using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildParam
{
    /// <summary>
    /// 参数列表 缓存
    /// </summary>
    Dictionary<string, string> param = new Dictionary<string, string>();

    public const string CUSTOMER_PARAM_HEADER = "-customerParam:";
    public static string Args = string.Empty;
    /// <summary>
    /// 获取自定义的命令行参数
    /// </summary>
    /// <returns></returns>
    public static BuildParam GetCustomerParam()
    {
        var result = new BuildParam();

        string[] args;

        if (string.IsNullOrEmpty(Args))
        {
            args = Environment.GetCommandLineArgs();
        }
        else
        {
            args = new string[] { Args };
        }
        if (args != null)
        {
            var param = Array.Find(args, (arg) => arg != null && arg.StartsWith(CUSTOMER_PARAM_HEADER));
            if (param != null)
            {
                param = param.Substring(CUSTOMER_PARAM_HEADER.Length);
                //Debug.Log("Customer Param:" + param);
                var paramGroup = param.Split(',');// 多个参数分割
                if (paramGroup != null)
                {
                  //  var sb = new System.Text.StringBuilder();
                    foreach (var groupItem in paramGroup)
                    {
                        if (groupItem == null) continue;
                        var paramKeyValue = groupItem.Split('^'); //每个参数 key, value
                        if (paramKeyValue != null && paramKeyValue.Length == 2)
                        {
                            result.param.Add(paramKeyValue[0], paramKeyValue[1]);
                       //     sb.AppendFormat("Param  Key:{0}  Value:{1}", paramKeyValue[0].PadRight(20, ' '), paramKeyValue[1]).AppendLine(); ;
                        }
                    }
                  //  Debug.Log(sb);
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 从参数列表中  提取参数
    /// </summary>
    public T Get<T>(string key, T defaultValue = default(T))
    {
        object result = defaultValue;
        string value = null;
        if (param != null && param.TryGetValue(key, out value))
        {
            var valueType = typeof(T);
            if (valueType.IsEnum)
            {
                result = Enum.Parse(valueType, value);
            }
            else if (valueType.Name == "String")
            {
                result = value;
            }
            else
            {
                var parseMethod = valueType.GetMethod("Parse", new Type[] { typeof(string) });
                if (parseMethod != null)
                {
                    result = parseMethod.Invoke(null, new object[] { value });
                }
            }
        }
        return (T)result;
    }

}

public class BuildConfigs
{
    public List<string> replaces;
    public List<SDKItem> sDKItems;
}
public class SDKItem
{
    public string name;

    public List<ReplaceItem> replaceItems;
    //android
    public string keyPath;
    public string keyaliasName;
    public string keyaliasPass;
    public string keystorePass;

    //ios
    public string cameraUsageDescription;
    public string microphoneUsageDescription;

    //commons
    public string identifier;
    public string productName;

    public List<string> modules;
}
public class MudulesItem
{
    public List<ReplaceItem> replaceItems;
}
public class ReplaceItem
{
    public string original;
    public string destinat;
}