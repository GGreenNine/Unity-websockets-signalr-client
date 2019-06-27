using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UINotifications : Singleton<UINotifications>
{
    [SerializeField] private RectTransform DefaultNotificationPanel;

    public void ShowDefaultNotification(string message)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            var UItext = DefaultNotificationPanel.GetComponentInChildren<Text>();
            Sequence mySequence = DOTween.Sequence();
            mySequence.Append(UItext.DOText(message, 0.5f)).AppendInterval(0.3f).Append(UItext.DOText("", 0.2f));
        });
        Debug.Log(message);
    }
}
