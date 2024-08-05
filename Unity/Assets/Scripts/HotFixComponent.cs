using Cysharp.Threading.Tasks;
using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
public class HotFixComponent : MonoBehaviour
{

    readonly List<string> aotMetaAssemblyFiles = new List<string>()
    {
        "mscorlib",
        "System",
        "System.Core",
        "Newtonsoft.Json",
       // "Google.Protobuf",
    };

    private bool Inited = false;

    public async UniTask Init()
    {
#if !UNITY_EDITOR
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in aotMetaAssemblyFiles)
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>(string.Format("AOTMetaAssembly/{0}.bytes", aotDllName));

            await handle.ToUniTask(this);

            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                RuntimeApi.LoadMetadataForAOTAssembly(handle.Result.bytes, mode);
           
            Debug.Log(string.Format("LoadMetadata{0} {1}", aotDllName, handle.Status));
            handle.Release();
        }
#endif
        {

#if UNITY_EDITOR
            var assembly = GameFramework.Utility.Assembly.GetAssemblies().First(a => a.GetName().Name == "GameAssembly");
            s_Assemblies[0] = assembly;
            gameObject.AddComponent(assembly.GetType("Enter"));
#else
            var handle = Addressables.LoadAssetAsync<TextAsset>("Assembly/GameAssembly.bytes");//Assemblie 
            await handle.ToUniTask(this);

            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                var bytes = handle.Result.bytes;

                var handle2 = Addressables.LoadAssetAsync<TextAsset>("Assembly/GameAssembly_pdb.bytes");
                await handle2.ToUniTask(this);
                Assembly assembly = null;
                if (handle2.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                   assembly = Assembly.Load(bytes, handle2.Result.bytes);
                }
                else
                {
                    assembly = Assembly.Load(bytes);
                }
                s_Assemblies[0] = assembly;
                gameObject.AddComponent(assembly.GetType("Enter"));

                handle2.Release();
            }
            handle.Release();
#endif
        }
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>("Assembly/HybridProject.bytes");
            await handle.ToUniTask(this);

            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                var bytes = handle.Result.bytes;

                s_Assemblies[1] = Assembly.Load(bytes);

                Type entryType = s_Assemblies[1].GetType("HybridProject.Enter");
                MethodInfo method = entryType.GetMethod("Init");
                method.Invoke(null, null);

                var updatemethod = entryType.GetMethod("Update");
                updateAction = Delegate.CreateDelegate(typeof(Action<float, float>), null, updatemethod) as Action<float, float>;

                var onDestroy = entryType.GetMethod("ShutDown");
                onDestroyAction = Delegate.CreateDelegate(typeof(Action), null, onDestroy) as Action;

             
            }
            handle.Release();
            //else
            // Log.Warning(handle.LastError);

            //Type netparseentryType = assembly.GetType("HotfixMain.ILNetHelper");
            //var netparse = netparseentryType.GetMethod("HandleMsg");

            //netParseHandle = (Action<int, CodedInputStream>)Delegate.CreateDelegate(typeof(Action<int, CodedInputStream>), null, netparse);
        }

        Inited = true;
    }
    private Action<float, float> updateAction;
    private Action onDestroyAction;

    private readonly Assembly[] s_Assemblies = new Assembly[2];

    private void Update()
    {
        if (Inited)
        {
            updateAction?.Invoke(Time.deltaTime, Time.realtimeSinceStartup);
        }
    }

    private void OnDestroy()
    {
        if (Inited)
        {
            onDestroyAction?.Invoke();
        }
    }
}
