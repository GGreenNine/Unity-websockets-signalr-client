using System.Collections;
using System.Collections.Generic;
using ModelsLibrary;
using UnityEngine;

public abstract class MovingNetworkTransform : NetWorkingTransform
{
    protected MovingNetworkTransform(string creatorAuthority, string modelAuthority) : base(creatorAuthority,
        modelAuthority)
    {
    }
}

public abstract class NetWorkingTransform : MonoBehaviour
{
    public string CreatorAuthority;
    public string ModelAuthority;

    protected NetWorkingTransform(string creatorAuthority, string modelAuthority)
    {
        CreatorAuthority = creatorAuthority;
        ModelAuthority = modelAuthority;
    }

    public Vector3 newPosition;
    public Vector3 newRotation;

    public void UpdateMoving()
    {
        if (CreatorAuthority == UserManager.CurrentUser.connectionId || CreatorAuthority == null) return;
        if (newPosition != null && newPosition != transform.position ||
            newRotation != null && newRotation != transform.rotation.eulerAngles)
        {
//            transform.position = Vector3.MoveTowards(transform.position, OldState.pos, OldState.distance);
//            var newRotation = new Quaternion(OldState.rot.x, OldState.rot.y, OldState.rot.z, OldState.rot.w);
//            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 0.1f);
        }
    }


    public abstract void Initialize();

    public abstract SyncObjectModel GetModelInfo();

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
