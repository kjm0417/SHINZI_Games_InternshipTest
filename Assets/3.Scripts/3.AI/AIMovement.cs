using UnityEngine;
using UnityEngine.AI;

public class AIMovement : MonoBehaviour, IKnockbackReceiver
{
    //넉백 관련
    [SerializeField] private float knockbackDecay = 20f;
    private Vector3 knockbackVelocity;

    //대쉬 관련
    private float dashRemainingTime;
    private float dashCooldownRemaining;
    private Vector3 dashDirection;

    // 대시가 끝난 뒤 기존 경로를 이어갈지 저장
    private bool resumePathAfterDash;

    private AIData aiData;
    public bool IsDashing => dashRemainingTime > 0f;
    public bool IsDashReady => dashCooldownRemaining <= 0f;

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        UpdateDash(deltaTime);
        UpdateKnockback(deltaTime);
    }



    public bool Initialize(AIData data)
    {
        if (data == null)
        {
            return false;
        }

        aiData = data;

        agent.speed = data.Speed;

        dashRemainingTime = 0f;
        dashCooldownRemaining = 0f;
        dashDirection = Vector3.zero;
        resumePathAfterDash = false;


        knockbackVelocity = Vector3.zero;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        return true;
    }


    public void MoveTo(Vector3 destination)
    {
        if (!agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        //대시 중에는 일반 이동으로 대시를 취소x
        //대시 종료 후 이어갈 목적지를 갱신
        if (IsDashing)
        {
            resumePathAfterDash = true;
            agent.SetDestination(destination);
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (!agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        //대시 자체는 유지 대시 종료 후 기존 경로를 다시 시작 x
        if (IsDashing)
        {
            resumePathAfterDash = false;
            agent.ResetPath();
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();
    }

    public void ApplyKnockback(Vector3 direction, float power)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f || power <= 0f) return;

        knockbackVelocity = direction.normalized * power;
    }
    private void UpdateKnockback(float deltaTime)
    {
        if (!agent.enabled || !agent.isOnNavMesh)
        {
            knockbackVelocity = Vector3.zero;
            return;
        }

        if (knockbackVelocity.sqrMagnitude < 0.001f) return;

        agent.Move(knockbackVelocity * deltaTime);

        knockbackVelocity = Vector3.MoveTowards( knockbackVelocity, Vector3.zero, knockbackDecay * deltaTime);

        
    }

    public bool TryDash(Vector3 direction)
    {
        if (aiData == null)
        {
            return false;
        }

        if (IsDashing || !IsDashReady)
        {
            return false;
        }

        if (!agent.enabled || !agent.isOnNavMesh)
        {
            return false;
        }

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return false;
        }

        //대쉬 끝나고 해당 경로 이어서 이동
        resumePathAfterDash = !agent.isStopped && agent.hasPath;

        dashDirection = direction.normalized;
        dashRemainingTime = aiData.DashDuration;
        dashCooldownRemaining = aiData.DashCooldown;

        // NavMeshAgent의 자동 경로 이동만 일시 정지
        agent.isStopped = true;

        return true;
    }

    private void UpdateDash(float deltaTime)
    {
        if (dashCooldownRemaining > 0f)
        {
            dashCooldownRemaining = Mathf.Max(0f, dashCooldownRemaining - deltaTime);
        }

        if (!IsDashing)
        {
            return;
        }

        if (!agent.enabled || !agent.isOnNavMesh)
        {
            CancelDash();
            return;
        }

        agent.Move(dashDirection * aiData.DashSpeed * deltaTime);

        dashRemainingTime = Mathf.Max(0f, dashRemainingTime - deltaTime);

        if (dashRemainingTime <= 0f)
        {
            FinishDash();
        }
    }

    private void FinishDash()
    {
        dashRemainingTime = 0f;
        dashDirection = Vector3.zero;

        if (!agent.enabled || !agent.isOnNavMesh)
        {
            resumePathAfterDash = false;
            return;
        }

        //Chase나 근접 Engage 상태였다면
        //대시 이전 또는 대시 중 갱신된 경로를 다시 시작
        if (resumePathAfterDash && agent.hasPath)
        {
            agent.isStopped = false;
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        resumePathAfterDash = false;
    }

    public void CancelDash()
    {
        dashRemainingTime = 0f;
        dashDirection = Vector3.zero;
        resumePathAfterDash = false;

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }
}
