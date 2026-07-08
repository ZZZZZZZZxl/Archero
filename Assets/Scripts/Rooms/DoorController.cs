using UnityEngine;

public class DoorController : MonoBehaviour
{
    private const string BlockerName = "BlockCollider";

    [SerializeField] private RoomController _room;
    [SerializeField] private Collider _blocker;
    [SerializeField] private string _openTriggerName = "Open";

    private bool _isOpen;
    private int _openTriggerHash;
    private Animator _animator;

    public bool IsOpen => _isOpen;

    private void Awake()
    {
        ResolveReferences();
        _openTriggerHash = Animator.StringToHash(_openTriggerName);
        _isOpen = false;
    }

    private void OnEnable()
    {
        ResolveReferences();
        GameEventManager.MainInstance.AddEvent<RoomController>(EventName.RoomCleared, OnRoomCleared);

        if (_room && _room.IsCleared)
            Open();
    }

    private void OnDisable()
    {
        GameEventManager.MainInstance.RemoveEvent<RoomController>(EventName.RoomCleared, OnRoomCleared);
    }

    private void ResolveReferences()
    {
        if (!_room)
            _room = GetComponentInParent<RoomController>();

        if (!_blocker)
            _blocker = FindNamedBlocker();

        if (!_animator)
            _animator = GetComponentInChildren<Animator>(true);
    }

    private Collider FindNamedBlocker()
    {
        Transform blockerTransform = transform.Find(BlockerName);
        if (blockerTransform && blockerTransform.TryGetComponent(out Collider namedBlocker))
            return namedBlocker;

        return null;
    }

    private void OnRoomCleared(RoomController room)
    {
        if (_room && room != _room)
            return;

        Open();
    }

    public void Open()
    {
        if (_isOpen)
            return;

        _isOpen = true;
        if (AudioManager.MainInstance)
            AudioManager.MainInstance.PlaySfx(AudioSfx.DoorOpen);

        if (_animator)
            _animator.SetTrigger(_openTriggerHash);
        else
            OnDoorOpen();
    }

    public void OnDoorOpen()
    {
        if (_blocker)
            _blocker.enabled = false;
    }

    public void ResetClosed()
    {
        ResolveReferences();
        _isOpen = false;

        if (_blocker)
            _blocker.enabled = true;

        if (_animator)
        {
            _animator.Rebind();
            _animator.Update(0f);
        }

        DoorExitTrigger[] exitTriggers = GetComponentsInChildren<DoorExitTrigger>(true);
        foreach (DoorExitTrigger exitTrigger in exitTriggers)
            exitTrigger.ResetTrigger();
    }
}
