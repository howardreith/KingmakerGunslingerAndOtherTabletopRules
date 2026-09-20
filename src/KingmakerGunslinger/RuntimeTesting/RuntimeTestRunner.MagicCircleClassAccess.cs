using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static void CircleOptionalPublicationContracts(List<RuntimeTestAssertion> assertions)
        {
            var library = BlueprintBootstrap.Library; var circles = BlueprintBootstrap.MagicCircles;
            // Independently inspect final installed books/lists after every mod's
            // publication. Do not call the optional publisher or synthesize lists.
            var expected = new[] {
                new[] { "Oracle", "32c02466b2364c8a906e6e4761175099", "3587fa91b34341e49b3a22cfb5450e0d", "f305174b73f64783a8379238a14c3283" },
                new[] { "Warpriest", "e119d84528144a7797ad34fd718b1f87", "9995149d6ff043868cb1fd22ae6ac332", "9ef48172d50446aca4c80f321402f743" },
                new[] { "Summoner", "0f4c4ada51334b43a802350c5c0b85f5", "20f4a4b890204302ae9895fdc45bb20c", "972048af37924e59b174653974b255a5" },
                new[] { "Shaman", "6b1d00511e824f0fbc27ec8f54b8edb2", "20fbd5cd3f79455aa9f133ecf21797ab", "7113337f695742559ecdecc8905b132a" },
                new[] { "Spiritualist", "2e31173d30f043aabd910f7a418594ec", "249100aac10349e3b9a8196ae313990f", "71d7e14b9f674272b40ed2d9093f34d5" },
                new[] { "Occultist", "1b76f3c73aa84f91a1c65513fb23aa01", "465a0ae7aa10419cb990a03e4859dca5", "a6ece1eaa2de452987843dff8f7ca01b" },
                new[] { "Antipaladin", "db03b55aa414444cba0b42f3c429fe2d", "564482126cac4778b94d0e7079fe41e9", "dbd2049eee48480abec0ffd7abceae7f" }
            };
            foreach (var row in expected) {
                bool present = library.BlueprintsByAssetId.ContainsKey(row[1]);
                bool pass;
                if (!present) pass = !library.BlueprintsByAssetId.ContainsKey(row[2]) && !library.BlueprintsByAssetId.ContainsKey(row[3]);
                else {
                    var owner = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, row[1], row[0]);
                    var book = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellbook>(library, row[2], row[0]);
                    var list = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellList>(library, row[3], row[0]);
                    pass = ReferenceEquals(owner.Spellbook, book) && ReferenceEquals(book.CharacterClass, owner) &&
                        ReferenceEquals(book.SpellList, list) && circles.All(circle =>
                            list.SpellsByLevel.Single(level => level.SpellLevel == 3).Spells.Count(spell => spell.AssetGuid == circle.Spell.AssetGuid) ==
                            (row[0] == "Antipaladin" && circle.Alignment != "Good" && circle.Alignment != "Law" ? 0 : 1));
                }
                assertions.Add(Assertion("circle-optional-class-" + row[0],
                    "present class retains its native book and exact legitimate level-three entries; absent class creates no book/list",
                    "present=" + present, pass, "final installed library, exact primary-source identities; no publication invoked by this check"));
            }
            foreach (var row in new[] {
                new[] { "ArcanistPrepared", "ab76417567444a6cb87d9d53e9752955", "ba0401fdeb4062f40a7aa95b6f07fe89", "yes" },
                new[] { "ArcanistSpontaneous", "0c21cfcab6ce4395bd4df330ab3cf715", "ba0401fdeb4062f40a7aa95b6f07fe89", "yes" },
                new[] { "UnletteredPrepared", "f0f72c9fd15046cbb20b15851fbc7752", "422490cf62744e16a3e131efd94cf290", "no" },
                new[] { "UnletteredSpontaneous", "7c83e4d8c1db4d21b6b32c9540f25128", "422490cf62744e16a3e131efd94cf290", "no" } }) {
                bool present = library.BlueprintsByAssetId.ContainsKey(row[1]);
                bool pass = true;
                if (present) {
                    var book = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellbook>(library, row[1], row[0]);
                    pass = book.CharacterClass.AssetGuid == "19c3cf3d51cf4cbf9a136a600c26585a" && book.SpellList.AssetGuid == row[2] &&
                        circles.All(circle => book.SpellList.SpellsByLevel.Single(level => level.SpellLevel == 3).Spells.Count(spell => spell.AssetGuid == circle.Spell.AssetGuid) ==
                            (row[3] == "yes" ? 1 : 0));
                }
                assertions.Add(Assertion("circle-optional-inherited-" + row[0], "actual inherited list exposes only entitled variants",
                    "present=" + present, pass, "loaded prepared/spontaneous books; Wizard positive and Witch negative"));
            }
            foreach (var row in new[] {
                new[] { "Occultist", "e88366e9f64b44ac92d0f3a52074fb0a", "95d408f6c23d4ec2ad9049228b60cca6", "1b76f3c73aa84f91a1c65513fb23aa01" },
                new[] { "RelicHunter", "c32ade60841f47469575484576d6c0e0", "dac110582eff44159734a79314f13daa", "f1a70d9e1b0b41e49874e1fa9052a1ce" } }) {
                bool present = library.BlueprintsByAssetId.ContainsKey(row[1]); bool pass = true;
                if (present) {
                    var feature = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, row[1], row[0]);
                    var list = BlueprintLibraryLookup.RequireExact<Kingmaker.Blueprints.Classes.Spells.BlueprintSpellList>(library, row[2], row[0]);
                    pass = feature.ComponentsArray.Any(component => {
                        if (component.GetType().FullName != "CallOfTheWild.NewMechanics.addClassSpellChoice") return false;
                        var type = component.GetType();
                        return (int)type.GetField("spell_level").GetValue(component) == 3 &&
                            ReferenceEquals(type.GetField("spell_list").GetValue(component), list) &&
                            (type.GetField("character_class").GetValue(component) as BlueprintCharacterClass)?.AssetGuid == row[3];
                    }) && circles.All(circle => list.SpellsByLevel.Single(level => level.SpellLevel == 3).Spells.Count(spell => spell.AssetGuid == circle.Spell.AssetGuid) == 1);
                }
                assertions.Add(Assertion("circle-optional-implement-" + row[0], "Abjuration choices retain actual class/list/level ownership",
                    "present=" + present, pass, "installed foreign feature components and their actual selection list"));
            }
        }

        private static void CirclePaladinClassAccess(UnitEntityData anchor, UnitEntityData bearer,
            List<UnitEntityData> actors, List<BlueprintUnit> prototypes,
            List<RuntimeTestAssertion> assertions, List<string> diagnostics)
        {
            var circles = BlueprintBootstrap.MagicCircles;
            var game = Kingmaker.Game.Instance;
            var beforeAreas = game.State.AreaEffects.All.ToArray();
            var beforeItems = game.Player.Inventory.ToArray();
            var itemState = CircleSavedItems(game.Player.Inventory);
            var foreignBuffs = game.State.Units.All.ToDictionary(unit => unit, unit => unit.Buffs.Enumerable.ToArray());
            var paladinClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                "bfa11238e7ae3544bbeb4d0b92e897ec", "native Paladin");
            var restriction = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                "f8c91c0135d5fc3458fcc131c4b77e96", "native Paladin alignment restriction");
            var raceSource = Kingmaker.Game.Instance.Player.Party.First(value => value.Descriptor.Progression.Race != null);
            var paladin = CircleSpawn("PaladinSpellAccess", anchor.Position, anchor, actors, prototypes);
            LevelUpController controller = null;
            try {
                paladin.Stats.Charisma.BaseValue = 30;
                paladin.Descriptor.Alignment.Set(Alignment.LawfulGood);
                for (int level = 0; level < 11; level++) {
                    controller = LevelUpController.StartWithoutAssigningStaticInstance(paladin.Descriptor, false, null, null,
                        level == 0 ? LevelUpState.CharBuildMode.CharGen : LevelUpState.CharBuildMode.LevelUp);
                    if (level == 0) {
                        controller.SelectRace(raceSource.Descriptor.Progression.Race);
                        controller.SelectGender(raceSource.Descriptor.Gender);
                        controller.SelectAlignment(Alignment.LawfulGood);
                    }
                    if (!controller.SelectClass(paladinClass)) throw new InvalidOperationException("Native Paladin class selection failed.");
                    FillOracleNativeChoices(controller);
                    typeof(LevelUpController).GetMethod("ApplyLevelup", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(controller, new object[] { paladin.Descriptor });
                    controller.Cancel(); controller = null;
                    if (paladin.Descriptor.Progression.GetClassLevel(paladinClass) != level + 1)
                        throw new InvalidOperationException("Native Paladin progression did not advance exactly once.");
                }
                diagnostics.Add("native-paladin-progression:classLevel=" + paladin.Descriptor.Progression.GetClassLevel(paladinClass) +
                    ";books=" + string.Join(",", paladin.Descriptor.Spellbooks.Select(value => value.Blueprint.AssetGuid + ":" + value.CasterLevel)));
                var book = paladin.Descriptor.Spellbooks.Single(value => ReferenceEquals(value.Blueprint, paladinClass.Spellbook));
                book.UpdateAllSlotsSize(false); book.Rest();
                if (!paladin.IsPlayerFaction || !paladin.Descriptor.HasFact(restriction))
                    throw new InvalidOperationException("Native Paladin progression did not supply its player alignment restriction.");
                assertions.Add(Assertion("circle-paladin-class-entitlements", "native prepared book knows exactly Evil/Chaos, with its original owner and restriction",
                    "known=" + string.Join(",", circles.Where(c => book.IsKnown(c.Spell)).Select(c => c.Alignment)),
                    circles.All(c => book.IsKnown(c.Spell) == (c.Alignment == "Evil" || c.Alignment == "Chaos")) &&
                    ReferenceEquals(book.Owner, paladin.Descriptor), "actual native Paladin level progression and published list; no AddKnown bypass"));
                foreach (var circle in circles.Where(c => c.Alignment == "Evil" || c.Alignment == "Chaos")) {
                    var slot = RawSlots(book, 3).First(value => value.Type == SpellSlotType.Common && value.Spell == null);
                    if (!book.Memorize(new AbilityData(circle.Spell, book), slot)) throw new InvalidOperationException("Legal Paladin Circle cannot be prepared.");
                    book.Rest();
                    bool allowed = slot.Spell.IsAvailable && !(paladin.Get<UnitPartForbiddenSpellbooks>()?.IsForbidden(book.Blueprint) ?? false);
                    paladin.Descriptor.Alignment.Set(Alignment.ChaoticEvil);
                    bool forbidden = !slot.Spell.IsAvailable && paladin.Get<UnitPartForbiddenSpellbooks>()?.IsForbidden(book.Blueprint) == true;
                    paladin.Descriptor.Alignment.Set(Alignment.LawfulGood);
                    bool restored = slot.Spell.IsAvailable && !(paladin.Get<UnitPartForbiddenSpellbooks>()?.IsForbidden(book.Blueprint) ?? false);
                    assertions.Add(Assertion("circle-paladin-alignment-gate-" + circle.Alignment, "native lawful-good book availability disables on deviation and restores on return",
                        "allowed=" + allowed + ";forbidden=" + forbidden + ";restored=" + restored, allowed && forbidden && restored,
                        "native ForbidSpellbookOnAlignmentDeviation; no new alignment policy or forced resource result"));
                    CircleCast(paladin, bearer, slot.Spell, diagnostics);
                    var carrier = CircleBuffs(bearer, circle.Carrier).Single(); var area = CircleArea(carrier);
                    CircleRefresh(area, actors);
                    assertions.Add(Assertion("circle-paladin-prepared-cast-" + circle.Alignment, "one legitimate level-three Paladin preparation creates the native bearer circle",
                        "cl=" + carrier.Context.Params.CasterLevel, !slot.Available && ReferenceEquals(carrier.Context.MaybeCaster, paladin) &&
                        carrier.Context.Params.CasterLevel == book.CasterLevel, "native Memorize/Rest/UnitUseAbility and original Paladin book"));
                    carrier.Remove(); CircleRefresh(area, actors); book.ForgetMemorized(slot);
                }
            }
            finally {
                controller?.Cancel();
                foreach (var circle in circles)
                    foreach (var carrier in CircleBuffs(bearer, circle.Carrier).Where(value => ReferenceEquals(value.Context.MaybeCaster, paladin))) carrier.Remove();
                // Full native progression grants its own auras. They are exact
                // sources owned by this disposable Paladin, not foreign areas.
                foreach (var area in game.State.AreaEffects.All.Except(beforeAreas).ToArray()) {
                    if (!ReferenceEquals(area.Context?.MaybeCaster, paladin) && !ReferenceEquals(area.Context?.MaybeOwner, paladin.Descriptor))
                        throw new InvalidOperationException("Unexpected area source during isolated Paladin progression: " + area.Blueprint.AssetGuid);
                    diagnostics.Add("native-paladin-owned-area-cleanup:" + area.Blueprint.AssetGuid + ";source=" + paladin.UniqueId);
                    area.ForceEnd(); area.Tick();
                }
                var pet = paladin.Descriptor.Pet;
                if (pet != null) {
                    if (!ReferenceEquals(pet.Descriptor.Master.Value, paladin) || foreignBuffs.ContainsKey(pet))
                        throw new InvalidOperationException("Paladin fixture companion ownership is ambiguous.");
                    pet.Descriptor.SetMaster(null); pet.Destroy();
                }
                paladin.Destroy(); game.EntityDestroyer.Tick(); game.EntityDestroyer.Tick(); actors.Remove(paladin);
                // Spawn and level-up are the only item-producing operations in
                // this paused request-local scope. Preserve original instances.
                foreach (var item in game.Player.Inventory.Except(beforeItems).ToArray()) game.Player.Inventory.Remove(item);
                assertions.Add(Assertion("circle-paladin-fixture-cleanup", "exact original areas, recipient facts and inventory restored", "areas=" + game.State.AreaEffects.All.Count(),
                    game.State.AreaEffects.All.SequenceEqual(beforeAreas) && foreignBuffs.All(pair => pair.Key.Buffs.Enumerable.SequenceEqual(pair.Value)) &&
                    game.Player.Inventory.SequenceEqual(beforeItems) && CircleSavedItems(game.Player.Inventory).SequenceEqual(itemState),
                    "only newly created source-owned Paladin areas, companion and starter items removed; no point dispel crosses a foreign source"));
            }
        }
    }
}
