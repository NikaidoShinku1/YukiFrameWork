using System;
using System.Collections.Generic;

namespace YukiFrameWork.Extension
{
    public enum SerializationType
    {
        Json,
        Xml,
        Bytes
    }
    [Serializable]
    public class SerializeConfig
    {
        public SerializationType type = SerializationType.Json;

        public Dictionary<string,string> serializedDict = new Dictionary<string, string>();

        public string SerializationInfo(object serializable)
        {
            string key = serializedType;
            if (string.IsNullOrEmpty(key)) return string.Empty;
            try
            {
                if (!serializedDict.TryGetValue(key, out var info))
                {
                    info = string.Empty;
                    switch (type)
                    {

                        case SerializationType.Json:
                            info = AssemblyHelper.SerializedObject(serializable);
                            break;
                        case SerializationType.Xml:
                            {
                                info = AssemblyHelper.XmlSerializedObject(serializable);
                            }
                            break;
                        case SerializationType.Bytes:
                            {
                                Byte[] bytes = AssemblyHelper.ByteSerializeObject(serializable);

                                string hex = BitConverter.ToString(bytes).Replace("-", " ");

                                info = hex;
                            }
                            break;
                    }
                }
                return info;
            }
            catch(Exception ex)
            { throw ex; }                   
        }     
     
        public string serializedType;

        public string genericPath = "Assets/Data";

        public string customName;

        public bool IsCustomOpen = false;

        public string GetSuffix()
        {
            return type switch
            {
                SerializationType.Json => ".json",
                SerializationType.Xml => ".xml",
                SerializationType.Bytes => ".bytes",
                _ => string.Empty
            } ;
        }


    }
}