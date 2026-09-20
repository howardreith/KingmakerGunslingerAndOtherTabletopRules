using System;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Verified from the installed Favored Class blueprint export, not a
        // name search or a synthesized selection grant.
        private const string CircleOracleClassId = "32c02466b2364c8a906e6e4761175099";
        private const string CircleFavoredThirdId = "bab7a67de47e4b6690c03fd5b744c482";
        private const string CircleFavoredOracleId = "52ee82659e040ed231ed36c8e5457e38";
        private const string CircleFavoredSpellId = "9ba3858327354e2093613efb9de198d7";
        private const string CircleFavoredPartialId = "b19759026d6508b9022f1edb4ec4b31f";
        private const string CircleFavoredHpId = "c16eded5cc1948faab43177135fc7845";

        private void PrepareCircleFavoredOracle(UnitEntityData unit)
        {
            if (!BlueprintBootstrap.Library.BlueprintsByAssetId.ContainsKey(CircleFavoredThirdId)) {
                _circlePersistenceRecord["favoredClass"] = new JObject { ["status"] = "optional feature absent; unqualified" };
                return;
            }
            var oracle = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library, CircleOracleClassId, "installed Oracle");
            var third = BlueprintLibraryLookup.RequireExact<BlueprintParametrizedFeature>(BlueprintBootstrap.Library, CircleFavoredThirdId, "installed Oracle third-level favored spell choice");
            var human = BlueprintLibraryLookup.RequireExact<BlueprintRace>(BlueprintBootstrap.Library, "0a5d473ead98b0646b94495af250fdc4", "native Human");
            var chosen = BlueprintBootstrap.MagicCircles.Single(circle => circle.Alignment == "Evil").Spell;
            var audit = new JObject { ["feature"] = CircleAuditIdentity(third), ["fields"] = CircleAuditFields(third, 0),
                ["components"] = new JArray(third.ComponentsArray.Select(component => CircleAuditValue(component, 0))),
                ["route"] = new JArray(), ["status"] = "probing" };
            _circlePersistenceRecord["favoredClass"] = audit;
            if (third.name != "FavoredOracleOracleSpellList3ParametrizedFeature" || third.SpellList != oracle.Spellbook.SpellList ||
                third.SpellcasterClass != oracle || third.SpellLevel != 3)
                throw new InvalidOperationException("Installed Favored Class identity/Oracle relationship differs.");
            CircleFavoredPublicationContracts();
            unit.Stats.Charisma.BaseValue = 18; unit.Stats.Intelligence.BaseValue = 10;
            unit.Descriptor.Alignment.Set(Alignment.LawfulGood);
            bool selectedCircle = false;
            for (int level = 1; level <= 8; level++) {
                LevelUpController backend = null;
                try {
                    backend = LevelUpController.StartWithoutAssigningStaticInstance(unit.Descriptor, false, null, null,
                        level == 1 ? LevelUpState.CharBuildMode.CharGen : LevelUpState.CharBuildMode.LevelUp);
                    if (level == 1) { backend.SelectRace(human); backend.SelectGender(unit.Descriptor.Gender); backend.SelectAlignment(Alignment.LawfulGood); }
                    if (!backend.SelectClass(oracle)) throw new InvalidOperationException("Oracle native class selection failed.");
                    for (int step = 0; step < 256; step++) {
                        if (backend.State.AttributePoints > 0 && backend.SpendAttributePoint(StatType.Charisma)) continue;
                        var selection = backend.State.Selections.FirstOrDefault(value => !value.Selected && value.CanSelectAnything(backend.State, backend.Preview));
                        if (selection != null) {
                            var items = selection.Selection.ExtractSelectionItems(backend.Unit, backend.Preview).ToArray();
                            if (ReferenceEquals(selection.Selection, third)) {
                                var circles = BlueprintBootstrap.MagicCircles;
                                var rows = new JArray(circles.Select(circle => new JObject { ["spell"] = circle.Spell.AssetGuid,
                                    ["extracted"] = items.Count(item => item.Param?.Blueprint == circle.Spell),
                                    ["items"] = third.Items.Count(item => item.Param?.Blueprint == circle.Spell),
                                    ["canSelect"] = items.Any(item => item.Param?.Blueprint == circle.Spell && third.CanSelect(backend.Preview, backend.State, selection, item)) }));
                                audit["enumeration"] = rows;
                                var circleItem = items.SingleOrDefault(value => value.Param?.Blueprint == chosen);
                                bool allowed = circleItem != null && third.CanSelect(backend.Preview, backend.State, selection, circleItem);
                                int knownBefore = backend.Preview.GetSpellbook(oracle.Spellbook).GetKnownSpells(3).Count();
                                CirclePersistenceCheck("favored-native-enumeration", rows.All(row => (int)row["extracted"] == 1 && (int)row["items"] == 1 && (bool)row["canSelect"]),
                                    "exact installed Oracle third-level feature enumerates all four distinct parameters through native Items, extraction and CanSelect");
                                if (level != 8 || selectedCircle || !allowed || !backend.SelectFeature(selection, circleItem))
                                    throw new InvalidOperationException("Native Favored Class Magic Circle selection rejected; see real enumeration evidence.");
                                selectedCircle = true;
                                int knownAfter = backend.Preview.GetSpellbook(oracle.Spellbook).GetKnownSpells(3).Count();
                                audit["favoredKnownBefore"] = knownBefore; audit["favoredKnownAfter"] = knownAfter;
                                CirclePersistenceCheck("favored-one-known-choice", knownAfter == knownBefore + 1 &&
                                    BlueprintBootstrap.MagicCircles.Count(circle => backend.Preview.GetSpellbook(oracle.Spellbook).IsKnown(circle.Spell)) == 1,
                                    "one real parametrized selection adds exactly one known spell and one alignment variant");
                                ((JArray)audit["route"]).Add(new JObject { ["level"] = level, ["selection"] = CircleFavoredThirdId, ["parameter"] = chosen.AssetGuid });
                                continue;
                            }
                            if (level >= 7 && (selection.Selection as BlueprintScriptableObject)?.AssetGuid == "c6f18fa1194d0bfb35e1913983b8da98") {
                                string intended = level == 7 ? CircleFavoredPartialId : CircleFavoredSpellId;
                                audit["favoredOptions" + level] = new JArray(items.Select(value => new JObject {
                                    ["feature"] = CircleAuditIdentity(value.Feature), ["canSelect"] = selection.Selection.CanSelect(backend.Preview, backend.State, selection, value) }));
                                if (!items.Any(value => value.Feature.AssetGuid == intended && selection.Selection.CanSelect(backend.Preview, backend.State, selection, value)))
                                    throw new InvalidOperationException("Installed two-level Favored Class route is unavailable at level " + level);
                            }
                            var choice = items.Where(item => selection.Selection.CanSelect(backend.Preview, backend.State, selection, item))
                                .OrderBy(item => item.Feature.AssetGuid == CircleFavoredOracleId ? 0 :
                                    (level == 7 && item.Feature.AssetGuid == CircleFavoredPartialId ||
                                        level == 8 && (item.Feature.AssetGuid == CircleFavoredSpellId || item.Feature.AssetGuid == CircleFavoredThirdId)) ? 1 :
                                    item.Feature.AssetGuid == CircleFavoredHpId ? 2 : item.Feature.name.Contains("BattleMystery") ? 3 : 4)
                                .ThenBy(item => item.Feature.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
                            if (choice != null && backend.SelectFeature(selection, choice)) {
                                ((JArray)audit["route"]).Add(new JObject { ["level"] = level,
                                    ["selection"] = CircleAuditIdentity(selection.Selection as BlueprintScriptableObject), ["choice"] = CircleAuditIdentity(choice.Feature) });
                                continue;
                            }
                        }
                        bool filled = false;
                        foreach (var spells in backend.State.SpellSelections.ToArray()) {
                            var book = backend.Preview.GetSpellbook(spells.Spellbook);
                            for (int spellLevel = 0; spellLevel < spells.LevelCount.Length && !filled; spellLevel++) {
                                var slots = spells.LevelCount[spellLevel]; if (slots == null) continue;
                                int slot = Array.FindIndex(slots.SpellSelections, value => value == null); if (slot < 0) continue;
                                var spell = spells.SpellList.GetSpells(spellLevel).Where(value => !book.IsKnown(value) &&
                                    !BlueprintBootstrap.MagicCircles.Any(circle => ReferenceEquals(circle.Spell, value)))
                                    .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
                                if (spell != null) filled = backend.SelectSpell(spells.Spellbook, spells.SpellList, spellLevel, spell, slot);
                            }
                            if (filled) break;
                        }
                        if (filled) continue;
                        foreach (StatType skill in Enum.GetValues(typeof(StatType)))
                            if (skill.ToString().StartsWith("Skill", StringComparison.Ordinal) && backend.State.SkillPointsRemaining > 0 && backend.SpendSkillPoint(skill)) { filled = true; break; }
                        if (!filled) break;
                    }
                    if (level == 8) {
                        audit["complete"] = backend.State.IsComplete();
                        audit["remainingSelections"] = new JArray(backend.State.Selections.Where(value => !value.Selected).Select(value => CircleAuditIdentity(value.Selection as BlueprintScriptableObject)));
                        audit["normalSlots3"] = new JArray(backend.State.SpellSelections.SelectMany(value => value.LevelCount[3]?.SpellSelections ?? Array.Empty<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>()).Select(value => value?.AssetGuid));
                        int ordinaryAllowance = (oracle.Spellbook.SpellsKnown.GetCount(8, 3) ?? 0) - (oracle.Spellbook.SpellsKnown.GetCount(7, 3) ?? 0);
                        CirclePersistenceCheck("favored-normal-known-limit", ((JArray)audit["normalSlots3"]).Count == ordinaryAllowance,
                            "Favored Class choice leaves the native ordinary level-three known-spell allowance unchanged");
                        if (!selectedCircle || !backend.State.IsComplete()) throw new InvalidOperationException("Legitimate Oracle level-up is incomplete; no commit/save authorized.");
                        backend.Commit(); backend = null;
                    } else {
                        // Seed prior levels with native selection actions; final
                        // level uses the public commit and normal completeness gate.
                        typeof(LevelUpController).GetMethod("ApplyLevelup", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(backend, new object[] { unit.Descriptor });
                    }
                } finally { backend?.Cancel(); }
            }
            audit["committed"] = CircleFavoredOracleState(unit);
            audit["status"] = "native selection committed; fresh persistence pending";
            VerifyCircleFavoredOracle(unit);
        }

        private void CircleFavoredPublicationContracts()
        {
            var fixture = UnityEngine.ScriptableObject.CreateInstance<BlueprintParametrizedFeature>();
            var circles = BlueprintBootstrap.MagicCircles;
            var replacement = UnityEngine.Object.Instantiate(circles[0].Spell);
            var transaction = new MagicCircleSpellListPublication();
            try {
                fixture.ParameterType = FeatureParameterType.LearnSpell;
                fixture.BlueprintParameterVariants = Array.Empty<BlueprintScriptableObject>();
                var before = fixture.Items.ToArray(); // Warm the real native cache.
                foreach (var circle in circles) transaction.AddFavoredParameter(fixture, circle.Spell);
                foreach (var circle in circles) transaction.AddFavoredParameter(fixture, circle.Spell);
                bool once = fixture.Items.Count() == 4 && circles.All(circle =>
                    fixture.Items.Count(item => ReferenceEquals(item.Param?.Blueprint, circle.Spell)) == 1);
                transaction.Rollback();
                bool removed = !fixture.Items.Any() && fixture.BlueprintParameterVariants.Length == 0;
                foreach (var circle in circles) transaction.AddFavoredParameter(fixture, circle.Spell);
                fixture.BlueprintParameterVariants = fixture.BlueprintParameterVariants
                    .Select(value => ReferenceEquals(value, circles[0].Spell) ? replacement : value)
                    .Concat(new BlueprintScriptableObject[] { circles[0].Delivery }).ToArray();
                transaction.Rollback();
                CirclePersistenceCheck("favored-publication-ownership", before.Length == 0 && once && removed &&
                    fixture.BlueprintParameterVariants.Length == 2 && fixture.BlueprintParameterVariants.Contains(replacement) &&
                    fixture.BlueprintParameterVariants.Contains(circles[0].Delivery) && fixture.Items.Count() == 2,
                    "production insertion/rollback on unregistered native selector: warmed cache refreshed, idempotent, foreign replacement and addition preserved");
            } finally {
                transaction.Rollback();
                UnityEngine.Object.Destroy(fixture); UnityEngine.Object.Destroy(replacement);
            }
        }

        private static JObject CircleFavoredOracleState(UnitEntityData unit)
        {
            var book = unit.Descriptor.Spellbooks.SingleOrDefault(value => value.Blueprint.AssetGuid == "3587fa91b34341e49b3a22cfb5450e0d");
            return new JObject { ["book"] = book?.Blueprint.AssetGuid, ["level"] = book?.CasterLevel,
                ["normalAllowance3"] = book?.Blueprint.SpellsKnown.GetCount(book.CasterLevel, 3),
                ["known3"] = new JArray(book == null ? Array.Empty<string>() : book.GetKnownSpells(3).Select(value => value.Blueprint.AssetGuid).OrderBy(value => value, StringComparer.Ordinal).ToArray()),
                ["selections"] = new JArray(unit.Descriptor.Progression.Features.Enumerable.Where(value => value.Blueprint.AssetGuid == CircleFavoredThirdId)
                    .Select(value => new JObject { ["feature"] = value.Blueprint.AssetGuid, ["parameter"] = value.Param?.Blueprint?.AssetGuid, ["rank"] = value.Rank })) };
        }

        private void VerifyCircleFavoredOracle(UnitEntityData unit)
        {
            if (!BlueprintBootstrap.Library.BlueprintsByAssetId.ContainsKey(CircleFavoredThirdId)) return;
            var state = CircleFavoredOracleState(unit); var known = (JArray)state["known3"]; var selections = (JArray)state["selections"];
            var chosen = BlueprintBootstrap.MagicCircles.Single(circle => circle.Alignment == "Evil").Spell;
            CirclePersistenceCheck("favored-known-and-parameter", (int?)state["level"] == 8 && selections.Count == 1 &&
                (string)selections[0]["parameter"] == chosen.AssetGuid && (int)selections[0]["rank"] == 1 &&
                known.Count(value => (string)value == chosen.AssetGuid) == 1 &&
                BlueprintBootstrap.MagicCircles.Count(circle => known.Any(value => (string)value == circle.Spell.AssetGuid)) == 1,
                "native Favored Class parameter and exactly one Circle variant persist; no direct AddKnown used for this route");
            _circlePersistenceRecord["favoredClassObserved"] = state;
        }
    }
}
