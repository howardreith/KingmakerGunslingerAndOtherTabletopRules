using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>I06/S04 selected oracle revelation adapters.</summary>
    internal static class FavoredClassRevelationTests
    {
        private static readonly Regex Guid = new Regex("^[0-9a-f]{32}$");
        private static readonly Regex RankRead = new Regex("^[0-9a-f]{32}\\|[A-Za-z]+$");

        private static string Source(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "FavoredClass" }.Concat(parts).ToArray()));
        }

        // The generated manifest is the audit's implementable set: 52 targets,
        // Dragon revelations one feature per colour; every published target
        // implements its level gates, and only the possession BAB target is
        // excluded (review finding 2, charter 8.10).
        internal static void ManifestIsTheAuditedImplementableSet()
        {
            IList<FavoredClassRevelationTarget> all = FavoredClassRevelationManifest.All;
            Assertions.Equal(52, all.Count, "Audited implementable revelations.");
            Assertions.Equal(52, all.Select(target => target.Key).Distinct(StringComparer.Ordinal).Count(),
                "Unique target keys.");
            Assertions.Equal(1, all.Count(target => !target.Published),
                "Only one target is excluded rather than published partially.");
            FavoredClassRevelationTarget warrior = FavoredClassRevelationManifest.For("SpiritOfTheWarrior");
            Assertions.True(!warrior.Published && warrior.ExcludedReason.Contains("base attack bonus"),
                "Spirit of the Warrior is excluded: its possession sets BAB, which a counter must never raise.");
            foreach (FavoredClassRevelationTarget target in all)
            {
                Assertions.True(Regex.IsMatch(target.Key, "^[A-Z][A-Za-z]+$"), target.Key + " is a symbol segment.");
                Assertions.True(target.FeatureGuids.Length > 0 && target.FeatureGuids.All(Guid.IsMatch),
                    target.Key + " lists exact revelation identities.");
                Assertions.True(target.Families.Length > 0 && target.Families.Trim().Length > 0 &&
                    (target.HasFamily('A') || target.HasFamily('B') || target.HasFamily('C')),
                    target.Key + " has an audited adapter family.");
                Assertions.True(target.ExtraRoots.All(Guid.IsMatch) && target.ExcludedRanks.All(RankRead.IsMatch),
                    target.Key + " scope overrides are exact.");
            }
            Assertions.Equal(10, FavoredClassRevelationManifest.For("BreathWeapon").FeatureGuids.Length,
                "Breath Weapon: one feature per dragon colour.");
            Assertions.Equal(10, FavoredClassRevelationManifest.For("FormOfTheDragon").FeatureGuids.Length,
                "Form of the Dragon: one feature per dragon colour.");
            Assertions.Equal(22, FavoredClassRevelationManifest.For("Channel").ExtraRoots.Length,
                "The Life channel's derived channels execute its own channel.");
            Assertions.True(all.Where(target => target.Key != "Channel").All(target => target.ExtraRoots.Length == 0),
                "Only the channel has further roots.");
            Assertions.True(FavoredClassRevelationManifest.For("SpiritOfTheWarrior").ExcludedRanks.Length == 1 &&
                all.Count(target => target.ExcludedRanks.Length > 0) == 1,
                "Only the possession BAB read is excluded (the charter never raises BAB).");
            Assertions.False(all.Any(target => target.Key == "Fortune" || target.Key == "Misfortune"),
                "Dual-Cursed Fortune is an owner decision, not a published target.");
            Assertions.Equal("32c02466b2364c8a906e6e4761175099", FavoredClassRevelationManifest.OracleClassGuid,
                "Call of the Wild Oracle identity.");
        }

        // 104 committed leaves at 1/6 uncapped (full 3, partial 17), one counter per revelation.
        internal static void RevelationLeavesAreCommittedCounters()
        {
            string effect = FavoredClassCatalog.EffectSelectedRevelation;
            Assertions.True(FavoredClassLeafCatalog.IsImplemented(effect), "Revelations are implemented.");
            IList<string> keys = FavoredClassLeafCatalog.TargetKeys(effect);
            Assertions.True(keys.SequenceEqual(FavoredClassRevelationManifest.All.Select(target => target.Key)),
                "Targets follow the manifest order.");
            IList<FavoredClassLeafSpec> leaves = FavoredClassLeafCatalog.LeavesFor(effect);
            Assertions.Equal(104, leaves.Count, "Revelation leaves.");
            foreach (FavoredClassLeafSpec leaf in leaves)
            {
                bool full = leaf.Role == FavoredClassInvestmentRole.Full;
                Assertions.Equal("KMG.FavoredClass.Oracle.Revelation." + leaf.TargetKey + (full ? ".Full" : ".Partial"),
                    leaf.Symbol, "Revelation symbol.");
                Assertions.Equal(full ? 3 : 17, leaf.Ranks, leaf.Symbol + " capacity.");
                Assertions.Equal("BlueprintFeature", FavoredClassIdentityCatalog.ForSymbol(leaf.Symbol).PlannedType,
                    leaf.Symbol + " has a committed identity.");
                Assertions.True(leaf.Description.Contains("Each revelation keeps its own separate count") &&
                    leaf.Description.Contains("Ifrit") && leaf.Description.Contains("Sylph") &&
                    leaf.Description.Contains("so do the abilities and forms the revelation itself gains at later oracle levels") &&
                    leaf.Description.Contains("It never grants another revelation or revelation choice") &&
                    !leaf.Description.Contains("still gained at your actual oracle level") &&
                    !leaf.Description.Contains("Still at your actual oracle level") &&
                    !leaf.Description.Contains("limited to"),
                    leaf.Symbol + " discloses its counter, routes and effective-level gates.");
                FavoredClassRevelationTarget target = FavoredClassRevelationManifest.For(leaf.TargetKey);
                Assertions.Equal(!target.Published, leaf.Description.Contains("Not offered:"),
                    leaf.Symbol + " states its exclusion exactly when it is excluded.");
            }
            Assertions.Equal("Combat Healer (Battle)", FavoredClassLeafCatalog.TargetTitle(effect, "BattleCombatHealer"),
                "Shared names carry their mystery.");
            Assertions.Equal("Combat Healer (Life)", FavoredClassLeafCatalog.TargetTitle(effect, "LifeCombatHealer"),
                "Shared names carry their mystery.");
            Assertions.Equal("Fire Breath", FavoredClassLeafCatalog.TargetTitle(effect, "FireBreath"),
                "Unique names stay plain.");
            Assertions.Equal(104, leaves.Select(leaf => leaf.Name).Distinct(StringComparer.Ordinal).Count(),
                "Every revelation leaf has its own name.");
        }

        // Native resource arithmetic at the effective level, with every audited formula shape.
        internal static void ResourceMaximumMatchesTheNativeFormula()
        {
            Func<int, int, int, int, int, FavoredClassResourceAmount> sds = (b, s, i, n, p) =>
                new FavoredClassResourceAmount
                {
                    BaseValue = b, IncreasedByLevelStartPlusDivStep = true, StartingLevel = s, StartingIncrease = i,
                    LevelStep = n, PerStepIncrease = p, DivScalesWithOracle = true
                };
            Func<int, FavoredClassResourceAmount> perLevel = k => new FavoredClassResourceAmount
            {
                IncreasedByLevel = true, LevelIncrease = k, LevelScalesWithOracle = true
            };
            Func<FavoredClassResourceAmount, int, int> at = (amount, level) =>
                FavoredClassMechanicsPolicy.ResourceMaximum(amount, level, level, level, 3, 0);
            // RES-SDS(1,5,1,5,1): Fire Breath, Heat Aura, Aging Touch, Blood of Heroes.
            FavoredClassResourceAmount breath = sds(1, 5, 1, 5, 1);
            Assertions.True(new[] { 1, 1, 1, 1, 2, 2, 2, 2, 2, 3 }.SequenceEqual(
                Enumerable.Range(1, 10).Select(level => at(breath, level))), "Fire Breath uses by level.");
            Assertions.Equal(1, FavoredClassMechanicsPolicy.ResourceDelta(breath, 9, 9, 9, 0, 2),
                "Two steps at 9th level: 2 -> 3 uses.");
            // RES-SDS(1,7,0,4,1): Rewind Time 1, 2@11, 3@15, 4@19.
            FavoredClassResourceAmount rewind = sds(1, 7, 0, 4, 1);
            Assertions.True(at(rewind, 7) == 1 && at(rewind, 10) == 1 && at(rewind, 11) == 2 &&
                at(rewind, 15) == 3 && at(rewind, 19) == 4, "Rewind Time native steps.");
            Assertions.Equal(1, FavoredClassMechanicsPolicy.ResourceDelta(rewind, 10, 10, 10, 0, 1),
                "One step at 10th level reaches the 11th-level use.");
            // RES-SDS(0,1,2,1,2) = 2L (Spirit Walk rounds).
            FavoredClassResourceAmount walk = sds(0, 1, 2, 1, 2);
            Assertions.True(Enumerable.Range(1, 20).All(level => at(walk, level) == 2 * level), "2L rounds.");
            Assertions.Equal(4, FavoredClassMechanicsPolicy.ResourceDelta(walk, 9, 9, 9, 0, 2), "2 steps = 4 rounds.");
            // RES-LVL(1) = L minutes (Ancestral Weapon, Time Flicker).
            Assertions.Equal(2, FavoredClassMechanicsPolicy.ResourceDelta(perLevel(1), 9, 0, 9, 0, 2),
                "Per-level resources gain exactly the steps.");
            // One-threshold amounts of the revelation's own resource (Erase From Time @11,
            // Iron Skin @15, Scaled Toughness @13, Raise the Dead @10) move with the
            // effective level (charter 8.10: the owned power's thresholds are traced).
            var thresholds = new[]
            {
                Tuple.Create(sds(1, 11, 1, 100, 0), 11), Tuple.Create(sds(1, 15, 1, 10, 0), 15),
                Tuple.Create(sds(1, 13, 1, 13, 0), 13), Tuple.Create(sds(1, 10, 1, 10, 0), 10)
            };
            foreach (var threshold in thresholds)
            {
                FavoredClassResourceAmount amount = threshold.Item1;
                int level = threshold.Item2;
                Assertions.True(amount.ScalesWithOracle, "A single threshold of the owned resource is traced.");
                Assertions.Equal(1, at(amount, level - 1), "Below the threshold: one use.");
                Assertions.Equal(2, at(amount, level), "At the threshold: two uses.");
                Assertions.Equal(1, FavoredClassMechanicsPolicy.ResourceDelta(amount, level - 1, level - 1,
                    level - 1, 0, 1), "One step reaches the threshold one level early.");
                Assertions.Equal(0, FavoredClassMechanicsPolicy.ResourceDelta(amount, level - 3, level - 3,
                    level - 3, 0, 1), "One step short of the threshold adds nothing.");
                Assertions.Equal(0, FavoredClassMechanicsPolicy.ResourceDelta(amount, level, level, level, 0, 3),
                    "Past the threshold nothing more is added.");
            }
            Assertions.True(breath.ScalesWithOracle && rewind.ScalesWithOracle && walk.ScalesWithOracle &&
                perLevel(1).ScalesWithOracle, "Scaling resources are recognized.");
            // The native clamp applies before handler bonuses: the delta stops at the maximum.
            FavoredClassResourceAmount clamped = perLevel(1);
            clamped.UseMax = true;
            clamped.Max = 10;
            Assertions.Equal(1, FavoredClassMechanicsPolicy.ResourceDelta(clamped, 9, 0, 9, 0, 2), "Clamped at 10.");
            Assertions.Equal(0, FavoredClassMechanicsPolicy.ResourceDelta(clamped, 12, 0, 12, 0, 2), "Already clamped.");
            // Stat and base terms are identical on both sides.
            FavoredClassResourceAmount stat = perLevel(1);
            stat.IncreasedByStat = true;
            stat.BaseValue = 1;
            Assertions.Equal(14, FavoredClassMechanicsPolicy.ResourceMaximum(stat, 9, 0, 9, 4, 0), "1 + L + Cha.");
            Assertions.Equal(2, FavoredClassMechanicsPolicy.ResourceDelta(stat, 9, 0, 9, 4, 2), "Stat term cancels.");
            // A minimum per-step increase follows the native Math.Max.
            FavoredClassResourceAmount minimum = sds(0, 1, 0, 4, 1);
            minimum.MinClassLevelIncrease = 1;
            Assertions.True(at(minimum, 1) == 1 && at(minimum, 5) == 1 && at(minimum, 9) == 2, "Minimum increase.");
            // Classes that do not include the oracle, or other classes' levels, never shift.
            FavoredClassResourceAmount foreign = sds(1, 5, 1, 5, 1);
            foreign.DivScalesWithOracle = false;
            Assertions.False(foreign.ScalesWithOracle, "No oracle class, no scaling.");
            Assertions.Equal(0, FavoredClassMechanicsPolicy.ResourceDelta(foreign, 9, 9, 9, 0, 2), "No shift.");
            FavoredClassResourceAmount mixed = sds(1, 5, 1, 5, 1);
            mixed.OtherClassesModifier = 0.5f;
            Assertions.False(mixed.ScalesWithOracle, "Mixed character-level amounts are not scaled.");
            Assertions.Equal(0, FavoredClassMechanicsPolicy.ResourceDelta(mixed, 9, 9, 12, 0, 2), "No mixed shift.");
            Assertions.Equal(0, FavoredClassMechanicsPolicy.ResourceDelta(breath, 9, 9, 9, 0, 0), "No steps, no bonus.");
        }

        // The walker, adapters and rank hook follow the audited scoping rules.
        internal static void AdaptersAreScopedToTheChosenRevelation()
        {
            string scopes = Source("FavoredClassRevelationScopes.cs");
            foreach (string token in new[]
            {
                "ContextRankBaseValueType.SummClassLevelWithArchetype",
                "ReferenceEquals(RankArchetype.GetValue(config), hunter)",
                "== ContextRankProgression.Custom",
                "scope.Target.ExcludedRanks.Contains(key)",
                "scope.Evidence.Add(\"tier:\" + label);",
                "scope.Evidence.Add(\"breakpoint-table:\" + label);",
                "if (actionsOnly || value is BlueprintFeatureSelection)",
                "component is IInitiatorRulebookSubscriber",
                "IsRuleReaction(current.Value)",
                "component is Prerequisite",
                "WithholdShared(byKey.Values.ToList());",
                "!owners.TryGetValue(context.AssociatedBlueprint, out scope)",
                "formula.Amount.ScalesWithOracle",
                "FavoredClassRevelationScopes.IsDeparting(leaf)",
                "!FavoredClassRuntime.MechanicsEnabled",
                "if (levelGate != null && current.Owner != null && IsOracleGate(levelGate, oracle))",
                "scope.Gates.Add(recorded);",
                "scope.Evidence.Add(\"withheld-shared-gate:\" + gate.Label);",
                "scope.PartialReason = ",
                "if (!scope.Target.Published)",
                "if (!scope.HasReadPoints || scope.PartialReason != null)",
                "result = FavoredClassMechanicsPolicy.GateApplies(level + steps, gate.Level, gate.BeforeThisLevel);",
                "ReplaceCasterLevelOfAbility.CalculateClassLevel(gate.Class, gate.AdditionalClasses, owner,"
            })
                Assertions.True(scopes.Contains(token), "Scopes: " + token);
            string level = Source("Mechanics", "FavoredClassSelectedRevelationLevel.cs");
            foreach (string token in new[]
            {
                "!evt.ReplaceCasterLevel.HasValue",
                "scope.ParamsAbilities.Contains(evt.Blueprint)",
                "evt.ReplaceCasterLevel = level + earned;",
                "FavoredClassMechanicsPolicy.HalfLevelDelta(level, earned);",
                "scope.Resources.ContainsKey(resource)",
                "if (IsReapplying)",
                "FavoredClassRevelationScopes.BeginDeparture(fact);",
                "feature.Recalculate();",
                "feature.CallComponents<AddFeatureOnClassLevel>(gate => gate.HandleUnitGainLevel(owner, null));"
            })
                Assertions.True(level.Contains(token), "Revelation level: " + token);
            string gateHook = Source("Hooks", "FavoredClassRevelationGatePatch.cs");
            Assertions.True(gateHook.Contains("[HarmonyPatch(typeof(AddFeatureOnClassLevel), \"IsFeatureShouldBeApplied\")]") &&
                gateHook.Contains("FavoredClassRevelationScopes.GateResult(__instance, ref __result);") &&
                gateHook.Contains("catch (Exception)"),
                "The gate hook re-decides only scoped gates and fails safe to the native decision.");
            string publication = Source("FavoredClassPublication.cs");
            Assertions.True(publication.Contains("!FavoredClassRevelationManifest.For(pair.TargetKey).Published)"),
                "An excluded revelation is never published.");
            Assertions.False(level.Contains("AddBonusCasterLevel") || level.Contains("AddBonusDC"),
                "No rank bonus: Call of the Wild would count it again in every Oracle rank.");
            string hook = Source("Hooks", "FavoredClassRevelationRankPatch.cs");
            Assertions.True(hook.Contains("[HarmonyPatch(typeof(ContextRankConfig), \"GetBaseValue\")]") &&
                hook.Contains("[HarmonyAfter(\"CallOfTheWild\")]") && hook.Contains("[HarmonyPriority(Priority.Last)]") &&
                hook.Contains("FavoredClassRevelationScopes.RankBonus(__instance, context)") &&
                hook.Contains("catch (Exception)"),
                "The rank hook adds to the base value after Call of the Wild's postfix.");
            string coordinator = Source("FavoredClassIntegrationCoordinator.cs");
            Assertions.True(coordinator.Contains("FavoredClassRevelationScopes.Build(BlueprintBootstrap.Library, set)") &&
                coordinator.Contains("FavoredClassRevelationScopes.Clear();") &&
                coordinator.IndexOf("FavoredClassRevelationScopes.Build(", StringComparison.Ordinal) <
                    coordinator.IndexOf("publication = FavoredClassPublication.Plan(", StringComparison.Ordinal),
                "Scopes are built before planning, withhold unfound targets, and roll back with the publication.");
            string blueprints = Source("FavoredClassBlueprints.cs");
            Assertions.True(blueprints.Contains("{ FavoredClassCatalog.Oracle, FavoredClassRevelationManifest.OracleClassGuid }") &&
                blueprints.Contains("PrerequisiteFavoredClassOwnsAny") &&
                blueprints.Contains("revelation.TargetKey = targetKey;"),
                "Revelation leaves require the owned revelation and carry their own target.");
        }
    }
}
