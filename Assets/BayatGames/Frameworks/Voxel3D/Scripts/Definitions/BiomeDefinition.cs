using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BayatGames.Frameworks.Voxel3D.Definitions
{

    [CreateAssetMenu(menuName = "Voxel 3D/Biome")]
    public class BiomeDefinition : ScriptableObject
    {

        [SerializeField]
        protected float moistureMin = 0f;
        [SerializeField]
        protected float moistureMax = 1f;
        [SerializeField]
        protected List<LayerDefinition> layers;
        [SerializeField]
        protected BlockDefinition air;

        public virtual float MoistureMin
        {
            get
            {
                return this.moistureMin;
            }
        }

        public virtual float MoistureMax
        {
            get
            {
                return this.moistureMax;
            }
        }

        public virtual List<LayerDefinition> Layers
        {
            get
            {
                return this.layers;
            }
        }

        public virtual BlockDefinition Air
        {
            get
            {
                return this.air;
            }
        }

        public virtual List<BlockDefinition> Blocks
        {
            get
            {
                List<BlockDefinition> blocks = new List<BlockDefinition>();
                for (int i = 0; i < this.layers.Count; i++)
                {
                    LayerDefinition layer = this.layers[i];
                    if (!blocks.Contains(layer.Block))
                    {
                        blocks.Add(layer.Block);
                    }
                }
                return blocks;
            }
        }

    }

}