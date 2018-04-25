using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;

using BayatGames.Frameworks.Voxel3D.Definitions;
using BayatGames.Frameworks.Voxel3D.Utilities;

namespace BayatGames.Frameworks.Voxel3D
{

    public class VoxelManager : MonoBehaviour
    {

        public static VoxelManager Instance { get; private set; }

        [SerializeField]
        protected List<WorldDefinition> worlds = new List<WorldDefinition>();
        [SerializeField]
        protected List<BiomeDefinition> biomes = new List<BiomeDefinition>();
        [SerializeField]
        protected List<BlockDefinition> blocks = new List<BlockDefinition>();
        [SerializeField]
        protected WorldDefinition currentWorld;
        [SerializeField]
        protected BiomeDefinition currentBiome;
        [SerializeField]
        protected TerrainGenerator terrainGenerator;
        [SerializeField]
        protected Vector3Int renderDistance = new Vector3Int(6, 6, 6);
        [SerializeField]
        protected Transform chunksContainer;
        [SerializeField]
        protected float targetFPS = 100f;
        [SerializeField]
        protected int maxThreads = 8;
        protected Dictionary<string, BlockDefinition> blocksDictionary = new Dictionary<string, BlockDefinition>();
        protected Dictionary<Texture2D, Rect> texturesUVDictionary = new Dictionary<Texture2D, Rect>();
        protected Dictionary<Vector3Int, TerrainChunk> chunks = new Dictionary<Vector3Int, TerrainChunk>();
        protected Queue<Vector3Int> chunkQueue = new Queue<Vector3Int>();
        protected Queue<TerrainChunk> updateQueue = new Queue<TerrainChunk>();
        protected float averageFPS;
        protected float deltaTimeFPS;
        protected int threadCount = 0;
        protected int meshesLastFrame = 0;
        protected bool isUpdatingChunks = false;
        protected bool isGenerating = false;
        protected TerrainChunk updatingChunk;
        protected WaitWhile waitWhileUpdatingChunk;

        public virtual List<WorldDefinition> Worlds
        {
            get
            {
                return this.worlds;
            }
        }

        public virtual List<BiomeDefinition> Biomes
        {
            get
            {
                return this.biomes;
            }
        }

        public virtual List<BlockDefinition> Blocks
        {
            get
            {
                return this.blocks;
            }
        }

        public virtual WorldDefinition CurrentWorld
        {
            get
            {
                return this.currentWorld;
            }
        }

        public virtual BiomeDefinition CurrentBiome
        {
            get
            {
                return this.currentBiome;
            }
        }

        public virtual BiomeDefinition DefaultBiome
        {
            get
            {
                return this.biomes.FirstOrDefault();
            }
        }

        public virtual Dictionary<Vector3Int, TerrainChunk> Chunks
        {
            get
            {
                return this.chunks;
            }
        }

        public virtual bool IsGenerating
        {
            get
            {
                return this.isGenerating;
            }
        }

        public virtual int ThreadCount
        {
            get
            {
                return this.threadCount;
            }
            set
            {
                this.threadCount++;
            }
        }

        protected virtual void Awake()
        {
            if (VoxelManager.Instance == null)
            {
                VoxelManager.Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
                return;
            }
            this.waitWhileUpdatingChunk = new WaitWhile(() =>
            {
                return this.updatingChunk != null && this.updatingChunk.IsUpdating;
            });
            for (int i = 0; i < this.worlds.Count; i++)
            {
                WorldDefinition world = this.worlds[i];
                for (int j = 0; j < world.Blocks.Count; j++)
                {
                    BlockDefinition block = world.Blocks[j];
                    if (!this.blocks.Contains(block))
                    {
                        this.blocks.Add(block);
                        this.blocksDictionary.Add(block.Identifier, block);
                    }
                }
            }
            for (int j = 0; j < this.currentWorld.TexturesUV.Count; j++)
            {
                TextureUV textureUV = this.currentWorld.TexturesUV[j];
                if (!this.texturesUVDictionary.ContainsKey(textureUV.Texture))
                {
                    this.texturesUVDictionary.Add(textureUV.Texture, textureUV.UV);
                }
            }
        }

        protected virtual void Update()
        {
            this.deltaTimeFPS += (Time.deltaTime - this.deltaTimeFPS) * 0.1f;
            this.averageFPS = Mathf.Lerp(this.averageFPS, 1f / this.deltaTimeFPS, 0.05f);
        }

        protected virtual void OnGUI()
        {
            GUILayout.Label(string.Format("Delta Time FPS: {0}", this.deltaTimeFPS), GUIStyle.none);
            GUILayout.Label(string.Format("Average FPS: {0}", this.averageFPS), GUIStyle.none);
        }

        protected virtual void LateUpdate()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //LoadChunks();
            UpdateChunks(stopwatch);
        }

        public virtual void LoadChunks()
        {
            int count = this.currentWorld.TerrainMaxHeight / this.currentWorld.ChunkPrefab.Size.y;
            if (this.isGenerating || this.chunkQueue.Count == 0 || this.chunkQueue.Count % count != 0)
            {
                return;
            }
            Vector3Int chunkPos = default(Vector3Int);
            for (int i = 0; i < count; i++)
            {
                chunkPos = this.chunkQueue.Dequeue();
                CreateChunkAt(chunkPos, true);
            }
        }

        public virtual void UpdateChunks(Stopwatch stopwatch)
        {
            if (this.isGenerating)
            {
                return;
            }
            this.meshesLastFrame = 0;
            int num = Mathf.RoundToInt(this.averageFPS - this.targetFPS);
            while (this.updateQueue.Count > 0 && !this.isGenerating)
            {
                TerrainChunk chunk = this.updateQueue.Dequeue();
                if (chunk != null)
                {
                    chunk.UpdateMesh();
                    this.meshesLastFrame++;
                    if (stopwatch.ElapsedMilliseconds >= num)
                    {
                        break;
                    }
                }
            }
            //if (this.updateQueue.Count == 0 || this.isUpdatingChunks)
            //{
            //    return;
            //}
            //StartCoroutine("UpdateChunksAsync");
        }

        protected virtual IEnumerator UpdateChunksAsync()
        {
            this.isUpdatingChunks = true;
            this.updatingChunk = this.updateQueue.Dequeue();
            if (this.updatingChunk != null)
            {
                this.updatingChunk.UpdateMeshAsync();
                yield return this.waitWhileUpdatingChunk;
                yield return CoroutineUtils.EndOfFrame;
            }
            this.isUpdatingChunks = false;
        }

        public virtual void DestroyChunkAt(Vector3Int position)
        {
            DestroyChunkAt(position, false);
        }

        public virtual void DestroyChunkAt(Vector3Int position, bool isChunkPosition)
        {
            if (!isChunkPosition)
            {
                position = ToChunkPosition(position);
                isChunkPosition = true;
            }
            TerrainChunk chunk = GetChunkAt(position, isChunkPosition);
            if (chunk != null)
            {
                this.chunks.Remove(position);
                Destroy(chunk.gameObject);
            }
        }

        public virtual void LoadChunkAt(Vector3Int position)
        {
            LoadChunkAt(position, false);
        }

        public virtual void LoadChunkAt(Vector3Int position, bool isChunkPosition)
        {
            if (!isChunkPosition)
            {
                position = ToChunkPosition(position);
                isChunkPosition = true;
            }
            if (!this.chunkQueue.Contains(position))
            {
                this.chunkQueue.Enqueue(position);
            }
        }

        public virtual TerrainChunk CreateChunkAt(Vector3Int position)
        {
            return CreateChunkAt(position, false);
        }

        public virtual TerrainChunk CreateChunkAt(Vector3Int position, bool isChunkPosition)
        {
            if (!isChunkPosition)
            {
                position = ToChunkPosition(position);
                isChunkPosition = true;
            }
            TerrainChunk chunk = Instantiate<TerrainChunk>(this.currentWorld.ChunkPrefab, position, Quaternion.identity, this.chunksContainer);
            this.chunks.Add(position, chunk);
            chunk.Initialize(position);
            if (position.y == this.currentWorld.TerrainMaxHeight)
            {
                Thread thread = new Thread(() =>
                {
                    this.threadCount++;
                    this.isGenerating = true;
                    this.terrainGenerator.GenerateTerrainForChunkColumn(position);
                    this.isGenerating = false;
                    for (int y = position.y; y > this.currentWorld.TerrainMinHeight; y -= this.currentWorld.ChunkPrefab.Size.y)
                    {
                        Vector3Int chunkPosition = new Vector3Int(position.x, y, position.z);
                        TerrainChunk chunkToUpdate = GetChunkAt(chunkPosition);
                        UpdateChunk(chunkToUpdate);
                        UpdateChunk(chunkToUpdate.RightChunk);
                        UpdateChunk(chunkToUpdate.LeftChunk);
                        UpdateChunk(chunkToUpdate.ForwardChunk);
                        UpdateChunk(chunkToUpdate.BackChunk);
                    }
                    this.threadCount--;
                });
                thread.Start();
            }
            return chunk;
        }

        public virtual void MarkAsUpdateRequired(params TerrainChunk[] chunks)
        {
            for (int i = 0; i < chunks.Length; i++)
            {
                TerrainChunk chunk = chunks[i];
                chunk.IsUpdateRequired = true;
            }
        }

        public virtual TerrainChunk[] GetAdjacentChunksAt(Vector3Int position, bool isChunkPosition)
        {
            Vector3Int[] positions = GetAdjacentChunksPositionAt(position, isChunkPosition);
            TerrainChunk[] chunks = new TerrainChunk[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                chunks[i] = GetChunkAt(positions[i], true);
            }
            return chunks;
        }

        public virtual void UpdateAdjacentChunksAt(Vector3Int position, bool isChunkPosition)
        {
            Vector3Int[] positions = GetAdjacentChunksPositionAt(position, isChunkPosition);
            for (int i = 0; i < positions.Length; i++)
            {
                UpdateChunkAt(positions[i], true);
            }
        }

        public virtual Vector3Int[] GetAdjacentChunksPositionAt(Vector3Int position, bool isChunkPosition)
        {
            if (!isChunkPosition)
            {
                position = ToChunkPosition(position);
            }
            return new Vector3Int[] {
                position + (new Vector3Int(0, 0, 1) * this.CurrentWorld.ChunkPrefab.Size.z),
                position + (new Vector3Int(0, 0, -1) * this.CurrentWorld.ChunkPrefab.Size.z),
                position + (new Vector3Int(1, 0, 0) * this.CurrentWorld.ChunkPrefab.Size.x),
                position + (new Vector3Int(-1, 0, 0) * this.CurrentWorld.ChunkPrefab.Size.x),
                position + (new Vector3Int(0, 1, 0) * this.CurrentWorld.ChunkPrefab.Size.y),
                position + (new Vector3Int(0, -1, 0) * this.CurrentWorld.ChunkPrefab.Size.y),
            };
        }

        public virtual void UpdateChunkAt(Vector3Int position)
        {
            UpdateChunkAt(position, false);
        }

        public virtual void UpdateChunkAt(Vector3Int position, bool isChunkPosition)
        {
            TerrainChunk chunk = GetChunkAt(position, isChunkPosition);
            UpdateChunk(chunk);
        }

        public virtual void UpdateChunk(TerrainChunk chunk)
        {
            if (chunk != null && chunk.IsUpdateRequired && !this.updateQueue.Contains(chunk))
            {
                this.updateQueue.Enqueue(chunk);
            }
        }

        public virtual Block GetBlockAt(Vector3Int position)
        {
            Block block = null;
            TerrainChunk chunk = GetChunkAt(position);
            if (chunk != null)
            {
                block = chunk.GetBlockAt(position, true);
            }
            return block;
        }

        public virtual Block SetBlockAt(Vector3Int position, Block block)
        {
            return SetBlockAt(position, block, false);
        }

        public virtual Block SetBlockAt(Vector3Int position, Block block, bool immediateUpdate)
        {
            Block oldBlock = null;
            TerrainChunk chunk = GetChunkAt(position);
            if (chunk != null)
            {
                oldBlock = chunk.SetBlockAt(position, block, true, immediateUpdate);
            }
            return oldBlock;
        }

        public virtual TerrainChunk GetChunkAt(Vector3Int position)
        {
            return GetChunkAt(position, false);
        }

        public virtual TerrainChunk GetChunkAt(Vector3Int position, bool isChunkPosition)
        {
            if (!isChunkPosition)
            {
                position = ToChunkPosition(position);
                isChunkPosition = true;
            }
            TerrainChunk chunk;
            this.chunks.TryGetValue(position, out chunk);
            return chunk;
        }

        public virtual Vector3Int ToChunkPosition(Vector3Int position)
        {
            return new Vector3Int(
                    Mathf.FloorToInt(position.x / (float)this.currentWorld.ChunkPrefab.Size.x) * this.currentWorld.ChunkPrefab.Size.x,
                    Mathf.FloorToInt(position.y / (float)this.currentWorld.ChunkPrefab.Size.y) * this.currentWorld.ChunkPrefab.Size.y,
                    Mathf.FloorToInt(position.z / (float)this.currentWorld.ChunkPrefab.Size.z) * this.currentWorld.ChunkPrefab.Size.z);
        }

        public virtual BiomeDefinition FindBiomeWithMoisture(float moisture)
        {
            return this.currentWorld.Biomes.Find(biome =>
            {
                return biome.MoistureMin <= moisture && biome.MoistureMax > moisture;
            });
        }

        public virtual Rect GetTextureUV(Texture2D texture)
        {
            if (texture == null)
            {
                return new Rect(0f, 0f, 0f, 0f);
            }
            return this.texturesUVDictionary[texture];
        }

    }

}