using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gyulari.HexSensor.Util
{
    public static class EditorUtil
    {
        public static Material GLMaterial = CreateGLMaterial();

        private static Material CreateGLMaterial()
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            Material material = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            material.SetInt("_ZWrite", 0);
            return material;
        }

        public static void Repaint(SerializedObject obj)
        {
            GetEditor(obj).Repaint();
        }

        public static Editor GetEditor(SerializedObject obj)
        {
            foreach (var editor in ActiveEditorTracker.sharedTracker.activeEditors) {
                if (editor.serializedObject == obj) {
                    return editor;
                }
            }

            throw new MissingComponentException("Editor not available for " + obj);
        }

        public static void HideBehaviorParametersEditor()
        {
            var tracker = ActiveEditorTracker.sharedTracker;
            var editors = tracker.activeEditors;
            for (int i = 0; i < editors.Length; i++) {
                // Can't check type because BehaviorParametersEditor is internal.
                if (editors[i].ToString() == " (Unity.MLAgents.Editor.BehaviorParametersEditor)") {
                    tracker.SetVisible(i, 0);
                }
            }
        }
    }
}

