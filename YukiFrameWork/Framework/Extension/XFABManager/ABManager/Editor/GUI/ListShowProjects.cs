using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace XFABManager
{
    public class ListShowProjects : BaseShowProjects
    {

        private ProjectListTree listTree;

#if UNITY_6000_2_OR_NEWER
        TreeViewState<int> projectListState;
#else
        TreeViewState projectListState;
#endif

        ProjectListTree projectListView;
        MultiColumnHeaderState projectListMCHState;

        private Rect buttonRect;
        private Rect searchRect;
        private SearchField bundleSerchField;
        public override void DrawProjects(Rect rect, EditorWindow window)
        {
            base.DrawProjects(rect, window);


            if (projectListState == null || projectListView == null)
            {
#if UNITY_6000_2_OR_NEWER
                projectListState = new TreeViewState<int>();
#else
                projectListState = new TreeViewState();
#endif

                var headerState = ProjectListTree.CreateDefaultMultiColumnHeaderState();// multiColumnTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(projectListMCHState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(projectListMCHState, headerState);
                projectListMCHState = headerState;

                projectListView = new ProjectListTree(projectListState, projectListMCHState, this.window);
                projectListView.Reload();
            }

            searchRect.Set(rect.x, rect.y, rect.width, 20);

            OnGUIBundleSerchFiled(searchRect);

            rect.y += 20;
            rect.height -= 20;

            rect.height -= 30;

            projectListView.OnGUI(rect);


            buttonRect.Set(rect.x, rect.y + rect.height + 2, rect.width, 23);
            if (GUI.Button(buttonRect, "新建项目"))
            {
                this.CreateProject();
            }
        }

        void OnGUIBundleSerchFiled(Rect rect)
        {
            if (bundleSerchField == null)
            {
                bundleSerchField = new SearchField();
            }

            projectListView.searchString = bundleSerchField.OnGUI(rect, projectListView.searchString);
        }
    }
}

#endif