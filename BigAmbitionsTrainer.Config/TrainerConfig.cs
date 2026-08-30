using MelonLoader;
using MelonLoader.Preferences;
using BigAmbitionsTrainer.UI;

namespace BigAmbitionsTrainer.Config;

/// <summary>
/// 修改器配置（MelonPreferences）。
/// 语言 / 全部开关 / 倍率统一管理，支持保存加载。
/// </summary>
public static class TrainerConfig
{
    private static MelonPreferences_Category _category;

    public static MelonPreferences_Entry<string> Language;
    public static MelonPreferences_Entry<bool> DisableEnergy;
    public static MelonPreferences_Entry<bool> DisableHappiness;
    public static MelonPreferences_Entry<bool> DisableAging;
    public static MelonPreferences_Entry<bool> DisableHunger;
    public static MelonPreferences_Entry<bool> DisableVehicleDamage;
    public static MelonPreferences_Entry<bool> DisableVehicleFuel;
    public static MelonPreferences_Entry<bool> AllCoursesUnlocked;
    public static MelonPreferences_Entry<bool> AllContactsUnlocked;
    public static MelonPreferences_Entry<bool> DisableWholesaleImportLimits;
    public static MelonPreferences_Entry<bool> AllProductsFromImporters;
    public static MelonPreferences_Entry<bool> DisableTraffic;
    public static MelonPreferences_Entry<bool> DisableTutorial;
    public static MelonPreferences_Entry<bool> Invincibility;
    public static MelonPreferences_Entry<float> GameSpeed;
    public static MelonPreferences_Entry<bool> PhoneIntegration;
    public static MelonPreferences_Entry<int> CandidateLevel;
    public static MelonPreferences_Entry<int> LastTab;
    public static MelonPreferences_Entry<float> WinPosX;
    public static MelonPreferences_Entry<float> WinPosY;
    public static MelonPreferences_Entry<float> PanelOpacity;
    public static MelonPreferences_Entry<float> PanelScale;

    // —— 自定义控件颜色（key -> hex(int)，-1 表示用默认）——
    private static readonly System.Collections.Generic.Dictionary<string, MelonPreferences_Entry<int>> _colorEntries
        = new System.Collections.Generic.Dictionary<string, MelonPreferences_Entry<int>>();

    private static void InitEntry<T>(out MelonPreferences_Entry<T> e, string id, T def, string display)
    {
        e = _category.CreateEntry<T>(id, def, display);
    }

    public static void Initialize()
    {
        _category = MelonPreferences.CreateCategory("ItzRealOzoneTrainer", "ItzRealOzone Trainer (Mono)");

        InitEntry(out Language, "Language", BigAmbitionsTrainer.L.Loc.LangZh, "Language / 语言");
        InitEntry(out DisableEnergy, "DisableEnergy", false, "Disable Energy Decay");
        InitEntry(out DisableHappiness, "DisableHappiness", false, "Disable Happiness Decay");
        InitEntry(out DisableAging, "DisableAging", false, "Disable Aging");
        InitEntry(out DisableHunger, "DisableHunger", false, "Disable Hunger Decay");
        InitEntry(out DisableVehicleDamage, "DisableVehicleDamage", false, "Disable Vehicle Damage");
        InitEntry(out DisableVehicleFuel, "DisableVehicleFuel", false, "Disable Vehicle Fuel");
        InitEntry(out AllCoursesUnlocked, "AllCoursesUnlocked", false, "Unlock All Courses");
        InitEntry(out AllContactsUnlocked, "AllContactsUnlocked", false, "Unlock All Contacts");
        InitEntry(out DisableWholesaleImportLimits, "DisableWholesaleImportLimits", false, "Disable Wholesale/Import Limits");
        InitEntry(out AllProductsFromImporters, "AllProductsFromImporters", false, "All Products From Importers");
        InitEntry(out DisableTraffic, "DisableTraffic", false, "Disable Traffic");
        InitEntry(out DisableTutorial, "DisableTutorial", false, "Disable Tutorial");
        InitEntry(out Invincibility, "Invincibility", false, "Invincibility");
        InitEntry(out GameSpeed, "GameSpeed", 1f, "Game Speed Multiplier");
        InitEntry(out PhoneIntegration, "PhoneIntegration", true, "Phone Menu Integration");
        InitEntry(out CandidateLevel, "CandidateLevel", 100, "Employee Candidate Skill Level");
        InitEntry(out LastTab, "LastTab", 0, "Last open tab");
        InitEntry(out WinPosX, "WinPosX", -1f, "Panel window X (-1 = center)");
        InitEntry(out WinPosY, "WinPosY", -1f, "Panel window Y (-1 = center)");
        InitEntry(out PanelOpacity, "PanelOpacity", 1f, "Panel opacity (0-1)");
        InitEntry(out PanelScale, "PanelScale", 1f, "Panel UI scale (0.7-1.6)");

        // 为每个可自定义颜色创建配置项（默认 -1 = 用默认色）
        _colorEntries.Clear();
        foreach (var item in BigAmbitionsTrainer.UI.TrainerTheme.CustomColors)
        {
            var entry = _category.CreateEntry<int>("Color_" + item.Key, -1, item.Key);
            _colorEntries[item.Key] = entry;
        }

        // 初始化本地化语言
        BigAmbitionsTrainer.L.Loc.Current = Language.Value == BigAmbitionsTrainer.L.Loc.LangEn
            ? BigAmbitionsTrainer.L.Loc.LangEn
            : BigAmbitionsTrainer.L.Loc.LangZh;

        // 应用主题（默认蓝科技 + 自定义覆盖）
        ThemeManager.ResetDefault();
        ThemeManager.ApplyCustomColors(GetColorHexOrDefault);

        MelonLogger.Msg("[Config] Preferences loaded. Language=" + Language.Value);
    }

    /// <summary>读取某个控件颜色的配置 hex；未设置返回 -1。</summary>
    public static int? GetColorHex(string key)
    {
        MelonPreferences_Entry<int> e;
        return _colorEntries.TryGetValue(key, out e) ? (int?)e.Value : null;
    }

    /// <summary>读取某个控件颜色配置，未设置返回 -1。</summary>
    private static int? GetColorHexOrDefault(string key)
    {
        return GetColorHex(key);
    }

    /// <summary>设置某个控件颜色并保存。</summary>
    public static void SetColorHex(string key, int hex)
    {
        MelonPreferences_Entry<int> e;
        if (_colorEntries.TryGetValue(key, out e))
        {
            e.Value = hex;
            Save();
        }
    }

    /// <summary>清除所有自定义颜色，恢复默认蓝科技。</summary>
    public static void ResetColors()
    {
        foreach (var pair in _colorEntries)
            pair.Value.Value = -1;
        Save();
        ThemeManager.ResetDefault();
    }

    public static void ApplyThemeToManager()
    {
        ThemeManager.ResetDefault();
        ThemeManager.ApplyCustomColors(GetColorHexOrDefault);
    }

    public static void Save()
    {
        _category.SaveToFile(false);
    }

    public static void Load()
    {
        _category.LoadFromFile();
        BigAmbitionsTrainer.L.Loc.Current = Language.Value;
        ThemeManager.ResetDefault();
        ThemeManager.ApplyCustomColors(GetColorHexOrDefault);
    }
}
