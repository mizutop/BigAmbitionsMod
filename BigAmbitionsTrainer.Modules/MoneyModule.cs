using System;
using MelonLoader;

namespace BigAmbitionsTrainer.Modules;

/// <summary>资金与经济类修改。</summary>
public static class MoneyModule
{
    private static int _readCooldown;
    private const int ReadInterval = 30;

    public static float CurrentMoney { get; private set; }
    public static float CurrentNetWorth { get; private set; }
    public static int TaxPercentage { get; set; }
    public static float MarketPriceMultiplier { get; set; }
    public static float ExportMultiplier { get; set; }

    public static void Initialize()
    {
        _readCooldown = 5;
        MelonLogger.Msg("[MoneyModule] Initialized.");
    }

    public static void OnUpdate()
    {
        if (--_readCooldown > 0) return;
        _readCooldown = ReadInterval;
        try
        {
            var current = SaveGameManager.Current;
            if (current == null) return;
            CurrentMoney = current.Money;
            CurrentNetWorth = current.NetWorth;
            var gv = current.gameVariables;
            if (gv != null)
            {
                TaxPercentage = gv.taxPercentage;
                MarketPriceMultiplier = gv.marketPriceMultiplier;
                ExportMultiplier = gv.exportMultiplier;
            }
        }
        catch { }
    }

    public static void AddMoney(float amount)
    {
        try
        {
            GameManager.Command_ChangeMoney(amount);
            MelonLogger.Msg("[Money] Added " + amount);
        }
        catch (Exception ex) { MelonLogger.Warning("[Money] Add error: " + ex.Message); }
    }

    public static void SetMoney(float amount)
    {
        try
        {
            GameManager.Command_SetMoney(amount);
            MelonLogger.Msg("[Money] Set to " + amount);
        }
        catch (Exception ex) { MelonLogger.Warning("[Money] Set error: " + ex.Message); }
    }

    public static void ApplyTaxPercentage(int value)
    {
        try
        {
            var current = SaveGameManager.Current;
            var gv = current != null ? current.gameVariables : null;
            if (gv != null) { gv.taxPercentage = value; TaxPercentage = value; }
        }
        catch (Exception ex) { MelonLogger.Warning("[Money] Tax error: " + ex.Message); }
    }

    public static void ApplyMarketPriceMultiplier(float value)
    {
        try
        {
            var current = SaveGameManager.Current;
            var gv = current != null ? current.gameVariables : null;
            if (gv != null) { gv.marketPriceMultiplier = value; MarketPriceMultiplier = value; }
        }
        catch (Exception ex) { MelonLogger.Warning("[Money] Price mult error: " + ex.Message); }
    }

    public static void ApplyExportMultiplier(float value)
    {
        try
        {
            var current = SaveGameManager.Current;
            var gv = current != null ? current.gameVariables : null;
            if (gv != null) { gv.exportMultiplier = value; ExportMultiplier = value; }
        }
        catch (Exception ex) { MelonLogger.Warning("[Money] Export mult error: " + ex.Message); }
    }
}
