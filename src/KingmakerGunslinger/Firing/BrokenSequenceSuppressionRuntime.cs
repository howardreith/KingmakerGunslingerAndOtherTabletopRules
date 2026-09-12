using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Runtime bookkeeping for the newly-Broken sequence interruption. Per
    /// (wielder, exact weapon) it counts committed degradations and holds the
    /// interrupted-sequence suppression. Suppression is released ONLY by a
    /// genuine new player attack order for that exact executor and target —
    /// never by time, weapon repair, reload, or automatic target changes —
    /// so a repaired firearm is usable again for deliberate orders while the
    /// cancelled order's automatic continuations stay cancelled.
    /// Player-attack authorization is scoped to the verified order itself:
    /// when the native player unit-click handler starts, authorization is
    /// recorded for each currently selected unit against the clicked target
    /// only where that target is actually attackable; the authorization is
    /// one-shot per executor, requires the exact (executor, target) pair at
    /// consumption, and is cleared when the click handler returns normally
    /// (a leftover from a faulting handler additionally expires with its
    /// engine frame). All state is process-memory only and weakly keyed, so
    /// scene transitions and save/load cannot leak it onto unrelated future
    /// commands.
    /// </summary>
    internal static class BrokenSequenceSuppressionRuntime
    {
        private static readonly object Gate = new object();
        private static ConditionalWeakTable<object, Dictionary<object, Entry>>
            _suppressions = new ConditionalWeakTable<object, Dictionary<object, Entry>>();
        private static readonly Dictionary<object, PlayerAttackAuthorization>
            _playerAttackAuthorizations =
                new Dictionary<object, PlayerAttackAuthorization>();

        internal sealed class Entry
        {
            internal int Epoch;
            internal bool Suppressed;
        }

        internal sealed class PlayerAttackAuthorization
        {
            internal object Executor;
            internal object Target;
            internal int Frame;
            // Bridge records exist only through the guarded runtime-test
            // seam; production records additionally require the genuine
            // native click handler to still be on the consuming call stack.
            internal bool Bridge;
        }

        /// <summary>
        /// Shared notification for every verified committed degradation of an
        /// exact firearm during an attack sequence: the ordinary misfire
        /// path, Dead Shot, and Scatter Shot all call this after their
        /// guarded transition commits and verifies. A rolled-back or
        /// prevented break must never reach this method.
        /// </summary>
        internal static void OnCommittedDegradation(object wielder, object weapon)
        {
            if (wielder == null || weapon == null)
            {
                return;
            }

            lock (Gate)
            {
                Entry entry = GetOrCreate(wielder, weapon);
                entry.Epoch = checked(entry.Epoch + 1);
                entry.Suppressed = true;
            }
        }

        internal static bool IsSuppressed(object wielder, object weapon)
        {
            lock (Gate)
            {
                Entry entry = Find(wielder, weapon);
                return entry != null && entry.Suppressed;
            }
        }

        internal static int GetDegradationEpoch(object wielder, object weapon)
        {
            lock (Gate)
            {
                Entry entry = Find(wielder, weapon);
                return entry == null ? 0 : entry.Epoch;
            }
        }

        internal static void ConsumeSuppression(object wielder, object weapon)
        {
            lock (Gate)
            {
                Entry entry = Find(wielder, weapon);
                if (entry != null)
                {
                    entry.Suppressed = false;
                }
            }
        }

        /// <summary>
        /// Called when the native player unit-click handler starts: records
        /// one-shot player-attack authorization for each currently selected
        /// unit against the clicked unit, but only for pairs the native
        /// attack path itself would accept (the clicked unit is attackable
        /// by that unit). Interaction-only clicks therefore record nothing.
        /// </summary>
        internal static void BeginPlayerAttackClick(
            Kingmaker.EntitySystem.Entities.UnitEntityData clickedUnit)
        {
            lock (Gate)
            {
                _playerAttackAuthorizations.Clear();
                if (clickedUnit == null)
                {
                    return;
                }

                Kingmaker.Game game = Kingmaker.Game.Instance;
                SelectionManagerBase selection =
                    game == null || game.UI == null
                        ? null
                        : game.UI.SelectionManager;
                List<Kingmaker.EntitySystem.Entities.UnitEntityData> selected =
                    selection == null ? null : selection.SelectedUnits;
                if (selected == null)
                {
                    return;
                }

                int frame = CurrentFrame();
                foreach (Kingmaker.EntitySystem.Entities.UnitEntityData unit in selected)
                {
                    if (unit == null || unit.Descriptor == null ||
                        !clickedUnit.CanAttack(unit))
                    {
                        continue;
                    }

                    _playerAttackAuthorizations[unit] =
                        new PlayerAttackAuthorization
                        {
                            Executor = unit,
                            Target = clickedUnit,
                            Frame = frame
                        };
                }
            }
        }

        /// <summary>
        /// Called when the native player unit-click handler returns
        /// normally: every recorded authorization is discarded, so nothing
        /// later in the same frame can inherit the click.
        /// </summary>
        internal static void EndPlayerAttackClick()
        {
            lock (Gate)
            {
                _playerAttackAuthorizations.Clear();
            }
        }

        /// <summary>
        /// Consumes the player-attack authorization for an attack-command
        /// construction only when it exactly matches the genuine order: the
        /// same executor and the same clicked target, recorded by the
        /// still-current click, one use only. A production authorization is
        /// consumable only while the genuine native click handler is still
        /// on the consuming call stack, so a leftover from a faulting
        /// handler can never be inherited by later automatic work even in
        /// the same frame (review CR2-04). A wrong-target query returns
        /// false WITHOUT deleting the executor's authorization for its
        /// legitimate target; only successful consumption or frame expiry
        /// removes a record.
        /// </summary>
        internal static bool TryConsumePlayerAttackAuthorization(
            object executor,
            object target)
        {
            lock (Gate)
            {
                PlayerAttackAuthorization authorization;
                if (executor == null ||
                    !_playerAttackAuthorizations.TryGetValue(
                        executor, out authorization))
                {
                    return false;
                }

                if (authorization.Frame != CurrentFrame())
                {
                    _playerAttackAuthorizations.Remove(executor);
                    return false;
                }

                if (!ReferenceEquals(authorization.Target, target))
                {
                    return false;
                }

                if (!authorization.Bridge &&
                    !IsNativePlayerClickOnStack())
                {
                    _playerAttackAuthorizations.Remove(executor);
                    return false;
                }

                _playerAttackAuthorizations.Remove(executor);
                return true;
            }
        }

        /// <summary>
        /// Proves the genuine native player unit-click handler is still
        /// executing on this call stack: the only construction authorized
        /// to consume a production player-attack order is one performed by
        /// that handler itself.
        /// </summary>
        private static bool IsNativePlayerClickOnStack()
        {
            System.Diagnostics.StackTrace stack =
                new System.Diagnostics.StackTrace(2, false);
            for (int index = 0; index < stack.FrameCount; index++)
            {
                System.Reflection.MethodBase method =
                    stack.GetFrame(index).GetMethod();
                System.Type declaring = method == null
                    ? null
                    : method.DeclaringType;
                if (declaring != null &&
                    declaring.FullName ==
                        "Kingmaker.Controllers.Clicks.Handlers.ClickUnitHandler")
                {
                    return true;
                }
            }

            return false;
        }

        internal static int CurrentFrame()
        {
            return UnityEngine.Time.frameCount;
        }

        /// <summary>
        /// Guarded runtime-test seam only: records one player-attack
        /// authorization exactly as the verified click path would for an
        /// already-selected, attackable pair. Ordinary play never calls it;
        /// scenarios using it must label the assertion as a bridge test and
        /// rely on the native interactive lanes for production input proof.
        /// </summary>
        internal static void AuthorizePlayerAttackOrderForRuntimeTest(
            object executor,
            object target)
        {
            lock (Gate)
            {
                if (executor == null || target == null)
                {
                    return;
                }

                _playerAttackAuthorizations[executor] =
                    new PlayerAttackAuthorization
                    {
                        Executor = executor,
                        Target = target,
                        Frame = CurrentFrame(),
                        Bridge = true
                    };
            }
        }

        /// <summary>
        /// Guarded runtime-test seam only: resets all interruption bookkeeping.
        /// Ordinary play never calls it.
        /// </summary>
        internal static void ClearForRuntimeTest()
        {
            lock (Gate)
            {
                _suppressions =
                    new ConditionalWeakTable<object, Dictionary<object, Entry>>();
                _playerAttackAuthorizations.Clear();
            }
        }

        private static Entry Find(object wielder, object weapon)
        {
            if (wielder == null || weapon == null)
            {
                return null;
            }

            Dictionary<object, Entry> perWeapon;
            if (!_suppressions.TryGetValue(wielder, out perWeapon))
            {
                return null;
            }

            Entry entry;
            return perWeapon.TryGetValue(weapon, out entry) ? entry : null;
        }

        private static Entry GetOrCreate(object wielder, object weapon)
        {
            Dictionary<object, Entry> perWeapon;
            if (!_suppressions.TryGetValue(wielder, out perWeapon))
            {
                perWeapon = new Dictionary<object, Entry>();
                _suppressions.Add(wielder, perWeapon);
            }

            Entry entry;
            if (!perWeapon.TryGetValue(weapon, out entry))
            {
                entry = new Entry();
                perWeapon.Add(weapon, entry);
            }

            return entry;
        }
    }
}
