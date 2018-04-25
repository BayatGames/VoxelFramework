using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using BayatGames.Frameworks.Voxel3D.Definitions;
using BayatGames.Frameworks.Voxel3D.Utilities;

namespace BayatGames.Frameworks.Voxel3D
{

    public class ChunkLoaderAgent : MonoBehaviour
    {

        [SerializeField]
        protected int loadRadius = 8;
        [SerializeField]
        protected int waitBetweenDeletes = 10;
        [SerializeField]
        protected int waitBetweenGenerates = 1;
        protected int deleteTimer = 0;
        protected int generateTimer = 0;
        protected Vector3Int[] chunkLoadOrder;
        protected bool isLoadingChunks = false;
        [SerializeField]
        protected BlockDefinition block;
        [SerializeField]
        protected Transform placeHighlight;
        [SerializeField]
        protected Transform destroyHighlight;
        [SerializeField]
        protected GameObject light;
        protected WaitWhile waitWhileGenerating = new WaitWhile(() =>
        {
            return VoxelManager.Instance.IsGenerating;
        });

        protected virtual void Awake()
        {
            List<Vector3Int> chunkLoads = new List<Vector3Int>();
            for (int x = -this.loadRadius; x <= this.loadRadius; x++)
            {
                for (int z = -this.loadRadius; z <= this.loadRadius; z++)
                {
                    chunkLoads.Add(new Vector3Int(x, 0, z));
                }
            }

            // limit how far away the blocks can be to achieve a circular loading pattern
            float maxRadius = this.loadRadius * 1.55f;

            //sort 2d vectors by closeness to center
            this.chunkLoadOrder = chunkLoads
                                .Where(pos => Mathf.Abs(pos.x) + Mathf.Abs(pos.z) < maxRadius)
                                .OrderBy(pos => Mathf.Abs(pos.x) + Mathf.Abs(pos.z)) //smallest magnitude vectors first
                                .ThenBy(pos => Mathf.Abs(pos.x)) //make sure not to process e.g (-10,0) before (5,5)
                                .ThenBy(pos => Mathf.Abs(pos.z))
                                .ToArray();
        }

        protected virtual void Update()
        {
            RaycastHit hit;
            Vector3 origin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(origin, Camera.main.transform.forward, out hit, 100f, LayerMask.GetMask("Terrain")))
            {
                TerrainChunk chunk = hit.collider.GetComponent<TerrainChunk>();
                Debug.DrawLine(origin, hit.point, Color.red);
                Vector3Int normal = Truncate(hit.normal);
                Vector3Int placePosition = FloorToInt(hit.point);
                Vector3Int destroyPosition = FloorToInt(hit.point);
                if (normal.x > 0)
                {
                    destroyPosition.x -= normal.x;
                }
                if (normal.y > 0)
                {
                    destroyPosition.y -= normal.y;
                }
                if (normal.z > 0)
                {
                    destroyPosition.z -= normal.z;
                }
                if (normal.x < 0)
                {
                    placePosition.x += normal.x;
                }
                if (normal.y < 0)
                {
                    placePosition.y += normal.y;
                }
                if (normal.z < 0)
                {
                    placePosition.z += normal.z;
                }
                this.destroyHighlight.transform.position = destroyPosition;
                this.placeHighlight.transform.position = placePosition;
                placePosition.x++;
                destroyPosition.x++;
                if (Input.GetMouseButton(0))
                {
                    VoxelManager.Instance.SetBlockAt(destroyPosition, null, true);
                }
                if (Input.GetMouseButton(1))
                {
                    VoxelManager.Instance.SetBlockAt(placePosition, new Block(this.block), true);
                }
            }
        }

        protected virtual Vector3Int FloorToInt(Vector3 vector)
        {
            Vector3Int vectorInt = new Vector3Int();
            vectorInt.x = Mathf.FloorToInt(vector.x);
            vectorInt.y = Mathf.FloorToInt(vector.y);
            vectorInt.z = Mathf.FloorToInt(vector.z);
            return vectorInt;
        }

        protected virtual Vector3Int Truncate(Vector3 vector)
        {
            Vector3Int vectorInt = new Vector3Int();
            vectorInt.x = (int)Decimal.Truncate((decimal)vector.x);
            vectorInt.y = (int)Decimal.Truncate((decimal)vector.y);
            vectorInt.z = (int)Decimal.Truncate((decimal)vector.z);
            return vectorInt;
        }

        protected virtual void LateUpdate()
        {
            if (this.deleteTimer == this.waitBetweenDeletes)
            {
                DeleteChunks();
                this.deleteTimer = 0;
                return;
            }
            else
            {
                this.deleteTimer++;
            }
            if (this.isLoadingChunks)
            {
                this.generateTimer = 0;
            }
            else
            {
                if (this.generateTimer == this.waitBetweenGenerates)
                {
                    StartCoroutine("FindChunksAndLoad");
                    //FindChunksAndLoad();
                    this.generateTimer = 0;
                    return;
                }
                else if (this.generateTimer != this.waitBetweenGenerates)
                {
                    this.generateTimer++;
                }
            }
        }

        void DeleteChunks()
        {
            List<Vector3Int> chunksToDelete = new List<Vector3Int>();
            foreach (var chunk in VoxelManager.Instance.Chunks)
            {
                Vector3 chunkPosition = chunk.Key;
                float distance = Vector3.Distance(
                    new Vector3(chunkPosition.x, 0, chunkPosition.z),
                    new Vector3(transform.position.x, 0, transform.position.z));
                if (distance > 16 * 8 * 1.5f)
                {
                    chunksToDelete.Add(chunk.Key);
                }
            }
            foreach (var chunk in chunksToDelete)
            {
                VoxelManager.Instance.DestroyChunkAt(chunk, true);
            }
        }

        protected virtual IEnumerator FindChunksAndLoad()
        {
            this.isLoadingChunks = true;
            //Cycle through the array of positions
            for (int i = 0; i < this.chunkLoadOrder.Length; i++)
            {
                //Get the position of this gameobject to generate around
                Vector3Int playerPos = VoxelManager.Instance.ToChunkPosition(new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z));

                //translate the player position and array position into chunk position
                Vector3Int newChunkPos = new Vector3Int(
                    this.chunkLoadOrder[i].x * VoxelManager.Instance.CurrentWorld.ChunkPrefab.Size.x + playerPos.x,
                    0,
                    this.chunkLoadOrder[i].z * VoxelManager.Instance.CurrentWorld.ChunkPrefab.Size.y + playerPos.z);

                //Get the chunk in the defined position
                TerrainChunk newChunk = VoxelManager.Instance.GetChunkAt(newChunkPos, true);

                //If the chunk already exists and it's already
                //rendered or in queue to be rendered continue
                if (newChunk != null)
                {
                    continue;
                }
                LoadChunkColumn(newChunkPos);
                //yield return this.waitWhileGenerating;
                yield return CoroutineUtils.EndOfFrame;
            }
            this.isLoadingChunks = false;
        }

        protected virtual void LoadChunkColumn(Vector3Int columnPosition)
        {
            //First create the chunk game objects in the world class
            //The world class wont do any generation when threaded chunk creation is enabled
            for (int y = VoxelManager.Instance.CurrentWorld.TerrainMinHeight; y <= VoxelManager.Instance.CurrentWorld.TerrainMaxHeight; y += VoxelManager.Instance.CurrentWorld.ChunkPrefab.Size.y)
            {
                Vector3Int position = new Vector3Int(columnPosition.x, y, columnPosition.z);
                TerrainChunk chunk = VoxelManager.Instance.GetChunkAt(position, true);
                if (chunk == null)
                {
                    VoxelManager.Instance.CreateChunkAt(position, true);
                }
            }
        }

    }

}