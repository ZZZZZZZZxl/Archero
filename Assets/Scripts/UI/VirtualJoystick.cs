using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI")]
    [SerializeField] private RectTransform touchArea;
    [SerializeField] private RectTransform visualRoot;
    [SerializeField] private CanvasGroup visualGroup;
    [SerializeField] private RectTransform nub;

    [Header("Feel")]
    [Range(0f, 0.45f)]
    [SerializeField] private float deadZone = 0.05f;
    [SerializeField] private float maxNubDistance = 72f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public Vector2 Direction { get; private set; }
    public float RawIntensity { get; private set; }
    public float Intensity { get; private set; }
    public float SpeedFactor { get; private set; }
    public bool IsDragging { get; private set; }
    public bool HasInput => Intensity > 0f;

    private const int NoPointer = int.MinValue;
    private int activePointerId = NoPointer;
    private Vector2 startLocalPoint;

    private float Radius
    {
        get
        {
            if (maxNubDistance > 0f)
            {
                return maxNubDistance;
            }

            RectTransform radiusSource = visualRoot != null ? visualRoot : touchArea;
            Rect rect = radiusSource.rect;
            return Mathf.Min(rect.width, rect.height) * 0.5f;
        }
    }

    private void Awake()
    {
        if (touchArea == null)
        {
            touchArea = transform as RectTransform;
        }

        if (visualRoot == null)
        {
            visualRoot = transform as RectTransform;
        }

        if (visualGroup == null && visualRoot != null)
        {
            visualGroup = visualRoot.GetComponent<CanvasGroup>();
        }

        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.raycastTarget = true;
        }

        SetVisible(false);
        ResetInput();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsDragging)
        {
            return;
        }

        if (!TryGetLocalPoint(eventData, out startLocalPoint))
        {
            return;
        }

        activePointerId = eventData.pointerId;
        IsDragging = true;

        if (visualRoot != null)
        {
            visualRoot.anchoredPosition = startLocalPoint;
        }

        SetVisible(true);
        UpdateInput(Vector2.zero);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsActivePointer(eventData))
        {
            return;
        }

        if (!TryGetLocalPoint(eventData, out Vector2 currentLocalPoint))
        {
            return;
        }

        UpdateInput(currentLocalPoint - startLocalPoint);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsActivePointer(eventData))
        {
            return;
        }

        activePointerId = NoPointer;
        IsDragging = false;
        ResetInput();
        SetVisible(false);
    }

    private void OnDisable()
    {
        activePointerId = NoPointer;
        IsDragging = false;
        ResetInput();
        SetVisible(false);
    }

    private bool TryGetLocalPoint(PointerEventData eventData, out Vector2 localPoint)
    {
        if (touchArea == null)
        {
            localPoint = Vector2.zero;
            return false;
        }

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            touchArea,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint);
    }

    private bool IsActivePointer(PointerEventData eventData)
    {
        return IsDragging && eventData.pointerId == activePointerId;
    }

    private void UpdateInput(Vector2 delta)
    {
        if (nub == null)
        {
            return;
        }

        float radius = Mathf.Max(Radius, 1f);
        Vector2 clampedPoint = Vector2.ClampMagnitude(delta, radius);
        nub.anchoredPosition = clampedPoint;

        float rawIntensity = Mathf.Clamp01(clampedPoint.magnitude / radius);
        RawIntensity = rawIntensity;
        if (rawIntensity <= deadZone)
        {
            Direction = Vector2.zero;
            Intensity = 0f;
            SpeedFactor = 0f;
            return;
        }

        Direction = clampedPoint.normalized;
        Intensity = Mathf.InverseLerp(deadZone, 1f, rawIntensity);
        SpeedFactor = Mathf.Clamp01(speedCurve.Evaluate(Intensity));
    }

    private void ResetInput()
    {
        Direction = Vector2.zero;
        RawIntensity = 0f;
        Intensity = 0f;
        SpeedFactor = 0f;

        if (nub != null)
        {
            nub.anchoredPosition = Vector2.zero;
        }
    }

    private void SetVisible(bool visible)
    {
        if (visualGroup == null)
        {
            return;
        }

        visualGroup.alpha = visible ? 1f : 0f;
        visualGroup.interactable = false;
        visualGroup.blocksRaycasts = false;
    }
}
