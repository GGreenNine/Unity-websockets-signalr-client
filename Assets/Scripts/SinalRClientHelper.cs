using System;
using UnityEngine;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using Newtonsoft.Json;
using ModelsLibrary;
using Newtonsoft.Json.Converters;
using UnityEngine.Serialization;

public class SinalRClientHelper : Singleton<SinalRClientHelper>
{
    public string signalRUrl = "84.201.161.171/MyNetworkApp/";

    public delegate void ConnectedToRoom();

    public event ConnectedToRoom IsConnectedToRoom = delegate { };
    public event ConnectedToRoom IsDisconnectedFromRoom = delegate { };

    private HubConnection _hubConnection = null;
    public static IHubProxy _gameHubProxy;
    public static IHubProxy _userHubProxy;

    public static ConcurrentDictionary<string, List<object>> _queueToSend =
        new ConcurrentDictionary<string, List<object>>();

    string _result;

    public void ConnectToRoom()
    {
        UINotifications.Instance.ShowDefaultNotification($"ConnectedToRoom");
        IsConnectedToRoom?.Invoke();
        UnityMainThreadDispatcher.Instance().Enqueue(delegate { StartCoroutine(TransformPackageSender()); });
    }

    private void Start()
    {
        ConnectToHub();
        //IsConnectedToRoom += delegate { StartCoroutine(TransformPackageSender()); };
    }

    private void ConnectToHub()
    {
        if (_hubConnection == null)
        {
            /*
             * Creating hub connection
             */
            //_hubConnection = new HubConnection("http://84.201.161.171/signalr/");
            _hubConnection = new HubConnection("http://localhost:52527");
            _hubConnection.Error += hubConnection_Error;
            _hubConnection.StateChanged += HubConnectionOnStateChanged;
            _gameHubProxy = _hubConnection.CreateHubProxy("GameHub");
            _userHubProxy = _hubConnection.CreateHubProxy("UserHub");

            Subscription sceneUpdateModelsData = _gameHubProxy.Subscribe("GameBroadcaster_UpdateMoving");
            sceneUpdateModelsData.Received += SceneUpdateModelsDataOnReceived;
            /*
             * Connection to game hub updateModels
             */
            Subscription sceneCreateData = _gameHubProxy.Subscribe("GameBroadcaster_CreateModel");
            sceneCreateData.Received += SceneCreateData;

            Subscription sceneDeleteData = _gameHubProxy.Subscribe("GameBroadcaster_GameBroadcaster_DeleteModel");
            sceneDeleteData.Received += SceneDeleteData;
            /*
             *  Connection to registrate status
             */
            Subscription userHubRegistrate = _userHubProxy.Subscribe("RegistrateStatus");
            userHubRegistrate.Received += UserRegistrationData;
            /*
             * Connection to Authorization status
             */
            Subscription userHubAuthorization = _userHubProxy.Subscribe("AuthorizationStatus");
            userHubAuthorization.Received += UserAuthorizationData;
            /*
             * Get user joined the room data
             */
            Subscription userHubJoiningRoom = _gameHubProxy.Subscribe("UserJoinRoomStatus");
            userHubJoiningRoom.Received += UserJoinedRoomData;
            /*
             * Get user leaved the room data
             */
            Subscription userLeavedRoom = _gameHubProxy.Subscribe("UserLeavedRoom");
            userLeavedRoom.Received += UserLeavedRoomData;
            /*
             * 
             */
            Subscription updateScene = _gameHubProxy.Subscribe("UpdateSceneFor");
            updateScene.Received += UpdateSceneData;

            Debug.Log(" _hubConnection.Start();");
        }

        // Start hub connection
        _hubConnection.Start().Wait();
        if (_hubConnection.State == ConnectionState.Connected)
            Debug.Log("connected");
    }

    private void SceneUpdateModelsDataOnReceived(IList<JToken> obj)
    {
        var g = new VectorConverter(false, true, false);

        SyncObjectModel modelToCreate = JsonConvert.DeserializeObject<SyncObjectModel>(obj.First().ToString());
        if (modelToCreate == null)
            return;
        try
        {
            ObjectsStateManager.Instance.modelsLoadedFromServerDictionary.TryGetValue(modelToCreate.ModelId,
                out var model);
            model.newPosition = JsonConvert.DeserializeObject<Vector3>(modelToCreate.ModelPosition, g);
            model.newRotation = JsonConvert.DeserializeObject<Vector3>(modelToCreate.ModelRotation, g);
            model.SyncObjectModel.Distance = modelToCreate.Distance;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void UpdateSceneData(IList<JToken> obj)
    {
        var sceneObjects = JArray.Parse(obj.First().ToString());
        VectorConverter g = new VectorConverter(false, true, false);

        foreach (var model in sceneObjects)
        {
            var modelObject = JsonConvert.DeserializeObject<SyncObjectModel>(model.ToString(), g);
            ClientFunctional.Instance.CreateModelOther(modelObject);
        }
    }

    private void HubConnectionOnStateChanged(StateChange obj)
    {
        switch (obj.NewState)
        {
            case ConnectionState.Connected:
                Debug.Log(_hubConnection.Url);
                Debug.Log("Connected");
                break;
            case ConnectionState.Disconnected:
                Debug.Log("Connection was lost");
                break;
            case ConnectionState.Connecting:
                break;
            case ConnectionState.Reconnecting:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UserUpdateSceneState(IList<JToken> obj)
    {
        foreach (var model in obj)
        {
            Debug.Log(model);
        }
    }

    /// <summary>
    /// Append registration data from server
    /// </summary>
    /// <param name="obj"></param>
    private void UserRegistrationData(IList<JToken> obj)
    {
        var user = JsonConvert.DeserializeObject<UserModel>(obj.First().ToString());
        if (user == null)
        {
            UINotifications.Instance.ShowDefaultNotification("Registration failed...");
        }
        else
        {
            UINotifications.Instance.ShowDefaultNotification("You are registrated!");
            UserManager.CurrentUser = user;
        }
    }

    /// <summary>
    /// User join room data
    /// </summary>
    /// <param name="obj"></param>
    private void UserJoinedRoomData(IList<JToken> obj)
    {
        var user = JsonConvert.DeserializeObject<UserModel>(obj.First().ToString());
        if (user == null)
            return;
        if (UserManager.CurrentUser.UserName == user.UserName)
        {
            UserManager.CurrentUser = user;
            ConnectToRoom();
            UINotifications.Instance.ShowDefaultNotification($"You are joined the room");
        }
        else
        {
            UINotifications.Instance.ShowDefaultNotification($"User is join the room : {user.UserName}");
            /*
             * При подключении другого пользователя отправляем информацию о состоянии
             * Обьектов, которыми владеем
             */
            ObjectsStateManager.Instance.NotifyNetworingTransforms();
        }
    }

    /// <summary>
    /// User leaved room data
    /// </summary>
    /// <param name="obj"></param>
    private void UserLeavedRoomData(IList<JToken> obj)
    {
        var user = JsonConvert.DeserializeObject<UserModel>(obj.First().ToString());
        if (user == null)
            return;
        if (UserManager.CurrentUser.UserName == user.UserName)
        {
            UserManager.CurrentUser = user;
            IsDisconnectedFromRoom();
        }

        UINotifications.Instance.ShowDefaultNotification($"User has leaved the room : {user.UserName}");
    }

    /// <summary>
    /// Append authorization data from server
    /// </summary>
    /// <param name="obj"></param>
    private void UserAuthorizationData(IList<JToken> obj)
    {
        var user = JsonConvert.DeserializeObject<UserModel>(obj.First().ToString());
        if (user == null)
        {
            UINotifications.Instance.ShowDefaultNotification("Authorization failed");
            return;
        }

        UserManager.CurrentUser = user;
        UINotifications.Instance.ShowDefaultNotification("You are authorized!");
    }

    /// <summary>
    /// Error handler
    /// </summary>
    /// <param name="obj"></param>
    private void hubConnection_Error(System.Exception obj)
    {
        Debug.Log("Hub Error - " + obj.Message + System.Environment.NewLine + obj.InnerException +
                  System.Environment.NewLine + obj.Data +
                  System.Environment.NewLine + obj.StackTrace +
                  System.Environment.NewLine + obj.TargetSite);
    }

    /// <summary>
    /// Drop the connection
    /// </summary>
    void OnApplicationQuit()
    {
        ClientFunctional.Instance.Disable();
        _hubConnection.Error -= hubConnection_Error;
        _hubConnection.Dispose();
    }

    /// <summary>
    /// Create model message processing
    /// </summary>
    /// <param name="obj"></param>
    private void SceneCreateData(IList<JToken> obj)
    {
        SyncObjectModel modelToCreate = JsonConvert.DeserializeObject<SyncObjectModel>(obj.First().ToString());

        ClientFunctional.Instance.CreateModelOther(modelToCreate);
        UINotifications.Instance.ShowDefaultNotification($"Creating new object {modelToCreate.PrefabName}");
    }

    /// <summary>
    /// Delete model message processing
    /// </summary>
    /// <param name="obj"></param>
    private void SceneDeleteData(IList<JToken> obj)
    {
        SyncObjectModel modelToCreate = JsonConvert.DeserializeObject<SyncObjectModel>(obj.First().ToString());
        if (modelToCreate == null)
        {
            UINotifications.Instance.ShowDefaultNotification("Deleting object failed");
            return;
        }

        NetWorkingTransform matchModel;
        ObjectsStateManager.Instance.modelsLoadedFromServerDictionary.TryGetValue(modelToCreate.ModelId,
            out matchModel);
        if (matchModel != null)
            matchModel.DisableMe(out matchModel);
    }

    /// <summary>
    /// Sending data packages to the server
    /// </summary>
    /// <returns></returns>
    private IEnumerator TransformPackageSender()
    {
        yield return new WaitForSeconds(0.02f);
        Debug.Log("TransformPachage sdanpodnjapo");
        foreach (var package in _queueToSend)
        {
            foreach (var value in package.Value)
            {
                _gameHubProxy.Invoke(package.Key, value);
            }
        }

        _queueToSend.Clear();
        StartCoroutine(TransformPackageSender());
    }
}