using System;
using System.Collections.Generic;
using BigAmbitionsTrainer.Config;
using MelonLoader;

namespace BigAmbitionsTrainer.Modules;

/// <summary>商业相关修改：满意度 / 解锁 / 倍率 / 传送。</summary>
public static class BusinessModule
{
    public class PlayerBusinessInfo
    {
        public string DisplayName;
        public string BusinessName;
        public int HouseNumber;
        public string StreetName;
        internal BuildingRegistration _registration;
    }

    public static bool AllCoursesUnlocked { get; private set; }
    public static bool AllContactsUnlocked { get; private set; }
    public static bool DisableWholesaleImportLimits { get; private set; }
    public static bool AllProductsFromImporters { get; private set; }
    public static float CustomerPromotionMultiplier { get; set; }
    public static float EmployeeSalaryMultiplier { get; set; }
    public static float WholesaleUrgentFeeMultiplier { get; set; }
    public static float ImporterUrgentFeeMultiplier { get; set; }
    public static float BankInterestRate { get; set; }
    public static float RivalsDifficultyMultiplier { get; set; }

    public static List<PlayerBusinessInfo> PlayerBusinesses { get; private set; } = new List<PlayerBusinessInfo>();

    public static void Initialize()
    {
        MelonLogger.Msg("[BusinessModule] Initialized.");
    }

    public static void OnUpdate()
    {
        try
        {
            var current = SaveGameManager.Current;
            var gv = current != null ? current.gameVariables : null;
            if (gv == null) return;
            AllCoursesUnlocked = gv.allCoursesUnlocked;
            AllContactsUnlocked = gv.allContactsUnlocked;
            DisableWholesaleImportLimits = gv.disableWholesaleAndImportLimits;
            AllProductsFromImporters = gv.allProductsAvailableFromImporters;
            CustomerPromotionMultiplier = gv.baseCustomerPromotionMultiplier;
            EmployeeSalaryMultiplier = gv.employeeHourlySalaryMultiplier;
            WholesaleUrgentFeeMultiplier = gv.wholesaleUrgentFeeMultiplier;
            ImporterUrgentFeeMultiplier = gv.importerUrgentFeeMultiplier;
            BankInterestRate = gv.bankInterestMultiplier;
            RivalsDifficultyMultiplier = gv.rivalsDifficultyMultiplier;

            if (TrainerConfig.AllCoursesUnlocked.Value && !gv.allCoursesUnlocked) gv.allCoursesUnlocked = true;
            if (TrainerConfig.AllContactsUnlocked.Value && !gv.allContactsUnlocked) gv.allContactsUnlocked = true;
            if (TrainerConfig.DisableWholesaleImportLimits.Value && !gv.disableWholesaleAndImportLimits) gv.disableWholesaleAndImportLimits = true;
            if (TrainerConfig.AllProductsFromImporters.Value && !gv.allProductsAvailableFromImporters) gv.allProductsAvailableFromImporters = true;
        }
        catch { }
    }

    public static void ToggleAllCourses(bool value)
    {
        try
        {
            var gv = SaveGameManager.Current != null ? SaveGameManager.Current.gameVariables : null;
            if (gv != null) gv.allCoursesUnlocked = value;
            TrainerConfig.AllCoursesUnlocked.Value = value;
        }
        catch (Exception ex) { MelonLogger.Warning("[Business] Courses error: " + ex.Message); }
    }

    public static void ToggleAllContacts(bool value)
    {
        try
        {
            var gv = SaveGameManager.Current != null ? SaveGameManager.Current.gameVariables : null;
            if (gv != null) gv.allContactsUnlocked = value;
            TrainerConfig.AllContactsUnlocked.Value = value;
        }
        catch (Exception ex) { MelonLogger.Warning("[Business] Contacts error: " + ex.Message); }
    }

    public static void ToggleWholesaleImportLimits(bool value)
    {
        try
        {
            var gv = SaveGameManager.Current != null ? SaveGameManager.Current.gameVariables : null;
            if (gv != null) gv.disableWholesaleAndImportLimits = value;
            TrainerConfig.DisableWholesaleImportLimits.Value = value;
        }
        catch (Exception ex) { MelonLogger.Warning("[Business] Import limits error: " + ex.Message); }
    }

    public static void ToggleAllProductsFromImporters(bool value)
    {
        try
        {
            var gv = SaveGameManager.Current != null ? SaveGameManager.Current.gameVariables : null;
            if (gv != null) gv.allProductsAvailableFromImporters = value;
            TrainerConfig.AllProductsFromImporters.Value = value;
        }
        catch (Exception ex) { MelonLogger.Warning("[Business] Import products error: " + ex.Message); }
    }

    public static void ApplyCustomerPromotionMultiplier(float value)
    {
        try { var gv = SaveGameManager.Current?.gameVariables; if (gv != null) gv.baseCustomerPromotionMultiplier = value; }
        catch (Exception ex) { MelonLogger.Warning("[Business] Promotion error: " + ex.Message); }
    }

    public static void ApplyEmployeeSalaryMultiplier(float value)
    {
        try { var gv = SaveGameManager.Current?.gameVariables; if (gv != null) gv.employeeHourlySalaryMultiplier = value; }
        catch (Exception ex) { MelonLogger.Warning("[Business] Salary error: " + ex.Message); }
    }

    public static void ApplyWholesaleUrgentFeeMultiplier(float value)
    {
        try { var gv = SaveGameManager.Current?.gameVariables; if (gv != null) gv.wholesaleUrgentFeeMultiplier = value; }
        catch (Exception ex) { MelonLogger.Warning("[Business] Wholesale fee error: " + ex.Message); }
    }

    public static void ApplyImporterUrgentFeeMultiplier(float value)
    {
        try { var gv = SaveGameManager.Current?.gameVariables; if (gv != null) gv.importerUrgentFeeMultiplier = value; }
        catch (Exception ex) { MelonLogger.Warning("[Business] Importer fee error: " + ex.Message); }
    }

    public static void ApplyBankInterestRate(float value)
    {
        try { var gv = SaveGameManager.Current?.gameVariables; if (gv != null) gv.bankInterestMultiplier = value; }
        catch (Exception ex) { MelonLogger.Warning("[Business] Interest error: " + ex.Message); }
    }

    public static void ApplyRivalsDifficultyMultiplier(float value)
    {
        try { var gv = SaveGameManager.Current?.gameVariables; if (gv != null) gv.rivalsDifficultyMultiplier = value; }
        catch (Exception ex) { MelonLogger.Warning("[Business] Rivals difficulty error: " + ex.Message); }
    }

    /// <summary>最大化所有玩家所属建筑的满意度。</summary>
    public static int MaxAllSatisfaction()
    {
        try
        {
            var current = SaveGameManager.Current;
            if (current == null || current.BuildingRegistrations == null) return 0;
            int count = 0;
            foreach (var b in current.BuildingRegistrations)
            {
                try
                {
                    if (b != null && b.BuildingOwnedByPlayer && b.satisfaction != null)
                    {
                        b.satisfaction.customerService = 100;
                        b.satisfaction.pricing = 100;
                        b.satisfaction.cleanliness = 100;
                        b.satisfaction.facility = 100;
                        b.satisfaction.overall = 100;
                        count++;
                    }
                }
                catch { }
            }
            return count;
        }
        catch (Exception ex) { MelonLogger.Warning("[Business] Max satisfaction error: " + ex.Message); return 0; }
    }

    /// <summary>刷新玩家所属建筑列表（供商业 tab 展示 / 传送）。</summary>
    public static void RefreshPlayerBusinesses(string searchFilter)
    {
        try
        {
            PlayerBusinesses.Clear();
            var current = SaveGameManager.Current;
            if (current == null || current.BuildingRegistrations == null) return;
            string filter = (searchFilter ?? "").Trim().ToLowerInvariant();
            foreach (var val in current.BuildingRegistrations)
            {
                try
                {
                    if (val == null || !val.BuildingOwnedByPlayer) continue;
                    string text;
                    try { text = val.GetDisplayName(); } catch { text = "Unknown"; }
                    string business;
                    try { business = val.BusinessName ?? ""; } catch { business = ""; }
                    if (filter.Length > 0 && text.ToLowerInvariant().IndexOf(filter, StringComparison.Ordinal) < 0
                        && business.ToLowerInvariant().IndexOf(filter, StringComparison.Ordinal) < 0)
                        continue;
                    var info = new PlayerBusinessInfo { DisplayName = text, BusinessName = business, _registration = val };
                    try { info.HouseNumber = val.StreetNumber; } catch { info.HouseNumber = 0; }
                    try { info.StreetName = val.StreetName ?? ""; } catch { info.StreetName = "?"; }
                    PlayerBusinesses.Add(info);
                }
                catch { }
            }
        }
        catch (Exception ex) { MelonLogger.Warning("[Business] Refresh error: " + ex.Message); }
    }

    /// <summary>传送到指定建筑。</summary>
    public static bool TeleportToBusiness(int index)
    {
        try
        {
            if (index < 0 || index >= PlayerBusinesses.Count) return false;
            var info = PlayerBusinesses[index];
            if (info._registration == null) return false;
            GameManager.Command_TeleportPlayerToAddress(info._registration.StreetNumber, info._registration.StreetName);
            return true;
        }
        catch (Exception ex) { MelonLogger.Warning("[Business] Teleport error: " + ex.Message); return false; }
    }
}
