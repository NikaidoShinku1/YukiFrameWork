namespace YukiFrameWork.AddressExtension
{
    /// <summary>
    /// 资源条件器，用于简化对于资源的获取规则
    /// </summary>
    public interface IResourcesConditioner
    {
        /// <summary>
        /// 规则路径,这个路径会被用来生成Addressable的资源获取标签，生成的标签格式为：RulePath/YouResName.Suffix
        /// </summary>
        public string RulePath { get; }
        /// <summary>
        /// 资源的后缀，这个后缀会被用来生成Addressable的资源获取标签，生成的标签格式为：RulePath/YouResName.Suffix
        /// </summary>
        public string Suffix { get; }
    }

}