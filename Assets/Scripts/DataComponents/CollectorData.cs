﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

[GenerateAuthoringComponent]
public struct CollectorData : IComponentData
{
    public bool canCollect;
    public int attractorID;
}
