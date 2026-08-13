using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputReader : MonoBehaviour
{
    //Move
    public Vector2 MoveInput { get;private set; }

    //에임 관련
    public Vector2 AimScreenPosition { get; private set; }
    public bool HasAimPosition { get; private set; } 

    //대쉬 관련
    public bool DashPressed { get; private set; }


    //인풋액션 저장
    private PlayerInput playerInput;
    private InputAction movementAction;
    private InputAction aimAction;
    private InputAction dashAction;


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        movementAction = playerInput.actions.FindAction("Move", true);
        aimAction = playerInput.actions.FindAction("Aim", true);
        dashAction = playerInput.actions.FindAction("Dash", true);
    }

    private void OnEnable()
    {
        playerInput.onActionTriggered += OnActionTriggered;
    }

    private void OnDisable()
    {
        playerInput.onActionTriggered -= OnActionTriggered;
    }

  

    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        if(context.action == movementAction)
        {
            if(context.performed)
            {
                MoveInput = context.ReadValue<Vector2>();
            }
            else if(context.canceled)
            {
                MoveInput = Vector2.zero;
            }
        }

        if (context.action == aimAction && context.performed)
        {
            AimScreenPosition = context.ReadValue<Vector2>();
            HasAimPosition = true;
        }

        if(context.action == dashAction && context.performed)
        {
                DashPressed = true;
        }
    }

    #region 대쉬 상태 파악
    public bool ConsumeDash()
    {
        if (!DashPressed) return false;

        DashPressed = false;
        return true;
    }

    #endregion
}
