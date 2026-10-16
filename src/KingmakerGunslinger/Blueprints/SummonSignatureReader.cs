using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Reads a live summon ability into a <see cref="SummonSignature"/>.
    /// Spawns, pool clears and conditionals are modelled; any other action is
    /// kept as its exact type, and a spawn reachable only through an action
    /// the reader does not model is counted as hidden so the graph is
    /// rejected rather than half-understood.
    /// </summary>
    internal static class SummonSignatureReader
    {
        private const BindingFlags Instance = BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly FieldInfo FullRoundField =
            typeof(BlueprintAbility).GetField("m_IsFullRoundAction", Instance);
        private static readonly FieldInfo ExtendableField =
            typeof(ContextDurationValue).GetField("m_IsExtendable", Instance);

        internal static SummonSignature Read(BlueprintAbility ability)
        {
            if (ability == null) throw new ArgumentNullException("ability");
            BlueprintComponent[] components = ability.ComponentsArray ??
                new BlueprintComponent[0];
            var result = new SummonSignature {
                AbilityType = ability.Type.ToString(),
                ActionType = ability.ActionType.ToString(),
                FullRoundField = FullRoundField != null &&
                    (bool)FullRoundField.GetValue(ability),
                Range = ability.Range.ToString(),
                School = components.OfType<SpellComponent>().Select(value =>
                    value.School.ToString()).FirstOrDefault() ?? "none",
                Descriptor = ((SpellDescriptor)ability.SpellDescriptor).ToString(),
                SpellLists = components.OfType<SpellListComponent>().Count(),
                RunActions = components.OfType<AbilityEffectRunAction>().Count(),
                // The host's own menus only: this module's published
                // variants component on a direct root is not host data.
                Variants = components.OfType<AbilityVariants>().Count(value =>
                    !string.Equals(value.name,
                        AlternateSummoningExposure.VariantsComponentName,
                        StringComparison.Ordinal))
            };
            foreach (BlueprintComponent component in components)
            {
                if (component == null) continue;
                AbilityResourceLogic resource = component as AbilityResourceLogic;
                if (resource != null)
                {
                    result.Resources.Add(new SummonResource {
                        Resource = (component.GetType() == typeof(AbilityResourceLogic) ?
                            "" : component.GetType().FullName + ":") +
                            Guid(resource.RequiredResource),
                        Spend = resource.IsSpendResource, Amount = resource.Amount,
                        CustomCost = resource.CostIsCustom,
                        IncreasingFacts = (resource.ResourceCostIncreasingFacts ??
                            new List<Kingmaker.Blueprints.Facts.BlueprintUnitFact>())
                            .Select(Guid).ToList() });
                }
                if (component is IAbilityCasterChecker &&
                    !(component is AlternateSummoningAvailability))
                    result.CasterCheckers.Add(Describe(component));
                if (component is IAbilityResourceCostCalculator)
                    result.CostCalculators.Add(Describe(component));
                ContextRankConfig rank = component as ContextRankConfig;
                if (rank != null) result.Ranks.Add(Rank(rank));
            }
            AbilityEffectRunAction run = components.OfType<AbilityEffectRunAction>()
                .FirstOrDefault();
            if (run != null && run.Actions != null)
                result.Actions = Nodes(run.Actions.Actions);
            int modelled = result.Spawns().Count();
            int all = AlternateSummoningGraph.Objects<ContextActionSpawnMonster>(
                components).Count;
            result.HiddenSpawns = all - modelled;
            return result;
        }

        private static List<SummonNode> Nodes(IEnumerable<GameAction> actions)
        {
            var result = new List<SummonNode>();
            foreach (GameAction action in actions ?? Enumerable.Empty<GameAction>())
            {
                if (action == null) { result.Add(SummonNode.ForOther("null")); continue; }
                Type type = action.GetType();
                if (type == typeof(ContextActionSpawnMonster))
                    result.Add(SummonNode.ForSpawn(Spawn((ContextActionSpawnMonster)action)));
                else if (type == typeof(ContextActionClearSummonPool))
                    result.Add(SummonNode.ForClear(Guid(
                        ((ContextActionClearSummonPool)action).SummonPool)));
                else if (type == typeof(Conditional))
                {
                    var conditional = (Conditional)action;
                    ConditionsChecker checker = conditional.ConditionsChecker;
                    result.Add(SummonNode.ForConditional(
                        checker == null ? "none" : checker.Operation.ToString(),
                        (checker == null || checker.Conditions == null ?
                            new Condition[0] : checker.Conditions).Select(value =>
                                value == null ? "null" : Describe(value)),
                        Nodes(conditional.IfTrue == null ? null :
                            conditional.IfTrue.Actions),
                        Nodes(conditional.IfFalse == null ? null :
                            conditional.IfFalse.Actions)));
                }
                else result.Add(SummonNode.ForOther(type.FullName));
            }
            return result;
        }

        private static SummonSpawn Spawn(ContextActionSpawnMonster spawn)
        {
            ContextDiceValue count = spawn.CountValue;
            ContextDurationValue duration = spawn.DurationValue;
            return new SummonSpawn {
                Unit = Guid(spawn.Blueprint), Pool = Guid(spawn.SummonPool),
                CountDice = count == null ? "null" : count.DiceType.ToString(),
                CountDiceCount = Value(count == null ? null : count.DiceCountValue),
                CountBonus = Value(count == null ? null : count.BonusValue),
                DurationRate = duration == null ? "null" : duration.Rate.ToString(),
                DurationDice = duration == null ? "null" : duration.DiceType.ToString(),
                DurationDiceCount = Value(duration == null ? null :
                    duration.DiceCountValue),
                DurationBonus = Value(duration == null ? null : duration.BonusValue),
                DurationExtendable = duration != null && ExtendableField != null &&
                    (bool)ExtendableField.GetValue(duration),
                Level = Value(spawn.LevelValue),
                DoNotLinkToCaster = spawn.DoNotLinkToCaster,
                DirectlyControllable = spawn.IsDirectlyControllable,
                AfterSpawn = (spawn.AfterSpawn == null || spawn.AfterSpawn.Actions == null ?
                    new GameAction[0] : spawn.AfterSpawn.Actions).Select(value =>
                        value == null ? "null" : value.GetType().FullName).ToList() };
        }

        private static SummonValue Value(ContextValue value)
        {
            if (value == null) return new SummonValue("null", 0, "", "", "", "");
            return new SummonValue(value.ValueType.ToString(), value.Value,
                value.ValueRank.ToString(), value.ValueShared.ToString(),
                value.Property.ToString(), Guid(value.CustomProperty));
        }

        private static SummonRank Rank(ContextRankConfig rank)
        {
            return new SummonRank {
                Type = Field(rank, "m_Type"),
                BaseValueType = Field(rank, "m_BaseValueType"),
                Progression = Field(rank, "m_Progression"),
                Classes = Array(rank, "m_Class"),
                Archetype = Field(rank, "Archetype"),
                Feature = Field(rank, "m_Feature"),
                FeatureList = Array(rank, "m_FeatureList"),
                Stat = Field(rank, "m_Stat"),
                StartLevel = Int(rank, "m_StartLevel"),
                StepLevel = Int(rank, "m_StepLevel"),
                UseMin = Bool(rank, "m_UseMin"), Min = Int(rank, "m_Min"),
                UseMax = Bool(rank, "m_UseMax"), Max = Int(rank, "m_Max"),
                ExceptClasses = Bool(rank, "m_ExceptClasses"),
                CustomProperty = Field(rank, "m_CustomProperty"),
                CustomProgression = Render(Get(rank, "m_CustomProgression")),
                BasedOnClassLevel = rank.IsBasedOnClassLevel,
                FeatureListBased = rank.IsFeatureList };
        }

        /// <summary>A component or condition as its exact type and its own
        /// public fields - pools, flags, fact lists - never its owner.</summary>
        internal static string Describe(object value)
        {
            Type type = value.GetType();
            var fields = new List<KeyValuePair<string, string>>();
            for (Type current = type; current != null &&
                current != typeof(BlueprintComponent) && current != typeof(Element) &&
                current != typeof(UnityEngine.ScriptableObject) &&
                current != typeof(object); current = current.BaseType)
            {
                foreach (FieldInfo field in current.GetFields(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.DeclaredOnly))
                    fields.Add(SummonSignatureFormat.Field(field.Name,
                        Render(field.GetValue(value))));
            }
            return SummonSignatureFormat.Component(type.FullName, fields);
        }

        private static string Render(object value)
        {
            if (value == null) return "null";
            BlueprintScriptableObject blueprint = value as BlueprintScriptableObject;
            if (blueprint != null) return blueprint.AssetGuid;
            if (value is string) return (string)value;
            if (value is bool) return (bool)value ? "True" : "False";
            if (value is IFormattable && value.GetType().IsPrimitive)
                return ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture);
            if (value.GetType().IsEnum) return value.ToString();
            IEnumerable sequence = value as IEnumerable;
            if (sequence != null)
                return SummonSignatureFormat.List(sequence.Cast<object>().Select(Render));
            if (value is UnityEngine.Object) return value.GetType().FullName;
            if (value.GetType().IsValueType) return value.ToString();
            var fields = value.GetType().GetFields(BindingFlags.Instance |
                BindingFlags.Public).Select(field => SummonSignatureFormat.Field(
                    field.Name, Render(field.GetValue(value))));
            return SummonSignatureFormat.Component(value.GetType().FullName, fields);
        }

        private static string Guid(BlueprintScriptableObject value)
        { return value == null ? "null" : value.AssetGuid; }

        private static object Get(object target, string name)
        {
            FieldInfo field = target.GetType().GetField(name, Instance);
            if (field == null) throw new MissingFieldException(
                target.GetType().FullName, name);
            return field.GetValue(target);
        }

        private static string Field(object target, string name)
        { return Render(Get(target, name)); }

        private static List<string> Array(object target, string name)
        {
            IEnumerable values = Get(target, name) as IEnumerable;
            return values == null ? new List<string>() :
                values.Cast<object>().Select(Render).ToList();
        }

        private static int Int(object target, string name)
        { return (int)Get(target, name); }

        private static bool Bool(object target, string name)
        { return (bool)Get(target, name); }
    }
}
