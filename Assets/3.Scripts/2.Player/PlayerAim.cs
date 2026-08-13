using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private Transform rotationTarget;


    private void Awake()
    {
        if(gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }

        if(rotationTarget == null)
        {
            rotationTarget = this.transform;
        }
    }

    public void Tick(Vector2 screenPosition)
    {
        //카메라에서 마우스 위치로 ray발사
        Ray ray = gameplayCamera.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(Vector3.up, rotationTarget.position);

        if (!plane.Raycast(ray, out float distance)) return;

        Vector3 aimPoint = ray.GetPoint(distance);
        Vector3 aimDirection = aimPoint - rotationTarget.position;

        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude < 0.001f) return;

        rotationTarget.rotation = Quaternion.LookRotation(aimDirection);
    }
}

