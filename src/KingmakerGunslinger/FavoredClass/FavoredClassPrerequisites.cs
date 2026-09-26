using System;
using System.Globalization;
using System.Reflection;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// Opens exactly one leaf of a target counter at a time, using the same
    /// full/partial alternation as the Favored Class host
    /// (next pick is full iff (full + partial + 1) % d == 0), with KMG-owned
    /// ceilings instead of the host's truncating partial capacity. Like the
    /// host, a leaf already chosen in this selection chain cannot be chosen
    /// again in the same level-up.
    /// </summary>
    public sealed class PrerequisiteFavoredClassInvestment : Prerequisite
    {
        public BlueprintFeature Full;
        public BlueprintFeature Partial;
        public int Divisor = 1;

        /// <summary>Printed cap in benefit steps; zero means uncapped.</summary>
        public int CapSteps;

        public bool PartialRole;

        public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit,
            LevelUpState state)
        {
            if (unit == null || Full == null || Divisor < 1 || (Divisor > 1 && Partial == null))
                return false;
            if (selectionState != null)
            {
                if (selectionState.IsSelectedInChildren(Full))
                    return false;
                if (Partial != null && selectionState.IsSelectedInChildren(Partial))
                    return false;
            }
            int full = unit.Progression.Features.GetRank(Full);
            int partial = Partial == null ? 0 : unit.Progression.Features.GetRank(Partial);
            return FavoredClassRankPolicy.CanInvest(Rate, full, partial,
                PartialRole ? FavoredClassInvestmentRole.Partial : FavoredClassInvestmentRole.Full);
        }

        public override string GetUIText()
        {
            if (Divisor <= 1)
                return "Favored class investment";
            return PartialRole
                ? string.Format(CultureInfo.InvariantCulture,
                    "Favored class investment (not the {0}th of {0})", Divisor)
                : string.Format(CultureInfo.InvariantCulture,
                    "Favored class investment (completes {0} of {0})", Divisor);
        }

        internal FavoredClassRate Rate
        {
            get { return new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null); }
        }
    }

    /// <summary>
    /// Ancestry eligibility for one canonical effect: the unit's actual race
    /// identity plus verified permissions (FAQ, preserved host policy, Mostly
    /// Human), restricted to enabled profiles. Full and partial leaves carry
    /// identical instances of this prerequisite, so both halves of a counter
    /// always share the same restrictions.
    /// </summary>
    public sealed class PrerequisiteFavoredClassAncestry : Prerequisite
    {
        public string EffectId;

        /// <summary>The rows that open this target; empty or null means every row.</summary>
        public string[] RowIds;

        public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit,
            LevelUpState state)
        {
            if (unit == null || string.IsNullOrEmpty(EffectId))
                return false;
            return FavoredClassRuntime.IsEffectEligible(EffectId, unit,
                RowIds == null || RowIds.Length == 0 ? null : RowIds);
        }

        public override string GetUIText()
        {
            return "Favored class race option";
        }
    }

    /// <summary>
    /// The improved class feature is still attainable: none of the unit's
    /// archetypes of the host class removes every listed feature (O06: both
    /// paladin auras; O07: the Hunter's Bond that grants the companion).
    /// Earlier, dormant investment in a feature gained later stays legal.
    /// </summary>
    public sealed class PrerequisiteFavoredClassFeatureAvailable : Prerequisite
    {
        public BlueprintCharacterClass CharacterClass;
        public BlueprintFeature[] Features;

        public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit,
            LevelUpState state)
        {
            if (unit == null || CharacterClass == null || Features == null || Features.Length == 0)
                return false;
            ClassData data = unit.Progression.GetClassData(CharacterClass);
            BlueprintArchetype[] archetypes = data == null ? new BlueprintArchetype[0] :
                System.Linq.Enumerable.ToArray(data.Archetypes);
            return System.Linq.Enumerable.Any(Features, feature => feature != null &&
                !System.Linq.Enumerable.Any(archetypes, archetype => Removes(archetype, feature)));
        }

        private static bool Removes(BlueprintArchetype archetype, BlueprintFeature feature)
        {
            if (archetype == null || archetype.RemoveFeatures == null)
                return false;
            return System.Linq.Enumerable.Any(archetype.RemoveFeatures, entry => entry != null &&
                entry.Features != null && entry.Features.Contains(feature));
        }

        public override string GetUIText()
        {
            return "Can gain " + string.Join(" or ", System.Linq.Enumerable.ToArray(
                System.Linq.Enumerable.Select(Features ?? new BlueprintFeature[0],
                    feature => feature == null ? "?" : feature.Name)));
        }
    }

    /// <summary>
    /// The native level-up replays its picks in priority order, and the
    /// host's reward selection has an earlier priority than the bloodline, the
    /// revelation or the power chosen in the same level-up. While a level-up
    /// replays, its own later picks count as chosen, so a counter can target
    /// what the same level-up gains; outside a replay nothing changes.
    /// Each replayed pick's check and application run inside a scope of its
    /// own controller that is closed in a finally block (the replay hook
    /// routes exactly those two native calls through Check and Apply): an
    /// exception in a pick leaves nothing marked, a nested replay restores
    /// the outer one and a retry opens a fresh scope.
    /// A stored level plan (auto-level, a pregen, an imported companion) is
    /// applied by the controller itself, one AddAction per planned pick in
    /// the same priority order; while each runs (the plan hook routes that
    /// one native call through AddPlanned) the plan's own picks count as
    /// chosen too, exactly as a replay's later picks do.
    /// </summary>
    internal static class FavoredClassPendingPicks
    {
        [ThreadStatic]
        private static FavoredClassScopeStack<LevelUpController> s_Replays;

        [ThreadStatic]
        private static FavoredClassScopeStack<PlannedLevel> s_Plans;

        private static Func<LevelUpController, ILevelUpAction, bool, bool> s_NativeAddAction;

        /// <summary>
        /// Guarded runtime qualification only: runs inside a replayed pick's
        /// scope before the native call, so a failure can be injected into
        /// the real replay; null in play.
        /// </summary>
        internal static Action<ILevelUpAction> FaultInjection;

        /// <summary>Whether the replay hook scoped both native calls (set when it applies).</summary>
        internal static bool Installed { get; set; }

        /// <summary>Whether the plan hook scoped the plan's native AddAction call (set when it applies).</summary>
        internal static bool PlanInstalled { get; set; }

        /// <summary>Open replay and plan scopes on this thread; zero outside a replayed or planned pick.</summary>
        internal static int Depth
        {
            get { return (s_Replays == null ? 0 : s_Replays.Depth) + (s_Plans == null ? 0 : s_Plans.Depth); }
        }

        private static FavoredClassScopeStack<LevelUpController> Replays
        {
            get { return s_Replays ?? (s_Replays = new FavoredClassScopeStack<LevelUpController>()); }
        }

        private static FavoredClassScopeStack<PlannedLevel> Plans
        {
            get { return s_Plans ?? (s_Plans = new FavoredClassScopeStack<PlannedLevel>()); }
        }

        /// <summary>
        /// Binds the controller's own AddAction(action, ignoreOrder) once;
        /// false (and the plan is left unscoped) when its shape differs.
        /// </summary>
        internal static bool BindNativeAddAction(MethodInfo method)
        {
            if (method == null || method.IsStatic || method.ReturnType != typeof(bool))
                return false;
            try
            {
                s_NativeAddAction = (Func<LevelUpController, ILevelUpAction, bool, bool>)Delegate.CreateDelegate(
                    typeof(Func<LevelUpController, ILevelUpAction, bool, bool>), method);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// A stored plan's AddAction(action, ignoreOrder), made inside a scope
        /// of its controller and the plan of the level it is applying.
        /// </summary>
        internal static bool AddPlanned(LevelUpController controller, ILevelUpAction action, bool ignoreOrder)
        {
            LevelPlanData plan = controller == null || controller.Unit == null || controller.State == null ? null :
                controller.Unit.Progression.GetLevelPlan(controller.State.NextLevel);
            return Plans.Run(new PlannedLevel(controller, plan == null ? null : plan.Actions),
                () => s_NativeAddAction(controller, action, ignoreOrder));
        }

        /// <summary>The native replay's action.Check, made inside its controller's scope.</summary>
        internal static bool Check(ILevelUpAction action, LevelUpState state, UnitDescriptor unit,
            LevelUpController controller)
        {
            return Replays.Run(controller, () =>
            {
                Fault(action);
                return action.Check(state, unit);
            });
        }

        /// <summary>The native replay's action.Apply, made inside its controller's scope.</summary>
        internal static void Apply(ILevelUpAction action, LevelUpState state, UnitDescriptor unit,
            LevelUpController controller)
        {
            Replays.Run(controller, () =>
            {
                Fault(action);
                action.Apply(state, unit);
            });
        }

        private static void Fault(ILevelUpAction action)
        {
            Action<ILevelUpAction> fault = FaultInjection;
            if (fault != null)
                fault(action);
        }

        /// <summary>
        /// Whether the level-up replaying (or applying the plan of) this exact
        /// state picks one of these features.
        /// </summary>
        internal static bool Selects(LevelUpState state, string[] featureGuids)
        {
            LevelUpController replaying = Installed && s_Replays != null ? s_Replays.Current : null;
            PlannedLevel planned = PlanInstalled && s_Plans != null ? s_Plans.Current : null;
            LevelUpController controller = replaying ?? (planned == null ? null : planned.Controller);
            if (controller == null || state == null || featureGuids == null || !ReferenceEquals(controller.State, state))
                return false;
            if (Picks(controller.LevelUpActions, featureGuids))
                return true;
            return planned != null && ReferenceEquals(planned.Controller, controller) &&
                Picks(planned.Actions, featureGuids);
        }

        private static bool Picks(System.Collections.Generic.IEnumerable<ILevelUpAction> actions, string[] featureGuids)
        {
            if (actions == null)
                return false;
            foreach (ILevelUpAction action in actions)
            {
                var pick = action as SelectFeature;
                BlueprintFeature feature = pick == null || pick.Item == null ? null : pick.Item.Feature;
                if (feature != null && Array.IndexOf(featureGuids, feature.AssetGuid) >= 0)
                    return true;
            }
            return false;
        }

        /// <summary>One planned pick's scope: its controller and the plan it is applying.</summary>
        private sealed class PlannedLevel
        {
            internal PlannedLevel(LevelUpController controller, ILevelUpAction[] actions)
            {
                Controller = controller;
                Actions = actions;
            }

            internal LevelUpController Controller { get; private set; }
            internal ILevelUpAction[] Actions { get; private set; }
        }
    }

    /// <summary>
    /// I06/S04 and O01: the unit already has the chosen revelation (any one of
    /// its selectable features; a Dragon revelation has one per colour) or
    /// performance. Some belong to an optional provider that may create them
    /// after KMG registers, so they are matched by identity when checked.
    /// </summary>
    public sealed class PrerequisiteFavoredClassOwnsAny : Prerequisite
    {
        public string[] FeatureGuids;

        /// <summary>Player-facing name of what must be owned ("the revelation Fire Breath").</summary>
        public string Title;

        public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit,
            LevelUpState state)
        {
            if (unit == null || unit.Progression == null || FeatureGuids == null || FeatureGuids.Length == 0)
                return false;
            foreach (Feature feature in unit.Progression.Features.Enumerable)
                if (feature != null && feature.Blueprint != null &&
                    System.Array.IndexOf(FeatureGuids, feature.Blueprint.AssetGuid) >= 0)
                    return true;
            // The target chosen in this same level-up (its pick replays after
            // the host's earlier-priority reward pick) is owned as well.
            return FavoredClassPendingPicks.Selects(state, FeatureGuids);
        }

        public override string GetUIText()
        {
            return "Has " + (Title ?? "?");
        }
    }
}
