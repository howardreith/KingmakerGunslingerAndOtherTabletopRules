using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Repair R5 (SPELL-01..03, MECH-01..02), one save-free phase in the
        // established Oleg outdoor native Default fixture. Loaded
        // configuration first; then commanded native executions (brains
        // inactive, controlled failed saves on the disposable hostile):
        // Mastodon trample, the Cloud Giant's provider Obscuring Mist at a
        // point (twice: at will), the Pixie's four spell-likes (each twice)
        // and its once-per-summoning provider Dance; the Griffon's live
        // Charge count; native description windows. No input, no save.
        private const string R5DanceBuffGuid = "4d283e0b70fb489ba79e69387818c3f3";

        // Controlled roll: the delivering unit's attack d20 is seeded (natural
        // 20) so the native touch delivery is observed deterministically.
        private sealed class R5AttackSeed : IGlobalRulebookHandler<Kingmaker.RuleSystem.Rules.RuleAttackRoll>, IDisposable
        {
            private readonly UnitEntityData _attacker; internal readonly JArray Rows = new JArray();
            internal R5AttackSeed(UnitEntityData attacker) { _attacker = attacker; EventBus.Subscribe(this); }
            public void OnEventAboutToTrigger(Kingmaker.RuleSystem.Rules.RuleAttackRoll evt)
            { if (ReferenceEquals(evt.Initiator, _attacker)) UnityEngine.Random.InitState(FindNativeD20Seed(20)); }
            public void OnEventDidTrigger(Kingmaker.RuleSystem.Rules.RuleAttackRoll evt)
            { if (ReferenceEquals(evt.Initiator, _attacker)) Rows.Add(new JObject { ["touch"] = evt.AttackType.ToString(), ["result"] = evt.Result.ToString(), ["hit"] = evt.IsHit, ["controlled"] = true }); }
            public void Dispose() { EventBus.Unsubscribe(this); }
        }

        private IEnumerable<int> R5RunData(UnitEntityData owner, AbilityData data, TargetWrapper target, JObject row)
        {
            row["blueprint"] = data.Blueprint.AssetGuid; row["blueprintName"] = data.Blueprint.name;
            row["actionType"] = data.Blueprint.ActionType.ToString();
            row["availableBefore"] = data.IsAvailable; row["canTarget"] = data.CanTarget(target);
            if (!data.IsAvailable || !data.CanTarget(target))
                throw new InvalidOperationException("Native ability unavailable: " + data.Blueprint.name);
            var command = new UnitUseAbility(data, target);
            row["startFrame"] = Time.frameCount;
            owner.Commands.Run(command);
            DateTime deadline = DateTime.UtcNow.AddSeconds(25);
            int frames = 0;
            while (frames++ < 3000 && DateTime.UtcNow < deadline)
            {
                if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Tick();
                yield return 0;
                if (command.IsFinished && command.ExecutionProcess != null && command.ExecutionProcess.IsEnded) break;
            }
            row["finished"] = command.IsFinished;
            row["executionEnded"] = command.ExecutionProcess != null && command.ExecutionProcess.IsEnded;
            row["endFrame"] = Time.frameCount;
            if (!command.IsFinished || command.ExecutionProcess == null || !command.ExecutionProcess.IsEnded)
                throw new InvalidOperationException("Native ability did not finish: " + data.Blueprint.name);
        }

        private static Kingmaker.EntitySystem.Entities.AreaEffectEntityData[] R5Areas(string areaGuid, UnitEntityData caster)
        {
            return Game.Instance.State.AreaEffects.All.Where(area => area != null && area.Blueprint != null &&
                area.Blueprint.AssetGuid == areaGuid && area.Context != null && ReferenceEquals(area.Context.MaybeCaster, caster)).ToArray();
        }

        private IEnumerable<int> ReviewSummonPlaytestSpellsR5(ExpandedSummoningCorrectionFixture fixture, string phase,
            Cleanup2NativeDefaultScope mode)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || fixture.Hostile == null || mode == null || !mode.Current ||
                !_context.FeatureModules.Active.ExpandedSummoning ||
                phase != ExpandedSummoningNativeProfilePolicy.Sprint39PlaytestSpellsPhase ||
                !ExpandedSummoningNativeProfilePolicy.Sprint32NativeDefaultOutdoor(_request.Scenario, phase) ||
                !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Exact fresh R5 spells fixture in owned outdoor native Default, without narrative, required.");
            const string prefix = "r5-spells-";
            var game = Game.Instance; var ui = game.UI;
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            IDictionary<string, BlueprintScriptableObject> symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId,
                ["fixture"] = "established non-DLC Oleg outdoor native Default; save-free",
                ["limits"] = "Commanded native executions with brains inactive; AI-action references are configuration, not observed autonomous casts. Saves on the disposable hostile are controlled failures. Provider-absent omission is not observed here (installed profile has Call of the Wild). Screenshots support presentation only." };
            var owned = new List<UnitEntityData>();
            Vector3 centre = fixture.Caster.Position;
            var hostile = fixture.Hostile;
            Vector3 hostilePosition = hostile.Position;
            NativeIconScreenEvidence screens = null;
            TooltipData openDescription = null;
            R4MephitRecorder recorder = null; bool subscribed = false;
            Sprint26Saves saves = null;
            R5AttackSeed attackSeed = null;
            Func<bool> worldOwned = () => ExpandedSummoningNativeProfilePolicy.SummonPlaytestR3SurfaceOwned(mode.Current,
                Cleanup2NoNarrativeOverlay(), R3UnrelatedModal(), !ui.DescriptionController.DescWindow.gameObject.activeInHierarchy);
            Func<string, BlueprintAbility> abilityOf = symbol => (BlueprintAbility)symbols[symbol];
            try
            {
                // 1. Loaded configuration.
                var cloud = (BlueprintUnit)symbols["KMG.Summoning.Unit.CloudGiant"];
                var pixieUnit = (BlueprintUnit)symbols["KMG.Summoning.Unit.Pixie"];
                var mastodonUnit = (BlueprintUnit)symbols["KMG.Summoning.Unit.Mastodon"];
                var griffonUnit = (BlueprintUnit)symbols["KMG.Summoning.Unit.Griffon"];
                BlueprintAbility mist = abilityOf(ExpandedSummoningSpecialBuilder.CloudMistSymbolR5);
                BlueprintAbility fog = abilityOf("KMG.Summoning.Giant.Sprint29.CloudFogCloud");
                var mistAi = (BlueprintAiCastSpell)symbols[ExpandedSummoningSpecialBuilder.CloudMistAiSymbolR5];
                var fogAi = (BlueprintAiCastSpell)symbols["KMG.Summoning.Giant.Sprint29.CloudFogAi"];
                BlueprintAbility dance = abilityOf("KMG.Summoning.Special.Pixie.IrresistibleDance");
                BlueprintAbility trample = abilityOf(SummonPlaytestR5Policy.MastodonTrampleSymbol);
                var pixieGrant = pixieUnit.ComponentsArray.OfType<AddAbilityToCharacterComponent>().Single().Abilities;
                var pixieBrainAbilities = pixieUnit.Brain.Actions.OfType<BlueprintAiCastSpell>().Select(value => value.Ability).ToArray();
                var spellLikes = SummonPlaytestR5Policy.PixieAtWill.Select(spec => abilityOf(spec.Symbol)).ToArray();
                var mistSpawn = (mist.ComponentsArray ?? new BlueprintComponent[0]).OfType<AbilityEffectRunAction>()
                    .SelectMany(run => run.Actions.Actions.OfType<ContextActionSpawnAreaEffect>()).ToArray();
                var sticky = (dance.ComponentsArray ?? new BlueprintComponent[0]).OfType<AbilityEffectStickyTouch>().ToArray();
                var config = new JObject {
                    ["provider"] = R5CotwSpells.Disposition, ["mist"] = R5CotwSpells.MistDisposition, ["dance"] = R5CotwSpells.DanceDisposition,
                    ["cloudMistGrants"] = cloud.AddFacts.Count(value => ReferenceEquals(value, mist)),
                    ["cloudFogGrants"] = cloud.AddFacts.Count(value => ReferenceEquals(value, fog)),
                    ["cloudBrainMist"] = cloud.Brain.Actions.Count(value => ReferenceEquals(value, mistAi)),
                    ["cloudBrainFog"] = cloud.Brain.Actions.Count(value => ReferenceEquals(value, fogAi)),
                    ["mistAiAbility"] = mistAi.Ability == null ? null : mistAi.Ability.AssetGuid,
                    ["mistHidden"] = mist.Hidden, ["mistType"] = mist.Type.ToString(), ["mistRange"] = mist.Range.ToString(),
                    ["mistArea"] = mistSpawn.Length == 1 && mistSpawn[0].AreaEffect != null ? mistSpawn[0].AreaEffect.AssetGuid : null,
                    ["danceTouch"] = sticky.Length == 1 && sticky[0].TouchDeliveryAbility != null ? sticky[0].TouchDeliveryAbility.AssetGuid : null,
                    ["danceHidden"] = dance.Hidden, ["danceResourceCost"] = dance.ComponentsArray.OfType<AbilityResourceLogic>().Count(),
                    ["pixieGranted"] = new JArray(pixieGrant.Select(value => value.name)),
                    ["pixieBrainAbilities"] = new JArray(pixieBrainAbilities.Where(value => value != null).Select(value => value.name)),
                    ["spellLikeResourceCosts"] = spellLikes.Sum(value => value.ComponentsArray.OfType<AbilityResourceLogic>().Count()),
                    ["mastodonTrampleGrants"] = mastodonUnit.AddFacts.Count(value => ReferenceEquals(value, trample)),
                    ["griffonExplicitCharge"] = griffonUnit.AddFacts.Count(value => value != null && value.AssetGuid == SummonPlaytestR5Policy.NativeChargeAbilityGuid) };
                bool configOk = R5CotwSpells.MistActive && R5CotwSpells.DanceActive &&
                    (int)config["cloudMistGrants"] == 1 && (int)config["cloudFogGrants"] == 0 && (int)config["cloudBrainMist"] == 1 &&
                    (int)config["cloudBrainFog"] == 0 && ReferenceEquals(mistAi.Ability, mist) && !mist.Hidden && mist.Type == AbilityType.SpellLike &&
                    (string)config["mistArea"] == SummonPlaytestR5Policy.CotwObscuringMistAreaGuid &&
                    (string)config["danceTouch"] == SummonPlaytestR5Policy.CotwIrresistibleDanceGuid && !dance.Hidden &&
                    (int)config["danceResourceCost"] == 1 && pixieGrant.Contains(dance) && spellLikes.All(value => pixieGrant.Contains(value)) &&
                    pixieBrainAbilities.Contains(dance) && spellLikes.All(value => pixieBrainAbilities.Contains(value)) &&
                    (int)config["spellLikeResourceCosts"] == 0 && (int)config["mastodonTrampleGrants"] == 1 && (int)config["griffonExplicitCharge"] == 0;
                record["config"] = config;
                Sprint25Check(prefix + "loaded-configuration", configOk, (JObject)config.DeepClone(),
                    "Loaded configuration: Call of the Wild bound both spells; the Cloud Giant grants and AI-references only the provider-backed Obscuring Mist (no Fog Cloud); the Pixie grants and AI-references Dance plus four resource-free spell-likes; the Mastodon grants its trample; the Griffon makes no explicit Charge grant.");

                recorder = new R4MephitRecorder(); EventBus.Subscribe(recorder); subscribed = true;
                saves = new Sprint26Saves(hostile) { DesiredRoll = 2 };
                screens = new NativeIconScreenEvidence(_request);

                // 2. Griffon: one native Charge on the live unit.
                UnitEntityData griffon = CastExpandedSummoningOwnTier(fixture, "griffon"); owned.Add(griffon);
                Sprint28SetBrainActivePreservingActions(griffon, false);
                int charges = griffon.Descriptor.Abilities.Enumerable.Count(value => value.Blueprint.AssetGuid == SummonPlaytestR5Policy.NativeChargeAbilityGuid);
                Sprint25Check(prefix + "griffon-one-charge", charges == 1, new JObject { ["griffon"] = griffon.UniqueId, ["chargeAbilities"] = charges,
                    ["pounce"] = griffon.Descriptor.HasFact(BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library, "1a8149c09e0bdfc48a305ee6ac3729a8", "native Pounce")) },
                    "A fresh live Griffon has exactly one native Charge ability (the engine grant) and keeps Pounce.");

                // 3. Mastodon: a real commanded trample (existing qualified driver).
                foreach (int frame in Sprint23NativeTrampleCommand(fixture, false, "mastodon", 29, 2, DiceType.D8, 18,
                    (name, passed, observed, expected) => Sprint25Check(prefix + "mastodon-" + name, passed, observed, expected)))
                    yield return frame;
                PlaceExpandedSummoningUnit(hostile, ObstacleAnalyzer.GetNearestNode(centre + Vector3.forward * 12f).clampedPosition);
                for (int frame = 0; frame < 10; frame++) yield return 0;

                // 4. Cloud Giant: provider Obscuring Mist at a target point, twice.
                UnitEntityData giant = CastExpandedSummoningOwnTier(fixture, "cloud-giant"); owned.Add(giant);
                Sprint28SetBrainActivePreservingActions(giant, false);
                giant.Commands.InterruptAll(command => true); giant.Commands.RemoveFinishedAndUpdateQueue();
                PlaceExpandedSummoningUnit(giant, ObstacleAnalyzer.GetNearestNode(centre + Vector3.left * 6f).clampedPosition);
                for (int frame = 0; frame < 10; frame++) yield return 0;
                var liveMist = giant.Descriptor.Abilities.Enumerable.Where(value => ReferenceEquals(value.Blueprint, mist)).ToArray();
                bool liveRoutes = liveMist.Length == 1 && !giant.Descriptor.Abilities.Enumerable.Any(value => ReferenceEquals(value.Blueprint, fog)) &&
                    giant.Brain.Actions.All(action => !ReferenceEquals(action.Blueprint, fogAi)) &&
                    giant.Brain.Actions.Count(action => ReferenceEquals(action.Blueprint, mistAi)) == 1;
                Sprint25Check(prefix + "cloud-custom-routes-removed", liveRoutes, new JObject { ["mistFacts"] = liveMist.Length,
                    ["brain"] = new JArray(giant.Brain.Actions.Select(action => action.Blueprint == null ? null : action.Blueprint.name)) },
                    "A fresh live Cloud Giant has one Obscuring Mist spell-like and neither custom fog grant nor its AI route.");
                var mistRows = new JArray(); Vector3[] points = {
                    ObstacleAnalyzer.GetNearestNode(hostile.Position + Vector3.right * 3f).clampedPosition,
                    ObstacleAnalyzer.GetNearestNode(hostile.Position + Vector3.left * 4f).clampedPosition };
                for (int cast = 0; cast < 2; cast++)
                {
                    var row = new JObject(); int appliedBefore = recorder.Applied.Count(context => ReferenceEquals(context.AbilityBlueprint, mist));
                    foreach (int frame in R5RunData(giant, new AbilityData(liveMist[0]), new TargetWrapper(points[cast]), row)) yield return frame;
                    for (int frame = 0; frame < 15; frame++) yield return 0;
                    var areas = R5Areas(SummonPlaytestR5Policy.CotwObscuringMistAreaGuid, giant);
                    row["applied"] = recorder.Applied.Count(context => ReferenceEquals(context.AbilityBlueprint, mist)) - appliedBefore;
                    row["areas"] = areas.Length;
                    row["nearestAreaToPoint"] = areas.Length == 0 || areas.All(area => area.View == null) ? -1f :
                        areas.Where(area => area.View != null).Min(area => Vector3.Distance(area.View.transform.position, points[cast]));
                    row["availableAfter"] = new AbilityData(liveMist[0]).IsAvailable;
                    mistRows.Add(row);
                    if (cast == 0)
                        foreach (int frame in screens.Capture("r5-cloud-mist-point", new JObject { ["caster"] = giant.UniqueId, ["areas"] = areas.Length }, worldOwned))
                            yield return frame;
                }
                record["mist"] = mistRows;
                bool mistOk = mistRows.OfType<JObject>().All(row => (bool)row["finished"] && (int)row["applied"] == 1 &&
                    (float)row["nearestAreaToPoint"] >= 0f && (float)row["nearestAreaToPoint"] < 1.5f && (bool)row["availableAfter"]) &&
                    (int)mistRows[1]["areas"] == 2;
                Sprint25Check(prefix + "cloud-mist-target-point-at-will", mistOk, new JObject { ["casts"] = mistRows.DeepClone() },
                    "Two commanded casts of the Cloud Giant's Obscuring Mist each create Call of the Wild's stationary mist area at the chosen point (not on the caster), with the ability still available after each (at will, no resource).");
                // The mist is ended before the Pixie cells so its concealment
                // cannot shape their targeting.
                foreach (var area in R5Areas(SummonPlaytestR5Policy.CotwObscuringMistAreaGuid, giant)) area.ForceEnd();
                for (int frame = 0; frame < 10; frame++) yield return 0;

                // 5. Pixie: four spell-likes, each twice; then the Dance once.
                UnitEntityData pixie = CastExpandedSummoningOwnTier(fixture, "pixie"); owned.Add(pixie);
                Sprint28SetBrainActivePreservingActions(pixie, false);
                pixie.Commands.InterruptAll(command => true); pixie.Commands.RemoveFinishedAndUpdateQueue();
                PlaceExpandedSummoningUnit(pixie, ObstacleAnalyzer.GetNearestNode(centre + Vector3.right * 4f).clampedPosition);
                for (int frame = 0; frame < 10; frame++) yield return 0;
                var spellRows = new JObject(); bool spellsOk = true;
                foreach (PixieSpellLikeSpec spec in SummonPlaytestR5Policy.PixieAtWill)
                {
                    BlueprintAbility ability = abilityOf(spec.Symbol);
                    BlueprintAbility native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, spec.NativeGuid, "native " + spec.DisplayName);
                    Ability fact = pixie.Descriptor.Abilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, ability));
                    var casts = new JArray();
                    for (int cast = 0; cast < 2; cast++)
                    {
                        var row = new JObject();
                        AbilityData data = new AbilityData(fact);
                        TargetWrapper target = new TargetWrapper(hostile);
                        BlueprintAbility executed = ability;
                        if (spec.Token == "DispelMagic")
                        {
                            executed = abilityOf(SummonPlaytestR5Policy.PixiePrefix + "DispelMagicTarget");
                            data = new AbilityData(data, executed);
                        }
                        else if (spec.Token == "Shield") target = new TargetWrapper(pixie);
                        else if (!ability.CanTargetEnemies && ability.CanTargetPoint) target = new TargetWrapper(hostile.Position);
                        row["target"] = target.Unit != null ? (target.Unit == pixie ? "self" : "hostile") : "point";
                        int appliedBefore = recorder.Applied.Count(context => ReferenceEquals(context.AbilityBlueprint, executed));
                        foreach (int frame in R5RunData(pixie, data, target, row)) yield return frame;
                        for (int frame = 0; frame < 10; frame++) yield return 0;
                        row["applied"] = recorder.Applied.Count(context => ReferenceEquals(context.AbilityBlueprint, executed)) - appliedBefore;
                        row["availableAfter"] = new AbilityData(fact).IsAvailable;
                        casts.Add(row);
                    }
                    bool nativeCost = ability.ActionType == native.ActionType && !ability.ComponentsArray.OfType<AbilityResourceLogic>().Any();
                    bool effect = spec.Token == "Shield" ? pixie.Buffs.Enumerable.Any(buff => buff.Context != null && ReferenceEquals(buff.Context.SourceAbility, ability)) :
                        spec.Token == "Confusion" ? hostile.Buffs.Enumerable.Any(buff => buff.Context != null && ReferenceEquals(buff.Context.SourceAbility, ability)) : true;
                    bool ok = nativeCost && effect && casts.OfType<JObject>().All(row => (bool)row["finished"] && (int)row["applied"] >= 1 && (bool)row["availableAfter"]);
                    spellsOk &= ok;
                    spellRows[spec.Token] = new JObject { ["casts"] = casts, ["nativeActionType"] = native.ActionType.ToString(),
                        ["ownedActionType"] = ability.ActionType.ToString(), ["resourceFree"] = !ability.ComponentsArray.OfType<AbilityResourceLogic>().Any(),
                        ["effectObserved"] = effect, ["passed"] = ok };
                }
                record["pixieSpellLikes"] = spellRows;
                Sprint25Check(prefix + "pixie-four-at-will", spellsOk, (JObject)spellRows.DeepClone(),
                    "Each of the Pixie's Entangle, Confusion, Dispel Magic (targeted variant) and Shield executes twice under command with its native action type, no resource, and remains available (Shield's buff on the Pixie and Confusion's on the hostile observed; controlled failed saves).");

                PlaceExpandedSummoningUnit(pixie, ObstacleAnalyzer.GetNearestNode(hostile.Position + Vector3.back * 1.2f).clampedPosition);
                for (int frame = 0; frame < 10; frame++) yield return 0;
                Ability danceFact = pixie.Descriptor.Abilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, dance));
                var danceResource = dance.ComponentsArray.OfType<AbilityResourceLogic>().Single().RequiredResource;
                var danceRow = new JObject { ["resourceBefore"] = pixie.Descriptor.Resources.GetResourceAmount(danceResource) };
                attackSeed = new R5AttackSeed(pixie);
                foreach (int frame in R5RunData(pixie, new AbilityData(danceFact), new TargetWrapper(hostile), danceRow)) yield return frame;
                int waited = 0;
                while (waited++ < 600 && !hostile.Buffs.Enumerable.Any(buff => buff.Blueprint.AssetGuid == R5DanceBuffGuid)) yield return 0;
                danceRow["touchAttacks"] = attackSeed.Rows.DeepClone();
                attackSeed.Dispose(); attackSeed = null;
                var danceBuff = hostile.Buffs.Enumerable.FirstOrDefault(buff => buff.Blueprint.AssetGuid == R5DanceBuffGuid);
                danceRow["waitFrames"] = waited;
                danceRow["danceBuffOnHostile"] = danceBuff != null;
                danceRow["buffCaster"] = danceBuff == null || danceBuff.Context.MaybeCaster == null ? null : danceBuff.Context.MaybeCaster.UniqueId;
                danceRow["resourceAfter"] = pixie.Descriptor.Resources.GetResourceAmount(danceResource);
                danceRow["availableAfter"] = new AbilityData(danceFact).IsAvailable;
                danceRow["hostileSaves"] = saves.Rows.DeepClone();
                bool danceOk = danceBuff != null && ReferenceEquals(danceBuff.Context.MaybeCaster, pixie) &&
                    (int)danceRow["resourceBefore"] == 1 && (int)danceRow["resourceAfter"] == 0 && !(bool)danceRow["availableAfter"];
                record["dance"] = danceRow;
                Sprint25Check(prefix + "pixie-dance-provider", danceOk, (JObject)danceRow.DeepClone(),
                    "The Pixie's Dance delivers Call of the Wild's actual Irresistible Dance (controlled natural-20 touch delivery and failed save; its buff on the touched hostile, cast by the Pixie), spending the one use per summoning; it is then unavailable.");

                // 6. Native description windows for the new and replaced abilities.
                UnitEntityData mastodon = CastExpandedSummoningOwnTier(fixture, "mastodon"); owned.Add(mastodon);
                Sprint28SetBrainActivePreservingActions(mastodon, false);
                for (int frame = 0; frame < 10; frame++) yield return 0;
                var windows = new[] {
                    new { Label = "cloud-obscuring-mist", Unit = giant, Ability = mist },
                    new { Label = "pixie-irresistible-dance", Unit = pixie, Ability = dance },
                    new { Label = "pixie-entangle", Unit = pixie, Ability = abilityOf(SummonPlaytestR5Policy.PixiePrefix + "Entangle") },
                    new { Label = "pixie-confusion", Unit = pixie, Ability = abilityOf(SummonPlaytestR5Policy.PixiePrefix + "Confusion") },
                    new { Label = "pixie-dispel-magic", Unit = pixie, Ability = abilityOf(SummonPlaytestR5Policy.PixiePrefix + "DispelMagic") },
                    new { Label = "pixie-shield", Unit = pixie, Ability = abilityOf(SummonPlaytestR5Policy.PixiePrefix + "Shield") },
                    new { Label = "mastodon-trample", Unit = mastodon, Ability = trample } };
                var descriptions = new JArray();
                foreach (var item in windows)
                {
                    var target = new R3DescriptionTarget { Label = item.Label, Creature = item.Unit.Blueprint.name, Blueprint = item.Ability,
                        Duration = R3Text(item.Ability.LocalizedDuration), SavingThrow = R3Text(item.Ability.LocalizedSavingThrow) };
                    Ability fact = item.Unit.Descriptor.Abilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, item.Ability));
                    var window = ui.DescriptionController.DescWindow;
                    if (R3UnrelatedModal()) throw new InvalidOperationException("Unrelated native modal shown before opening a description; UI interaction stopped.");
                    if (window.gameObject.activeInHierarchy || ui.TooltipsController.ContextTooltipData != null)
                        throw new InvalidOperationException("A foreign description window or hover is open.");
                    var data = new TooltipData { Type = TooltipType.Ability, Ability = fact };
                    ui.DescriptionController.HandleOpenDescription(data); openDescription = data;
                    for (int frame = 0; frame < 600 && !R3DescriptionOwned(mode, fact, data); frame++)
                    {
                        if (R3UnrelatedModal()) throw new InvalidOperationException("Unrelated native modal shown while opening a description.");
                        yield return 0;
                    }
                    if (!R3DescriptionOwned(mode, fact, data)) throw new InvalidOperationException("Exact owned description window did not become visible: " + item.Label);
                    Func<bool> owns = () => R3DescriptionOwned(mode, fact, data);
                    for (int frame = 0; frame < 20; frame++) { yield return 0; if (!owns()) throw new InvalidOperationException("Description ownership lost: " + item.Label); }
                    JObject card = R3DescriptionCard(target);
                    foreach (int frame in screens.Capture("r5-description-" + item.Label, card, owns)) yield return frame;
                    card = R3DescriptionCard(target);
                    bool passed = (bool)card["contentCorrect"] && (bool)card["rowsVisible"];
                    descriptions.Add(new JObject { ["construction"] = R3Construction(data, fact), ["card"] = card, ["passed"] = passed });
                    Sprint25Check(prefix + "render-" + item.Label, passed, (JObject)card.DeepClone(),
                        "Captured native description window for the live fact shows its name, description, visible icon and only its own rows, unclipped, with no <null>.");
                    ui.DescriptionController.HandleCloseDescriptionWindow(window);
                    for (int frame = 0; frame < 120 && window.gameObject.activeInHierarchy; frame++) yield return 0;
                    if (window.gameObject.activeInHierarchy) throw new InvalidOperationException("Native description window did not close.");
                    openDescription = null;
                }
                record["descriptions"] = descriptions;
            }
            finally
            {
                var errors = new JArray();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                if (openDescription != null)
                    restore("description", () => {
                        var window = ui.DescriptionController.DescWindow;
                        if (ReferenceEquals(window.Data, openDescription)) ui.DescriptionController.HandleCloseDescriptionWindow(window);
                    });
                restore("saves", () => { if (saves != null) saves.Dispose(); });
                restore("attack", () => { if (attackSeed != null) attackSeed.Dispose(); });
                restore("recorder", () => { if (subscribed) EventBus.Unsubscribe(recorder); });
                restore("screens", () => { if (screens != null) screens.Dispose(); });
                restore("areas", () => {
                    foreach (var area in Game.Instance.State.AreaEffects.All.Where(area => area != null && area.Context != null &&
                        owned.Contains(area.Context.MaybeCaster)).ToArray()) area.ForceEnd();
                });
                restore("combat", () => {
                    foreach (UnitEntityData unit in owned.Concat(new[] { fixture.Hostile, fixture.Caster }).Where(value => value != null && !value.Destroyed))
                    {
                        unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                        if (unit.CombatState.IsInCombat) unit.CombatState.LeaveCombat();
                    }
                });
                restore("hostile", () => { if (!fixture.Hostile.Destroyed) PlaceExpandedSummoningUnit(fixture.Hostile, hostilePosition); });
                PlaceExpandedSummoningUnit(fixture.Caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                Sprint25Check(prefix + "owned-ui-restored", errors.Count == 0 && !ui.DescriptionController.DescWindow.gameObject.activeInHierarchy,
                    new JObject { ["errors"] = errors }, "Owned windows, readers, areas, combat and positions restore.");
                record["restoreErrors"] = errors;
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"), record.ToString(Formatting.Indented));
            }
        }
    }
}
