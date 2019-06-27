using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelsLibrary;
using Newtonsoft.Json.Linq;
using Random = System.Random;

public class ClientFunctional : Singleton<ClientFunctional>
{
    public void CreateSimpleCube()
    {    
        var randomPlace = new Vector3(UnityEngine.Random.Range(0f,10f),UnityEngine.Random.Range(0f,10f),UnityEngine.Random.Range(0f,10f));
        var randomRotation = new Quaternion(UnityEngine.Random.Range(0f,10f),UnityEngine.Random.Range(0f,10f),UnityEngine.Random.Range(0f,10f),UnityEngine.Random.Range(0f,10f));
        CreateModel(randomPlace, randomRotation, "SimpleCube");
    }

    public void CreateModel(Vector3 position, Quaternion rotation, string prefabName)
    {
        var myModelPrefab = Instantiate(Resources.Load(prefabName), position, rotation);
        var myModel = new SyncObjectModel
        {
//            Authority = UserManager.CurrentUser.connectionId,
//            ModelAuthority = Guid.NewGuid().ToString(),
            ModelPosition = new JObject() {"Position", position},
            ModelRotation = new JObject() {"Position", rotation},
            PrefabName = prefabName
            
        };
        SinalRClientHelper._gameHubProxy.Invoke("CreateModel", myModel);
    }
}