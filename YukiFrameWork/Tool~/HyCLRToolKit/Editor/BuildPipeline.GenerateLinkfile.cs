using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
#if UNITY_EDITOR_WIN
using HybridCLR.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Custom.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        private static string _il2cppManagedPath = string.Empty;

        private static string il2cppManagedPath
        {
            get
            {
                if (string.IsNullOrEmpty(_il2cppManagedPath))
                {
                    var contentsPath = EditorApplication.applicationContentsPath;
                    var extendPath = "";

                    var buildTarget = EditorUserBuildSettings.activeBuildTarget;
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX
                    switch (buildTarget)
                    {
                        case BuildTarget.StandaloneWindows64:
                        case BuildTarget.StandaloneWindows:
                            extendPath = "PlaybackEngines/windowsstandalonesupport/Variations/il2cpp/Managed/";
                            break;
                        case BuildTarget.iOS:
                            extendPath = "PlaybackEngines/iOSSupport/Variations/il2cpp/Managed/";
                            break;
                        case BuildTarget.Android:
                            extendPath = "PlaybackEngines/AndroidPlayer/Variations/il2cpp/Managed/";
                            break;
                        default:
                            throw new Exception($"[BuildPipeline::GenerateLinkfile] 请选择合适的平台, 目前是:{buildTarget}");
                    }
#elif UNITY_EDITOR_OSX
                switch (buildTarget)
                {
                    case BuildTarget.StandaloneOSX:
                        extendPath = "PlaybackEngines/MacStandaloneSupport/Variations/il2cpp/Managed/";
                        break;
                    case BuildTarget.iOS:
                        extendPath = "../../PlaybackEngines/iOSSupport/Variations/il2cpp/Managed/";
                        break;
                    case BuildTarget.Android:
                        extendPath = "../../PlaybackEngines/AndroidPlayer/Variations/il2cpp/Managed/";
                        break;
                    default:
                        throw new Exception($"[BuildPipeline::GenerateLinkfile] 请选择合适的平台, 目前是:{buildTarget}");
                }
#endif
                    if (string.IsNullOrEmpty(extendPath))
                    {
                        throw new Exception($"[BuildPipeline::GenerateLinkfile] 请选择合适的平台, 目前是:{buildTarget}");
                    }

                    _il2cppManagedPath = Path.Combine(contentsPath, extendPath).Replace('\\', '/');
                }

                return _il2cppManagedPath;
            }
        }

        private static List<string> IgnoreClass = new List<string>()
        {
            "editor", "netstandard", "Bee.", "dnlib", ".framework", "Test", "plastic", "Gradle", "log4net", "Analytics", "System.Drawing",
            "NVIDIA", "VisualScripting", "UIElements", "IMGUIModule", ".Cecil", "GIModule", "GridModule", "HotReloadModule", "StreamingModule", 
            "TLSModule", "XRModule", "WindModule", "VRModule", "VirtualTexturingModule", "compiler", "BuildProgram", "NiceIO", "ClothModule",
            "VFXModule", "ExCSS", "GeneratedCode", "mscorlib", "System", "SyncToolsDef"
        };
        private static bool IsIngoreClass(string classFullName)
        {
            var tmpName = classFullName.ToLower();
            foreach (var ic in IgnoreClass)
            {
                if (tmpName.Contains(ic.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<string> IgnoreType = new List<string>()
        {
            "jetbrain", "editor", "PrivateImplementationDetails", "experimental", "microsoft.", "compiler"
        };
        private static bool IsIgnoreType(string typeFullName)
        {
            var tmpName = typeFullName.ToLower();
            foreach (var ic in IgnoreType)
            {
                if (tmpName.Contains(ic.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        public static void GenerateLinkfile(string outPath)
        {
            var basePath = il2cppManagedPath;
            if (!Directory.Exists(basePath))
            {
                Debug.LogWarning($"[BuildPipeline::GenerateLinkfile] can't find il2cpp's dlls [{basePath}]");
                basePath = basePath.Replace("/il2cpp/", "/mono/");
            }

            if (!Directory.Exists(basePath))
            {
                Debug.LogWarning($"[BuildPipeline::GenerateLinkfile] can't find il2cpp's dlls [{basePath}]");
                return;
            }

            LinkedList<string> Assemblies;
            Dictionary<string, Assembly> AllAssemblies = new Dictionary<string, Assembly>();

            var hashAss = new HashSet<string>();
            var files = new List<string>(Directory.GetFiles(basePath, "*.dll"));
            foreach (var file in files)
            {
                var ass = Assembly.LoadFile(file);
                if (ass != null)
                {
                    var name = ass.GetName().Name;
                    if (IsIngoreClass(name))
                    {
                        continue;
                    }

                    if (!hashAss.Contains(name))
                    {
                        hashAss.Add(name);

                        AllAssemblies[name] = ass;
                    }
                }
            }

            var names = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;
            var localAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var localPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..")).Replace('\\', '/');
            foreach (var ass in localAssemblies)
            {
                if (ass.IsDynamic)
                {
                    Debug.LogWarning($"[BuildPipeline::GenerateLinkfile] {ass.FullName} is dynamic!!!");
                    continue;
                }

                var assPath = Path.GetFullPath(ass.Location).Replace('\\', '/');
                if (assPath.Contains(localPath) && assPath.ToLower().Contains("/editor/"))
                {
                    continue;
                }

                var name = ass.GetName().Name;
                if (hashAss.Contains(name))
                {
                    continue;
                }
                
                var ignore = false;
                foreach (var n in names)
                {
                    if (name.Contains(n))
                    {
                        ignore = true;
                        break;
                    }
                }

                if (ignore)
                {
                    continue;
                }

                hashAss.Add(name);
                AllAssemblies[name] = ass;
            }

            var fullPreserve = new List<string>();
            var otherAss = new List<string>();
            var otherAssemblies = new Dictionary<string, List<string>>();

            foreach (var ass in AllAssemblies)
            {
                if (IsIngoreClass(ass.Key))
                {
                    continue;
                }

                var allTypes = ass.Value.GetTypes();
                var stripTypes = new List<string>();
                foreach (var type in allTypes)
                {
                    if (IsIgnoreType(type.FullName))
                    {
                        continue;
                    }

                    stripTypes.Add(type.FullName);
                }

                if (stripTypes.Count == 0)
                {
                    continue;
                }
                else if (allTypes.Length < 5)
                {
                    fullPreserve.Add(ass.Key);
                }
                else if (allTypes.Length - stripTypes.Count > allTypes.Length * 0.1f)
                {
                    otherAssemblies.Add(ass.Key, stripTypes);
                    otherAss.Add(ass.Key);
                }
                else
                {
                    fullPreserve.Add(ass.Key);
                }
            }

            fullPreserve.Sort();
            otherAss.Sort();

            var fileName = outPath;
            var writer = System.Xml.XmlWriter.Create(fileName, new System.Xml.XmlWriterSettings()
            {
                Encoding = new UTF8Encoding(false),
                Indent = true
            });

            writer.WriteStartDocument();
            writer.WriteStartElement("linker");

            foreach (var fp in fullPreserve)
            {
                writer.WriteStartElement("assembly");
                writer.WriteAttributeString("fullname", fp);
                writer.WriteAttributeString("preserve", "all");
                writer.WriteEndElement();
            }

            foreach (var fp in otherAss)
            {
                writer.WriteStartElement("assembly");
                writer.WriteAttributeString("fullname", fp);

                foreach (var type in otherAssemblies[fp])
                {
                    writer.WriteStartElement("type");
                    writer.WriteAttributeString("fullname", type);
                    writer.WriteAttributeString("preserve", "all");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();

            var outAssName = fileName.Replace('\\', '/').Substring(0, fileName.LastIndexOf('/'));
            checkass(Path.Combine(outAssName, "ReservedAssembly.cs"));
        }

        public class AssemblyComp : IComparable
        {
            public int CompareTo(object obj)
            {
                throw new NotImplementedException();
            }
        }

        private class AsmDefHeader
        {
            public string name;
            public List<string> includePlatforms;
            public List<string> defineConstraints;
        }

        private static List<AsmDefHeader> GetAssemblyDefinitionAsset()
        {
            var ret = new List<AsmDefHeader>();
            string[] folderContents = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");

            foreach (var asset in folderContents)
            {
                var tmp = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(AssetDatabase.GUIDToAssetPath(asset));
                if (tmp != null)
                {
                    var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<AsmDefHeader>(tmp.text);
                    if (jsonObj != null && jsonObj.includePlatforms != null)
                    {
                        if (jsonObj.includePlatforms.Count == 0)
                        {
                            if (jsonObj.defineConstraints == null || jsonObj.defineConstraints.Count == 0)
                            {
                                ret.Add(jsonObj);
                            }
                        }
                        else
                        {
                            var hasEditor = false;
                            foreach (var p in jsonObj.includePlatforms)
                            {
                                if (p.ToLower() == "editor")
                                {
                                    hasEditor = true;
                                    break;
                                }
                            }

                            if (hasEditor)
                            {
                                continue;
                            }

                            if (jsonObj.defineConstraints == null || jsonObj.defineConstraints.Count == 0)
                            {
                                ret.Add(jsonObj);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        private static void checkass(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var codeTemplate = @"using System.Text;
using UnityEngine;

namespace GameMain.Scripts.HybridCLR
{
    public class ReservedAssembly : MonoBehaviour
	{
	    private void Awake()
		{
            var sb = new StringBuilder();

			void Reserved<T>()
			{
				sb.AppendLine(typeof(T).ToString());
			}

//Replace This
            Debug.Log(sb.ToString());
		}
	}
}";
            var assemblyDefinitionAssets = GetAssemblyDefinitionAsset();

            var watchAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var successAssemblies = new Dictionary<string, Assembly>();
            foreach (var wa in watchAssemblies)
            {
                var locName = Path.GetFileName(wa.Location).ToLower();
                if (successAssemblies.ContainsKey(locName))
                {
                    continue;
                }

                foreach (var ada in assemblyDefinitionAssets)
                {
                    if (locName == ada.name.ToLower() + ".dll")
                    {
                        successAssemblies.Add(locName, wa);
                        break;
                    }
                }
            }

            var assCsharp = watchAssemblies.First(a => a.GetName().Name == "Assembly-CSharp");
            if (assCsharp != null)
            {
                successAssemblies.Add("assembly-csharp.dll", assCsharp);
            }

            var distAssembly = new Dictionary<string, Assembly>();            
            foreach (var sa in successAssemblies)
            {
                var ass = sa.Value;
                if (ass.IsDynamic)
                {
                    continue;
                }

                if (ass.FullName.Contains("Editor"))
                {
                    continue;
                }

                var hasEditor = false;
                var ras = ass.GetReferencedAssemblies();
                foreach (var r in ras)
                {
                    if (r.Name.Contains("UnityEditor"))
                    {
                        hasEditor = true;
                        break;
                    }
                }

                if (hasEditor)
                {
                    continue;
                }

                distAssembly.Add(ass.FullName, ass);
                foreach (var r in ras)
                {
                    foreach (var wa in watchAssemblies)
                    {
                        if (wa.FullName == r.FullName && !distAssembly.ContainsKey(wa.FullName))
                        {
                            distAssembly.Add(wa.FullName, wa);
                        }
                    }
                }
            }

            var info = new Dictionary<string, string>();
            foreach (var _ass in distAssembly.Values)
            {
                var ass = _ass;
                if (ass.IsDynamic)
                {
                    continue;
                }

                if (ass.FullName.Contains("Assembly-CSharp") 
                    || ass.FullName.Contains("TestRunner")
                    || ass.FullName.Contains("HybridCLR")
                    || ass.FullName.Contains("nunit")
                    )
                {
                    continue;
                }               

                Debug.Log($"{ass.FullName}");

                var files = Directory.GetFiles(il2cppManagedPath, Path.GetFileName(ass.Location), SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    ass = Assembly.LoadFile(files[0]);
                }

                var classOk = false;
                var allTypes = ass.GetExportedTypes();
                foreach (var type in allTypes)
                {
                    if (!type.IsPublic || type.IsAbstract || type.IsGenericType)
                    {
                        continue;
                    }

                    var name = type.FullName;
                    if (name.Contains("System.") && ass.FullName.Contains("Newt"))
                    {
                        continue;
                    }

                    var atts = new List<CustomAttributeData>(type.CustomAttributes.Where(a => a.AttributeType.Name.Contains("Obsolete")));
                    if (atts.Count > 0)
                    {
                        continue;
                    }

                    if (classOk)
                    {
                        break;
                    }

                    var con = type.GetConstructors();
                    if (con != null && con.Length > 0)
                    {
                        foreach (var c in con)
                        {
                            if (c.IsConstructor && c.IsPublic)
                            {
                                if (info.TryGetValue(type.FullName, out var val))
                                {
                                    Debug.LogError($"{type.FullName} {val} {ass.Location}");
                                }
                                else
                                {

                                    info.Add(type.FullName, ass.Location);
                                }
                                classOk = true;
                                break;
                            }
                        }
                    }
                }
            }


            var sb = new StringBuilder();
            var keys = new HashSet<string>(info.Keys);
            foreach (var k in keys)
            {
                var x = info[k];
                sb.AppendLine($"\t\t\tReserved<{k}>(); // {Path.GetFileName(x)}");
            }

            File.WriteAllText(fileName, codeTemplate.Replace("//Replace This", sb.ToString()));
        }
    }
}
#endif