using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorUtil
{
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
