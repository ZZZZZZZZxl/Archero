using UnityEngine;

public class PlayerReusableData
{
    private Transform _enemy;
    private bool _hasInput;

    public ref Transform Enemy => ref _enemy;
    public ref bool HasInput => ref _hasInput;
}