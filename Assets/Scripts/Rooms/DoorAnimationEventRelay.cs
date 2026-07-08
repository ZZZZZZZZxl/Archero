using UnityEngine;

public class DoorAnimationEventRelay : MonoBehaviour
{
    [SerializeField] private DoorController _doorController;

    private void Awake()
    {
        if (!_doorController)
            _doorController = GetComponentInParent<DoorController>();
    }

    public void OnDoorOpen()
    {
        if (!_doorController)
            _doorController = GetComponentInParent<DoorController>();

        if (_doorController)
            _doorController.OnDoorOpen();
    }
}
