using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UI.Tooltip;
using Kingmaker.UI.TurnBasedMode;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Repair R3 (UI-01..06), completion observer. One save-free phase in
        // the established non-DLC Oleg outdoor fixture under an owned native
        // Default scope (the mansion prologue storybook covered every screen
        // of the first R3 run; it is never dismissed or treated as idle).
        //  1. Whole registered roster, live library: own portrait art; donors
        //     untouched (cheap catalog-level audit).
        //  2. Seven representative summons cast through their ordinary own-tier
        //     children: live portrait source, live facts, live brain, and the
        //     affected actions' icons and tooltip semantics.
        //  3. Native turn-based combat (the existing performance-combat seam):
        //     the rendered initiative tracker, then the native description
        //     window for each affected live Ability fact. Every capture is held
        //     by real ownership/visibility checks; obscured or foreign UI fails.
        // Brains are inactive; no summon acts; no input; no save.
        private static readonly string[] SummonPlaytestR3Keys =
        { "dire-rat", "lantern-archon", "ghaele-azata", "nalfeshnee", "brachiosaurus", "cheetah", "trumpet-archon" };
        private const string R3NativeOverrunDescriptionPrefix = "You can attempt to run over your target";

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
                ["iconTexture"] = blueprint.Icon == null || blueprint.Icon.texture == null ? null : blueprint.Icon.texture.name,
                ["description"] = blueprint.Description ?? "",
                ["duration"] = ability == null ? null : R3Text(ability.LocalizedDuration),
                ["savingThrow"] = ability == null ? null : R3Text(ability.LocalizedSavingThrow),
                ["durationObject"] = ability != null && ability.LocalizedDuration != null,
                ["savingThrowObject"] = ability != null && ability.LocalizedSavingThrow != null };
        }

        private sealed class R3DescriptionTarget
        {
            internal string Label, Creature;
            internal BlueprintAbility Blueprint;
            internal string Duration, SavingThrow;
        }

        // The rendered portrait is InitiativeTrackerPortraitZone.m_Picture:
        // the base view's SetupPortrait calls the character zone's SetUnit,
        // which assigns unit.Portrait.SmallPortrait. The order view's own
        // m_Portrait is an unused pooled image (the first completion run read
        // it and saw a stale native sprite; that observer defect is retained).
        private static readonly FieldInfo R3TrackerPortraitField =
            typeof(Kingmaker.UI._ConsoleUI.TurnBasedMode.InitiativeTrackerPortraitZone).GetField("m_Picture", Cleanup2Fields);

        private static Image[] R3ZonePictures(Component view)
        {
            return view.GetComponentsInChildren<Kingmaker.UI._ConsoleUI.TurnBasedMode.InitiativeTrackerPortraitZone>(true)
                .Where(zone => zone.isActiveAndEnabled && zone.gameObject.activeInHierarchy)
                .Select(zone => R3TrackerPortraitField == null ? null : R3TrackerPortraitField.GetValue(zone) as Image)
                .Where(image => image != null).ToArray();
        }

        private static UnitEntityData R3TrackerUnit(Component view)
        {
            for (Type type = view.GetType(); type != null; type = type.BaseType)
            {
                PropertyInfo property = type.GetProperty("ViewModel", Cleanup2Fields | BindingFlags.DeclaredOnly);
                if (property == null) continue;
                object model = property.GetValue(view, null);
                FieldInfo unit = model == null ? null : model.GetType().GetField("Unit", Cleanup2Fields);
                return unit == null ? null : unit.GetValue(model) as UnitEntityData;
            }
            return null;
        }

        // Rendered initiative tracker: each representative must appear with
        // its own small portrait in a visible native order view (or as the
        // current unit's visible portrait).
        private JObject R3TrackerState(IDictionary<string, UnitEntityData> units)
        {
            var orders = UnityEngine.Object.FindObjectsOfType<InitiativeTrackerUnitOrderHorizontalPCView>()
                .Where(view => view != null && view.isActiveAndEnabled).ToArray();
            var currents = UnityEngine.Object.FindObjectsOfType<InitiativeTrackerCurrentUnitPCView>()
                .Where(view => view != null && view.isActiveAndEnabled).ToArray();
            var current = Game.Instance.TurnBasedCombatController.CurrentTurn;
            var rows = new JObject(); bool all = R3TrackerPortraitField != null;
            foreach (var pair in units)
            {
                Sprite expected = pair.Value.Portrait == null ? null : pair.Value.Portrait.SmallPortrait;
                var bound = orders.Where(view => ReferenceEquals(R3TrackerUnit(view), pair.Value)).ToArray();
                var images = bound.SelectMany(R3ZonePictures).ToArray();
                bool order = images.Any(image => ReferenceEquals(image.sprite, expected) && Cleanup2TooltipGraphicVisible(image));
                bool isCurrent = current != null && ReferenceEquals(current.Unit, pair.Value);
                bool currentView = isCurrent && currents.SelectMany(R3ZonePictures)
                    .Any(image => ReferenceEquals(image.sprite, expected) && Cleanup2TooltipGraphicVisible(image));
                bool shown = expected != null && expected.name == SummonPortraitPolicy.PortraitName(pair.Key) && (order || currentView);
                all &= shown;
                rows[pair.Key] = new JObject { ["unit"] = pair.Value.UniqueId, ["expected"] = expected == null ? null : expected.name,
                    ["orderViews"] = bound.Length, ["orderSprites"] = new JArray(images.Select(image => image.sprite == null ? null : image.sprite.name)),
                    ["orderVisible"] = order, ["isCurrent"] = isCurrent, ["currentViewVisible"] = currentView, ["shown"] = shown };
            }
            return new JObject { ["passed"] = all, ["orderViews"] = orders.Length, ["currentViews"] = currents.Length,
                ["round"] = Game.Instance.TurnBasedCombatController.RoundNumber,
                ["currentTurn"] = current == null ? null : current.Unit.UniqueId, ["units"] = rows };
        }

        // Rendered native description window for one live Ability fact.
        private JObject R3DescriptionCard(R3DescriptionTarget target, Ability fact, TooltipData data)
        {
            var window = Game.Instance.UI.DescriptionController.DescWindow;
            var texts = window.GetComponentsInChildren<TextMeshProUGUI>(true)
                .Where(value => value.isActiveAndEnabled).Select(value => value.GetParsedText() ?? "").ToArray();
            var images = window.GetComponentsInChildren<Image>(true).Where(value => value.isActiveAndEnabled).ToArray();
            string durationTitle = UIUtility.GetGlossaryEntryName(TooltipElement.Duration.ToString());
            string saveTitle = UIUtility.GetGlossaryEntryName(TooltipElement.SavingThrow.ToString());
            Func<string, bool> has = value => texts.Any(text => string.Equals(text.Trim(), value, StringComparison.OrdinalIgnoreCase));
            string description = target.Blueprint.Description ?? "";
            string chunk = description.Length > 24 ? description.Substring(0, 24) : description;
            bool owned = window.gameObject.activeInHierarchy && ReferenceEquals(window.Data, data) &&
                ReferenceEquals(data.Ability, fact) && fact.Owner != null && !fact.Owner.Unit.Destroyed &&
                fact.Owner.Abilities.Enumerable.Contains(fact);
            bool title = has(target.Blueprint.Name);
            bool body = chunk.Length > 0 && texts.Any(text => text.IndexOf(chunk, StringComparison.OrdinalIgnoreCase) >= 0);
            bool icon = target.Blueprint.Icon != null && images.Any(image => ReferenceEquals(image.sprite, target.Blueprint.Icon) &&
                Cleanup2TooltipGraphicVisible(image));
            bool durationRow = string.IsNullOrEmpty(target.Duration) ? !has(durationTitle) : has(durationTitle) && has(target.Duration);
            bool saveRow = string.IsNullOrEmpty(target.SavingThrow) ? !has(saveTitle) : has(saveTitle) && has(target.SavingThrow);
            bool clean = !texts.Any(text => text.Contains("<null>") || string.Equals(text.Trim(), "Trample", StringComparison.Ordinal) && target.Blueprint.Name != "Trample" ||
                text.IndexOf(R3NativeOverrunDescriptionPrefix, StringComparison.OrdinalIgnoreCase) >= 0);
            return new JObject { ["label"] = target.Label, ["creature"] = target.Creature, ["blueprint"] = target.Blueprint.AssetGuid,
                ["name"] = target.Blueprint.Name, ["icon"] = target.Blueprint.Icon == null ? null : target.Blueprint.Icon.name,
                ["expectedDuration"] = target.Duration, ["expectedSavingThrow"] = target.SavingThrow,
                ["durationTitle"] = durationTitle, ["savingThrowTitle"] = saveTitle,
                ["owned"] = owned, ["title"] = title, ["description"] = body, ["iconVisible"] = icon,
                ["durationRow"] = durationRow, ["savingThrowRow"] = saveRow, ["noNullOrInheritedText"] = clean,
                ["passed"] = owned && title && body && icon && durationRow && saveRow && clean,
                ["texts"] = new JArray(texts.Where(text => text.Trim().Length > 0).Take(40)),
                ["images"] = new JArray(images.Where(image => image.sprite != null).Take(24).Select(image => image.sprite.name)),
                ["surface"] = "Native DescriptionController.HandleOpenDescription(TooltipData{Ability=live fact}) -> DescriptionTemplatesAbility; the same Duration/Saving Throw rows the hover tooltip builds (empty text omits a row)." };
        }

        private IEnumerable<int> ReviewSummonPlaytestUiR3(ExpandedSummoningCorrectionFixture fixture, string phase,
            Cleanup2NativeDefaultScope mode)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || fixture.Hostile == null || mode == null || !mode.Current ||
                !_context.FeatureModules.Active.ExpandedSummoning ||
                phase != ExpandedSummoningNativeProfilePolicy.Sprint39PlaytestUiPhase ||
                !ExpandedSummoningNativeProfilePolicy.Sprint32NativeDefaultOutdoor(_request.Scenario, phase) ||
                !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Exact fresh R3 UI fixture in owned outdoor native Default, without narrative, required.");
            const string prefix = "r3-ui-";
            var game = Game.Instance; var ui = game.UI;
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            IDictionary<string, BlueprintScriptableObject> symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId,
                ["fixture"] = "established non-DLC Oleg outdoor native Default; save-free",
                ["limits"] = "Portrait/action checks read live blueprints and units. Screens are the rendered initiative tracker and native description window for live Ability facts; the hover template itself is not opened (same DescriptionTemplatesAbility rows). Owner visual approval is separate. No summon acts." };
            var owned = new List<UnitEntityData>();
            Vector3 centre = fixture.Caster.Position;
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = game.IsPaused;
            bool oldEngagement = SettingsRoot.Instance.PauseOnEngagement.CurrentValue;
            var tutorial = Kingmaker.Blueprints.Root.BlueprintRoot.Instance.UITutorials.TBMStarted.Lock;
            bool tutorialLocked = tutorial != null && tutorial.IsLocked, tutorialTouched = false, combatOwned = false;
            var flagsBefore = game.Player.UnlockableFlags.UnlockedFlags.ToArray();
            UnitEntityData anchor = game.Player.MainCharacter.Value;
            Vector3 hostilePosition = fixture.Hostile.Position;
            NativeIconScreenEvidence screens = null;
            TooltipData openDescription = null;
            var captures = new JArray();
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
                    string donorGuid = ExpandedSummoningDonorCatalog.For(creature.Key).Guid;
                    audit.Add(new JObject { ["key"] = creature.Key, ["published"] = SummonIconCatalog.IsPublishedSomewhere(creature),
                        ["unitGuid"] = unit == null ? null : unit.AssetGuid, ["small"] = small == null ? null : small.name,
                        ["donor"] = donorGuid, ["exact"] = exact });
                    if (donorsSeen.Add(donorGuid))
                    {
                        var donor = library.BlueprintsByAssetId.ContainsKey(donorGuid) ?
                            library.BlueprintsByAssetId[donorGuid] as BlueprintUnit : null;
                        BlueprintPortrait donorPortrait = donor == null ? null : donor.PortraitSafe;
                        Sprite donorSmall = donorPortrait == null ? null : donorPortrait.Data.SmallPortrait;
                        bool touched = donorPortrait != null && (donorPortrait.name.StartsWith(SummonPortraitPolicy.PortraitNamePrefix, StringComparison.Ordinal) ||
                            donorSmall != null && donorSmall.name.StartsWith(SummonPortraitPolicy.PortraitNamePrefix, StringComparison.Ordinal));
                        if (touched) donorTouched++;
                        donorRows.Add(new JObject { ["donor"] = donorGuid, ["portrait"] = donorPortrait == null ? null : donorPortrait.name,
                            ["small"] = donorSmall == null ? null : donorSmall.name, ["touched"] = touched });
                    }
                }
                record["portraitAudit"] = audit; record["donorPortraits"] = donorRows;
                Sprint25Check(prefix + "portrait-roster", wrong == 0 && audit.Count == ExpandedSummoningIdentityCatalog.UnitCount,
                    new JObject { ["units"] = audit.Count, ["wrong"] = wrong },
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
                    Vector3 offset = Quaternion.Euler(0f, index * (360f / SummonPlaytestR3Keys.Length), 0f) * Vector3.forward * 5f;
                    PlaceExpandedSummoningUnit(summon, ObstacleAnalyzer.GetNearestNode(centre + offset).clampedPosition);
                }
                for (int frame = 0; frame < 10; frame++) { if (!mode.Current) throw new InvalidOperationException("Lost native Default."); yield return 0; }
                var live = new JObject(); bool portraitsLive = true;
                foreach (string key in SummonPlaytestR3Keys)
                {
                    UnitEntityData unit = units[key];
                    Sprite small = unit.Portrait == null ? null : unit.Portrait.SmallPortrait;
                    bool exact = small != null && small.name == SummonPortraitPolicy.PortraitName(key) &&
                        unit.UISettings.PortraitBlueprintRaw == null && unit.UISettings.CustomPortraitRaw == null;
                    portraitsLive &= exact;
                    var actions = new JArray();
                    foreach (var ability in unit.Descriptor.Abilities.Enumerable) actions.Add(R3Action(ability.Blueprint));
                    foreach (var toggle in unit.Descriptor.ActivatableAbilities.Enumerable) actions.Add(R3Action(toggle.Blueprint));
                    live[key] = new JObject { ["unit"] = unit.UniqueId, ["blueprint"] = unit.Blueprint.name,
                        ["initiativePortrait"] = small == null ? null : small.name, ["portraitExact"] = exact, ["actions"] = actions };
                }
                record["representatives"] = live;
                Sprint25Check(prefix + "initiative-portraits-live", portraitsLive,
                    new JObject { ["units"] = new JArray(SummonPlaytestR3Keys.Select(key => (string)live[key]["initiativePortrait"])) },
                    "Each representative summon's unit.Portrait.SmallPortrait (the initiative tracker's source) is its own art, with no unit-level portrait written.");

                Func<string, BlueprintAbility> abilityOf = symbol => (BlueprintAbility)symbols[symbol];
                Func<UnitEntityData, BlueprintAbility, int> count = (unit, blueprint) =>
                    unit.Descriptor.Abilities.Enumerable.Count(value => ReferenceEquals(value.Blueprint, blueprint));

                BlueprintAbility light = abilityOf("KMG.Summoning.Special.LanternArchon.LightRay");
                BlueprintAbility twin = abilityOf("KMG.Summoning.Special.LanternArchon.LightRayFull");
                Sprint25Check(prefix + "lantern-rays", light.Icon != null && twin.Icon != null &&
                    !ExpandedSummoningSpecialBuilder.SameIconArt(light.Icon, twin.Icon) &&
                    count(units["lantern-archon"], light) == 1 && count(units["lantern-archon"], twin) == 1,
                    new JObject { ["lightRay"] = R3Action(light), ["twinLightRay"] = R3Action(twin) },
                    "Lantern Archon's two live ray actions carry distinct native light-spell art (sprite and texture region).");

                // UI-03: the proven grant path, the live brain, and the donor.
                var ghaele = units["ghaele-azata"];
                var nativeRay = (BlueprintAbility)library.BlueprintsByAssetId["33e8997912cf76b4c99dca0445082804"];
                var nativeAi = (BlueprintAiCastSpell)library.BlueprintsByAssetId["dcfc5e9aec5bea540b36caf754989164"];
                var donorUnit = (BlueprintUnit)library.BlueprintsByAssetId[ExpandedSummoningSpecialBuilder.GhaeleDonorGuid];
                BlueprintAbility clone = abilityOf(ExpandedSummoningSpecialBuilder.GhaeleRaySymbol);
                var ownedAi = (BlueprintAiCastSpell)symbols[ExpandedSummoningSpecialBuilder.GhaeleRayAiSymbol];
                var ownedBrain = (BlueprintBrain)symbols[ExpandedSummoningSpecialBuilder.GhaeleBrainSymbol];
                var liveBrainAbilities = ghaele.Brain == null ? new BlueprintAbility[0] : ghaele.Brain.Actions
                    .Select(action => action.Blueprint as BlueprintAiCastSpell).Where(value => value != null)
                    .Select(value => value.Ability).ToArray();
                var blueprintGrant = ghaele.Blueprint.ComponentsArray.OfType<AddAbilityToCharacterComponent>()
                    .SelectMany(component => component.Abilities ?? new BlueprintAbility[0]).ToArray();
                bool donorUnchanged = donorUnit.ComponentsArray.OfType<AddAbilityToCharacterComponent>()
                    .Count(component => component.Abilities != null && component.Abilities.Contains(nativeRay)) == 1 &&
                    donorUnit.Brain != null && donorUnit.Brain.Actions.Count(action => ReferenceEquals(action, nativeAi)) == 1 &&
                    ReferenceEquals(nativeAi.Ability, nativeRay) && nativeRay.Icon == null && string.IsNullOrEmpty(nativeRay.Name) &&
                    !donorUnit.Brain.Actions.Any(action => ReferenceEquals(action, ownedAi));
                JObject cloneRow = R3Action(clone);
                var ghaeleBlank = new JArray(((JArray)live["ghaele-azata"]["actions"]).Where(row =>
                    string.IsNullOrEmpty((string)row["name"]) || row["icon"].Type == JTokenType.Null));
                var ghaeleRow = new JObject { ["grant"] = ExpandedSummoningSpecialBuilder.GhaeleRayGrant,
                    ["clone"] = cloneRow, ["cloneFacts"] = count(ghaele, clone), ["nativeFacts"] = count(ghaele, nativeRay),
                    ["blueprintGrant"] = new JArray(blueprintGrant.Select(value => value.AssetGuid)),
                    ["blueprintBrain"] = ghaele.Blueprint.Brain == null ? null : ghaele.Blueprint.Brain.name,
                    ["liveBrainCastsClone"] = liveBrainAbilities.Count(value => ReferenceEquals(value, clone)),
                    ["liveBrainCastsNative"] = liveBrainAbilities.Count(value => ReferenceEquals(value, nativeRay)),
                    ["ownedAiAbility"] = ownedAi.Ability == null ? null : ownedAi.Ability.AssetGuid,
                    ["donorUnchanged"] = donorUnchanged, ["blankActions"] = ghaeleBlank };
                record["ghaele"] = ghaeleRow;
                Sprint25Check(prefix + "ghaele-ray-owned-grant",
                    count(ghaele, clone) == 1 && count(ghaele, nativeRay) == 0 &&
                    blueprintGrant.Count(value => ReferenceEquals(value, clone)) == 1 && !blueprintGrant.Contains(nativeRay) &&
                    ReferenceEquals(ghaele.Blueprint.Brain, ownedBrain) && ReferenceEquals(ownedAi.Ability, clone) &&
                    liveBrainAbilities.Count(value => ReferenceEquals(value, clone)) == 1 &&
                    !liveBrainAbilities.Contains(nativeRay) && donorUnchanged && ghaeleBlank.Count == 0 &&
                    clone.Name == "Light Ray" && clone.Icon != null && !string.IsNullOrEmpty(clone.Description) &&
                    clone.LocalizedDuration != null && clone.LocalizedSavingThrow != null &&
                    R3Text(clone.LocalizedDuration) == "" && R3Text(clone.LocalizedSavingThrow) == "",
                    (JObject)ghaeleRow.DeepClone(),
                    "The live KMG Ghaele holds exactly one owned, named, iconed, described light ray through its AddAbilityToCharacterComponent grant, no blank native ray, and its live brain casts the owned ray; the native donor component, ray, AI action and brain are unchanged.");

                // UI-05/06: Overrun versus the effective Trample.
                var brach = units["brachiosaurus"];
                BlueprintAbility overrun = abilityOf(GiantBodyRulesPolicy.BrachiosaurusOverrunSymbol);
                BlueprintAbility trample = abilityOf(GiantBodyRulesPolicy.BrachiosaurusTrampleSymbol);
                string overrunDuration = R3Text(overrun.LocalizedDuration), overrunSave = R3Text(overrun.LocalizedSavingThrow);
                string trampleDuration = R3Text(trample.LocalizedDuration), trampleSave = R3Text(trample.LocalizedSavingThrow);
                Func<string, bool> inherited = text => text == "<null>" || text == "Trample" ||
                    text.IndexOf(R3NativeOverrunDescriptionPrefix, StringComparison.OrdinalIgnoreCase) >= 0;
                var maneuver = new JObject { ["overrun"] = R3Action(overrun), ["trample"] = R3Action(trample),
                    ["overrunTexture"] = overrun.Icon == null || overrun.Icon.texture == null ? null : overrun.Icon.texture.name,
                    ["trampleTexture"] = trample.Icon == null || trample.Icon.texture == null ? null : trample.Icon.texture.name };
                record["maneuvers"] = maneuver;
                Sprint25Check(prefix + "overrun-trample-presentation",
                    count(brach, overrun) == 1 && count(brach, trample) == 1 && overrun.Icon != null && trample.Icon != null &&
                    !ExpandedSummoningSpecialBuilder.SameIconArt(overrun.Icon, trample.Icon) &&
                    overrun.LocalizedDuration != null && overrun.LocalizedSavingThrow != null && trample.LocalizedDuration != null &&
                    overrunDuration == SummonActionTooltipPolicy.InstantaneousDuration && overrunSave == SummonActionTooltipPolicy.OverrunSavingThrow &&
                    trampleDuration == SummonActionTooltipPolicy.InstantaneousDuration &&
                    trampleSave.StartsWith("Reflex DC ", StringComparison.Ordinal) && trampleSave.Contains("attack of opportunity") &&
                    !inherited(overrunDuration) && !inherited(overrunSave) && !inherited(trampleDuration) && !inherited(trampleSave),
                    (JObject)maneuver.DeepClone(),
                    "Overrun and the effective Trample show distinct art (sprite and texture region); neither inherits the native overrun's Duration 'Trample' or maneuver-description Saving Throw; Overrun's maneuver check is not a save; Trample states its attack-of-opportunity/Reflex alternative.");

                BlueprintAbility nimbus = abilityOf("KMG.Summoning.Demon.Nalfeshnee.UnholyNimbus");
                BlueprintAbility sprint = abilityOf("KMG.Summoning.Special.Cheetah.Sprint");
                BlueprintAbility blast = abilityOf("KMG.Summoning.OutsiderCelestial.TrumpetArchon.TrumpetBlast");
                Sprint25Check(prefix + "metadata-rows",
                    nimbus.Icon != null && count(units["nalfeshnee"], nimbus) == 1 &&
                    R3Text(nimbus.LocalizedDuration).StartsWith("Bursts after 1 round", StringComparison.Ordinal) &&
                    R3Text(nimbus.LocalizedSavingThrow).StartsWith("Will negates", StringComparison.Ordinal) &&
                    count(units["cheetah"], sprint) == 1 && R3Text(sprint.LocalizedDuration) == "1 round" && R3Text(sprint.LocalizedSavingThrow) == "None" &&
                    R3Text(blast.LocalizedDuration) == "1d4 rounds" && R3Text(blast.LocalizedSavingThrow) == "Fortitude negates",
                    new JObject { ["nimbus"] = R3Action(nimbus), ["sprint"] = R3Action(sprint), ["trumpetBlast"] = R3Action(blast) },
                    "Unholy Nimbus, Cheetah Sprint and sibling Trumpet Blast carry their implemented Duration/Saving Throw text.");

                // 3. Rendered native UI in owned turn-based combat.
                if (tutorial == null) throw new InvalidOperationException("Native turn tutorial flag absent.");
                tutorial.Unlock(); tutorialTouched = true;
                SettingsRoot.Instance.PauseOnEngagement.CurrentValue = false;
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = true;
                game.TurnBasedCombatController.Activate(); combatOwned = true;
                var hostile = fixture.Hostile;
                PlaceExpandedSummoningUnit(hostile, ObstacleAnalyzer.GetNearestNode(centre + Vector3.back * 9f).clampedPosition);
                var fighters = owned.Concat(new[] { fixture.Caster }).ToArray();
                hostile.Wake(0f); if (!game.State.AwakeUnits.Contains(hostile)) game.State.AwakeUnits.Add(hostile);
                foreach (var unit in fighters)
                {
                    unit.Wake(0f); if (!game.State.AwakeUnits.Contains(unit)) game.State.AwakeUnits.Add(unit);
                    unit.Memory.Add(hostile); hostile.Memory.Add(unit);
                }
                // The native join controller owns the party combat edge and the
                // turn-order initialization (see the performance-combat seam).
                foreach (var unit in fighters) unit.JoinCombat();
                hostile.JoinCombat();
                JObject tracker = null;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                for (int frame = 0; frame < 3000 && watch.Elapsed.TotalSeconds < 30; frame++)
                {
                    if (!mode.Current || !Cleanup2NoNarrativeOverlay()) throw new InvalidOperationException("Native Default or narrative-free state lost while entering combat.");
                    game.IsPaused = false;
                    tracker = R3TrackerState(units);
                    if ((bool)tracker["passed"] && game.TurnBasedCombatController.CurrentTurn != null) break;
                    yield return 0;
                }
                record["trackerBeforePause"] = tracker;
                if (tracker == null || !(bool)tracker["passed"])
                {
                    Sprint25Check(prefix + "tracker-rendered", false, tracker ?? new JObject(), "All seven representatives render their own portraits in the native initiative tracker.");
                    throw new InvalidOperationException("Native initiative tracker did not render every representative portrait.");
                }
                game.IsPaused = true;
                for (int frame = 0; frame < 30; frame++) yield return 0;
                Func<bool> trackerOwned = () => mode.Current && Cleanup2NoNarrativeOverlay() && game.IsPaused &&
                    !ui.DescriptionController.DescWindow.gameObject.activeInHierarchy && (bool)R3TrackerState(units)["passed"];
                tracker = R3TrackerState(units);
                Sprint25Check(prefix + "tracker-rendered", (bool)tracker["passed"] && trackerOwned(), (JObject)tracker.DeepClone(),
                    "All seven representatives render their own portraits in visible native initiative-tracker views (or the visible current-unit view).");
                screens = new NativeIconScreenEvidence(_request);
                foreach (int frame in screens.Capture("r3-initiative-tracker", tracker, trackerOwned)) yield return frame;
                captures.Add(tracker);

                var targets = new[] {
                    new R3DescriptionTarget { Label = "lantern-light-ray", Creature = "lantern-archon", Blueprint = light,
                        Duration = R3Text(light.LocalizedDuration), SavingThrow = R3Text(light.LocalizedSavingThrow) },
                    new R3DescriptionTarget { Label = "lantern-twin-rays", Creature = "lantern-archon", Blueprint = twin,
                        Duration = R3Text(twin.LocalizedDuration), SavingThrow = R3Text(twin.LocalizedSavingThrow) },
                    new R3DescriptionTarget { Label = "ghaele-light-ray", Creature = "ghaele-azata", Blueprint = clone, Duration = "", SavingThrow = "" },
                    new R3DescriptionTarget { Label = "nalfeshnee-nimbus", Creature = "nalfeshnee", Blueprint = nimbus,
                        Duration = R3Text(nimbus.LocalizedDuration), SavingThrow = R3Text(nimbus.LocalizedSavingThrow) },
                    new R3DescriptionTarget { Label = "brachiosaurus-overrun", Creature = "brachiosaurus", Blueprint = overrun, Duration = "", SavingThrow = "" },
                    new R3DescriptionTarget { Label = "brachiosaurus-trample", Creature = "brachiosaurus", Blueprint = trample, Duration = "", SavingThrow = trampleSave },
                    new R3DescriptionTarget { Label = "cheetah-sprint", Creature = "cheetah", Blueprint = sprint, Duration = "1 round", SavingThrow = "None" },
                    new R3DescriptionTarget { Label = "trumpet-blast", Creature = "trumpet-archon", Blueprint = blast, Duration = "1d4 rounds", SavingThrow = "Fortitude negates" } };
                foreach (var target in targets)
                {
                    UnitEntityData unit = units[target.Creature];
                    Ability fact = unit.Descriptor.Abilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, target.Blueprint));
                    var window = ui.DescriptionController.DescWindow;
                    if (window.gameObject.activeInHierarchy || ui.TooltipsController.ContextTooltipData != null)
                        throw new InvalidOperationException("A foreign description window or hover is open.");
                    var data = new TooltipData { Ability = fact };
                    ui.DescriptionController.HandleOpenDescription(data); openDescription = data;
                    JObject card = null;
                    var layout = System.Diagnostics.Stopwatch.StartNew();
                    for (int frame = 0; frame < 600 && layout.Elapsed.TotalSeconds < 8; frame++)
                    {
                        if (!mode.Current || !Cleanup2NoNarrativeOverlay()) throw new InvalidOperationException("Native Default lost during description layout.");
                        card = R3DescriptionCard(target, fact, data);
                        if ((bool)card["passed"]) break;
                        yield return 0;
                    }
                    Sprint25Check(prefix + "render-" + target.Label, card != null && (bool)card["passed"], card ?? new JObject(),
                        "The native description window for the live Ability fact renders its name, description, visible icon, and exactly the intended Duration/Saving Throw rows (omitted when empty), with no <null> or inherited native overrun text.");
                    if (card == null || !(bool)card["passed"]) throw new InvalidOperationException("Rendered description failed: " + target.Label);
                    foreach (int frame in screens.Capture("r3-description-" + target.Label, card, () => mode.Current &&
                        Cleanup2NoNarrativeOverlay() && game.IsPaused && (bool)R3DescriptionCard(target, fact, data)["passed"])) yield return frame;
                    captures.Add(card);
                    ui.DescriptionController.HandleCloseDescriptionWindow(window); openDescription = null;
                    for (int frame = 0; frame < 120 && window.gameObject.activeInHierarchy; frame++) yield return 0;
                    if (window.gameObject.activeInHierarchy) throw new InvalidOperationException("Native description window did not close.");
                }
                record["captures"] = captures;
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
                restore("screens", () => screens?.Dispose());
                restore("combat", () => {
                    foreach (UnitEntityData unit in owned.Concat(new[] { fixture.Hostile, fixture.Caster })
                        .Where(value => value != null && !value.Destroyed))
                    {
                        unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                        if (unit.IsInCombat) unit.LeaveCombat();
                    }
                });
                restore("turn-mode", () => {
                    SettingsRoot.Instance.PauseOnEngagement.CurrentValue = oldEngagement;
                    if (combatOwned && mode.Current)
                    { SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; game.TurnBasedCombatController.Activate(); }
                });
                restore("tutorial", () => { if (tutorialTouched && tutorialLocked) tutorial.Lock(); });
                bool flagsRestored = flagsBefore.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal)
                    .SequenceEqual(game.Player.UnlockableFlags.UnlockedFlags.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal));
                game.IsPaused = oldPause;
                restore("hostile", () => { if (!fixture.Hostile.Destroyed) PlaceExpandedSummoningUnit(fixture.Hostile, hostilePosition); });
                PlaceExpandedSummoningUnit(fixture.Caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                Sprint25Check(prefix + "owned-ui-restored", errors.Count == 0 && flagsRestored &&
                    !ui.DescriptionController.DescWindow.gameObject.activeInHierarchy,
                    new JObject { ["errors"] = errors, ["flagsRestored"] = flagsRestored, ["tutorialInitiallyLocked"] = tutorialLocked,
                        ["anchorUnchanged"] = anchor == null || ReferenceEquals(game.Player.MainCharacter.Value, anchor) },
                    "Owned description window, combat, turn mode, engagement pause, tutorial flag and pause restore exactly.");
                record["captures"] = captures; record["restoreErrors"] = errors;
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"),
                    record.ToString(Formatting.Indented));
            }
        }
    }
}
