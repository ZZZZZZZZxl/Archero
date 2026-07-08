using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform _selectNode;

    public bool HasSelectNode => _selectNode;

    private void Start()
    {
        SetSelected(false);
    }

    public void SetSelectNode(Transform selectNode)
    {
        _selectNode = selectNode;
        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (!_selectNode)
            return;

        _selectNode.gameObject.SetActive(isSelected);
    }
}
