using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PTG.Terrain
{
    public class ProcGenDebugUI : MonoBehaviour
    {
        [SerializeField] private Button regenerateButton;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private ProcGenManager targetManager;

        public void OnRegenerate()
        {
            regenerateButton.interactable = false;
            StartCoroutine(PerformRegeneration());
        }

        private IEnumerator PerformRegeneration()
        {
            yield return targetManager.AsyncRegenerateWorld(OnStatusReported);
            regenerateButton.interactable = true;

            yield return null;
        }

        private void OnStatusReported(int step, int totalSteps, string status) =>
            statusText.text = $"Step {step} of {totalSteps}: {status}";
    }
}