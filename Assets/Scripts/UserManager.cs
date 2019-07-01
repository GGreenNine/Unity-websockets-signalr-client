using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModelsLibrary;

public class UserManager : Singleton<UserManager>
{
    public InputField UserName;
    public InputField Password;

    private static UserModel _currentUser;

    public static UserModel CurrentUser
    {
        get => _currentUser;
        set => _currentUser = value;
    }

    private void Start()
    {
        UIManager.Instance.OnAuthorizationContext(true);
    }

    public void Registrate()
    {
        UserModel user = new UserModel()
        {
            UserName = UserName.text,
            connectionId = "null",
            Password = Password.text
        };
        SinalRClientHelper._userHubProxy.Invoke("RegisterUser", user);
    }

    public void Authorization()
    {
        UserModel user = new UserModel()
        {
            UserName = UserName.text,
            connectionId = "null",
            Password = Password.text
        };
        SinalRClientHelper._userHubProxy.Invoke("UserAuthorization", user);
    }
}