using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ModelsLibrary;
using UnityEngine;

public class ObjectsStateManager : Singleton<ObjectsStateManager>
{
    public ConcurrentDictionary<string,NetWorkingTransform> modelsLoadedFromServerDictionary = new ConcurrentDictionary<string, NetWorkingTransform>();
    
}
