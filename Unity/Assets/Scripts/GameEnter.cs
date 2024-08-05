using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class GameEnter : MonoBehaviour
{
    public Image Image;

    public HotFixComponent hotFixComponent;
    // Start is called before the first frame update
    void Start()
    {
        LoadTest().Forget();

    }
    async UniTask LoadTest()
    {
        await InitAddresable();

        await hotFixComponent.Init();

        var handle = Addressables.LoadAssetAsync<Sprite>("equip/BFSword.png");

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
            Image.sprite = handle.Result;
    }

    private bool _isCheckUpdate;

    List<object> keys;
    private long UpdateSize;
    async UniTask InitAddresable()
    {
        var initHandle = Addressables.InitializeAsync();
        await initHandle.Task;
        if (_isCheckUpdate)
        {
            return;
        }
        _isCheckUpdate = true;

        var _checkHandle = Addressables.CheckForCatalogUpdates(false);
        await _checkHandle.Task;
        AsyncOperationHandle<List<IResourceLocator>> updateHandle = default;
        AsyncOperationHandle<long> downloadsize = default;
        if (_checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            List<string> catalogs = _checkHandle.Result;
            keys = new List<object>();

            if (catalogs != null && catalogs.Count > 0)
            {
                updateHandle = Addressables.UpdateCatalogs(catalogs, false);//更新catalog

                await updateHandle.Task;
            }

            var locators = Addressables.ResourceLocators;

            //遍历所有的key
            foreach (var locator in locators)
            {
                keys.AddRange(locator.Keys);
            }

            downloadsize = Addressables.GetDownloadSizeAsync(keys as IEnumerable);
            await downloadsize.Task;
            UpdateSize = downloadsize.Result;

        }

        if (UpdateSize > 0)
        {
            Debug.Log("开始更新");
            bool wifiEnv = Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
            if (wifiEnv)
            {
                await DownLoadCatalogs((v) =>
                {

                    // Debug.Log(v);
                });
                Debug.Log("更新完成");
            }
            else
            {
                Debug.Log("需要确认");
                //检查到有资源需要更新
                //m= UpdateSize * 1f / 1024 / 1024
                //提示更新
            }
        }
        else
        {
            Debug.Log("无资源更新");
            // OnUpdateEnd?.Invoke();
            // DoClose();
        }
        if (downloadsize.IsValid())
            downloadsize.Release();
        if (updateHandle.IsValid())
            updateHandle.Release();
        _checkHandle.Release();
    }
    async UniTask DownLoadCatalogs(Action<float> progressCallback)
    {
        var downloadHandle = Addressables.DownloadDependenciesAsync(keys as IEnumerable, Addressables.MergeMode.Union);

        var progress = Progress.Create<float>(x =>
        {
            progressCallback?.Invoke(x);
        });
        await downloadHandle.ToUniTask(progress);

        downloadHandle.Release();

        await UniTask.DelayFrame(1);

        keys = null;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
