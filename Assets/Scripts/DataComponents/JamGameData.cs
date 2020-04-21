using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

[GenerateAuthoringComponent]
public struct JamGameData : IComponentData
{
    public bool isAttracted;
    public int curPosIndex;
    public int attractorID;
}
