using System;
using Helpers;

namespace BigAmbitionsTrainer.Modules;

/// <summary>
/// Mono 环境下的单例获取辅助。
/// 游戏类型的单例模式通常是 T : InstanceBehavior&lt;T&gt;（HGExtensions 提供），
/// 直接用 InstanceBehavior&lt;T&gt;.Instance 获取单例（与游戏内部一致）。
/// </summary>
public static class GameRef
{
    /// <summary>获取继承 InstanceBehavior&lt;T&gt; 的组件单例。找不到返回 null。</summary>
    public static T Get<T>() where T : InstanceBehavior<T>
    {
        try { return InstanceBehavior<T>.Instance; }
        catch (Exception) { return null; }
    }
}
