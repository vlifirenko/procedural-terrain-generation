using PTG.Model.Enum;
using UnityEngine;
using UnityEngine.Serialization;

namespace PTG.Model.Config
{
    [CreateAssetMenu(menuName = "ProcGen/Tile config", fileName = "ProcGenTileConfig")]
    public class ProcGenTileConfig : ScriptableObject
    {
        [Header("Common")]
        public float tileSize = 200f;
        [FormerlySerializedAs("resolution")] public ETerrainResolution terrainResolution = ETerrainResolution.Size_64x64;
        [Header("Height")]
        public float maxHeight = 100f;
        [Header("Painting")]
        public EPaintingMode paintingMode;
        public ETextureResolution textureResolution = ETextureResolution.Resolution_512x512;
    }
}