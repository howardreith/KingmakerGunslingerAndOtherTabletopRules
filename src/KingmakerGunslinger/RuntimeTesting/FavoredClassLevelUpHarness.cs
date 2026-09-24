using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Real LevelUpController visits for favored-class evidence on detached,
    /// request-local chargen units. Every favored-class choice is made through
    /// the native selection state and SelectFeature; every other open choice
    /// is filled by a deterministic filler that never touches a favored-class
    /// state; a level is applied only after LevelUpState.IsComplete() (the gate
    /// CharacterBuildController.Next enforces before Commit) through
    /// ApplyLevelup, the method Commit calls. Evidence says exactly that.
    /// </summary>
    internal static class FavoredClassLevelUpHarness
    {
        /// <summary>The host's level-1 favored class choice (BasicFeatsProgression).</summary>
        internal const string FavoredClassSelectionGuid = "27947ef789544982a437200c3189c59a";

        private static readonly MethodInfo Start = typeof(LevelUpController)
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Single(value => value.Name == "StartWithoutAssigningStaticInstance" &&
                value.GetParameters().Length == 5);

        private static readonly MethodInfo ApplyLevelupMethod = typeof(LevelUpController)
            .GetMethod("ApplyLevelup", BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance);

        private static readonly object LevelUpMode = Enum.Parse(
            Start.GetParameters()[4].ParameterType, "LevelUp", false);

        internal static UnitEntityData CreateUnit(int wisdom)
        {
            UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            unit.Stats.Wisdom.BaseValue = wisdom;
            if (!unit.Descriptor.IsTurnedOn)
                unit.Descriptor.TurnOn();
            return unit;
        }

        /// <summary>
        /// Opens one native visit. The first visit settles character creation
        /// with the requested race; the class is then selected.
        /// </summary>
        internal static LevelUpController Open(UnitDescriptor unit, BlueprintRace race,
            BlueprintCharacterClass characterClass, string name)
        {
            var controller = (LevelUpController)Start.Invoke(null,
                new object[] { unit, false, null, null, LevelUpMode });
            if (controller == null)
                throw new InvalidOperationException("No native level-up controller was created.");
            try
            {
                CharGenRoot chargen = BlueprintRoot.Instance.CharGen;
                if (controller.State.CanSelectRace && !controller.SelectRace(race))
                    throw new InvalidOperationException("Native race selection rejected " + race.name);
                if (controller.State.CanSelectGender)
                    controller.SelectGender(unit.Gender);
                if (controller.State.CanSelectName)
                    controller.SelectName(name);
                if (controller.State.CanSelectPortrait)
                    controller.SelectPortrait(chargen.Portraits.First(value => value != null));
                if (controller.State.CanSelectVoice)
                    controller.SelectVoice((unit.Gender == Gender.Male ? chargen.MaleVoices :
                        chargen.FemaleVoices).First(value => value != null));
                if (!controller.SelectClass(characterClass, false))
                    throw new InvalidOperationException("Native class selection rejected " +
                        characterClass.name);
                controller.ApplyClassMechanics();
                controller.ApplySpellbook();
                controller.ApplySkillPoints();
                return controller;
            }
            catch
            {
                try { controller.Cancel(); } catch (Exception) { }
                throw;
            }
        }

        internal static void Close(LevelUpController controller)
        {
            if (controller != null)
                controller.Cancel();
        }

        /// <summary>The first unselected state of an exact selection identity.</summary>
        internal static FeatureSelectionState FindOpenState(LevelUpController controller,
            string selectionGuid)
        {
            return controller.State.Selections.FirstOrDefault(value => !value.Selected &&
                value.Selection is BlueprintScriptableObject &&
                string.Equals(((BlueprintScriptableObject)value.Selection).AssetGuid,
                    selectionGuid, StringComparison.Ordinal));
        }

        internal static IFeatureSelectionItem[] Items(LevelUpController controller,
            FeatureSelectionState state)
        {
            return state.Selection.ExtractSelectionItems(controller.Unit, controller.Preview)
                .Where(item => item != null && item.Feature != null).ToArray();
        }

        internal static IFeatureSelectionItem Item(LevelUpController controller,
            FeatureSelectionState state, BlueprintFeature feature)
        {
            return Items(controller, state).FirstOrDefault(item =>
                ReferenceEquals(item.Feature, feature));
        }

        internal static bool CanSelect(LevelUpController controller, FeatureSelectionState state,
            BlueprintFeature feature)
        {
            IFeatureSelectionItem item = Item(controller, state, feature);
            return item != null && state.Selection.CanSelect(controller.Preview, controller.State,
                state, item);
        }

        internal static bool Select(LevelUpController controller, FeatureSelectionState state,
            BlueprintFeature feature)
        {
            IFeatureSelectionItem item = Item(controller, state, feature);
            return item != null && controller.SelectFeature(state, item);
        }

        /// <summary>
        /// Picks the host's favored-class progression whose class list is
        /// exactly the target class.
        /// </summary>
        internal static BlueprintFeature ChooseFavoredClass(LevelUpController controller,
            BlueprintCharacterClass characterClass, JObject evidence)
        {
            FeatureSelectionState state = FindOpenState(controller, FavoredClassSelectionGuid);
            evidence["favoredClassSelectionOpen"] = state != null;
            if (state == null)
                return null;
            IFeatureSelectionItem choice = Items(controller, state).FirstOrDefault(item =>
                item.Feature is BlueprintProgression &&
                ((BlueprintProgression)item.Feature).Classes != null &&
                ((BlueprintProgression)item.Feature).Classes.Length == 1 &&
                ReferenceEquals(((BlueprintProgression)item.Feature).Classes[0], characterClass));
            evidence["favoredClassProgression"] = choice == null ? "<absent>" : choice.Feature.name;
            if (choice == null || !controller.SelectFeature(state, choice))
                return null;
            return choice.Feature;
        }

        /// <summary>
        /// Deterministically completes every open requirement except
        /// favored-class states (the level-1 favored class choice and every
        /// selection whose identity is in <paramref name="reserved"/>).
        /// </summary>
        internal static JObject FillOthers(LevelUpController controller, ICollection<string> reserved)
        {
            if (controller.State.CanSelectRaceStat)
                controller.SelectRaceStat(StatType.Strength);
            var resolved = new JArray();
            // Feature choices such as a deity add alignment restrictions and
            // may grant points, and the controller replays every action on each
            // change, so an alignment chosen first can be dropped later. Settle
            // alignment, points and features together until they are stable.
            for (int pass = 0; pass < 3; pass++)
            {
                SettleAlignment(controller, resolved);
                SpendPoints(controller);
                FillFeatureSelections(controller, reserved, resolved);
                if (!controller.State.CanSelectAlignment &&
                    controller.State.StatsDistribution.IsComplete() &&
                    controller.State.SkillPointsRemaining == 0 &&
                    controller.State.AttributePoints == 0 &&
                    !OpenUnreserved(controller, reserved))
                    break;
            }
            return new JObject
            {
                ["skillPointsRemaining"] = controller.State.SkillPointsRemaining,
                ["attributePoints"] = controller.State.AttributePoints,
                ["resolved"] = resolved
            };
        }

        private static readonly Alignment[] AlignmentOrder =
        {
            Alignment.TrueNeutral, Alignment.NeutralGood, Alignment.LawfulNeutral,
            Alignment.ChaoticNeutral, Alignment.NeutralEvil, Alignment.LawfulGood,
            Alignment.ChaoticGood, Alignment.LawfulEvil, Alignment.ChaoticEvil
        };

        private static void SettleAlignment(LevelUpController controller, JArray resolved)
        {
            foreach (Alignment alignment in AlignmentOrder)
            {
                if (!controller.State.CanSelectAlignment)
                    return;
                if (!controller.State.IsAlignmentRestricted(alignment) &&
                    controller.SelectAlignment(alignment))
                    resolved.Add("Alignment=" + alignment);
            }
        }

        private static void SpendPoints(LevelUpController controller)
        {
            int guard = 0;
            while (!controller.State.StatsDistribution.IsComplete() && guard++ < 200)
                if (!StatTypeHelper.Attributes.Any(attribute => controller.AddStatPoint(attribute)))
                    break;
            guard = 0;
            while (controller.State.SkillPointsRemaining < 0 && guard++ < 200)
                if (!StatTypeHelper.Skills.Any(skill => controller.UnspendSkillPoint(skill)))
                    break;
            guard = 0;
            while (controller.State.SkillPointsRemaining > 0 && guard++ < 200)
                if (!StatTypeHelper.Skills.Any(skill => controller.State.SkillPointsRemaining > 0 &&
                    controller.SpendSkillPoint(skill)))
                    break;
            guard = 0;
            while (controller.State.AttributePoints > 0 && guard++ < 200)
                if (!StatTypeHelper.Attributes.Any(attribute => controller.State.AttributePoints > 0 &&
                    controller.SpendAttributePoint(attribute)))
                    break;
        }

        private static void FillFeatureSelections(LevelUpController controller,
            ICollection<string> reserved, JArray resolved)
        {
            int guard = 0;
            while (guard++ < 60)
            {
                LevelUpState state = controller.State;
                UnitDescriptor preview = controller.Preview;
                FeatureSelectionState pending = state.Selections.FirstOrDefault(value =>
                    !value.Selected && value.Selection != null && !IsReserved(value, reserved) &&
                    value.CanSelectAnything(state, preview));
                if (pending == null)
                    break;
                IFeatureSelectionItem choice = pending.Selection.ExtractSelectionItems(preview, preview)
                    .Where(item => item != null && item.Feature != null &&
                        pending.Selection.CanSelect(preview, state, pending, item))
                    .OrderBy(item => item.Feature.AssetGuid, StringComparer.Ordinal)
                    .FirstOrDefault();
                if (choice == null || !controller.SelectFeature(pending, choice))
                    break;
                resolved.Add(Name(pending.Selection) + "=" + choice.Feature.name);
            }
        }

        private static bool OpenUnreserved(LevelUpController controller, ICollection<string> reserved)
        {
            LevelUpState state = controller.State;
            UnitDescriptor preview = controller.Preview;
            return state.Selections.Any(value => !value.Selected && value.Selection != null &&
                !IsReserved(value, reserved) && value.CanSelectAnything(state, preview));
        }

        /// <summary>Every term of LevelUpState.IsComplete, for an incomplete visit.</summary>
        internal static JObject Completion(LevelUpController controller)
        {
            LevelUpState state = controller.State;
            UnitDescriptor preview = controller.Preview;
            return new JObject
            {
                ["statsDistributionComplete"] = state.StatsDistribution.IsComplete(),
                ["canSelectAlignment"] = state.CanSelectAlignment,
                ["canSelectRace"] = state.CanSelectRace,
                ["canSelectRaceStat"] = state.CanSelectRaceStat,
                ["canSelectName"] = state.CanSelectName,
                ["canSelectPortrait"] = state.CanSelectPortrait,
                ["canSelectGender"] = state.CanSelectGender,
                ["canSelectVoice"] = state.CanSelectVoice,
                ["attributePoints"] = state.AttributePoints,
                ["selectedClass"] = state.SelectedClass == null ? "<none>" : state.SelectedClass.name,
                ["skillPointsComplete"] = state.IsSkillPointsComplete(),
                ["openSelections"] = Blockers(controller),
                ["openSpellSelections"] = state.SpellSelections.Count(data =>
                    data.CanSelectAnything(preview))
            };
        }

        /// <summary>Open, unfinished selections that still block completion.</summary>
        internal static JArray Blockers(LevelUpController controller)
        {
            LevelUpState state = controller.State;
            UnitDescriptor preview = controller.Preview;
            return new JArray(state.Selections.Where(value => !value.Selected &&
                value.Selection != null && value.CanSelectAnything(state, preview))
                .Select(value => Name(value.Selection)));
        }

        /// <summary>
        /// Applies the level only when LevelUpState.IsComplete() is true, then
        /// cancels the controller (disposing its preview).
        /// </summary>
        internal static bool Confirm(LevelUpController controller, UnitDescriptor unit,
            JObject evidence)
        {
            bool complete = controller.State.IsComplete();
            evidence["isComplete"] = complete;
            if (!complete)
            {
                evidence["blockers"] = Blockers(controller);
                evidence["completion"] = Completion(controller);
                return false;
            }
            ApplyLevelupMethod.Invoke(controller, new object[] { unit });
            evidence["confirmationRoute"] =
                "LevelUpState.IsComplete gate, then LevelUpController.ApplyLevelup (the call Commit makes)";
            if (!unit.IsTurnedOn)
                unit.TurnOn();
            return true;
        }

        internal static int Rank(UnitDescriptor unit, BlueprintFeature feature)
        {
            return feature == null ? 0 : unit.Progression.Features.GetRank(feature);
        }

        private static bool IsReserved(FeatureSelectionState state, ICollection<string> reserved)
        {
            var blueprint = state.Selection as BlueprintScriptableObject;
            if (blueprint == null)
                return false;
            return string.Equals(blueprint.AssetGuid, FavoredClassSelectionGuid, StringComparison.Ordinal) ||
                (reserved != null && reserved.Contains(blueprint.AssetGuid));
        }

        internal static string Name(IFeatureSelection selection)
        {
            var blueprint = selection as BlueprintScriptableObject;
            return blueprint == null ? "<unnamed>" : blueprint.name;
        }
    }
}
