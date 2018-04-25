using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace BayatGames.Frameworks.Voxel3D.Definitions
{

    [Serializable]
    public struct TextureUV
    {

        [SerializeField]
        private Texture2D texture;
        [SerializeField]
        private Rect uv;

        public Texture2D Texture
        {
            get
            {
                return this.texture;
            }
        }

        public Rect UV
        {
            get
            {
                return this.uv;
            }
        }

        public TextureUV(Texture2D texture, Rect uv)
        {
            this.texture = texture;
            this.uv = uv;
        }
    }

    [CreateAssetMenu(menuName = "Voxel 3D/World")]
    public class WorldDefinition : ScriptableObject
    {

        [SerializeField]
        protected List<BiomeDefinition> biomes;
        [SerializeField]
        protected int terrainMaxHeight = 128;
        [SerializeField]
        protected int terrainMinHeight = 0;
        [SerializeField]
        protected TerrainChunk chunkPrefab;
        [SerializeField]
        protected Texture2D sheetTexture;
        [SerializeField]
        protected List<TextureUV> texturesUV;

        public virtual List<BiomeDefinition> Biomes
        {
            get
            {
                return this.biomes;
            }
        }

        public virtual int TerrainMaxHeight
        {
            get
            {
                return this.terrainMaxHeight;
            }
        }

        public virtual int TerrainMinHeight
        {
            get
            {
                return this.terrainMinHeight;
            }
        }

        public virtual TerrainChunk ChunkPrefab
        {
            get
            {
                return this.chunkPrefab;
            }
        }

        public virtual Texture2D SheetTexture
        {
            get
            {
                return this.sheetTexture;
            }
        }

        public virtual List<TextureUV> TexturesUV
        {
            get
            {
                return this.texturesUV;
            }
        }

        public virtual List<BlockDefinition> Blocks
        {
            get
            {
                List<BlockDefinition> blocks = new List<BlockDefinition>();
                for (int i = 0; i < this.biomes.Count; i++)
                {
                    BiomeDefinition biome = this.biomes[i];
                    for (int j = 0; j < biome.Blocks.Count; j++)
                    {
                        BlockDefinition block = biome.Blocks[j];
                        if (!blocks.Contains(block))
                        {
                            blocks.Add(block);
                        }
                    }
                }
                return blocks;
            }
        }

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            this.texturesUV.Clear();
            string assetPath = "";
            if (this.sheetTexture == null)
            {
                assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
                assetPath = Path.Combine(assetPath, this.name + " Sheet.png");
            }
            else
            {
                assetPath = AssetDatabase.GetAssetPath(this.sheetTexture);
            }
            Texture2D sheet = new Texture2D(0, 0, TextureFormat.ARGB32, false);
            List<Texture2D> textures = new List<Texture2D>();
            List<BlockDefinition> blocks = this.Blocks;
            for (int i = 0; i < blocks.Count; i++)
            {
                BlockDefinition block = blocks[i];
                foreach (Texture2D texture in block.Textures)
                {
                    if (!textures.Contains(texture))
                    {
                        textures.Add(texture);
                    }
                }
            }
            Rect[] uvs = sheet.PackTextures(textures.ToArray(), 0, 2048, false);
            for (int i = 0; i < uvs.Length; i++)
            {
                Rect uv = uvs[i];
                Texture2D texture = textures[i];
                TextureUV textureUV = new TextureUV(texture, uv);
                this.texturesUV.Add(textureUV);
            }
            File.WriteAllBytes(assetPath, sheet.EncodeToPNG());
            AssetDatabase.Refresh();
            this.sheetTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(assetPath);
            textureImporter.mipmapEnabled = false;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.alphaIsTransparency = true;
            textureImporter.SaveAndReimport();
#endif
        }

    }

}