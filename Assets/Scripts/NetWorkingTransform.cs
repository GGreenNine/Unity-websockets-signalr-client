using System.Collections;
using System.Collections.Generic;
using ModelsLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

interface INetworkObject
{
    void NotifyOtherClientsSyncObjectData();
    bool IsMine();
    string CreatorAuthority { get; set; }
    string ModelAuthority { get; set; }
    string PrefabName { get; set; }
    Vector3 newPosition { get; set; }
    Vector3 newRotation { get; set; }
}

interface IMovingNetworkObject
{
    void Initialize();
}

public class NetWorkingTransform : MonoBehaviour, INetworkObject
{
    private SyncObjectModel syncObjectModel;

    public NetWorkingTransform(SyncObjectModel syncObjectModel)
    {
        VectorConverter g = new VectorConverter(true, true, true);
        this.syncObjectModel = syncObjectModel;
        CreatorAuthority = syncObjectModel.UserName;
        ModelAuthority = syncObjectModel.ModelId;
        PrefabName = syncObjectModel.PrefabName;
        this.newPosition = JsonConvert.DeserializeObject<Vector3>(syncObjectModel.ModelPosition, g);
        this.newRotation = JsonConvert.DeserializeObject<Vector3>(syncObjectModel.ModelRotation, g);
    }

    protected NetWorkingTransform()
    {
    }

    public SyncObjectModel SyncObjectModel
    {
        get => syncObjectModel;
        set
        {
            VectorConverter g = new VectorConverter(true, true, true);
            syncObjectModel = value;
            CreatorAuthority = value.UserName;
            ModelAuthority = value.ModelId;
            newPosition = JsonConvert.DeserializeObject<Vector3>(value.ModelPosition, g);
            newRotation = JsonConvert.DeserializeObject<Vector3>(value.ModelRotation, g);
        }
    }

    public void NotifyOtherClientsSyncObjectData()
    {
        SinalRClientHelper._queueToSend.TryAdd("CreateModel", new List<dynamic>() {GetModelInfo()});
    }

    public bool IsMine()
    {
        return CreatorAuthority == UserManager.CurrentUser.UserName;
    }

    public string CreatorAuthority { get; set; }
    public string ModelAuthority { get; set; }
    public string PrefabName { get; set; }

    public Vector3 newPosition { get; set; }
    public Vector3 newRotation { get; set; }

    /// <summary>
    /// Get Current Model Info
    /// </summary>
    /// <returns></returns>
    public virtual SyncObjectModel GetModelInfo()
    {
        VectorConverter g = new VectorConverter(false, true, false);

        if (UserManager.CurrentUser.RoomModelId != null)
            return new SyncObjectModel()
            {
                Distance = 0,
                ModelPosition = JsonConvert.SerializeObject(transform.position, g),
                ModelRotation = JsonConvert.SerializeObject(transform.eulerAngles, g),
                UserName = UserManager.CurrentUser.UserName,
                PrefabName = PrefabName,
                RoomModelId = (int) UserManager.CurrentUser.RoomModelId,
                Rotation = 0,
                ModelId = ModelAuthority,
                UserModel = UserManager.CurrentUser
            };
        return null;
    }

    public void DisableMe(out NetWorkingTransform disabledOut)
    {
        // O(1)
        ObjectsStateManager.Instance.modelsLoadedFromServerDictionary.TryRemove(ModelAuthority, out disabledOut);
        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            this.gameObject.SetActive(false);
            this.enabled = false;
        });
    }
}