using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    //참조 관련
    [SerializeField] private PlayerData playerData;
    private CharacterController characterController;

    //이동 관련
    private float verticalVelocity;

    //대쉬 관련
    private float dashRemainingTime; //대시 잔여 시간
    private float dashCooldownRemaining; //대시 재사용 대기시간 잔여량
    private Vector3 dashDirection;

    public bool IsDashing => dashRemainingTime > 0f;
    public bool IsDashReady => dashCooldownRemaining <= 0f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }


    public void Tick(Vector2 moveInput, float deltaTime)
    {
        //지역 변수로 한 이유 : Tick 끝나고 값으 버려도 상관없음
        Vector3 moveDirection = ConvertToMoveDirection(moveInput);

        UpdateDashTimers(deltaTime);

        Vector3 horizontalVelocity;
        if (IsDashing)
        {
            horizontalVelocity =  dashDirection * playerData.DashSpeed;
        }
        else
        {
            horizontalVelocity = moveDirection * playerData.Speed;
        }


        UpdateGravity(deltaTime);

        Vector3 finalVelocity = horizontalVelocity +Vector3.up * verticalVelocity;

        characterController.Move(finalVelocity * deltaTime);
    }

    public bool TryDash(Vector2 moveInput)
    {
        if (IsDashing || !IsDashReady) return false;

        Vector3 direction = ConvertToMoveDirection(moveInput);

        if (direction.sqrMagnitude < 0.001f) return false;

        dashDirection = direction;
        dashRemainingTime = playerData.DashDuration;
        dashCooldownRemaining = playerData.DashCooldown;

        return true;
    }

    private Vector3 ConvertToMoveDirection(Vector2 moveInput)
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);

        return Vector3.ClampMagnitude(direction, 1f);
    }

    private void UpdateDashTimers(float deltaTime)
    {
        if (dashRemainingTime > 0f)
        {
            dashRemainingTime = Mathf.Max(0f, dashRemainingTime - deltaTime);

            if (dashRemainingTime <= 0f)
            {
                dashDirection = Vector3.zero;
            }    
        }

        if (dashCooldownRemaining > 0f)
        {
            dashCooldownRemaining = Mathf.Max(0f, dashCooldownRemaining - deltaTime);
            Debug.Log($"{dashCooldownRemaining}초 남았음");
        }

    }

    private void UpdateGravity(float deltaTime)
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * deltaTime;
        }
    }
}


