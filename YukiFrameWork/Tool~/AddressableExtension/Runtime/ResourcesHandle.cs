using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using YukiFrameWork.QF;

namespace YukiFrameWork.AddressExtension
{
    public static class AddressableKitExtension
    {
        public static async UniTask<TObject> Wait<TObject>(this AsyncOperationHandle<IList<TObject>> handle,string objName) where TObject : Object
        {
            await handle;
            return WaitSyncPrivate(handle, objName);
            return null;
        }
        
        public static TObject WaitSync<TObject>(this AsyncOperationHandle<IList<TObject>> handle,string objName) where TObject : Object
        {
            if (!handle.IsDone)
            {
                Debug.LogError($"资源 {handle.DebugName} 尚未加载完成，请等待加载完成后再获取资源");
                return null;
            }

            return WaitSyncPrivate(handle, objName);

            return null;
        }
        
        private static TObject WaitSyncPrivate<TObject>(AsyncOperationHandle<IList<TObject>> handle,string objName) where TObject : Object
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                for (var i = 0; i < handle.Result.Count; i++)
                {
                    var item = handle.Result[i];

                    if (item.name == objName)
                        return item;
                }
            }

            return null;
        }

        public static TObject WaitSync<TObject>(this AsyncOperationHandle<TObject> handle) where TObject : Object
        {
            if(!handle.IsDone)
                Debug.LogError($"资源 {handle.DebugName} 尚未加载完成，请等待加载完成后再获取资源");
            
            if(handle.Status == AsyncOperationStatus.Succeeded)
                 return handle.Result;
            
            Debug.LogError("资源加载失败，无法获取资源 " + handle.Status);

            return null;
        }
        
    }
}