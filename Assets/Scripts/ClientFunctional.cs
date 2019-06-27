using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelsLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Random = System.Random;

public class ClientFunctional : Singleton<ClientFunctional>
{
//    public void CreateSimpleCube()
//    {
//        var randomPlace = new Vector3(UnityEngine.Random.Range(0f, 10f), UnityEngine.Random.Range(0f, 10f),
//            UnityEngine.Random.Range(0f, 10f));
//        var randomRotation = new Quaternion(UnityEngine.Random.Range(0f, 10f), UnityEngine.Random.Range(0f, 10f),
//            UnityEngine.Random.Range(0f, 10f), UnityEngine.Random.Range(0f, 10f));
//        CreateModel(randomPlace, randomRotation, "SimpleCube");
//    }

    public void GeneratePlayerModel()
    {
        CreateModel(Vector3.zero, Vector3.one, "PlayerPrefab");
    }

    public void GenerateOtherPlayerModel()
    {
    }

    public void CreateModel(Vector3 position, Vector3 rotation, string prefabName)
    {
        var myModelPrefab = Instantiate(Resources.Load(prefabName), position, new Quaternion(rotation.x,rotation.y,rotation.z,1));
        var myModel = new SyncObjectModel
        {
            PrefabName = prefabName,
            ModelId = Guid.NewGuid().ToString(),
            User = UserManager.CurrentUser,
        };
        
        VectorConverter g = new VectorConverter(false, true, false);
        
        var pos = JObject.Parse(JsonConvert.SerializeObject(position, g));
        var rot = JObject.Parse(JsonConvert.SerializeObject(rotation, g));
        
        myModel.ModelPosition = new JObject()
        {
            "position",
            pos
        };
        myModel.ModelRotation = new JObject()
        {
            "rotation",
            rot
        };
        SinalRClientHelper._gameHubProxy.Invoke("CreateModel", myModel);
    }

    public void CreateModelOther(SyncObjectModel inModel)
    {
        var position = JsonConvert.DeserializeObject<Vector3>(inModel.ModelPosition.First.ToString());
        var rotation = JsonConvert.DeserializeObject<Vector3>(inModel.ModelRotation.First.ToString());
        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            var otherModelPrefab = Instantiate(Resources.Load(inModel.PrefabName), position,
                new Quaternion(rotation.x, rotation.y, rotation.z, 1)) as GameObject;
            var netWorkingTransform = otherModelPrefab.GetComponent<NetWorkingTransform>();
            ObjectsStateManager.Instance.modelsLoadedFromServerDictionary.TryAdd(inModel.ModelId, netWorkingTransform);
        });
    }
}