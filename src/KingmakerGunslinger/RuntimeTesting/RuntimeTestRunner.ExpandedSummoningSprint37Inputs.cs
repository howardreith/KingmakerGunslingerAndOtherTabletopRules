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
        /// The one bounded Sprint 37 installed-library check. It reads exact
        /// metadata at mod load and never creates an actor, loads a save,
        /// mutates a native blueprint or exports proprietary assets.
        ///
        /// <para>Three open questions are settled here with live evidence
        /// rather than inference, which is why this runs before the mechanics
        /// candidate rather than after it:</para>
        /// <list type="number">
        /// <item>whether the installed library carries a genuine Vrock unit
        /// and prefab, which would be referenced directly instead of authoring
        /// an original body;</item>
        /// <item>whether any demon subtype fact exists, because the screech
        /// exemption currently uses an exact enumerated identity set for the
        /// same reason the released archon and goblinoid exemptions do;</item>
        /// <item>the exact identities of the installed Heroism and Mirror
        /// Image graphs, which this project has never recorded. Guessing an
        /// asset GUID is not acceptable, so the two printed once-per-day
        /// spell-like abilities are wired after this receipt names them.</item>
        /// </list>
        /// </summary>
        private RuntimeTestResult RunExpandedSummoningSprint37Inputs()
        {
            var checks = new List<RuntimeTestAssertion>();
            var document = new JObject {
                ["runId"] = _request.RunId,
                ["scope"] = "Sprint37 fixed Vrock body, demon-identity, spell-graph and Spores-counterplay audit; research only",
                ["searchLimits"] = "Name-substring searches over the loaded blueprint library, each capped at twenty matches and eighty subtype candidates. A query returning nothing establishes that nothing matched that name in this captured scope; it does not establish that no differently named asset or engine primitive exists."
            };
            try
            {
                BlueprintScriptableObject[] all = BlueprintBootstrap.Library
                    .GetAllBlueprints().Where(value => value != null).ToArray();
                Func<string, string> normalized = name => new string(
                    (name ?? string.Empty).Where(char.IsLetterOrDigit)
                        .ToArray());

                // 1. Is there a native Vrock at all? A name in an old search
                // list or another game's screenshot does not establish one.
                string[] species = { "Vrock" };
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
                        "Fixed Sprint37 exact-species audit exceeded 16 candidates.");
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
                                "sprint37-exact-species", unit.AssetGuid,
                                unit.name, unit.Prefab.AssetId);
                    }
                    row["graph"] = DescribeGraph(unit, 0,
                        new HashSet<object>(
                            NativeDonorReferenceComparer.Instance), 5);
                    unitRows.Add(row);
                }
                document["units"] = unitRows;
                document["nativeVrockUnitCount"] = unitRows.Count;

                // 2. Every candidate demon subtype fact. The released archon
                // and goblinoid exemptions are enumerated identity sets for
                // want of a subtype fact; if one exists this receipt names it.
                BlueprintUnitFact[] demonFacts = all.OfType<BlueprintUnitFact>()
                    .Where(value => value.name != null &&
                        !value.name.StartsWith("KMG_",
                            StringComparison.Ordinal) &&
                        (value.name.IndexOf("Demon",
                             StringComparison.OrdinalIgnoreCase) >= 0 ||
                         value.name.IndexOf("Subtype",
                             StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .Take(80).ToArray();
                document["demonIdentityCandidates"] = new JArray(
                    demonFacts.Select(value => new JObject {
                        ["name"] = value.name,
                        ["guid"] = value.AssetGuid,
                        ["type"] = value.GetType().FullName,
                        ["components"] = new JArray(
                            (value.ComponentsArray ??
                             Array.Empty<BlueprintComponent>())
                            .Where(component => component != null)
                            .Select(component =>
                                component.GetType().FullName))
                    }));
                // The exact enumerated set the production exemption uses, so a
                // reader can compare the two side by side.
                document["enumeratedProjectDemons"] = new JArray(
                    new[] { "Dretch", "Babau", "Hezrou", "Vrock" }
                    .Select(token => "KMG_Summoning_Unit_" + token));

                // 3. The printed spell-like abilities and the dispositions.
                // Heroism and Mirror Image are wired from whatever this names;
                // Telekinesis is expected to be absent and is queried anyway,
                // because an omission should rest on a live check and not only
                // on the absence of the name in the engine assembly.
                // The printed Spores entry says the spores "can be
                // destroyed by casting bless on the affected creatures or
                // by sprinkling them with holy water" and that the attack
                // "can also be halted by effects that remove or provide
                // immunity to disease". The disease half is already wired
                // through the native Disease descriptor; these queries
                // establish what the bless and holy-water halves can
                // actually attach to before any code is written for them.
                string[] consumers = { "Heroism", "MirrorImage",
                    "Telekinesis", "Stunned", "Stun", "GreaterTeleport",
                    "Bless", "HolyWater", "RemoveDisease", "Disease" };
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
                // The two native components that make a Disease-tagged
                // buff respect the printed counterplay without any new
                // code, recorded here by their live presence rather than
                // assumed from the engine assembly alone.
                document["diseaseDescriptorConsumers"] = new JArray(
                    all.Where(value => value.ComponentsArray != null &&
                        value.ComponentsArray.Any(component =>
                            component != null &&
                            (component is Kingmaker.UnitLogic.FactLogic
                                 .BuffDescriptorImmunity ||
                             component is Kingmaker.UnitLogic.FactLogic
                                 .SpellImmunityToSpellDescriptor)))
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .Take(40).Select(value => new JObject {
                        ["name"] = value.name,
                        ["guid"] = value.AssetGuid,
                        ["type"] = value.GetType().FullName,
                        ["descriptorComponents"] = new JArray(
                            value.ComponentsArray
                            .Where(component => component != null &&
                                (component is Kingmaker.UnitLogic.FactLogic
                                     .BuffDescriptorImmunity ||
                                 component is Kingmaker.UnitLogic.FactLogic
                                     .SpellImmunityToSpellDescriptor))
                            .Select(component =>
                                component.GetType().FullName))
                    }));

                document["consumers"] = graphs;

                // The printed profile this sprint has to reach, recorded beside
                // the live evidence so a later reader compares like with like.
                document["printedContract"] = new JObject {
                    ["hitDice"] = VrockRules37Policy.VrockHitDice,
                    ["screechDc"] = VrockRules37Policy.PrintedScreechDc,
                    ["screechRadiusFeet"] =
                        VrockRules37Policy.ScreechRadiusFeet,
                    ["screechRechargeHours"] =
                        VrockRules37Policy.ScreechRechargeHours,
                    ["sporeCadenceRounds"] =
                        VrockRules37Policy.SporeCadenceRounds,
                    ["sporeVineRounds"] = VrockRules37Policy.SporeVineRounds,
                    ["effectiveFlySpeedFeet"] =
                        VrockRules37Policy.EffectiveFlySpeedFeet,
                    ["omittedLandSpeedFeet"] =
                        VrockRules37Policy.OmittedLandSpeedFeet,
                    ["spellLikeCasterLevel"] =
                        VrockRules37Policy.SpellLikeCasterLevel
                };

                checks.Add(Assertion("sprint37-native-input-capture",
                    "fixed installed input audit completes",
                    "units=" + unitRows.Count + ";demonCandidates=" +
                        demonFacts.Length + ";consumerQueries=" +
                        graphs.Count + ";diseaseImmunityCarriers=" +
                        ((JArray)document["diseaseDescriptorConsumers"])
                            .Count,
                    graphs.Count == consumers.Length,
                    "Absence or an unsuitable native graph remains explicit; this receipt is not gameplay proof."));
            }
            catch (Exception error)
            {
                document["exception"] =
                    DescribeExpandedSummoningCorrectionException(error);
                checks.Add(Assertion("sprint37-native-input-capture",
                    "bounded audit completes", error.ToString(), false,
                    "Unknown never establishes native suitability."));
            }
            string path = Path.Combine(_request.EvidenceDirectory,
                "sprint37-inputs.json");
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
