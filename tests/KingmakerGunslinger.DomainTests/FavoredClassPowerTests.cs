using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.FavoredClass;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>I08/S06 selected bloodline power adapters (Phase 4).</summary>
    internal static class FavoredClassPowerTests
    {
        private static string Source(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "FavoredClass" }.Concat(parts).ToArray()));
        }

        // floor((L+e)/2) - floor(L/2): the DC change of a half-level binding.
        internal static void HalfLevelDcDeltaIsExact()
        {
            var cases = new[]
            {
                Tuple.Create(9, 1, 1), Tuple.Create(10, 1, 0), Tuple.Create(9, 2, 1),
                Tuple.Create(10, 2, 1), Tuple.Create(0, 1, 0), Tuple.Create(1, 1, 1),
                Tuple.Create(20, 2, 1), Tuple.Create(19, 2, 1), Tuple.Create(7, 0, 0),
                Tuple.Create(-3, 2, 1)
            };
            foreach (var value in cases)
                Assertions.Equal(value.Item3, FavoredClassMechanicsPolicy.HalfLevelDelta(value.Item1, value.Item2),
                    "HalfLevelDelta(" + value.Item1 + "," + value.Item2 + ")");
        }

        // Fire powers open only through the Ifrit row, air powers only through the Sylph row.
        internal static void PowerTargetsFollowTheirOwnRows()
        {
            string effect = FavoredClassCatalog.EffectSelectedBloodlinePower;
            Assertions.True(FavoredClassLeafCatalog.TargetKeys(effect).SequenceEqual(
                new[] { "FireRay", "FireBlast", "AirRay", "AirBlast", "FireResistance", "AirResistance" }),
                "Six implemented power targets.");
            Assertions.True(FavoredClassLeafCatalog.TargetRows(effect, "FireRay").SequenceEqual(new[] { "I08" }) &&
                FavoredClassLeafCatalog.TargetRows(effect, "FireBlast").SequenceEqual(new[] { "I08" }) &&
                FavoredClassLeafCatalog.TargetRows(effect, "AirRay").SequenceEqual(new[] { "S06" }) &&
                FavoredClassLeafCatalog.TargetRows(effect, "AirBlast").SequenceEqual(new[] { "S06" }) &&
                FavoredClassLeafCatalog.TargetRows(effect, "FireResistance").SequenceEqual(new[] { "I08" }) &&
                FavoredClassLeafCatalog.TargetRows(effect, "AirResistance").SequenceEqual(new[] { "S06" }),
                "Per-target source rows.");
            FavoredClassEffectSpec spec = FavoredClassCatalog.Effect(effect);
            Func<string, string, bool> eligible = (ancestry, target) => FavoredClassEligibility.IsEligible(spec,
                new System.Collections.Generic.HashSet<string>(new[] { ancestry }, StringComparer.Ordinal),
                profile => profile != FavoredClassProfile.None, race => true,
                FavoredClassLeafCatalog.TargetRows(effect, target));
            Assertions.True(eligible(FavoredClassAncestry.Ifrit, "FireRay") &&
                eligible(FavoredClassAncestry.Ifrit, "FireBlast") &&
                eligible(FavoredClassAncestry.Ifrit, "FireResistance"), "Ifrit opens fire powers.");
            Assertions.True(eligible(FavoredClassAncestry.Sylph, "AirResistance") &&
                !eligible(FavoredClassAncestry.Ifrit, "AirResistance") &&
                !eligible(FavoredClassAncestry.Sylph, "FireResistance"), "Elemental Resistance follows its element.");
            Assertions.False(eligible(FavoredClassAncestry.Ifrit, "AirRay") ||
                eligible(FavoredClassAncestry.Ifrit, "AirBlast"), "Ifrit never opens air powers.");
            Assertions.True(eligible(FavoredClassAncestry.Sylph, "AirRay") &&
                eligible(FavoredClassAncestry.Sylph, "AirBlast"), "Sylph opens air powers.");
            Assertions.False(eligible(FavoredClassAncestry.Sylph, "FireRay") ||
                eligible(FavoredClassAncestry.Human, "FireRay") || eligible(FavoredClassAncestry.Human, "AirRay"),
                "Other ancestries open no power.");
            foreach (FavoredClassLeafSpec leaf in FavoredClassLeafCatalog.LeavesFor(effect))
            {
                bool fire = leaf.TargetKey.StartsWith("Fire", StringComparison.Ordinal);
                Assertions.True(leaf.Description.Contains(fire ? "Ifrit" : "Sylph") &&
                    !leaf.Description.Contains(fire ? "Sylph" : "Ifrit"),
                    leaf.Symbol + " names only its own route.");
                Assertions.True(leaf.Description.Contains("Each bloodline power keeps its own separate count"),
                    leaf.Symbol + " discloses its separate counter.");
            }
        }

        // Charter 8.10: exactly the fire and air elemental bloodline identities
        // and their proven Seeker/Crossblooded copies; never a Primal copy.
        internal static void EligibleBloodlinesAreExact()
        {
            string[] primal = { "3bb24b6f1cd741f78e585485b163dc45", "248fc512bb12418ebd5f2bd10917bf7a" };
            foreach (string target in new[] { "FireRay", "FireBlast", "AirRay", "AirBlast" })
            {
                System.Collections.Generic.KeyValuePair<string, string[]> eligible =
                    FavoredClassLeafCatalog.EligibleBloodlines(target);
                bool fire = target.StartsWith("Fire", StringComparison.Ordinal);
                Assertions.Equal(fire ? "the fire elemental bloodline" : "the air elemental bloodline", eligible.Key,
                    target + " bloodline name.");
                Assertions.True(eligible.Value.SequenceEqual(fire
                    ? new[] { "17cc794d47408bc4986c55265475c06f", "3950cf0cafa5c6ba1d1fc840d2682837",
                        "ae4f8d4d7f23c49929b4f4b88757524c" }
                    : new[] { "cd788df497c6f10439c7025e87864ee4", "e3e43bb57f23bc7abcb49f38019ba6bc",
                        "74fb79f4afa5be59881fa3c054a4dcc7" }), target + " eligible identities.");
                Assertions.False(eligible.Value.Intersect(primal).Any(), target + " excludes Primal copies.");
            }
            string blueprints = Source("FavoredClassBlueprints.cs");
            Assertions.True(blueprints.Contains("FavoredClassLeafCatalog.EligibleBloodlines(targetKey);") &&
                blueprints.Contains("bloodline.FeatureGuids = bloodlines.Value;"),
                "Bloodline power leaves require an eligible bloodline identity.");
            Assertions.True(blueprints.Contains("owned.FeatureGuids = new[] { usablePower.AssetGuid };") &&
                !blueprints.Contains("CreateInstance<PrerequisiteFeature>()"),
                "The usable power is an owned target like the bloodline and the revelations.");
            string prerequisites = Source("FavoredClassPrerequisites.cs");
            string replay = Source(Path.Combine("Hooks", "FavoredClassLevelUpReplayPatch.cs"));
            Assertions.True(prerequisites.Contains("return FavoredClassPendingPicks.Selects(state, FeatureGuids);") &&
                prerequisites.Contains("!ReferenceEquals(controller.State, state)") &&
                prerequisites.Contains("return Replays.Run(controller, () =>") &&
                prerequisites.Contains("            Replays.Run(controller, () =>") &&
                prerequisites.Contains("                action.Apply(state, unit);") &&
                prerequisites.Contains("LevelUpController replaying = Installed && s_Replays != null ? s_Replays.Current : null;") &&
                !prerequisites.Contains("s_Replaying") &&
                replay.Contains("[HarmonyPatch(typeof(LevelUpController), \"ApplyLevelup\")]") &&
                replay.Contains("FavoredClassCallScoping.ReplaceSingle(values, NativeCheck, ScopedCheck, true)") &&
                replay.Contains("FavoredClassCallScoping.ReplaceSingle(values, NativeApply, ScopedApply, true)") &&
                !replay.Contains("Prefix") && !replay.Contains("Postfix"),
                "A target chosen in the same level-up counts only inside the finally-closed scope of a replayed pick.");
        }

        // E15: a stored level plan's picks are checked in the plan's own
        // scope, so its same-level target counts; only the one native call is
        // replaced, and only the plan's own controller and state count.
        internal static void StoredPlanScopesItsSameLevelTarget()
        {
            string prerequisites = Source("FavoredClassPrerequisites.cs");
            string plan = Source(Path.Combine("Hooks", "FavoredClassLevelPlanPatch.cs"));
            foreach (string token in new[]
            {
                "[HarmonyPatch(typeof(LevelUpController), \"ApplyLevelUpPlan\")]",
                "FavoredClassPendingPicks.BindNativeAddAction(NativeAddAction) &&",
                "FavoredClassCallScoping.ReplaceSingle(values, NativeAddAction, ScopedAddAction, false);",
                "FavoredClassPendingPicks.PlanInstalled = scoped;",
                "return scoped ? values : original;",
                "new[] { typeof(ILevelUpAction), typeof(bool) }"
            })
                Assertions.True(plan.Contains(token), "Plan hook token: " + token);
            Assertions.True(!plan.Contains("Prefix") && !plan.Contains("Postfix"),
                "The plan scope closes in a finally block, never by a postfix.");
            foreach (string token in new[]
            {
                "return Plans.Run(new PlannedLevel(controller, plan == null ? null : plan.Actions),",
                "controller.Unit.Progression.GetLevelPlan(controller.State.NextLevel);",
                "PlannedLevel planned = PlanInstalled && s_Plans != null ? s_Plans.Current : null;",
                "!ReferenceEquals(controller.State, state)",
                "return planned != null && ReferenceEquals(planned.Controller, controller) &&",
                "get { return (s_Replays == null ? 0 : s_Replays.Depth) + (s_Plans == null ? 0 : s_Plans.Depth); }"
            })
                Assertions.True(prerequisites.Contains(token), "Plan scope token: " + token);
            string coordinator = Source("FavoredClassIntegrationCoordinator.cs");
            Assertions.True(coordinator.Contains("if (!FavoredClassPendingPicks.PlanInstalled)") &&
                coordinator.Contains("the scoped level plan (LevelUpController.ApplyLevelUpPlan) is not installed"),
                "A missing plan hook is reported as degraded.");
        }

        // One chosen power's own ability only; exact DC delta from its own binding.
        internal static void EffectiveLevelIsScopedToTheChosenPower()
        {
            string level = Source("Mechanics", "FavoredClassSelectedPowerLevel.cs");
            foreach (string token in new[]
            {
                "evt.AddBonusCasterLevel(earned);",
                "int delta = FavoredClassMechanicsPolicy.HalfLevelDelta(level, earned);",
                "evt.AddBonusDC(delta);",
                "return spell != null && (spell == Ability || (spell.Parent != null && spell.Parent == Ability));",
                "value.Abilites != null && value.Abilites.Contains(Ability)",
                "!FavoredClassRuntime.MechanicsEnabled",
                "IResourceAmountBonusHandler, IUnitSubscriber",
                "int real = progression.CalcLevel(Owner);",
                "bonus += FavoredClassMechanicsPolicy.ThresholdUsesBetween(UseThresholds(progression, UsesResource),",
                "if (increase.Resource == resource && increase.Value > 0)",
                "foreach (KeyValuePair<AddFeatureOnClassLevel, int> gate in UseGates(PowerFeature, UsesResource))",
                "bonus += FavoredClassMechanicsPolicy.GateUsesDelta(level, earned, component.Level,",
                "int level = ReplaceCasterLevelOfAbility.CalculateClassLevel(component.Class,",
                "!Owner.HasFact(PowerFeature))"
            })
                Assertions.True(level.Contains(token), "Selected power token: " + token);
            foreach (string forbidden in new[] { "AddFakeClassLevel", "Progression.AddClass", "AddBonusSpellLevel",
                "Stats.BaseAttackBonus", "Spellbook.AddCasterLevel" })
                Assertions.False(level.Contains(forbidden), "No blanket class-level change: " + forbidden);
            string blueprints = Source("FavoredClassBlueprints.cs");
            foreach (string token in new[]
            {
                "\"ce0889b5c1b392e48baf1e004d1efd67\", \"1b4989258e5964149a909e47c72b7f67\"",
                "\"3022a5066a5604a498dd289b37dfd8aa\", \"b2d1d39cd406e0f4185c52fecc73c3b5\"",
                "\"acf668c24dfbcdd499276eaf1881486e\", \"4729c2ac98d02004fb440d17f7786e28\"",
                "\"553d9802d5d9de04b941b55cb47d3096\", \"6d005cc9c3ad3f24e8769aad2fbfdf3f\"",
                "{ FavoredClassCatalog.Sorcerer, \"b3a505fb61437dc4097f43c3f8f9a4cf\" },",
                "owned.FeatureGuids = new[] { usablePower.AssetGuid };",
                "prerequisite.RowIds = restrictedRows;",
                "level.UsesResource = logic == null ? null : logic.RequiredResource;",
                "level.BloodlineGuids = FavoredClassLeafCatalog.EligibleBloodlines(targetKey).Value;"
            })
                Assertions.True(blueprints.Contains(token), "Bloodline power wiring token: " + token);
        }

        // I08/S06 Elemental Resistance: no ability; only the owned power
        // feature's own level gates move, at most two steps, and the counter
        // re-decides them on gain and on a real removal.
        internal static void ElementalResistanceMovesOnlyItsOwnGates()
        {
            string blueprints = Source("FavoredClassBlueprints.cs");
            foreach (string token in new[]
            {
                "{ \"FireResistance\", new KeyValuePair<string, string>(",
                "\"24980315c1bdcc4478ebb717e9b81961\", null) },",
                "{ \"AirResistance\", new KeyValuePair<string, string>(",
                "\"6472c51065d734e4b99ac56694925920\", null) },",
                "if (power.Value == null)",
                "FavoredClassSelectedPowerGates.Register(gates);",
                "gates.Leaf = full;"
            })
                Assertions.True(blueprints.Contains(token), "Elemental Resistance wiring token: " + token);
            string hook = Source("Hooks", "FavoredClassRevelationGatePatch.cs");
            Assertions.True(hook.Contains("Mechanics.FavoredClassSelectedPowerGates.GateResult(__instance, ref __result);"),
                "The gate hook consults the power gates, fail-safe.");
            string gates = Source("Mechanics", "FavoredClassSelectedPowerGates.cs");
            foreach (string token in new[]
            {
                "ByPower.TryGetValue(fact.Blueprint, out registration)",
                "FavoredClassMechanicsPolicy.GateApplies(level + steps, gate.Level, gate.BeforeThisLevel)",
                "CapSteps > 0 ? CapSteps : (int?)null",
                "FavoredClassRevelationScopes.IsDeparting(leaf)",
                "FavoredClassRevelationScopes.BeginDeparture(fact);",
                "gate => gate.HandleUnitGainLevel(owner, null)",
                "!FavoredClassRuntime.MechanicsEnabled"
            })
                Assertions.True(gates.Contains(token), "Elemental Resistance gate token: " + token);
            Assertions.Equal(2, FavoredClassCatalog.Effect(FavoredClassCatalog.EffectSelectedBloodlinePower).Rate.CapSteps.Value,
                "Bloodline powers are capped at +2.");
            // Capped: two steps at 7th level reach the 9th-level step; one does not.
            Assertions.True(KingmakerGunslinger.FavoredClass.FavoredClassMechanicsPolicy.GateApplies(7 + 2, 9, false) &&
                !KingmakerGunslinger.FavoredClass.FavoredClassMechanicsPolicy.GateApplies(7 + 1, 9, false) &&
                !KingmakerGunslinger.FavoredClass.FavoredClassMechanicsPolicy.GateApplies(7 + 2, 9, true),
                "The resistance step moves exactly at the effective 9th level.");
        }
    }
}
