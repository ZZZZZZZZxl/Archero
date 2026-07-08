using UnityEngine;

[DisallowMultipleComponent]
public class TrapDamageController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _damage = 10f;
    [SerializeField, Min(0.01f)] private float _damageInterval = 0.5f;

    private float _lastDamageTime = -999f;

    public void TryDamage(PlayerController player, Vector3 hitPoint)
    {
        if (!player)
            return;

        if (GameFlowManager.MainInstance && !GameFlowManager.MainInstance.IsPlaying)
            return;

        if (Time.time - _lastDamageTime < _damageInterval)
            return;

        _lastDamageTime = Time.time;
        GameEventManager.MainInstance.Call(EventName.PlayerHit, _damage, hitPoint, transform);
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player)
            TryDamage(player, other.ClosestPoint(transform.position));
    }
}
