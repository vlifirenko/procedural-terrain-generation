using UnityEngine;

namespace PTG.Model.Config
{
    [CreateAssetMenu(fileName = "Biome Config", menuName = "Procedural Generation/Biome Config", order = -1)]
    public class BiomeConfig : ScriptableObject
    {
        public string Name;
    }
}