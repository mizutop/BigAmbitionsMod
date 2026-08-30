using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigAmbitionsTrainer.UI;

/// <summary>
/// 面板主题：管理全部颜色与字号（默认「蓝科技」深蓝配色）。
/// 支持对各个控件区域颜色做细粒度自定义。
/// </summary>
public sealed class TrainerTheme
{
    // —— 基础色 ——
    public Color WindowBg;
    public Color WindowBorder;
    public Color TabBg;
    public Color TabHoverBg;
    public Color TabText;
    public Color CardBg;            // 输入框/滑块轨道/分区背景
    public Color BtnNeutral;
    public Color BtnNeutralHover;
    public Color BtnText;
    public Color InputBorder;       // 输入框聚焦边框

    // —— 强调色（语义）——
    public Color Primary;           // 主强调（激活 Tab、滑块填充、强调线）
    public Color Success;           // 成功/正向
    public Color Danger;            // 危险/负向
    public Color Warning;           // 警示
    public Color PrimaryHover;
    public Color SuccessHover;
    public Color DangerHover;
    public Color WarningHover;

    // —— 文字色 ——
    public Color TextLight;
    public Color TextMuted;
    public Color SectionText;
    public Color TitleText;
    public Color WhiteText = Color.white;

    // —— 字号 ——
    public int FontTitle = 17;
    public int FontSection = 15;
    public int FontButton = 15;
    public int FontTab = 15;
    public int FontSliderValue = 15;
    public int FontInput = 14;

    /// <summary>可自定义的控件颜色项（key / 显示名 / 默认值 / 字段访问器）。</summary>
    public sealed class ColorItem
    {
        public string Key;
        public string DisplayZh;
        public string DisplayEn;
        public int DefaultHex;
        public Func<TrainerTheme, Color> Get;
        public Action<TrainerTheme, Color> Set;

        public ColorItem(string key, string zh, string en, int hex,
            Func<TrainerTheme, Color> get, Action<TrainerTheme, Color> set)
        {
            Key = key; DisplayZh = zh; DisplayEn = en; DefaultHex = hex; Get = get; Set = set;
        }
    }

    /// <summary>全部可自定义颜色项（顺序即 UI 显示顺序）。</summary>
    public static ColorItem[] CustomColors => _customColors;
    private static readonly ColorItem[] _customColors = new ColorItem[]
    {
        new ColorItem("WindowBg", "窗口背景", "Window BG", 0x1a1d29, t => t.WindowBg, (t, v) => t.WindowBg = v),
        new ColorItem("WindowBorder", "窗口边框", "Border", 0x3a4156, t => t.WindowBorder, (t, v) => t.WindowBorder = v),
        new ColorItem("TabBg", "页签背景", "Tab BG", 0x22263a, t => t.TabBg, (t, v) => t.TabBg = v),
        new ColorItem("CardBg", "控件背景", "Control BG", 0x272c40, t => t.CardBg, (t, v) => t.CardBg = v),
        new ColorItem("BtnNeutral", "按钮背景", "Button BG", 0x39405a, t => t.BtnNeutral, (t, v) => t.BtnNeutral = v),
        new ColorItem("BtnText", "按钮文字", "Button Text", 0xedf1f9, t => t.BtnText, (t, v) => t.BtnText = v),
        new ColorItem("Primary", "主强调", "Primary", 0x4f8cff, t => t.Primary, (t, v) => t.Primary = v),
        new ColorItem("Success", "成功色", "Success", 0x2ee59d, t => t.Success, (t, v) => t.Success = v),
        new ColorItem("Danger", "危险色", "Danger", 0xff5c6c, t => t.Danger, (t, v) => t.Danger = v),
        new ColorItem("Warning", "警示色", "Warning", 0xffa63d, t => t.Warning, (t, v) => t.Warning = v),
        new ColorItem("TextLight", "主文字", "Text", 0xe8ecf4, t => t.TextLight, (t, v) => t.TextLight = v),
        new ColorItem("SectionText", "分区标题", "Section", 0xa6c1ff, t => t.SectionText, (t, v) => t.SectionText = v),
        new ColorItem("InputBorder", "输入框边框", "Input Border", 0x6a76a8, t => t.InputBorder, (t, v) => t.InputBorder = v),
    };

    /// <summary>默认「蓝科技」配色，并自动补齐派生色（hover / 边框 / muted 等）。</summary>
    public static TrainerTheme BlueTech()
    {
        var t = new TrainerTheme();
        t.WindowBg = Hex(0x1a1d29);
        t.WindowBorder = Hex(0x3a4156);
        t.TabBg = Hex(0x22263a);
        t.TabHoverBg = Hex(0x2d3450);
        t.TabText = Hex(0x9aa4c0);
        t.CardBg = Hex(0x272c40);
        t.BtnNeutral = Hex(0x39405a);
        t.BtnNeutralHover = Hex(0x4a5270);
        t.BtnText = Hex(0xedf1f9);
        t.InputBorder = Hex(0x6a76a8);
        t.Primary = Hex(0x4f8cff);
        t.Success = Hex(0x2ee59d);
        t.Danger = Hex(0xff5c6c);
        t.Warning = Hex(0xffa63d);
        t.PrimaryHover = Hex(0x77a6ff);
        t.SuccessHover = Hex(0x63f0b6);
        t.DangerHover = Hex(0xff7c89);
        t.WarningHover = Hex(0xffbc6e);
        t.TextLight = Hex(0xe8ecf4);
        t.TextMuted = Hex(0x8b93ad);
        t.SectionText = Hex(0xa6c1ff);
        t.TitleText = Hex(0x9db8ff);
        return t;
    }

    /// <summary>将 0xRRGGBB 转为带 alpha=1 的 Color。</summary>
    public static Color FromHex(int rgb)
    {
        float r = ((rgb >> 16) & 0xFF) / 255f;
        float g = ((rgb >> 8) & 0xFF) / 255f;
        float b = (rgb & 0xFF) / 255f;
        return new Color(r, g, b, 1f);
    }

    /// <summary>将 Color 转回 0xRRGGBB（alpha 忽略）。</summary>
    public static int ToHex(Color c)
    {
        int r = Mathf.RoundToInt(c.r * 255f);
        int g = Mathf.RoundToInt(c.g * 255f);
        int b = Mathf.RoundToInt(c.b * 255f);
        return (r << 16) | (g << 8) | b;
    }

    private static Color Hex(int rgb) => FromHex(rgb);
}

/// <summary>主题管理：持有当前「蓝科技」主题实例，并提供自定义颜色的加载/持久化。</summary>
public static class ThemeManager
{
    private static TrainerTheme _current = TrainerTheme.BlueTech();

    /// <summary>当前主题实例。</summary>
    public static TrainerTheme Current => _current;

    /// <summary>主题名称（简单）。</summary>
    public const string ThemeNameZh = "蓝科技";
    public const string ThemeNameEn = "BlueTech";

    /// <summary>复位为默认「蓝科技」配色。</summary>
    public static void ResetDefault()
    {
        _current = TrainerTheme.BlueTech();
    }

    /// <summary>从配置读取自定义颜色并覆盖当前主题（未设置的项保持默认）。</summary>
    public static void ApplyCustomColors(System.Func<string, int?> getHex)
    {
        foreach (var item in TrainerTheme.CustomColors)
        {
            int? hex = getHex(item.Key);
            if (hex.HasValue && hex.Value >= 0)
                item.Set(_current, TrainerTheme.FromHex(hex.Value));
        }
    }

    /// <summary>读取当前主题某控件颜色。</summary>
    public static Color GetColor(TrainerTheme.ColorItem item)
    {
        return item.Get(_current);
    }
}
