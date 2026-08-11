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
    public Vector2 AimScreenPostion { get; private set; }

    //인풋액션 저장
    private PlayerInput playerInput;
    private InputAction moventAction;
    private InputAction aimAction;


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moventAction = playerInput.actions.FindAction("Move", true);
        aimAction = playerInput.actions.FindAction("Aim", true);
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
        if(context.action == moventAction)
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
        else if(context.action == aimAction)
        {
            if(context.performed)
            {
                AimScreenPostion = context.ReadValue<Vector2>();
            } 
        }
    }
}
