using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorExitTrigger : MonoBehaviour
{
    [SerializeField] private DoorController _doorController;

    private bool _hasTriggered;

    private void Awake()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;

        if (!_doorController)
            _doorController = GetComponentInParent<DoorController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered)
            return;

        if (!_doorController || !_doorController.IsOpen)
            return;

        if (GameFlowManager.MainInstance && !GameFlowManager.MainInstance.IsPlaying)
            return;

        if (!other.GetComponentInParent<PlayerController>())
            return;

        _hasTriggered = true;
        if (GameFlowManager.MainInstance)
            GameFlowManager.MainInstance.EnterNextLevel();
    }

    public void ResetTrigger()
    {
        _hasTriggered = false;
    }
}
