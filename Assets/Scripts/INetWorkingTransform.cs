using System.Collections;
using System.Collections.Generic;
using ModelsLibrary;
using UnityEngine;

abstract class MovingNetworkTransform : NetWorkingTransform
{
    
}

public abstract class NetWorkingTransform : MonoBehaviour
{
    public string CreatorAuthority;
    public string ModelAuthority;

    public abstract void Initialize();

    public abstract SyncObjectModel GetModelInfo();
    
}
