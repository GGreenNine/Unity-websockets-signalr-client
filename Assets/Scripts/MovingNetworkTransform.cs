using System.Collections;
using System.Collections.Generic;
using ModelsLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

public abstract class NetWorkingTransform : MonoBehaviour
{
    private SyncObjectModel syncObjectModel;
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
    
    public string CreatorAuthority;
    public string ModelAuthority;
    public string PrefabName;
    
    public Vector3 newPosition;
    public Vector3 newRotation;

    public void UpdateMoving()
    {
        if (CreatorAuthority == UserManager.CurrentUser.UserName || CreatorAuthority == null) return;
        if (newPosition != transform.position ||
            newRotation != transform.rotation.eulerAngles)
        {
            transform.position = Vector3.MoveTowards(transform.position, newPosition, SyncObjectModel.Distance);
            var newRotation = new Quaternion(this.newRotation.x, this.newRotation.y, this.newRotation.z, 1);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 0.1f);
        }
    }

    public abstract void NotifyOtherClientsSyncObjectData();    

    public abstract void Initialize();

    public virtual SyncObjectModel GetModelInfo()
    {
        VectorConverter g = new VectorConverter(false, true, false);

        return new SyncObjectModel()
        {
            Distance = 0,
            ModelId = ModelAuthority,
            ModelPosition = JsonConvert.SerializeObject(transform.position, g),
            ModelRotation = JsonConvert.SerializeObject(transform.rotation, g),
            UserName = CreatorAuthority,
            PrefabName = PrefabName,
            RoomModelId = SyncObjectModel.RoomModelId,
            RoomModel = SyncObjectModel.RoomModel,
            Rotation = 0,
            UserModel = SyncObjectModel.UserModel
        };
    }

    public void DisableMe(out NetWorkingTransform disabledOut)
    {
        ObjectsStateManager.Instance.modelsLoadedFromServerDictionary.TryRemove(ModelAuthority, out disabledOut);
        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            this.gameObject.SetActive(false);
            this.enabled = false;
        });
    }
}
