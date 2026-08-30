using System;
using BigAmbitionsTrainer.Config;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BigAmbitionsTrainer.Modules;

/// <summary>玩家需求 / 状态 / 速度 / 年龄 / 个人目标。</summary>
public static class PlayerStatsModule
{
    private static ThirdPersonCharacter _cachedPlayer;
    private static int _playerSearchCooldown;
    private static int _hungerRefillCounter;
    private static int _readCooldown;
    private const int ReadInterval = 30;

    public static float CurrentEnergy { get; private set; }
    public static float CurrentHappiness { get; private set; }
    public static float CurrentHunger { get; private set; }
    public static bool IsEnergyDisabled { get; private set; }
    public static bool IsHappinessDisabled { get; private set; }
    public static bool IsAgingDisabled { get; private set; }
    public static bool IsHungerDisabled { get; private set; }
    public static int PlayerSpeedIndex { get; private set; }

    public static void Initialize()
    {
        _readCooldown = 10;
        MelonLogger.Msg("[PlayerStatsModule] Initialized.");
    }

    public static void OnUpdate()
    {
        try
        {
            var current = SaveGameManager.Current;
            if (current == null) return;
            var gv = current.gameVariables;

            if (gv != null)
            {
                if (TrainerConfig.DisableEnergy.Value && !gv.disableEnergy) gv.disableEnergy = true;
                if (TrainerConfig.DisableHappiness.Value && !gv.disableHappiness) gv.disableHappiness = true;
                if (TrainerConfig.DisableAging.Value && !gv.disableAging) gv.disableAging = true;
            }

            if (TrainerConfig.DisableHunger.Value)
            {
                _hungerRefillCounter++;
                if (_hungerRefillCounter >= 60)
                {
                    _hungerRefillCounter = 0;
                    try { GameManager.Command_ChangeHunger(100); } catch { }
                }
            }

            if (--_readCooldown > 0) return;
            _readCooldown = ReadInterval;

            CurrentEnergy = current.Energy;
            CurrentHappiness = current.Happiness;
            CurrentHunger = current.Hunger;
            IsHungerDisabled = TrainerConfig.DisableHunger.Value;
            if (gv != null)
            {
                IsEnergyDisabled = gv.disableEnergy;
                IsHappinessDisabled = gv.disableHappiness;
                IsAgingDisabled = gv.disableAging;
            }

            try
            {
                if ((Object)_cachedPlayer == null && _playerSearchCooldown <= 0)
                {
                    _cachedPlayer = Object.FindObjectOfType<ThirdPersonCharacter>();
                    _playerSearchCooldown = 60;
                }
                if (_playerSearchCooldown > 0) _playerSearchCooldown--;
                if ((Object)_cachedPlayer != null)
                {
                    var ws = _cachedPlayer.walkingSpeed;
                    switch ((int)ws)
                    {
                        case 1: PlayerSpeedIndex = 0; break;
                        case 2: PlayerSpeedIndex = 1; break;
                        case 3: PlayerSpeedIndex = 2; break;
                        case 4: PlayerSpeedIndex = 3; break;
                    }
                }
            }
            catch { }
        }
        catch { }
    }

    public static void SetEnergy(float value)
    {
        if (SaveGameManager.Current == null) return;
        try { GameManager.Command_SetEnergy(value); }
        catch (Exception ex) { MelonLogger.Warning("[PlayerStats] Energy error: " + ex.Message); }
    }

    public static void ChangeHappiness(int amount)
    {
        if (SaveGameManager.Current == null) return;
        try { GameManager.Command_ChangeHappiness(amount); }
        catch (Exception ex) { MelonLogger.Warning("[PlayerStats] Happiness error: " + ex.Message); }
    }

    public static void ChangeHunger(int amount)
    {
        if (SaveGameManager.Current == null) return;
        try { GameManager.Command_ChangeHunger(amount); }
        catch (Exception ex) { MelonLogger.Warning("[PlayerStats] Hunger error: " + ex.Message); }
    }

    public static void FillAllNeeds()
    {
        if (SaveGameManager.Current == null) return;
        try { GameManager.Command_SetEnergy(100f); } catch (Exception ex) { MelonLogger.Warning(ex.Message); }
        try { GameManager.Command_ChangeHappiness(100); } catch (Exception ex) { MelonLogger.Warning(ex.Message); }
        try { GameManager.Command_ChangeHunger(100); } catch (Exception ex) { MelonLogger.Warning(ex.Message); }
    }

    public static void ToggleDisableEnergy(bool value)
    {
        try
        {
            var gv = SaveGameManager.Current != null ? SaveGameManager.Current.gameVariables : null;
            if (gv != null) gv.disableEnergy = value;
            TrainerConfig.DisableEnergy.Value = value;
        }
        catch (Exception ex) { MelonLogger.Warning("[PlayerStats] Toggle energy error: " + ex.Message); }
    }

    public static void ToggleDisableHappiness(bool value)
    {
        try
        {
            var gv = SaveGameManager.Current != null ? SaveGameManager.Current.gameVariables : null;
            if (gv != null) gv.disableHappiness = value;
            TrainerConfig.DisableHappiness.Value = value;
        }
        catch (Exception ex) { MelonLogger.Warning("[PlayerStats] Toggle happy error: " + ex.Message); }
    }

    public static void ToggleDisableAging(bool value)
    {
        try
        {
            var gv = SaveGameManager.Current != null ? SaveGameManager.Current.gameVariables : null;
            if (gv != null) gv.disableAging = value;
            TrainerConfig.DisableAging.Value = value;
        }
        catch (Exception ex) { MelonLogger.Warning("[PlayerStats] Toggle aging error: " + ex.Message); }
    }

    public static void ToggleDisableHunger(bool value)
    {
        try
        {
            TrainerConfig.DisableHunger.Value = value;
            IsHungerDisabled = value;
            _hungerRefillCounter = 0;
            if (value) { try { GameManager.Command_ChangeHunger(100); } catch { } }
        }
        catch (Exception ex) { MelonLogger.Warning("[PlayerStats] Toggle hunger error: " + ex.Message); }
    }

    public static void SetPlayerSpeed(int speedIndex)
    {
        try
        {
            if ((Object)_cachedPlayer == null) _cachedPlayer = Object.FindObjectOfType<ThirdPersonCharacter>();
            var p = _cachedPlayer;
            if ((Object)p != null)
            {
                ThirdPersonCharacter.WalkingSpeed ws;
                switch (speedIndex)
                {
                    case 1: ws = ThirdPersonCharacter.WalkingSpeed.Jog; break;
                    case 2: ws = ThirdPersonCharacter.WalkingSpeed.Run; break;
                    case 3: ws = ThirdPersonCharacter.WalkingSpeed.Scooter; break;
                    default: ws = ThirdPersonCharacter.WalkingSpeed.Walk; break;
                }
                p.walkingSpeed = ws;
                PlayerSpeedIndex = speedIndex;
            }
        }
        catch (Exception ex) { MelonLogger.Warning("[PlayerStats] Speed error: " + ex.Message); }
    }

    public static void ChangeAge(float delta)
    {
        try { GameManager.Command_ChangeAge(delta); }
        catch (Exception ex) { MelonLogger.Warning("[PlayerStats] Age error: " + ex.Message); }
    }

    /// <summary>完成所有未完成的个人目标。</summary>
    public static void CompletePersonalGoals()
    {
        try
        {
            var gm = GameRef.Get<GameManager>();
            if (gm == null || gm.personalGoals == null)
            {
                MelonLogger.Warning("[PlayerStats] No personalGoals found.");
                return;
            }
            var current = SaveGameManager.Current;
            if (current == null || current.completedPersonalGoals == null) return;
            int count = 0;
            foreach (var goal in gm.personalGoals)
            {
                try
                {
                    if (goal == null || goal.identifier == null) continue;
                    if (!current.completedPersonalGoals.Contains(goal.identifier))
                    {
                        current.completedPersonalGoals.Add(goal.identifier);
                        count++;
                    }
                }
                catch { }
            }
            MelonLogger.Msg("[PlayerStats] Completed " + count + " personal goals.");
        }
        catch (Exception ex) { MelonLogger.Warning("[PlayerStats] Complete goals error: " + ex.Message); }
    }
}
