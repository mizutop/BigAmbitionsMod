using System;
using BigAmbitionsTrainer.Config;
using BigAmbitionsTrainer.L;
using BigAmbitionsTrainer.Modules;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BigAmbitionsTrainer.UI;

/// <summary>
/// F8 IMGUI 悬浮修改器面板（Mono netstandard 版）。
/// 全部文案走 Loc 本地化，支持中英切换；布局针对中文做了宽度/行高适配。
/// </summary>
public static class TrainerOverlay
{
    // —— 状态 ——
    private static bool _visible;
    private static bool _closing;
    private static float _animStartTime;
    private const float FadeDuration = 0.15f;
    private static int _activeTab;
    private static Vector2 _scrollPos;
    private static bool _stylesInited;
    private static bool _needRebuild;   // 主题/缩放变化后置位，下一帧开头安全重建

    // —— 窗口位置 / 拖动 ——
    private static Vector2 _winPos;            // 面板左上角（屏幕坐标）
    private static bool _winPosInitialized;
    private static bool _dragging;
    private static Vector2 _dragOffset;        // 按下点相对窗口左上角的偏移
    private static float[] _tabScrollPos = new float[TabCount]; // 每个 tab 独立滚动位置

    // —— 尺寸 ——
    private const float WinW = 980f;
    private const float WinH = 680f;
    private const float TitleBarH = 36f;
    private const float TabH = 42f;
    private const float ContentTop = 90f;
    private const float ContentH = 570f;
    private const int TabCount = 8;

    // —— 缩放 / 透明度 ——
    private static float _uiScale = 1f;
    private static float _uiOpacity = 1f;

    // —— 组件 id ——
    private const int IdEnergy = 20;
    private const int IdGameSpeed = 40;
    private const int IdWorldInterest = 41;
    private const int IdStaffSalary = 50;
    private const int IdMoneyTax = 10, IdMoneyPrice = 11, IdMoneyExport = 12;
    private const int IdBizPromo = 30, IdBizSalary = 31, IdBizInterest = 32, IdBizRivals = 33;
    private const int IdBizWholesale = 34, IdBizImporter = 35;

    private static int _draggingSliderId = -1;
    private static int _focusedInputId = -1;
    private static bool _colorDirty;            // 有颜色变更待保存
    private static string[] _inputTexts = new string[8];
    private static float _cursorBlinkTime;
    private static bool _cursorVisible;

    private static GUIStyle _winStyle, _winBorderStyle, _tabStyle, _tabActiveStyle, _tabHoverStyle,
        _btnStyle, _sectionStyle, _titleStyle, _sliderValueStyle, _inputTextStyle;
    private static Texture2D _winBg, _winBorder, _tabBg, _tabActiveBg, _tabHoverBg, _btnBg,
        _sectionBg, _accentLine, _track, _fill, _handle, _inputBg, _inputBorder;

    // —— 色板（全部读当前主题，支持运行期切换）——
    private static Color AccentBlue => ThemeManager.Current.Primary;
    private static Color AccentGreen => ThemeManager.Current.Success;
    private static Color AccentRed => ThemeManager.Current.Danger;
    private static Color AccentOrange => ThemeManager.Current.Warning;
    private static Color TextMuted => ThemeManager.Current.TextMuted;
    private static Color BtnNeutral => ThemeManager.Current.BtnNeutral;
    private static Color BtnNeutralHover => ThemeManager.Current.BtnNeutralHover;
    private static Color GreenHover => ThemeManager.Current.SuccessHover;
    private static Color BlueHover => ThemeManager.Current.PrimaryHover;
    private static Color RedHover => ThemeManager.Current.DangerHover;
    private static Color OrangeHover => ThemeManager.Current.WarningHover;
    private static Color TextLight => ThemeManager.Current.TextLight;
    private static Color BtnText => ThemeManager.Current.BtnText;
    private static Color CardBg => ThemeManager.Current.CardBg;
    private static Color SectionBg => ThemeManager.Current.CardBg;
    private static Color TrackBg => ThemeManager.Current.CardBg;
    private static Color InputBorder => ThemeManager.Current.BtnNeutralHover;
    private static Color WindowBg => ThemeManager.Current.WindowBg;
    private static Color WindowBorder => ThemeManager.Current.WindowBorder;
    private static Color TabBg => ThemeManager.Current.TabBg;
    private static Color TabHoverBg => ThemeManager.Current.TabHoverBg;
    private static Color TabText => ThemeManager.Current.TabText;
    private static Color SectionText => ThemeManager.Current.SectionText;
    private static Color TitleText => ThemeManager.Current.TitleText;
    private static Color WhiteText => ThemeManager.Current.WhiteText;

    public static bool Visible => _visible;

    public static void Toggle()
    {
        if (_closing) { _closing = false; _visible = true; _animStartTime = Time.unscaledTime; }
        else if (_visible) { Close(); }
        else { Open(); }
    }

    /// <summary>打开面板：恢复上次 tab 与窗口位置。</summary>
    private static void Open()
    {
        _visible = true;
        _animStartTime = Time.unscaledTime;
        _activeTab = Mathf.Clamp(TrainerConfig.LastTab.Value, 0, TabCount - 1);
        // 窗口位置初始化/恢复交由 OnGUI 首次渲染时按当前缩放完成
        _draggingSliderId = -1;
        _focusedInputId = -1;
    }

    /// <summary>关闭面板：记录当前 tab 与位置并保存。</summary>
    private static void Close()
    {
        TrainerConfig.LastTab.Value = _activeTab;
        SaveWindowPosition();
        TrainerConfig.Save();
        _closing = true;
        _animStartTime = Time.unscaledTime;
    }

    private static void SaveWindowPosition()
    {
        TrainerConfig.WinPosX.Value = _winPos.x;
        TrainerConfig.WinPosY.Value = _winPos.y;
    }

    /// <summary>将窗口位置（UI 空间）钳位到屏幕内（至少露出标题栏可拖动）。</summary>
    private static void ClampToScreen()
    {
        float sw = Screen.width / _uiScale;
        float sh = Screen.height / _uiScale;
        _winPos.x = Mathf.Clamp(_winPos.x, -WinW + 140f, sw - 60f);
        _winPos.y = Mathf.Clamp(_winPos.y, -TitleBarH, sh - 60f);
    }

    public static void OnGUI()
    {
        if (!_visible && !_closing) return;

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
        {
            if (_focusedInputId >= 0) { _focusedInputId = -1; Event.current.Use(); return; }
            if (_visible && !_closing) { Close(); Event.current.Use(); return; }
        }

        try
        {
            // 读取缩放 / 透明度配置
            _uiScale = Mathf.Clamp(TrainerConfig.PanelScale.Value, 0.6f, 1.8f);
            _uiOpacity = Mathf.Clamp01(TrainerConfig.PanelOpacity.Value);

            float elapsed = Time.unscaledTime - _animStartTime;
            float alpha = _closing ? 1f - Mathf.Clamp01(elapsed / FadeDuration) : Mathf.Clamp01(elapsed / FadeDuration);
            alpha *= _uiOpacity; // 叠加面板不透明度
            if (_closing && alpha <= 0f) { DestroyStyles(); _visible = false; _closing = false; _focusedInputId = -1; return; }

            _cursorBlinkTime += Time.unscaledDeltaTime;
            if (_cursorBlinkTime > 0.5f) { _cursorVisible = !_cursorVisible; _cursorBlinkTime = 0f; }
            if (_needRebuild) { DestroyStyles(); _needRebuild = false; }
            EnsureStyles();

            // 整体缩放（GUI.matrix 放大绘制与事件坐标；布局坐标为 UI 空间）
            Matrix4x4 prevMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(_uiScale, _uiScale, 1f));

            try
            {
                // 首次渲染时初始化窗口位置（UI 空间；居中或恢复保存值）
                if (!_winPosInitialized)
                {
                    float savedX = TrainerConfig.WinPosX.Value;
                    float savedY = TrainerConfig.WinPosY.Value;
                    _winPos = (savedX >= 0f && savedY >= 0f)
                        ? new Vector2(savedX, savedY)
                        : new Vector2((Screen.width / _uiScale - WinW) * 0.5f, (Screen.height / _uiScale - WinH) * 0.5f);
                    _winPosInitialized = true;
                }
                ClampToScreen();

                float cx = _winPos.x;
                float cy = _winPos.y;
                Color prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha);

                DrawWindow(cx, cy);
                DrawTitleBar(cx, cy);
                HandleDrag(cx, cy);
                DrawTabs(cx, cy);

                float vw = WinW - 30f;
                Rect viewRect = new Rect(cx + 15f, cy + ContentTop, vw, ContentH);
                float lastScroll = _tabScrollPos[_activeTab];
                _scrollPos.y = lastScroll;
                if (Event.current.type == EventType.ScrollWheel && _focusedInputId < 0 && viewRect.Contains(Event.current.mousePosition))
                { _scrollPos.y -= Event.current.delta.y * 40f / _uiScale; Event.current.Use(); }

                float sy = 6f - _scrollPos.y;
                float contentBottom = 0f, maxScroll = 0f;
                GUI.BeginGroup(viewRect);
                try
                {
                    switch (_activeTab)
                    {
                        case 0: DrawMoneyTab(ref sy, vw); break;
                        case 1: DrawPlayerTab(ref sy, vw); break;
                        case 2: DrawVehicleTab(ref sy, vw); break;
                        case 3: DrawBusinessTab(ref sy, vw); break;
                        case 4: DrawGameplayTab(ref sy, vw); break;
                        case 5: DrawStaffTab(ref sy, vw); break;
                        case 6: DrawRivalsTab(ref sy, vw); break;
                        case 7: DrawSettingsTab(ref sy, vw); break;
                    }
                    contentBottom = sy + 8f;
                    maxScroll = Mathf.Max(0f, contentBottom - ContentH);
                    _scrollPos.y = Mathf.Clamp(_scrollPos.y, 0f, maxScroll);
                    _tabScrollPos[_activeTab] = _scrollPos.y; // 记住当前 tab 的滚动位置
                }
                finally { GUI.EndGroup(); }

                DrawScrollbar(cx, cy, maxScroll, contentBottom);
                if (_draggingSliderId >= 0 && Event.current.type == EventType.MouseUp) _draggingSliderId = -1;
                GUI.color = prev;
            }
            finally { GUI.matrix = prevMatrix; }
        }
        catch (Exception ex) { MelonLogger.Warning("[TrainerOverlay] " + ex.Message); }
    }

    public static void EnsureStyles()
    {
        if (_stylesInited) return;
        var th = ThemeManager.Current;

        _winBg = MakeTex(th.WindowBg);
        _winStyle = new GUIStyle(); _winStyle.normal.background = _winBg;

        _winBorder = MakeBorderTex(th.WindowBorder);
        _winBorderStyle = new GUIStyle(); _winBorderStyle.normal.background = _winBorder;

        _tabBg = MakeTex(th.TabBg);
        _tabStyle = new GUIStyle();
        _tabStyle.normal.background = _tabBg;
        _tabStyle.normal.textColor = th.TabText;
        _tabStyle.fontSize = th.FontTab; _tabStyle.alignment = TextAnchor.MiddleCenter;

        _tabActiveBg = MakeTex(th.Primary);
        _tabActiveStyle = new GUIStyle();
        _tabActiveStyle.normal.background = _tabActiveBg;
        _tabActiveStyle.normal.textColor = th.WhiteText;
        _tabActiveStyle.fontSize = th.FontTab; _tabActiveStyle.alignment = TextAnchor.MiddleCenter;

        _tabHoverBg = MakeTex(th.TabHoverBg);
        _tabHoverStyle = new GUIStyle();
        _tabHoverStyle.normal.background = _tabHoverBg;
        _tabHoverStyle.normal.textColor = th.TextLight;
        _tabHoverStyle.fontSize = th.FontTab; _tabHoverStyle.alignment = TextAnchor.MiddleCenter;

        _btnBg = MakeTex(th.BtnNeutral);
        _btnStyle = new GUIStyle();
        _btnStyle.normal.background = _btnBg;
        _btnStyle.normal.textColor = th.BtnText;
        _btnStyle.fontSize = th.FontButton; _btnStyle.alignment = TextAnchor.MiddleCenter;
        _btnStyle.padding = new RectOffset(4, 4, 2, 2);

        _sectionBg = MakeTex(th.CardBg);
        _sectionStyle = new GUIStyle();
        _sectionStyle.normal.textColor = th.SectionText;
        _sectionStyle.fontSize = th.FontSection; _sectionStyle.alignment = TextAnchor.MiddleLeft;
        _sectionStyle.fontStyle = FontStyle.Bold;
        _sectionStyle.padding = new RectOffset(6, 4, 2, 2);

        _titleStyle = new GUIStyle();
        _titleStyle.normal.textColor = th.TitleText;
        _titleStyle.fontSize = th.FontTitle; _titleStyle.alignment = TextAnchor.MiddleLeft;
        _titleStyle.fontStyle = FontStyle.Bold;

        _sliderValueStyle = new GUIStyle();
        _sliderValueStyle.normal.textColor = th.Primary;
        _sliderValueStyle.fontSize = th.FontSliderValue; _sliderValueStyle.alignment = TextAnchor.MiddleRight;
        _sliderValueStyle.fontStyle = FontStyle.Bold;

        _inputTextStyle = new GUIStyle();
        _inputTextStyle.normal.textColor = th.TextLight;
        _inputTextStyle.fontSize = th.FontInput; _inputTextStyle.alignment = TextAnchor.MiddleLeft;
        _inputTextStyle.padding = new RectOffset(8, 8, 2, 2);

        _track = MakeTex(th.CardBg);
        _fill = MakeTex(th.Primary);
        _handle = MakeTex(Color.white);
        _inputBg = MakeTex(th.CardBg);
        _inputBorder = MakeTex(th.InputBorder);
        _accentLine = MakeTex(th.Primary);
        _stylesInited = true;
    }

    /// <summary>标记需要重建样式（主题 / 缩放变化后下一帧安全重建）。</summary>
    public static void InvalidateStyles()
    {
        _needRebuild = true;
    }

    public static void Cleanup() => DestroyStyles();

    private static void DestroyStyles()
    {
        _stylesInited = false;
        DestroyTex(ref _winBg); DestroyTex(ref _winBorder); DestroyTex(ref _tabBg);
        DestroyTex(ref _tabActiveBg); DestroyTex(ref _tabHoverBg); DestroyTex(ref _btnBg);
        DestroyTex(ref _sectionBg); DestroyTex(ref _accentLine); DestroyTex(ref _track);
        DestroyTex(ref _fill); DestroyTex(ref _handle); DestroyTex(ref _inputBg);
        DestroyTex(ref _inputBorder);
        _winStyle = _winBorderStyle = _tabStyle = _tabActiveStyle = _tabHoverStyle =
            _btnStyle = _sectionStyle = _titleStyle = _sliderValueStyle = _inputTextStyle = null;
    }

    private static void DestroyTex(ref Texture2D t)
    {
        if (t != null) { Object.DestroyImmediate(t); t = null; }
    }

    // ================= 基础组件 =================

    private static void SectionLabel(string text, ref float sy, float vw)
    {
        GUI.Box(new Rect(6f, sy, vw - 12f, 32f), GUIContent.none, _dyn(_sectionBg));
        GUI.Box(new Rect(6f, sy, 3f, 32f), GUIContent.none, _dyn(_accentLine));
        GUI.Label(new Rect(14f, sy, vw - 14f, 32f), text, _sectionStyle);
        sy += 44f;
    }

    private static bool ClickableColorBtn(Rect rect, string text, Color bg, Color hover)
    {
        Color prev = GUI.color;
        bool hovering = Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition);
        GUI.color = hovering ? hover : bg;
        GUI.Box(rect, GUIContent.none, _btnStyle);
        GUI.Label(rect, text, _btnStyle);
        GUI.color = prev;
        return IsClick(rect);
    }

    private static bool ToggleBtn(string label, bool current, float x, float y, float w)
    {
        Color prev = GUI.color;
        Rect rect = new Rect(x, y, w, 38f);
        bool hovering = Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition);
        float bright = hovering ? 1.15f : 1f;
        Color c = current ? new Color(AccentGreen.r * bright, AccentGreen.g * bright, AccentGreen.b * bright, 1f)
                          : new Color(BtnNeutral.r * bright, BtnNeutral.g * bright, BtnNeutral.b * bright, 1f);
        GUI.color = c;
        GUI.Box(rect, GUIContent.none, _btnStyle);
        GUI.Label(rect, "[" + (current ? Loc.T("on") : Loc.T("off")) + "]  " + label, _btnStyle);
        GUI.color = prev;
        return IsClick(rect) ? !current : current;
    }

    private static float CustomSlider(Rect rect, string label, float value, float min, float max, bool whole, int id)
    {
        float trackH = 14f, handleSize = 18f;
        float trackY = rect.y + (rect.height - trackH) * 0.5f;
        float trackX = rect.x + 150f;
        float trackW = rect.width - 150f - 70f;
        if (trackW < 20f) trackW = 20f;

        GUI.Label(new Rect(rect.x, rect.y, 146f, rect.height), label, _sectionStyle);
        string valStr = whole ? ((int)value).ToString() : value.ToString("F1");
        GUI.Label(new Rect(rect.x + rect.width - 66f, rect.y, 62f, rect.height), valStr, _sliderValueStyle);

        Rect trackRect = new Rect(trackX, trackY, trackW, trackH);
        GUI.Box(trackRect, GUIContent.none, _dyn(_track));
        float fillW = ((value - min) / (max - min)) * trackW;
        GUI.Box(new Rect(trackX, trackY, fillW, trackH), GUIContent.none, _dyn(_fill));

        float handleX = Mathf.Clamp(trackX + fillW - handleSize * 0.5f, trackX, trackX + trackW - handleSize);
        Rect handleRect = new Rect(handleX, rect.y + (rect.height - handleSize) * 0.5f, handleSize, handleSize);
        Color prev = GUI.color;
        bool over = Event.current.type == EventType.Repaint && (handleRect.Contains(Event.current.mousePosition) || _draggingSliderId == id);
        GUI.color = over ? new Color(1f, 1f, 0.7f, 1f) : new Color(0.9f, 0.9f, 0.9f, 0.95f);
        GUI.Box(handleRect, GUIContent.none, _dyn(_handle));
        GUI.color = prev;

        bool inTrack = handleRect.Contains(Event.current.mousePosition) || trackRect.Contains(Event.current.mousePosition);
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && inTrack)
        {
            _draggingSliderId = id;
            value = DragValue(trackX, trackW, min, max, whole);
            Event.current.Use();
        }
        if (_draggingSliderId == id && Event.current.type == EventType.MouseDrag)
        {
            value = DragValue(trackX, trackW, min, max, whole);
            Event.current.Use();
        }
        return value;
    }

    private static float DragValue(float trackX, float trackW, float min, float max, bool whole)
    {
        float rel = Mathf.Clamp01((Event.current.mousePosition.x - trackX) / trackW);
        float v = min + rel * (max - min);
        return whole ? Mathf.Round(v) : v;
    }

    /// <summary>紧凑滑条（无标签/无值文本），用于颜色 RGB 微调。</summary>
    private static float MiniSlider(Rect rect, float value, float min, float max, bool whole, int id)
    {
        float trackH = 14f, handleSize = 16f;
        float trackY = rect.y + (rect.height - trackH) * 0.5f;
        float pad = 6f;
        float trackX = rect.x + pad;
        float trackW = rect.width - pad * 2f;
        if (trackW < 14f) trackW = 14f;

        Rect trackRect = new Rect(trackX, trackY, trackW, trackH);
        GUI.Box(trackRect, GUIContent.none, _dyn(_track));
        float fillW = ((value - min) / (max - min)) * trackW;
        GUI.Box(new Rect(trackX, trackY, fillW, trackH), GUIContent.none, _dyn(_fill));
        float handleX = Mathf.Clamp(trackX + fillW - handleSize * 0.5f, trackX, trackX + trackW - handleSize);
        Rect handleRect = new Rect(handleX, rect.y + (rect.height - handleSize) * 0.5f, handleSize, handleSize);
        Color prev = GUI.color;
        bool over = Event.current.type == EventType.Repaint && (handleRect.Contains(Event.current.mousePosition) || _draggingSliderId == id);
        GUI.color = over ? new Color(1f, 1f, 0.7f, 1f) : new Color(0.9f, 0.9f, 0.9f, 0.95f);
        GUI.Box(handleRect, GUIContent.none, _dyn(_handle));
        GUI.color = prev;

        bool inTrack = handleRect.Contains(Event.current.mousePosition) || trackRect.Contains(Event.current.mousePosition);
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && inTrack)
        { _draggingSliderId = id; value = DragValue(trackX, trackW, min, max, whole); Event.current.Use(); }
        if (_draggingSliderId == id && Event.current.type == EventType.MouseDrag)
        { value = DragValue(trackX, trackW, min, max, whole); Event.current.Use(); }
        return value;
    }

    /// <summary>一行颜色选择器：色块 + 名称 + R/G/B 三滑条。</summary>
    private static bool ColorPickerRow(ref float sy, float vw, TrainerTheme.ColorItem item, int baseId)
    {
        const float RowH = 46f;
        Color cur = item.Get(ThemeManager.Current);
        float r0 = cur.r * 255f, g0 = cur.g * 255f, b0 = cur.b * 255f;

        // 色块
        Color prev = GUI.color;
        GUI.color = cur;
        GUI.Box(new Rect(6f, sy + 3f, 40f, RowH - 6f), GUIContent.none, _btnStyle);
        GUI.color = prev;
        // 名称
        string name = Loc.IsChinese ? item.DisplayZh : item.DisplayEn;
        GUI.Label(new Rect(52f, sy, 120f, RowH), name, _sectionStyle);

        float x0 = 182f;
        float sw = (vw - x0 - 8f) / 3f;
        float r = MiniSlider(new Rect(x0, sy, sw, RowH), r0, 0f, 255f, true, baseId);
        float g = MiniSlider(new Rect(x0 + sw + 4f, sy, sw, RowH), g0, 0f, 255f, true, baseId + 1);
        float b = MiniSlider(new Rect(x0 + (sw + 4f) * 2f, sy, sw, RowH), b0, 0f, 255f, true, baseId + 2);

        bool changed = Mathf.Abs(r - r0) > 0.5f || Mathf.Abs(g - g0) > 0.5f || Mathf.Abs(b - b0) > 0.5f;
        if (changed)
        {
            item.Set(ThemeManager.Current, new Color(Mathf.Clamp01(r / 255f), Mathf.Clamp01(g / 255f), Mathf.Clamp01(b / 255f), 1f));
            InvalidateStyles();
        }
        sy += RowH + 4f;
        return changed;
    }

    private static string InputField(Rect rect, string placeholder, string text, int id)
    {
        bool focused = _focusedInputId == id;
        GUI.Box(rect, GUIContent.none, _dyn(focused ? _inputBorder : _inputBg));
        if (focused && Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.Backspace && text.Length > 0) { text = text.Substring(0, text.Length - 1); Event.current.Use(); }
            else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) { _focusedInputId = -1; Event.current.Use(); }
        }
        if (focused && Event.current.type == EventType.KeyDown && Event.current.character != 0 && !char.IsControl(Event.current.character))
        { text += Event.current.character; Event.current.Use(); }

        string display = text;
        if (focused && _cursorVisible) display += "|";
        Color prev = GUI.color;
        if (string.IsNullOrEmpty(text) && !focused) { GUI.color = TextMuted; GUI.Label(new Rect(rect.x + 8f, rect.y, rect.width - 16f, rect.height), placeholder, _inputTextStyle); }
        else GUI.Label(new Rect(rect.x + 8f, rect.y, rect.width - 16f, rect.height), display, _inputTextStyle);
        GUI.color = prev;

        if (IsClick(rect)) { _focusedInputId = id; _cursorBlinkTime = 0f; _cursorVisible = true; }
        if (focused && Event.current.type == EventType.MouseDown && !rect.Contains(Event.current.mousePosition) && _draggingSliderId < 0)
            _focusedInputId = -1;
        return text;
    }

    private static bool IsClick(Rect r)
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && r.Contains(Event.current.mousePosition))
        { Event.current.Use(); return true; }
        return false;
    }

    private static GUIStyle _dyn(Texture2D t) { var s = new GUIStyle(); s.normal.background = t; return s; }
    private static Texture2D MakeTex(Color c) { var t = new Texture2D(1, 1); t.SetPixel(0, 0, c); t.Apply(); return t; }
    private static Texture2D MakeBorderTex(Color c)
    {
        var t = new Texture2D(2, 2);
        for (int y = 0; y < 2; y++) for (int x = 0; x < 2; x++)
            t.SetPixel(x, y, (x == 0 || y == 0 || x == 1 || y == 1) ? c : Color.clear);
        t.filterMode = FilterMode.Bilinear;
        t.Apply(); return t;
    }

    // ================= 窗口 =================

    private static void DrawWindow(float cx, float cy)
    {
        GUI.Box(new Rect(cx, cy, WinW, WinH), GUIContent.none, _winStyle);
        GUI.Box(new Rect(cx, cy, WinW, WinH), GUIContent.none, _winBorderStyle);
    }

    private static void DrawTitleBar(float cx, float cy)
    {
        GUI.Label(new Rect(cx + 16f, cy + 6f, 460f, TitleBarH), Loc.T("title") + "  v1.0.2", _titleStyle);
    }

    private static readonly string[] TabKeys = { "tab_money", "tab_player", "tab_vehicles", "tab_business", "tab_gameplay", "tab_staff", "tab_rivals", "tab_settings" };
    private static readonly string[] TabLabels = new string[TabCount];

    private static void DrawTabs(float cx, float cy)
    {
        float tw = (WinW - 20f) / TabCount;
        for (int i = 0; i < TabCount; i++)
        {
            if (TabLabels[i] == null) TabLabels[i] = Loc.T(TabKeys[i]);
            float tx = cx + 10f + i * tw;
            Rect r = new Rect(tx, cy + TitleBarH + 4f, tw, TabH);
            bool hover = Event.current.type == EventType.Repaint && r.Contains(Event.current.mousePosition);
            if (i == _activeTab) { GUI.Box(r, GUIContent.none, _tabActiveStyle); GUI.Label(r, TabLabels[i], _tabActiveStyle); }
            else if (hover) { GUI.Box(r, GUIContent.none, _tabHoverStyle); GUI.Label(r, TabLabels[i], _tabHoverStyle); }
            else { GUI.Box(r, GUIContent.none, _tabStyle); GUI.Label(r, TabLabels[i], _tabStyle); }
            if (IsClick(r)) { SwitchTab(i); _focusedInputId = -1; }
        }
    }

    private static void SwitchTab(int i)
    {
        if (i == _activeTab) return;
        _activeTab = i; _draggingSliderId = -1; _focusedInputId = -1;
        // 滚动位置由 OnGUI 从 _tabScrollPos 恢复
    }

    /// <summary>标题栏拖拽：按下开始，拖动跟手，松开保存位置。</summary>
    private static void HandleDrag(float cx, float cy)
    {
        Rect titleRect = new Rect(cx, cy, WinW, TitleBarH);
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && titleRect.Contains(Event.current.mousePosition))
        {
            _dragging = true;
            _dragOffset = Event.current.mousePosition - _winPos;
            Event.current.Use();
            return;
        }

        if (_dragging && Event.current.type == EventType.MouseDrag)
        {
            _winPos = Event.current.mousePosition - _dragOffset;
            ClampToScreen();
            Event.current.Use();
            return;
        }

        if (_dragging && Event.current.type == EventType.MouseUp)
        {
            _dragging = false;
            SaveWindowPosition();
            TrainerConfig.Save();
            Event.current.Use();
        }
    }

    private static void DrawScrollbar(float cx, float cy, float maxScroll, float contentBottom)
    {
        if (maxScroll <= 0f) return;
        float sbX = cx + WinW - 27f;
        float sbY = cy + ContentTop;
        GUI.Box(new Rect(sbX, sbY, 12f, ContentH), GUIContent.none, _dyn(_track));
        float thumbH = Mathf.Max(32f, ContentH * (ContentH / contentBottom));
        float thumbY = sbY + (_scrollPos.y / maxScroll) * (ContentH - thumbH);
        Color prev = GUI.color;
        GUI.color = new Color(0.6f, 0.62f, 0.7f, 0.6f);
        GUI.Box(new Rect(sbX, thumbY, 12f, thumbH), GUIContent.none, _dyn(_handle));
        GUI.color = prev;
    }

    // ================= Tab: Money =================

    private static void DrawMoneyTab(ref float sy, float vw)
    {
        SectionLabel(Loc.T("money_quickadd"), ref sy, vw);
        float bw = (vw - 24f) / 4f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 38), "$1K", AccentGreen, GreenHover)) AddMoney(1000f);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 38), "$5K", AccentGreen, GreenHover)) AddMoney(5000f);
        if (ClickableColorBtn(new Rect(14 + bw * 2, sy, bw, 38), "$10K", AccentGreen, GreenHover)) AddMoney(10000f);
        if (ClickableColorBtn(new Rect(18 + bw * 3, sy, bw, 38), "$50K", AccentGreen, GreenHover)) AddMoney(50000f);
        sy += 46f;
        bw = (vw - 16f) / 3f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 38), "$100K", AccentGreen, GreenHover)) AddMoney(100000f);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 38), "$500K", AccentGreen, GreenHover)) AddMoney(500000f);
        if (ClickableColorBtn(new Rect(14 + bw * 2, sy, bw, 38), "$1M", AccentOrange, OrangeHover)) AddMoney(1000000f);
        sy += 46f;

        SectionLabel(Loc.T("money_custom"), ref sy, vw);
        float iw = vw - 12f - 130f;
        _inputTexts[0] = InputField(new Rect(6, sy, iw, 38), Loc.T("money_enteramount"), _inputTexts[0] ?? "", 0);
        if (ClickableColorBtn(new Rect(10 + iw, sy, 62, 38), Loc.T("add"), AccentGreen, GreenHover))
        { float amt; if (float.TryParse(_inputTexts[0], out amt)) { MoneyModule.AddMoney(amt); _inputTexts[0] = ""; } }
        if (ClickableColorBtn(new Rect(76 + iw, sy, 62, 38), Loc.T("set"), AccentOrange, OrangeHover))
        { float amt; if (float.TryParse(_inputTexts[0], out amt)) { MoneyModule.SetMoney(amt); ToastNotification.Show(Loc.T("money_setto") + " $" + amt.ToString("N0")); _inputTexts[0] = ""; } }
        sy += 46f;

        SectionLabel(Loc.T("money_economy"), ref sy, vw);
        MoneyModule.ApplyTaxPercentage(AsInt(CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("money_tax"), MoneyModule.TaxPercentage, 0f, 100f, true, IdMoneyTax)));
        sy += 42f;
        MoneyModule.MarketPriceMultiplier = CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("money_pricemult"), MoneyModule.MarketPriceMultiplier, 0.1f, 5f, false, IdMoneyPrice);
        MoneyModule.ApplyMarketPriceMultiplier(MoneyModule.MarketPriceMultiplier);
        sy += 42f;
        MoneyModule.ExportMultiplier = CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("money_exportmult"), MoneyModule.ExportMultiplier, 0.1f, 10f, false, IdMoneyExport);
        MoneyModule.ApplyExportMultiplier(MoneyModule.ExportMultiplier);
        sy += 46f;
        sy += 6f;
    }

    // ================= Tab: Player =================

    private static void DrawPlayerTab(ref float sy, float vw)
    {
        SectionLabel(Loc.T("player_needs"), ref sy, vw);
        if (ClickableColorBtn(new Rect(6, sy, vw - 12f, 40), Loc.T("player_fillall"), AccentGreen, GreenHover)) { PlayerStatsModule.FillAllNeeds(); ToastNotification.Show(Loc.T("player_needfilled")); }
        sy += 48f;

        SectionLabel(Loc.T("player_energy"), ref sy, vw);
        float energy = PlayerStatsModule.CurrentEnergy;
        float newEnergy = CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("player_level"), energy, 0f, 100f, true, IdEnergy);
        if (Math.Abs(newEnergy - energy) > 0.01f) PlayerStatsModule.SetEnergy(newEnergy);
        sy += 42f;
        float bw = (vw - 24f) / 4f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 38), "25", AccentBlue, BlueHover)) PlayerStatsModule.SetEnergy(25f);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 38), "50", AccentBlue, BlueHover)) PlayerStatsModule.SetEnergy(50f);
        if (ClickableColorBtn(new Rect(14 + bw * 2, sy, bw, 38), "75", AccentBlue, BlueHover)) PlayerStatsModule.SetEnergy(75f);
        if (ClickableColorBtn(new Rect(18 + bw * 3, sy, bw, 38), "100", AccentGreen, GreenHover)) PlayerStatsModule.SetEnergy(100f);
        sy += 46f;

        SectionLabel(Loc.T("player_happiness"), ref sy, vw);
        bw = (vw - 32f) / 4f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 38), "-25", AccentRed, RedHover)) PlayerStatsModule.ChangeHappiness(-25);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 38), "-10", AccentRed, RedHover)) PlayerStatsModule.ChangeHappiness(-10);
        if (ClickableColorBtn(new Rect(14 + bw * 2, sy, bw, 38), "+10", AccentGreen, GreenHover)) PlayerStatsModule.ChangeHappiness(10);
        if (ClickableColorBtn(new Rect(18 + bw * 3, sy, bw, 38), "+25", AccentGreen, GreenHover)) PlayerStatsModule.ChangeHappiness(25);
        sy += 46f;

        SectionLabel(Loc.T("player_hunger"), ref sy, vw);
        bw = (vw - 32f) / 4f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 38), "-25", AccentRed, RedHover)) PlayerStatsModule.ChangeHunger(-25);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 38), "-10", AccentRed, RedHover)) PlayerStatsModule.ChangeHunger(-10);
        if (ClickableColorBtn(new Rect(14 + bw * 2, sy, bw, 38), "+10", AccentGreen, GreenHover)) PlayerStatsModule.ChangeHunger(10);
        if (ClickableColorBtn(new Rect(18 + bw * 3, sy, bw, 38), "+25", AccentGreen, GreenHover)) PlayerStatsModule.ChangeHunger(25);
        sy += 46f;

        SectionLabel(Loc.T("player_speed"), ref sy, vw);
        bw = (vw - 24f) / 4f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 38), Loc.T("player_speed_walk"), BtnNeutral, BtnNeutralHover)) PlayerStatsModule.SetPlayerSpeed(0);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 38), Loc.T("player_speed_jog"), AccentBlue, BlueHover)) PlayerStatsModule.SetPlayerSpeed(1);
        if (ClickableColorBtn(new Rect(14 + bw * 2, sy, bw, 38), Loc.T("player_speed_run"), AccentOrange, OrangeHover)) PlayerStatsModule.SetPlayerSpeed(2);
        if (ClickableColorBtn(new Rect(18 + bw * 3, sy, bw, 38), Loc.T("player_speed_scooter"), AccentGreen, GreenHover)) PlayerStatsModule.SetPlayerSpeed(3);
        sy += 46f;

        SectionLabel(Loc.T("player_toggles"), ref sy, vw);
        bw = (vw - 14f) / 2f;
        TrainerConfig.DisableEnergy.Value = ToggleBtn(Loc.T("player_energydecay"), TrainerConfig.DisableEnergy.Value, 6, sy, bw);
        TrainerConfig.DisableHappiness.Value = ToggleBtn(Loc.T("player_happydecay"), TrainerConfig.DisableHappiness.Value, 10 + bw, sy, bw);
        sy += 46f;
        TrainerConfig.DisableHunger.Value = ToggleBtn(Loc.T("player_hungerdecay"), TrainerConfig.DisableHunger.Value, 6, sy, bw);
        TrainerConfig.DisableAging.Value = ToggleBtn(Loc.T("player_aging"), TrainerConfig.DisableAging.Value, 10 + bw, sy, bw);
        sy += 46f;

        SectionLabel(Loc.T("player_age"), ref sy, vw);
        bw = (vw - 32f) / 4f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 38), "-5 " + Loc.T("player_years"), AccentRed, RedHover)) PlayerStatsModule.ChangeAge(-5f);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 38), "-1 " + Loc.T("player_years"), AccentRed, RedHover)) PlayerStatsModule.ChangeAge(-1f);
        if (ClickableColorBtn(new Rect(14 + bw * 2, sy, bw, 38), "+1 " + Loc.T("player_years"), AccentBlue, BlueHover)) PlayerStatsModule.ChangeAge(1f);
        if (ClickableColorBtn(new Rect(18 + bw * 3, sy, bw, 38), "+5 " + Loc.T("player_years"), AccentBlue, BlueHover)) PlayerStatsModule.ChangeAge(5f);
        sy += 46f;

        if (ClickableColorBtn(new Rect(6, sy, vw - 12f, 40), Loc.T("player_completegoals"), AccentGreen, GreenHover)) PlayerStatsModule.CompletePersonalGoals();
        sy += 48f;
        sy += 6f;
    }

    // ================= Tab: Vehicles =================

    private static void DrawVehicleTab(ref float sy, float vw)
    {
        SectionLabel(Loc.T("veh_toggles"), ref sy, vw);
        float bw = (vw - 14f) / 2f;
        TrainerConfig.DisableVehicleDamage.Value = ToggleBtn(Loc.T("veh_damage"), TrainerConfig.DisableVehicleDamage.Value, 6, sy, bw);
        TrainerConfig.DisableVehicleFuel.Value = ToggleBtn(Loc.T("veh_fuel"), TrainerConfig.DisableVehicleFuel.Value, 10 + bw, sy, bw);
        sy += 46f;

        SectionLabel(Loc.T("veh_actions"), ref sy, vw);
        bw = (vw - 14f) / 2f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 40), Loc.T("veh_repair"), AccentGreen, GreenHover)) VehicleModule.RepairVehicle();
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 40), Loc.T("veh_refuel"), AccentBlue, BlueHover)) VehicleModule.RefuelVehicle();
        sy += 48f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 40), Loc.T("veh_clean"), BtnNeutral, BtnNeutralHover)) VehicleModule.CleanVehicle();
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 40), Loc.T("veh_cleartickets"), AccentOrange, OrangeHover)) VehicleModule.ClearParkingTickets();
        sy += 48f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 40), Loc.T("veh_towgas"), AccentBlue, BlueHover)) VehicleModule.TowToGasStation();
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 40), Loc.T("veh_towrepair"), AccentOrange, OrangeHover)) VehicleModule.TowToAutoRepair();
        sy += 48f;
        sy += 6f;
    }

    // ================= Tab: Business =================

    private static void DrawBusinessTab(ref float sy, float vw)
    {
        SectionLabel(Loc.T("bus_satisfaction"), ref sy, vw);
        if (ClickableColorBtn(new Rect(6, sy, vw - 12f, 40), Loc.T("bus_maxall"), AccentGreen, GreenHover)) BusinessModule.MaxAllSatisfaction();
        sy += 48f;

        SectionLabel(Loc.T("bus_unlocks"), ref sy, vw);
        float bw = (vw - 14f) / 2f;
        TrainerConfig.AllCoursesUnlocked.Value = ToggleBtn(Loc.T("bus_courses"), TrainerConfig.AllCoursesUnlocked.Value, 6, sy, bw);
        TrainerConfig.AllContactsUnlocked.Value = ToggleBtn(Loc.T("bus_contacts"), TrainerConfig.AllContactsUnlocked.Value, 10 + bw, sy, bw);
        sy += 46f;
        TrainerConfig.DisableWholesaleImportLimits.Value = ToggleBtn(Loc.T("bus_importlimits"), TrainerConfig.DisableWholesaleImportLimits.Value, 6, sy, bw);
        TrainerConfig.AllProductsFromImporters.Value = ToggleBtn(Loc.T("bus_importproducts"), TrainerConfig.AllProductsFromImporters.Value, 10 + bw, sy, bw);
        sy += 46f;

        SectionLabel(Loc.T("bus_multipliers"), ref sy, vw);
        BusinessModule.ApplyCustomerPromotionMultiplier(CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("bus_promotion"), BusinessModule.CustomerPromotionMultiplier, 0.1f, 10f, false, IdBizPromo));
        sy += 42f;
        BusinessModule.ApplyEmployeeSalaryMultiplier(CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("bus_salary"), BusinessModule.EmployeeSalaryMultiplier, 0f, 5f, false, IdBizSalary));
        sy += 42f;
        BusinessModule.ApplyBankInterestRate(CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("bus_interest"), BusinessModule.BankInterestRate, 0f, 5f, false, IdBizInterest));
        sy += 42f;
        BusinessModule.ApplyRivalsDifficultyMultiplier(CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("bus_rivals"), BusinessModule.RivalsDifficultyMultiplier, 0f, 5f, false, IdBizRivals));
        sy += 42f;
        BusinessModule.ApplyWholesaleUrgentFeeMultiplier(CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("bus_wholesalefee"), BusinessModule.WholesaleUrgentFeeMultiplier, 0f, 5f, false, IdBizWholesale));
        sy += 42f;
        BusinessModule.ApplyImporterUrgentFeeMultiplier(CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("bus_importerfee"), BusinessModule.ImporterUrgentFeeMultiplier, 0f, 5f, false, IdBizImporter));
        sy += 46f;
        sy += 6f;
    }

    // ================= Tab: Gameplay =================

    private static void DrawGameplayTab(ref float sy, float vw)
    {
        SectionLabel(Loc.T("game_speed"), ref sy, vw);
        GameplayModule.SetGameSpeed(CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("game_speed"), GameplayModule.GameSpeed, 0f, 10f, false, IdGameSpeed));
        sy += 44f;
        float bw = (vw - 36f) / 5f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 38), Loc.T("game_pause"), AccentRed, RedHover)) GameplayModule.SetGameSpeed(0f);
        if (ClickableColorBtn(new Rect(12 + bw, sy, bw, 38), "1x", BtnNeutral, BtnNeutralHover)) GameplayModule.SetGameSpeed(1f);
        if (ClickableColorBtn(new Rect(18 + bw * 2, sy, bw, 38), "2x", AccentBlue, BlueHover)) GameplayModule.SetGameSpeed(2f);
        if (ClickableColorBtn(new Rect(24 + bw * 3, sy, bw, 38), "5x", AccentOrange, OrangeHover)) GameplayModule.SetGameSpeed(5f);
        if (ClickableColorBtn(new Rect(30 + bw * 4, sy, bw, 38), "10x", AccentGreen, GreenHover)) GameplayModule.SetGameSpeed(10f);
        sy += 46f;

        SectionLabel(Loc.T("game_time"), ref sy, vw);
        if (ClickableColorBtn(new Rect(6, sy, vw - 12f, 40), Loc.T("game_skipnextday"), AccentBlue, BlueHover)) { GameplayModule.SkipToNextDay(); ToastNotification.Show(Loc.T("toast_skipped")); }
        sy += 48f;
        bw = (vw - 24f) / 4f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 40), "6:00", AccentOrange, OrangeHover)) GameplayModule.SetTimeOfDay(6, 0);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 40), "12:00", AccentBlue, BlueHover)) GameplayModule.SetTimeOfDay(12, 0);
        if (ClickableColorBtn(new Rect(14 + bw * 2, sy, bw, 40), "18:00", AccentOrange, OrangeHover)) GameplayModule.SetTimeOfDay(18, 0);
        if (ClickableColorBtn(new Rect(18 + bw * 3, sy, bw, 40), "22:00", AccentBlue, BlueHover)) GameplayModule.SetTimeOfDay(22, 0);
        sy += 48f;

        SectionLabel(Loc.T("game_settime"), ref sy, vw);
        float iw1 = (vw - 28f) / 3f;
        _inputTexts[1] = InputField(new Rect(6, sy, iw1, 40), Loc.T("game_hour"), _inputTexts[1] ?? "", 1);
        _inputTexts[2] = InputField(new Rect(10 + iw1, sy, iw1, 40), Loc.T("game_min"), _inputTexts[2] ?? "", 2);
        if (ClickableColorBtn(new Rect(14 + iw1 * 2, sy, iw1, 40), Loc.T("set"), AccentBlue, BlueHover))
        {
            int h, m;
            if (int.TryParse(_inputTexts[1], out h) && int.TryParse(_inputTexts[2], out m) && h >= 0 && h <= 23 && m >= 0 && m <= 59)
            { GameplayModule.SetTimeOfDay(h, m); _inputTexts[1] = ""; _inputTexts[2] = ""; }
        }
        sy += 48f;

        SectionLabel(Loc.T("game_toggles"), ref sy, vw);
        bw = (vw - 14f) / 2f;
        TrainerConfig.DisableTraffic.Value = ToggleBtn(Loc.T("game_traffic"), TrainerConfig.DisableTraffic.Value, 6, sy, bw);
        TrainerConfig.Invincibility.Value = ToggleBtn(Loc.T("game_invincible"), TrainerConfig.Invincibility.Value, 10 + bw, sy, bw);
        sy += 46f;

        SectionLabel(Loc.T("game_quests"), ref sy, vw);
        if (ClickableColorBtn(new Rect(6, sy, vw - 12f, 40), Loc.T("game_completequest"), AccentGreen, GreenHover)) GameplayModule.CompleteQuest();
        sy += 48f;
        if (ClickableColorBtn(new Rect(6, sy, vw - 12f, 40), Loc.T("game_completeobj"), AccentBlue, BlueHover)) GameplayModule.CompleteObjective();
        sy += 48f;
        if (ClickableColorBtn(new Rect(6, sy, vw - 12f, 40), Loc.T("game_unlockcontacts"), AccentBlue, BlueHover)) GameplayModule.UnlockAllContacts();
        sy += 48f;

        SectionLabel(Loc.T("game_imports"), ref sy, vw);
        bw = (vw - 14f) / 2f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 40), Loc.T("game_deliverpaid"), AccentBlue, BlueHover)) GameplayModule.DeliverAllImportsPaid();
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 40), Loc.T("game_deliverfree"), AccentGreen, GreenHover)) GameplayModule.DeliverAllImportsFree();
        sy += 48f;
        if (ClickableColorBtn(new Rect(6, sy, vw - 12f, 40), Loc.T("game_save"), AccentGreen, GreenHover)) { bool ok = GameplayModule.SaveGame(); ToastNotification.Show(ok ? Loc.T("toast_gamesaved") : Loc.T("error"), ok); }
        sy += 48f;
        sy += 6f;
    }

    // ================= Tab: Staff =================

    private static void DrawStaffTab(ref float sy, float vw)
    {
        SectionLabel(Loc.T("staff_bulk"), ref sy, vw);
        if (ClickableColorBtn(new Rect(6, sy, vw - 12f, 40), Loc.T("staff_maxsatisfaction"), AccentGreen, GreenHover)) EmployeeModule.MaxAllSatisfaction();
        sy += 48f;

        SectionLabel(Loc.T("staff_salarymult"), ref sy, vw);
        EmployeeModule.ApplySalaryMultiplier(CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("bus_salary"), EmployeeModule.SalaryMultiplier, 0f, 5f, false, IdStaffSalary));
        sy += 42f;
        float bw = (vw - 32f) / 4f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 38), Loc.T("staff_free"), AccentGreen, GreenHover)) EmployeeModule.ApplySalaryMultiplier(0f);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 38), "0.5x", AccentBlue, BlueHover)) EmployeeModule.ApplySalaryMultiplier(0.5f);
        if (ClickableColorBtn(new Rect(14 + bw * 2, sy, bw, 38), "1x", BtnNeutral, BtnNeutralHover)) EmployeeModule.ApplySalaryMultiplier(1f);
        if (ClickableColorBtn(new Rect(18 + bw * 3, sy, bw, 38), "2x", AccentOrange, OrangeHover)) EmployeeModule.ApplySalaryMultiplier(2f);
        sy += 46f;

        SectionLabel(Loc.T("staff_setwages"), ref sy, vw);
        float iw = vw - 12f - 130f;
        _inputTexts[3] = InputField(new Rect(6, sy, iw, 40), Loc.T("staff_enterwage"), _inputTexts[3] ?? "", 3);
        if (ClickableColorBtn(new Rect(10 + iw, sy, 120, 40), Loc.T("staff_setallwages"), AccentBlue, BlueHover))
        { float w; if (float.TryParse(_inputTexts[3], out w)) { EmployeeModule.SetAllWages(w); _inputTexts[3] = ""; } }
        sy += 48f;

        SectionLabel(Loc.T("staff_candidates"), ref sy, vw);
        float iw2 = vw - 12f - 150f;
        _inputTexts[4] = InputField(new Rect(6, sy, iw2, 40), Loc.T("staff_skilllevel"), _inputTexts[4] ?? "", 4);
        if (ClickableColorBtn(new Rect(10 + iw2, sy, 140, 40), Loc.T("set"), AccentBlue, BlueHover))
        { int lv; if (int.TryParse(_inputTexts[4], out lv) && lv >= 1 && lv <= 100) { EmployeeModule.CandidateSkillLevel = lv; _inputTexts[4] = ""; } }
        sy += 48f;

        bw = (vw - 14f) / 2f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 40), Loc.T("staff_custservice"), AccentBlue, BlueHover)) EmployeeModule.GenerateCandidate(0, EmployeeModule.CandidateSkillLevel);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 40), Loc.T("staff_cleaning"), AccentBlue, BlueHover)) EmployeeModule.GenerateCandidate(1, EmployeeModule.CandidateSkillLevel);
        sy += 48f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 40), Loc.T("staff_lawyer"), AccentBlue, BlueHover)) EmployeeModule.GenerateCandidate(2, EmployeeModule.CandidateSkillLevel);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 40), Loc.T("staff_purchasing"), AccentBlue, BlueHover)) EmployeeModule.GenerateCandidate(3, EmployeeModule.CandidateSkillLevel);
        sy += 48f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 40), Loc.T("staff_logistics"), AccentBlue, BlueHover)) EmployeeModule.GenerateCandidate(4, EmployeeModule.CandidateSkillLevel);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 40), Loc.T("staff_delivery"), AccentBlue, BlueHover)) EmployeeModule.GenerateCandidate(5, EmployeeModule.CandidateSkillLevel);
        sy += 48f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 40), Loc.T("staff_programmer"), AccentBlue, BlueHover)) EmployeeModule.GenerateCandidate(6, EmployeeModule.CandidateSkillLevel);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 40), Loc.T("staff_hr"), AccentBlue, BlueHover)) EmployeeModule.GenerateCandidate(7, EmployeeModule.CandidateSkillLevel);
        sy += 48f;
        sy += 6f;
    }

    // ================= Tab: Rivals =================

    private static void DrawRivalsTab(ref float sy, float vw)
    {
        SectionLabel(Loc.T("rival_actions"), ref sy, vw);
        float bw = (vw - 14f) / 2f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 40), Loc.T("rival_refresh"), AccentBlue, BlueHover)) RivalsModule.RefreshRivals();
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 40), Loc.T("rival_defeatall"), AccentRed, RedHover)) { int n = RivalsModule.DefeatAllRivals(); ToastNotification.Show(Loc.T("rival_defeatall") + " (" + n + ")"); }
        sy += 48f;

        SectionLabel(Loc.T("rival_difficulty"), ref sy, vw);
        bw = (vw - 32f) / 4f;
        if (ClickableColorBtn(new Rect(6, sy, bw, 40), Loc.T("rival_easy"), AccentGreen, GreenHover)) BusinessModule.ApplyRivalsDifficultyMultiplier(0.5f);
        if (ClickableColorBtn(new Rect(10 + bw, sy, bw, 40), Loc.T("rival_normal"), AccentBlue, BlueHover)) BusinessModule.ApplyRivalsDifficultyMultiplier(1f);
        if (ClickableColorBtn(new Rect(14 + bw * 2, sy, bw, 40), Loc.T("rival_hard"), AccentOrange, OrangeHover)) BusinessModule.ApplyRivalsDifficultyMultiplier(2f);
        if (ClickableColorBtn(new Rect(18 + bw * 3, sy, bw, 40), Loc.T("rival_brutal"), AccentRed, RedHover)) BusinessModule.ApplyRivalsDifficultyMultiplier(5f);
        sy += 48f;
        sy += 6f;
    }

    // ================= Tab: Settings =================

    private static void DrawSettingsTab(ref float sy, float vw)
    {
        SectionLabel(Loc.T("set_settings"), ref sy, vw);
        float halfW = (vw - 18f) / 2f;
        if (ClickableColorBtn(new Rect(6, sy, halfW, 42), Loc.T("set_saveall"), AccentGreen, GreenHover)) { TrainerConfig.Save(); ToastNotification.Show(Loc.T("toast_saved")); }
        if (ClickableColorBtn(new Rect(10 + halfW, sy, halfW, 42), Loc.T("set_loadall"), AccentBlue, BlueHover)) { TrainerConfig.Load(); RefreshTabs(); ToastNotification.Show(Loc.T("toast_loaded")); }
        sy += 50f;

        SectionLabel(Loc.T("set_language"), ref sy, vw);
        float langW = (vw - 18f) / 2f;
        if (ClickableColorBtn(new Rect(6, sy, langW, 42), Loc.T("set_lang_zh"), Loc.IsChinese ? AccentGreen : BtnNeutral, Loc.IsChinese ? GreenHover : BtnNeutralHover))
        { if (!Loc.IsChinese) { Loc.Current = Loc.LangZh; TrainerConfig.Language.Value = Loc.LangZh; RefreshTabs(); } }
        if (ClickableColorBtn(new Rect(10 + langW, sy, langW, 42), Loc.T("set_lang_en"), Loc.IsChinese ? BtnNeutral : AccentGreen, Loc.IsChinese ? BtnNeutralHover : GreenHover))
        { if (Loc.IsChinese) { Loc.Current = Loc.LangEn; TrainerConfig.Language.Value = Loc.LangEn; RefreshTabs(); } }
        sy += 50f;

        // —— 外观定制：主题(蓝科技) / 颜色自定义 / 透明度 / 缩放 ——
        SectionLabel(Loc.T("set_appearance"), ref sy, vw);
        // 主题名（简洁）
        string themeName = Loc.IsChinese ? ThemeManager.ThemeNameZh : ThemeManager.ThemeNameEn;
        GUI.Label(new Rect(6f, sy, 200f, 30f), Loc.T("set_theme") + ": " + themeName, _titleStyle);
        sy += 38f;

        // 恢复默认配色
        if (ClickableColorBtn(new Rect(6, sy, vw - 12f, 40), Loc.T("set_reset_colors"), BtnNeutral, BtnNeutralHover))
        { TrainerConfig.ResetColors(); InvalidateStyles(); ToastNotification.Show(Loc.T("toast_color_reset")); }
        sy += 48f;

        // 各控件颜色自定义（R/G/B）
        int cid = 70;
        bool anyColorChanged = false;
        foreach (var item in TrainerTheme.CustomColors)
        {
            if (ColorPickerRow(ref sy, vw, item, cid)) anyColorChanged = true;
            cid += 3;
        }
        // 松手时统一保存颜色变更（拖动中仅内存预览）
        if (anyColorChanged) _colorDirty = true;
        if (_colorDirty && Event.current.type == EventType.MouseUp) { SaveAllColors(); _colorDirty = false; }

        SectionLabel(Loc.T("set_layout"), ref sy, vw);
        float newOp = CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("set_opacity"), TrainerConfig.PanelOpacity.Value, 0.3f, 1f, false, 60);
        if (Math.Abs(newOp - TrainerConfig.PanelOpacity.Value) > 0.001f) { TrainerConfig.PanelOpacity.Value = newOp; }
        sy += 42f;
        float newScale = CustomSlider(new Rect(6, sy, vw - 12f, 34), Loc.T("set_scale"), TrainerConfig.PanelScale.Value, 0.6f, 1.8f, false, 61);
        if (Math.Abs(newScale - TrainerConfig.PanelScale.Value) > 0.001f) { TrainerConfig.PanelScale.Value = newScale; InvalidateStyles(); TrainerConfig.Save(); }
        sy += 42f;
        if (ClickableColorBtn(new Rect(6, sy, vw - 12f, 40), Loc.T("set_reset_pos"), BtnNeutral, BtnNeutralHover))
        { _winPos = new Vector2((Screen.width / _uiScale - WinW) * 0.5f, (Screen.height / _uiScale - WinH) * 0.5f); SaveWindowPosition(); TrainerConfig.Save(); }
        sy += 48f;

        SectionLabel(Loc.T("set_about"), ref sy, vw);
        float infoX = 12f;
        GUI.Label(new Rect(infoX, sy, vw - 24f, 26f), Loc.T("title") + "  v1.0.2", _titleStyle);
        sy += 30f;
        Color saveC = GUI.color;
        GUI.color = TextMuted;
        GUI.Label(new Rect(infoX, sy, vw - 24f, 26f), Loc.T("set_presshint"), _sectionStyle);
        GUI.color = saveC;
        sy += 30f;
        GUI.Label(new Rect(infoX, sy, vw - 24f, 26f), "Author: Mizuof", _sectionStyle);
        sy += 30f;

        if (ClickableColorBtn(new Rect(6, sy, vw - 12f, 40), Loc.T("set_close"), AccentRed, RedHover)) { Close(); }
        sy += 48f;
        sy += 6f;
    }

    // ================= Tool =================

    private static int AsInt(float f) => (int)f;

    private static void AddMoney(float amt)
    {
        MoneyModule.AddMoney(amt);
        ToastNotification.Show(Loc.T("money_added") + " $" + amt.ToString("N0"));
    }

    private static void RefreshTabs()
    {
        for (int i = 0; i < TabCount; i++) TabLabels[i] = null;
    }

    /// <summary>将所有自定义颜色写入配置并保存（拖动 RGB 松手时调用）。</summary>
    private static void SaveAllColors()
    {
        foreach (var item in TrainerTheme.CustomColors)
        {
            int hex = TrainerTheme.ToHex(item.Get(ThemeManager.Current));
            TrainerConfig.SetColorHex(item.Key, hex);
        }
        TrainerConfig.Save();
    }
}
