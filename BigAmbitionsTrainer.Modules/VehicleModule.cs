using System;
using BigAmbitionsTrainer.Config;
using Helpers;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BigAmbitionsTrainer.Modules;

/// <summary>载具相关修改。</summary>
public static class VehicleModule
{
    public static float CurrentFuel { get; private set; }
    public static float CurrentCondition { get; private set; }
    public static float CurrentDirtiness { get; private set; }
    public static float UnpaidParkingAmount { get; private set; }
    public static bool HasSelectedVehicle { get; private set; }
    public static bool IsVehicleDamageDisabled { get; private set; }
    public static bool IsVehicleFuelDisabled { get; private set; }

    public static void Initialize()
    {
        MelonLogger.Msg("[VehicleModule] Initialized.");
    }

    private static GameManager GetGM()
    {
        return GameRef.Get<GameManager>();
    }

    public static void OnUpdate()
    {
        try
        {
            var current = SaveGameManager.Current;
            var gv = current != null ? current.gameVariables : null;
            if (gv != null)
            {
                IsVehicleDamageDisabled = gv.disableVehicleDamage;
                IsVehicleFuelDisabled = gv.disableVehicleFuel;
                if (TrainerConfig.DisableVehicleDamage.Value && !gv.disableVehicleDamage) gv.disableVehicleDamage = true;
                if (TrainerConfig.DisableVehicleFuel.Value && !gv.disableVehicleFuel) gv.disableVehicleFuel = true;
            }
            var gm = GetGM();
            if (gm == null) return;
            var sel = gm.selectedVehicle;
            if ((Object)sel != null)
            {
                HasSelectedVehicle = true;
                CurrentFuel = sel.GetCurrentFuel();
                CurrentCondition = sel.GetCurrentCondition();
                if (sel.vehicleInstance != null)
                {
                    CurrentDirtiness = sel.vehicleInstance.dirtiness;
                    UnpaidParkingAmount = sel.vehicleInstance.unpaidParkingAmount;
                }
            }
            else
            {
                HasSelectedVehicle = false;
                CurrentFuel = 0f; CurrentCondition = 0f; CurrentDirtiness = 0f; UnpaidParkingAmount = 0f;
            }
        }
        catch { }
    }

    private static VehicleController GetSelectedVehicle()
    {
        var gm = GetGM();
        return gm != null ? gm.selectedVehicle : null;
    }

    public static void ToggleVehicleDamage(bool disabled)
    {
        try
        {
            var gv = SaveGameManager.Current != null ? SaveGameManager.Current.gameVariables : null;
            if (gv != null) gv.disableVehicleDamage = disabled;
            TrainerConfig.DisableVehicleDamage.Value = disabled;
        }
        catch (Exception ex) { MelonLogger.Warning("[Vehicle] Damage toggle error: " + ex.Message); }
    }

    public static void ToggleVehicleFuel(bool disabled)
    {
        try
        {
            var gv = SaveGameManager.Current != null ? SaveGameManager.Current.gameVariables : null;
            if (gv != null) gv.disableVehicleFuel = disabled;
            TrainerConfig.DisableVehicleFuel.Value = disabled;
        }
        catch (Exception ex) { MelonLogger.Warning("[Vehicle] Fuel toggle error: " + ex.Message); }
    }

    public static bool RepairVehicle()
    {
        try
        {
            var v = GetSelectedVehicle();
            if ((Object)v == null) return false;
            v.Repair();
            return true;
        }
        catch (Exception ex) { MelonLogger.Warning("[Vehicle] Repair error: " + ex.Message); return false; }
    }

    public static bool RefuelVehicle()
    {
        try
        {
            var v = GetSelectedVehicle();
            if ((Object)v == null) return false;
            v.SetFuel(100f);
            return true;
        }
        catch (Exception ex) { MelonLogger.Warning("[Vehicle] Refuel error: " + ex.Message); return false; }
    }

    public static bool CleanVehicle()
    {
        try
        {
            var v = GetSelectedVehicle();
            if ((Object)v == null) return false;
            v.SetDirtiness(0f);
            return true;
        }
        catch (Exception ex) { MelonLogger.Warning("[Vehicle] Clean error: " + ex.Message); return false; }
    }

    public static bool ClearParkingTickets()
    {
        try
        {
            var v = GetSelectedVehicle();
            if ((Object)v == null || v.vehicleInstance == null) return false;
            v.vehicleInstance.unpaidParkingAmount = 0f;
            return true;
        }
        catch (Exception ex) { MelonLogger.Warning("[Vehicle] Tickets error: " + ex.Message); return false; }
    }

    public static void TowToGasStation()
    {
        try { VehicleHelper.Command_TowVehicle("ba:towdestination_gasstation"); }
        catch (Exception ex) { MelonLogger.Warning("[Vehicle] Tow gas error: " + ex.Message); }
    }

    public static void TowToAutoRepair()
    {
        try { VehicleHelper.Command_TowVehicle("ba:towdestination_autorepairshop"); }
        catch (Exception ex) { MelonLogger.Warning("[Vehicle] Tow repair error: " + ex.Message); }
    }
}
