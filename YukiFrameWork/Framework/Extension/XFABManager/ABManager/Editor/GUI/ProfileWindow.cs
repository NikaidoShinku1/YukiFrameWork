#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using YukiFrameWork;

namespace XFABManager {

    public class ProfileWindow 
    {

#if UNITY_6000_2_OR_NEWER
		TreeViewState<int> profileState;
#else
        TreeViewState profileState;
#endif

        ProfileTreeView profileView;
        MultiColumnHeaderState profileMCHState;

        Rect area = new Rect();
        [OnInspectorGUI]
        private void OnGUI()
        {

            if (profileState == null || profileView == null)
            {
#if UNITY_6000_2_OR_NEWER
                profileState = new TreeViewState<int>();
#else
                profileState = new TreeViewState();
#endif

                var headerState = ProfileTreeView.CreateDefaultMultiColumnHeaderState();// multiColumnTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(profileMCHState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(profileMCHState, headerState);
                profileMCHState = headerState;

                profileView = new ProfileTreeView(profileState, profileMCHState, FrameWorkDisignWindow.Instance);
                profileView.Reload();
            }



            area.Set(0, 0, FrameWorkDisignWindow.Instance.position.width, FrameWorkDisignWindow.Instance.position.height);
            profileView.OnGUI(area);

        }

    }

}

#endif

