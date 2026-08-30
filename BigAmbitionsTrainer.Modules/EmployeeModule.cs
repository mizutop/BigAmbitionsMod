using System;
using System.Collections.Generic;
using BigAmbitionsTrainer.Config;
using Helpers;
using MelonLoader;

namespace BigAmbitionsTrainer.Modules;

/// <summary>员工管理：满意度 / 薪资倍率 / 招聘候选人。</summary>
public static class EmployeeModule
{
    private static readonly string[] SkillKeys =
    {
        "ba:skill_customerservice",   // 0 客服
        "ba:skill_cleaning",          // 1 清洁
        "ba:skill_lawyer",            // 2 律师
        "ba:skill_purchasingagent",   // 3 采购
        "ba:skill_logisticsmanager",  // 4 物流
        "ba:skill_deliverydriver",    // 5 配送
        "ba:skill_programmer",        // 6 程序员
        "ba:skill_hrmanager",         // 7 人事经理
    };

    public static int EmployeeCount { get; private set; }
    public static float SalaryMultiplier { get; private set; } = 1f;
    public static int SelectedEmployeeIndex { get; set; }
    public static string SelectedEmployeeName { get; private set; } = "";
    public static float SelectedEmployeeSatisfaction { get; private set; }
    public static float SelectedEmployeeWage { get; private set; }
    public static int CandidateSkillLevel { get; set; } = 100;

    public static void Initialize()
    {
        CandidateSkillLevel = 100;
        MelonLogger.Msg("[EmployeeModule] Initialized.");
    }

    public static void OnUpdate()
    {
        try
        {
            var current = SaveGameManager.Current;
            if (current == null) return;
            var emps = current.EmployeeInstances;
            EmployeeCount = emps != null ? emps.Count : 0;
            var gv = current.gameVariables;
            if (gv != null) SalaryMultiplier = gv.employeeHourlySalaryMultiplier;

            if (emps == null || emps.Count == 0) { SelectedEmployeeName = ""; SelectedEmployeeSatisfaction = 0f; SelectedEmployeeWage = 0f; return; }

            if (SelectedEmployeeIndex >= emps.Count) SelectedEmployeeIndex = emps.Count - 1;
            if (SelectedEmployeeIndex < 0) SelectedEmployeeIndex = 0;
            try
            {
                var emp = emps[SelectedEmployeeIndex];
                if (emp != null)
                {
                    try { SelectedEmployeeName = emp.characterData != null ? emp.characterData.name ?? "Unknown" : "Unknown"; }
                    catch { SelectedEmployeeName = "Unknown"; }
                    try { SelectedEmployeeSatisfaction = emp.satisfaction; } catch { }
                    try { SelectedEmployeeWage = emp.hourlyWage; } catch { }
                }
            }
            catch { }
        }
        catch { }
    }

    public static void MaxSelectedSatisfaction()
    {
        try
        {
            var current = SaveGameManager.Current;
            var emps = current != null ? current.EmployeeInstances : null;
            if (emps == null || emps.Count == 0 || SelectedEmployeeIndex < 0 || SelectedEmployeeIndex >= emps.Count) return;
            var emp = emps[SelectedEmployeeIndex];
            if (emp != null) emp.satisfaction = 100f;
        }
        catch (Exception ex) { MelonLogger.Warning("[Employee] Max selected error: " + ex.Message); }
    }

    public static void ApplySalaryMultiplier(float value)
    {
        try { var gv = SaveGameManager.Current?.gameVariables; if (gv != null) gv.employeeHourlySalaryMultiplier = value; }
        catch (Exception ex) { MelonLogger.Warning("[Employee] Salary mult error: " + ex.Message); }
    }

    public static int MaxAllSatisfaction()
    {
        try
        {
            var emps = SaveGameManager.Current != null ? SaveGameManager.Current.EmployeeInstances : null;
            if (emps == null) return 0;
            int count = 0;
            foreach (var emp in emps)
            {
                try { if (emp != null) { emp.satisfaction = 100f; count++; } } catch { }
            }
            return count;
        }
        catch (Exception ex) { MelonLogger.Warning("[Employee] Max all error: " + ex.Message); return 0; }
    }

    public static int SetAllWages(float wage)
    {
        try
        {
            var emps = SaveGameManager.Current != null ? SaveGameManager.Current.EmployeeInstances : null;
            if (emps == null) return 0;
            int count = 0;
            foreach (var emp in emps)
            {
                try { if (emp != null) { emp.hourlyWage = wage; count++; } } catch { }
            }
            return count;
        }
        catch (Exception ex) { MelonLogger.Warning("[Employee] Set wages error: " + ex.Message); return 0; }
    }

    /// <summary>生成指定技能的招聘候选人。</summary>
    public static bool GenerateCandidate(int skillIndex, int skillLevel)
    {
        try
        {
            if (skillIndex < 0 || skillIndex >= SkillKeys.Length) return false;
            RecruitmentCommands.Command_GenerateCandidate(SkillKeys[skillIndex], skillLevel);
            return true;
        }
        catch (Exception ex) { MelonLogger.Warning("[Employee] Candidate error: " + ex.Message); return false; }
    }
}
