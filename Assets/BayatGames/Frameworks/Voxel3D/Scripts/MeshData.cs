using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BayatGames.Frameworks.Voxel3D
{

    [Serializable]
    public class MeshData
    {

        [SerializeField]
        protected List<Vector3> vertices = new List<Vector3>();
        [SerializeField]
        protected List<List<int>> triangles = new List<List<int>>();
        [SerializeField]
        protected List<Vector2> uv = new List<Vector2>();

        public virtual int SubMeshCount
        {
            get
            {
                return this.triangles.Count;
            }
            set
            {
                if (value > this.triangles.Count)
                {
                    int difference = value - this.triangles.Count;
                    for (int i = 0; i < difference; i++)
                    {
                        this.triangles.Add(new List<int>());
                    }
                }
                else if (this.triangles.Count > value)
                {
                    int difference = this.triangles.Count - value;
                    for (int i = 0; i < difference; i++)
                    {
                        this.triangles.RemoveAt(this.triangles.Count - i);
                    }
                }
            }
        }

        public virtual List<Vector3> Vertices
        {
            get
            {
                return this.vertices;
            }
        }

        public virtual List<List<int>> Triangles
        {
            get
            {
                return this.triangles;
            }
        }

        public virtual List<Vector2> UV
        {
            get
            {
                return this.uv;
            }
        }

        public MeshData() : this(1)
        {
        }

        public MeshData(int subMeshCount)
        {
            this.SubMeshCount = subMeshCount;
        }

        public virtual void AddVertex(Vector3 vertex)
        {
            this.vertices.Add(vertex);
        }

        public virtual void AddTriangle(int subMesh, int triangle)
        {
            if (subMesh >= this.triangles.Count)
            {
                return;
            }
            this.triangles[subMesh].Add(triangle);
        }

        public virtual void AddUV(Vector2 uv)
        {
            this.uv.Add(uv);
        }

        public virtual void AddCube(Vector3 position, Rect[] uvs)
        {
            AddCube(position, 0, uvs);
        }

        public virtual void AddCube(Vector3 position, int subMesh, Rect[] uvs)
        {
            AddQuad(position, subMesh, VoxelDirection.Forward, uvs[0]);
            AddQuad(position, subMesh, VoxelDirection.Back, uvs[1]);
            AddQuad(position, subMesh, VoxelDirection.Right, uvs[2]);
            AddQuad(position, subMesh, VoxelDirection.Left, uvs[3]);
            AddQuad(position, subMesh, VoxelDirection.Up, uvs[4]);
            AddQuad(position, subMesh, VoxelDirection.Down, uvs[5]);
        }

        public virtual void AddQuad(Vector3 position, VoxelDirection direction, Rect uv)
        {
            AddQuad(position, 0, direction, uv);
        }

        public virtual void AddQuad(Vector3 position, int subMesh, VoxelDirection direction, Rect uv)
        {
            switch (direction)
            {
                case VoxelDirection.Forward:
                    this.vertices.Add(position + Vector3.forward + Vector3.left);
                    this.vertices.Add(position + Vector3.forward);
                    this.vertices.Add(position + Vector3.forward + Vector3.left + Vector3.up);
                    this.vertices.Add(position + Vector3.forward + Vector3.up);
                    break;
                case VoxelDirection.Back:
                    this.vertices.Add(position);
                    this.vertices.Add(position + Vector3.left);
                    this.vertices.Add(position + Vector3.up);
                    this.vertices.Add(position + Vector3.left + Vector3.up);
                    break;
                case VoxelDirection.Right:
                    this.vertices.Add(position + Vector3.forward);
                    this.vertices.Add(position);
                    this.vertices.Add(position + Vector3.forward + Vector3.up);
                    this.vertices.Add(position + Vector3.up);
                    break;
                case VoxelDirection.Left:
                    this.vertices.Add(position + Vector3.left);
                    this.vertices.Add(position + Vector3.left + Vector3.forward);
                    this.vertices.Add(position + Vector3.left + Vector3.up);
                    this.vertices.Add(position + Vector3.left + Vector3.forward + Vector3.up);
                    break;
                case VoxelDirection.Up:
                    this.vertices.Add(position + Vector3.up);
                    this.vertices.Add(position + Vector3.up + Vector3.left);
                    this.vertices.Add(position + Vector3.up + Vector3.forward);
                    this.vertices.Add(position + Vector3.up + Vector3.forward + Vector3.left);
                    break;
                case VoxelDirection.Down:
                    this.vertices.Add(position + Vector3.forward);
                    this.vertices.Add(position + Vector3.forward + Vector3.left);
                    this.vertices.Add(position);
                    this.vertices.Add(position + Vector3.left);
                    break;
            }
            this.triangles[subMesh].Add(this.vertices.Count - 4);
            this.triangles[subMesh].Add(this.vertices.Count - 3);
            this.triangles[subMesh].Add(this.vertices.Count - 2);
            this.triangles[subMesh].Add(this.vertices.Count - 3);
            this.triangles[subMesh].Add(this.vertices.Count - 1);
            this.triangles[subMesh].Add(this.vertices.Count - 2);
            this.uv.Add(new Vector2(uv.x + uv.width, uv.y));
            this.uv.Add(new Vector2(uv.x, uv.y));
            this.uv.Add(new Vector2(uv.x + uv.width, uv.y + uv.height));
            this.uv.Add(new Vector2(uv.x, uv.y + uv.height));
        }

        public virtual Mesh BuildMesh()
        {
            Mesh mesh = new Mesh();
            mesh.subMeshCount = this.triangles.Count;
            mesh.vertices = this.vertices.ToArray();
            for (int i = 0; i < this.triangles.Count; i++)
            {
                mesh.SetTriangles(this.triangles[i], i, true);
            }
            mesh.uv = this.uv.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }

    }

}