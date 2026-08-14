using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;

    //이동 관련 정보 가져오기
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAim aim;

    //체력 관련 정보
    [SerializeField] private PlayerData playerData;
    [SerializeField] private CharacterHealthSystem healthSystem;

    

    private void Awake()
    {
        if (inputReader == null) inputReader = GetComponent<InputReader>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (aim == null) aim = GetComponent<PlayerAim>();
        if (healthSystem == null) healthSystem = GetComponent<CharacterHealthSystem>();
    }

    private void Start()
    {
        healthSystem.Initialize(playerData.MaxHp);
    }

    //캐릭터 컨트롤러는 어디 위치로 이동해라 이기때문에 fixUpdate말고 Update를 사용
    private void Update()
    {
        HandleDash();
        HandleMovement();
        HandleAim();
        
    }

    private void HandleMovement()
    {
        movement.Tick(inputReader.MoveInput, Time.deltaTime);
    }

    private void HandleAim()
    {
        if (!inputReader.HasAimPosition) return;

        aim.Tick(inputReader.AimScreenPosition);
    }

    private void HandleDash()
    {
        if (!inputReader.ConsumeDash()) return;

        movement.TryDash(inputReader.MoveInput);
    }
}
