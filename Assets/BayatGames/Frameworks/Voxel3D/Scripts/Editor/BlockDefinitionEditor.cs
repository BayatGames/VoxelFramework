using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using BayatGames.Frameworks.Voxel3D.Definitions;

namespace BayatGames.Frameworks.Voxel3D
{

    [CustomEditor(typeof(BlockDefinition), true)]
    public class BlockDefinitionEditor : Editor
    {

        protected BlockDefinition blockDefinition;
        protected Editor modelEditor;

        protected virtual void OnEnable()
        {
            this.blockDefinition = (BlockDefinition)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            //if (this.modelEditor == null || GUILayout.Button("Reload"))
            //{
            //    this.modelEditor = Editor.CreateEditor(this.blockDefinition.Model);
            //}
            //this.modelEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(500, 500), GUIStyle.none);
            //this.previewRender = new PreviewRenderUtility();
            //Rect rect = GUILayoutUtility.GetRect(500, 500);
            //this.previewRender.BeginPreview(rect, GUIStyle.none);
            //this.previewRender.DrawMesh(this.blockDefinition.Model, Vector3.one, Quaternion.identity, null, 0);
            //this.previewRender.EndAndDrawPreview(rect);
        }

    }

}