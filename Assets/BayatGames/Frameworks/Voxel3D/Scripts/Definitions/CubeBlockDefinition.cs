using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BayatGames.Frameworks.Voxel3D.Definitions
{

    [CreateAssetMenu(menuName = "Voxel 3D/Blocks/Cube")]
    public class CubeBlockDefinition : BlockDefinition
    {

        [Serializable]
        public struct CubeFace
        {

            [SerializeField]
            private bool solid;
            [SerializeField]
            private Texture2D texture;
            [SerializeField]
            private int subMesh;

            public bool IsSolid
            {
                get
                {
                    return this.solid;
                }
            }

            public Texture2D Texture
            {
                get
                {
                    return this.texture;
                }
            }

            public int SubMesh
            {
                get
                {
                    return this.subMesh;
                }
            }

        }

        [SerializeField]
        protected CubeFace forwardFace;
        [SerializeField]
        protected CubeFace backFace;
        [SerializeField]
        protected CubeFace rightFace;
        [SerializeField]
        protected CubeFace leftFace;
        [SerializeField]
        protected CubeFace upFace;
        [SerializeField]
        protected CubeFace downFace;

        public override Texture2D[] Textures
        {
            get
            {
                return new Texture2D[] {
                    this.forwardFace.Texture,
                    this.backFace.Texture,
                    this.rightFace.Texture,
                    this.leftFace.Texture,
                    this.upFace.Texture,
                    this.downFace.Texture
                };
            }
        }

        public override void AddMeshData(Vector3Int position, MeshData meshData, TerrainChunk chunk, Block block)
        {
            if (this.emptyMesh)
            {
                return;
            }
            //Rect[] uvs = new Rect[6] {
            //    VoxelManager.Instance.GetTextureUV(this.forwardTexture),
            //    VoxelManager.Instance.GetTextureUV(this.backTexture),
            //    VoxelManager.Instance.GetTextureUV(this.rightTexture),
            //    VoxelManager.Instance.GetTextureUV(this.leftTexture),
            //    VoxelManager.Instance.GetTextureUV(this.upTexture),
            //    VoxelManager.Instance.GetTextureUV(this.downTexture)
            //};
            Block adjacentBlock = chunk.GetBlockAt(position + new Vector3Int(0, 0, 1), false);
            if (adjacentBlock == null || !adjacentBlock.Definition.IsSolid(VoxelDirection.Back))
            {
                meshData.AddQuad(position, this.forwardFace.SubMesh, VoxelDirection.Forward, VoxelManager.Instance.GetTextureUV(this.forwardFace.Texture));
            }
            adjacentBlock = chunk.GetBlockAt(position + new Vector3Int(0, 0, -1), false);
            if (adjacentBlock == null || !adjacentBlock.Definition.IsSolid(VoxelDirection.Forward))
            {
                meshData.AddQuad(position, this.backFace.SubMesh, VoxelDirection.Back, VoxelManager.Instance.GetTextureUV(this.backFace.Texture));
            }
            adjacentBlock = chunk.GetBlockAt(position + new Vector3Int(1, 0, 0), false);
            if (adjacentBlock == null || !adjacentBlock.Definition.IsSolid(VoxelDirection.Left))
            {
                meshData.AddQuad(position, this.rightFace.SubMesh, VoxelDirection.Right, VoxelManager.Instance.GetTextureUV(this.rightFace.Texture));
            }
            adjacentBlock = chunk.GetBlockAt(position + new Vector3Int(-1, 0, 0), false);
            if (adjacentBlock == null || !adjacentBlock.Definition.IsSolid(VoxelDirection.Right))
            {
                meshData.AddQuad(position, this.leftFace.SubMesh, VoxelDirection.Left, VoxelManager.Instance.GetTextureUV(this.leftFace.Texture));
            }
            adjacentBlock = chunk.GetBlockAt(position + new Vector3Int(0, 1, 0), false);
            if (adjacentBlock == null || !adjacentBlock.Definition.IsSolid(VoxelDirection.Down))
            {
                meshData.AddQuad(position, this.upFace.SubMesh, VoxelDirection.Up, VoxelManager.Instance.GetTextureUV(this.upFace.Texture));
            }
            adjacentBlock = chunk.GetBlockAt(position + new Vector3Int(0, -1, 0), false);
            if (adjacentBlock == null || !adjacentBlock.Definition.IsSolid(VoxelDirection.Up))
            {
                meshData.AddQuad(position, this.downFace.SubMesh, VoxelDirection.Down, VoxelManager.Instance.GetTextureUV(this.downFace.Texture));
            }
        }

        public override bool IsSolid(VoxelDirection direction)
        {
            switch (direction)
            {
                case VoxelDirection.Forward:
                    return this.forwardFace.IsSolid;
                case VoxelDirection.Back:
                    return this.backFace.IsSolid;
                case VoxelDirection.Right:
                    return this.rightFace.IsSolid;
                case VoxelDirection.Left:
                    return this.leftFace.IsSolid;
                case VoxelDirection.Up:
                    return this.upFace.IsSolid;
                case VoxelDirection.Down:
                    return this.downFace.IsSolid;
                default:
                    throw new InvalidOperationException();
            }
        }

    }

}