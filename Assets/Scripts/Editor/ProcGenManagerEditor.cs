using System;
using UnityEditor;
using UnityEngine;

namespace PTG.Editor
{
    [CustomEditor(typeof(ProcGenManager))]
    public class ProcGenManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Regenerate World"))
            {
                var targetManager = serializedObject.targetObject as ProcGenManager;
                if (targetManager == null)
                    throw new Exception("targetManager is null");
                
                targetManager.RegenerateWorld();
            }
        }
    }
}