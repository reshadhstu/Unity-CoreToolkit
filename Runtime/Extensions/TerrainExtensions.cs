using UnityEngine;

namespace CoreToolkit.Runtime.Extensions
{
    public static class TerrainExtensions
    {
        public static Texture2D GetTextureAtPosition(this Terrain terrain, Vector3 position)
        {
            var mix = GetTextureMix();
            var splats = terrain.terrainData.terrainLayers;

            if(splats.Length == 0)
                return null;

            float maxMix = 0;
            int maxIndex = 0;

            for (int n = 0; n < mix.Length; ++n)
            {

                if (mix[n] > maxMix)
                {
                    maxIndex = n;
                    maxMix = mix[n];
                }
            }

            float[] GetTextureMix()
            {
                int mapX = (int)(((position.x - terrain.transform.position.x) / terrain.terrainData.size.x) * terrain.terrainData.alphamapWidth);
                int mapZ = (int)(((position.z - terrain.transform.position.z) / terrain.terrainData.size.z) * terrain.terrainData.alphamapHeight);
                float[,,] splatmapData = terrain.terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
                float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

                for (int n = 0; n < cellMix.Length; ++n)
                {
                    cellMix[n] = splatmapData[0, 0, n];
                }

                return cellMix;
            }

            return splats[maxIndex].diffuseTexture;
        }
    }
}