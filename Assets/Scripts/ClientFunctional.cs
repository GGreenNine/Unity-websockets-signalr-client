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
    public void CreateSimpleCube()
    {
        var randomPlace = new Vector3(UnityEngine.Random.Range(0f, 10f), UnityEngine.Random.Range(0f, 10f),
            UnityEngine.Random.Range(0f, 10f));
        var randomRotation = new Vector3(UnityEngine.Random.Range(0f, 10f), UnityEngine.Random.Range(0f, 10f),
            UnityEngine.Random.Range(0f, 10f));
        CreateModel(randomPlace, randomRotation, "PlayerPrefab");
    }

    public void GeneratePlayerModel()
    {
        CreateModel(Vector3.zero, Vector3.one, "PlayerPrefab");
    }

    public void CreateModel(Vector3 position, Vector3 rotation, string prefabName)
    {
        var myModelPrefab = Instantiate(Resources.Load(prefabName), position, new Quaternion(rotation.x,rotation.y,rotation.z,1));
        var myModel = new SyncObjectModel
        {
            PrefabName = prefabName,
            ModelId = Guid.NewGuid().ToString(),
            //PlayerId = UserManager.CurrentUser.PlayerId,
            //RoomModelId = UserManager.CurrentUser.RoomModelId
        };
        
        VectorConverter g = new VectorConverter(false, true, false);
        
        var pos = JObject.Parse(JsonConvert.SerializeObject(position, g));
        var rot = JObject.Parse(JsonConvert.SerializeObject(rotation, g));

        myModel.ModelPosition = new JObject();
        myModel.ModelRotation = new JObject();

        myModel.ModelPosition.Add(new JProperty("Position", pos));
        myModel.ModelRotation.Add(new JProperty("Rotation", rot));
        
        SinalRClientHelper._gameHubProxy.Invoke("CreateModel",myModel, UserManager.CurrentUser);
    }

    public void CreateModelOther(SyncObjectModel inModel)
    {
        VectorConverter g = new VectorConverter(true, true, true);

        var position = JsonConvert.DeserializeObject<Vector3>(inModel.ModelPosition.First.First.ToString(), g);
        g = new VectorConverter(true, true, true);
        var rotation = JsonConvert.DeserializeObject<Vector3>(inModel.ModelRotation.First.First.ToString(), g);
        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            var otherModelPrefab = Instantiate(Resources.Load(inModel.PrefabName), position,
                new Quaternion(rotation.x, rotation.y, rotation.z, 1)) as GameObject;
            var netWorkingTransform = otherModelPrefab.GetComponent<NetWorkingTransform>();
            netWorkingTransform.syncObjectModel = inModel;
            ObjectsStateManager.Instance.modelsLoadedFromServerDictionary.TryAdd(inModel.ModelId, netWorkingTransform);
        });
    }
}