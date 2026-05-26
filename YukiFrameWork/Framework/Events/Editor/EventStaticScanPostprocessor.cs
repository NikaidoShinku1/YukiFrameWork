#if UNITY_EDITOR
using UnityEditor;

namespace YukiFrameWork.Events.Editor
{
    public class EventStaticScanPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                if (path.EndsWith(".cs"))
                {
                    EventStaticAnalyzer.InvalidateCache();
                    return;
                }
            }

            foreach (var path in deletedAssets)
            {
                if (path.EndsWith(".cs"))
                {
                    EventStaticAnalyzer.InvalidateCache();
                    return;
                }
            }

            for (var i = 0; i < movedAssets.Length; i++)
            {
                if (movedAssets[i].EndsWith(".cs") || movedFromAssetPaths[i].EndsWith(".cs"))
                {
                    EventStaticAnalyzer.InvalidateCache();
                    return;
                }
            }
        }
    }
}
#endif
