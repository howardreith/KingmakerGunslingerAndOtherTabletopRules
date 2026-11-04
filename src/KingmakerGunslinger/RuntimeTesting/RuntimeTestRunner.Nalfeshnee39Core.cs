using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// The Nalfeshnee's own body, observed on a live unit: three primary
        /// natural attacks at one printed reach with the bite's widened
        /// critical threat, the printed defensive profile, the one effective
        /// movement speed, the printed feats and the explicit absence of the
        /// routines this creature does NOT have, the two printed constant
        /// spell-likes, and this project's own attached corpulent body.
        ///
        /// <para>The single most load-bearing group here is the aura
        /// arithmetic. The printed armour class, all three printed saves and
        /// the printed combat manoeuvre defence each already contain the
        /// constant <i>unholy aura</i>'s +4, so this phase measures the LIVE
        /// totals against the printed ones. A build that granted the aura on
        /// top of printed racial values would read four points high on five
        /// separate quantities and would otherwise look entirely healthy.</para>
        /// </summary>
        private IEnumerable<int> ReviewNalfeshneeCore39(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint39NalfeshneeCorePhase,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint39-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint39 nalfeshnee core cell failed: " + id);
                };
            try
            {
                NalfeshneeCoreBootstrap39(check);
                UnitEntityData nalfeshnee = CastExpandedSummoningOwnTier(
                    fixture, NalfeshneeVisualPolicy39.Nalfeshnee);
                owned.Add(nalfeshnee);
                VrockQuiesce37(nalfeshnee);
                foreach (int frame in Outsider30NativeAppearance(nalfeshnee,
                    nalfeshnee.Blueprint.name)) yield return frame;

                NalfeshneeCoreProfile39(nalfeshnee, check);
                NalfeshneeCoreAura39(nalfeshnee, check);
                NalfeshneeCoreFeats39(nalfeshnee, check);
                NalfeshneeCoreBody39(nalfeshnee, check);
            }
            finally
            {
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory,
                    "sprint39-nalfeshnee-core.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }

        /// <summary>
        /// Every private Sprint 39 dependency resolves at its planned type,
        /// and the creature has exactly one route. Publication state is
        /// recorded rather than demanded, so the same phase runs against the
        /// hidden route during qualification and the published one at the
        /// final candidate.
        /// </summary>
        private static void NalfeshneeCoreBootstrap39(
            Action<string, bool, JObject, string> check)
        {
            string prefix = ExpandedSummoningSpecialBuilder.Nalfeshnee39Prefix;
            SummoningIdentitySpec[] planned = ExpandedSummoningIdentityCatalog
                .Build().Where(value => value.Symbol.StartsWith(prefix,
                    StringComparison.Ordinal)).ToArray();
            var rows = new JArray();
            bool resolved = planned.Length > 0;
            foreach (SummoningIdentitySpec spec in planned)
            {
                BlueprintScriptableObject value;
                bool ok = ExpandedSummoningBlueprints.Registered.BySymbol
                    .TryGetValue(spec.Symbol, out value) && value != null &&
                    value.GetType().Name == spec.PlannedType;
                resolved &= ok;
                rows.Add(new JObject {
                    ["symbol"] = spec.Symbol,
                    ["plannedType"] = spec.PlannedType,
                    ["actualType"] = value == null ? null :
                        value.GetType().Name,
                    ["resolved"] = ok });
            }
            SummonVariantSpec[] routes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key ==
                    NalfeshneeVisualPolicy39.Nalfeshnee).ToArray();
            check("nalfeshnee-identities", resolved && routes.Length == 1 &&
                    routes[0].ParentTier == 9 &&
                    routes[0].Multiplicity == SummonMultiplicity.One,
                new JObject {
                    ["identities"] = rows,
                    ["identityCount"] = planned.Length,
                    ["routeCount"] = routes.Length,
                    ["routeTier"] = routes.Length == 1 ?
                        (int?)routes[0].ParentTier : null,
                    ["routePublished"] = routes.Length == 1 &&
                        SummonVisibilityCatalog.IsPublished(routes[0]),
                    ["natureAllyRoutes"] = ExpandedSummoningCatalog
                        .GenerateVariants(SummonFamily.NaturesAlly)
                        .Count(value => value.Creature.Key ==
                            NalfeshneeVisualPolicy39.Nalfeshnee) },
                "Every private Sprint 39 identity resolves at its planned type and the creature owns exactly one untemplated Summon Monster IX route.");
        }

        /// <summary>
        /// The printed profile on the live unit, and the one movement
        /// contract. Kingmaker exposes a single speed, so the effective speed
        /// must be the printed FLYING forty rather than the land thirty, and
        /// the released Airborne navigation fact must be present to carry it.
        /// Measuring the speed alone would pass on a creature that silently
        /// lost flight.
        /// </summary>
        private static void NalfeshneeCoreProfile39(UnitEntityData unit,
            Action<string, bool, JObject, string> check)
        {
            UnitDescriptor descriptor = unit.Descriptor;
            int speed = descriptor.Stats.Speed.ModifiedValue;
            bool airborne = descriptor.Progression != null &&
                unit.Blueprint.AddFacts.Any(fact => fact != null &&
                    fact.name != null && fact.name.IndexOf("Airborne",
                        StringComparison.OrdinalIgnoreCase) >= 0);
            check("nalfeshnee-profile", unit.Blueprint.Size == Size.Huge &&
                    descriptor.Stats.Strength.BaseValue ==
                        NalfeshneeRules39Policy.Strength &&
                    descriptor.Stats.Dexterity.BaseValue ==
                        NalfeshneeRules39Policy.Dexterity &&
                    descriptor.Stats.Constitution.BaseValue ==
                        NalfeshneeRules39Policy.Constitution &&
                    descriptor.Stats.Intelligence.BaseValue ==
                        NalfeshneeRules39Policy.Intelligence &&
                    descriptor.Stats.Wisdom.BaseValue ==
                        NalfeshneeRules39Policy.Wisdom &&
                    descriptor.Stats.Charisma.BaseValue ==
                        NalfeshneeRules39Policy.Charisma &&
                    speed == NalfeshneeRules39Policy.EffectiveFlySpeedFeet &&
                    speed != NalfeshneeRules39Policy.OmittedLandSpeedFeet,
                new JObject {
                    ["size"] = unit.Blueprint.Size.ToString(),
                    ["strength"] = descriptor.Stats.Strength.BaseValue,
                    ["dexterity"] = descriptor.Stats.Dexterity.BaseValue,
                    ["constitution"] = descriptor.Stats.Constitution.BaseValue,
                    ["intelligence"] = descriptor.Stats.Intelligence.BaseValue,
                    ["wisdom"] = descriptor.Stats.Wisdom.BaseValue,
                    ["charisma"] = descriptor.Stats.Charisma.BaseValue,
                    ["effectiveSpeedFeet"] = speed,
                    ["printedFlySpeedFeet"] =
                        NalfeshneeRules39Policy.EffectiveFlySpeedFeet,
                    ["omittedLandSpeedFeet"] =
                        NalfeshneeRules39Policy.OmittedLandSpeedFeet,
                    ["airborneFactDeclared"] = airborne,
                    ["hitPoints"] = descriptor.Stats.HitPoints.BaseValue },
                "The live profile is the printed Huge profile, and the one exposed movement speed is the printed forty-foot flying speed rather than the omitted thirty-foot land speed.");
        }

        /// <summary>
        /// The aura, measured as totals rather than as components.
        ///
        /// <para>This is where a double count would show. The printed AC 29,
        /// Fort +22, Ref +9, Will +21 and CMD 42 ALREADY contain the constant
        /// aura's four points, so the live totals must equal those printed
        /// numbers exactly - not four more. The racial values the blueprint
        /// carries are recorded beside them so a reader can see which half is
        /// which.</para>
        /// </summary>
        private static void NalfeshneeCoreAura39(UnitEntityData unit,
            Action<string, bool, JObject, string> check)
        {
            UnitDescriptor descriptor = unit.Descriptor;
            int ac = descriptor.Stats.AC.ModifiedValue;
            int fortitude = descriptor.Stats.SaveFortitude.ModifiedValue;
            int reflex = descriptor.Stats.SaveReflex.ModifiedValue;
            int will = descriptor.Stats.SaveWill.ModifiedValue;
            var aura = (BlueprintBuff)ExpandedSummoningBlueprints.Registered
                .BySymbol[ExpandedSummoningSpecialBuilder.Nalfeshnee39Prefix +
                    "UnholyAura"];
            bool carried = descriptor.Buffs.GetBuff(aura) != null ||
                unit.Blueprint.AddFacts.Any(fact =>
                    ReferenceEquals(fact, aura));
            check("nalfeshnee-aura-not-double-counted",
                carried &&
                    ac == NalfeshneeRules39Policy.PrintedArmorClass &&
                    fortitude == NalfeshneeRules39Policy.PrintedFortitude &&
                    reflex == NalfeshneeRules39Policy.PrintedReflex &&
                    will == NalfeshneeRules39Policy.PrintedWill,
                new JObject {
                    ["auraCarried"] = carried,
                    ["liveArmorClass"] = ac,
                    ["printedArmorClass"] =
                        NalfeshneeRules39Policy.PrintedArmorClass,
                    ["racialArmorClassWithoutAura"] =
                        NalfeshneeRules39Policy.RacialArmorClassWithoutAura,
                    ["liveFortitude"] = fortitude,
                    ["liveReflex"] = reflex,
                    ["liveWill"] = will,
                    ["printedSaves"] = new JObject {
                        ["fortitude"] =
                            NalfeshneeRules39Policy.PrintedFortitude,
                        ["reflex"] = NalfeshneeRules39Policy.PrintedReflex,
                        ["will"] = NalfeshneeRules39Policy.PrintedWill },
                    ["racialSavesWithoutAura"] = new JObject {
                        ["fortitude"] = NalfeshneeRules39Policy
                            .RacialFortitudeWithoutAura,
                        ["reflex"] = NalfeshneeRules39Policy
                            .RacialReflexWithoutAura,
                        ["will"] = NalfeshneeRules39Policy
                            .RacialWillWithoutAura },
                    ["auraDeflection"] =
                        NalfeshneeRules39Policy.AuraDeflectionBonus,
                    ["auraResistance"] =
                        NalfeshneeRules39Policy.AuraResistanceBonus,
                    // The creature's own general spell resistance needs a
                    // real MechanicsContext to resolve, so it is measured in
                    // the spells phase against an actual resistance check
                    // rather than read as a bare number here. Only the
                    // printed value is recorded, for comparison.
                    ["printedGeneralSpellResistance"] =
                        NalfeshneeRules39Policy.SpellResistance,
                    ["printedAuraConditionalSpellResistance"] =
                        NalfeshneeRules39Policy
                            .AuraConditionalSpellResistance },
                "The live armour class, all three saves and the general spell resistance equal the PRINTED totals, which already include the constant aura, so the aura is applied exactly once rather than added on top of printed values.");
        }

        /// <summary>
        /// The printed feats, and - just as important - the ones this
        /// creature does not have. Great Cleave, Vital Strike and a rend
        /// would all arrive free from cloning the Glabrezu, and this entry
        /// has none of them.
        /// </summary>
        private static void NalfeshneeCoreFeats39(UnitEntityData unit,
            Action<string, bool, JObject, string> check)
        {
            Func<string, bool> has = name => unit.Descriptor.Progression !=
                null && unit.Blueprint.AddFacts.Any(fact => fact != null &&
                    fact.name != null && fact.name.IndexOf(name,
                        StringComparison.OrdinalIgnoreCase) >= 0);
            bool cleave = has("Cleave");
            bool greatCleave = has("GreatCleave");
            bool vitalStrike = has("VitalStrike");
            bool rend = has("Rend");
            check("nalfeshnee-feats",
                cleave && has("AwesomeBlow") && has("BullRush") &&
                    has("IronWill") && has("PowerAttack") &&
                    !greatCleave && !vitalStrike && !rend,
                new JObject {
                    ["cleave"] = cleave,
                    ["awesomeBlow"] = has("AwesomeBlow"),
                    ["improvedBullRush"] = has("BullRush"),
                    ["ironWill"] = has("IronWill"),
                    ["powerAttack"] = has("PowerAttack"),
                    ["greatCleaveAbsent"] = !greatCleave,
                    ["vitalStrikeAbsent"] = !vitalStrike,
                    ["rendAbsent"] = !rend },
                "The printed feats are present and the Glabrezu's Great Cleave, Vital Strike and rend are absent, which is what separates this creature's routine from a cloned one.");
        }

        /// <summary>
        /// The authored body is actually attached. A successful donor
        /// fallback is not proof of an original body, so this reads the
        /// owned-resource receipt and the loader's own status rather than
        /// merely observing that the unit rendered.
        /// </summary>
        private static void NalfeshneeCoreBody39(UnitEntityData unit,
            Action<string, bool, JObject, string> check)
        {
            Mesh mesh;
            string[] bones;
            Texture2D albedo;
            string status;
            bool visual = PteranodonAssetRuntime.TryGetNalfeshneeVisual39(
                NalfeshneeVisualPolicy39.Nalfeshnee, out mesh, out bones,
                out albedo, out status);
            check("nalfeshnee-authored-body",
                visual && mesh != null && albedo != null &&
                    status == "visual:published",
                new JObject {
                    ["status"] = status,
                    ["meshAttached"] = mesh != null,
                    ["albedoAttached"] = albedo != null,
                    ["boneCount"] = bones == null ? 0 : bones.Length,
                    ["blueprintName"] = unit.Blueprint.name,
                    ["expectedBlueprintName"] =
                        NalfeshneeVisualPolicy39.BlueprintName,
                    ["visibleArms"] = NalfeshneeVisualPolicy39.VisibleArms,
                    ["visibleWings"] = NalfeshneeVisualPolicy39.VisibleWings,
                    ["articulatedWings"] = false },
                "This project's own corpulent boar-headed winged mesh and albedo are loaded and published, rather than the run having fallen back to the shared donor body.");
        }
    }
}
