using System;
using System.Collections.Generic;
using System.Reflection;
using BigAmbitions.Rivals;
using MelonLoader;
using UnityEngine;

namespace BigAmbitionsTrainer.Modules;

/// <summary>
/// 对手管理。1.0 中 RivalsHelper.RivalDataCache 为私有静态字段，
/// 通过反射读取以遍历对手列表；击败使用公共方法 RivalsHelper.DefeatRival。
/// </summary>
public static class RivalsModule
{
    public class RivalInfo
    {
        public string Name;
        public string Id;
        public bool Defeated;
        public string Neighbourhood;
        public int Buildings;
        public int Businesses;
        public float WeeklyIncome;
        internal RivalData _data;
    }

    public static List<RivalInfo> Rivals { get; private set; } = new List<RivalInfo>();
    public static int SelectedRivalIndex { get; set; }
    private static Dictionary<string, RivalData> _cacheCache;

    public static void Initialize()
    {
        MelonLogger.Msg("[RivalsModule] Initialized.");
    }

    public static void OnUpdate()
    {
        // 无需每帧轮询
    }

    /// <summary>通过反射读取 RivalsHelper 的私有 RivalDataCache。</summary>
    private static Dictionary<string, RivalData> GetCache()
    {
        try
        {
            if (_cacheCache != null) return _cacheCache;
            var f = typeof(RivalsHelper).GetField("RivalDataCache",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (f == null) return null;
            _cacheCache = f.GetValue(null) as Dictionary<string, RivalData>;
            return _cacheCache;
        }
        catch (Exception ex)
        {
            MelonLogger.Warning("[Rivals] Cache reflection error: " + ex.Message);
            return null;
        }
    }

    public static void RefreshRivals()
    {
        try
        {
            Rivals.Clear();
            var cache = GetCache();
            if (cache != null)
            {
                foreach (var kv in cache)
                {
                    try
                    {
                        var value = kv.Value;
                        if (value == null) continue;
                        var info = new RivalInfo { _data = value };
                        try { info.Name = value.rivalName; } catch { info.Name = "Unknown"; }
                        try { info.Id = value.id; } catch { info.Id = ""; }
                        try { info.Defeated = RivalsHelper.IsRivalDefeated(value.id); } catch { }
                        try { info.Neighbourhood = Convert.ToString(value.MostActiveNeighborhood); } catch { info.Neighbourhood = "?"; }
                        try { info.Buildings = value.ownedBuildings != null ? value.ownedBuildings.Count : 0; } catch { }
                        try { info.Businesses = value.ownedBusinesses != null ? value.ownedBusinesses.Count : 0; } catch { }
                        try { info.WeeklyIncome = value.WeeklyIncome; } catch { }
                        Rivals.Add(info);
                    }
                    catch { }
                }
            }
            _cacheCache = null; // 下次刷新重新读取
            if (SelectedRivalIndex >= Rivals.Count) SelectedRivalIndex = Math.Max(0, Rivals.Count - 1);
            MelonLogger.Msg("[Rivals] Found " + Rivals.Count + " rivals.");
        }
        catch (Exception ex) { MelonLogger.Warning("[Rivals] Refresh error: " + ex.Message); }
    }

    public static bool DefeatSelectedRival()
    {
        try
        {
            if (SelectedRivalIndex < 0 || SelectedRivalIndex >= Rivals.Count) return false;
            var info = Rivals[SelectedRivalIndex];
            if (info._data == null) return false;
            try { RivalsHelper.DefeatRival(info._data); return true; }
            catch (Exception ex) { MelonLogger.Warning("[Rivals] Defeat error: " + ex.Message); return false; }
        }
        catch { return false; }
    }

    public static int DefeatAllRivals()
    {
        try
        {
            if (Rivals.Count == 0) return 0;
            int count = 0;
            foreach (var info in Rivals)
            {
                try
                {
                    if (info._data == null) continue;
                    RivalsHelper.DefeatRival(info._data);
                    count++;
                }
                catch { }
            }
            RefreshRivals();
            return count;
        }
        catch (Exception ex) { MelonLogger.Warning("[Rivals] Defeat all error: " + ex.Message); return 0; }
    }
}
