using System;
using System.Collections;
using System.Collections.Generic;
using ModelsLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

public class DefaultNetWorkObject : NetWorkingTransform
{
    public override void NotifyOtherClientsSyncObjectData()
    {
        SinalRClientHelper._gameHubProxy.Invoke("CreateModel", GetModelInfo());
        SinalRClientHelper._queueToSend.TryAdd("CreateModel", new List<dynamic>() {GetModelInfo()});
    }

    public override void Initialize()
    {
    }
}