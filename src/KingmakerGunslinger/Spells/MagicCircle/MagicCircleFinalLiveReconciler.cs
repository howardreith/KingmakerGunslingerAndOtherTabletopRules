using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityModManagerNet;

namespace KingmakerGunslinger.Spells.MagicCircle
{
    internal static class MagicCircleFinalLiveReconciler
    {
        private static ModContext _context;
        private static bool _attached;
        internal static void AttachFirstUpdate(ModContext context)
        {
            if (_attached) return;
            _context = context; _attached = true;
            context.ModEntry.OnUpdate += FirstUpdate;
        }
        private static void FirstUpdate(UnityModManager.ModEntry entry, float delta)
        {
            _context.ModEntry.OnUpdate -= FirstUpdate; _attached = false;
            try {
                var library = BlueprintBootstrap.Library;
                var circles = BlueprintBootstrap.MagicCircles;
                if (library == null || circles == null) return;
                var owned = circles.SelectMany(circle => new[] { circle.Spell, circle.Delivery }).ToArray();
                var foreign = library.BlueprintsByAssetId.Values.OfType<BlueprintAbility>().Where(ability =>
                    !owned.Contains(ability) && IsCircle(ability)).ToArray();
                if (foreign.Length != 0) {
                    BlueprintBootstrap.DisableMagicCirclePublication();
                    _context.Logger.Failure("magic-circle", "duplicate.final-live", "Foreign Magic Circle content retained; new Magic Circle publication disabled: " +
                        string.Join(",", foreign.Select(ability => ability.AssetGuid + ":" + ability.name)), null);
                    return;
                }
                if (BlueprintBootstrap.MagicCirclePublication == null) return;
                // Optional mods can materialize native filtered lists after our
                // bootstrap. Reconcile through the same ownership transaction.
                BlueprintBootstrap.MagicCirclePublication.ReconcileNative(library, circles);
                PublishOptional(library);
                BlueprintBootstrap.MagicCirclePublication.ReconcileNative(library, circles);
                PublishOptional(library);
            }
            catch (Exception exception) {
                BlueprintBootstrap.DisableMagicCirclePublication();
                _context.Logger.Failure("magic-circle", "optional-list.failed", "Optional Magic Circle reconciliation refused; unrelated content preserved.", exception);
            }
        }

        private static bool IsCircle(BlueprintAbility ability)
        {
            string text = ((ability.name ?? string.Empty) + " " + ability.Name).ToLowerInvariant().Replace(" ", string.Empty).Replace("_", string.Empty);
            return text.Contains("magiccircleagainst") || text.Contains("magiccirclefrom");
        }

        // Verified installed CotW identities and actual primary book/list links.
        // Absence is ordinary. A present incompatible class is never matched by
        // a display name or granted spells through a different archetype book.
        private static readonly string[][] Optional = {
            new[] { "Oracle", "32c02466b2364c8a906e6e4761175099", "3587fa91b34341e49b3a22cfb5450e0d", "f305174b73f64783a8379238a14c3283" },
            new[] { "Warpriest", "e119d84528144a7797ad34fd718b1f87", "9995149d6ff043868cb1fd22ae6ac332", "9ef48172d50446aca4c80f321402f743" },
            new[] { "Summoner", "0f4c4ada51334b43a802350c5c0b85f5", "20f4a4b890204302ae9895fdc45bb20c", "972048af37924e59b174653974b255a5" },
            new[] { "Shaman", "6b1d00511e824f0fbc27ec8f54b8edb2", "20fbd5cd3f79455aa9f133ecf21797ab", "7113337f695742559ecdecc8905b132a" },
            new[] { "Spiritualist", "2e31173d30f043aabd910f7a418594ec", "249100aac10349e3b9a8196ae313990f", "71d7e14b9f674272b40ed2d9093f34d5" },
            new[] { "Occultist", "1b76f3c73aa84f91a1c65513fb23aa01", "465a0ae7aa10419cb990a03e4859dca5", "a6ece1eaa2de452987843dff8f7ca01b" },
            new[] { "Antipaladin", "db03b55aa414444cba0b42f3c429fe2d", "564482126cac4778b94d0e7079fe41e9", "dbd2049eee48480abec0ffd7abceae7f" }
        };

        private static void PublishOptional(LibraryScriptableObject library)
        {
            var publication = BlueprintBootstrap.MagicCirclePublication;
            foreach (var spec in Optional) {
                if (!library.BlueprintsByAssetId.ContainsKey(spec[1])) continue;
                var characterClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, spec[1], "optional " + spec[0]);
                var book = BlueprintLibraryLookup.RequireExact<BlueprintSpellbook>(library, spec[2], "optional " + spec[0] + " primary book");
                var list = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(library, spec[3], "optional " + spec[0] + " actual list");
                if (!ReferenceEquals(characterClass.Spellbook, book) || !ReferenceEquals(book.CharacterClass, characterClass) ||
                    !ReferenceEquals(book.SpellList, list) || list.MaxLevel < 3)
                    throw new InvalidOperationException("Optional primary book/list contract changed: " + spec[0]);
                foreach (var circle in BlueprintBootstrap.MagicCircles)
                    if (spec[0] != "Antipaladin" || circle.Alignment == "Good" || circle.Alignment == "Law") publication.Add(list, circle.Spell);
                _context.Logger.Info("magic-circle", "optional-list", "class=" + characterClass.AssetGuid + ";book=" + book.AssetGuid + ";list=" + list.AssetGuid + ";level=3");
            }
            PublishImplement(library, "e88366e9f64b44ac92d0f3a52074fb0a", "95d408f6c23d4ec2ad9049228b60cca6", "1b76f3c73aa84f91a1c65513fb23aa01");
            PublishImplement(library, "c32ade60841f47469575484576d6c0e0", "dac110582eff44159734a79314f13daa", "f1a70d9e1b0b41e49874e1fa9052a1ce");
            PublishFavoredOracle(library);
            // Arcanist's casting and memorization books already use the actual
            // Wizard list; Unlettered Arcanist's Witch list receives no grant.
        }
        private static void PublishFavoredOracle(LibraryScriptableObject library)
        {
            const string featureId = "bab7a67de47e4b6690c03fd5b744c482";
            if (!library.BlueprintsByAssetId.ContainsKey(featureId)) return;
            var feature = BlueprintLibraryLookup.RequireExact<BlueprintParametrizedFeature>(library, featureId, "Oracle third-level Favored Class choice");
            var oracle = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, Optional[0][1], "Favored Oracle class");
            var book = BlueprintLibraryLookup.RequireExact<BlueprintSpellbook>(library, Optional[0][2], "Favored Oracle book");
            var list = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(library, Optional[0][3], "Favored Oracle list");
            var learning = feature.ComponentsArray.OfType<LearnSpellParametrized>().SingleOrDefault();
            var prerequisite = feature.ComponentsArray.OfType<PrerequisiteClassSpellLevel>().SingleOrDefault();
            if (oracle.Spellbook != book || book.CharacterClass != oracle || book.SpellList != list ||
                feature.name != "FavoredOracleOracleSpellList3ParametrizedFeature" || feature.ParameterType != FeatureParameterType.LearnSpell ||
                feature.SpellcasterClass != oracle || feature.SpellList != list || !feature.SpecificSpellLevel ||
                feature.SpellLevel != 3 || feature.SpellLevelPenalty != 0 || feature.DisallowSpellsInSpellList ||
                learning == null || learning.SpellcasterClass != oracle || learning.SpellList != list ||
                !learning.SpecificSpellLevel || learning.SpellLevel != 3 || learning.SpellLevelPenalty != 0 ||
                prerequisite == null || prerequisite.CharacterClass != oracle || prerequisite.RequiredSpellLevel != 4)
                throw new InvalidOperationException("Oracle Favored Class third-level selection contract changed: " + featureId);
            foreach (var circle in BlueprintBootstrap.MagicCircles)
                BlueprintBootstrap.MagicCirclePublication.AddFavoredParameter(feature, circle.Spell);
            _context.Logger.Info("magic-circle", "favored-oracle", "feature=" + featureId + ";level=3;parameters=4;native-prerequisites-preserved");
        }

        private static void PublishImplement(LibraryScriptableObject library, string featureId, string listId, string classId)
        {
            if (!library.BlueprintsByAssetId.ContainsKey(featureId)) return;
            var feature = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, featureId, "optional Abjuration implement");
            var list = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(library, listId, "optional Abjuration implement choices");
            var choices = feature.ComponentsArray.Where(component => component.GetType().FullName == "CallOfTheWild.NewMechanics.addClassSpellChoice").ToArray();
            bool linked = choices.Any(component => {
                var type = component.GetType();
                var level = type.GetField("spell_level", BindingFlags.Instance | BindingFlags.Public);
                var spells = type.GetField("spell_list", BindingFlags.Instance | BindingFlags.Public);
                var owner = type.GetField("character_class", BindingFlags.Instance | BindingFlags.Public);
                return level != null && spells != null && owner != null && (int)level.GetValue(component) == 3 &&
                    ReferenceEquals(spells.GetValue(component), list) && (owner.GetValue(component) as BlueprintCharacterClass)?.AssetGuid == classId;
            });
            if (!linked) throw new InvalidOperationException("Optional implement list ownership changed: " + featureId);
            foreach (var circle in BlueprintBootstrap.MagicCircles) BlueprintBootstrap.MagicCirclePublication.Add(list, circle.Spell);
        }
    }
}
