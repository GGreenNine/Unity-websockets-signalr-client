using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

[RequireComponent(typeof(Rigidbody))]
public class PertainTransform : MovingTransform, IMovingNetworkObject
{
    private Rigidbody _rigit;
    
    private void FixedUpdate()
    {
        UpdateMoving();
    }

    private void Start()
    {
        _rigit = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        RigidBodyControl();
    }

    private void RigidBodyControl()
    {
        if (!IsMine())
            _rigit.isKinematic = true;
    }
    
    
    public void Initialize()
    {
        StartCoroutine(SendTransformData());
    }
}
