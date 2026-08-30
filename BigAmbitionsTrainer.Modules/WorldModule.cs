using System;
using MelonLoader;

namespace BigAmbitionsTrainer.Modules;

/// <summary>世界/时间相关修改。</summary>
public static class WorldModule
{
    public static int CurrentDay { get; private set; }
    public static int CurrentHour { get; private set; }
    public static float CurrentMinute { get; private set; }
    public static float BankInterestMultiplier { get; set; } = 1f;

    public static void Initialize()
    {
        MelonLogger.Msg("[WorldModule] Initialized.");
    }

    public static void OnUpdate()
    {
        try
        {
            var current = SaveGameManager.Current;
            if (current == null) return;
            CurrentDay = current.Day;
            CurrentHour = current.Hour;
            CurrentMinute = current.Minute;
            var gv = current.gameVariables;
            if (gv != null) BankInterestMultiplier = gv.bankInterestMultiplier;
        }
        catch { }
    }

    public static void ApplyBankInterestMultiplier(float value)
    {
        try
        {
            var current = SaveGameManager.Current;
            var gv = current != null ? current.gameVariables : null;
            if (gv != null) gv.bankInterestMultiplier = value;
        }
        catch (Exception ex) { MelonLogger.Warning("[World] Bank interest error: " + ex.Message); }
    }
}
