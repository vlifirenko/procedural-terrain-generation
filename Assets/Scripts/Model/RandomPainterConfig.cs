using System;

namespace PTG.Model
{
    [Serializable]
    public class RandomPainterConfig
    {
        public TextureConfig textureToPaint;
        public float intensityModifier = 1f;
        public float noiseScale;
        public float noiseThreshold;
    }
}