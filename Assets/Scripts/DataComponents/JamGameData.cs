using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

[GenerateAuthoringComponent]
public struct JamGameData : IComponentData
{
    //public NativeArray<float3> targetPositions;
    public bool isAttracted;
    public int curPosIndex;
    public int attractorID;
    //public float distToTarget;
    //public float speed;
}
