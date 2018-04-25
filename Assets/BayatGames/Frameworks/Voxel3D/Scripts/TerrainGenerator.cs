using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


using BayatGames.Frameworks.Voxel3D.Definitions;

namespace BayatGames.Frameworks.Voxel3D
{

    public class TerrainGenerator : MonoBehaviour
    {

        protected FastNoise noise;

        protected int temperatureScale = 100;
        protected int drainageScale = 100;
        protected int elevationScale = 100;
        protected int rainfallScale = 100;

        protected virtual void Awake()
        {
            int seed = Random.Range(int.MinValue, int.MaxValue);
            this.noise = new FastNoise(seed);
            this.noise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            this.noise.SetInterp(FastNoise.Interp.Quintic);
            this.noise.SetFractalType(FastNoise.FractalType.FBM);
            //this.noise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Natural);
            this.noise.SetCellularJitter(0.45f);
            this.noise.SetCellularReturnType(FastNoise.CellularReturnType.CellValue);
            this.noise.SetFrequency(1f);
            this.noise.SetFractalGain(1f);
            this.noise.SetFractalLacunarity(1f);
            this.noise.SetFractalOctaves(5);
        }

        public virtual void GenerateTerrainForChunkColumn(Vector3Int position)
        {
            for (int x = position.x; x < position.x + VoxelManager.Instance.CurrentWorld.ChunkPrefab.Size.x; x++)
            {
                for (int z = position.z; z < position.z + VoxelManager.Instance.CurrentWorld.ChunkPrefab.Size.z; z++)
                {
                    GenerateTerrainForBlockColumn(x, z);
                }
            }
        }

        public int GenerateTerrainForBlockColumn(int x, int z)
        {
            int height = VoxelManager.Instance.CurrentWorld.TerrainMinHeight;
            float moisture = this.noise.GetCellular(x / 500f, 0, z / 500f);
            BiomeDefinition biome = VoxelManager.Instance.FindBiomeWithMoisture(moisture);
            if (biome == null)
            {
                biome = VoxelManager.Instance.DefaultBiome;
            }
            List<LayerDefinition> layers = biome.Layers;
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i] == null)
                {
                    Debug.LogError("Layer name '" + layers[i] + "' in layer order didn't match a valid layer");
                    continue;
                }
                layers[i].Noise = this.noise;
                if (layers[i].LayerType != LayerType.Structure)
                {
                    height = layers[i].Generate(x, z, height);
                }
            }
            return height;
        }

    }

}