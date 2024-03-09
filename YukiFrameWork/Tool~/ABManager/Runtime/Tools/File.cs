using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace XFABManager
{



    internal class File
    {
        private static Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();

        // Fix编码

        internal static void WriteAllBytes(string path, byte[] bytes)
        {

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                if (files.ContainsKey(path))
                    files[path] = bytes;
                else
                    files.Add(path, bytes);
            }
            else
            {
                System.IO.File.WriteAllBytes(path, bytes);
            }


        }

        internal static byte[] ReadAllBytes(string path)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                if (files.ContainsKey(path))
                    return files[path];
            }
            else
            {
                return System.IO.File.ReadAllBytes(path);
            }


            return null;
        }

        internal static void Delete(string path)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                if (!files.ContainsKey(path))
                    return;
                files.Remove(path);
            }
            else
            {
                System.IO.File.Delete(path);
            }
        }

        internal static bool Exists(string path)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return files.ContainsKey(path);
            else
                return System.IO.File.Exists(path);
        }

        internal static string[] ReadAllLines(string path)
        {

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                string content = Encoding.UTF8.GetString(ReadAllBytes(path));
                return content.Split('\n');
            }
            else
                return System.IO.File.ReadAllLines(path);

        }


        internal static void Copy(string source, string target, bool overwrite = false)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer) { 
                if (files.ContainsKey(source)) 
                    WriteAllBytes(target, files[source]);
            }
            else
                System.IO.File.Copy(source, target, overwrite);

        }

        internal static void WriteAllText(string path, string content)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                WriteAllBytes(path, Encoding.UTF8.GetBytes(content));
            else
                System.IO.File.WriteAllText(path, content);
        }

        internal static string ReadAllText(string path)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return Encoding.UTF8.GetString(ReadAllBytes(path));
            else
                return System.IO.File.ReadAllText(path);
        }


        internal static void Move(string source, string target)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Copy(source, target, true);
                Delete(source);
            }
            else
                System.IO.File.Move(source, target);
        }
    }
}