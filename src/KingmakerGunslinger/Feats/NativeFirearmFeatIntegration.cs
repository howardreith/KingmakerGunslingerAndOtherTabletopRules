using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Feats
{
    internal static class NativeFirearmFeatIntegration
    {
        private static readonly object Sync = new object();
        private static BlueprintParametrizedFeature[] _native;
        private static BlueprintFeature[] _parameters;
        private static BlueprintComponent[][] _originalComponents;

        internal static void Configure(BlueprintParametrizedFeature weaponFocus,
            BlueprintParametrizedFeature[] dependent,
            BlueprintFeature[] parameters, BlueprintFeature[][] legacyDependent)
        {
            if (weaponFocus == null || dependent == null || dependent.Length != 4 ||
                dependent.Any(value => value == null) || parameters == null ||
                parameters.Length != 5 || parameters.Any(value => value == null))
                throw new ArgumentException("Native firearm feat integration is incomplete.");
            lock (Sync)
            {
                RollbackLocked();
                _native = new[] { weaponFocus }.Concat(dependent).ToArray();
                _parameters = (BlueprintFeature[])parameters.Clone();
                _originalComponents = _native.Select(value =>
                    value.ComponentsArray).ToArray();
                FirearmWeaponFeatEffect[] effects = {
                    FirearmWeaponFeatEffect.Attack,
                    FirearmWeaponFeatEffect.Attack,
                    FirearmWeaponFeatEffect.Damage,
                    FirearmWeaponFeatEffect.Damage,
                    FirearmWeaponFeatEffect.DoubleCriticalEdge };
                for (int index = 0; index < _native.Length; index++)
                    AddAdapter(_native[index], effects[index]);
            }
        }

        internal static void Rollback()
        {
            lock (Sync) RollbackLocked();
        }

        private static void RollbackLocked()
        {
            if (_native != null && _originalComponents != null)
            {
                int count = Math.Min(_native.Length, _originalComponents.Length);
                for (int index = 0; index < count; index++)
                    if (_native[index] != null)
                        _native[index].ComponentsArray = _originalComponents[index];
            }
            _native = null;
            _parameters = null;
            _originalComponents = null;
        }

        private static void AddAdapter(BlueprintParametrizedFeature feature,
            FirearmWeaponFeatEffect effect)
        {
            BlueprintComponent[] source = feature.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            if (source.OfType<NativeFirearmParametrizedBonus>().Any()) return;
            var adapter = UnityEngine.ScriptableObject.CreateInstance<
                NativeFirearmParametrizedBonus>();
            adapter.Effect = effect;
            feature.ComponentsArray = source.Concat(new BlueprintComponent[]
                { adapter }).ToArray();
        }

        internal static bool IsIntegrated(BlueprintParametrizedFeature feature)
        {
            lock (Sync) return _native != null && Array.IndexOf(_native, feature) >= 0;
        }

        internal static IEnumerable<FeatureUIData> Append(
            BlueprintParametrizedFeature feature, IEnumerable<FeatureUIData> source)
        {
            FeatureUIData[] existing = (source ?? Enumerable.Empty<FeatureUIData>())
                .ToArray();
            BlueprintFeature[] parameters;
            lock (Sync)
            {
                if (_native == null || Array.IndexOf(_native, feature) < 0)
                    return existing;
                parameters = (BlueprintFeature[])_parameters.Clone();
            }
            var result = new List<FeatureUIData>(existing);
            for (int index = 0; index < parameters.Length; index++)
            {
                BlueprintFeature parameter = parameters[index];
                if (existing.Any(value => value != null &&
                    ReferenceEquals(value.Param.Blueprint, parameter))) continue;
                string displayName = DisplayName(FirearmFeatBlueprints.Kinds[index]);
                result.Add(new FeatureUIData(feature, new FeatureParam(parameter),
                    displayName, parameter.Description, parameter.Icon,
                    displayName));
            }
            return result.OrderBy(value => value == null ? string.Empty : value.Name,
                StringComparer.CurrentCultureIgnoreCase).ToArray();
        }

        private static string DisplayName(FirearmKind kind)
        {
            return kind.ToString();
        }

        internal static bool TryKind(FeatureParam parameter, out FirearmKind kind)
        {
            kind = default(FirearmKind);
            BlueprintScriptableObject blueprint = parameter.Blueprint;
            lock (Sync)
            {
                if (_parameters == null) return false;
                int index = Array.IndexOf(_parameters, blueprint);
                if (index < 0 || index >= FirearmFeatBlueprints.Kinds.Length) return false;
                kind = FirearmFeatBlueprints.Kinds[index];
                return true;
            }
        }

        internal static bool TryWeaponKind(ItemEntityWeapon weapon, out FirearmKind kind)
        {
            kind = default(FirearmKind);
            if (weapon == null || weapon.Blueprint == null ||
                weapon.Blueprint.Type == null) return false;
            FirearmDefinitionComponent[] markers = weapon.Blueprint.Type.ComponentsArray
                .OfType<FirearmDefinitionComponent>().ToArray();
            if (markers.Length != 1) return false;
            try { kind = markers[0].Definition.Kind; return true; }
            catch (Exception) { return false; }
        }
    }

    internal sealed class NativeFirearmParametrizedBonus :
        ParametrizedFeatureComponent,
        IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>,
        IInitiatorRulebookHandler<RuleCalculateWeaponStats>
    {
        public FirearmWeaponFeatEffect Effect;

        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            FirearmKind selected, actual;
            if (Effect != FirearmWeaponFeatEffect.Attack || evt == null ||
                !NativeFirearmFeatIntegration.TryKind(Param, out selected) ||
                !NativeFirearmFeatIntegration.TryWeaponKind(evt.Weapon, out actual) ||
                selected != actual) return;
            evt.AddBonus(1, Fact);
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt) { }

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            FirearmKind selected, actual;
            if (evt == null || !NativeFirearmFeatIntegration.TryKind(Param, out selected) ||
                !NativeFirearmFeatIntegration.TryWeaponKind(evt.Weapon, out actual) ||
                selected != actual) return;
            if (Effect == FirearmWeaponFeatEffect.Damage)
                evt.AddBonusDamage(2);
            else if (Effect == FirearmWeaponFeatEffect.DoubleCriticalEdge)
                evt.DoubleCriticalEdge = true;
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }
    }

    [HarmonyPatch(typeof(BlueprintParametrizedFeature), "GetFullSelectionItems")]
    internal static class NativeFirearmFeatFullMenuPatch
    {
        private static void Postfix(BlueprintParametrizedFeature __instance,
            ref IEnumerable<FeatureUIData> __result)
        {
            __result = NativeFirearmFeatIntegration.Append(__instance, __result);
        }
    }
}
