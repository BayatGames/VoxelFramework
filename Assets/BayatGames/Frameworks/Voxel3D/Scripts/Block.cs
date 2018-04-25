using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BayatGames.Frameworks.Voxel3D.Definitions;

namespace BayatGames.Frameworks.Voxel3D
{

    public class Block
    {

        [NonSerialized]
        protected BlockDefinition definition;

        public virtual BlockDefinition Definition
        {
            get
            {
                return this.definition;
            }
            set
            {
                this.definition = value;
            }
        }

        public Block(BlockDefinition definition)
        {
            this.definition = definition;
        }

    }

}