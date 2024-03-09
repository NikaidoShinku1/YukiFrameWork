using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace XFABManager {
	
	public class ProfileWindow : EditorWindow
	{

		TreeViewState profileState;
		ProfileTreeView profileView;
		MultiColumnHeaderState profileMCHState;

		Rect area = new Rect();

		private void OnGUI() {

			if (profileState == null || profileView ==null) {
				profileState = new TreeViewState();

                var headerState = ProfileTreeView.CreateDefaultMultiColumnHeaderState();// multiColumnTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(profileMCHState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(profileMCHState, headerState);
                profileMCHState = headerState;

                profileView = new ProfileTreeView(profileState, profileMCHState,this);
				profileView.Reload();
            }

			 

			area.Set(0, 0, position.width, position.height);
			profileView.OnGUI(area);

		}

	}

}


	 
