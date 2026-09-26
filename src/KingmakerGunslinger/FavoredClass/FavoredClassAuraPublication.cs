using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Properties;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// O06 native read point. The ally buffs of Aura of Courage and Aura of
    /// Resolve each apply one native Morale save modifier of 4 + Bonus
    /// against their descriptor, in the buff's own context whose caster is
    /// the aura's paladin. This transaction feeds Bonus from a rank the
    /// buff's context computes on that caster (the owned earned-steps
    /// property), so the increment lives inside the same non-stacking Morale
    /// modifier and belongs to that paladin alone. The aura radius, the
    /// paladin's own immunities and every other save are untouched. It
    /// validates the exact native shape first and restores the exact
    /// previous components and values on rollback.
    /// </summary>
    internal sealed class FavoredClassAuraPublication
    {
        internal const string CourageEffectBuffGuid = "1044ac71f6200f84bbfbcfa2bcb3bd66";
        internal const string ResolveEffectBuffGuid = "d8f2f84899d6e1e4d83859e18f697ae3";
        internal const AbilityRankType RankType = AbilityRankType.StatBonus;
        internal const int NativeMoraleValue = 4;

        private readonly List<KeyValuePair<BlueprintBuff, BlueprintComponent[]>> _components =
            new List<KeyValuePair<BlueprintBuff, BlueprintComponent[]>>();
        private readonly List<KeyValuePair<SavingThrowBonusAgainstDescriptor, ContextValue>> _bonuses =
            new List<KeyValuePair<SavingThrowBonusAgainstDescriptor, ContextValue>>();
        private readonly List<string> _evidence = new List<string>();

        internal IList<string> Evidence
        {
            get { return _evidence.AsReadOnly(); }
        }

        internal bool IsCommitted { get; private set; }

        /// <summary>Validates the exact native shape without changing anything.</summary>
        internal static void Check(LibraryScriptableObject library)
        {
            if (library == null) throw new ArgumentNullException("library");
            Validate(BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                CourageEffectBuffGuid, "native Aura of Courage ally buff"), SpellDescriptor.Fear);
            Validate(BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                ResolveEffectBuffGuid, "native Aura of Resolve ally buff"), SpellDescriptor.Charm);
        }

        internal static FavoredClassAuraPublication Apply(LibraryScriptableObject library,
            BlueprintUnitProperty steps)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (steps == null) throw new ArgumentNullException("steps");
            var targets = new[]
            {
                Tuple.Create(BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                    CourageEffectBuffGuid, "native Aura of Courage ally buff"), SpellDescriptor.Fear),
                Tuple.Create(BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                    ResolveEffectBuffGuid, "native Aura of Resolve ally buff"), SpellDescriptor.Charm)
            };
            var plan = new List<Tuple<BlueprintBuff, SavingThrowBonusAgainstDescriptor>>();
            foreach (var target in targets)
                plan.Add(Tuple.Create(target.Item1, Validate(target.Item1, target.Item2)));
            var publication = new FavoredClassAuraPublication();
            try
            {
                foreach (var entry in plan)
                {
                    BlueprintBuff buff = entry.Item1;
                    SavingThrowBonusAgainstDescriptor save = entry.Item2;
                    BlueprintComponent[] before = buff.ComponentsArray;
                    publication._components.Add(new KeyValuePair<BlueprintBuff, BlueprintComponent[]>(
                        buff, before));
                    buff.ComponentsArray = before.Concat(new BlueprintComponent[]
                        { CreateRank(buff, steps) }).ToArray();
                    // The two buffs may share one native component instance.
                    if (!publication._bonuses.Any(value => ReferenceEquals(value.Key, save)))
                    {
                        publication._bonuses.Add(new KeyValuePair<SavingThrowBonusAgainstDescriptor,
                            ContextValue>(save, save.Bonus));
                        save.Bonus = new ContextValue { ValueType = ContextValueType.Rank, ValueRank = RankType };
                    }
                    publication._evidence.Add("buff=" + buff.name + ";descriptor=" +
                        (SpellDescriptor)save.SpellDescriptor.Value + ";morale=" + save.Value +
                        "+rank(" + RankType + ")");
                }
            }
            catch
            {
                publication.Rollback();
                throw;
            }
            publication.IsCommitted = true;
            return publication;
        }

        internal void Rollback()
        {
            for (int index = _bonuses.Count - 1; index >= 0; index--)
                _bonuses[index].Key.Bonus = _bonuses[index].Value;
            for (int index = _components.Count - 1; index >= 0; index--)
                _components[index].Key.ComponentsArray = _components[index].Value;
            _bonuses.Clear();
            _components.Clear();
            IsCommitted = false;
        }

        private static SavingThrowBonusAgainstDescriptor Validate(BlueprintBuff buff,
            SpellDescriptor descriptor)
        {
            BlueprintComponent[] components = buff.ComponentsArray ?? new BlueprintComponent[0];
            SavingThrowBonusAgainstDescriptor[] saves = components
                .OfType<SavingThrowBonusAgainstDescriptor>().ToArray();
            if (saves.Length != 1 || saves[0].ModifierDescriptor != ModifierDescriptor.Morale ||
                saves[0].Value != NativeMoraleValue ||
                ((SpellDescriptor)saves[0].SpellDescriptor.Value & descriptor) == SpellDescriptor.None ||
                saves[0].Bonus == null ||
                !(saves[0].Bonus.ValueType == ContextValueType.Simple && saves[0].Bonus.Value == 0) ||
                components.OfType<ContextRankConfig>().Any(rank => RankTypeOf(rank) == RankType))
                throw new InvalidOperationException(buff.name +
                    " no longer matches the qualified native aura contract.");
            return saves[0];
        }

        private static AbilityRankType RankTypeOf(ContextRankConfig rank)
        {
            FieldInfo field = typeof(ContextRankConfig).GetField("m_Type",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return field == null ? AbilityRankType.Default : (AbilityRankType)field.GetValue(rank);
        }

        private static ContextRankConfig CreateRank(BlueprintBuff buff, BlueprintUnitProperty steps)
        {
            var rank = ScriptableObject.CreateInstance<ContextRankConfig>();
            rank.name = "$KMG_FavoredClass_Paladin_AuraAllyBonus_" + buff.name + "_Rank";
            Set(rank, "m_Type", RankType);
            Set(rank, "m_BaseValueType", ContextRankBaseValueType.CustomProperty);
            Set(rank, "m_CustomProperty", steps);
            Set(rank, "m_Progression", ContextRankProgression.AsIs);
            Set(rank, "m_UseMin", true);
            Set(rank, "m_Min", 0);
            return rank;
        }

        private static void Set(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || !field.FieldType.IsInstanceOfType(value))
                throw new MissingFieldException(target.GetType().FullName, name);
            field.SetValue(target, value);
        }
    }
}
