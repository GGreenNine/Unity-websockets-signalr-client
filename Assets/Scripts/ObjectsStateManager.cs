using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ModelsLibrary;
using UnityEngine;

public class ObjectsStateManager : Singleton<ObjectsStateManager>
{
    public ConcurrentDictionary<string, NetWorkingTransform> modelsLoadedFromServerDictionary =
        new ConcurrentDictionary<string, NetWorkingTransform>();

    public ConcurrentQueue<NetWorkingTransform> myModelsDictionary = new ConcurrentQueue<NetWorkingTransform>();

    public ConcurrentDictionary<string, NetWorkingTransform> myModelsDictionartLocal =
        new ConcurrentDictionary<string, NetWorkingTransform>();

    /// <summary>
    /// Проходит по списку созданных нами обьектов,
    /// Дает каждому из них команду отправить о себе информацию на сервер
    /// </summary>
    public void NotifyNetworingTransforms()
    {
        foreach (var k in myModelsDictionary)
        {
            k.NotifyOtherClientsSyncObjectData();
        }
    }
    
    public void UpdateMoving(SyncObjectModel model)
    {
        NetWorkingTransform value;
        modelsLoadedFromServerDictionary.TryGetValue(model.ModelId, out value);
        //value?.UpdateMoving();
        
    }
}