using System;
using System.Collections.Generic;

namespace BigAmbitionsTrainer.L;

/// <summary>
/// 中英双语本地化。默认中文。
/// 全部 UI 文案 / Toast / 配置显示名统一走这里实现语言切换。
/// </summary>
public static class Loc
{
    /// <summary>枚举语言常量。</summary>
    public const string LangZh = "zh";
    public const string LangEn = "en";

    private static readonly Dictionary<string, string> zh = new Dictionary<string, string>
    {
        // —— 通用 ——
        ["on"] = "开启", ["off"] = "关闭",
        ["add"] = "添加", ["set"] = "设置", ["none"] = "无",
        ["confirm"] = "确认", ["cancel"] = "取消",
        ["success"] = "成功", ["error"] = "错误",

        // —— Tab 名称 ——
        ["tab_money"] = "资金", ["tab_player"] = "玩家", ["tab_vehicles"] = "载具",
        ["tab_business"] = "商业", ["tab_gameplay"] = "玩法", ["tab_staff"] = "员工",
        ["tab_rivals"] = "对手", ["tab_settings"] = "设置",

        // —— 主标题 ——
        ["title"] = "Big Ambitions 修改器",

        // —— Money ——
        ["money_quickadd"] = "快捷增加资金",
        ["money_custom"] = "自定义金额",
        ["money_economy"] = "经济",
        ["money_enteramount"] = "输入金额...",
        ["money_tax"] = "税率 %",
        ["money_pricemult"] = "物价倍率",
        ["money_exportmult"] = "出口倍率",
        ["money_added"] = "已添加资金",
        ["money_setto"] = "资金已设置为",
        ["money_invalid"] = "无效金额",

        // —— Player ——
        ["player_needs"] = "需求与状态",
        ["player_fillall"] = "填满所有需求",
        ["player_energy"] = "体力",
        ["player_level"] = "等级",
        ["player_happiness"] = "心情",
        ["player_hunger"] = "饱食",
        ["player_speed"] = "移动速度",
        ["player_toggles"] = "开关",
        ["player_age"] = "年龄",
        ["player_speed_walk"] = "行走",
        ["player_speed_jog"] = "慢跑",
        ["player_speed_run"] = "奔跑",
        ["player_speed_scooter"] = "滑板车",
        ["player_energydecay"] = "体力衰减",
        ["player_happydecay"] = "心情衰减",
        ["player_hungerdecay"] = "饱食衰减",
        ["player_aging"] = "衰老",
        ["player_completegoals"] = "完成所有个人目标",
        ["player_needfilled"] = "所有需求已填满",
        ["player_years"] = "年",

        // —— Vehicles ——
        ["veh_toggles"] = "开关",
        ["veh_actions"] = "操作",
        ["veh_damage"] = "载具损坏",
        ["veh_fuel"] = "载具油耗",
        ["veh_repair"] = "维修",
        ["veh_refuel"] = "加油",
        ["veh_clean"] = "清洗",
        ["veh_cleartickets"] = "清除罚单",
        ["veh_towgas"] = "拖到加油站",
        ["veh_towrepair"] = "拖到维修厂",

        // —— Business ——
        ["bus_satisfaction"] = "客户满意度",
        ["bus_maxall"] = "最大化所有客户满意度",
        ["bus_unlocks"] = "解锁与开关",
        ["bus_multipliers"] = "商业倍率",
        ["bus_courses"] = "所有课程",
        ["bus_contacts"] = "所有联系人",
        ["bus_importlimits"] = "无进口限制",
        ["bus_importproducts"] = "所有进口商品",
        ["bus_promotion"] = "促销",
        ["bus_salary"] = "薪资",
        ["bus_interest"] = "利率 %",
        ["bus_rivals"] = "对手难度",
        ["bus_wholesalefee"] = "批发费用",
        ["bus_importerfee"] = "进口费用",

        // —— Gameplay ——
        ["game_speed"] = "游戏速度",
        ["game_pause"] = "暂停",
        ["game_time"] = "时间控制",
        ["game_skipnextday"] = "跳过到下一天",
        ["game_settime"] = "设置自定义时间",
        ["game_hour"] = "时 (0-23)",
        ["game_min"] = "分 (0-59)",
        ["game_toggles"] = "开关",
        ["game_traffic"] = "交通",
        ["game_invincible"] = "无敌",
        ["game_tutorial"] = "教程",
        ["game_quests"] = "任务与联系人",
        ["game_completequest"] = "完成任务",
        ["game_completeobj"] = "完成目标",
        ["game_unlockcontacts"] = "解锁所有联系人",
        ["game_teleport"] = "传送",
        ["game_toquest"] = "到任务目标",
        ["game_todest"] = "到目的地",
        ["game_imports"] = "进口交货",
        ["game_deliverpaid"] = "全部交货(付费)",
        ["game_deliverfree"] = "全部交货(免费)",
        ["game_bankinterest"] = "银行利率",
        ["game_save"] = "保存游戏 (TrainerSave)",

        // —— Staff ——
        ["staff_bulk"] = "批量操作",
        ["staff_maxsatisfaction"] = "最大化所有员工满意度",
        ["staff_salarymult"] = "薪资倍率",
        ["staff_free"] = "免费",
        ["staff_setwages"] = "设置薪资",
        ["staff_enterwage"] = "例如 15.00",
        ["staff_setallwages"] = "设置所有薪资",
        ["staff_candidates"] = "招聘候选人",
        ["staff_skilllevel"] = "技能等级 (1-100)",
        ["staff_custservice"] = "客服",
        ["staff_cleaning"] = "清洁",
        ["staff_lawyer"] = "律师",
        ["staff_purchasing"] = "采购",
        ["staff_logistics"] = "物流",
        ["staff_delivery"] = "配送",
        ["staff_programmer"] = "程序员",
        ["staff_hr"] = "人事经理",

        // —— Rivals ——
        ["rival_actions"] = "对手操作",
        ["rival_refresh"] = "刷新对手机数据",
        ["rival_defeatall"] = "击败所有对手",
        ["rival_difficulty"] = "对手难度",
        ["rival_easy"] = "容易 (0.5x)",
        ["rival_normal"] = "正常 (1x)",
        ["rival_hard"] = "困难 (2x)",
        ["rival_brutal"] = "残酷 (5x)",

        // —— Settings ——
        ["set_settings"] = "设置",
        ["set_saveall"] = "保存所有设置",
        ["set_loadall"] = "读取所有设置",
        ["set_integration"] = "集成",
        ["set_showinphone"] = "在手机中显示修改器",
        ["set_about"] = "关于",
        ["set_language"] = "语言 / Language",
        ["set_close"] = "关闭面板",
        ["set_presshint"] = "按 F8 切换   |   按 ESC 关闭",
        ["set_lang_zh"] = "简体中文",
        ["set_lang_en"] = "English",
        // —— 主题 / 外观 ——
        ["set_appearance"] = "外观定制",
        ["set_theme"] = "主题",
        ["set_reset_colors"] = "恢复默认配色",
        ["toast_color_reset"] = "配色已恢复默认",
        ["set_layout"] = "布局",
        ["set_opacity"] = "面板不透明度",
        ["set_scale"] = "界面缩放",
        ["set_reset_pos"] = "重置面板位置",

        // —— Toast ——
        ["toast_saved"] = "设置已保存",
        ["toast_loaded"] = "设置已读取",
        ["toast_speed"] = "游戏速度",
        ["toast_paused"] = "已暂停",
        ["toast_skipped"] = "已跳到下一天",
        ["toast_questdone"] = "任务已完成",
        ["toast_objdone"] = "目标已完成",
        ["toast_contactunlocked"] = "联系人已解锁",
        ["toast_delivered"] = "进口已交货",
        ["toast_gamesaved"] = "游戏已保存",
        ["toast_invalidwage"] = "无效薪资金额",
        ["toast_invalidtime"] = "无效时间 (时0-23,分0-59)",
        ["toast_repaired"] = "载具已维修",
        ["toast_refueled"] = "载具已加油",
        ["toast_cleaned"] = "载具已清洗",
        ["toast_tickets"] = "罚单已清除",
        ["toast_towing"] = "拖车中...",
    };

    private static readonly Dictionary<string, string> en = new Dictionary<string, string>
    {
        ["on"] = "ON", ["off"] = "OFF",
        ["add"] = "Add", ["set"] = "Set", ["none"] = "None",
        ["confirm"] = "Confirm", ["cancel"] = "Cancel",
        ["success"] = "Success", ["error"] = "Error",

        ["tab_money"] = "Money", ["tab_player"] = "Player", ["tab_vehicles"] = "Vehicles",
        ["tab_business"] = "Business", ["tab_gameplay"] = "Gameplay", ["tab_staff"] = "Staff",
        ["tab_rivals"] = "Rivals", ["tab_settings"] = "Settings",

        ["title"] = "Big Ambitions Trainer",

        ["money_quickadd"] = "QUICK ADD MONEY",
        ["money_custom"] = "CUSTOM MONEY",
        ["money_economy"] = "ECONOMY",
        ["money_enteramount"] = "Enter amount...",
        ["money_tax"] = "Tax %",
        ["money_pricemult"] = "Price Mult",
        ["money_exportmult"] = "Export Mult",
        ["money_added"] = "Added",
        ["money_setto"] = "Money set to",
        ["money_invalid"] = "Invalid amount",

        ["player_needs"] = "NEEDS & STATS",
        ["player_fillall"] = "Fill All Needs",
        ["player_energy"] = "ENERGY",
        ["player_level"] = "Level",
        ["player_happiness"] = "HAPPINESS",
        ["player_hunger"] = "HUNGER",
        ["player_speed"] = "MOVEMENT SPEED",
        ["player_toggles"] = "TOGGLES",
        ["player_age"] = "AGE",
        ["player_speed_walk"] = "Walk",
        ["player_speed_jog"] = "Jog",
        ["player_speed_run"] = "Run",
        ["player_speed_scooter"] = "Scooter",
        ["player_energydecay"] = "Energy Decay",
        ["player_happydecay"] = "Happy Decay",
        ["player_hungerdecay"] = "Hunger Decay",
        ["player_aging"] = "Aging",
        ["player_completegoals"] = "Complete All Personal Goals",
        ["player_needfilled"] = "All needs filled!",
        ["player_years"] = "yrs",

        ["veh_toggles"] = "TOGGLES",
        ["veh_actions"] = "ACTIONS",
        ["veh_damage"] = "Vehicle Damage",
        ["veh_fuel"] = "Vehicle Fuel",
        ["veh_repair"] = "Repair",
        ["veh_refuel"] = "Refuel",
        ["veh_clean"] = "Clean",
        ["veh_cleartickets"] = "Clear Tickets",
        ["veh_towgas"] = "Tow to Gas",
        ["veh_towrepair"] = "Tow to Repair",

        ["bus_satisfaction"] = "CUSTOMER SATISFACTION",
        ["bus_maxall"] = "Max All Customer Satisfaction",
        ["bus_unlocks"] = "UNLOCKS & TOGGLES",
        ["bus_multipliers"] = "BUSINESS MULTIPLIERS",
        ["bus_courses"] = "All Courses",
        ["bus_contacts"] = "All Contacts",
        ["bus_importlimits"] = "No Import Limits",
        ["bus_importproducts"] = "All Import Products",
        ["bus_promotion"] = "Promotion",
        ["bus_salary"] = "Salary",
        ["bus_interest"] = "Interest %",
        ["bus_rivals"] = "Rivals",
        ["bus_wholesalefee"] = "Wholesale Fee",
        ["bus_importerfee"] = "Importer Fee",

        ["game_speed"] = "GAME SPEED",
        ["game_pause"] = "Pause",
        ["game_time"] = "TIME CONTROLS",
        ["game_skipnextday"] = "Skip to Next Day",
        ["game_settime"] = "SET CUSTOM TIME",
        ["game_hour"] = "Hour (0-23)",
        ["game_min"] = "Min (0-59)",
        ["game_toggles"] = "TOGGLES",
        ["game_traffic"] = "Traffic",
        ["game_invincible"] = "Invincibility",
        ["game_tutorial"] = "Tutorial",
        ["game_quests"] = "QUESTS & CONTACTS",
        ["game_completequest"] = "Complete Quest",
        ["game_completeobj"] = "Complete Objective",
        ["game_unlockcontacts"] = "Unlock All Contacts",
        ["game_teleport"] = "TELEPORTATION",
        ["game_toquest"] = "To Quest Target",
        ["game_todest"] = "To Destination",
        ["game_imports"] = "IMPORT DELIVERIES",
        ["game_deliverpaid"] = "Deliver All (Paid)",
        ["game_deliverfree"] = "Deliver All (Free)",
        ["game_bankinterest"] = "Bank Interest",
        ["game_save"] = "Save Game (TrainerSave)",

        ["staff_bulk"] = "BULK ACTIONS",
        ["staff_maxsatisfaction"] = "Max ALL Employee Satisfaction",
        ["staff_salarymult"] = "SALARY MULTIPLIER",
        ["staff_free"] = "Free",
        ["staff_setwages"] = "SET WAGES",
        ["staff_enterwage"] = "e.g. 15.00",
        ["staff_setallwages"] = "Set All Wages",
        ["staff_candidates"] = "RECRUITMENT CANDIDATES",
        ["staff_skilllevel"] = "Skill Level (1-100)",
        ["staff_custservice"] = "CustService",
        ["staff_cleaning"] = "Cleaning",
        ["staff_lawyer"] = "Lawyer",
        ["staff_purchasing"] = "Purchasing",
        ["staff_logistics"] = "Logistics",
        ["staff_delivery"] = "Delivery",
        ["staff_programmer"] = "Programmer",
        ["staff_hr"] = "HR Manager",

        ["rival_actions"] = "RIVAL ACTIONS",
        ["rival_refresh"] = "Refresh Rivals Data",
        ["rival_defeatall"] = "Defeat ALL Rivals",
        ["rival_difficulty"] = "RIVALS DIFFICULTY",
        ["rival_easy"] = "Easy (0.5x)",
        ["rival_normal"] = "Normal (1x)",
        ["rival_hard"] = "Hard (2x)",
        ["rival_brutal"] = "Brutal (5x)",

        ["set_settings"] = "SETTINGS",
        ["set_saveall"] = "Save All Settings",
        ["set_loadall"] = "Load All Settings",
        ["set_integration"] = "INTEGRATION",
        ["set_showinphone"] = "Show Trainer in Phone",
        ["set_about"] = "ABOUT",
        ["set_language"] = "Language",
        ["set_close"] = "Close Overlay",
        ["set_presshint"] = "Press F8 to toggle  |  Press ESC to close",
        ["set_lang_zh"] = "简体中文 (中文)",
        ["set_lang_en"] = "English (English)",
        // —— Theme / Appearance ——
        ["set_appearance"] = "APPEARANCE",
        ["set_theme"] = "Theme",
        ["set_reset_colors"] = "Reset Colors",
        ["toast_color_reset"] = "Colors reset",
        ["set_layout"] = "LAYOUT",
        ["set_opacity"] = "Panel Opacity",
        ["set_scale"] = "UI Scale",
        ["set_reset_pos"] = "Reset Panel Position",

        ["toast_saved"] = "Settings saved!",
        ["toast_loaded"] = "Settings loaded!",
        ["toast_speed"] = "Speed",
        ["toast_paused"] = "Paused",
        ["toast_skipped"] = "Skipped to next day",
        ["toast_questdone"] = "Quest completed!",
        ["toast_objdone"] = "Objective completed!",
        ["toast_contactunlocked"] = "Contacts unlocked!",
        ["toast_delivered"] = "Imports delivered",
        ["toast_gamesaved"] = "Game saved!",
        ["toast_invalidwage"] = "Invalid wage amount",
        ["toast_invalidtime"] = "Invalid time (hour 0-23, min 0-59)",
        ["toast_repaired"] = "Vehicle repaired!",
        ["toast_refueled"] = "Vehicle refueled!",
        ["toast_cleaned"] = "Vehicle cleaned!",
        ["toast_tickets"] = "Tickets cleared!",
        ["toast_towing"] = "Towing...",
    };

    /// <summary>当前语言（默认中文）。</summary>
    public static string Current = LangZh;

    public static bool IsChinese => Current == LangZh;

    /// <summary>按 key 取当前语言文本；缺失时回退为 key。</summary>
    public static string T(string key)
    {
        var dict = IsChinese ? zh : en;
        string v;
        return dict.TryGetValue(key, out v) ? v : key;
    }

    /// <summary>强制按指定语言取文本。</summary>
    public static string T(string key, string lang)
    {
        var dict = lang == LangEn ? en : zh;
        string v;
        return dict.TryGetValue(key, out v) ? v : key;
    }
}
