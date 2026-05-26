#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork
{
    public static class ViewControllerPartialCodeGenerator
    {
        private static readonly string[] DefaultHeaderLines =
        {
            "///=====================================================",
            "///这是由代码工具生成的代码文件,请勿手动改动此文件!",
            "///如果在代码里命名空间进行了变动,请在编辑器设置也对命名空间作出相同修改!",
            "///====================================================="
        };

        public static string GetExampleFilePath(GenericDataBase data)
            => $"{data.ScriptPath}/{data.ScriptName}.Example.cs";

        public static void Generate(
            GenericDataBase data,
            ISerializedFieldInfo serialized,
            YukiBind[] binds,
            System.Action onMarkPartialLoading = null,
            params string[] extraUsings)
        {
            if (data == null || serialized == null) return;

            var examplePath = GetExampleFilePath(data);
            var exists = File.Exists(examplePath);
            var fileMode = exists ? FileMode.Open : FileMode.Create;
            if (exists)
            {
                File.WriteAllText(examplePath, string.Empty);
                AssetDatabase.Refresh();
            }

            var codeCore = CodeCore.CreateCodeCore();
            foreach (var line in DefaultHeaderLines)
                codeCore.Descripton(line);

            var codeWriter = CodeWriter.CreateWriter();
            AppendFieldLines(codeWriter, serialized.GetSerializeFields());
            if (binds != null)
            {
                foreach (var bind in binds)
                {
                    if (bind == null) continue;
                    AppendFieldLine(codeWriter, bind._fields, bind.description);
                }
            }

            codeCore.Using("YukiFrameWork").Using("UnityEngine");
            foreach (var us in extraUsings)
            {
                if (!string.IsNullOrWhiteSpace(us))
                    codeCore.Using(us);
            }

            codeCore.EmptyLine()
                .CodeSetting(data.ScriptNamespace, data.ScriptName, string.Empty, codeWriter, false, true);

            WriteExampleFile(examplePath, fileMode, codeCore.builder.ToString());
            onMarkPartialLoading?.Invoke();
            AssetDatabase.Refresh();
        }

        private static void AppendFieldLines(CodeWriter writer, IEnumerable<SerializeFieldData> fields)
        {
            foreach (var info in fields)
                AppendFieldLine(writer, info, null);
        }

        private static void AppendFieldLine(CodeWriter writer, SerializeFieldData info, string description)
        {
            if (info == null) return;
            var level = info.fieldLevel[info.fieldLevelIndex];
            var typeName = info.Components[info.fieldTypeIndex];
            var desSuffix = description.IsNullOrEmpty() ? string.Empty : $"//Des:{description}";
            writer.CustomCode($"[SerializeField]{level} {typeName} {info.fieldName};{desSuffix}");
        }

        private static void WriteExampleFile(string path, FileMode mode, string content)
        {
            using var stream = new FileStream(path, mode, FileAccess.ReadWrite, FileShare.ReadWrite);
            using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
            streamWriter.Write(content);
        }
    }
}
#endif
