using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BayatGames.Frameworks.Voxel3D.Definitions
{

    [CreateAssetMenu(menuName = "Voxel 3D/Layer")]
    public class LayerDefinition : ScriptableObject
    {

        [SerializeField]
        protected LayerType layerType;
        [Header("Absolute and additive layer parameters")]
        [Range(0, 256)]
        [Tooltip("Minimum height of this layer")]
        [SerializeField]
        protected int baseHeight = 0;
        [Range(1, 256)]
        [Tooltip("Distance between peaks")]
        [SerializeField]
        protected int frequency = 10;
        [Range(1, 256)]
        [Tooltip("The max height of peaks")]
        [SerializeField]
        protected int amplitude = 10;
        [Range(1, 3)]
        [Tooltip("Applies the height to the power of this value")]
        [SerializeField]
        protected float exponent = 1;
        [SerializeField]
        protected int chanceToSpawnBlock = 10;
        [SerializeField]
        protected BlockDefinition block;
        protected FastNoise noise;

        public virtual FastNoise Noise
        {
            get
            {
                return this.noise;
            }
            set
            {
                this.noise = value;
            }
        }

        public virtual LayerType LayerType
        {
            get
            {
                return this.layerType;
            }
        }

        public virtual BlockDefinition Block
        {
            get
            {
                return this.block;
            }
        }

        public virtual int Generate(int x, int z, int height)
        {
            Vector3Int position = new Vector3Int(x, height, z);
            if (layerType == LayerType.Chance)
            {
                if (GetNoise(x, -10555, z, 1, 100, 1) < chanceToSpawnBlock)
                {
                    VoxelManager.Instance.SetBlockAt(position, new Block(this.block));
                    return height + 1;
                }
                else
                {
                    return height;
                }
            }
            int maxHeight = GetNoise(x, 0, z, frequency, amplitude, exponent);
            maxHeight += baseHeight;
            if (layerType == LayerType.Absolute)
            {
                for (int y = height; y < maxHeight + VoxelManager.Instance.CurrentWorld.TerrainMinHeight; y++)
                {
                    position.y = y;
                    VoxelManager.Instance.SetBlockAt(position, new Block(this.block));
                }
            }
            else //additive or surface
            {
                for (int y = height; y < maxHeight + height; y++)
                {
                    position.y = y;
                    VoxelManager.Instance.SetBlockAt(position, new Block(this.block));
                }
            }
            if (layerType == LayerType.Additive || layerType == LayerType.Surface)
            {
                return height + maxHeight;
            }
            else //absolute
            {
                if (VoxelManager.Instance.CurrentWorld.TerrainMinHeight + maxHeight > height)
                {
                    return VoxelManager.Instance.CurrentWorld.TerrainMinHeight + maxHeight;
                }
            }
            return height;
        }

        public int GetNoise(int x, int y, int z, float scale, int max, float power)
        {
            float noise = (this.noise.GetNoise(x / scale, y / scale, z / scale) + 1f);
            noise *= (max / 2f);
            if (power != 1)
            {
                noise = Mathf.Pow(noise, power);
            }
            return Mathf.FloorToInt(noise);
        }

    }

}