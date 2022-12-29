using PTG.Model.Config;
using UnityEngine;

namespace PTG.Tile.Modifiers
{
    public class PaintTileModifier_SimpleHeight : BaseTileModifier
    {
        public override void Execute(int numVertsPerSide, Vector3[] vertices, Color[] vertexColors, Vector3[] normals, Texture2D texture,
            ProcGenTileConfig config, ProcGenTile tile)
        {
            for (var row = 0; row < numVertsPerSide; row++)
            {
                for (var col = 0; col < numVertsPerSide; col++)
                {
                    var vertIndex = row * numVertsPerSide + col;
                    var heightProgress = vertices[vertIndex].y / config.maxHeight;

                    vertexColors[vertIndex] = Color.Lerp(Color.green, Color.white, heightProgress);
                }
            }
        }
    }
}