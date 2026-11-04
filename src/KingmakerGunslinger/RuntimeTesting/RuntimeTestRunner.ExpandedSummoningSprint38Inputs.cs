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
        /// The one bounded Sprint 38 installed-library check. It reads exact
        /// metadata at mod load and never creates an actor, loads a save,
        /// mutates a native blueprint or exports proprietary assets.
        ///
        /// <para>It runs before the mechanics candidate because four printed
        /// abilities are deliberately unwired until it names their installed
        /// identities. Guessing an asset GUID is not acceptable, and the one
        /// no-launch blueprint census available on this machine turned out to
        /// contain only the installed third-party mods' own additions - every
        /// known-native identity the Sprint 37 audit recorded is absent from
        /// it - so it establishes nothing about the installed game.</para>
        ///
        /// <list type="number">
        /// <item>whether the installed library carries a genuine Glabrezu unit
        /// and prefab, which would be referenced instead of authoring an
        /// original body;</item>
        /// <item>the exact identities of <i>confusion</i>, <i>true seeing</i>,
        /// <i>veil</i> and plain Vital Strike, which this project has never
        /// recorded, and whether a native Power Word Stun graph exists that
        /// would be more faithful than assembling the ability from the native
        /// stun carrier;</item>
        /// <item>what the library actually returns for gravity, pit and
        /// teleportation names, so the Reverse Gravity omission and the
        /// released travel exclusions rest on a live check rather than only on
        /// the absence of those names from the engine assembly.</item>
        /// </list>
        /// </summary>
        private RuntimeTestResult RunExpandedSummoningSprint38Inputs()
        {
            var checks = new List<RuntimeTestAssertion>();
            var document = new JObject {
                ["runId"] = _request.RunId,
                ["scope"] = "Sprint38 Glabrezu body, spell-graph, feat and gravity/pit audit; research only",
                ["searchLimits"] = "Name-substring searches over the loaded blueprint library, each capped at twenty matches. A query returning nothing establishes that nothing matched that name in this captured scope; it does not establish that no differently named asset or engine primitive exists."
            };
            try
            {
                BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                Func<string, string> normalized = name => new string(
                    (name ?? string.Empty).Where(char.IsLetterOrDigit)
                        .ToArray());

                // 1. Is there a native Glabrezu at all? The released
                // four-armed Girallon and Xill both authored original bodies
                // because the library has neither creature; this asks the
                // same question for this creature rather than assuming it.
                string[] species = { "Glabrezu" };
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
                        "Fixed Sprint38 exact-species audit exceeded 16 candidates.");
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
                                "sprint38-exact-species", unit.AssetGuid,
                                unit.name, unit.Prefab.AssetId);
                    }
                    row["graph"] = DescribeGraph(unit, 0,
                        new HashSet<object>(
                            NativeDonorReferenceComparer.Instance), 5);
                    unitRows.Add(row);
                }
                document["units"] = unitRows;
                document["nativeGlabrezuUnitCount"] = unitRows.Count;

                // 2. The printed abilities and feats whose installed identity
                // this sprint needs, and the dispositions that depend on a
                // live answer. Confusion, TrueSeeing, Veil and VitalStrike are
                // the four this sprint has deliberately left unwired.
                // PowerWordStun is queried because a faithful native graph, if
                // one exists, is preferable to assembling the ability from the
                // native stun carrier - and because every Power Word Stun name
                // on this machine belongs to an installed third-party mod, so
                // the installed game's own answer is unknown.
                // ReverseGravity, Gravity and Pit are queried so the omission
                // rests on a live check. GreaterTeleport is immaterial to the
                // decision - the released summon policy excludes travel for
                // every summon regardless - and is recorded anyway.
                string[] consumers = { "Confusion", "TrueSeeing", "Veil",
                    "VitalStrike", "PowerWordStun", "PowerWord", "Stunned",
                    "ImprovedCritical", "ReverseGravity", "Gravity",
                    "CreatePit", "Pit", "GreaterTeleport", "MirrorImage",
                    "DispelMagic" };
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

                // 3. Which shipped blueprints actually carry the engine's
                // AreaEffectPit component. This is a component scan rather
                // than a name search, so it answers the question a name
                // search cannot: whether the bounded vertical-capture
                // framework ships with usable donors at all.
                //
                // Reverse Gravity is OMITTED by the owner decision of
                // 2026-11-03 and this sprint builds nothing on this
                // framework. The row is captured because the offline
                // feasibility finding that informed that decision was read
                // from the engine assembly, and a live confirmation of what
                // ships is worth having on the record if the owner ever
                // revisits it.
                document["areaEffectPitCarriers"] = new JArray(
                    all.Where(value => value.ComponentsArray != null &&
                        value.ComponentsArray.Any(component =>
                            component is Kingmaker.UnitLogic.Abilities
                                .Components.AreaEffects.AreaEffectPit))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .Take(40).Select(value => new JObject {
                        ["name"] = value.name,
                        ["guid"] = value.AssetGuid,
                        ["type"] = value.GetType().FullName
                    }));

                // The printed contract this sprint has to reach, beside the
                // live evidence, so a later reader compares like with like.
                document["printedContract"] = new JObject {
                    ["hitDice"] = GlabrezuRules38Policy.GlabrezuHitDice,
                    ["spellLikeCasterLevel"] =
                        GlabrezuRules38Policy.SpellLikeCasterLevel,
                    ["naturalArmor"] = GlabrezuRules38Policy.NaturalArmor,
                    ["spellResistance"] =
                        GlabrezuRules38Policy.SpellResistance,
                    ["hitPoints"] = GlabrezuRules38Policy.HitPoints,
                    ["speedFeet"] = GlabrezuRules38Policy.SpeedFeet,
                    ["reachFeet"] = GlabrezuRules38Policy.ReachFeet,
                    ["pincerCriticalRollEdge"] =
                        GlabrezuRules38Policy.PincerCriticalRollEdge,
                    ["rendPincerCount"] =
                        GlabrezuRules38Policy.RendPincerCount,
                    ["powerWordStunUses"] =
                        GlabrezuRules38Policy.PowerWordStunUses,
                    ["powerWordStunCeiling"] =
                        GlabrezuRules38Policy.PowerWordStunHitPointCeiling,
                    ["powerWordStunBuckets"] = new JArray(
                        new[] { 50, 100, 150, 151 }.Select(hp => new JObject {
                            ["currentHitPoints"] = hp,
                            ["diceCount"] = GlabrezuRules38Policy
                                .PowerWordStunDiceCount(hp)
                        })),
                    ["reverseGravity"] = "omitted by owner decision 2026-11-03; nothing substitutes for it"
                };

                // The one authoritative demon set this sprint extends, so the
                // receipt names the seven identities the Vrock screech
                // exempts rather than leaving it to be inferred.
                SummonDemonIdentityCatalog.Validate();
                document["projectDemonIdentities"] = new JArray(
                    SummonDemonIdentityCatalog.UnitSymbols());
                document["projectDemonKeys"] = new JArray(
                    SummonDemonIdentityCatalog.CreatureKeys);

                checks.Add(Assertion("sprint38-native-input-capture",
                    "fixed installed input audit completes",
                    "units=" + unitRows.Count + ";consumerQueries=" +
                        graphs.Count + ";pitCarriers=" +
                        ((JArray)document["areaEffectPitCarriers"]).Count +
                        ";demons=" +
                        ((JArray)document["projectDemonIdentities"]).Count,
                    graphs.Count == consumers.Length &&
                    ((JArray)document["projectDemonIdentities"]).Count == 7,
                    "Absence or an unsuitable native graph remains explicit; this receipt is not gameplay proof."));
            }
            catch (Exception error)
            {
                document["exception"] =
                    DescribeExpandedSummoningCorrectionException(error);
                checks.Add(Assertion("sprint38-native-input-capture",
                    "bounded audit completes", error.ToString(), false,
                    "Unknown never establishes native suitability."));
            }
            string path = Path.Combine(_request.EvidenceDirectory,
                "sprint38-inputs.json");
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
