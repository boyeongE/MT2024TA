using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class CameraCollisionHandler : MonoBehaviour
{
    [SerializeField]
    private Transform _target;

    [SerializeField]
    private float _distanceFromTarget = 3.0f;

    [SerializeField]
    private float _minDistanceFromTarget = 1.0f; // 최소 거리 설정

    [SerializeField]
    private LayerMask _collisionLayers;

    public Vector3 AdjustCameraPosition(Vector3 desiredPosition)
    {
        RaycastHit hit;

        // Use Raycast to check if there's an obstacle between the target and the camera
        if (Physics.Raycast(_target.position, -transform.forward, out hit, _distanceFromTarget, _collisionLayers))
        {
            // Position the camera in front of the obstacle, maintaining a minimum distance from the target
            float distance = Vector3.Distance(_target.position, hit.point);
            if (distance < _minDistanceFromTarget)
            {
                desiredPosition = _target.position - transform.forward * _minDistanceFromTarget;
            }
            else
            {
                desiredPosition = hit.point;
            }
        }

        return desiredPosition;
    }
}