using System;
using MelonLoader;

namespace BigAmbitionsTrainer.Modules;

/// <summary>简单撤销系统（保留原设计，未接线到 UI）。</summary>
public static class UndoSystem
{
    private static Action _undoAction;
    public static string LastActionDescription { get; private set; } = "";
    public static bool HasUndo => _undoAction != null;

    public static void RegisterUndo(string description, Action undoAction)
    {
        LastActionDescription = description;
        _undoAction = undoAction;
    }

    public static void Undo()
    {
        if (_undoAction == null) return;
        try { _undoAction(); } catch (Exception ex) { MelonLogger.Warning("[UndoSystem] Undo failed: " + ex.Message); }
        finally { Clear(); }
    }

    public static void Clear()
    {
        _undoAction = null;
        LastActionDescription = "";
    }
}
