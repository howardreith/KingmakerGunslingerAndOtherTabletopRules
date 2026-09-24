using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>O01 Oread Bard selected performance range.</summary>
    internal static class FavoredClassPerformanceTests
    {
        private static readonly Regex Guid = new Regex("^[0-9a-f]{32}$");

        private static readonly string[] PublishedKeys =
        {
            "InspireCourage", "InspireCompetence", "Fascinate", "DirgeOfDoom", "InspireGreatness",
            "FrighteningTune", "InspireHeroics", "InciteRage", "FireDance", "SongOfFieryGaze", "Satire",
            "GloriousEpic", "Scandal", "DanceOfTheDead"
        };

        private static string Source(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "FavoredClass" }.Concat(parts).ToArray()));
        }

        // Classified by mechanics: maintained persistent-area performances are
        // published; two registered counters without a truthful display are
        // never published; one-shot, personal, masterpiece and add-on entries
        // are not targets.
        internal static void ManifestIsTheAuditedAreaSet()
        {
            IList<FavoredClassPerformanceTarget> all = FavoredClassPerformanceManifest.All;
            Assertions.Equal(16, all.Count, "Registered performance targets.");
            Assertions.Equal(16, all.Select(target => target.Key).Distinct(StringComparer.Ordinal).Count(),
                "Unique keys.");
            Assertions.True(all.Where(target => target.Published).Select(target => target.Key)
                .SequenceEqual(PublishedKeys), "The published targets, in registration order.");
            Assertions.True(all.Where(target => !target.Published).Select(target => target.Key)
                .SequenceEqual(new[] { "StormCall", "Mockery" }), "Two registered counters are never published.");
            foreach (FavoredClassPerformanceTarget target in all)
            {
                Assertions.True(Guid.IsMatch(target.FeatureGuid) && target.ToggleGuids.Length > 0 &&
                    target.ToggleGuids.All(Guid.IsMatch) && target.AreaGuids.Length == target.ToggleGuids.Length &&
                    target.AreaGuids.All(Guid.IsMatch), target.Key + " has exact feature, toggle and area identities.");
                Assertions.True(target.BaseFeet == 30 || target.BaseFeet == 50, target.Key + " native radius.");
                Assertions.True(Guid.IsMatch(target.RingAssetId) && target.RingSpawns == (target.Key != "Scandal"),
                    target.Key + " names its exact ring link (Scandal's provider link spawns no ring).");
                foreach (string area in target.AreaGuids)
                    Assertions.Equal(target.Published ? target.Key : null, FavoredClassPerformanceManifest.KeyForArea(area),
                        target.Key + " owns its area only when published.");
                foreach (string fact in target.ToggleGuids.Concat(new[] { target.FeatureGuid }))
                    Assertions.Equal(target.Published ? target.Key : null, FavoredClassPerformanceManifest.KeyForFact(fact),
                        target.Key + " owns its displayed facts only when published.");
                Assertions.Equal(!target.Published, !string.IsNullOrEmpty(target.Exclusion),
                    target.Key + " records why it is excluded.");
            }
            Assertions.Equal(18, all.SelectMany(target => target.AreaGuids).Distinct(StringComparer.Ordinal).Count(),
                "Eighteen distinct areas (Incite Rage has three).");
            Assertions.Equal(3, FavoredClassPerformanceManifest.For("InciteRage").AreaGuids.Length,
                "Incite Rage: enemies, allies and all.");
            Assertions.Equal(6, all.Count(target => target.Provider), "Six Call of the Wild performances.");
            Assertions.True(FavoredClassPerformanceManifest.For("StormCall").Exclusion.Contains("50 feet") &&
                FavoredClassPerformanceManifest.For("Mockery").Exclusion.Contains("single selected target"),
                "Storm Call's text/area mismatch and Mockery's single target are the recorded reasons.");
            Assertions.True(FavoredClassPerformanceManifest.KeyForArea("0000000000000000000000000000000f") == null &&
                FavoredClassPerformanceManifest.KeyForArea(null) == null &&
                FavoredClassPerformanceManifest.KeyForFact(null) == null, "Other areas and facts belong to no target.");
            IList<FavoredClassPerformanceNonTarget> non = FavoredClassPerformanceManifest.NonTargets;
            Assertions.Equal(14, non.Count, "Fourteen classified non-targets.");
            Assertions.True(non.All(value => Guid.IsMatch(value.FeatureGuid) && !string.IsNullOrEmpty(value.Reason) &&
                    !all.Any(target => target.FeatureGuid == value.FeatureGuid)),
                "Every non-target has an exact identity, a reason and no counter.");
            foreach (string kind in new[] { "instantaneous", "personal", "masterpiece", "inert" })
                Assertions.True(non.Any(value => value.Reason.StartsWith(kind, StringComparison.Ordinal)),
                    "Non-target class: " + kind);
            Assertions.Equal("772c83a25e2268e448e841dcd548235f", FavoredClassPerformanceManifest.BardClassGuid,
                "Kingmaker Bard.");
        }

        // Sixteen full-only leaves (divisor 1), six ranks each (+30 feet).
        internal static void PerformanceLeavesAreCappedCounters()
        {
            string effect = FavoredClassCatalog.EffectPerformanceRange;
            Assertions.True(FavoredClassLeafCatalog.IsImplemented(effect), "O01 is implemented.");
            IList<FavoredClassLeafSpec> leaves = FavoredClassLeafCatalog.LeavesFor(effect);
            Assertions.Equal(16, leaves.Count, "One leaf per registered performance.");
            foreach (FavoredClassLeafSpec leaf in leaves)
            {
                Assertions.Equal(FavoredClassInvestmentRole.Full, leaf.Role, leaf.Symbol + " is a full leaf.");
                Assertions.Equal(6, leaf.Ranks, leaf.Symbol + " stops at +30 feet.");
                Assertions.Equal("KMG.FavoredClass.Bard.PerformanceRange." + leaf.TargetKey + ".Full", leaf.Symbol,
                    "Performance symbol.");
                Assertions.Equal("BlueprintFeature", FavoredClassIdentityCatalog.ForSymbol(leaf.Symbol).PlannedType,
                    leaf.Symbol + " has a committed identity.");
                FavoredClassPerformanceTarget target = FavoredClassPerformanceManifest.For(leaf.TargetKey);
                Assertions.True(leaf.Description.Contains("Oread") &&
                    leaf.Description.Contains("+5 feet to the radius of " + target.Title + " (" + target.BaseFeet +
                        " feet natively)") &&
                    leaf.Description.Contains("Each performance keeps its own separate count") &&
                    leaf.Description.Contains("limited to 6 steps") &&
                    leaf.Description.Contains("that area's ring and your performance's description show your range"),
                    leaf.Symbol + " discloses its step, native radius, counter, cap and presentation.");
                Assertions.False(leaf.Description.Contains("keeps its standard size"),
                    leaf.Symbol + " never promises a range behind an unchanged ring.");
                Assertions.Equal(target.Published && target.Provider, leaf.Description.Contains("provided by Call of the Wild"),
                    leaf.Symbol + " names its provider exactly when it is published from one.");
                Assertions.Equal(!target.Published, leaf.Description.Contains("Not offered: " + target.Exclusion),
                    leaf.Symbol + " states why it is never offered.");
            }
            Assertions.True(FavoredClassLeafCatalog.TargetRows(effect, "InspireCourage").SequenceEqual(new[] { "O01" }),
                "Only the Oread row opens performance range.");
        }

        // Five feet per step in Kingmaker's own feet-to-meters ratio, capped at six steps.
        internal static void RadiusFollowsTheEarnedSteps()
        {
            float courage = 50 * FavoredClassMechanicsPolicy.FeetToMeters;
            Assertions.True(Math.Abs(FavoredClassMechanicsPolicy.PerformanceRadiusMeters(courage, 0) - courage) < 1e-4f,
                "No steps, native radius.");
            Assertions.True(Math.Abs(FavoredClassMechanicsPolicy.PerformanceRadiusMeters(courage, 2) -
                60 * FavoredClassMechanicsPolicy.FeetToMeters) < 1e-4f, "Two steps: 60 feet.");
            Assertions.True(Math.Abs(FavoredClassMechanicsPolicy.PerformanceRadiusMeters(30 *
                FavoredClassMechanicsPolicy.FeetToMeters, 6) - 60 * FavoredClassMechanicsPolicy.FeetToMeters) < 1e-4f,
                "Six steps: +30 feet.");
            Assertions.True(Math.Abs(FavoredClassMechanicsPolicy.PerformanceRadiusMeters(courage, -3) - courage) < 1e-4f,
                "Negative steps never shrink the area.");
            FavoredClassPerformanceTarget competence = FavoredClassPerformanceManifest.For("InspireCompetence");
            Assertions.Equal(35, FavoredClassPerformanceManifest.OwnerFeet(competence, 1), "One step: 35 feet.");
            Assertions.Equal(60, FavoredClassPerformanceManifest.OwnerFeet(competence, 6), "Six steps: 60 feet.");
            Assertions.Equal(60, FavoredClassPerformanceManifest.OwnerFeet(competence, 9), "The cap holds.");
            Assertions.Equal(30, FavoredClassPerformanceManifest.OwnerFeet(competence, -1), "No negative range.");
        }

        // The owner's description states the owner's range; nothing else changes.
        internal static void OwnerTextStatesTheOwnersRange()
        {
            Assertions.Equal("They must be within 35 feet and able to see and hear the bard.",
                FavoredClassPerformanceText.OwnerDescription(
                    "They must be within 30 feet and able to see and hear the bard.", 30, 35), "One statement.");
            Assertions.Equal("An enemy must be within 60 feet. It persists while the enemy is within 60 feet.",
                FavoredClassPerformanceText.OwnerDescription(
                    "An enemy must be within 30 feet. It persists while the enemy is within 30 feet.", 30, 60),
                "Every statement.");
            Assertions.Equal("A 55-foot aura.", FavoredClassPerformanceText.OwnerDescription("A 50-foot aura.", 50, 55),
                "Hyphenated form.");
            Assertions.Equal("Allies gain a bonus. Range: 60 feet.",
                FavoredClassPerformanceText.OwnerDescription("Allies gain a bonus. ", 50, 60),
                "A description without a range gains one.");
            Assertions.Equal("Within 130 feet. Range: 35 feet.",
                FavoredClassPerformanceText.OwnerDescription("Within 130 feet.", 30, 35),
                "Another number is never rewritten.");
            Assertions.Equal("Within 30 feet.", FavoredClassPerformanceText.OwnerDescription("Within 30 feet.", 30, 30),
                "An uninvested owner keeps the native text.");
            Assertions.True(FavoredClassPerformanceText.OwnerDescription(null, 30, 35) == null, "No text, no text.");
        }

        // Hooks: the owner's own instance, ring and facts only; exact restoration.
        internal static void RangeHookIsCasterAndInstanceScoped()
        {
            string hook = Source("Hooks", "FavoredClassPerformanceRangePatch.cs");
            foreach (string token in new[]
            {
                "[HarmonyPatch(typeof(AreaEffectView), \"InitAtRuntime\")]",
                "blueprint.Shape != AreaEffectShape.Cylinder",
                "FavoredClassPerformanceManifest.KeyForArea(blueprint.AssetGuid)",
                "context.MaybeCaster",
                "FavoredClassEarnedSteps.For(caster.Descriptor, FavoredClassCatalog.EffectPerformanceRange,",
                "catch (Exception)",
                "__instance.Shape as ScriptZoneCylinder",
                "native = blueprint.Size.Meters;",
                "FavoredClassMechanicsPolicy.PerformanceRadiusMeters(native, steps)",
                "FavoredClassPerformanceRing.Scale(ring, widened / native);",
                "[HarmonyPatch(typeof(AreaEffectView), \"SpawnFxs\")]",
                "FavoredClassPerformanceRangePatch.ScaleLateRing(__instance);",
                "FavoredClassPerformanceRing.IsScaled(ring)",
                "Math.Abs(cylinder.Radius - widened) < 0.0001f",
                "[HarmonyPatch(typeof(Kingmaker.Visual.Particles.GameObjectsPool), \"Release\")]",
                "FavoredClassPerformanceRing.Restore(instance);"
            })
                Assertions.True(hook.Contains(token), "Range hook: " + token);
            Assertions.False(hook.Contains("blueprint.Size =") || hook.Contains(".Fx ="),
                "The shared area blueprint and its effect link are never changed.");
            string ring = Source("Mechanics", "FavoredClassPerformanceRing.cs");
            foreach (string token in new[]
            {
                "mode != LocalScalingMode && HasScaledAncestor(transform, effect.transform, scaledTransforms)",
                "record.Scales.Add(new KeyValuePair<Transform, Vector3>(transform, before));",
                "Math.Abs((transform.rotation * axis).y) < 0.7071f ? factor : 1f",
                "throw new InvalidOperationException(\"A performance ring was scaled twice.\");",
                "transform.localScale = record.Scales[index].Value;"
            })
                Assertions.True(ring.Contains(token), "Ring: " + token);
            string text = Source("Hooks", "FavoredClassPerformanceTextPatch.cs");
            foreach (string token in new[]
            {
                "[HarmonyPatch(typeof(Fact), \"SelectUIData\")]",
                "[HarmonyPatch(typeof(MechanicActionBarSlotActivableAbility), \"GetDescription\")]",
                "FavoredClassPerformanceManifest.KeyForFact(blueprintGuid)",
                "FavoredClassEarnedSteps.For(owner, FavoredClassCatalog.EffectPerformanceRange, key)",
                "type != UIDataType.Description"
            })
                Assertions.True(text.Contains(token), "Owner text: " + token);
            string publication = Source("FavoredClassPublication.cs");
            Assertions.True(publication.Contains("!FavoredClassPerformanceManifest.For(pair.TargetKey).Published") &&
                publication.Contains("return \"excluded-target:\" + pair.TargetKey;"),
                "Excluded targets are never published.");
            string blueprints = Source("FavoredClassBlueprints.cs");
            Assertions.True(blueprints.Contains("{ FavoredClassCatalog.Bard, FavoredClassPerformanceManifest.BardClassGuid }") &&
                blueprints.Contains("owned.Title = \"the performance \" + performance.Title;") &&
                blueprints.Contains("case FavoredClassCatalog.EffectPerformanceRange:"),
                "Performance leaves require the owned performance and publish into the Bard selection.");
        }
    }
}
