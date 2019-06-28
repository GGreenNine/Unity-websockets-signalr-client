using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using Newtonsoft.Json;
using ModelsLibrary;
using UnityEngine.Serialization;

public class SinalRClientHelper : Singleton<SinalRClientHelper>
{
    public string signalRUrl = "84.201.161.171/MyNetworkApp/";


    private HubConnection _hubConnection = null;
    public static IHubProxy _gameHubProxy;
    public static IHubProxy _userHubProxy;
    string _result;

    private void Start()
    {
        ConnectToHub();
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
            Subscription userHubJoiningRoom = _userHubProxy.Subscribe("UserJoinRoomStatus");
            userHubJoiningRoom.Received += UserJoinedRoomData;
            /*
             * Get user leaved the room data
             */
            Subscription userLeavedRoom = _userHubProxy.Subscribe("UserLeavedRoom");
            userLeavedRoom.Received += UserLeavedRoomData;
            
            Debug.Log(" _hubConnection.Start();");
        }

        // Start hub connection
        _hubConnection.Start().Wait();
        if (_hubConnection.State == ConnectionState.Connected)
            Debug.Log("connected");
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
            UserManager.CurrentUser = user;
        UINotifications.Instance.ShowDefaultNotification($"User is join the room : {user.UserName}");
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
            UserManager.CurrentUser = user;
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
        Debug.Log("OnApplicationQuit() " + Time.time + " seconds");
        _hubConnection.Error -= hubConnection_Error;
        _hubConnection.Stop();
    }

    private void SceneCreateData(IList<JToken> obj)
    {
        SyncObjectModel modelToCreate = JsonConvert.DeserializeObject<SyncObjectModel>(obj.First().ToString());
        if (modelToCreate == null)
        {
            UINotifications.Instance.ShowDefaultNotification("Creating new object failed");
            return;
        }
        ClientFunctional.Instance.CreateModelOther(modelToCreate);
        UINotifications.Instance.ShowDefaultNotification($"Creating new object {modelToCreate.PrefabName}");

    }
    
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
        if(matchModel!=null)
            matchModel.DisableMe(out matchModel);
    }
    
    
    
    /// <summary>
         /// Sending data packages to the server
         /// </summary>
         /// <returns></returns>
//         private IEnumerator TransformPackageSender()
//         {
//             yield return new WaitForSeconds(0.02f);
//             foreach (var package in _queueToSend)
//             {
//                 Debug.Log(package.Length * sizeof(char) + "- Отправлен пакет");
//                 webSocket.Send(package);
//             }
//     
//             _queueToSend.Clear();
//             StartCoroutine(TransformPackageSender());
//         }
    
    
}