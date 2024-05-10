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

		TreeViewState profileState;
		ProfileTreeView profileView;
		MultiColumnHeaderState profileMCHState;

		Rect area = new Rect();

		[OnInspectorGUI]
		private void OnGUI() {

			if (profileState == null || profileView ==null) {
				profileState = new TreeViewState();

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

