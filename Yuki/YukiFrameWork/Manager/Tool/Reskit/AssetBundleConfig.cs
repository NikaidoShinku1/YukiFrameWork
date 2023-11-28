using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
namespace YukiFrameWork.Res
{

    [System.Serializable]
    public class AssetBundleConfig
    {
        [XmlElement("ABList")]
        public List<AssetBundleBase> ABList { get;set; }

        [XmlAttribute("LoadFromAssetBundle")]
        public bool LoadFromAssetBundle { get; set; }

        public AssetBundleConfig() 
        {

        }

        public AssetBundleBase FindBase(string path)
        {
            uint crc = CRC32.GetCRC32(path);
            for (int i = 0; i < ABList.Count; i++)
            {
                if(ABList[i].Crc == crc)
                    return ABList[i];
            }
            return null;
        }
    }

    [System.Serializable]
    public class AssetBundleBase
    {
        [XmlAttribute("Path")]
        public string Path { get; set; }

        [XmlAttribute("Crc")]
        public uint Crc { get; set; }

        [XmlAttribute("AbName")]
        public string ABName { get; set; }

        [XmlAttribute("AssetsName")]
        public string AssetsName { get; set; }
        
        [XmlElement("ABDependce")]
        public List<string> ABDependce { get; set; }
    }
}
