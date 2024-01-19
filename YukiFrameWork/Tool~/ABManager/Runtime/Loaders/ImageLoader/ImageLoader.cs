using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI; 


namespace YukiFrameWork.ABManager
{

    [Serializable]
    public enum ImageLoaderType {
        /// <summary>
        /// 网络图片
        /// </summary>
        [Tooltip("从网络加载")]
        Network,
        /// <summary>
        /// 本地图片
        /// </summary>
        [Tooltip("从本地加载")]
        Local,

        /// <summary>
        /// AssetBundle中的图片
        /// </summary>
        [Tooltip("从AssetBundle加载")]
        AssetBundle
    }

    [Serializable]
    public struct ImageModel 
    {
        /// <summary>
        /// 图片加载方式
        /// </summary>
        [Tooltip("图片加载方式")]
        public ImageLoaderType type;
        /// <summary>
        /// 如果加载类型为 ImageLoaderType.Network 则path为网络图片地址 
        /// 如果加载类型为 ImageLoaderType.Local 则path为本地图片路径
        /// 如果加载类型为 ImageLoaderType.AssetBundle 则path字段无效
        /// </summary>
        [Tooltip("如果加载类型为 Network 则path为网络图片地址 \n如果加载类型为 Local 则path为本地图片路径 \n如果加载类型为 AssetBundle 则path字段无效 ")]
        public string path;

        /// <summary>
        /// 如果加载类型为 ImageLoaderType.AssetBundle 则 projectName 为资源所在的模块名称
        /// 如果加载类型为 其他 , 则该字段无效
        /// </summary>
        [Tooltip("如果加载类型为 AssetBundle 则 projectName 为资源所在的模块名称 \n如果加载类型为 其他 , 则该字段无效")]
        public string projectName;
        /// <summary>
        /// 如果加载类型为 ImageLoaderType.AssetBundle 则 assetName 为资源名
        /// 如果加载类型为 其他 , 则该字段无效
        /// </summary>
        [Tooltip("如果加载类型为AssetBundle则assetName为资源名\n如果加载类型为其他,则该字段无效")]
        public string assetName;

        [Tooltip("加载类型")]
        public ImageLoadType load_type;

        public string Key
        {
            get 
            {
                if (type == ImageLoaderType.AssetBundle)
                    return string.Format("{0}/{1}",projectName,assetName);
                else 
                    return string.Format("{0}/{1}",type ,path);
            }
        }

        public override bool Equals(object obj)
        {
            ImageModel other = (ImageModel)obj; 
            if (other.type != this.type) return false;

            switch (other.type)
            {
                case ImageLoaderType.Network:
                case ImageLoaderType.Local:
                    return other.path == this.path;
                case ImageLoaderType.AssetBundle:
                    return other.projectName == this.projectName && other.assetName == this.assetName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool IsEmpty() {
            if (type == ImageLoaderType.Local || type == ImageLoaderType.Network)
                return string.IsNullOrEmpty(path); 
            else if(type == ImageLoaderType.AssetBundle)
                return string.IsNullOrEmpty(assetName) || string.IsNullOrEmpty(projectName);
             
            return true;
        }

    }


    /// <summary>
    /// 目标组件类型
    /// </summary> 
    [Serializable]
    public enum TargetComponentType 
    { 
        Image,
        RawImage,
        SpriteRenderer,
        Other
    }

    [Serializable]
    public enum ImageLoadType {
        [Tooltip("同步加载")]
        Sync,
        [Tooltip("异步加载")]
        ASync
    }


    [Serializable]
    public class OnImageLoaderLoading : UnityEvent<float> {}

 
    [Serializable]
    public class OnImageLoaderLoadCompleted : UnityEvent<bool,string>{}
     

    [Serializable]
    public class OnImageLoaderLoadFailure : UnityEvent<string> { }

    /// <summary>
    /// 图片加载器
    /// </summary> 
    [Serializable]
    public class ImageLoader : MonoBehaviour 
    { 
        private static Dictionary<TargetComponentType, TargetComponentAdapter> _allComponentAdapter;

        private static Dictionary<TargetComponentType, TargetComponentAdapter> allComponentAdapter
        {
            get 
            {
                if (_allComponentAdapter == null) 
                {
                    _allComponentAdapter = new Dictionary<TargetComponentType, TargetComponentAdapter>();

                    // 通过反射查询到所有的适配器
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly assembly in assemblies)
                    {
                        if (!assembly.FullName.StartsWith("ABManager"))
                            continue;

                        foreach (var item in assembly.GetTypes())
                        {
                            if (ABTools.IsImpInterface(item, typeof(TargetComponentAdapter)))
                            {
                                TargetComponentAdapter adapter = System.Activator.CreateInstance(item) as TargetComponentAdapter;
                                if (adapter == null) continue;
                                if (_allComponentAdapter.ContainsKey(adapter.TargetComponentType)) continue;
                                _allComponentAdapter.Add(adapter.TargetComponentType, adapter);
                            }
                        } 
                    } 
                }

                return _allComponentAdapter;
            }

        }

        [SerializeField]
        private ImageModel imageModel;


        /// <summary>
        /// 需要加载的图像数据
        /// </summary>
        private ImageModel ImageModel
        {
            get { 
                return imageModel;
            }
            set {
                if (imageModel.Equals(value)) return;

                if (!imageModel.IsEmpty())
                    ImageLoaderManager.UnloadImage(imageModel, this.GetHashCode());

                ImageData = null;

                imageModel = value;

#if UNITY_EDITOR
                if (Application.isPlaying)
                    // 刷新
                    RefreshImage();
#else 
                // 刷新
                RefreshImage();
#endif
            }
        }

        /// <summary>
        /// 图片加载方式
        /// </summary>
        public ImageLoaderType Type
        {
            set {
                ImageModel model = ImageModel;
                model.type = value;
                ImageModel = model;
            }

            get {

                return ImageModel.type;
            }
        }

        /// <summary>
        /// 图片加载路径 *注: 当加载图片的方式为 ImageLoaderType.Network 或 ImageLoaderType.Local 时有效
        /// </summary>
        public string Path
        {
            set
            {
                ImageModel model = ImageModel;
                model.path = value;
                ImageModel = model;
            }

            get
            {

                return ImageModel.path; ;
            }
        }

        /// <summary>
        /// 待加载的图片资源所在的模块名 *注: 当加载图片的方式为 ImageLoaderType.AssetBundle 时有效 
        /// </summary>
        public string ProjectName {
            set
            {
                ImageModel model = ImageModel;
                model.projectName = value;
                ImageModel = model;
            }

            get
            {

                return ImageModel.projectName;
            }
        }

        /// <summary>
        /// 待加载的图片资源名 *注: 当加载图片的方式为 ImageLoaderType.AssetBundle 时有效 
        /// </summary>
        public string AssetName
        {
            set
            {
                ImageModel model = ImageModel;
                model.assetName = value;
                ImageModel = model;
            }

            get
            { 
                return ImageModel.assetName;
            }
        } 


        private ImageLoaderRequest request_image;


        [Tooltip("加载到的图片赋值给目标组件的类型")]
        [SerializeField]
        private TargetComponentType targetComponentType = TargetComponentType.Image;

        internal TargetComponentType TargetComponentType => targetComponentType;

        private ImageData ImageData { get;set; }
  
        internal UnityEngine.Object TargetComponent { get; set; }



        /// <summary>
        /// 需要赋值的目标组件的全名(包含命名空间) 例如: UnityEngine.UI.Image
        /// </summary>
        [Tooltip("需要赋值的目标组件的全名(包含命名空间) 例如: UnityEngine.UI.Image")] 
        public string target_component_full_name;
        /// <summary>
        /// 需要赋值的目标组件的字段名 例如: sprite
        /// </summary>
        [Tooltip("需要赋值的目标组件的字段名 例如: sprite")]
        public string target_component_fields_name;

        [Tooltip("加载中的颜色")]
        public Color loading_color = Color.white;
        [Tooltip("加载完成的颜色")]
        public Color load_complete_color = Color.white;
        [Tooltip("是否自动 SetNativeSize")]
        public bool auto_set_native_size;
         
        /// <summary>
        /// 开始加载图片的回调
        /// </summary>
        [Header("当开始加载图片时触发")]
        public UnityEvent OnStartLoad;
        /// <summary>
        /// 正在加载的回调 参数: float: 加载进度(0-1)
        /// </summary>
        [Header("正在加载的回调 参数: float: 加载进度(0-1)")]
        public OnImageLoaderLoading OnLoading;
        /// <summary>
        /// 加载完成的回调 参数: bool:是否成功 ImageData:图像数据(如果没有加载成功 为空) string:如果加载失败,则是失败的原因,如果成功则为:success
        /// </summary>
        [Header("加载完成的回调 参数: bool:是否成功 string:如果加载失败,则是失败的原因,如果成功则为:success")]
        public OnImageLoaderLoadCompleted OnLoadCompleted;
        /// <summary>
        /// 加载成功的回调(如果加载失败则不会触发)
        /// </summary>
        [Header("加载成功的回调(如果加载失败则不会触发)")]
        public UnityEvent OnLoadSuccess;
        /// <summary>
        /// 加载失败的回调(如果加载成功则不会触发) 参数:string:失败的原因
        /// </summary>
        [Header("加载失败的回调(如果加载成功则不会触发) 参数:string:失败的原因")]
        public OnImageLoaderLoadFailure OnLoadFailure;

        /// <summary>
        /// 是否正在加载图片中...
        /// </summary>
        private bool isLoading = false;

#if UNITY_EDITOR
        private void Reset()
        {
            targetComponentType = TargetComponentType.Other;
            foreach (var item in allComponentAdapter.Values)
            {
                if (item.TargetComponentType == TargetComponentType.Other) continue;
                if (item.TargetComponent(this) != null ) 
                {
                    targetComponentType = item.TargetComponentType;
                    break;
                }
            }
        }
#endif
         
        private void OnEnable()
        { 
            RefreshImage();
        }

        private void OnDisable()
        {
            ImageLoaderManager.UnloadImage(imageModel, this.GetHashCode());
            request_image?.Abort(); // 如果正在请求 立即中断

            if (isLoading)
                OnLoadCompleted?.Invoke(false, "加载中游戏物体被隐藏或销毁导致中断!"); 
        }

        private void RefreshImage() {

            if (imageModel.type == ImageLoaderType.Local || imageModel.type == ImageLoaderType.Network)
                if (string.IsNullOrEmpty(imageModel.path)) return;

            if (imageModel.type == ImageLoaderType.AssetBundle )
                if (string.IsNullOrEmpty(imageModel.projectName) && string.IsNullOrEmpty(imageModel.assetName)) return;

            if (!gameObject.activeInHierarchy) return;

            ImageData = null;

            if (allComponentAdapter.ContainsKey(targetComponentType)) {
                allComponentAdapter[targetComponentType].SetValue(this, null);
                allComponentAdapter[targetComponentType].SetColor(this, loading_color); 
            }

            // 如果正在请求 立即中断
            request_image?.Abort(); 

            OnStartLoad?.Invoke();

            request_image = ImageLoaderManager.LoadImage(imageModel,this.GetHashCode());

            request_image.onProgressChange -= OnProgressChange;
            request_image.onProgressChange += OnProgressChange;


            if (request_image != null)
                request_image.AddCompleteEvent(OnRefreshFinsh); 

            isLoading = true; // 正在加载中...
        }

        private void OnRefreshFinsh(ImageLoaderRequest request_image)
        {
#if ABManager_LOG_OPEN_TESTING
            Debug.LogFormat("图片加载完成:sprite:{0} texture2d:{1}", request_image.NetworkImage != null ? request_image.NetworkImage.sprite:null,
               request_image.NetworkImage != null ? request_image.NetworkImage.texture : null);
#endif

            // 加载完成
            isLoading = false;
            if (request_image != null)
                request_image.RemoveCompleteEvent(OnRefreshFinsh);  

            ImageData = request_image.NetworkImage;

            if (request_image != null && ImageData != null)
            {
                if (allComponentAdapter.ContainsKey(targetComponentType)) {
                    allComponentAdapter[targetComponentType].SetColor(this, load_complete_color);
                    allComponentAdapter[targetComponentType].SetValue(this, ImageData);
                }
            }

            OnLoadCompleted?.Invoke(string.IsNullOrEmpty(request_image.error), string.IsNullOrEmpty(request_image.error) ? "success" : request_image.error);

            if (string.IsNullOrEmpty(request_image.error))
                OnLoadSuccess?.Invoke();
            else 
                OnLoadFailure?.Invoke(request_image.error);
        }
  
        private void OnProgressChange(float progress)
        {
            OnLoading?.Invoke(progress);
        }

    }

}

