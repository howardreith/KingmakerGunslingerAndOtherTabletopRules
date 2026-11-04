using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// The one bounded Sprint 39 installed-library check. It reads exact
        /// metadata at mod load and never creates an actor, loads a save,
        /// mutates a native blueprint or exports proprietary assets.
        ///
        /// <para>Deliberately narrow. The Sprint 39 intake already settled,
        /// offline from the private reference assembly and with no guarded
        /// transaction spent, every question that the engine assembly can
        /// answer on its own:</para>
        /// <list type="bullet">
        /// <item><c>UnholyGrace</c> exists and carries <b>only</b>
        /// <c>m_AcModifier</c> and <c>m_ReflexModifier</c>, so it supplies two
        /// of the printed aura's branches and not the Fortitude and Will
        /// resistance bonuses, the conditional spell resistance or the
        /// Strength-damage response;</item>
        /// <item><c>SpellResistanceAgainstAlignment</c> exists and takes a
        /// <c>ContextValue</c> and an <c>AlignmentComponent</c>, which is the
        /// aura's conditional resistance against good spells and good
        /// casters - a different fact from this creature's own general
        /// resistance, which merely prints the same number;</item>
        /// <item><c>ForbidSpellCasting</c> exists, with a
        /// <c>ForbidMagicItems</c> flag, which is what feeblemind's
        /// prohibition needs;</item>
        /// <item><c>ContextActionDispelMagic</c> supports exactly <b>one</b>
        /// removal (<c>m_StopAfterFirstRemoved</c> true) or <b>every</b>
        /// eligible removal (false). It has no success-count field at all, so
        /// neither setting is the printed greater form's one-per-four-caster-
        /// levels cap;</item>
        /// <item>there is no <c>ContextActionDealStatDamage</c>, only
        /// <c>RuleDealStatDamage</c>, while healing stat damage <i>does</i>
        /// have a native action.</item>
        /// </list>
        ///
        /// <para>So this scenario asks only what the loaded blueprint library
        /// can answer and the assembly cannot - which shipped graphs and
        /// identities actually exist:</para>
        /// <list type="number">
        /// <item>whether the installed library carries a genuine Nalfeshnee
        /// unit and prefab, which would be referenced instead of authoring an
        /// original body;</item>
        /// <item>whether a native <i>feeblemind</i> graph exists, and what
        /// carries its condition, ability-score change, casting prohibition,
        /// arcane save penalty and cure - and which cure consumers ship at
        /// all, since the printed spell names cures this game may not have;</item>
        /// <item>whether a native <i>unholy aura</i> graph exists and which
        /// shipped blueprints actually carry <c>UnholyGrace</c>, which is a
        /// component scan rather than a name search and so answers what a
        /// name search cannot;</item>
        /// <item>whether a native <i>greater dispel magic</i> graph exists and
        /// which of its targeted, area and counterspell forms the installed
        /// game exposes;</item>
        /// <item>whether a native Improved Critical (bite) feature exists,
        /// which decides whether the printed 19-20 bite threat range comes
        /// from a feat or from the weapon's own critical edge. It must come
        /// from exactly one of them.</item>
        /// </list>
        ///
        /// <para>A query returning nothing establishes that nothing matched
        /// that name in this captured scope. It does not establish that no
        /// differently named asset exists, and it is not gameplay proof.</para>
        /// </summary>
        private RuntimeTestResult RunExpandedSummoningSprint39Inputs()
        {
            var checks = new List<RuntimeTestAssertion>();
            var document = new JObject {
                ["runId"] = _request.RunId,
                ["scope"] = "Sprint39 Nalfeshnee body, feeblemind, unholy aura, greater dispel and bite-critical audit; research only",
                ["searchLimits"] = "Name-substring searches over the loaded blueprint library, each capped at twenty matches. A query returning nothing establishes that nothing matched that name in this captured scope; it does not establish that no differently named asset or engine primitive exists.",
                ["settledOffline"] = "UnholyGrace carries only AC and Reflex modifiers; SpellResistanceAgainstAlignment supplies the aura's conditional resistance; ForbidSpellCasting supplies feeblemind's prohibition; ContextActionDispelMagic offers only one-removal or every-removal and has no success-count field; there is no ContextActionDealStatDamage but ContextActionHealStatDamage exists. Read from the private reference assembly, not from this run."
            };
            try
            {
                BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                Func<string, string> normalized = name => new string(
                    (name ?? string.Empty).Where(char.IsLetterOrDigit)
                        .ToArray());

                // 1. Is there a native Nalfeshnee at all? Every released
                // demon in this series authored an original body because the
                // library had no such creature; this asks the same question
                // for this creature rather than assuming either answer.
                string[] species = { "Nalfeshnee" };
                BlueprintUnit[] units = all.OfType<BlueprintUnit>()
                    .Where(value => value.name != null &&
                        !value.name.StartsWith("KMG_",
                            StringComparison.Ordinal) &&
                        species.Any(name => normalized(value.name).IndexOf(
                            name, StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .ToArray();
                if (units.Length > 16)
                    throw new InvalidOperationException(
                        "Fixed Sprint39 exact-species audit exceeded 16 candidates.");
                var unitRows = new JArray();
                foreach (BlueprintUnit unit in units)
                {
                    var row = new JObject {
                        ["nativeUnit"] = DescribeNativeUnit(unit) };
                    if (unit.Prefab != null &&
                        !string.IsNullOrEmpty(unit.Prefab.AssetId))
                    {
                        var prefab = unit.Prefab.Load(false);
                        row["view"] = prefab == null ? null :
                            CapturePrimateCensusViewMetadata(prefab,
                                "sprint39-exact-species", unit.AssetGuid,
                                unit.name, unit.Prefab.AssetId);
                    }
                    row["graph"] = DescribeGraph(unit, 0,
                        new HashSet<object>(
                            NativeDonorReferenceComparer.Instance), 5);
                    unitRows.Add(row);
                }
                document["units"] = unitRows;
                document["nativeNalfeshneeUnitCount"] = unitRows.Count;

                // 2. The graphs and identities whose installed existence this
                // sprint genuinely does not know.
                //
                // Feeblemind and its cures: the printed spell is removed only
                // by heal, limited wish, miracle or wish, and this game may
                // ship none of the last three. Restoration and Break
                // Enchantment are queried because they are the cures this
                // game does have, so the gap between the printed cure list
                // and the available one is recorded from evidence rather than
                // assumed in either direction.
                //
                // UnholyAura and HolyAura: the printed constant aura. Holy
                // Aura is its mirror and is queried because if either ships,
                // its graph shows how this engine assembles the family.
                //
                // DispelMagicGreater: the greater form. Dispel is queried
                // alongside it so the ordinary form this project already
                // consumes appears in the same receipt for comparison.
                //
                // ImprovedCritical: this project already consumes a native
                // Improved Critical (claw) at 76a335b7d69691c4e8376f9379338778,
                // so a bite variant plausibly ships. Whichever answer comes
                // back, the printed 19-20 is delivered exactly once.
                string[] consumers = { "Feeblemind", "Feeble",
                    "UnholyAura", "HolyAura", "DispelMagicGreater",
                    "GreaterDispel", "Dispel", "ImprovedCritical",
                    "Restoration", "BreakEnchantment", "Heal",
                    "CallLightning", "Slow", "Dazed", "Nalfeshnee" };
                var graphs = new JArray();
                foreach (string name in consumers)
                {
                    BlueprintScriptableObject[] matches = all.Where(value =>
                        value.name != null &&
                        !value.name.StartsWith("KMG_",
                            StringComparison.Ordinal) &&
                        value.name.IndexOf(name,
                            StringComparison.OrdinalIgnoreCase) >= 0)
                        .OrderBy(value => !string.Equals(value.name, name,
                            StringComparison.OrdinalIgnoreCase))
                        .ThenBy(value => value.name, StringComparer.Ordinal)
                        .Take(20).ToArray();
                    graphs.Add(new JObject {
                        ["query"] = name,
                        ["exactNameMatches"] = new JArray(matches
                            .Where(value => string.Equals(value.name, name,
                                StringComparison.OrdinalIgnoreCase))
                            .Select(value => new JObject {
                                ["name"] = value.name,
                                ["guid"] = value.AssetGuid,
                                ["type"] = value.GetType().FullName
                            })),
                        ["matches"] = new JArray(matches.Select(value =>
                            new JObject {
                                ["name"] = value.name,
                                ["guid"] = value.AssetGuid,
                                ["type"] = value.GetType().FullName,
                                ["isAbility"] = value is BlueprintAbility,
                                ["graph"] = DescribeGraph(value, 0,
                                    new HashSet<object>(
                                        NativeDonorReferenceComparer.Instance),
                                    8)
                            }).ToArray())
                    });
                }
                document["consumers"] = graphs;

                // 3. Which shipped blueprints actually carry UnholyGrace, and
                // which carry the alignment-conditional spell resistance. This
                // is a component scan rather than a name search, so it answers
                // the question a name search cannot: whether the printed
                // aura's branches have real shipped donors, and how a shipped
                // consumer configures the two modifiers UnholyGrace owns.
                document["unholyGraceCarriers"] = new JArray(
                    all.Where(value => value.ComponentsArray != null &&
                        value.ComponentsArray.Any(component =>
                            component is Kingmaker.UnitLogic.FactLogic
                                .UnholyGrace))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .Take(40).Select(value => new JObject {
                        ["name"] = value.name,
                        ["guid"] = value.AssetGuid,
                        ["type"] = value.GetType().FullName,
                        ["graph"] = DescribeGraph(value, 0,
                            new HashSet<object>(
                                NativeDonorReferenceComparer.Instance), 6)
                    }));
                document["alignmentSpellResistanceCarriers"] = new JArray(
                    all.Where(value => value.ComponentsArray != null &&
                        value.ComponentsArray.Any(component =>
                            component is Kingmaker.UnitLogic.FactLogic
                                .SpellResistanceAgainstAlignment))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .Take(40).Select(value => new JObject {
                        ["name"] = value.name,
                        ["guid"] = value.AssetGuid,
                        ["type"] = value.GetType().FullName,
                        ["graph"] = DescribeGraph(value, 0,
                            new HashSet<object>(
                                NativeDonorReferenceComparer.Instance), 6)
                    }));
                document["forbidSpellCastingCarriers"] = new JArray(
                    all.Where(value => value.ComponentsArray != null &&
                        value.ComponentsArray.Any(component =>
                            component is Kingmaker.UnitLogic.FactLogic
                                .ForbidSpellCasting))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .Take(40).Select(value => new JObject {
                        ["name"] = value.name,
                        ["guid"] = value.AssetGuid,
                        ["type"] = value.GetType().FullName,
                        ["graph"] = DescribeGraph(value, 0,
                            new HashSet<object>(
                                NativeDonorReferenceComparer.Instance), 6)
                    }));

                // The printed contract this sprint has to reach, beside the
                // live evidence, so a later reader compares like with like.
                // Every value is derived by the policy from the printed entry;
                // none is a pinned outcome.
                NalfeshneeRules39Policy.Validate();
                document["printedContract"] = new JObject {
                    ["hitDice"] = NalfeshneeRules39Policy.NalfeshneeHitDice,
                    ["spellLikeCasterLevel"] =
                        NalfeshneeRules39Policy.SpellLikeCasterLevel,
                    ["naturalArmor"] = NalfeshneeRules39Policy.NaturalArmor,
                    ["racialArmorClassWithoutAura"] = NalfeshneeRules39Policy
                        .RacialArmorClassWithoutAura,
                    ["printedArmorClass"] =
                        NalfeshneeRules39Policy.PrintedArmorClass,
                    ["generalSpellResistance"] =
                        NalfeshneeRules39Policy.SpellResistance,
                    ["auraConditionalSpellResistance"] =
                        NalfeshneeRules39Policy
                            .AuraConditionalSpellResistance,
                    ["hitPoints"] = NalfeshneeRules39Policy.HitPoints,
                    ["speedFeet"] = NalfeshneeRules39Policy.SpeedFeet,
                    ["flySpeedFeet"] =
                        NalfeshneeRules39Policy.FlySpeedFeet,
                    ["reachFeet"] = NalfeshneeRules39Policy.ReachFeet,
                    ["biteCriticalRollEdge"] =
                        NalfeshneeRules39Policy.BiteCriticalRollEdge,
                    ["racialSavesWithoutAura"] = new JObject {
                        ["fortitude"] = NalfeshneeRules39Policy
                            .RacialFortitudeWithoutAura,
                        ["reflex"] = NalfeshneeRules39Policy
                            .RacialReflexWithoutAura,
                        ["will"] = NalfeshneeRules39Policy
                            .RacialWillWithoutAura
                    },
                    ["printedSaves"] = new JObject {
                        ["fortitude"] =
                            NalfeshneeRules39Policy.PrintedFortitude,
                        ["reflex"] = NalfeshneeRules39Policy.PrintedReflex,
                        ["will"] = NalfeshneeRules39Policy.PrintedWill
                    },
                    ["nimbus"] = new JObject {
                        ["usesPerDay"] =
                            NalfeshneeRules39Policy.NimbusUsesPerDay,
                        ["radiusFeet"] =
                            NalfeshneeRules39Policy.NimbusRadiusFeet,
                        ["delayRounds"] =
                            NalfeshneeRules39Policy.NimbusDelayRounds,
                        ["dazeDice"] =
                            NalfeshneeRules39Policy.NimbusDazeDiceCount + "d" +
                            NalfeshneeRules39Policy.NimbusDazeDieSides,
                        ["derivedDc"] = NalfeshneeRules39Policy
                            .SupernaturalDifficultyClass(
                                NalfeshneeRules39Policy.NalfeshneeHitDice,
                                NalfeshneeRules39Policy.CharismaBonus)
                    },
                    ["derivedSpellLikeDcs"] = new JObject {
                        ["callLightning"] = NalfeshneeRules39Policy
                            .SpellDifficultyClass(NalfeshneeRules39Policy
                                .CallLightningSpellLevel,
                                NalfeshneeRules39Policy.CharismaBonus),
                        ["feeblemind"] = NalfeshneeRules39Policy
                            .SpellDifficultyClass(NalfeshneeRules39Policy
                                .FeeblemindSpellLevel,
                                NalfeshneeRules39Policy.CharismaBonus),
                        ["slow"] = NalfeshneeRules39Policy
                            .SpellDifficultyClass(NalfeshneeRules39Policy
                                .SlowSpellLevel,
                                NalfeshneeRules39Policy.CharismaBonus),
                        ["unholyAura"] = NalfeshneeRules39Policy
                            .SpellDifficultyClass(NalfeshneeRules39Policy
                                .UnholyAuraSpellLevel,
                                NalfeshneeRules39Policy.CharismaBonus)
                    },
                    ["derivedRangesFeet"] = new JObject {
                        ["medium"] = NalfeshneeRules39Policy.MediumRangeFeet(
                            NalfeshneeRules39Policy.SpellLikeCasterLevel),
                        ["long"] = NalfeshneeRules39Policy.LongRangeFeet(
                            NalfeshneeRules39Policy.SpellLikeCasterLevel)
                    },
                    ["greaterDispelMaxRemovals"] = NalfeshneeRules39Policy
                        .GreaterDispelMaxRemovals(NalfeshneeRules39Policy
                            .SpellLikeCasterLevel),
                    ["excludedByCharter"] = new JArray(
                        "greater teleport (planar travel)",
                        "summon (demon reinforcements)")
                };

                // The one authoritative demon set this sprint extends. Its own
                // Validate() owns the invariant, so no count is pinned here;
                // the identities are recorded so the receipt names the set the
                // Nimbus exempts rather than leaving it to be inferred.
                SummonDemonIdentityCatalog.Validate();
                document["projectDemonIdentities"] = new JArray(
                    SummonDemonIdentityCatalog.UnitSymbols());
                document["projectDemonKeys"] = new JArray(
                    SummonDemonIdentityCatalog.CreatureKeys);

                checks.Add(Assertion("sprint39-native-input-capture",
                    "fixed installed input audit completes",
                    "units=" + unitRows.Count + ";consumerQueries=" +
                        graphs.Count + ";unholyGraceCarriers=" +
                        ((JArray)document["unholyGraceCarriers"]).Count +
                        ";alignmentSrCarriers=" +
                        ((JArray)document["alignmentSpellResistanceCarriers"])
                            .Count + ";forbidCastingCarriers=" +
                        ((JArray)document["forbidSpellCastingCarriers"])
                            .Count + ";demons=" +
                        ((JArray)document["projectDemonIdentities"]).Count,
                    graphs.Count == consumers.Length,
                    "Absence or an unsuitable native graph remains explicit; this receipt is not gameplay proof."));
            }
            catch (Exception error)
            {
                document["exception"] =
                    DescribeExpandedSummoningCorrectionException(error);
                checks.Add(Assertion("sprint39-native-input-capture",
                    "bounded audit completes", error.ToString(), false,
                    "Unknown never establishes native suitability."));
            }
            string path = Path.Combine(_request.EvidenceDirectory,
                "sprint39-inputs.json");
            RuntimeTestResultWriter.WriteAtomic(path,
                document.ToString(Formatting.Indented));
            var result = CreateResult(checks.All(value =>
                value.Status == RuntimeTestStatuses.Pass) ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                checks, null);
            result.EvidenceFiles.Add(path);
            return result;
        }
    }
}
