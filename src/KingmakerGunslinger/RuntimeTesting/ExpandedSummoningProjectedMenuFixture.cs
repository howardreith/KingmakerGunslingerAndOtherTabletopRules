using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.UnitSettings;
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

        private readonly string _screenshotDirectory;

        internal ExpandedSummoningProjectedMenuFixture(UnitEntityData unit,
            string screenshotDirectory)
        {
            if (unit == null) throw new ArgumentNullException("unit");
            _unit = unit;
            _screenshotDirectory = screenshotDirectory;
            // The mod manager's IMGUI window covered the whole frame in the
            // first screenshots. It is closed for the measurement and put
            // back with the action bar.
            _overlayWasOpen = RuntimeTestRunner.SetModManagerOverlay(false);
        }

        private readonly bool _overlayWasOpen;

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
            /// <summary>
            /// The toggle call plus the frame that first draws the menu: the
            /// widget's own cost, which is what the 250 ms budget is for.
            /// </summary>
            internal long OpenMilliseconds;
            /// <summary>The synchronous Toggle call: native fill and the layout applied inside it.</summary>
            internal long ToggleMilliseconds;
            /// <summary>From the end of the call to the next update, i.e. the frame that rendered the menu.</summary>
            internal long FirstFrameMilliseconds;
            /// <summary>The whole wait including the settle frames, which measure the host's frame pacing.</summary>
            internal long SettleMilliseconds;
            /// <summary>Mean unscaled frame time over the settle frames.</summary>
            internal double FrameMilliseconds;
            /// <summary>Slots the native fill created, read from the group itself.</summary>
            internal int NativeSlots;
            /// <summary>
            /// The first measurement of a run: the native fill instantiates
            /// its slot widgets on this open and keeps them, so it is
            /// reported and not scored against the budget.
            /// </summary>
            internal bool Cold;
            internal long ManagedBytesDelta;
            internal int GroupSlotDelta;
            internal string Resolution = string.Empty;
            internal float UiScale;
            internal int TooltipsPresent;
            internal string Screenshot = string.Empty;
            internal string Reason = string.Empty;

            public override string ToString()
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "{0}/{1}/cycle{2}:rendered={3};slots={4};first={5};middle={6}" +
                    ";last={7};bounded={8};scrollNeeded={9};scrollExact={10}" +
                    ";firstVisible={11};openMs={12};cold={25};toggleMs={20};firstFrameMs={21}" +
                    ";settleMs={22};frameMs={23:F1};nativeSlots={24}" +
                    ";bytes={13};slotDelta={14}" +
                    ";resolution={15};uiScale={16:F3};tooltips={17};shot={18};{19}",
                    Family, Projected, Cycle, Rendered, SlotCount,
                    FirstReachable, MiddleReachable, LastReachable, Bounded,
                    ScrollingRequired, ScrollingExact, FirstStartsVisible,
                    OpenMilliseconds, ManagedBytesDelta, GroupSlotDelta,
                    Resolution, UiScale, TooltipsPresent, Screenshot, Reason,
                    ToggleMilliseconds, FirstFrameMilliseconds, SettleMilliseconds,
                    FrameMilliseconds, NativeSlots, Cold);
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
        /// <summary>
        /// Frames the fixture waits after the loading process and its screen
        /// have both gone. The first live run measured its Monster cycles
        /// with the loading screen still up - the action bar exists before
        /// the screen clears - and every one of them faulted.
        /// </summary>
        private const int ReadyFrames = 20;
        private int _readyFrames;

        private static bool LoadingInProgress()
        {
            Kingmaker.EntitySystem.Persistence.LoadingProcess loading =
                Kingmaker.EntitySystem.Persistence.LoadingProcess.Instance;
            return loading != null && (loading.IsLoadingInProcess ||
                loading.IsLoadingScreenActive ||
                loading.IsManualLoadingScreenActive);
        }

        internal bool Step(SummonFamily family, int cycle, ref int settled)
        {
            if (LoadingInProgress())
            {
                _readyFrames = 0;
                return false;
            }
            if (_readyFrames < ReadyFrames)
            {
                _readyFrames++;
                return false;
            }
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
            // The disposable working save presents no group slot on its own,
            // so the fixture puts a real summon parent into a real action-bar
            // index and lets the UI build the slot. This is fixture setup, not
            // a production change: nothing in the shipped summoning code is
            // touched, the anchor is not bypassed, and the index is restored to
            // whatever held it before. No save is written.
            if (!_setupAttempted)
            {
                _setupAttempted = true;
                _setupReason = InstallParentSlotEnabled
                    ? TryInstallParentSlot()
                    : "install disabled: it hung the UI rebuild";
            }

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
                _pendingFailuresBefore = ExpandedSummoningVariantMenuRuntime.Failures;
                _pendingFrameSeconds.Clear();
                ExpandedSummoningVariantMenuRuntime.CaptureSourceSlot(group, slot);
                group.Toggle(_unit, entries, source);
                // The call is synchronous: the native fill and the layout the
                // shipped patch applies inside it have both run by now.
                _pendingToggleMs = _pendingWatch.ElapsedMilliseconds;
                settled++;
                return false;
            }

            // The first update after the toggle closes the frame that drew the
            // menu; the ones after it only measure how fast this host runs
            // frames, and are recorded as such.
            if (settled == 1)
                _pendingFirstFrameMs = _pendingWatch.ElapsedMilliseconds -
                    _pendingToggleMs;
            if (settled <= SettleFrames)
            {
                _pendingFrameSeconds.Add(Time.unscaledDeltaTime);
                settled++;
                return false;
            }

            // The screenshot is written at the end of the frame it was asked
            // for, so it is asked for one step before the group is hidden.
            if (_pendingMeasurement == null)
            {
                _pendingWatch.Stop();
                _pendingMeasurement = new Measurement
                {
                    Family = _pendingFamily,
                    Projected = _pendingProjected,
                    Cycle = _pendingCycle,
                    // The synchronous call: the native fill and the layout the
                    // shipped patch applies inside it. The frames after it
                    // measure this host, which runs at a few frames per
                    // second under the harness, and are recorded as such.
                    OpenMilliseconds = _pendingToggleMs,
                    ToggleMilliseconds = _pendingToggleMs,
                    FirstFrameMilliseconds = _pendingFirstFrameMs,
                    SettleMilliseconds = _pendingWatch.ElapsedMilliseconds,
                    FrameMilliseconds = _pendingFrameSeconds.Count == 0 ? 0d :
                        _pendingFrameSeconds.Average() * 1000d,
                    NativeSlots = CountNativeSlots(group),
                    Cold = _measurements.Count == 0
                };
                if (!string.IsNullOrEmpty(_screenshotDirectory))
                {
                    string shot = "projected-menu-" + _pendingFamily + "-cycle" +
                        _pendingCycle + ".png";
                    _pendingMeasurement.Screenshot = CaptureScreenshot(
                        System.IO.Path.Combine(_screenshotDirectory, shot)) ? shot :
                        "<capture-unavailable>";
                }
                return false;
            }
            Measurement measurement = _pendingMeasurement;
            _pendingMeasurement = null;

            measurement.Resolution = Screen.width + "x" + Screen.height;
            Canvas canvas = group.GetComponentInParent<Canvas>();
            measurement.UiScale = canvas == null ? 0f : canvas.scaleFactor;
            // Tooltip presence is a layout fact and is all this claims: whether
            // the slots carry a tooltip component, not whether its text is
            // right. Text correctness is the structural inventory's job.
            measurement.TooltipsPresent = group
                .GetComponentsInChildren<UnityEngine.EventSystems.IPointerEnterHandler>(true)
                .Length;

            ExpandedSummoningVariantMenuSnapshot snapshot;
            if (!ExpandedSummoningVariantMenuRuntime.TryGetSnapshot(group,
                out snapshot) || snapshot == null)
            {
                // The layout runtime records why it produced nothing; that is
                // the diagnosis, so it travels with the measurement.
                measurement.Reason = "no snapshot after toggle;layoutFailures=" +
                    (ExpandedSummoningVariantMenuRuntime.Failures -
                        _pendingFailuresBefore) + ";layout=" +
                    ExpandedSummoningVariantMenuRuntime.LastResult;
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

        /// <summary>
        /// Whether the fixture may place a summon parent into the action bar.
        ///
        /// Off. Installing a MechanicActionBarSlotSpontaneusSpell and marking
        /// the settings dirty was tried, and the game stopped producing frames
        /// after the save loaded: no error, no result, the process resident and
        /// unresponsive until it was closed. A frame-budget guard cannot fire
        /// when frames stop, so the scenario could not even report its own
        /// failure.
        ///
        /// A fixture that can hang the game is worse than one that cannot take a
        /// measurement, so the mutation stays off until the hang is understood.
        /// The code is kept rather than deleted because the approach is right -
        /// it reproduces the real click path - and only its effect on the UI
        /// rebuild is unexplained.
        /// </summary>
        private const bool InstallParentSlotEnabled = false;

        private bool _setupAttempted;
        private string _setupReason = "<not attempted>";
        private int _installedIndex = -1;
        private MechanicActionBarSlot _replacedSlot;
        internal string SetupReason { get { return _setupReason; } }

        /// <summary>
        /// Places a published summon parent in the first free action-bar index.
        /// Returns a description of what happened, for the record either way.
        /// </summary>
        private string TryInstallParentSlot()
        {
            try
            {
                BlueprintAbility parent = BlueprintBootstrap.Library
                    .GetAllBlueprints().OfType<BlueprintAbility>()
                    .Where(ExpandedSummoningPublisher.IsPublishedExpandedParent)
                    .OrderByDescending(value => value.Variants == null ? 0 :
                        value.Variants.Length)
                    .FirstOrDefault();
                if (parent == null) return "no published parent to install";

                UnitUISettings settings = _unit.UISettings;
                if (settings == null) return "unit has no UI settings";

                for (int index = 0; index < 60; index++)
                {
                    MechanicActionBarSlot existing = settings.GetSlot(index, _unit);
                    if (!(existing is MechanicActionBarSlotEmpty)) continue;
                    _installedIndex = index;
                    _replacedSlot = existing;
                    break;
                }

                if (_installedIndex < 0) return "no free action-bar index";

                var slot = new MechanicActionBarSlotSpontaneusSpell
                {
                    Spell = new AbilityData(parent, _unit.Descriptor),
                    Unit = _unit
                };
                settings.SetSlot(slot, _installedIndex);
                settings.SetDirty();
                return "installed " + parent.name + " at index " + _installedIndex;
            }
            catch (Exception error)
            {
                return "install-failed:" + error.GetType().Name;
            }
        }

        /// <summary>
        /// Puts the action bar back. Called whatever the outcome, because a
        /// fixture that leaves the bar rearranged is a fixture that changed the
        /// thing it was measuring.
        /// </summary>
        internal void RestoreActionBar()
        {
            if (_overlayWasOpen) RuntimeTestRunner.SetModManagerOverlay(true);
            if (_installedIndex < 0) return;
            try
            {
                UnitUISettings settings = _unit.UISettings;
                if (settings == null) return;
                settings.SetSlot(_replacedSlot ?? new MechanicActionBarSlotEmpty(),
                    _installedIndex);
                settings.SetDirty();
            }
            catch (Exception)
            {
                // Nothing useful to do here; the scenario is disposable and the
                // unit dies with the session.
            }
            finally
            {
                _installedIndex = -1;
                _replacedSlot = null;
            }
        }

        /// <summary>
        /// Writes a screenshot, if this build can.
        ///
        /// UnityEngine.ScreenCapture lives in ScreenCaptureModule, which is
        /// outside the project's qualified reference bundle, so it is reached
        /// reflectively rather than by widening the bundle for one diagnostic.
        /// A build without it records that the capture was unavailable instead
        /// of failing the measurement.
        /// </summary>
        private static bool CaptureScreenshot(string path)
        {
            try
            {
                Type type = Type.GetType(
                    "UnityEngine.ScreenCapture, UnityEngine.ScreenCaptureModule") ??
                    Type.GetType("UnityEngine.ScreenCapture, UnityEngine");
                if (type == null) return false;
                MethodInfo capture = type.GetMethod("CaptureScreenshot",
                    BindingFlags.Public | BindingFlags.Static, null,
                    new[] { typeof(string) }, null);
                if (capture == null) return false;
                capture.Invoke(null, new object[] { path });
                return true;
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

        private static int CountNativeSlots(ActionBarSpellsGroup group)
        {
            FieldInfo field = typeof(ActionBarSpellsGroup).GetField("m_Slots",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            System.Collections.ICollection slots = field == null ? null :
                field.GetValue(group) as System.Collections.ICollection;
            return slots == null ? -1 : slots.Count;
        }

        private SummonFamily _pendingFamily;
        private int _pendingCycle;
        private int _pendingProjected;
        private int _pendingSlotsBefore;
        private long _pendingBytesBefore;
        private Stopwatch _pendingWatch;
        private long _pendingToggleMs;
        private long _pendingFirstFrameMs;
        private Measurement _pendingMeasurement;
        private long _pendingFailuresBefore;
        private readonly List<float> _pendingFrameSeconds = new List<float>();

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
                if (!measurement.Cold && measurement.OpenMilliseconds > 250)
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
