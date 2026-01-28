/// 📦 MizuLog
/// 📣 Simple Editor-only debug log with GROUP support
///
/// 🧪 Usage:
///     MizuLog.Combat("🎯 Hit enemy");
///     MizuLog.UI("🖼️ Panel opened");
///
/// 🔧 To disable a group:
///     MizuLog.Disable("Combat");
///     MizuLog.Enable("Combat");
///     MizuLog.DisableAll();
///     MizuLog.EnableAll();
///
/// 💡 Only logs in Unity Editor


using System.Diagnostics;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

public static class MizuLog
{
    // ═══════════════════════════════════════════════════════
    // 🔧 TOGGLE GROUPS - Remove from list to disable
    // ═══════════════════════════════════════════════════════
    private static HashSet<string> enabledGroups = new HashSet<string>
    {
        "Combat",
        "UI",
        "Audio",
        "Spawn",
        "General",
        "Save",
        "Movement",
        "EventAndAds"
    };
    // ═══════════════════════════════════════════════════════

    // 🔧 Control methods
    public static void Enable(string group) => enabledGroups.Add(group);
    public static void Disable(string group) => enabledGroups.Remove(group);
    public static void EnableAll() => enabledGroups = new HashSet<string> { "Combat", "UI", "Audio", "Spawn", "General", "Save", "Movement", "EventAndAds" };
    public static void DisableAll() => enabledGroups.Clear();

    // 📣 Log groups
    public static void Combat(string msg) => Log(msg, "Combat", "⚔️");
    public static void UI(string msg) => Log(msg, "UI", "🖼️");
    public static void Audio(string msg) => Log(msg, "Audio", "🔊");
    public static void Spawn(string msg) => Log(msg, "Spawn", "👾");
    public static void General(string msg) => Log(msg, "General", "📝");
    public static void Save(string msg) => Log(msg, "Save", "💾");
    public static void Movement(string msg) => Log(msg, "Movement", "🏃");
    public static void EventAndAds(string msg) => Log(msg, "EventAndAds", "📺");

    // 🔧 Core
    private static void Log(string message, string group, string icon)
    {
    #if UNITY_EDITOR
        if (!enabledGroups.Contains(group)) return;

        var frame = new StackTrace(true).GetFrame(2);
        if (frame != null)
        {
            var method = frame.GetMethod();
            string file = System.IO.Path.GetFileName(frame.GetFileName());
            Debug.Log($"[{icon} {group}] {message} → {file} → {method.DeclaringType?.Name}.{method.Name}()");
        }
        else
        {
            Debug.Log($"[{icon} {group}] {message}");
        }
    #endif
    }
}