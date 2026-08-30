using System;
using System.Collections.Generic;
using BigAmbitionsTrainer.Config;
using MelonLoader;
using UnityEngine;
using Entities;
using Helpers;

namespace BigAmbitionsTrainer.Modules;

/// <summary>玩法：游戏速度 / 时间 / 交通 / 无敌 / 任务 / 进口交货。</summary>
public static class GameplayModule
{
    public static float GameSpeed { get; private set; } = 1f;
    public static bool TrafficEnabled { get; private set; } = true;
    public static bool TutorialEnabled { get; private set; } = true;
    public static bool Invincibility { get; private set; }
    public static int CurrentDay { get; private set; }
    public static int CurrentHour { get; private set; }
    public static float CurrentMinute { get; private set; }

    public static void Initialize()
    {
        MelonLogger.Msg("[GameplayModule] Initialized.");
    }

    public static void OnUpdate()
    {
        try
        {
            var current = SaveGameManager.Current;
            if (current != null)
            {
                CurrentDay = current.Day;
                CurrentHour = current.Hour;
                CurrentMinute = current.Minute;
                var gv = current.gameVariables;
                if (gv != null)
                {
                    TutorialEnabled = gv.tutorialEnabled;
                    if (TrainerConfig.DisableTutorial.Value && gv.tutorialEnabled)
                        gv.tutorialEnabled = false;
                }
            }

            var gm = GameRef.Get<GameManager>();
            if (gm != null)
            {
                TrafficEnabled = gm.spawnTraffic;
                Invincibility = gm.setInvincibilityOnStart;
                if (TrainerConfig.DisableTraffic.Value && gm.spawnTraffic)
                    gm.spawnTraffic = false;
                if (TrainerConfig.Invincibility.Value && !gm.setInvincibilityOnStart)
                    gm.setInvincibilityOnStart = true;
            }
        }
        catch { }
    }

    /// <summary>设置游戏时间流速。1.0 中 MinutesMultiplier 为私有字段，只能经 SetMinutesMultiplier 写入。</summary>
    public static void SetGameSpeed(float speed)
    {
        try
        {
            GameManager.SetMinutesMultiplier(speed);
            TrainerConfig.GameSpeed.Value = speed;
            GameSpeed = speed;
        }
        catch (Exception ex) { MelonLogger.Warning("[Gameplay] Speed error: " + ex.Message); }
    }

    public static void ToggleTraffic(bool enabled)
    {
        try
        {
            var gm = GameRef.Get<GameManager>();
            if (gm != null) gm.spawnTraffic = enabled;
            TrainerConfig.DisableTraffic.Value = !enabled;
            if (!enabled) GameManager.Command_ToggleTraffic();
        }
        catch (Exception ex) { MelonLogger.Warning("[Gameplay] Traffic error: " + ex.Message); }
    }

    public static void ToggleTutorial(bool enabled)
    {
        try
        {
            var current = SaveGameManager.Current;
            var gv = current != null ? current.gameVariables : null;
            if (gv != null) gv.tutorialEnabled = enabled;
            TrainerConfig.DisableTutorial.Value = !enabled;
        }
        catch (Exception ex) { MelonLogger.Warning("[Gameplay] Tutorial error: " + ex.Message); }
    }

    public static void ToggleInvincibility(bool enabled)
    {
        try
        {
            var gm = GameRef.Get<GameManager>();
            if (gm != null) gm.setInvincibilityOnStart = enabled;
            TrainerConfig.Invincibility.Value = enabled;
        }
        catch (Exception ex) { MelonLogger.Warning("[Gameplay] Invincibility error: " + ex.Message); }
    }

    public static bool SaveGame()
    {
        try
        {
            var gm = GameRef.Get<GameManager>();
            if (gm == null) return false;
            return gm.SaveGame("TrainerSave", false);
        }
        catch (Exception ex)
        {
            MelonLogger.Warning("[Gameplay] Save error: " + ex.Message);
            return false;
        }
    }

    public static void TeleportToQuestTarget()
    {
        try { GameManager.Command_TeleportPlayerToQuestTarget(); }
        catch (Exception ex) { MelonLogger.Warning("[Gameplay] TP quest error: " + ex.Message); }
    }

    public static void TeleportToDestination()
    {
        try { GameManager.Command_TeleportPlayerToDestination(); }
        catch (Exception ex) { MelonLogger.Warning("[Gameplay] TP dest error: " + ex.Message); }
    }

    public static void DeliverAllImportsPaid()
    {
        try
        {
            var current = SaveGameManager.Current;
            if (current == null) return;
            int day = current.Day;
            int num = 0;
            var contracts = current.DeliveryContracts;
            if (contracts != null)
            {
                foreach (var c in contracts)
                {
                    try { if (c.enabled && c.nextDeliveryDay > day) { c.nextDeliveryDay = day; num++; } }
                    catch { }
                }
            }
            ImportPartnership.DoAllDeliveries();
            MelonLogger.Msg("[Gameplay] Delivered " + num + " contracts (paid).");
        }
        catch (Exception ex) { MelonLogger.Warning("[Gameplay] Deliver paid error: " + ex.Message); }
    }

    public static void DeliverAllImportsFree()
    {
        try
        {
            var current = SaveGameManager.Current;
            if (current == null) return;
            int day = current.Day;
            var contracts = current.DeliveryContracts;
            int num = 0;
            var fees = new List<float>();
            if (contracts != null)
            {
                foreach (var c in contracts)
                {
                    try
                    {
                        fees.Add(c.deliveryFee);
                        if (c.enabled)
                        {
                            c.deliveryFee = 0f;
                            if (c.nextDeliveryDay > day) { c.nextDeliveryDay = day; num++; }
                        }
                    }
                    catch { fees.Add(0f); }
                }
            }
            ImportPartnership.DoAllDeliveries();
            if (contracts != null)
            {
                for (int i = 0; i < contracts.Count && i < fees.Count; i++)
                {
                    try { contracts[i].deliveryFee = fees[i]; } catch { }
                }
            }
            MelonLogger.Msg("[Gameplay] Delivered " + num + " contracts (free).");
        }
        catch (Exception ex) { MelonLogger.Warning("[Gameplay] Deliver free error: " + ex.Message); }
    }

    public static void SkipToNextDay()
    {
        try
        {
            var current = SaveGameManager.Current;
            if (current == null) return;
            int day = current.Day;
            current.Day = day + 1;
            current.Hour = 6;
            current.Minute = 0f;
            MelonLogger.Msg("[Gameplay] Skipped to Day " + (day + 1) + ", 06:00.");
        }
        catch (Exception ex) { MelonLogger.Warning("[Gameplay] Skip day error: " + ex.Message); }
    }

    public static void SetTimeOfDay(int hour, int minute)
    {
        try
        {
            var current = SaveGameManager.Current;
            if (current == null) return;
            current.Hour = hour;
            current.Minute = minute;
        }
        catch (Exception ex) { MelonLogger.Warning("[Gameplay] Set time error: " + ex.Message); }
    }

    public static void UnlockAllContacts()
    {
        try { Entities.ContactsHelper.UnlockAllContacts(); }
        catch (Exception ex) { MelonLogger.Warning("[Gameplay] Contacts error: " + ex.Message); }
    }

    public static void CompleteQuest()
    {
        try { TutorialHelper.Command_CompleteQuest(); }
        catch (Exception ex) { MelonLogger.Warning("[Gameplay] Quest error: " + ex.Message); }
    }

    public static void CompleteObjective()
    {
        try { TutorialHelper.Command_CompleteObjective(); }
        catch (Exception ex) { MelonLogger.Warning("[Gameplay] Objective error: " + ex.Message); }
    }
}
