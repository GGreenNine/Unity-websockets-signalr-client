using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelsLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Random = System.Random;

public class ClientFunctional : Singleton<ClientFunctional>, IUserInterface
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
        var myModelPrefab = Instantiate(Resources.Load(prefabName), position,
            new Quaternion(rotation.x, rotation.y, rotation.z, 1));
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

        SinalRClientHelper._gameHubProxy.Invoke("CreateModel", myModel, UserManager.CurrentUser);
    }

    /// <summary>
    /// Создание чужой модели
    /// </summary>
    /// <param name="inModel"></param>
    public void CreateModelOther(SyncObjectModel inModel)
    {
        /*
         * Проверяем, есть ли данная модель на сцене
         * Если есть, не создаем повторную и выходим из тела метода
         */
        if (ObjectsStateManager.Instance.modelsLoadedFromServerDictionary.ContainsKey(inModel.ModelId)) return;

        VectorConverter g = new VectorConverter(true, true, true);

        var position = JsonConvert.DeserializeObject<Vector3>(inModel.ModelPosition.First.First.ToString(), g);
        g = new VectorConverter(true, true, true);
        var rotation = JsonConvert.DeserializeObject<Vector3>(inModel.ModelRotation.First.First.ToString(), g);

        /*
         * Модель может создаваться только в основном потоке,
         * Поэтому диспатчим ее и засовываем в основной поток.
         */
        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            var otherModelPrefab = Instantiate(Resources.Load(inModel.PrefabName), position,
                new Quaternion(rotation.x, rotation.y, rotation.z, 1)) as GameObject;
            var netWorkingTransform = otherModelPrefab.GetComponent<NetWorkingTransform>();
            netWorkingTransform.syncObjectModel = inModel;
            ObjectsStateManager.Instance.modelsLoadedFromServerDictionary.TryAdd(inModel.ModelId, netWorkingTransform);
        });
    }

    private void Start()
    {
        Enable();
    }

    public void UpdateScene()
    {
        SinalRClientHelper._gameHubProxy.Invoke("UpdateScene", UserManager.CurrentUser);
    }

    public void Enable()
    {
        SinalRClientHelper.Instance.IsConnectedToRoom += UpdateScene;
    }

    public void Disable()
    {
        throw new NotImplementedException();
    }

    public void UpdateUser()
    {
        throw new NotImplementedException();
    }

    public void ConnectToRoom()
    {
        SinalRClientHelper._gameHubProxy.Invoke("JoinRoom", UserManager.CurrentUser);
    }

    public void DisconnectFromRoom()
    {
        if (UserManager.CurrentUser.RoomModelId == null) return;
        SinalRClientHelper._gameHubProxy.Invoke("LeaveRoom", UserManager.CurrentUser);
    }
}