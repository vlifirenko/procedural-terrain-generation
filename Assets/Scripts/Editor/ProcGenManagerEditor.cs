using System;
using System.Collections;
using PTG.Terrain;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace PTG.Editor
{
    [CustomEditor(typeof(ProcGenManager))]
    public class ProcGenManagerEditor : UnityEditor.Editor
    {
        private int _progressID;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Regenerate Textures"))
            {
                var targetManager = serializedObject.targetObject as ProcGenManager;
                if (targetManager == null)
                    throw new Exception("targetManager is null");

                targetManager.RegenerateTextures();
            }

            if (GUILayout.Button("Regenerate World"))
            {
                var targetManager = serializedObject.targetObject as ProcGenManager;
                if (targetManager == null)
                    throw new Exception("targetManager is null");

                EditorCoroutineUtility.StartCoroutine(PerformRegeneration(targetManager), this);
            }
        }

        private IEnumerator PerformRegeneration(ProcGenManager targetManager)
        {
            _progressID = Progress.Start("Regenerating terrain");

            yield return targetManager.AsyncRegenerateWorld(OnStatusReported);

            Progress.Remove(_progressID);

            yield return null;
        }

        private void OnStatusReported(int step, int totalSteps, string status)
        {
            Progress.Report(_progressID, step, totalSteps, status);
        }
    }
}