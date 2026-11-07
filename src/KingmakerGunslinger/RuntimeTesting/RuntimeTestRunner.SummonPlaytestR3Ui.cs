using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Repair R3 (UI-01..06): one save-free observation in the static interior.
        //  1. Whole registered roster, live library: each KMG summon unit's
        //     PortraitSafe small portrait is its own creature art; every donor
        //     unit's portrait object is untouched.
        //  2. Representative summons cast through their ordinary own-tier
        //     children: unit.Portrait.SmallPortrait (what the initiative
        //     tracker reads), no unit-level portrait written, and the affected
        //     actions' live name, icon and tooltip Duration/Saving Throw text.
        //  3. Best-effort native turn-based screen captures with each
        //     representative as the current unit, for visual inspection. The
        //     captures are evidence only, not a predicate.
        // Brains are inactive; no summon acts; no input; no save.
        private static readonly string[] SummonPlaytestR3Keys =
        { "dire-rat", "lantern-archon", "ghaele-azata", "nalfeshnee", "brachiosaurus", "cheetah", "trumpet-archon" };

        private static string R3Text(object value)
        {
            if (value == null) return "<null>";
            string text = value.ToString();
            return text ?? "<null>";
        }

        private static JObject R3Action(BlueprintUnitFact blueprint)
        {
            var ability = blueprint as BlueprintAbility;
            return new JObject {
                ["blueprint"] = blueprint.name, ["guid"] = blueprint.AssetGuid,
                ["name"] = blueprint.Name ?? "", ["icon"] = blueprint.Icon == null ? null : blueprint.Icon.name,
                ["duration"] = ability == null ? null : R3Text(ability.LocalizedDuration),
                ["savingThrow"] = ability == null ? null : R3Text(ability.LocalizedSavingThrow),
                ["durationObject"] = ability != null && ability.LocalizedDuration != null,
                ["savingThrowObject"] = ability != null && ability.LocalizedSavingThrow != null };
        }

        private IEnumerable<int> ReviewSummonPlaytestUiR3(ExpandedSummoningCorrectionFixture fixture, string phase)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || fixture.Hostile == null ||
                !_context.FeatureModules.Active.ExpandedSummoning ||
                phase != ExpandedSummoningNativeProfilePolicy.Sprint39PlaytestUiPhase)
                throw new InvalidOperationException("Exact fresh R3 UI fixture with the summoning module enabled required.");
            const string prefix = "r3-ui-";
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            IDictionary<string, BlueprintScriptableObject> symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId,
                ["limits"] = "Static interior. Portrait and action checks read the live blueprints and live units. Screen captures are best-effort evidence for visual inspection, not predicates. No summon acts." };
            var owned = new List<UnitEntityData>();
            Vector3 centre = fixture.Caster.Position;
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = Game.Instance.IsPaused;
            bool combatEntered = false;
            NativeIconScreenEvidence screens = null;
            var captures = new JArray(); var captureErrors = new JArray();
            try
            {
                // 1. Whole-roster portrait audit in the live library.
                var audit = new JArray(); int wrong = 0; var donorRows = new JArray(); int donorTouched = 0;
                var donorsSeen = new HashSet<string>(StringComparer.Ordinal);
                foreach (SummonCreatureSpec creature in SummonPortraitPolicy.PortraitCreatures())
                {
                    var unit = symbols[ExpandedSummoningIdentityCatalog.UnitSymbol(creature)] as BlueprintUnit;
                    BlueprintPortrait portrait = unit == null ? null : unit.PortraitSafe;
                    Sprite small = portrait == null ? null : portrait.Data.SmallPortrait;
                    Sprite art = ExpandedSummoningProjectIcons.Require(creature.Key);
                    bool exact = portrait != null && portrait.name == SummonPortraitPolicy.PortraitName(creature.Key) &&
                        small != null && small.name == SummonPortraitPolicy.PortraitName(creature.Key) &&
                        ReferenceEquals(small.texture, art.texture);
                    if (!exact) wrong++;
                    bool published = SummonIconCatalog.IsPublishedSomewhere(creature);
                    string donorGuid = ExpandedSummoningDonorCatalog.For(creature.Key).Guid;
                    audit.Add(new JObject { ["key"] = creature.Key, ["published"] = published,
                        ["unit"] = unit == null ? null : unit.name, ["unitGuid"] = unit == null ? null : unit.AssetGuid,
                        ["portrait"] = portrait == null ? null : portrait.name, ["small"] = small == null ? null : small.name,
                        ["smallRect"] = small == null ? null : small.rect.ToString(), ["donor"] = donorGuid, ["exact"] = exact });
                    if (donorsSeen.Add(donorGuid))
                    {
                        var donor = library.BlueprintsByAssetId.ContainsKey(donorGuid) ?
                            library.BlueprintsByAssetId[donorGuid] as BlueprintUnit : null;
                        BlueprintPortrait donorPortrait = donor == null ? null : donor.PortraitSafe;
                        Sprite donorSmall = donorPortrait == null ? null : donorPortrait.Data.SmallPortrait;
                        bool touched = donorPortrait != null && (donorPortrait.name.StartsWith(SummonPortraitPolicy.PortraitNamePrefix, StringComparison.Ordinal) ||
                            donorSmall != null && donorSmall.name.StartsWith(SummonPortraitPolicy.PortraitNamePrefix, StringComparison.Ordinal));
                        if (touched) donorTouched++;
                        donorRows.Add(new JObject { ["donor"] = donorGuid, ["unit"] = donor == null ? null : donor.name,
                            ["portrait"] = donorPortrait == null ? null : donorPortrait.name,
                            ["small"] = donorSmall == null ? null : donorSmall.name,
                            ["smallRect"] = donorSmall == null ? null : donorSmall.rect.ToString(), ["touched"] = touched });
                    }
                }
                record["portraitAudit"] = audit; record["donorPortraits"] = donorRows;
                Sprint25Check(prefix + "portrait-roster", wrong == 0 && audit.Count == ExpandedSummoningIdentityCatalog.UnitCount,
                    new JObject { ["units"] = audit.Count, ["wrong"] = wrong,
                        ["published"] = audit.Count(row => (bool)row["published"]) },
                    "Every registered KMG summon unit's PortraitSafe small portrait is its own creature art.");
                Sprint25Check(prefix + "donor-portraits-untouched", donorTouched == 0 && donorRows.Count > 0,
                    new JObject { ["donors"] = donorRows.Count, ["touched"] = donorTouched },
                    "No native donor unit's portrait object or sprite was replaced.");

                // 2. Representative live summons.
                var units = new Dictionary<string, UnitEntityData>(StringComparer.Ordinal);
                for (int index = 0; index < SummonPlaytestR3Keys.Length; index++)
                {
                    string key = SummonPlaytestR3Keys[index];
                    UnitEntityData summon = CastExpandedSummoningOwnTier(fixture, key);
                    owned.Add(summon); units[key] = summon;
                    Sprint28SetBrainActivePreservingActions(summon, false);
                    summon.Commands.InterruptAll(command => true); summon.Commands.RemoveFinishedAndUpdateQueue();
                    Vector3 offset = Quaternion.Euler(0f, index * (360f / SummonPlaytestR3Keys.Length), 0f) * Vector3.forward * 4f;
                    PlaceExpandedSummoningUnit(summon, ObstacleAnalyzer.GetNearestNode(centre + offset).clampedPosition);
                }
                for (int frame = 0; frame < 10; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                var live = new JObject(); bool portraitsLive = true;
                foreach (string key in SummonPlaytestR3Keys)
                {
                    UnitEntityData unit = units[key];
                    Sprite small = unit.Portrait == null ? null : unit.Portrait.SmallPortrait;
                    bool exact = small != null && small.name == SummonPortraitPolicy.PortraitName(key) &&
                        unit.UISettings.PortraitBlueprintRaw == null && unit.UISettings.CustomPortraitRaw == null;
                    portraitsLive &= exact;
                    var actions = new JArray();
                    foreach (var ability in unit.Descriptor.Abilities.Enumerable)
                        actions.Add(R3Action(ability.Blueprint));
                    foreach (var toggle in unit.Descriptor.ActivatableAbilities.Enumerable)
                        actions.Add(R3Action(toggle.Blueprint));
                    live[key] = new JObject { ["unit"] = unit.UniqueId, ["blueprint"] = unit.Blueprint.name,
                        ["initiativePortrait"] = small == null ? null : small.name, ["portraitExact"] = exact,
                        ["unitLevelPortrait"] = unit.UISettings.PortraitBlueprintRaw != null || unit.UISettings.CustomPortraitRaw != null,
                        ["actions"] = actions };
                }
                record["representatives"] = live;
                Sprint25Check(prefix + "initiative-portraits-live", portraitsLive, (JObject)live.DeepClone(),
                    "Each representative summon's unit.Portrait.SmallPortrait (the initiative tracker's source) is its own art, with no unit-level portrait written.");

                Func<string, string, JObject> actionOf = (key, symbol) =>
                {
                    var blueprint = symbols[symbol] as BlueprintUnitFact;
                    return R3Action(blueprint);
                };
                Func<JObject, bool> named = row => !string.IsNullOrEmpty((string)row["name"]) && row["icon"].Type != JTokenType.Null;
                Func<JObject, bool> rowsClean = row => (bool)row["durationObject"] && (bool)row["savingThrowObject"] &&
                    (string)row["duration"] != "<null>" && (string)row["savingThrow"] != "<null>";
                Func<UnitEntityData, string, bool> hasFact = (unit, symbol) =>
                    unit.Descriptor.HasFact((BlueprintUnitFact)symbols[symbol]);

                JObject light = actionOf("lantern-archon", "KMG.Summoning.Special.LanternArchon.LightRay");
                JObject twin = actionOf("lantern-archon", "KMG.Summoning.Special.LanternArchon.LightRayFull");
                Sprint25Check(prefix + "lantern-rays", named(light) && named(twin) &&
                    (string)light["icon"] != (string)twin["icon"] &&
                    hasFact(units["lantern-archon"], "KMG.Summoning.Special.LanternArchon.LightRay"),
                    new JObject { ["lightRay"] = light, ["twinLightRay"] = twin },
                    "Lantern Archon's Light Ray and Twin Light Rays are named and carry distinct native light-spell icons.");

                var ghaele = units["ghaele-azata"];
                var nativeRay = library.BlueprintsByAssetId["33e8997912cf76b4c99dca0445082804"] as BlueprintUnitFact;
                JObject clone = actionOf("ghaele-azata", ExpandedSummoningSpecialBuilder.GhaeleRaySymbol);
                var ghaeleBlank = new JArray(((JArray)live["ghaele-azata"]["actions"]).Where(row =>
                    string.IsNullOrEmpty((string)row["name"]) || row["icon"].Type == JTokenType.Null));
                bool brainUsesClone = ghaele.Brain != null && ghaele.Brain.Actions.Any(action =>
                    action.Blueprint is Kingmaker.Controllers.Brain.Blueprints.BlueprintAiCastSpell &&
                    ReferenceEquals(((Kingmaker.Controllers.Brain.Blueprints.BlueprintAiCastSpell)action.Blueprint).Ability,
                        symbols[ExpandedSummoningSpecialBuilder.GhaeleRaySymbol]));
                var ghaeleRow = new JObject { ["grant"] = ExpandedSummoningSpecialBuilder.GhaeleRayGrant,
                    ["nativeRay"] = R3Action(nativeRay), ["nativeRayOnKmgGhaele"] = ghaele.Descriptor.HasFact(nativeRay),
                    ["clone"] = clone, ["cloneOnKmgGhaele"] = hasFact(ghaele, ExpandedSummoningSpecialBuilder.GhaeleRaySymbol),
                    ["brainUsesClone"] = brainUsesClone, ["brain"] = ghaele.Blueprint.Brain == null ? null : ghaele.Blueprint.Brain.name,
                    ["blankActions"] = ghaeleBlank };
                record["ghaele"] = ghaeleRow;
                Sprint25Check(prefix + "ghaele-ray-named", named(clone) && !ghaele.Descriptor.HasFact(nativeRay) &&
                    hasFact(ghaele, ExpandedSummoningSpecialBuilder.GhaeleRaySymbol) && brainUsesClone && ghaeleBlank.Count == 0,
                    (JObject)ghaeleRow.DeepClone(),
                    "The KMG Ghaele's light ray is its owned, named, iconed clone (the blank native ray is no longer granted to it), its AI still uses the ray, and no Ghaele action is nameless or iconless.");

                JObject nimbus = actionOf("nalfeshnee", "KMG.Summoning.Demon.Nalfeshnee.UnholyNimbus");
                Sprint25Check(prefix + "nalfeshnee-nimbus", named(nimbus) && rowsClean(nimbus) &&
                    hasFact(units["nalfeshnee"], "KMG.Summoning.Demon.Nalfeshnee.UnholyNimbus"), (JObject)nimbus.DeepClone(),
                    "Unholy Nimbus has an icon and real Duration/Saving Throw text.");
                JObject overrun = actionOf("brachiosaurus", "KMG.Summoning.Special.Brachiosaurus.Overrun");
                Sprint25Check(prefix + "brachiosaurus-overrun", named(overrun) &&
                    hasFact(units["brachiosaurus"], "KMG.Summoning.Special.Brachiosaurus.Overrun"), (JObject)overrun.DeepClone(),
                    "Brachiosaurus Overrun has a maneuver icon.");
                JObject sprint = actionOf("cheetah", "KMG.Summoning.Special.Cheetah.Sprint");
                Sprint25Check(prefix + "cheetah-sprint", named(sprint) && rowsClean(sprint) &&
                    hasFact(units["cheetah"], "KMG.Summoning.Special.Cheetah.Sprint"), (JObject)sprint.DeepClone(),
                    "Cheetah Sprint has an icon and real Duration/Saving Throw text.");
                JObject blast = actionOf("trumpet-archon", "KMG.Summoning.OutsiderCelestial.TrumpetArchon.TrumpetBlast");
                Sprint25Check(prefix + "trumpet-blast-sibling", named(blast) && rowsClean(blast), (JObject)blast.DeepClone(),
                    "Sibling Trumpet Blast keeps its name and icon and now has real Duration/Saving Throw text.");
                // Sibling inspection: every representative action whose tooltip rows are absent objects.
                var nullRows = new JArray();
                foreach (var property in live.Properties())
                    foreach (JObject row in (JArray)property.Value["actions"])
                        if (row["durationObject"].Type == JTokenType.Boolean && (!(bool)row["durationObject"] || !(bool)row["savingThrowObject"]))
                            nullRows.Add(new JObject { ["creature"] = property.Name, ["blueprint"] = row["blueprint"],
                                ["duration"] = row["duration"], ["savingThrow"] = row["savingThrow"] });
                record["siblingNullRows"] = nullRows;

                // 3. Best-effort native turn-based captures (evidence only).
                try
                {
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = true;
                    Game.Instance.TurnBasedCombatController.Activate();
                    fixture.Caster.JoinCombat(); fixture.Hostile.JoinCombat();
                    foreach (UnitEntityData unit in owned) unit.JoinCombat();
                    Game.Instance.Player.UpdateIsInCombat();
                    Game.Instance.TurnBasedCombatController.HandlePartyCombatStateChanged(true);
                    combatEntered = true;
                    screens = new NativeIconScreenEvidence(_request);
                }
                catch (Exception error) { captureErrors.Add("enter:" + error.GetType().Name + ":" + error.Message); }
                var capturedKeys = new HashSet<string>(StringComparer.Ordinal);
                TurnController lastForced = null;
                for (int frame = 0; screens != null && frame < 2400 && capturedKeys.Count < SummonPlaytestR3Keys.Length; frame++)
                {
                    Game.Instance.IsPaused = false;
                    TurnController turn = Game.Instance.TurnBasedCombatController.CurrentTurn;
                    string key = turn == null ? null : units.Where(pair => ReferenceEquals(pair.Value, turn.Unit))
                        .Select(pair => pair.Key).FirstOrDefault();
                    if (turn != null && key != null && !capturedKeys.Contains(key) &&
                        (turn.Status == TurnController.TurnStatus.Preparing || turn.Status == TurnController.TurnStatus.Acting))
                    {
                        capturedKeys.Add(key);
                        var state = new JObject { ["creature"] = key, ["unit"] = turn.Unit.UniqueId,
                            ["round"] = Game.Instance.TurnBasedCombatController.RoundNumber, ["status"] = turn.Status.ToString(),
                            ["initiativePortrait"] = turn.Unit.Portrait?.SmallPortrait?.name };
                        IEnumerator<int> capture = null;
                        try { capture = screens.Capture("r3-turn-" + key, state, () => true).GetEnumerator(); }
                        catch (Exception error) { captureErrors.Add(key + ":" + error.GetType().Name + ":" + error.Message); }
                        while (capture != null)
                        {
                            bool more;
                            try { more = capture.MoveNext(); }
                            catch (Exception error) { captureErrors.Add(key + ":" + error.GetType().Name + ":" + error.Message); break; }
                            if (!more) break;
                            yield return 0;
                        }
                        captures.Add(state);
                    }
                    if (turn != null && !ReferenceEquals(turn, lastForced) &&
                        (key == null || capturedKeys.Contains(key)) && turn.Status != TurnController.TurnStatus.Scrolling)
                    {
                        lastForced = turn;
                        try { turn.ForceToEnd(true); }
                        catch (Exception error) { captureErrors.Add("force:" + error.GetType().Name); }
                    }
                    yield return 0;
                }
                record["captures"] = captures; record["captureErrors"] = captureErrors;
            }
            finally
            {
                try { screens?.AppendEvidence(new List<string>()); } catch { }
                try { screens?.Dispose(); } catch { }
                foreach (UnitEntityData unit in owned.Concat(new[] { fixture.Hostile, fixture.Caster })
                    .Where(value => value != null && !value.Destroyed))
                {
                    unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                    if (unit.IsInCombat) unit.LeaveCombat();
                }
                if (combatEntered)
                {
                    Game.Instance.Player.UpdateIsInCombat();
                    try { Game.Instance.TurnBasedCombatController.HandlePartyCombatStateChanged(false); } catch { }
                }
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
                PlaceExpandedSummoningUnit(fixture.Caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                record["captures"] = captures; record["captureErrors"] = captureErrors;
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"),
                    record.ToString(Formatting.Indented));
            }
        }
    }
}
