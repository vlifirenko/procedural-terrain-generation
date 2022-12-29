using PTG.Model.Config;
using PTG.Model.Enum;
using UnityEngine;

namespace PTG.Tile
{
    public abstract class BaseTileModifier : MonoBehaviour
    {
        [SerializeField] private EProcGenPhase phase;
        [SerializeField, Range(0f, 1f)] private float intensity = 1f;

        public EProcGenPhase Phase => phase;

        public abstract void Execute(int numVertsPerSide, Vector3[] vertices, Color[] vertexColors, Vector3[] normals,
            Texture2D texture, ProcGenTileConfig config, ProcGenTile tile);
    }
}