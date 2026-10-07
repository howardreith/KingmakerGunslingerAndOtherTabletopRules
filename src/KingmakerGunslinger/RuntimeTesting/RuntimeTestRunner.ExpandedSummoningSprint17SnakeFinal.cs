using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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
            foreach (int step in ReviewSprint17PrivateRoutes(fixture)) yield return step;
            CreateExpandedSummoningCorrectionHostile(fixture);
            foreach (int step in ReviewSprint17SnakeNativeUi(fixture)) yield return step;
            foreach (int step in ReviewSprint17SnakeViewLifecycle(fixture)) yield return step;
        }

        private void CheckSprint17Final(string name, bool pass, JObject row, string contract)
        {
            row = (JObject)row.DeepClone();
            row["check"] = name; row["passed"] = pass;
            _serpentineBodyRows.Add(row);
            _serpentineBodyAssertions.Add(Assertion("sprint17-final-" + name, contract,
                row.ToString(Formatting.None), pass,
                "Closed disposable snake routes/UI/lifecycle only; not publication or complete Sprint17."));
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
                    unit.View.GetComponent<SerpentineVisualAttachment>() != null &&
                    unit.View.GetComponent<SerpentineVisualAttachment>().OriginalBodyLive &&
                    Sprint17BodyIntact(unit.View, SerpentineVisualPolicy.BodyRenderer(
                        unit.Blueprint.name == "KMG_Summoning_Unit_Viper" ? "viper" : "constrictor-snake"))))
                    yield break;
            }
            throw new InvalidOperationException("Owned original snake appearance did not settle natively.");
        }

        private IEnumerable<int> ReviewSprint17PrivateRoutes(ExpandedSummoningCorrectionFixture fixture)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var variants = SerpentineFinalReviewPolicy.Routes();
            bool closed = variants.Length == 32 &&
                variants.Count(value => value.Creature.Key == "viper") == 18 &&
                variants.Count(value => value.Creature.Key == "constrictor-snake") == 14 &&
                variants.All(value => !SummonVisibilityCatalog.IsPublished(value));
            CheckSprint17Final("private-route-census", closed, new JObject {
                ["routes"] = new JArray(variants.Select(value => value.StableKey)),
                ["published"] = variants.Count(SummonVisibilityCatalog.IsPublished) },
                "exact 18 Viper and14 Constrictor hidden roots; no publication mutation");
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
                    units = CastExpandedSummoningVariant(fixture.Blueprints, fixture.Caster,
                        variant, null, fixture.Evidence);
                    fixture.Created.AddRange(units);
                    foreach (var unit in units) SetExpandedSummoningBrainActive(unit, false);
                    resources = units.SelectMany(value =>
                        value.View.GetComponent<SerpentineVisualAttachment>().CaptureOwnedResources()).ToArray();
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
                        bool exact = ExpandedSummoningDurationExact(unit, fixture.Caster, out duration);
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
                        ExpandedSummoningSerpentineViewPatch.DescribeView(unit.View).StartsWith("visual:attached;", StringComparison.Ordinal));
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
                            ["ids"] = new JArray(units.Select(unit => unit.UniqueId)), ["published"] = false },
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
                foreach (int step in WaitSprint17FinalAppearance(owners.ToArray())) yield return step;
                game.IsPaused = true;
                target.Descriptor.Stats.SaveFortitude.BaseValue = -100;
                RuleAttackWithWeapon attack;
                Buff applied = DeliverSprint17Venom(viper, target, venom, out attack);
                Buff trait = constrictor.Buffs.Enumerable.Single(value => value.Blueprint.name == "KMG_Summoning_Special_ConstrictorSnake_CombatTraits");
                if (applied == null || attack.MeleeDamage == null || attack.MeleeDamage.Damage <= 0 ||
                    !ReferenceEquals(applied.Context.MaybeCaster, viper))
                    throw new InvalidOperationException("Actual Viper bite did not establish the source-owned UI state.");
                opened = true; ui.ServiceWindow.HandleOpenCharScreen();
                for (int frame = 0; frame < 10; frame++) yield return 0;
                int buffSection = sheet.BuffsAndConditions.SectionGroupIndex.First();
                foreach (Buff buff in new[] { trait, applied })
                {
                    sheet.SetCharacter(buff.Owner);
                    sheet.ShowSection(buffSection);
                    sheet.BuffsAndConditions.SetDirty(); sheet.Refresh();
                    for (int frame = 0; frame < 12; frame++) yield return 0;
                    var rows = sheet.BuffsAndConditions.GetComponentsInChildren<CharSComponentBuffSlot>(true)
                        .Where(value => value.gameObject.activeInHierarchy && ReferenceEquals(value.Buff, buff)).ToArray();
                    var row = rows.SingleOrDefault();
                    bool exact = row != null && sheet.IsShow && sheet.BuffsAndConditions.IsShowed &&
                        buff.Blueprint.Icon != null && row.Icon.isActiveAndEnabled &&
                        ReferenceEquals(row.Icon.sprite, buff.Blueprint.Icon) &&
                        row.Name.text == buff.Name && !row.Name.isTextTruncated;
                    CheckSprint17Final(buff.Blueprint.name + "-native-buff-row", exact,
                        new JObject { ["owner"] = buff.Owner.Unit.UniqueId, ["buff"] = buff.Blueprint.AssetGuid,
                            ["rows"] = rows.Length, ["sprite"] = row == null || row.Icon.sprite == null ? null : row.Icon.sprite.name,
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
