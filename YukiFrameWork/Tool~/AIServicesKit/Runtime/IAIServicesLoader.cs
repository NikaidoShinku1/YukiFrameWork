using System;
using XFABManager;
using YukiFrameWork;
namespace YukiFrameWork.AI
{
    public interface IAIServicesLoader : IResLoader<AIServicesConfig>
    {
        
    }

    public class ABManagerServicesLoader : IAIServicesLoader
    {
        private readonly string projectName;

        public ABManagerServicesLoader(string projectName)
        {
            this.projectName = projectName; 
        }

        public TItem Load<TItem>(string name) where TItem : AIServicesConfig
        {
            return AssetBundleManager.LoadAsset<TItem>(projectName,name);
        }

        public async void LoadAsync<TItem>(string name, Action<TItem> onCompleted) where TItem : AIServicesConfig
        {
            var result = await AssetBundleManager.LoadAssetAsync<TItem>(projectName, name);
            onCompleted?.Invoke(result);
        }

        public void UnLoad(AIServicesConfig item)
        {
            AssetBundleManager.UnloadAsset(item);
        }
    }
}