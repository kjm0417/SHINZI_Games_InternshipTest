using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour, IKnockbackReceiver
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

    //넉백 관련
    [SerializeField] private float knockbackDecay = 20f;
    private Vector3 knockbackVelocity;


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

        Vector3 finalVelocity = horizontalVelocity + knockbackVelocity + Vector3.up * verticalVelocity;

        characterController.Move(finalVelocity * deltaTime);

        UpdateKnockback(deltaTime);
    }

    #region 대쉬
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

    #endregion

    private Vector3 ConvertToMoveDirection(Vector2 moveInput)
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);

        return Vector3.ClampMagnitude(direction, 1f);
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

    #region 넉백
    public void ApplyKnockback(Vector3 direction, float power)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f || power <= 0f)
        {
            return;
        }

        knockbackVelocity = direction.normalized * power;
    }

    public void UpdateKnockback(float deltaTime)
    {
        knockbackVelocity = Vector3.MoveTowards(knockbackVelocity, Vector3.zero, knockbackDecay * deltaTime);
    }

    #endregion
}



