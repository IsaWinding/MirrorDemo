using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSmoothFollow : MonoBehaviour
{
    private static CameraSmoothFollow instance;
    public static CameraSmoothFollow Instance { get { return instance; } }

    public float smoothDampTime = 0.2f;
    public Vector3 cameraOffset;
    public bool useFixedUpdate = false;

    private Transform target;
    private Vector3 _smoothDampVelocity;
    private Camera camera_;
    private Vector3 oriPos;
    private void Awake()
    {
        instance = this;
        camera_ = Camera.main;
        oriPos = camera_.transform.position;
    }
    public void SetTarget(Transform pTaraget)
    {
        target = pTaraget;
    }

    void LateUpdate()
    {
        if (!useFixedUpdate)
            updateCameraPosition();
    }


    void FixedUpdate()
    {
        if (useFixedUpdate)
            updateCameraPosition();
    }
    private Vector3 newPos;
    void updateCameraPosition()
    {
        if (target == null || camera_ == null)
            return;
        newPos = Vector3.SmoothDamp(camera_.transform.position, target.position - cameraOffset, ref _smoothDampVelocity, smoothDampTime);
        newPos.z = oriPos.z;
        camera_.transform.position = newPos;
    }
}
