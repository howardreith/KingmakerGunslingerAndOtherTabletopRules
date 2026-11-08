using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Repair R6 (PRES-05, PRES-06): one native sound event when this ability
    /// actually executes. The native on-cast hook runs once at the cast point,
    /// after a cancelled command would already have stopped and before any
    /// target is resolved, so a blast hitting many creatures still gets one
    /// cue. The cue is posted on the caster's own view, so the sound engine
    /// releases it with that view. Presentation only: no mechanics or cost.
    /// </summary>
    internal sealed class SummonAbilityCastCue : BlueprintComponent, IAbilityOnCastLogic
    {
        public string EventName;

        public void OnCast(AbilityExecutionContext context)
        {
            try { SummonAbilityCastCueRuntime.Play(EventName, context); }
            catch (Exception error)
            {
                // A sound never interrupts the ability it decorates.
                Debug.LogException(error);
            }
        }
    }

    internal sealed class SummonCastCueRecord
    {
        internal string EventName;
        internal string Ability;
        internal string CasterId;
        internal DateTime Utc;
        internal int Frame;
        internal uint PlayingId;
    }

    internal static class SummonAbilityCastCueRuntime
    {
        private const int MaxRecords = 64;
        private static readonly object Played = new object();
        private static readonly ConditionalWeakTable<AbilityExecutionContext, object> Contexts =
            new ConditionalWeakTable<AbilityExecutionContext, object>();
        private static readonly List<SummonCastCueRecord> Recent = new List<SummonCastCueRecord>();
        private static bool s_BankRequested;

        /// <summary>Recent cues, newest last, as read-only evidence.</summary>
        internal static SummonCastCueRecord[] Snapshot()
        { lock (Recent) return Recent.ToArray(); }

        internal static void Play(string eventName, AbilityExecutionContext context)
        {
            UnitEntityData caster = context == null ? null : context.MaybeCaster;
            if (string.IsNullOrEmpty(eventName) || context == null || caster == null ||
                caster.View == null || !AkSoundEngine.IsInitialized()) return;
            lock (Recent)
            {
                object seen;
                if (Contexts.TryGetValue(context, out seen)) return;
                Contexts.Add(context, Played);
            }
            if (!s_BankRequested)
            {
                s_BankRequested = true;
                AkBankManager.LoadBank(SummonPlaytestR6Policy.CueBank, false, false);
            }
            uint id = AkSoundEngine.PostEvent(eventName, caster.View.gameObject);
            lock (Recent)
            {
                Recent.Add(new SummonCastCueRecord { EventName = eventName,
                    Ability = context.AbilityBlueprint == null ? null : context.AbilityBlueprint.name,
                    CasterId = caster.UniqueId, Utc = DateTime.UtcNow, Frame = Time.frameCount, PlayingId = id });
                if (Recent.Count > MaxRecords) Recent.RemoveAt(0);
            }
        }
    }
}
