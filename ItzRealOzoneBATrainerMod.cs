using System;
using BigAmbitionsTrainer.Config;
using BigAmbitionsTrainer.Modules;
using BigAmbitionsTrainer.UI;
using MelonLoader;
using UnityEngine;

namespace BigAmbitionsTrainer;

public class ItzRealOzoneBATrainerMod : MelonMod
{
    private static int _updateCooldown;
    private const int UpdateInterval = 30;
    private static bool _f8PrevDown;
    private static bool _guiInited;

    // F8 键码（user32 GetAsyncKeyState）
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    private const int VK_F8 = 0x77;

    private static bool IsF8Pressed()
    {
        bool down = (GetAsyncKeyState(VK_F8) & 0x8000) != 0;
        bool edge = down && !_f8PrevDown;
        _f8PrevDown = down;
        return edge;
    }

    public override void OnInitializeMelon()
    {
        PrintBanner();
        MelonLogger.Msg("===========================================");
        MelonLogger.Msg("  ItzRealOzone Trainer (Mono) v1.0.2 loaded!");
        MelonLogger.Msg("  Author: Mizuof");
        MelonLogger.Msg("  Press F8 to open the trainer overlay");
        MelonLogger.Msg("===========================================");

        TrainerConfig.Initialize();
        MoneyModule.Initialize();
        PlayerStatsModule.Initialize();
        VehicleModule.Initialize();
        BusinessModule.Initialize();
        GameplayModule.Initialize();
        EmployeeModule.Initialize();
        RivalsModule.Initialize();
        WorldModule.Initialize();

        _updateCooldown = 5;
        MelonLogger.Msg("[Trainer] All modules initialized.");
    }

    private static void PrintBanner()
    {
        MelonLogger.Msg("  ███╗   ███╗██╗███████╗██╗   ██╗ ██████╗ ███████╗");
        MelonLogger.Msg("  ████╗ ████║██║╚══███╔╝██║   ██║██╔═══██╗██╔════╝");
        MelonLogger.Msg("  ██╔████╔██║██║  ███╔╝ ██║   ██║██║   ██║█████╗  ");
        MelonLogger.Msg("  ██║╚██╔╝██║██║ ███╔╝  ██║   ██║██║   ██║██╔══╝  ");
        MelonLogger.Msg("  ██║ ╚═╝ ██║██║███████╗╚██████╔╝╚██████╔╝██║     ");
        MelonLogger.Msg("  ╚═╝     ╚═╝╚═╝╚══════╝ ╚═════╝  ╚═════╝ ╚═╝     ");
        MelonLogger.Msg("  Trainer for Big Ambitions — Author: Mizuof");
    }

    public override void OnUpdate()
    {
        try
        {
            if (IsF8Pressed()) TrainerOverlay.Toggle();

            _updateCooldown--;
            if (_updateCooldown > 0) return;
            _updateCooldown = UpdateInterval;

            PlayerStatsModule.OnUpdate();
            VehicleModule.OnUpdate();
            BusinessModule.OnUpdate();
            GameplayModule.OnUpdate();
            EmployeeModule.OnUpdate();
            MoneyModule.OnUpdate();
            RivalsModule.OnUpdate();
            WorldModule.OnUpdate();
        }
        catch (Exception ex)
        {
            MelonLogger.Error("[Trainer] OnUpdate error: " + ex);
        }
    }

    public override void OnGUI()
    {
        if (!_guiInited)
        {
            TrainerOverlay.EnsureStyles();
            _guiInited = true;
        }
        TrainerOverlay.OnGUI();
        ToastNotification.Draw();
    }

    public override void OnApplicationQuit()
    {
        TrainerOverlay.Cleanup();
        ToastNotification.Cleanup();
        TrainerConfig.Save();
    }
}
