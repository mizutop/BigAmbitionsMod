using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BigAmbitionsTrainer.UI;

/// <summary>轻量 IMGUI Toast 提示，挂载到主 MelonMod 的 OnGUI。</summary>
public static class ToastNotification
{
    private class Toast
    {
        public string Message;
        public bool IsSuccess;
        public float SpawnTime;
    }

    private const float Width = 340f;
    private const float Height = 42f;
    private const float AccentBarWidth = 5f;
    private const int MaxToasts = 6;

    private static readonly List<Toast> _toasts = new List<Toast>();
    private static GUIStyle _bgStyle, _textStyle;
    private static Texture2D _bgTex, _textTex, _fillTex;
    private static bool _inited;

    public static void Show(string message, bool success = true)
    {
        _toasts.Add(new Toast { Message = message, IsSuccess = success, SpawnTime = Time.unscaledTime });
        while (_toasts.Count > MaxToasts) _toasts.RemoveAt(0);
    }

    public static void Draw()
    {
        if (_toasts.Count == 0) return;
        try
        {
            Ensure();
            float totalDuration = 3f;
            float fadeStart = 2f;
            float now = Time.unscaledTime;
            for (int i = _toasts.Count - 1; i >= 0; i--)
                if (now - _toasts[i].SpawnTime >= totalDuration) _toasts.RemoveAt(i);
            if (_toasts.Count == 0) return;

            float x = Screen.width - Width - 12f;
            float y = 56f;
            for (int i = 0; i < _toasts.Count; i++)
            {
                Toast t = _toasts[i];
                float age = now - t.SpawnTime;
                float a = 1f;
                if (age > fadeStart) a = 1f - (age - fadeStart) / (totalDuration - fadeStart);
                a = Mathf.Clamp01(a);

                Color prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, a);

                // 背景
                Color prevB = GUI.backgroundColor;
                GUI.color = new Color(0.1f, 0.11f, 0.13f, a);
                GUI.Box(new Rect(x, y, Width, Height), GUIContent.none, _bgStyle);
                // 强调条
                GUI.color = t.IsSuccess ? new Color(0.22f, 0.72f, 0.35f, a) : new Color(0.85f, 0.25f, 0.25f, a);
                GUI.Box(new Rect(x, y, AccentBarWidth, Height), GUIContent.none, _bgStyle);
                // 文本
                GUI.color = new Color(0.92f, 0.93f, 0.95f, a);
                GUI.Label(new Rect(x + 14f, y, Width - 20f, Height), t.Message, _textStyle);

                GUI.color = prev;
                GUI.backgroundColor = prevB;
                y += Height + 8f;
            }
        }
        catch { }
    }

    private static void Ensure()
    {
        if (_inited) return;
        _bgTex = new Texture2D(1, 1);
        _bgTex.SetPixel(0, 0, Color.white); _bgTex.Apply();
        _bgStyle = new GUIStyle();
        GUIStyleState bg = new GUIStyleState();
        bg.background = _bgTex;
        _bgStyle.normal = bg;

        _textTex = new Texture2D(1, 1);
        _textTex.SetPixel(0, 0, Color.white); _textTex.Apply();
        _textStyle = new GUIStyle();
        GUIStyleState ts = new GUIStyleState();
        ts.textColor = Color.white;
        _textStyle.normal = ts;
        _textStyle.fontSize = 14;
        _textStyle.alignment = TextAnchor.MiddleLeft;
        _textStyle.wordWrap = true;

        _fillTex = new Texture2D(1, 1);
        _fillTex.SetPixel(0, 0, Color.white); _fillTex.Apply();
        _inited = true;
    }

    public static void Cleanup()
    {
        _toasts.Clear();
        if (_bgTex != null) Object.DestroyImmediate(_bgTex);
        if (_textTex != null) Object.DestroyImmediate(_textTex);
        if (_fillTex != null) Object.DestroyImmediate(_fillTex);
        _bgTex = _textTex = _fillTex = null;
        _bgStyle = _textStyle = null;
        _inited = false;
    }
}
