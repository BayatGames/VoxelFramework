using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using BayatGames.Frameworks.Voxel3D.Definitions;

namespace BayatGames.Frameworks.Voxel3D
{

    [InitializeOnLoad]
    public static class BlockDefinitionManager
    {

        static BlockDefinitionManager()
        {
        }

        public static List<BlockDefinition> GetBlocks()
        {
            List<BlockDefinition> blocks = new List<BlockDefinition>();
            string[] guids = AssetDatabase.FindAssets("t:BlockDefinition");
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                BlockDefinition block = AssetDatabase.LoadAssetAtPath<BlockDefinition>(assetPath);
                if (!blocks.Contains(block))
                {
                    blocks.Add(block);
                }
            }
            return blocks;
        }

        public static BlockDefinition IsDuplicate(string identifier)
        {
            List<BlockDefinition> blocks = GetBlocks();
            BlockDefinition targetBlock = null;
            bool exists = blocks.Exists((block) =>
            {
                if (block.Identifier == identifier)
                {
                    targetBlock = block;
                    return true;
                }
                return false;
            });
            return targetBlock;
        }

    }

}