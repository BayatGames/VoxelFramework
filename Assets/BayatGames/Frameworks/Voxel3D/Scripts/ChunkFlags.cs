using System;

namespace BayatGames.Frameworks.Voxel3D
{

    [Flags]
    public enum ChunkFlags
    {

        Loaded,
        Busy,
        MeshReady,
        Generated,
        QueuedForUpdate,
        MarkedForDeletion

    }

}