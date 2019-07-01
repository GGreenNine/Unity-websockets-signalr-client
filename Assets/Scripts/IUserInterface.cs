using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUserInterface
{
    void Enable();
    void Disable();
    void UpdateUser();
    void ConnectToRoom();
    void DisconnectFromRoom();
}
