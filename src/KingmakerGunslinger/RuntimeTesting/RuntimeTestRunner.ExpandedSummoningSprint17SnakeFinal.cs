using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
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
        private IEnumerable<int> ReviewSprint17SnakeFinalCases(ExpandedSummoningCorrectionFixture fixture)
        {
            // Publication is a distinct gate of this existing scenario, not
            // another hidden-observer retry or a new save/scenario family.
            if (SerpentineFinalReviewPolicy.Routes().Any(SummonVisibilityCatalog.IsPublished))
            {
                ReviewSprint17PublishedRoutes(fixture);
                yield return 0;
            }
            else foreach (int step in ReviewSprint17PrivateRoutes(fixture)) yield return step;
            CreateExpandedSummoningCorrectionHostile(fixture);
            foreach (int step in ReviewSprint17SnakeNativeUi(fixture)) yield return step;
            // The rule/UI hostile has a native hostile faction. It must not
            // participate in the independently isolated lifecycle drill.
            fixture.Hostile.Destroy();
            Game.Instance.EntityDestroyer.Tick();
            foreach (int step in ReviewSprint17SnakeViewLifecycle(fixture)) yield return step;
            foreach (int step in ReviewSprint17SalamanderLifecycle(fixture)) yield return step;
        }

        private void CheckSprint17Final(string name, bool pass, JObject row, string contract)
        {
            row = (JObject)row.DeepClone();
            row["check"] = name; row["passed"] = pass;
            _serpentineBodyRows.Add(row);
            _serpentineBodyAssertions.Add(Assertion("sprint17-final-" + name, contract,
                row.ToString(Formatting.None), pass,
                "Closed Sprint17 route/UI/lifecycle gate only. Public-route proof does not qualify Salamander mechanics or complete Sprint17/Phase2B."));
        }

        private void ReviewSprint17PublishedRoutes(ExpandedSummoningCorrectionFixture fixture)
        {
            object levelController = null;
            MethodInfo castRule = typeof(RuleCastSpell).GetMethod("OnTrigger");
            MethodInfo spawnAction = typeof(ContextActionSpawnMonster).GetMethod("RunAction");
            const BindingFlags statics = BindingFlags.NonPublic | BindingFlags.Static;
            try
            {
                var routes = SerpentineFinalReviewPolicy.PublishedRoutes();
                bool closed = routes.Length == 37 && routes.Select(v => v.StableKey).Distinct().Count() == 37 &&
                    routes.Count(v => v.Creature.Key == "viper") == 18 &&
                    routes.Count(v => v.Creature.Key == "constrictor-snake") == 14 &&
                    routes.Count(v => v.Creature.Key == "salamander") == 5;
                CheckSprint17Final("published-route-census", closed, new JObject {
                    ["newSnakeRoots"] = 32, ["preservedSalamanderRoots"] = 5,
                    ["roots"] = new JArray(routes.Select(v => v.StableKey)) },
                    "all32 new snake roots and exactly5 preserved Salamander roots;no unrelated-root replay");
                if (!closed) throw new InvalidOperationException("Closed Sprint17 publication scope changed.");
                fixture.Caster.Descriptor.Stats.Intelligence.BaseValue = 30;
                var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "ba34257984f4c41408ce1dc2004e342e", "native Wizard targeted player path");
                AdvanceDisposableSpellcaster(fixture.Caster.Descriptor, wizard, 20, ref levelController);
                Spellbook book = fixture.Caster.Descriptor.GetSpellbook(wizard);
                while (book.CasterLevel < 20) book.AddCasterLevel();
                book.UpdateAllSlotsSize(false); book.Rest();
                var parents = ExpandedSummoningInventoryObserver.CanonicalParentGuids.Select(guid =>
                    BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, guid,
                        "canonical summon parent")).ToArray();
                _context.Harmony.Patch(castRule, null, new HarmonyMethod(typeof(RuntimeTestRunner)
                    .GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                _context.Harmony.Patch(spawnAction, new HarmonyMethod(typeof(RuntimeTestRunner)
                    .GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                foreach (var variant in routes)
                {
                    var before = SnapshotReferences(fixture.AllUnits);
                    var cases = new List<ExpandedSummoningPlayerPathCase>();
                    AddExpandedSummoningPlayerPathRootCase(cases, fixture.Blueprints, fixture.Caster, book, parents,
                        variant, ExpandedSummoningPlayerPathAlignmentFor(variant),
                        variant.Family == SummonFamily.NaturesAlly ? (SummonAlignmentMode?)SummonAlignmentMode.Caster :
                            variant.Creature.MonsterTemplated ? (SummonAlignmentMode?)SummonAlignmentMode.Celestial : null,
                        fixture.SceneEntities, fixture.AllUnits, variant.StableKey);
                    var item = cases.Single();
                    // The existing summon witness resets this list at each
                    // cast. Preserve its native duration evidence after cleanup.
                    var durations = ExpandedSummoningRuleDurationCapture.ToArray();
                    bool duration = durations.Length == item.LiveCount && durations.All(value =>
                        Math.Abs(value.BaseDuration.TotalSeconds - 120d) <= .001d &&
                        value.BonusDuration.TotalSeconds >= 0 && value.Unit != null && value.Unit.Destroyed);
                    bool cleanup = before.SequenceEqual(SnapshotReferences(fixture.AllUnits));
                    bool exact = item.LiveContract && item.SlotContract && item.CommandStarted &&
                        item.CommandResult == "Success" && item.RuleCastCount == 1 && item.SpawnActionCount == 1 &&
                        item.QuantityContract && duration && cleanup;
                    CheckSprint17Final("published-root-" + variant.StableKey, exact,
                        new JObject { ["detail"] = item.Describe(), ["nativeDurationExact"] = duration,
                            ["nativeCleanupExact"] = cleanup,
                            ["nativeDurations"] = new JArray(durations.Select(value => new JObject {
                                ["baseSeconds"] = value.BaseDuration.TotalSeconds,
                                ["bonusSeconds"] = value.BonusDuration.TotalSeconds })) },
                        "native parent/variant spellbook path;one slot/RuleCast/spawn action;exact quantity/template/source/CL20 duration/renderable view and cleanup");
                }
            }
            finally
            {
                _expandedSummoningPlayerPathCaptureActive = false;
                foreach (var method in new[] { castRule, spawnAction })
                    _context.Harmony.Unpatch(method, HarmonyPatchType.All, _context.ModId);
                ExpandedSummoningPlayerPathEvents.Clear();
                if (levelController != null) levelController.GetType().GetMethod("Cancel").Invoke(levelController, null);
            }
        }

        private UnitEntityData CastSprint17FinalSnake(ExpandedSummoningCorrectionFixture fixture, string key)
        {
            UnitEntityData unit = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                ExpandedSummoningOwnTierVariant(key, SummonMultiplicity.One), null, fixture.Evidence).Single();
            fixture.Created.Add(unit);
            SetExpandedSummoningBrainActive(unit, false);
            if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
            string survey;
            PlaceExpandedSummoningUnit(unit, FindExpandedSummoningArtPoint(fixture.Caster, out survey));
            return unit;
        }

        private IEnumerable<int> WaitSprint17FinalAppearance(UnitEntityData[] units)
        {
            int frame = 0;
            while (++frame <= 600)
            {
                Game.Instance.IsPaused = false;
                yield return 0;
                if (units.All(unit => unit.View != null && unit.Descriptor.State.CanAct &&
                    unit.Descriptor.State.CanMove && EntityFadedIn(unit) &&
                    unit.Descriptor.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) == null &&
                    Sprint17OriginalBody(unit) != null &&
                    Sprint17BodyIntact(unit.View, Sprint17BodyName(Sprint17ReviewKey(unit)))))
                    yield break;
            }
            throw new InvalidOperationException("Owned original snake appearance did not settle natively.");
        }

        private IEnumerable<int> ReviewSprint17PrivateRoutes(ExpandedSummoningCorrectionFixture fixture)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var variants = SerpentineFinalReviewPolicy.Routes().Concat(
                ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Where(v => v.Creature.Key == "salamander")).ToArray();
            bool closed = variants.Length == 37 &&
                variants.Count(value => value.Creature.Key == "viper") == 18 &&
                variants.Count(value => value.Creature.Key == "constrictor-snake") == 14 &&
                variants.Count(value => value.Creature.Key == "salamander") == 5 &&
                variants.All(value => value.Creature.Key == "salamander" ? SummonVisibilityCatalog.IsPublished(value) :
                    !SummonVisibilityCatalog.IsPublished(value));
            CheckSprint17Final("private-route-census", closed, new JObject {
                ["routes"] = new JArray(variants.Select(value => value.StableKey)),
                ["published"] = variants.Count(SummonVisibilityCatalog.IsPublished) },
                "exact18 Viper/14 Constrictor hidden roots plus5 preserved published Salamander roots");
            if (!closed) throw new InvalidOperationException("Private-only snake route census changed.");
            var group = GroupController.Instance;
            var subGroup = Resources.FindObjectsOfTypeAll<ActionBarSpellsGroup>().First();
            var prefab = (ActionBarSpontaneousConvertedSlot)typeof(ActionBarSpellsGroup)
                .GetField("m_ActionBarSpontaneousConvertedSlotPrefab", flags).GetValue(subGroup);
            foreach (var variant in variants)
            {
                UnitEntityData[] units = new UnitEntityData[0];
                UnityEngine.Object[] resources = new UnityEngine.Object[0];
                ActionBarSpontaneousConvertedSlot widget = null;
                try
                {
                    Game.Instance.IsPaused = true;
                    TimeSpan castTime = Game.Instance.Player.GameTime;
                    units = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                        variant, null, fixture.Evidence);
                    fixture.Created.AddRange(units);
                    foreach (var unit in units) SetExpandedSummoningBrainActive(unit, false);
                    if (variant.Creature.Key == "salamander")
                    {
                        foreach (int step in WaitSprint17FinalAppearance(units)) yield return step;
                        Game.Instance.IsPaused = true;
                    }
                    resources = units.SelectMany(Sprint17ViewResources).ToArray();
                    var root = ExpandedSummoningRoot(fixture.Blueprints, variant);
                    widget = Kingmaker.UI.WidgetFactory.GetWidget(prefab);
                    widget.transform.SetParent(group.transform, false);
                    widget.Initialize();
                    widget.Set(fixture.Caster, new AbilityData(root, fixture.Caster.Descriptor));
                    for (int frame = 0; frame < 3; frame++) yield return 0;
                    bool templateExpected = variant.Family == SummonFamily.Monster && variant.Creature.MonsterTemplated;
                    var templates = units.Select(unit => unit.Descriptor.Buffs.Enumerable.Where(buff =>
                        buff.Blueprint.name.StartsWith("KMG_Summoning_Template_", StringComparison.Ordinal)).ToArray()).ToArray();
                    bool templatesExact = templates.All(buffs => templateExpected ?
                        buffs.Length == 1 && buffs[0].Blueprint.name.IndexOf("Celestial", StringComparison.Ordinal) >= 0 :
                        buffs.Length == 0);
                    bool icon = root.Icon != null && widget.gameObject.activeInHierarchy &&
                        widget.Icon.isActiveAndEnabled && ReferenceEquals(widget.Icon.sprite, root.Icon);
                    var durations = new JArray();
                    bool durationExact = true, alignmentExact = true;
                    foreach (var unit in units)
                    {
                        string duration;
                        double elapsed = variant.Creature.Key == "salamander" ?
                            (Game.Instance.Player.GameTime - castTime).TotalSeconds : 0;
                        bool exact = Sprint17PrivateRtwpDurationExact(unit, fixture.Caster, out duration, elapsed);
                        durationExact &= exact;
                        durations.Add(new JObject { ["unit"] = unit.UniqueId, ["exact"] = exact, ["nativeRuleAndBuff"] = duration });
                        if (templateExpected)
                        {
                            int expectedAlignment;
                            alignmentExact &= SummonAlignmentRuntimePolicy.TryResolve(SummonAlignmentMode.Celestial,
                                (int)unit.Blueprint.Alignment, null, out expectedAlignment) &&
                                (int)unit.Descriptor.Alignment.Value == expectedAlignment;
                        }
                    }
                    bool unitsExact = units.All(unit => ReferenceEquals(unit.Blueprint, ExpandedSummoningUnit(fixture.Blueprints, variant)) &&
                        ExpandedSummoningPlayerPathUnitExact(unit, fixture.Caster) &&
                        Sprint17OriginalBody(unit) != null);
                    CheckSprint17Final("private-root-" + variant.StableKey,
                        SerpentineFinalReviewPolicy.Quantity(variant.Multiplicity, units.Length) && unitsExact &&
                            templatesExact && alignmentExact && durationExact && icon,
                        new JObject { ["root"] = root.AssetGuid, ["units"] = units.Length, ["liveExact"] = unitsExact,
                            ["templatesExact"] = templatesExact, ["nativeWidgetIcon"] = icon,
                            ["alignmentExact"] = alignmentExact, ["nativeDurationExact"] = durationExact,
                            ["nativeRuleDurations"] = durations,
                            ["icon"] = root.Icon == null ? null : root.Icon.name,
                            ["templateBuffs"] = new JArray(templates.Select(buffs => new JArray(buffs.Select(buff => buff.Blueprint.name)))),
                            ["nativeDurations"] = new JArray(units.Select(unit => unit.Descriptor.Buffs.Enumerable.Single(buff =>
                                ReferenceEquals(buff.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff)).TimeLeft.TotalSeconds)),
                            ["ids"] = new JArray(units.Select(unit => unit.UniqueId)), ["published"] = SummonVisibilityCatalog.IsPublished(variant) },
                        "actual private execution, exact quantity/identity/source/duration/template/original view and native icon widget; NOT public spellbook navigation");
                }
                finally
                {
                    if (widget != null) Kingmaker.UI.WidgetFactory.DisposeWidget(widget);
                    DisposeExpandedSummoningUnits(fixture.Created, units);
                }
                for (int frame = 0; frame < 3; frame++) yield return 0;
                CheckSprint17Final("private-cleanup-" + variant.StableKey,
                    units.Length > 0 && units.All(unit => unit.Destroyed && unit.View == null && unit.HoldingState == null) &&
                    resources.Length >= units.Length * 5 && resources.All(value => value == null),
                    new JObject { ["units"] = units.Length, ["resources"] = resources.Length,
                        ["remaining"] = resources.Count(value => value != null) },
                    "native destruction reclaims every captured private object before the next root");
            }
        }

        private static bool Sprint17PrivateRtwpDurationExact(UnitEntityData unit,
            UnitEntityData caster, out string observation, double nativeElapsedSeconds = 0)
        {
            // Retain the existing detailed native capture, but do not use its
            // turn-based-only six-second expectation for this RTWP request.
            ExpandedSummoningDurationExact(unit, caster, out observation);
            var captures = ExpandedSummoningRuleDurationCapture.Where(value =>
                ReferenceEquals(value.Unit, unit)).ToArray();
            var buffs = unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                ReferenceEquals(value.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff)).ToArray();
            var capture = captures.SingleOrDefault();
            var buff = buffs.SingleOrDefault();
            bool turnBased = Kingmaker.UI.SettingsUI.SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            observation += ",mode=" + (turnBased ? "turn-based" : "real-time") + ",expectedGrace=0";
            return SerpentineFinalReviewPolicy.PrivateRtwpDuration(captures.Length, buffs.Length, turnBased,
                buff != null && buff.MaybeContext != null && ReferenceEquals(buff.MaybeContext.MaybeCaster, caster),
                buff == null || buff.MaybeContext == null ? -1 : buff.MaybeContext.Params.CasterLevel,
                buff != null && buff.IsPermanent, capture == null ? double.NaN : capture.BaseDuration.TotalSeconds,
                capture == null ? double.NaN : capture.BonusDuration.TotalSeconds,
                buff == null ? double.NaN : buff.TimeLeft.TotalSeconds, nativeElapsedSeconds);
        }

        private IEnumerable<int> ReviewSprint17SnakeNativeUi(ExpandedSummoningCorrectionFixture fixture)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var game = Game.Instance;
            var ui = game.UI;
            var group = GroupController.Instance;
            if (ui.ServiceWindow == null || ui.ServiceWindow.WindowTabs.IsShow ||
                ui.SelectionManagerPC == null || group == null || game.IsControllerGamepad)
                throw new InvalidOperationException("Snake native UI requires idle desktop service UI.");
            var sheet = ui.ServiceWindow.WindowTabs.SubWindowsList.Select(value => value.SubWindow)
                .OfType<CharacterScreenController>().Single();
            var characterField = typeof(CharacterScreenController).GetField("m_CurrentCharacter", flags);
            var sectionField = typeof(CharacterScreenController).GetField("m_CurrentSection", flags);
            var original = (UnitDescriptor)characterField.GetValue(sheet);
            int section = (int)sectionField.GetValue(sheet);
            var groupCharacter = group.GetCurrentCharacter();
            var restore = original ?? groupCharacter?.Descriptor;
            var selected = ui.SelectionManagerPC.SelectedUnits.ToArray();
            bool pause = game.IsPaused, opened = false;
            var clock = game.Player.GameTime;
            var random = UnityEngine.Random.state;
            var owners = new List<UnitEntityData>();
            var venom = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name == "KMG_Summoning_Natural_Viper_Venom");
            var target = fixture.Hostile;
            int fort = target.Descriptor.Stats.SaveFortitude.BaseValue;
            if (restore == null || sheet.IsShow) throw new InvalidOperationException("No exact idle native sheet.");
            try
            {
                var viper = CastSprint17FinalSnake(fixture, "viper"); owners.Add(viper);
                var constrictor = CastSprint17FinalSnake(fixture, "constrictor-snake"); owners.Add(constrictor);
                var salamander = CastSprint17FinalSnake(fixture, "salamander"); owners.Add(salamander);
                foreach (int step in WaitSprint17FinalAppearance(owners.ToArray())) yield return step;
                game.IsPaused = true;
                target.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                RuleAttackWithWeapon attack;
                Buff applied = DeliverSprint17Venom(viper, target, venom, out attack);
                Buff trait = constrictor.Buffs.Enumerable.Single(value => value.Blueprint.name == "KMG_Summoning_Special_ConstrictorSnake_CombatTraits");
                Buff salamanderTrait = salamander.Buffs.Enumerable.Single(value => value.Blueprint.name == "KMG_Summoning_Special_Salamander_CombatTraits");
                CheckSprint17Salamander("inspectable-type-icon", salamander.Blueprint.Type != null &&
                    ReferenceEquals(salamander.Blueprint.Type.Image,
                        KingmakerGunslinger.Blueprints.ExpandedSummoningProjectIcons.Require("salamander")),
                    new JObject { ["type"] = salamander.Blueprint.Type == null ? null : salamander.Blueprint.Type.name,
                        ["carrier"] = "final live Unit.Blueprint.Type.Image; the native trait widget is observed separately below" },
                    "exact existing Salamander painting on its live inspectable unit type, not donor identity");
                if (applied == null || attack.MeleeDamage == null || attack.MeleeDamage.Damage <= 0 ||
                    !ReferenceEquals(applied.Context.MaybeCaster, viper))
                    throw new InvalidOperationException("Actual Viper bite did not establish the source-owned UI state.");
                opened = true; ui.ServiceWindow.HandleOpenCharScreen();
                for (int frame = 0; frame < 10; frame++) yield return 0;
                int buffSection = sheet.BuffsAndConditions.SectionGroupIndex.First();
                foreach (Buff buff in new[] { trait, applied, salamanderTrait })
                {
                    sheet.SetCharacter(buff.Owner);
                    sheet.ShowSection(buffSection);
                    sheet.BuffsAndConditions.SetDirty(); sheet.Refresh();
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    var rows = sheet.BuffsAndConditions.GetComponentsInChildren<CharSComponentBuffSlot>(true)
                        .Where(value => value.gameObject.activeInHierarchy && ReferenceEquals(value.Buff, buff)).ToArray();
                    var row = rows.SingleOrDefault();
                    bool exactAssignment = ReferenceEquals(buff, trait) ? ReferenceEquals(buff.Blueprint.Icon,
                        KingmakerGunslinger.Blueprints.ExpandedSummoningProjectIcons.Require("constrictor-snake")) :
                        !ReferenceEquals(buff, salamanderTrait) || ReferenceEquals(buff.Blueprint.Icon,
                            KingmakerGunslinger.Blueprints.ExpandedSummoningProjectIcons.Require("salamander"));
                    bool exact = row != null && sheet.IsShow && sheet.BuffsAndConditions.IsShowed &&
                        exactAssignment && buff.Blueprint.Icon != null && row.Icon.isActiveAndEnabled &&
                        ReferenceEquals(row.Icon.sprite, buff.Blueprint.Icon) &&
                        row.Name.text == buff.Name && !row.Name.isTextTruncated;
                    CheckSprint17Final(buff.Blueprint.name + "-native-buff-row", exact,
                        new JObject { ["owner"] = buff.Owner.Unit.UniqueId, ["buff"] = buff.Blueprint.AssetGuid,
                            ["rows"] = rows.Length, ["sprite"] = row == null || row.Icon.sprite == null ? null : row.Icon.sprite.name,
                            ["exactAssignment"] = exactAssignment,
                            ["label"] = row == null ? null : row.Name.text, ["expectedLabel"] = buff.Name },
                        "one real native trait/venom status row with exact project icon and full label");
                }
            }
            finally
            {
                sheet.SetCharacter(restore); sheet.ShowSection(section);
                if (opened && ui.ServiceWindow.WindowTabs.IsShow && sheet.IsShow) ui.ServiceWindow.HandleOpenCharScreen();
                if (opened && sheet.IsShow) sheet.Show(false);
                if (original == null && ReferenceEquals(characterField.GetValue(sheet), restore)) characterField.SetValue(sheet, null);
                foreach (var row in sheet.BuffsAndConditions.GetComponentsInChildren<CharSComponentBuffSlot>(true))
                    if (row.Buff != null && (owners.Contains(row.Buff.Owner.Unit) || ReferenceEquals(row.Buff.Owner.Unit, target)))
                    { row.Clear(); row.Buff = null; row.Hide(true); }
                ClearSprint14Venom(target, venom);
                target.Descriptor.Stats.SaveFortitude.BaseValue = fort;
                ResetExpandedSummoningHostile(fixture);
                DisposeExpandedSummoningUnits(fixture.Created, owners.ToArray());
                UnityEngine.Random.state = random;
                group.SelectUnit(groupCharacter);
                ui.SelectionManagerPC.MultiSelect(selected.Select(value => value.View).ToArray(), false);
                game.Player.GameTime = clock; game.IsPaused = pause;
                bool restored = !ui.ServiceWindow.WindowTabs.IsShow && !sheet.IsShow &&
                    ReferenceEquals(characterField.GetValue(sheet), original) && (int)sectionField.GetValue(sheet) == section &&
                    ReferenceEquals(group.GetCurrentCharacter(), groupCharacter) &&
                    ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected) &&
                    game.Player.GameTime == clock && game.IsPaused == pause;
                CheckSprint17Final("native-ui-restoration", restored, new JObject { ["restored"] = restored,
                    ["tabsClosed"] = !ui.ServiceWindow.WindowTabs.IsShow, ["sheetClosed"] = !sheet.IsShow },
                    "exact sheet/group/selection/cache/section/pause/time restored through native close");
            }
            for (int frame = 0; frame < 5; frame++) yield return 0;
        }
    }
}
