using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace YukiFrameWork.XFABManager
{
    public class ListShowProjects : BaseShowProjects
    {

        private ProjectListTree listTree;

        TreeViewState projectListState;
        ProjectListTree projectListView;
        MultiColumnHeaderState projectListMCHState;

        private Rect buttonRect;
        private Rect searchRect;
        private SearchField bundleSerchField;
        public override void DrawProjects(Rect rect,EditorWindow window)
        {
            base.DrawProjects(rect,window);


            if (projectListState == null || projectListView == null)
            {
                projectListState = new TreeViewState();

                var headerState = ProjectListTree.CreateDefaultMultiColumnHeaderState();// multiColumnTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(projectListMCHState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(projectListMCHState, headerState);
                projectListMCHState = headerState;

                projectListView = new ProjectListTree(projectListState, projectListMCHState, this.window );
                projectListView.Reload();
            }

            searchRect.Set( rect.x,rect.y,rect.width,20 );
             
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
            if (bundleSerchField == null) {
                bundleSerchField = new SearchField();
            }

            projectListView.searchString = bundleSerchField.OnGUI(rect, projectListView.searchString);
        }
    }
}

