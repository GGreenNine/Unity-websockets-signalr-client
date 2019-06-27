using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ClientUIHolder : Singleton<ClientUIHolder>
{
    [Header("________________________")]
    [Header("Client UI elements only")]
    
    [FormerlySerializedAs("Authorization Menu")]  public RectTransform authorizationMenu;
    [FormerlySerializedAs("Client Action control Menu")]  public RectTransform clientControlMenu;
}
