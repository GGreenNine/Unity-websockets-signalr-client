using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIManager : Singleton<UIManager>
{
    private void Start()
    {
    }
    /// <summary>
    /// Hide/Show Authorization menu
    /// </summary>
    /// <param name="state"> true - show | false - hide </param>
    public void OnAuthorizationContext(bool state)
    {
        if (state)
            ClientUIHolder.Instance.authorizationMenu.DOMoveX(80, 1f);
        else
            ClientUIHolder.Instance.authorizationMenu.DOMoveX(-80, 1f);
    }
    /// <summary>
    /// Hide/Show Authorization menu
    /// </summary>
    /// <param name="state"> true - show | false - hide </param>
    public void OnClientControlContext(bool state)
    {
        if (state)
            ClientUIHolder.Instance.clientControlMenu.DOMoveX(80, 2f);
        else
            ClientUIHolder.Instance.clientControlMenu.DOMoveX(-80, 2f);
    }
    
}
