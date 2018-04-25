using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

using BayatGames.Frameworks.Voxel3D.Utilities;

namespace BayatGames.Frameworks.Voxel3D
{

    public class TerrainChunk : MonoBehaviour
    {

        [SerializeField]
        protected MeshRenderer meshRenderer;
        [SerializeField]
        protected MeshFilter meshFilter;
        [SerializeField]
        protected MeshCollider meshCollider;
        [SerializeField]
        protected Vector3Int size = new Vector3Int(16, 16, 16);
        protected Vector3Int position;
        protected Block[,,] blocks;
        protected bool isGenerated = false;
        [SerializeField]
        protected bool isUpdateRequired = false;
        protected TerrainChunk forwardChunk;
        protected TerrainChunk backChunk;
        protected TerrainChunk rightChunk;
        protected TerrainChunk leftChunk;
        protected TerrainChunk upChunk;
        protected TerrainChunk downChunk;
        protected ChunkFlags flags;
        protected int subMeshCount;
        protected bool isUpdating = false;
        protected WaitUntil waitUntilDone;
        protected bool isDone;

        public virtual MeshRenderer MeshRenderer
        {
            get
            {
                return this.meshRenderer;
            }
        }

        public virtual MeshFilter MeshFilter
        {
            get
            {
                return this.meshFilter;
            }
        }

        public virtual MeshCollider MeshCollider
        {
            get
            {
                return this.meshCollider;
            }
        }

        public virtual Vector3Int Size
        {
            get
            {
                return this.size;
            }
        }

        public virtual Vector3Int Position
        {
            get
            {
                return this.position;
            }
        }

        public virtual Block[,,] Blocks
        {
            get
            {
                return this.blocks;
            }
        }

        public virtual bool IsGenerated
        {
            get
            {
                return this.isGenerated;
            }
            set
            {
                this.isGenerated = value;
            }
        }

        public virtual bool IsUpdateRequired
        {
            get
            {
                return this.isUpdateRequired;
            }
            set
            {
                this.isUpdateRequired = value;
            }
        }

        public virtual bool IsUpdating
        {
            get
            {
                return this.isUpdating;
            }
        }

        public virtual TerrainChunk ForwardChunk
        {
            get
            {
                return VoxelManager.Instance.GetChunkAt(this.position + new Vector3Int(0, 0, this.size.z));
            }
        }

        public virtual TerrainChunk BackChunk
        {
            get
            {
                return VoxelManager.Instance.GetChunkAt(this.position - new Vector3Int(0, 0, this.size.z));
            }
        }

        public virtual TerrainChunk RightChunk
        {
            get
            {
                return VoxelManager.Instance.GetChunkAt(this.position + new Vector3Int(this.size.x, 0, 0));
            }
        }

        public virtual TerrainChunk LeftChunk
        {
            get
            {
                return VoxelManager.Instance.GetChunkAt(this.position - new Vector3Int(this.size.x, 0, 0));
            }
        }

        public virtual TerrainChunk UpChunk
        {
            get
            {
                return VoxelManager.Instance.GetChunkAt(this.position + new Vector3Int(0, this.size.y, 0));
            }
        }

        public virtual TerrainChunk DownChunk
        {
            get
            {
                return VoxelManager.Instance.GetChunkAt(this.position - new Vector3Int(0, this.size.y, 0));
            }
        }

        //protected virtual void LateUpdate()
        //{
        //    if (this.isUpdateRequired && this.isGenerated)
        //    {
        //        VoxelManager.Instance.UpdateChunk(this);
        //    }
        //}

        public virtual void Initialize(Vector3Int position)
        {
            this.waitUntilDone = new WaitUntil(() =>
            {
                return this.isDone;
            });
            this.name = string.Format("Chunk At {0}", position);
            this.subMeshCount = this.meshRenderer.materials.Length;
            this.position = position;
            this.blocks = new Block[this.size.x, this.size.y, this.size.z];
            for (int i = 0; i < this.meshRenderer.materials.Length; i++)
            {
                Material material = this.meshRenderer.materials[i];
                if (material != null)
                {
                    material.mainTexture = VoxelManager.Instance.CurrentWorld.SheetTexture;
                }
            }
            this.isGenerated = false;
        }

        public virtual void UpdateMesh()
        {
            this.isUpdating = true;
            MeshData meshData = BuildMesh();
            SetMesh(meshData.BuildMesh());
            this.isUpdateRequired = false;
            this.isUpdating = false;
        }

        public virtual void UpdateMeshAsync()
        {
            StartCoroutine("UpdateMeshCoroutine");
        }

        protected virtual IEnumerator UpdateMeshCoroutine()
        {
            this.isUpdating = true;
            MeshData meshData = null;
            this.isDone = false;
            Thread thread = new Thread(() =>
            {
                VoxelManager.Instance.ThreadCount++;
                meshData = BuildMesh();
                this.isDone = true;
                VoxelManager.Instance.ThreadCount--;
            });
            thread.Start();
            yield return this.waitUntilDone;
            yield return CoroutineUtils.EndOfFrame;
            SetMesh(meshData.BuildMesh());
            this.isUpdateRequired = false;
            this.isUpdating = false;
        }

        public virtual MeshData BuildMesh()
        {
            MeshData meshData = new MeshData(this.subMeshCount);
            for (int x = 0; x < this.size.x; x++)
            {
                for (int z = 0; z < this.size.z; z++)
                {
                    for (int y = 0; y < this.size.y; y++)
                    {
                        Vector3Int position = new Vector3Int(x, y, z);
                        Block block = this.blocks[x, y, z];
                        if (block != null)
                        {
                            block.Definition.AddMeshData(position, meshData, this, block);
                        }
                    }
                }
            }
            return meshData;
        }

        public virtual void SetMesh(Mesh mesh)
        {
            this.meshFilter.mesh = null;
            this.meshFilter.mesh = mesh;
            this.meshCollider.sharedMesh = null;
            this.meshCollider.sharedMesh = mesh;
        }

        public virtual Block GetBlockAt(Vector3Int position)
        {
            return GetBlockAt(position, false);
        }

        public virtual Block GetBlockAt(Vector3Int position, bool worldPosition)
        {
            Vector3Int blockPosition = position;
            if (worldPosition)
            {
                blockPosition = position - this.position;
            }
            else
            {
                position = position + this.position;
            }
            if (blockPosition.x >= 0 && blockPosition.x < this.size.x && blockPosition.y >= 0 && blockPosition.y < this.size.y && blockPosition.z >= 0 && blockPosition.z < this.size.z)
            {
                return this.blocks[blockPosition.x, blockPosition.y, blockPosition.z];
            }
            else
            {
                return VoxelManager.Instance.GetBlockAt(position);
            }
        }

        public virtual Block SetBlockAt(Vector3Int position, Block block)
        {
            return SetBlockAt(position, block, false, false);
        }

        public virtual Block SetBlockAt(Vector3Int position, Block block, bool worldPosition)
        {
            return SetBlockAt(position, block, worldPosition, false);
        }

        public virtual Block SetBlockAt(Vector3Int position, Block block, bool worldPosition, bool immediateUpdate)
        {
            Vector3Int blockPosition = position;
            if (worldPosition)
            {
                blockPosition = position - this.position;
            }
            else
            {
                position = position + this.position;
            }
            if (blockPosition.x >= 0 && blockPosition.x < this.size.x && blockPosition.y >= 0 && blockPosition.y < this.size.y && blockPosition.z >= 0 && blockPosition.z < this.size.z)
            {
                Block oldBlock = this.blocks[blockPosition.x, blockPosition.y, blockPosition.z];
                this.blocks[blockPosition.x, blockPosition.y, blockPosition.z] = block;
                this.isUpdateRequired = true;
                if (immediateUpdate)
                {
                    this.UpdateMesh();
                }
                if (blockPosition.x == 0)
                {
                    TerrainChunk chunk = VoxelManager.Instance.GetChunkAt(position - new Vector3Int(1, 0, 0), false);
                    if (chunk != null)
                    {
                        chunk.IsUpdateRequired = true;
                        if (immediateUpdate)
                        {
                            chunk.UpdateMesh();
                        }
                    }
                }
                if (blockPosition.x == this.size.x - 1)
                {
                    TerrainChunk chunk = VoxelManager.Instance.GetChunkAt(position + new Vector3Int(1, 0, 0), false);
                    if (chunk != null)
                    {
                        chunk.IsUpdateRequired = true;
                        if (immediateUpdate)
                        {
                            chunk.UpdateMesh();
                        }
                    }
                }
                if (blockPosition.y == 0)
                {
                    TerrainChunk chunk = VoxelManager.Instance.GetChunkAt(position - new Vector3Int(0, 1, 0), false);
                    if (chunk != null)
                    {
                        chunk.IsUpdateRequired = true;
                        if (immediateUpdate)
                        {
                            chunk.UpdateMesh();
                        }
                    }
                }
                if (blockPosition.y == this.size.y - 1)
                {
                    TerrainChunk chunk = VoxelManager.Instance.GetChunkAt(position + new Vector3Int(0, 1, 0), false);
                    if (chunk != null)
                    {
                        chunk.IsUpdateRequired = true;
                        if (immediateUpdate)
                        {
                            chunk.UpdateMesh();
                        }
                    }
                }
                if (blockPosition.z == 0)
                {
                    TerrainChunk chunk = VoxelManager.Instance.GetChunkAt(position - new Vector3Int(0, 0, 1), false);
                    if (chunk != null)
                    {
                        chunk.IsUpdateRequired = true;
                        if (immediateUpdate)
                        {
                            chunk.UpdateMesh();
                        }
                    }
                }
                if (blockPosition.z == this.size.z - 1)
                {
                    TerrainChunk chunk = VoxelManager.Instance.GetChunkAt(position + new Vector3Int(0, 0, 1), false);
                    if (chunk != null)
                    {
                        chunk.IsUpdateRequired = true;
                        if (immediateUpdate)
                        {
                            chunk.UpdateMesh();
                        }
                    }
                }
                return oldBlock;
            }
            else
            {
                return VoxelManager.Instance.SetBlockAt(position, block, immediateUpdate);
            }
        }

        public virtual bool HasFlag(ChunkFlags flag)
        {
            return (this.flags & flag) == flag;
        }

        public virtual void AddFlag(ChunkFlags flag)
        {
            this.flags |= flag;
        }

        public virtual void RemoveFlag(ChunkFlags flag)
        {
            this.flags &= ~flag;
        }

    }

}