using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BayatGames.Frameworks.Voxel3D.Definitions
{

    public abstract class BlockDefinition : ScriptableObject
    {

        [SerializeField]
        protected string identifier;
        [SerializeField]
        protected bool emptyMesh = false;

        public virtual string Identifier
        {
            get
            {
                return this.identifier;
            }
        }

        public virtual bool EmptyMesh
        {
            get
            {
                return this.emptyMesh;
            }
        }

        public abstract Texture2D[] Textures { get; }

        public abstract void AddMeshData(Vector3Int position, MeshData meshData, TerrainChunk chunk, Block block);

        public abstract bool IsSolid(VoxelDirection direction);

    }

}