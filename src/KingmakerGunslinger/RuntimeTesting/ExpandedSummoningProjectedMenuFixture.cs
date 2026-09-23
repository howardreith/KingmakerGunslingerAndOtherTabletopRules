using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Opens the real variant menu at the roster's projected worst-case size and
    /// measures it. Development-only.
    ///
    /// The shipped observation of this menu is supervised: it watches for a menu
    /// a human opened. That is the right shape for confirming what a player
    /// actually saw, and the wrong shape for a scalability gate, because it can
    /// only ever report on the list that exists today. The charter's roster
    /// grows the largest Summon Monster parent to 120 entries and the largest
    /// Summon Nature's Ally parent to 110, and the question that has to be
    /// answered before those entries are built is whether the widget survives
    /// them.
    ///
    /// So this drives the menu itself, with a projected list, and measures what
    /// a player would experience: whether every entry is reachable, whether the
    /// popup stays inside the safe area, whether scrolling appears exactly when
    /// the content needs it, how long the open takes, and whether repeated
    /// open/close cycles leak slots or managed memory.
    ///
    /// Nothing is published. The projected entries are transient
    /// <see cref="AbilityData"/> built from blueprints that are already
    /// published, held only for the duration of one measurement and never
    /// written to a parent, a spellbook, a save, or an action bar binding. No
    /// production path constructs this class: the guarded runtime-test runner
    /// does, for one allowlisted disposable scenario, and nothing else
    /// references it.
    /// </summary>
    internal sealed class ExpandedSummoningProjectedMenuFixture
    {
        /// <summary>
        /// Cycles per projected size. One open proves it renders; three prove it
        /// does not accumulate.
        /// </summary>
        internal const int Cycles = 3;

        /// <summary>Frames allowed for the layout to settle before measuring.</summary>
        private const int SettleFrames = 3;

        private readonly UnitEntityData _unit;
        private readonly List<Measurement> _measurements = new List<Measurement>();

        internal ExpandedSummoningProjectedMenuFixture(UnitEntityData unit)
        {
            if (unit == null) throw new ArgumentNullException("unit");
            _unit = unit;
        }

        internal sealed class Measurement
        {
            internal SummonFamily Family;
            internal int Projected;
            internal int Cycle;
            internal bool Rendered;
            internal int SlotCount;
            internal bool FirstReachable;
            internal bool MiddleReachable;
            internal bool LastReachable;
            internal bool Bounded;
            internal bool ScrollingRequired;
            internal bool ScrollingExact;
            internal bool FirstStartsVisible;
            internal long OpenMilliseconds;
            internal long ManagedBytesDelta;
            internal int GroupSlotDelta;
            internal string Reason = string.Empty;

            public override string ToString()
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "{0}/{1}/cycle{2}:rendered={3};slots={4};first={5};middle={6}" +
                    ";last={7};bounded={8};scrollNeeded={9};scrollExact={10}" +
                    ";firstVisible={11};openMs={12};bytes={13};slotDelta={14};{15}",
                    Family, Projected, Cycle, Rendered, SlotCount,
                    FirstReachable, MiddleReachable, LastReachable, Bounded,
                    ScrollingRequired, ScrollingExact, FirstStartsVisible,
                    OpenMilliseconds, ManagedBytesDelta, GroupSlotDelta, Reason);
            }
        }

        internal IReadOnlyList<Measurement> Measurements
        { get { return _measurements.AsReadOnly(); } }

        /// <summary>
        /// The projected worst-case parent sizes the charter's roster reaches.
        /// Derived, not written down, so a roster change moves the fixture with
        /// it rather than leaving it measuring a number nobody maintains.
        /// </summary>
        internal static int Projected(SummonFamily family)
        {
            return family == SummonFamily.Monster
                ? ExpandedSummoningIdealRosterCatalog.MonsterBaseEntryTarget
                : ExpandedSummoningIdealRosterCatalog.NaturesAllyBaseEntryTarget;
        }

        /// <summary>
        /// Builds a projected entry list for one family by repeating the
        /// family's real published variants until the projected count is
        /// reached.
        ///
        /// Repetition is deliberate. The question here is what the widget does
        /// with N entries - layout, scrolling, reachability, cost - and that
        /// does not depend on the entries being distinct creatures. Inventing
        /// plausible-looking future creatures would put unreviewed content in
        /// front of a measurement and prove nothing extra.
        /// </summary>
        private static List<AbilityData> BuildProjectedEntries(
            UnitEntityData unit, SummonFamily family, out BlueprintAbility parent)
        {
            BlueprintAbility[] parents = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintAbility>()
                .Where(ExpandedSummoningPublisher.IsPublishedExpandedParent)
                .ToArray();
            if (parents.Length == 0) throw new InvalidOperationException(
                "No published Expanded Summoning parent is available.");

            // The largest published parent of the requested family is the honest
            // starting point: its entries are the ones that would actually grow.
            string prefix = family == SummonFamily.Monster ? "SM" : "SNA";
            BlueprintAbility[] familyParents = parents.Where(value =>
                VariantsOf(value).Any(variant => variant != null &&
                    variant.name.IndexOf("_" + prefix + "_",
                        StringComparison.Ordinal) >= 0)).ToArray();
            BlueprintAbility[] pool = familyParents.Length != 0
                ? familyParents : parents;
            parent = pool.OrderByDescending(value => VariantsOf(value).Length)
                .First();

            BlueprintAbility[] variants = VariantsOf(parent)
                .Where(value => value != null).ToArray();
            if (variants.Length == 0) throw new InvalidOperationException(
                "The largest published parent exposes no variants.");

            int projected = Projected(family);
            var entries = new List<AbilityData>(projected);
            for (int index = 0; index < projected; index++)
                entries.Add(new AbilityData(variants[index % variants.Length],
                    unit.Descriptor));
            return entries;
        }

        private static BlueprintAbility[] VariantsOf(BlueprintAbility ability)
        {
            return ability == null || ability.Variants == null
                ? new BlueprintAbility[0] : ability.Variants;
        }

        /// <summary>
        /// One open/measure/close cycle. Returns false while the layout has not
        /// settled, so the caller can call it again on the next frame.
        /// </summary>
        internal bool Step(SummonFamily family, int cycle, ref int settled)
        {
            // Each ActionBarGroupSlot owns its own ActionBarSpellsGroup, so a
            // loaded save has dozens of them and picking a group directly
            // measures an arbitrary widget. The real player path is a click on a
            // group slot, which the shipped patch intercepts to record the slot
            // the popup anchors to; without that capture the layout code has
            // nothing to anchor against and produces no snapshot at all, which
            // is exactly what three earlier runs reported.
            //
            // So this reproduces the click: find a visible group slot, prefer
            // one whose own ability is a published summon parent, capture it the
            // way the patch does, and toggle that slot's own sub-group.
            ActionBarGroupSlot[] all = Resources
                .FindObjectsOfTypeAll<ActionBarGroupSlot>()
                .Where(value => value != null && value.gameObject != null)
                .ToArray();
            ActionBarGroupSlot[] scened = all.Where(value =>
                value.gameObject.scene.IsValid() &&
                value.gameObject.scene.isLoaded).ToArray();
            ActionBarGroupSlot[] slots = scened.Where(value =>
                value.gameObject.activeInHierarchy).ToArray();
            // Report each stage. "Nothing found" is several different problems
            // and they need different answers, so the counts say which.
            _availability = "loaded=" + all.Length + ";scened=" + scened.Length +
                ";active=" + slots.Length;
            _groupCount = slots.Length;
            ActionBarGroupSlot slot = slots.FirstOrDefault(IsPublishedParentSlot)
                ?? slots.FirstOrDefault();
            if (slot == null) return false;

            ActionBarSpellsGroup group = SubGroupOf(slot);
            if (group == null) return false;
            _liveGroupCount = 1;

            if (settled == 0)
            {
                BlueprintAbility parent;
                List<AbilityData> entries = BuildProjectedEntries(_unit, family,
                    out parent);
                var source = new AbilityData(parent, _unit.Descriptor);
                _pendingSlotsBefore = CountGroupSlots();
                _pendingBytesBefore = GC.GetTotalMemory(false);
                _pendingWatch = Stopwatch.StartNew();
                _pendingFamily = family;
                _pendingCycle = cycle;
                _pendingProjected = entries.Count;
                ExpandedSummoningVariantMenuRuntime.CaptureSourceSlot(group, slot);
                group.Toggle(_unit, entries, source);
                settled++;
                return false;
            }

            if (settled <= SettleFrames) { settled++; return false; }

            _pendingWatch.Stop();
            var measurement = new Measurement
            {
                Family = _pendingFamily,
                Projected = _pendingProjected,
                Cycle = _pendingCycle,
                OpenMilliseconds = _pendingWatch.ElapsedMilliseconds
            };

            ExpandedSummoningVariantMenuSnapshot snapshot;
            if (!ExpandedSummoningVariantMenuRuntime.TryGetSnapshot(group,
                out snapshot) || snapshot == null)
            {
                measurement.Reason = "no snapshot after toggle";
            }
            else
            {
                bool navigation = ExpandedSummoningVariantMenuRuntime
                    .TryValidateNavigation(group, out snapshot);
                measurement.Rendered = true;
                measurement.SlotCount = snapshot.SlotCount;
                measurement.FirstReachable = snapshot.FirstEntryReachable;
                measurement.MiddleReachable = snapshot.MiddleEntryReachable;
                measurement.LastReachable = snapshot.LastEntryReachable &&
                    navigation && snapshot.NavigationVerified;
                measurement.Bounded = snapshot.SafeRect.Contains(
                    snapshot.RenderedPopupRect,
                    SummonVariantMenuLayoutPolicy.Epsilon);
                measurement.ScrollingRequired = snapshot.DesiredHeight >
                    snapshot.SafeRect.Height +
                    SummonVariantMenuLayoutPolicy.Epsilon;
                measurement.ScrollingExact = !measurement.ScrollingRequired ||
                    snapshot.ScrollingRequired && snapshot.VerticalScrolling &&
                    snapshot.ScrollRectCount == 1 &&
                    snapshot.ViewportMarkerCount == 1 &&
                    snapshot.HasViewportRect;
                SummonVariantMenuRect visible = snapshot.HasViewportRect
                    ? snapshot.ViewportRect : snapshot.RenderedPopupRect;
                measurement.FirstStartsVisible =
                    snapshot.FirstSlotRect.YMax <= visible.YMax + 0.5f &&
                    snapshot.FirstSlotRect.YMax > visible.YMin;
                measurement.Reason = "measured";
            }

            group.Hide(true);
            measurement.GroupSlotDelta = CountGroupSlots() - _pendingSlotsBefore;
            measurement.ManagedBytesDelta = GC.GetTotalMemory(false) -
                _pendingBytesBefore;
            _measurements.Add(measurement);
            settled = 0;
            return true;
        }

        /// <summary>
        /// The slot's own popup group. ActionBarGroupSlot keeps it in a private
        /// field, which is how the shipped Harmony patch reaches it too.
        /// </summary>
        private static ActionBarSpellsGroup SubGroupOf(ActionBarGroupSlot slot)
        {
            FieldInfo field = typeof(ActionBarGroupSlot).GetField("SubGroup",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            return field == null ? null :
                field.GetValue(slot) as ActionBarSpellsGroup;
        }

        /// <summary>
        /// Whether this slot's own ability is one of the published summon
        /// parents. Preferring such a slot makes the measurement faithful to
        /// where the menu really opens; any visible slot still gives a valid
        /// anchor if none is on the bar.
        /// </summary>
        private static bool IsPublishedParentSlot(ActionBarGroupSlot slot)
        {
            try
            {
                FieldInfo field = typeof(ActionBarGroupSlot).GetField("SubGroup",
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
                if (field == null) return false;
                object mechanic = slot.GetType()
                    .GetProperty("MechanicSlot", BindingFlags.Instance |
                        BindingFlags.Public | BindingFlags.NonPublic)
                    ?.GetValue(slot, null);
                if (mechanic == null) return false;
                object spell = mechanic.GetType()
                    .GetProperty("Spell", BindingFlags.Instance |
                        BindingFlags.Public | BindingFlags.NonPublic)
                    ?.GetValue(mechanic, null) as AbilityData;
                AbilityData data = spell as AbilityData;
                return data != null && data.Blueprint != null &&
                    ExpandedSummoningPublisher.IsPublishedExpandedParent(
                        data.Blueprint);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static int CountGroupSlots()
        {
            ActionBarGroupSlot[] slots =
                Resources.FindObjectsOfTypeAll<ActionBarGroupSlot>();
            return slots == null ? 0 : slots.Length;
        }

        private SummonFamily _pendingFamily;
        private int _pendingCycle;
        private int _pendingProjected;
        private int _pendingSlotsBefore;
        private long _pendingBytesBefore;
        private Stopwatch _pendingWatch;

        /// <summary>Set when the widget assumption fails, for reporting.</summary>
        private int _groupCount = -1;
        private int _liveGroupCount = -1;
        private string _availability = "<unmeasured>";
        internal string Availability { get { return _availability; } }
        internal int ObservedSpellGroupCount { get { return _groupCount; } }
        internal int ObservedLiveSpellGroupCount { get { return _liveGroupCount; } }

        /// <summary>
        /// The rubric, evaluated. Written out in
        /// docs/EXPANDED-SUMMONING-PROJECTED-MENU-RUBRIC.md; this is the same
        /// set of conditions in executable form so the document and the gate
        /// cannot drift apart silently.
        /// </summary>
        internal bool Passes(out string summary)
        {
            var reasons = new List<string>();
            if (_liveGroupCount > 1)
                reasons.Add("live-action-bar-spell-groups=" + _liveGroupCount +
                    " (expected one; the measurement would be ambiguous)");
            if (_measurements.Count != 2 * Cycles)
                reasons.Add("expected " + (2 * Cycles) + " measurements, got " +
                    _measurements.Count);
            foreach (Measurement measurement in _measurements)
            {
                string tag = measurement.Family + "/" + measurement.Cycle;
                if (!measurement.Rendered) { reasons.Add(tag + ":not-rendered"); continue; }
                if (measurement.SlotCount != measurement.Projected)
                    reasons.Add(tag + ":slots=" + measurement.SlotCount +
                        " for " + measurement.Projected + " entries");
                if (!measurement.FirstReachable) reasons.Add(tag + ":first-unreachable");
                if (!measurement.MiddleReachable) reasons.Add(tag + ":middle-unreachable");
                if (!measurement.LastReachable) reasons.Add(tag + ":last-unreachable");
                if (!measurement.Bounded) reasons.Add(tag + ":outside-safe-area");
                if (!measurement.ScrollingExact) reasons.Add(tag + ":scrolling-inexact");
                if (!measurement.FirstStartsVisible)
                    reasons.Add(tag + ":first-entry-not-visible-on-open");
                if (measurement.OpenMilliseconds > 250)
                    reasons.Add(tag + ":open=" + measurement.OpenMilliseconds + "ms");
            }

            // Leak checks compare the last cycle with the first, because the
            // first open legitimately allocates the slots the widget then keeps.
            foreach (SummonFamily family in new[] { SummonFamily.Monster,
                SummonFamily.NaturesAlly })
            {
                Measurement[] familyCycles = _measurements
                    .Where(value => value.Family == family).ToArray();
                if (familyCycles.Length < 2) continue;
                int slotGrowth = familyCycles
                    .Skip(1).Sum(value => Math.Max(0, value.GroupSlotDelta));
                if (slotGrowth != 0)
                    reasons.Add(family + ":group-slots-grew-by-" + slotGrowth +
                        "-after-the-first-open");
            }

            summary = string.Join(" | ", _measurements
                .Select(value => value.ToString()).ToArray());
            if (reasons.Count != 0)
                summary = "FAILED[" + string.Join(",", reasons.ToArray()) +
                    "] " + summary;
            return reasons.Count == 0;
        }
    }
}
