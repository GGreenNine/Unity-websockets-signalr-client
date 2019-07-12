using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using UnityEngine;

public abstract class MovingTransform : NetWorkingTransform
{
    protected void UpdateMoving()
    {
        if (CreatorAuthority == UserManager.CurrentUser.UserName || CreatorAuthority == null) return;
        {
            transform.position = Vector3.MoveTowards(transform.position, newPosition, SyncObjectModel.Distance);
            var newRotation = new Quaternion(this.newRotation.x, this.newRotation.y, this.newRotation.z, 1);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 0.1f);
        }
    }

    protected IEnumerator SendTransformData()
    {
        newPosition = transform.position;
        newRotation = transform.eulerAngles;
        yield return new WaitForSeconds(0.2f);
        if (newPosition == transform.position &&
            newRotation == transform.eulerAngles)
        {
            StartCoroutine(SendTransformData());
            yield break;
        }

        var modelInfo = GetModelInfo();
        Debug.Log($"Distance {modelInfo.Distance} Position {modelInfo.ModelPosition} Rotation {modelInfo.Rotation}");
        modelInfo.Distance = (newPosition - transform.position).magnitude;
        SinalRClientHelper._queueToSend.TryAdd("UpdateMoving", new List<object>(){modelInfo});
        StartCoroutine(SendTransformData());
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
