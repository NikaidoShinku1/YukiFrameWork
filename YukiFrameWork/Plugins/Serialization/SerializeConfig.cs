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
                            info = SerializationTool.SerializedObject(serializable);
                            break;
                        case SerializationType.Xml:
                            {
                                info = SerializationTool.XmlSerializedObject(serializable);
                            }
                            break;
                        case SerializationType.Bytes:
                            {
                                Byte[] bytes = SerializationTool.ByteSerializedObject(serializable);

                                string hex = BitConverter.ToString(bytes).Replace("-", " ");

                                info = hex;
                            }
                            break;
                    }
                }
                return info;
            }
            catch
            { return string.Empty; }                   
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