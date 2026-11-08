using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.View;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Repair R6 (PRES-05, PRES-06): one native sound event when this ability
    /// actually executes. The native on-cast hook runs once at the cast point,
    /// after a cancelled command would already have stopped and before any
    /// target is resolved, so a blast hitting many creatures still gets one
    /// cue. Presentation only: no mechanics or cost.
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

    /// <summary>
    /// Tracks this view's cast cues until their end-of-event callback. If the
    /// caster retires first, each unfinished one is stopped by its own playing
    /// ID, which does not need the emitter to remain registered (the native
    /// registration can already be gone when this view is destroyed).
    /// </summary>
    internal sealed class SummonCastCuePlayback : MonoBehaviour
    {
        private readonly List<SummonCastCueRecord> _active = new List<SummonCastCueRecord>();
        private UnitEntityView _view;

        internal void Track(UnitEntityView view, SummonCastCueRecord record)
        {
            _view = view; _active.Add(record); enabled = true;
        }

        private void LateUpdate()
        {
            _active.RemoveAll(value => value.EndUtc.HasValue);
            if (_active.Count == 0) { enabled = false; return; }
            UnitEntityData unit = _view == null ? null : _view.EntityData;
            if (unit == null || unit.Destroyed || !unit.IsInState) Retire("entity-retired");
        }

        private void OnDestroy() { Retire("view-destroyed"); }

        private void Retire(string reason)
        {
            SummonCastCueLifecycle.Retire(_active, reason, () => DateTime.UtcNow, id =>
            {
                if (!AkSoundEngine.IsInitialized()) throw new InvalidOperationException("Sound engine is not initialized.");
                AkSoundEngine.StopPlayingID(id);
            });
        }
    }

    internal static class SummonAbilityCastCueRuntime
    {
        private const int MaxRecords = 64;
        private static readonly object Played = new object();
        private static readonly ConditionalWeakTable<AbilityExecutionContext, object> Contexts =
            new ConditionalWeakTable<AbilityExecutionContext, object>();
        private static readonly List<SummonCastCueRecord> Recent = new List<SummonCastCueRecord>();
        private static readonly AkCallbackManager.EventCallback Ended = OnEnded;
        private static bool s_BankRequested;

        /// <summary>Recent cues, newest last, as read-only evidence.</summary>
        internal static SummonCastCueRecord[] Snapshot()
        { lock (Recent) return Recent.ToArray(); }

        internal static void Play(string eventName, AbilityExecutionContext context)
        {
            UnitEntityData caster = context == null ? null : context.MaybeCaster;
            UnitEntityView view = caster == null ? null : caster.View;
            if (string.IsNullOrEmpty(eventName) || context == null || view == null || !AkSoundEngine.IsInitialized()) return;
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
            var record = new SummonCastCueRecord { EventName = eventName,
                Ability = context.AbilityBlueprint == null ? null : context.AbilityBlueprint.name,
                CasterId = caster.UniqueId, Utc = DateTime.UtcNow, Frame = Time.frameCount };
            record.PlayingId = AkSoundEngine.PostEvent(eventName, view.gameObject,
                (uint)AkCallbackType.AK_EndOfEvent, Ended, record);
            if (record.PlayingId != 0)
            {
                SummonCastCuePlayback playback = view.GetComponent<SummonCastCuePlayback>();
                if (playback == null) playback = view.gameObject.AddComponent<SummonCastCuePlayback>();
                playback.Track(view, record);
            }
            lock (Recent)
            {
                Recent.Add(record);
                if (Recent.Count > MaxRecords) Recent.RemoveAt(0);
            }
        }

        private static void OnEnded(object cookie, AkCallbackType type, object info)
        {
            var record = cookie as SummonCastCueRecord;
            if (record == null || record.EndUtc.HasValue) return;
            record.EndUtc = DateTime.UtcNow; record.EndFrame = Time.frameCount;
        }
    }
}
