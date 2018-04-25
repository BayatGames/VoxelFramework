using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BayatGames.Frameworks.Voxel3D.Utilities
{

    public static class CoroutineUtils
    {

        public static readonly WaitForEndOfFrame EndOfFrame = new WaitForEndOfFrame();
        public static readonly WaitForFixedUpdate FixedUpdate = new WaitForFixedUpdate();

    }


}