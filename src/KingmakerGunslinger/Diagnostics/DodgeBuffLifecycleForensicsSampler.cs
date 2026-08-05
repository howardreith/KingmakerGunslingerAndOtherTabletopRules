using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Bootstrap;
using UnityModManagerNet;

namespace KingmakerGunslinger.Diagnostics
{
    internal static class DodgeBuffLifecycleForensicsSampler
    {
        private static double _elapsed;
        private static readonly Dictionary<string, string> Last = new Dictionary<string, string>();
        internal static void Attach(ModContext context) { context.ModEntry.OnUpdate += Update; }
        private static void Update(UnityModManager.ModEntry ignored, float deltaTime)
        {
            if (!DodgeBuffLifecycleForensics.Enabled) return;
            _elapsed += deltaTime;
            if (_elapsed < 0.25d) return;
            bool heartbeat = _elapsed >= 1d;
            if (heartbeat) _elapsed = 0d; else _elapsed -= 0.25d;
            try
            {
                if (Game.Instance == null || Game.Instance.Player == null) return;
                foreach (var unit in Game.Instance.Player.Party.Where(value => value != null && value.Descriptor != null))
                {
                    var collection = unit.Descriptor.Buffs;
                    Buff[] buffs = collection.Enumerable.Where(value => value != null &&
                        DodgeBuffLifecycleForensics.IsExact(value.Blueprint)).ToArray();
                    string key = unit.UniqueId;
                    string state = string.Join("|", buffs.Select(value =>
                        System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value) + ":" +
                        value.EndTime.Ticks + ":" + value.TimeLeft.Ticks + ":" + value.Active));
                    string prior; bool changed = !Last.TryGetValue(key, out prior) || prior != state;
                    Last[key] = state;
                    if (!changed && !heartbeat) continue;
                    if (buffs.Length == 0)
                        DodgeBuffLifecycleForensics.Record("sample", null, collection, null, null, null);
                    else foreach (Buff buff in buffs)
                        DodgeBuffLifecycleForensics.Record("sample", buff, collection, null, null, null);
                }
            }
            catch (Exception exception)
            { DodgeBuffLifecycleForensics.Record("sampler-exception", null, null, null, exception, null); }
        }
    }
}
