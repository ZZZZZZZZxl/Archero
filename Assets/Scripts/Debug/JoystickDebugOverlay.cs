using UnityEngine;

public class JoystickDebugOverlay : MonoBehaviour
{
    [SerializeField] private VirtualJoystick _joystick;
    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private KeyCode _toggleKey = KeyCode.F3;
    [SerializeField] private bool _visible = true;
    [SerializeField, Range(0.8f, 1.6f)] private float _uiScale = 1.15f;

    private GUIStyle _boxStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _smallLabelStyle;
    private GUIStyle _titleStyle;
    private Texture2D _barTexture;
    private Font _debugFont;

    private void Awake()
    {
        TryResolveReferences();
        _barTexture = Texture2D.whiteTexture;
    }

    private void Update()
    {
        if (!_joystick || !_playerAnimator)
            TryResolveReferences();

        if (Input.GetKeyDown(_toggleKey))
            _visible = !_visible;
    }

    private void TryResolveReferences()
    {
        if (!_joystick)
            _joystick = FindObjectOfType<VirtualJoystick>();

        if (!_playerAnimator)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player)
                _playerAnimator = player.GetComponent<Animator>();
        }
    }

    private void OnGUI()
    {
        if (!_visible || !_joystick)
            return;

        EnsureStyles();

        Matrix4x4 previousMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * _uiScale);

        const float width = 610f;
        const float height = 500f;
        const float margin = 24f;
        float scaledScreenWidth = Screen.width / _uiScale;
        float scaledScreenHeight = Screen.height / _uiScale;
        Rect panelRect = new Rect(
            scaledScreenWidth - width - margin,
            scaledScreenHeight - height - margin,
            width,
            height);

        GUI.Box(panelRect, GUIContent.none, _boxStyle);

        float x = panelRect.x + 20f;
        float y = panelRect.y + 16f;
        float contentWidth = panelRect.width - 40f;

        GUI.Label(new Rect(x, y, contentWidth, 28f), "摇杆移动调试", _titleStyle);
        y += 38f;

        DrawValue(ref y, x, contentWidth, "有效强度 Intensity", _joystick.Intensity);

        float animatorMoveSpeed = GetAnimatorMoveSpeed();
        DrawValue(ref y, x, contentWidth, "转换成 MoveSpeed", animatorMoveSpeed);

        y += 8f;
        GUI.Label(new Rect(x, y, contentWidth, 24f), $"当前动作: {GetCurrentAction(animatorMoveSpeed)}", _labelStyle);
        y += 30f;
        GUI.Label(new Rect(x, y, contentWidth, 24f), $"摇杆方向: ({_joystick.Direction.x:0.00}, {_joystick.Direction.y:0.00})", _labelStyle);
        y += 24f;
        GUI.Label(new Rect(x, y, contentWidth, 24f), $"是否有输入: {_joystick.HasInput}", _labelStyle);
        y += 70f;

        DrawBlendSpace(new Rect(x, y, contentWidth, 92f), animatorMoveSpeed);
        y += 120f;
        GUI.Label(new Rect(x, y, contentWidth, 22f), $"按 {_toggleKey} 显示 / 隐藏调试面板", _smallLabelStyle);

        GUI.matrix = previousMatrix;
    }

    private float GetAnimatorMoveSpeed()
    {
        if (!_playerAnimator)
            return 0f;

        return _playerAnimator.GetFloat(AnimationParams.MoveSpeed);
    }

    private static string GetMoveBand(float moveSpeed)
    {
        if (moveSpeed <= 0f)
            return "停止";
        if (moveSpeed < 0.35f)
            return "慢走";
        if (moveSpeed < 0.8f)
            return "快走";
        return "奔跑";
    }

    private string GetCurrentAction(float animatorMoveSpeed)
    {
        if (!_joystick.HasInput)
            return "攻击 / 停止";

        return GetMoveBand(animatorMoveSpeed);
    }

    private void DrawValue(ref float y, float x, float width, string label, float value)
    {
        GUI.Label(new Rect(x, y, 190f, 24f), label, _labelStyle);
        GUI.Label(new Rect(x + width - 70f, y, 70f, 24f), $"{value * 100f:0}%", _labelStyle);
        DrawBar(new Rect(x + 190f, y + 5f, width - 270f, 16f), value);

        y += 36f;
    }

    private void DrawBlendSpace(Rect rect, float animatorMoveSpeed)
    {
        Rect titleRect = new Rect(rect.x, rect.y - 28f, rect.width, 22f);
        GUI.Label(titleRect, "MoveSpeed 在 BlendTree 中的位置", _labelStyle);

        Rect barRect = new Rect(rect.x, rect.y + 22f, rect.width, 28f);
        DrawGradientSegment(barRect, 0f, 0.35f, new Color(0.2f, 0.58f, 1f, 0.92f), new Color(0.25f, 0.9f, 1f, 0.92f));
        DrawGradientSegment(barRect, 0.35f, 0.8f, new Color(0.2f, 0.95f, 0.55f, 0.92f), new Color(0.85f, 0.95f, 0.25f, 0.92f));
        DrawGradientSegment(barRect, 0.8f, 1f, new Color(1f, 0.72f, 0.25f, 0.92f), new Color(1f, 0.32f, 0.22f, 0.92f));

        DrawBoundary(barRect, 0f, "0");
        DrawBoundary(barRect, 0.35f, "0.35");
        DrawBoundary(barRect, 0.8f, "0.8");
        DrawBoundary(barRect, 1f, "1");

        DrawSegmentLabel(barRect, 0f, 0.35f, "慢走区间");
        DrawSegmentLabel(barRect, 0.35f, 0.8f, "快走区间");
        DrawSegmentLabel(barRect, 0.8f, 1f, "奔跑区间");

        DrawBlendMarker(barRect, _joystick.SpeedFactor, "输入", 0f, new Color(1f, 1f, 1f, 1f));
        DrawBlendMarker(barRect, animatorMoveSpeed, "动画", 36f, new Color(0.55f, 0.85f, 1f, 1f));
    }

    private void DrawBar(Rect rect, float value)
    {
        GUI.color = new Color(1f, 1f, 1f, 0.18f);
        GUI.DrawTexture(rect, _barTexture);
        GUI.color = new Color(0.35f, 0.75f, 1f, 0.9f);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(value), rect.height), _barTexture);
        GUI.color = Color.white;
    }

    private void DrawGradientSegment(Rect rect, float from, float to, Color startColor, Color endColor)
    {
        const int steps = 24;
        float segmentX = rect.x + rect.width * from;
        float segmentWidth = rect.width * (to - from);

        for (int i = 0; i < steps; i++)
        {
            float t0 = i / (float)steps;
            float t1 = (i + 1) / (float)steps;
            GUI.color = Color.Lerp(startColor, endColor, t0);
            GUI.DrawTexture(
                new Rect(segmentX + segmentWidth * t0, rect.y, segmentWidth * (t1 - t0) + 1f, rect.height),
                _barTexture);
        }

        GUI.color = Color.white;
    }

    private void DrawBoundary(Rect rect, float value, string label)
    {
        float x = rect.x + rect.width * Mathf.Clamp01(value);
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(x - 1f, rect.y - 4f, 2f, rect.height + 8f), _barTexture);
        GUI.color = Color.white;

        float labelWidth = value >= 0.95f ? 32f : 46f;
        float labelX = Mathf.Clamp(x - labelWidth * 0.5f, rect.x, rect.xMax - labelWidth);
        GUI.Label(new Rect(labelX, rect.y + rect.height + 4f, labelWidth, 20f), label, _smallLabelStyle);
    }

    private void DrawSegmentLabel(Rect rect, float from, float to, string label)
    {
        float startX = rect.x + rect.width * from;
        float width = rect.width * (to - from);
        GUI.Label(new Rect(startX, rect.y - 24f, width, 22f), label, _smallLabelStyle);
    }

    private void DrawBlendMarker(Rect rect, float value, string label, float labelOffsetY, Color color)
    {
        float x = rect.x + rect.width * Mathf.Clamp01(value);
        GUI.color = color;
        GUI.DrawTexture(new Rect(x - 3f, rect.y - 8f, 6f, rect.height + 16f), _barTexture);
        GUI.color = Color.white;

        string text = $"{label}: {value:0.00}";
        float labelWidth = 110f;
        float labelX = Mathf.Clamp(x - labelWidth * 0.5f, rect.x, rect.xMax - labelWidth);
        GUI.Label(new Rect(labelX, rect.y + rect.height + 24f + labelOffsetY, labelWidth, 22f), text, _smallLabelStyle);
    }

    private void EnsureStyles()
    {
        if (_boxStyle != null)
            return;

        try
        {
            _debugFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch
        {
            _debugFont = null;
        }

        _boxStyle = new GUIStyle(GUI.skin.box);
        _boxStyle.normal.background = MakeTexture(new Color(0f, 0f, 0f, 0.72f));

        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            font = _debugFont,
            normal = { textColor = Color.white }
        };

        _smallLabelStyle = new GUIStyle(_labelStyle)
        {
            fontSize = 14,
            normal = { textColor = new Color(1f, 1f, 1f, 0.82f) }
        };

        _titleStyle = new GUIStyle(_labelStyle)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold
        };
    }

    private static Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
