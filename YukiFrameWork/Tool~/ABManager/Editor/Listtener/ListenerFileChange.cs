using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using XFABManager;

public class ListenerFileChange : UnityEditor.AssetModificationProcessor {

	// 创建资源
	public static void OnWillCreateAsset(string path)
	{

		EditorApplication.delayCall += () => {
			if (path.EndsWith(".asset") || path.EndsWith(".asset.meta"))
			{
				XFABProjectManager.Instance.RefreshProjects();
			}
		};

         
	}

     

    public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions option)
	{

		EditorApplication.delayCall += () =>
		{
			if (assetPath.EndsWith(".asset"))
			{
				XFABProjectManager.Instance.RefreshProjects();
			}
		};
		  
		if (assetPath.StartsWith("Assets/Resources/XFABManagerSettings"))
		{
			// Assets/Resources/XFABManagerSettings.asset
			EditorUtility.DisplayDialog("提示", "文件:Assets/Resources/XFABManagerSettings.asset 是XFABManager的配置文件,不能删除!否则会影响XFABManager的正常使用!", "确定");
			return AssetDeleteResult.FailedDelete;
		}
		else 
            return AssetDeleteResult.DidNotDelete;
        



        
	}

	// 数字越小 优先级越高
	// 如果返回 true 则认为当前的资源已经处理并且打开了，就不在执行其他的回调了
	// 如果返回 false 认为没有处理，会继续执行其他的回调，如果所有的回调都返回false,则使用系统的方法处理

	// 所以重写该方法时，需要自己处理的资源返回true,不需要自己处理的返回false,这样不会影响到其他的重写方法
	// 并且同一个文件只能有一个回调处理，因为返回true之后就不再执行其他的回调了
	[UnityEditor.Callbacks.OnOpenAsset(0)]
	private static bool OnOpenAsset(int insId, int line) 
	{ 
        XFABProject obj = EditorUtility.InstanceIDToObject(insId) as XFABProject;
		if (obj != null) BaseShowProjects.OpenProject(obj); 
        return obj != null;
	}
	 
}
