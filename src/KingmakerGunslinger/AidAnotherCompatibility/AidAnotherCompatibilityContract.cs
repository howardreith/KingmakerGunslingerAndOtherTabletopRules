using System;
using System.Reflection;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace KingmakerGunslinger.AidAnotherCompatibility
{
    internal sealed class CotwAidAnotherContract
    {
        internal CotwAidAnotherContract(Assembly assembly,
            MethodInfo creationMethod, ContextRankConfig configuration,
            FieldInfo featureListField, BlueprintBuff[] buffs,
            BlueprintAbility ordinaryAbility, BlueprintAbility selfAbility,
            string fingerprint)
        {
            Assembly = assembly ?? throw new ArgumentNullException("assembly");
            CreationMethod = creationMethod ??
                throw new ArgumentNullException("creationMethod");
            Configuration = configuration ??
                throw new ArgumentNullException("configuration");
            FeatureListField = featureListField ??
                throw new ArgumentNullException("featureListField");
            Buffs = buffs ?? throw new ArgumentNullException("buffs");
            OrdinaryAbility = ordinaryAbility ??
                throw new ArgumentNullException("ordinaryAbility");
            SelfAbility = selfAbility ??
                throw new ArgumentNullException("selfAbility");
            Fingerprint = fingerprint ?? string.Empty;
        }

        internal Assembly Assembly { get; private set; }
        internal MethodInfo CreationMethod { get; private set; }
        internal ContextRankConfig Configuration { get; private set; }
        internal FieldInfo FeatureListField { get; private set; }
        internal BlueprintBuff[] Buffs { get; private set; }
        internal BlueprintAbility OrdinaryAbility { get; private set; }
        internal BlueprintAbility SelfAbility { get; private set; }
        internal string Fingerprint { get; private set; }

        internal BlueprintFeature[] ReadFeatureList()
        {
            return FeatureListField.GetValue(Configuration) as BlueprintFeature[];
        }

        internal void WriteFeatureList(BlueprintFeature[] values)
        {
            FeatureListField.SetValue(Configuration, values);
        }
    }

    internal sealed class FavoredClassTraitContract
    {
        internal FavoredClassTraitContract(Assembly assembly,
            MethodInfo loadMethod, bool traitsEnabled,
            BlueprintFeatureSelection combatTraits,
            BlueprintFeatureSelection raceTraits,
            BlueprintFeatureSelection firstTrait,
            BlueprintFeatureSelection secondTrait,
            BlueprintFeatureSelection adopted,
            BlueprintFeature additionalTraits,
            BlueprintFeature halflingHelpful, string fingerprint)
        {
            Assembly = assembly ?? throw new ArgumentNullException("assembly");
            LoadMethod = loadMethod ?? throw new ArgumentNullException("loadMethod");
            TraitsEnabled = traitsEnabled;
            CombatTraits = combatTraits ??
                throw new ArgumentNullException("combatTraits");
            RaceTraits = raceTraits ?? throw new ArgumentNullException("raceTraits");
            FirstTrait = firstTrait ?? throw new ArgumentNullException("firstTrait");
            SecondTrait = secondTrait ?? throw new ArgumentNullException("secondTrait");
            Adopted = adopted ?? throw new ArgumentNullException("adopted");
            AdditionalTraits = additionalTraits ??
                throw new ArgumentNullException("additionalTraits");
            HalflingHelpful = halflingHelpful ??
                throw new ArgumentNullException("halflingHelpful");
            Fingerprint = fingerprint ?? string.Empty;
        }

        internal Assembly Assembly { get; private set; }
        internal MethodInfo LoadMethod { get; private set; }
        internal bool TraitsEnabled { get; private set; }
        internal BlueprintFeatureSelection CombatTraits { get; private set; }
        internal BlueprintFeatureSelection RaceTraits { get; private set; }
        internal BlueprintFeatureSelection FirstTrait { get; private set; }
        internal BlueprintFeatureSelection SecondTrait { get; private set; }
        internal BlueprintFeatureSelection Adopted { get; private set; }
        internal BlueprintFeature AdditionalTraits { get; private set; }
        internal BlueprintFeature HalflingHelpful { get; private set; }
        internal string Fingerprint { get; private set; }
    }

    internal sealed class AidAnotherContractResolution<T> where T : class
    {
        internal AidAnotherContractResolution(
            OptionalAidAnotherAvailability availability, string failedCheck,
            T contract)
        {
            Availability = availability;
            FailedCheck = failedCheck ?? string.Empty;
            Contract = contract;
        }

        internal OptionalAidAnotherAvailability Availability { get; private set; }
        internal string FailedCheck { get; private set; }
        internal T Contract { get; private set; }
        internal bool IsCompatible
        { get { return Availability == OptionalAidAnotherAvailability.Compatible &&
            Contract != null; } }
    }
}
