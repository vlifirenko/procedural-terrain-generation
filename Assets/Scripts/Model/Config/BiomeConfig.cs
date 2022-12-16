using UnityEngine;

namespace PTG.Model.Config
{
    [CreateAssetMenu(fileName = "Biome Config", menuName = "Procedural Generation/Biome Config", order = -1)]
    public class BiomeConfig : ScriptableObject
    {
        public string Name;
        
        [Range(0f, 1f)] public float minIntensity = 0.5f;
        [Range(0f, 1f)] public float maxIntensity = 1f;
        
        [Range(0f, 1f)] public float minDecayRate = 0.01f;
        [Range(0f, 1f)] public float maxDecayRate = 0.02f;

        public GameObject heightModifier;
    }
}