using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Bounded caster dependencies for the selected printed preparations and
        // Astral SLAs. Metadata only; native behavior still needs its own proof.
        private void CaptureCelestial31CasterInputs()
        {
            var native = BlueprintLibraryLookup.RequireExact<BlueprintSpellbook>(BlueprintBootstrap.Library,
                "4673d19a0cf2fab4f885cc4d1353da33", "canonical cleric preparation source");
            string[] wanted = { "aid", "banishment", "bladebarrier", "bless", "bullsstrength", "curelightwounds",
                "curemoderatewounds", "cureseriouswounds", "daylight", "detectmagic", "dispelevil", "dispelmagic",
                "dismissal", "divinefavor", "divinepower", "heal", "holyaura", "holysmite", "holyword",
                "magicvestment", "magicalvestment", "neutralizepoison", "owlswisdom", "protectionfromenergy", "protectionenergy",
                "removecurse", "removedisease", "removefear", "restorationlesser", "lesserrestoration", "sanctuary",
                "shieldoffaith", "stabilize", "virtue", "invisibilitypurge", "spellimmunity",
                "deathward", "resistance", "freedomofmovement" };
            Func<string, string> normalize = name => new string((name ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
            var rows = new JArray();
            foreach (var level in native.SpellList.SpellsByLevel.Where(value => value.SpellLevel >= 0 && value.SpellLevel <= 7))
            foreach (var spell in level.Spells.Where(value => value != null && wanted.Any(token => normalize(value.name).Contains(token))))
                rows.Add(new JObject { ["nativeClericLevel"] = level.SpellLevel, ["name"] = spell.name,
                    ["guid"] = spell.AssetGuid, ["type"] = spell.Type.ToString(), ["range"] = spell.Range.ToString(),
                    ["action"] = spell.ActionType.ToString(), ["spellResistance"] = spell.SpellResistance,
                    ["canTargetSelf"] = spell.CanTargetSelf, ["canTargetFriends"] = spell.CanTargetFriends,
                    ["canTargetEnemies"] = spell.CanTargetEnemies,
                    ["graph"] = DescribeGraph(spell, 0, new HashSet<object>(NativeDonorReferenceComparer.Instance), 5) });
            var all = BlueprintBootstrap.Library.GetAllBlueprints().Where(value => value != null).ToArray();
            var extras = new JArray(all.OfType<BlueprintAbility>().Where(value =>
                new[] { "invisibility", "invisibilitycast", "seeinvisibility", "seeinvisibilitycast", "dispelevil", "daylight", "detectmagic", "stabilize", "magicalvestment", "magicalvestmentarmor", "magicalvestmentshield" }.Contains(normalize(value.name)))
                .Select(spell => new JObject { ["name"] = spell.name, ["guid"] = spell.AssetGuid,
                    ["type"] = spell.Type.ToString(), ["range"] = spell.Range.ToString(),
                    ["graph"] = DescribeGraph(spell, 0, new HashSet<object>(NativeDonorReferenceComparer.Instance), 5) }));
            var feats = new JArray(all.OfType<BlueprintFeature>().Where(value => value.name == "BlindFight" || value.name == "Persuasive")
                .Select(value => new JObject { ["name"] = value.name, ["guid"] = value.AssetGuid,
                    ["graph"] = DescribeGraph(value, 0, new HashSet<object>(NativeDonorReferenceComparer.Instance), 5) }));
            // One bounded audit for the unresolved trumpet exemption consumer.
            // Do not infer subtype from alignment or a friendly faction. Retain
            // exact native/mod-provided named identities and their actual facts.
            var archonUnits = all.OfType<BlueprintUnit>().Where(value =>
                value.name.IndexOf("archon", StringComparison.OrdinalIgnoreCase) >= 0 &&
                !value.name.StartsWith("KMG_", StringComparison.Ordinal)).Take(32).ToArray();
            var archonFacts = all.OfType<BlueprintFeature>().Where(value =>
                value.name.IndexOf("archon", StringComparison.OrdinalIgnoreCase) >= 0 &&
                !value.name.StartsWith("KMG_", StringComparison.Ordinal)).Take(32).ToArray();
            var archons = new JObject {
                ["scope"] = "Named native/installed archon identities and subtype candidates only; metadata, not an exemption verdict",
                ["units"] = new JArray(archonUnits.Select(value => new JObject {
                    ["name"] = value.name, ["guid"] = value.AssetGuid,
                    ["facts"] = new JArray((value.AddFacts ?? new Kingmaker.Blueprints.Facts.BlueprintUnitFact[0])
                        .Where(fact => fact != null).Select(fact => new JObject {
                            ["name"] = fact.name, ["guid"] = fact.AssetGuid })) })),
                ["facts"] = new JArray(archonFacts.Select(value => new JObject {
                    ["name"] = value.name, ["guid"] = value.AssetGuid,
                    ["graph"] = DescribeGraph(value, 0, new HashSet<object>(NativeDonorReferenceComparer.Instance), 3) })) };
            var document = new JObject { ["runId"] = _request.RunId, ["scenario"] = _request.Scenario,
                ["mvid"] = typeof(RuntimeTestRunner).Assembly.ManifestModule.ModuleVersionId.ToString(),
                ["scope"] = "Selected S30/S31 caster dependency metadata; not behavior/qualification; no donor mutation",
                ["archonExemptionInputs"] = archons,
                ["clericBook"] = native.AssetGuid, ["selectedNativeSpells"] = rows, ["astralInvisibility"] = extras, ["remainingFeats"] = feats };
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, "celestial31-caster-inputs.json"), document.ToString());
        }
    }
}
