using System.Collections;
using System.Collections.Generic;
using ModelsLibrary;
using UnityEngine;

public class DefaultNetWorkObject : MovingNetworkTransform
{
    public DefaultNetWorkObject(string creatorAuthority, string modelAuthority) : base(creatorAuthority, modelAuthority)
    {
    }

    public override void Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override SyncObjectModel GetModelInfo()
    {
        throw new System.NotImplementedException();
    }
}
