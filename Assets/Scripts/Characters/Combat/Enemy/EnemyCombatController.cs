using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyHealthController))]
[RequireComponent(typeof(EnemyChaseController))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyCombatController : MonoBehaviour
{
    private EnemyHealthController _healthCtrl;
    private EnemyChaseController _chaseController;
    private NavMeshAgent _agent;
    private HitFlashFeedback _hitFlashFeedback;
    private DamageNumberFeedback _damageNumberFeedback;
    private Coroutine _knockbackCoroutine;

    private void Awake()
    {
        _healthCtrl = GetComponent<EnemyHealthController>();
        _chaseController = GetComponent<EnemyChaseController>();
        _agent = GetComponent<NavMeshAgent>();
        if (!_agent)
            _agent = gameObject.AddComponent<NavMeshAgent>();

        _hitFlashFeedback = GetComponent<HitFlashFeedback>();
        if (!_hitFlashFeedback)
            _hitFlashFeedback = gameObject.AddComponent<HitFlashFeedback>();

        _damageNumberFeedback = GetComponent<DamageNumberFeedback>();
        if (!_damageNumberFeedback)
            _damageNumberFeedback = gameObject.AddComponent<DamageNumberFeedback>();
    }

    public void OnHit(
        float damage,
        Vector3 hitDirection,
        float knockbackDistance,
        float knockbackDuration
    )
    {
        if (!_healthCtrl || _healthCtrl.IsDead)
            return;

        _healthCtrl.TakeDamage(damage);
        _hitFlashFeedback.Play();
        _damageNumberFeedback.Play(damage);

        if (AudioManager.MainInstance)
            AudioManager.MainInstance.PlaySfx(AudioSfx.EnemyHit);

        if (!_healthCtrl.IsDead)
            ApplyKnockback(hitDirection, knockbackDistance, knockbackDuration);
    }

    private void ApplyKnockback(Vector3 direction, float distance, float duration)
    {
        if (distance <= 0f || duration <= 0f)
            return;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();

        if (_knockbackCoroutine != null)
            StopCoroutine(_knockbackCoroutine);

        _knockbackCoroutine = StartCoroutine(KnockbackCoroutine(direction, distance, duration));
    }

    private IEnumerator KnockbackCoroutine(Vector3 direction, float distance, float duration)
    {
        if (_chaseController)
            _chaseController.SetMovementLocked(true);

        float timer = 0f;
        float previousT = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float stepDistance = (t - previousT) * distance;
            MoveOnNavMesh(direction * stepDistance);
            previousT = t;
            yield return null;
        }

        if (_chaseController)
            _chaseController.SetMovementLocked(false);

        _knockbackCoroutine = null;
    }

    private void MoveOnNavMesh(Vector3 delta)
    {
        if (delta.sqrMagnitude < 0.000001f)
            return;

        if (!_agent || !_agent.enabled || !_agent.isOnNavMesh)
            return;

        Vector3 start = transform.position;
        Vector3 target = start + delta;

        if (NavMesh.Raycast(start, target, out NavMeshHit hit, NavMesh.AllAreas))
            target = hit.position;

        _agent.Move(target - start);
    }

    private void OnDisable()
    {
        if (_knockbackCoroutine != null)
        {
            StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = null;
        }

        if (_chaseController)
            _chaseController.SetMovementLocked(false);
    }
}
