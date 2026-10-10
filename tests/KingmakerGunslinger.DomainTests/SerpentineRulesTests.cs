using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.RuntimeTesting;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SerpentineRulesTests
    {
        internal const int AppendedLedgerIdentities = 75;
        internal static void PersistenceRequiresTheExactNativeLoadCompletion()
        {
            var gate = new SerpentineLoadBoundaryReadiness();
            object state = new object(), otherState = new object(), area = new object(), otherArea = new object();
            Assertions.False(gate.Ready(state, area), "No elapsed time or clean symptoms imply a native callback.");
            gate.Completed(state, area, 10);
            Assertions.False(gate.Ready(state, area), "Completion without its scenes-loaded identity is stale.");
            gate.ScenesLoaded(state, area, 10);
            Assertions.False(gate.Ready(state, area), "The exact runtime failure: scenes loaded is insufficient.");
            foreach (object[] wrong in new[] { new[] { otherState, area }, new[] { state, otherArea },
                new[] { null, area }, new[] { state, null } })
            {
                gate.Completed(wrong[0], wrong[1], 12);
                Assertions.False(gate.Ready(state, area), "Foreign/null callback cannot unlock this fixture.");
            }
            gate.Completed(state, area, 9);
            Assertions.False(gate.Ready(state, area), "A preceding callback frame is not this load.");
            gate.Completed(state, area, 12);
            Assertions.True(gate.Ready(state, area), "Actual correlated callback return unlocks observation.");
            Assertions.Equal(12, gate.CompletedFrame, "Record native evidence, not a timeout guess.");
            Assertions.False(gate.Ready(otherState, area) || gate.Ready(state, otherArea), "Readiness cannot transfer.");
            gate.ScenesLoaded(state, area, 13);
            Assertions.False(gate.Ready(state, area), "Even same-state reload invalidates previous completion.");
            gate.Completed(state, area, 13);
            Assertions.True(gate.Ready(state, area), "No invented minimum frame delay after the actual event.");
            gate.ScenesLoaded(null, area, 14);
            gate.Completed(null, area, 15);
            Assertions.False(gate.Ready(null, area), "Null state cannot self-correlate.");
        }

        internal static void LoadResetTraceIsRestrictedToTheClosedProtocol()
        {
            string[] allowed = { RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningPrepare,
                RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup,
                RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyAbsent };
            foreach (string scenario in allowed.Concat(new[] { null, "", "working-save-smoke",
                "working-save-expanded-summoning-creature-review", "arbitrary" }))
            foreach (string scope in new[] { null, "", "snakes", "Snakes", "crocodilians", "all" })
                Assertions.Equal(allowed.Contains(scenario) && scope == "snakes",
                    SerpentinePersistenceReviewPolicy.ObserveLoad(scenario, scope),
                    "Read-only hooks arm only for the three accepted snake persistence requests.");
        }

        internal static void PersistenceResetDiagnosticsPreserveEveryOperand()
        {
            for (int mask = 0; mask < 128; mask++)
            foreach (int links in new[] { 0, 1, 4 })
            {
                bool grab = (mask & 1) == 0, initiator = (mask & 2) != 0, target = (mask & 4) != 0;
                bool hold = (mask & 8) != 0, grappled = (mask & 16) != 0;
                bool cantAct = (mask & 32) != 0, cantMove = (mask & 64) != 0;
                string[] failures = SerpentinePersistenceReviewPolicy.SessionResetFailures(
                    grab, initiator, target, links, hold, grappled, cantAct, cantMove);
                bool previous = grab && !initiator && !target && links == 0 &&
                    !hold && !grappled && !cantAct && !cantMove;
                Assertions.Equal(previous, failures.Length == 0,
                    "Diagnostics retain every original strict reset operand; no waiver.");
                Assertions.Equal(!grab, failures.Contains("grab-missing"), "Missing source component remains distinct.");
                Assertions.Equal(initiator, failures.Contains("initiator-part"), "Native owner part identified.");
                Assertions.Equal(target, failures.Contains("target-part"), "Native prey part identified.");
                Assertions.Equal(links != 0, failures.Contains("stored-links"), "Stored links identified.");
                Assertions.Equal(hold, failures.Contains("hold-buff"), "Owned hold fact identified.");
                Assertions.Equal(grappled, failures.Contains("grappled-buff"), "Owned held fact identified.");
                Assertions.Equal(cantAct, failures.Contains("cant-act"), "Action lock identified.");
                Assertions.Equal(cantMove, failures.Contains("cant-move"), "Movement lock identified.");
                Assertions.Equal(failures.Length, failures.Distinct().Count(), "Each cause is recorded once.");
            }
            Assertions.True(SerpentinePersistenceReviewPolicy.SessionResetFailures(
                true, false, false, -1, false, false, false, false).SequenceEqual(new[] { "invalid-link-count" }),
                "Malformed negative count cannot become a clean reset or invent a real link.");
        }

        internal static void CrowdAwakeRestoresOnlyOwnedReferences()
        {
            object foreignA = new object(), foreignB = new object();
            object first = new string('x', 1), second = new string('x', 1), added = new object();
            object[] owned = { first, second, added }, before = { foreignA, first, foreignB, second };
            foreach (string key in new[] { "viper", "constrictor-snake" })
            foreach (object[] native in new[] {
                new[] { foreignA, foreignB },
                new[] { foreignA, second, foreignB, added },
                new[] { first, foreignA, second, added, foreignB },
                before
            })
            {
                var live = new System.Collections.Generic.List<object>(native);
                string detail;
                Assertions.True(SerpentineCrowdReviewPolicy.RestoreOwnedAwake(key, live, before, owned,
                    value => true, out detail), "Restore omitted, reordered and introduced OWNED references.");
                Assertions.True(live.Count == before.Length && live.Select((value, index) =>
                    ReferenceEquals(value, before[index])).All(value => value), "Full exact sequence remains mandatory.");
                Assertions.True(live.Where(value => ReferenceEquals(value, foreignA) ||
                    ReferenceEquals(value, foreignB)).SequenceEqual(new[] { foreignA, foreignB }),
                    "Unrelated actors preserve their exact references/order.");
                Assertions.Equal(4, before.Length, "The source snapshot was not mutated.");
            }
        }

        internal static void CrowdAwakeRejectsForeignChangesWithoutMutation()
        {
            object foreignA = new object(), foreignB = new object(), foreignNew = new object();
            object first = new object(), second = new object();
            object[] before = { foreignA, first, foreignB, second }, owned = { first, second };
            foreach (object[] native in new[] {
                new[] { foreignA, first },
                new[] { foreignB, foreignA, second },
                new[] { foreignA, foreignB, foreignNew },
                new[] { foreignNew, first, foreignB }
            })
            {
                var live = new System.Collections.Generic.List<object>(native);
                string detail;
                Assertions.False(SerpentineCrowdReviewPolicy.RestoreOwnedAwake("viper", live, before, owned,
                    value => true, out detail), "Unrelated sleep/wake/reordering is a failure, never repair authority.");
                Assertions.Equal("unrelated-awake-sequence-changed-no-mutation", detail, "Failure is explicit.");
                Assertions.True(live.Count == native.Length && live.Select((value, index) =>
                    ReferenceEquals(value, native[index])).All(value => value), "Rejection made no list mutation.");
            }
            // Value equality cannot replace native object identity.
            object original = new string('a', 1), replacement = new string('a', 1);
            var replaced = new System.Collections.Generic.List<object> { replacement, first };
            string disposition;
            Assertions.False(SerpentineCrowdReviewPolicy.RestoreOwnedAwake("viper", replaced,
                new[] { original, first }, owned, value => true, out disposition),
                "A different equal-valued foreign object cannot impersonate the captured reference.");
            Assertions.True(ReferenceEquals(replaced[0], replacement), "Foreign replacement remains untouched.");
        }

        internal static void CrowdAwakeRejectsMalformedOrDeadScope()
        {
            object first = new object(), second = new object();
            object[] before = { first }, validOwned = { first, second };
            foreach (string key in new[] { null, "", "Viper", "Salamander", "crocodile", "purple-worm" })
            {
                var live = new System.Collections.Generic.List<object> { first, second };
                string detail;
                Assertions.False(SerpentineCrowdReviewPolicy.RestoreOwnedAwake(key, live, before, validOwned,
                    value => true, out detail), "Only the three exact Sprint 17 creature keys.");
                Assertions.True(live.SequenceEqual(validOwned), "Rejected roster leaves the list untouched.");
            }
            foreach (object[] owned in new[] { new object[0], new[] { first }, new[] { first, first },
                new[] { first, (object)null }, new[] { first, second, new object(), new object(), new object(), new object() } })
            {
                var live = new System.Collections.Generic.List<object> { first, second };
                string detail;
                Assertions.False(SerpentineCrowdReviewPolicy.RestoreOwnedAwake("viper", live, before, owned,
                    value => true, out detail), "Requires two-to-five distinct nonnull owned actors.");
                Assertions.True(live.SequenceEqual(validOwned), "Invalid scope leaves the list untouched.");
            }
            string reason;
            var salamander = new System.Collections.Generic.List<object> { first, second };
            Assertions.True(SerpentineCrowdReviewPolicy.RestoreOwnedAwake("salamander", salamander,
                before, validOwned, value => true, out reason), "Exact owned Salamander crowd shares the bounded fixture.");
            Assertions.True(salamander.Count == 1 && ReferenceEquals(salamander[0], first),
                "Only owned additional membership is retired; the captured reference remains exact.");
            var unchanged = new System.Collections.Generic.List<object> { first, second };
            Assertions.False(SerpentineCrowdReviewPolicy.RestoreOwnedAwake("viper", unchanged, before, validOwned,
                value => !ReferenceEquals(value, first), out reason), "Destroyed or foreign-area owned actor cannot be reawakened.");
            Assertions.True(unchanged.SequenceEqual(validOwned), "Liveness rejection is nonmutating.");
            Assertions.False(SerpentineCrowdReviewPolicy.RestoreOwnedAwake("viper", unchanged,
                new[] { first, first }, validOwned, value => true, out reason), "Ambiguous duplicate snapshot rejects.");
            Assertions.False(SerpentineCrowdReviewPolicy.RestoreOwnedAwake("viper", unchanged.AsReadOnly(),
                before, validOwned, value => true, out reason), "Readonly storage rejects before mutation.");
            Assertions.False(SerpentineCrowdReviewPolicy.RestoreOwnedAwake("viper", unchanged,
                before, validOwned, null, out reason), "No liveness/area authority rejects.");
            Assertions.True(unchanged.SequenceEqual(validOwned), "Every rejected boundary is unchanged.");
        }

        internal static void PersistenceArmingDiagnosticsDistinguishNativeOutcomes()
        {
            Func<bool, int?, int, bool, bool, string> observe = SerpentinePersistenceReviewPolicy.ArmingObservation;
            Assertions.Equal("bite-did-not-hit", observe(false, 0, 0, false, false), "A miss is not a poison defect.");
            Assertions.Equal("no-melee-damage-event", observe(true, null, 0, false, false), "Missing damage differs from zero.");
            Assertions.Equal("bite-did-not-wound", observe(true, 0, 0, false, false), "Zero damage must not deliver injury poison.");
            foreach (int count in new[] { 0, 2, 3 })
                Assertions.Equal("injury-save-census-not-one", observe(true, 1, count, false, false),
                    "Absent or duplicate native observations are not a known failed save.");
            Assertions.Equal("native-injury-save-succeeded", observe(true, 1, 1, true, false),
                "A real successful native save must prevent poison; never force failure.");
            Assertions.Equal("venom-missing-after-failed-save", observe(true, 1, 1, false, false),
                "A missing application after a measured failed save is separately observable.");
            Assertions.Equal("venom-after-successful-save", observe(true, 1, 1, true, true),
                "Inconsistent application evidence cannot be mislabeled a clean arming.");
            Assertions.Equal("wounding-bite-failed-save-venom-present", observe(true, 1, 1, false, true),
                "Established exposure requires the entire observed native chain.");
        }

        internal static void PersistenceAppearanceRequiresNativeLocksToEnd()
        {
            Func<bool, float, bool, bool, bool, bool> ready = SerpentinePersistenceReviewPolicy.NativeAppearanceReady;
            foreach (float dissolve in new[] { 0f, .01f, .02f })
                Assertions.True(ready(true, dissolve, true, true, false), "Intact visible and natively free actor can arm.");
            Assertions.False(ready(true, 0f, true, true, true), "Visual fade alone never proves summon lock expiry.");
            Assertions.False(ready(true, 0f, false, true, false), "An action-locked actor cannot arm.");
            Assertions.False(ready(true, 0f, true, false, false), "A movement-locked actor cannot arm.");
            Assertions.False(ready(false, 0f, true, true, false), "Native visibility must settle.");
            foreach (float dissolve in new[] { -.01f, .021f, 1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                Assertions.False(ready(true, dissolve, true, true, false), "Missing, invalid or incomplete dissolve rejects.");
        }

        internal static void PersistenceReceiptRequiresExactOwnedIdentity()
        {
            foreach (string role in SerpentinePersistenceReviewPolicy.Roles)
            {
                string[] valid = { SerpentinePersistenceReviewPolicy.ReceiptScope, role,
                    "owned-unit", "owned-unit", "working-caster", "working-caster", "registered-guid", "registered-guid" };
                Func<string[], bool> owns = values => SerpentinePersistenceReviewPolicy.Owns(
                    values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7]);
                Assertions.True(owns(valid), "Exact receipt, role, live unit, native caster and registered blueprint agree.");
                // Test actual serialized receipt-field round trip independently
                // from native UnitPart reconstruction, which requires runtime.
                var json = new JObject { ["scope"] = valid[0], ["role"] = role,
                    ["unit"] = valid[2], ["caster"] = valid[4] };
                var roundtrip = JObject.Parse(json.ToString());
                Assertions.True(SerpentinePersistenceReviewPolicy.Owns((string)roundtrip["scope"],
                    (string)roundtrip["role"], (string)roundtrip["unit"], valid[3],
                    (string)roundtrip["caster"], valid[5], valid[6], valid[7]), "Marker values survive JSON exactly.");
                for (int field = 0; field < valid.Length; field++)
                foreach (string invalid in new[] { null, "", " ", "foreign", valid[field].ToUpperInvariant() })
                {
                    var changed = (string[])valid.Clone(); changed[field] = invalid;
                    Assertions.False(owns(changed), "An ambiguous or foreign receipt field cannot authorize deletion.");
                }
            }
        }

        internal static void PersistenceFixtureHasOnlySixClosedRoles()
        {
            var roles = SerpentinePersistenceReviewPolicy.Roles;
            Assertions.Equal(6, roles.Length, "Three Sprint17 creatures and three independently marked targets.");
            Assertions.Equal(6, roles.Distinct().Count(), "No ambiguous role identity.");
            Assertions.Equal("snakes", SerpentinePersistenceReviewPolicy.Scope, "One closed persistence request value.");
            foreach (string role in roles)
            {
                string key = SerpentinePersistenceReviewPolicy.CreatureKey(role);
                var creature = ExpandedSummoningCatalog.All.Single(value => value.Key == key);
                Assertions.True(key == "salamander" ? creature.MonsterTier.HasValue : creature.NaturesAllyTier.HasValue,
                    "Every fixture uses its actual available single-summon family.");
                Assertions.Equal(role.EndsWith("-target", StringComparison.Ordinal) ? "wolf" : role, key,
                    "Targets never alias a party member, native hostile or arbitrary creature.");
            }
            foreach (string invalid in new[] { null, "", "Viper", "Salamander", "wolf", "crocodile", "foreign" })
                Assertions.True(SerpentinePersistenceReviewPolicy.CreatureKey(invalid) == null,
                    "Only explicit role tokens authorize a fixture identity.");
            roles[0] = "foreign";
            Assertions.Equal("viper", SerpentinePersistenceReviewPolicy.Roles[0], "Caller cannot mutate the closed role list.");
        }

        internal static void PersistenceVenomCannotResetOrDuplicateCounters()
        {
            for (int tick = 1; tick < 6; tick++)
            {
                Assertions.True(SerpentinePersistenceReviewPolicy.PreservedVenom(1, 13, tick, 0, 13, tick, 0),
                    "A single still-active native poison retains exact counters and DC.");
                Assertions.False(SerpentinePersistenceReviewPolicy.PreservedVenom(1, 13, tick, 0, 13, tick + 1, 0),
                    "A reset, skipped or repeated exposure is not persistence.");
                Assertions.False(SerpentinePersistenceReviewPolicy.PreservedVenom(1, 13, tick, 0, 12, tick, 0),
                    "A changed stored DC fails.");
            }
            foreach (int count in new[] { -1, 0, 2 })
                Assertions.False(SerpentinePersistenceReviewPolicy.PreservedVenom(count, 13, 1, 0, 13, 1, 0), "Exactly one application.");
            foreach (int tick in new[] { -1, 0, 6, 7 })
                Assertions.False(SerpentinePersistenceReviewPolicy.PreservedVenom(1, 13, tick, 0, 13, tick, 0), "Prepared poison is active.");
            Assertions.False(SerpentinePersistenceReviewPolicy.PreservedVenom(1, 14, 1, 0, 14, 1, 0), "Printed Viper DC, not self-consistent wrong data.");
            Assertions.False(SerpentinePersistenceReviewPolicy.PreservedVenom(1, 13, 1, 1, 13, 1, 1), "One successful save must already cure this venom.");
        }

        internal static void CrowdResourcesAreExactInstanceOwned()
        {
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                foreach (int instance in new[] { 1, -12, 123, int.MaxValue, int.MinValue })
                {
                    string mesh = "KMG_" + key + "_Original_" + instance.ToString(
                        System.Globalization.CultureInfo.InvariantCulture);
                    foreach (string resource in new[] { mesh, mesh + "_Albedo",
                        mesh + "_SuppressedGeometry", mesh + " (Instance)", mesh + "_Spear (Instance)" })
                        Assertions.True(SerpentineVisualPolicy.IsSnakeInstanceResource(key, mesh, resource),
                            "Exact owned mesh, texture, auxiliary mesh and instance-cloned materials are counted.");
                    foreach (string foreign in new[] { null, "", "Purple_Worm", mesh + "0",
                        mesh + "Foreign", mesh.ToUpperInvariant(), "KMG_salamander_Original_123" })
                        Assertions.False(SerpentineVisualPolicy.IsSnakeInstanceResource(key, mesh, foreign),
                            "Neither another instance with a shared numeric prefix nor native/foreign resources count.");
                }
                foreach (string invalid in new[] { "", "12 ", "+12", "012", "-0", "1.0", "foreign" })
                    Assertions.False(SerpentineVisualPolicy.IsSnakeInstanceResource(key,
                        "KMG_" + key + "_Original_" + invalid, "KMG_" + key + "_Original_" + invalid),
                        "Noncanonical instance names reject, including missing IDs.");
                // Review uses the same allocated roots after independent
                // publication; it never invents identities or moves tiers.
                var creature = ExpandedSummoningCatalog.All.Single(value => value.Key == key);
                foreach (var quantity in new[] { SummonMultiplicity.One, SummonMultiplicity.OneD4PlusOne })
                {
                    int tier = creature.NaturesAllyTier.Value + (quantity == SummonMultiplicity.One ? 0 : 2);
                    var route = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)
                        .Single(value => value.Creature.Key == key && value.ParentTier == tier &&
                            value.Multiplicity == quantity);
                    Assertions.True(SummonVisibilityCatalog.IsPublished(route),
                        "Qualified original/crowd routes retain their published catalog identities.");
                }
            }
            foreach (string key in new[] { null, "Viper", "salamander", "purple-worm", "foreign" })
                Assertions.False(SerpentineVisualPolicy.IsSnakeInstanceResource(key,
                    "KMG_viper_Original_12", "KMG_viper_Original_12"), "Only the two closed snake identities.");
        }

        internal static void AppearanceSuspensionPreservesNativeAiActions()
        {
            foreach (bool manual in new[] { false, true })
            {
                object first = new object(), second = new object();
                var actions = new System.Collections.Generic.List<object> { first, second };
                int stops = 0, deferrals = 0;
                SerpentineCommandReviewPolicy.SuspendAppearanceDriver(manual,
                    () => { stops++; actions.Clear(); }, () => { deferrals++; });
                Assertions.Equal(manual ? 1 : 0, stops, "Only manual setup empties actions.");
                Assertions.Equal(manual ? 0 : 1, deferrals, "AI setup defers its native scheduler.");
                Assertions.True(manual ? actions.Count == 0 :
                    actions.SequenceEqual(new[] { first, second }), "AI source actions remain the same references.");
            }
        }

        internal static void SnakeFootprintUsesBodyScaleWithoutChangingNativeFloor()
        {
            foreach (float radius in new[] { .25f, .5f, 1f, 3f, 5f, 10f })
            {
                float adjusted = SerpentineVisualPolicy.ScaleSnakeBaseCorpulence(radius);
                Assertions.Equal(radius * .2f, adjusted, "Only the serialized base radius is scaled.");
                foreach (float nativeSizeMultiplier in new[] { .66f, 1f, 1f / .66f })
                    Assertions.Equal(Math.Max(.5f, radius * .2f * nativeSizeMultiplier),
                        Math.Max(.5f, adjusted * nativeSizeMultiplier),
                        "Native getter retains its minimum and later rules-size multiplier.");
            }
            foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity,
                -1f, 0f, float.Epsilon })
                Assertions.Throws<ArgumentOutOfRangeException>(
                    () => SerpentineVisualPolicy.ScaleSnakeBaseCorpulence(invalid),
                    "Malformed/underflowed footprints fail closed.");
        }

        internal static void CommandSetupRequiresIntactOriginalAndNativeControl()
        {
            for (int mask = 0; mask < 32; mask++)
            {
                bool original = (mask & 1) != 0, intact = (mask & 2) != 0, canAct = (mask & 4) != 0;
                bool manual = (mask & 8) != 0, controllable = (mask & 16) != 0;
                bool expected = original && intact && canAct && (!manual || controllable);
                foreach (int frame in new[] { 60, 64, 600 })
                    Assertions.Equal(expected, SerpentineCommandReviewPolicy.ReadyToStart(original,
                        intact, canAct, manual, controllable, frame), "Every body/control prerequisite is mandatory.");
            }
            foreach (int frame in new[] { -1, 0, 30, 59, 601, int.MaxValue })
                Assertions.False(SerpentineCommandReviewPolicy.ReadyToStart(true, true, true, true, true, frame),
                    "No early success or extended settlement bound.");
        }

        internal static void BiteAnimationDistanceIsOwnedVisualOnly()
        {
            string[] clips = { "BiteAttack01_Short_3.5m", "BiteAttack01_Long_8m",
                "BiteAttack02_Short_3.5m", "BiteAttack02_Long_8m" };
            Assertions.True(SerpentineVisualPolicy.IsNativeSnakeBiteAction("Purple_Worm_AnimationSet_Bite 1", clips),
                "Exact preserved native census, in original entry/variant order.");
            foreach (var invalid in new[] { null, new string[0], clips.Take(3), clips.Reverse(), clips.Concat(clips) })
                Assertions.False(SerpentineVisualPolicy.IsNativeSnakeBiteAction("Purple_Worm_AnimationSet_Bite 1", invalid),
                    "Missing, reordered or replacement actions are not normalized.");
            Assertions.False(SerpentineVisualPolicy.IsNativeSnakeBiteAction("foreign", clips), "No action transplant.");
            foreach (string[] identity in new[] {
                new[] { "d8be82543ab64dc988c33e9f13608bad", "KMG_Summoning_Unit_Viper" },
                new[] { "f1a2eadf588e4c3b9fb670724d706364", "KMG_Summoning_Unit_ConstrictorSnake" } })
            {
                foreach (float distance in new[] { 0f, .7f, 1.6f, 1.7096f, 5f })
                {
                    float corrected;
                    Assertions.True(SerpentineVisualPolicy.TrySnakeBiteAnimationDistance(true,
                        identity[0], identity[1], SerpentineVisualPolicy.WormPrefab, true, true,
                        distance, out corrected), "Only exact original-body Bite is projected.");
                    Assertions.Equal(distance / .2f, corrected, "Native clip-space distance matches authored view scale.");
                    foreach (int missing in new[] { 0, 1, 2 })
                    {
                        Assertions.False(SerpentineVisualPolicy.TrySnakeBiteAnimationDistance(missing != 0,
                            identity[0], identity[1], SerpentineVisualPolicy.WormPrefab, missing != 1, missing != 2,
                            distance, out corrected), "Disabled/fallback/non-bite remains native.");
                        Assertions.Equal(distance, corrected, "Rejection preserves input exactly.");
                    }
                }
                foreach (float invalid in new[] { -1f, float.NaN, float.PositiveInfinity, float.MaxValue })
                {
                    float corrected;
                    Assertions.False(SerpentineVisualPolicy.TrySnakeBiteAnimationDistance(true,
                        identity[0], identity[1], SerpentineVisualPolicy.WormPrefab, true, true,
                        invalid, out corrected), "Malformed or overflowing distance is not projected.");
                }
            }
            foreach (string guid in new[] { "bf2216f48b3f4d24c9c502007649340d", "f8fb103168d74b4c93182437e5d2b4e4", "foreign" })
            {
                float corrected;
                Assertions.False(SerpentineVisualPolicy.TrySnakeBiteAnimationDistance(true, guid,
                    "KMG_Summoning_Unit_Viper", SerpentineVisualPolicy.WormPrefab, true, true, 1.7f, out corrected),
                    "Native Worm, Salamander and unrelated identities never match.");
                Assertions.Equal(1.7f, corrected, "Foreign visual distance is unchanged.");
            }
        }

        internal static void CommandRequestAndMatrixAreClosed()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeCommands;
            Assertions.Equal("disposable-expanded-summoning-snake-commands", scenario, "One closed command request.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(scenario) &&
                RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(scenario), "Working-save guards inherited.");
            foreach (string invalid in new[] { scenario.ToUpperInvariant(), scenario + "-arbitrary",
                "working-save-expanded-summoning-snake-commands" })
                Assertions.False(RuntimeTestScenarioCatalog.IsAllowed(invalid), "No scope/write alias.");
            var cells = SerpentineCommandReviewPolicy.Cells();
            Assertions.Equal(8, cells.Length, "Both creatures, modes and drivers.");
            Assertions.Equal("viper-rtwp-manual,viper-rtwp-ai,viper-turn-based-manual,viper-turn-based-ai," +
                "constrictor-snake-rtwp-manual,constrictor-snake-rtwp-ai,constrictor-snake-turn-based-manual,constrictor-snake-turn-based-ai",
                string.Join(",", cells.Select(value => string.Join("-", value))), "Closed ordered matrix.");
            cells[0][0] = "foreign";
            Assertions.Equal("viper", SerpentineCommandReviewPolicy.Cells()[0][0], "No mutable global matrix.");
        }

        internal static void FinalReviewRequestIsClosed()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeFinalReview;
            Assertions.Equal("disposable-expanded-summoning-snake-final-review", scenario, "One bounded request.");
            Assertions.True(SerpentineFinalReviewPolicy.ValidExit(scenario, true), "Bounded protocol exits.");
            Assertions.False(SerpentineFinalReviewPolicy.ValidExit(scenario, false), "No live fixture handoff.");
            Assertions.True(SerpentineFinalReviewPolicy.ValidExit(
                RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeCommands, false), "Other request exit contracts unchanged.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(scenario) &&
                RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(scenario), "Existing working-save guards.");
            foreach (string invalid in new[] { scenario.ToUpperInvariant(), scenario + "-all",
                "working-save-expanded-summoning-snake-final-review" })
                Assertions.False(RuntimeTestScenarioCatalog.IsAllowed(invalid), "No arbitrary scope or save authority.");
        }

        internal static void FinalReviewRoutesAndQuantitiesAreExact()
        {
            var routes = SerpentineFinalReviewPolicy.Routes();
            Assertions.Equal(32, routes.Length, "All and only snake roots.");
            Assertions.Equal(18, routes.Count(value => value.Creature.Key == "viper"), "Viper roots.");
            Assertions.Equal(14, routes.Count(value => value.Creature.Key == "constrictor-snake"), "Constrictor roots.");
            Assertions.Equal(32, routes.Select(value => value.StableKey).Distinct().Count(), "No duplicated route.");
            Assertions.True(routes.All(SummonVisibilityCatalog.IsPublished), "Exact independent publication keys.");
            var published = SerpentineFinalReviewPolicy.PublishedRoutes();
            Assertions.Equal(37, published.Length, "32 new roots plus five unchanged Salamander roots only.");
            Assertions.Equal(5, published.Count(v => v.Creature.Key == "salamander"), "Preserved Salamander identity and placements.");
            Assertions.Equal(37, published.Select(v => v.StableKey).Distinct().Count(), "No publication root duplicates.");
            Assertions.True(published.All(v => v.Creature.Key == "salamander" || SerpentineRulesPolicy.IsSnake(v.Creature.Key)),
                "No unrelated player-path census in the publication gate.");
            foreach (SummonMultiplicity kind in Enum.GetValues(typeof(SummonMultiplicity)))
                for (int count = -1; count <= 6; count++)
                    Assertions.Equal(kind == SummonMultiplicity.One ? count == 1 :
                        kind == SummonMultiplicity.OneD3 ? count >= 1 && count <= 3 :
                        kind == SummonMultiplicity.OneD4PlusOne && count >= 2 && count <= 5,
                        SerpentineFinalReviewPolicy.Quantity(kind, count), "Every exact quantity boundary.");
            Assertions.False(SerpentineFinalReviewPolicy.Quantity((SummonMultiplicity)99, 1), "Unknown quantity fails closed.");
        }

        internal static void FinalReviewDurationMatchesNativeRtwp()
        {
            Func<int, int, bool, bool, int, bool, double, double, double, bool> valid =
                SerpentineFinalReviewPolicy.PrivateRtwpDuration;
            Assertions.True(valid(1, 1, false, true, 20, false, 120, 0, 120), "Paused RTWP native duration.");
            Assertions.True(valid(1, 1, false, true, 20, false, 120, 12, 132), "Native bonus remains counted.");
            Assertions.False(valid(1, 1, false, true, 20, false, 120, 0, 126), "No invented TB-only grace.");
            Assertions.False(valid(1, 1, true, true, 20, false, 120, 0, 120), "Not a TB fixture.");
            Assertions.False(valid(1, 1, false, false, 20, false, 120, 0, 120), "Exact source context.");
            Assertions.False(valid(1, 1, false, true, 19, false, 120, 0, 120), "Exact caster level.");
            Assertions.False(valid(1, 1, false, true, 20, true, 120, 0, 120), "No permanent state.");
            foreach (int count in new[] { 0, 2 })
            {
                Assertions.False(valid(count, 1, false, true, 20, false, 120, 0, 120), "One native rule.");
                Assertions.False(valid(1, count, false, true, 20, false, 120, 0, 120), "One lifecycle fact.");
            }
            foreach (double bad in new[] { -1d, double.NaN, double.PositiveInfinity })
            {
                Assertions.False(valid(1, 1, false, true, 20, false, bad, 0, 120), "Finite exact base.");
                Assertions.False(valid(1, 1, false, true, 20, false, 120, bad, 120), "Finite nonnegative bonus.");
                Assertions.False(valid(1, 1, false, true, 20, false, 120, 0, bad), "Finite exact remaining.");
            }
            Assertions.False(valid(1, 1, false, true, 20, false, 119, 0, 119), "No shortened base.");
            Assertions.False(valid(1, 1, false, true, 20, false, 120, 0, 119), "No elapsed/mutated duration.");
        }

        internal static void FinalReviewLifecycleRejectsForeignCombat()
        {
            for (int mask = 0; mask < 64; mask++)
            {
                bool owned = (mask & 1) != 0, player = (mask & 2) != 0, party = (mask & 4) != 0,
                    a = (mask & 8) != 0, b = (mask & 16) != 0;
                int foreign = (mask & 32) != 0 ? 1 : 0;
                Assertions.Equal(owned && !player && !party && a && b && foreign == 0,
                    SerpentineFinalReviewPolicy.IsolatedLifecyclePair(owned, player, party, a, b, foreign),
                    "Only distinct isolated native enemies enter the hit drill.");
            }
        }

        internal static void ConstrictorPassiveIconHasOneExactConsumer()
        {
            Assertions.Equal("constrictor-snake",
                SummonIconCatalog.PassiveTraitIconFor(SummonIconCatalog.ConstrictorTraitsSymbol),
                "Existing original species painting intentionally marks its passive trait.");
            foreach (string other in new[] { null, "", SummonIconCatalog.ConstrictorTraitsSymbol.ToUpperInvariant(),
                "KMG.Summoning.Special.MonitorLizard.CombatTraits", "KMG.Summoning.Natural.Viper.Venom" })
                Assertions.True(SummonIconCatalog.PassiveTraitIconFor(other) == null, "No native or unrelated remapping.");
        }

        internal static void FinalReviewRequiresActualNativePlayback()
        {
            Assertions.True(SerpentineFinalReviewPolicy.PlayedNativeClip(true, true, "native", 1f, .2, .5f),
                "Positive native clip/time/weight.");
            Assertions.False(SerpentineFinalReviewPolicy.PlayedNativeClip(false, true, "native", 1f, .2, .5f), "Exact native action.");
            Assertions.False(SerpentineFinalReviewPolicy.PlayedNativeClip(true, false, "native", 1f, .2, .5f), "Actually started.");
            foreach (string name in new[] { null, "" })
                Assertions.False(SerpentineFinalReviewPolicy.PlayedNativeClip(true, true, name, 1f, .2, .5f), "No IsActed-only fallback.");
            foreach (float invalid in new[] { -1f, 0f, float.NaN, float.PositiveInfinity })
            {
                Assertions.False(SerpentineFinalReviewPolicy.PlayedNativeClip(true, true, "native", invalid, .2, .5f), "Finite positive duration.");
                Assertions.False(SerpentineFinalReviewPolicy.PlayedNativeClip(true, true, "native", 1f, invalid, .5f), "Finite elapsed time.");
                Assertions.False(SerpentineFinalReviewPolicy.PlayedNativeClip(true, true, "native", 1f, .2, invalid), "Nonzero actual weight.");
            }
            Assertions.False(SerpentineFinalReviewPolicy.PlayedNativeClip(true, true, "native", 1f, .2, 2f), "No impossible weight.");
        }

        internal static void FinalReviewRequiresNativeFrontalHitEligibility()
        {
            foreach (float frontal in new[] { .3f, .5f, 1f })
                Assertions.True(SerpentineFinalReviewPolicy.NativeFrontalHit(true, frontal),
                    "Native float dot is promoted before the double 0.3 comparison.");
            foreach (float other in new[] { -1f, 0f, .25f, .29999998f, 2f, float.NaN, float.PositiveInfinity })
                Assertions.False(SerpentineFinalReviewPolicy.NativeFrontalHit(true, other),
                    "Rear, side, below-threshold or invalid geometry cannot qualify a frontal hit drill.");
            Assertions.False(SerpentineFinalReviewPolicy.NativeFrontalHit(false, 1f), "A miss cannot request Hit.");
            Assertions.False(SerpentineFinalReviewPolicy.PlayedNativeClip(true, false, "Hit", 1f, .1, 1f),
                "Eligible facing does not waive actual native playback.");
        }

        internal static void FinalReviewPreservesNativeHitCarrierContract()
        {
            for (int mask = 0; mask < 128; mask++)
            {
                bool exact = (mask & 1) != 0, native = (mask & 2) != 0, candidate = (mask & 4) != 0,
                    wound = (mask & 8) != 0, finite = (mask & 16) != 0, alive = (mask & 32) != 0,
                    played = (mask & 64) != 0;
                bool expected = exact && native == candidate && wound && finite && alive && native == played;
                Assertions.Equal(expected, SerpentineFinalReviewPolicy.FaithfulHitLifecycle(
                    exact, native, candidate, wound, finite, alive, played),
                    "No missing carrier invented; present native carrier still needs real playback, real wound and stability.");
            }
        }

        internal static void CommandRetryNeverDrivesAiOrReplaysHeldAttack()
        {
            for (int attempt = 0; attempt < 4; attempt++)
                Assertions.True(SerpentineCommandReviewPolicy.CanIssueManual(true, true, attempt,
                    false, false, false, true), "Bounded completed real-command retries.");
            for (int mask = 0; mask < 64; mask++)
            {
                bool manual = (mask & 1) != 0, ready = (mask & 2) != 0, pending = (mask & 4) != 0;
                bool signature = (mask & 8) != 0, held = (mask & 16) != 0, alive = (mask & 32) != 0;
                Assertions.Equal(manual && ready && !pending && !signature && !held && alive,
                    SerpentineCommandReviewPolicy.CanIssueManual(manual, ready, 1, pending, signature, held, alive),
                    "Every readiness/control/relationship gate is mandatory.");
            }
            foreach (int invalid in new[] { -1, 4, 99 })
                Assertions.False(SerpentineCommandReviewPolicy.CanIssueManual(true, true, invalid,
                    false, false, false, true), "Attempt bound is closed.");
        }

        internal static void CommandContactRequiresPlayedClipAndActualGap()
        {
            Assertions.True(SerpentineCommandReviewPolicy.Contact(true, true, false, true, 50, .25f), "Exact threshold.");
            foreach (float gap in new[] { -.01f, .251f, float.NaN, float.PositiveInfinity })
                Assertions.False(SerpentineCommandReviewPolicy.Contact(true, true, false, true, 50, gap),
                    "No distant/nonfinite contact waiver.");
            Assertions.False(SerpentineCommandReviewPolicy.Contact(false, true, false, true, 50, 0), "Exact actors.");
            Assertions.False(SerpentineCommandReviewPolicy.Contact(true, false, false, true, 50, 0), "Actual command.");
            Assertions.False(SerpentineCommandReviewPolicy.Contact(true, true, true, true, 50, 0), "No incidental AoO.");
            Assertions.False(SerpentineCommandReviewPolicy.Contact(true, true, false, false, 50, 0), "No fallback IsActed.");
            Assertions.False(SerpentineCommandReviewPolicy.Contact(true, true, false, true, 0, 0), "Actual original points.");
        }

        internal static void CommandMaintainRequiresLaterRoundWithoutSecondAttack()
        {
            Assertions.True(SerpentineCommandReviewPolicy.LaterMaintain(1, 1, 2, 1, 1), "One initial, two later bundles.");
            foreach (int initial in new[] { 0, 2 })
                Assertions.False(SerpentineCommandReviewPolicy.LaterMaintain(initial, 1, 2, 1, 1), "Exact initial constrict.");
            foreach (int later in new[] { 0, 1, 3, 4 })
                Assertions.False(SerpentineCommandReviewPolicy.LaterMaintain(1, 1, later, 1, 1), "Exact later bite+constrict.");
            Assertions.False(SerpentineCommandReviewPolicy.LaterMaintain(1, 0, 2, 1, 1), "Not application frame.");
            Assertions.False(SerpentineCommandReviewPolicy.LaterMaintain(1, 1, 2, 0, 0), "Actual establishing attack.");
            Assertions.False(SerpentineCommandReviewPolicy.LaterMaintain(1, 1, 2, 1, 2), "No second attack credited as maintain.");
        }

        internal static void RenderedContactRequiresExactEventFrameAndIdentities()
        {
            for (int mask = 0; mask < 16; mask++)
                foreach (int frame in new[] { -1, 0, 10, 11, 12, int.MaxValue })
                    Assertions.Equal(frame == 11 && mask == 15,
                        SerpentineCommandReviewPolicy.SameRenderedAttack(11, frame,
                            (mask & 1) != 0, (mask & 2) != 0, (mask & 4) != 0, (mask & 8) != 0),
                        "No earlier/later frame, replacement command, handle, animation or clip is correlated.");
            Assertions.False(SerpentineCommandReviewPolicy.SameRenderedAttack(-1, -1, true, true, true, true),
                "Absent frame identity is not evidence.");
            Assertions.True(SerpentineCommandReviewPolicy.SameRenderedAttack(0, 0, true, true, true, true),
                "Frame zero is valid when all exact identities match.");
        }

        internal static void NativePoisonSavePhasesRemainDistinct()
        {
            for (int exposure = 1; exposure <= 6; exposure++)
            {
                Assertions.True(SerpentinePoisonReviewPolicy.ExactExposureCounts(exposure,
                    exposure, 1, exposure - 1), "One injury gate, then native buff saves.");
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExposureCounts(exposure,
                    exposure, 0, exposure), "Buff saves cannot substitute for the injury gate.");
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExposureCounts(exposure,
                    exposure, 1, exposure), "No invented second save on initial application.");
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExposureCounts(exposure,
                    exposure + 1, 1, exposure - 1), "Duplicate damage remains a failure.");
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExposureCounts(exposure,
                    exposure, 2, exposure - 1), "Duplicate injury gate remains a failure.");
            }
            foreach (int invalid in new[] { -1, 0, 7, int.MaxValue })
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExposureCounts(invalid,
                    invalid, 1, invalid - 1), "Only six native exposures are in scope.");
        }

        internal static void NativePoisonExhaustionRequiresRemovalWithoutReplay()
        {
            Assertions.True(SerpentinePoisonReviewPolicy.ExactExhaustion(6, 1, 5, false, 0, 0),
                "Six total saves/damage events, split into their actual native phases.");
            foreach (int damage in new[] { -1, 1, 2 })
            {
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExhaustion(6, 1, 5, false, damage, 0),
                    "Exhaustion cannot mutate Constitution.");
                Assertions.False(SerpentinePoisonReviewPolicy.ExactExhaustion(6, 1, 5, false, 0, damage),
                    "Duplicate exhausted callback cannot mutate Constitution.");
            }
            Assertions.False(SerpentinePoisonReviewPolicy.ExactExhaustion(6, 1, 5, true, 0, 0),
                "A retained exhausted buff is not removal.");
            Assertions.False(SerpentinePoisonReviewPolicy.ExactExhaustion(7, 1, 6, false, 0, 0),
                "A seventh event fails even if its damage was masked.");
            Assertions.False(SerpentinePoisonReviewPolicy.ExactExhaustion(6, 0, 5, false, 0, 0),
                "The initial injury save must be positively observed.");
        }

        internal static void SignatureRequestIsClosedWorkingSaveSlice()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeSignatures;
            Assertions.Equal("disposable-expanded-summoning-snake-signatures", scenario, "Closed native rules slice.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(scenario) &&
                RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(scenario),
                "The new request retains native working-save guards.");
            foreach (string invalid in new[] { scenario.ToUpperInvariant(), scenario + "-arbitrary",
                "working-save-expanded-summoning-snake-signatures" })
                Assertions.False(RuntimeTestScenarioCatalog.IsAllowed(invalid) ||
                    RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(invalid),
                    "No write-enabled alias or alternate asset selector.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(
                RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeProfiles),
                "The qualified closed38-check request remains independently available.");
        }

        internal static void ProfileRequestIsClosedWorkingSaveSlice()
        {
            string scenario = RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeProfiles;
            Assertions.Equal("disposable-expanded-summoning-snake-profiles", scenario, "One bounded profile/body request.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(scenario) &&
                RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(scenario),
                "Native request traverses the exact working-save guard.");
            foreach (string other in new[] { null, "", scenario.ToUpperInvariant(), scenario + "-arbitrary",
                "working-save-expanded-summoning-snake-profiles" })
                Assertions.False(RuntimeTestScenarioCatalog.IsAllowed(other) ||
                    RuntimeTestScenarioCatalog.IsExpandedSummoningRulesScenario(other),
                    "No alternate spelling, write authority or arbitrary creature scope.");
        }

        internal static void ProductionBodyHookRequiresExactHiddenSnakeIdentity()
        {
            JArray entries = (JArray)JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json")))["entries"];
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                string token = key == "viper" ? "Viper" : "ConstrictorSnake";
                string guid = (string)entries.Single(e =>
                    (string)e["symbol"] == "KMG.Summoning.Unit." + token)["guid"];
                string name = "KMG_Summoning_Unit_" + token, found;
                Assertions.True(SerpentineVisualPolicy.TryProductionSnake(true, guid, name,
                    SerpentineVisualPolicy.WormPrefab, out found) && found == key,
                    "The append-only identity resolves its own original body.");
                Assertions.False(SerpentineVisualPolicy.TryProductionSnake(false, guid, name,
                    SerpentineVisualPolicy.WormPrefab, out found), "Module-disabled has no attachment.");
                foreach (string bad in new[] { null, "", "foreign", guid.ToUpperInvariant(),
                    "bf2216f48b3f4d24c9c502007649340d", "f8fb103168d74b4c93182437e5d2b4e4" })
                    Assertions.False(SerpentineVisualPolicy.TryProductionSnake(true, bad, name,
                        SerpentineVisualPolicy.WormPrefab, out found), "Name alone cannot capture a donor/hybrid.");
                foreach (string bad in new[] { null, "", name.ToLowerInvariant(),
                    "KMG_Summoning_Unit_PurpleWorm", "KMG_Summoning_Unit_Salamander" })
                    Assertions.False(SerpentineVisualPolicy.TryProductionSnake(true, guid, bad,
                        SerpentineVisualPolicy.WormPrefab, out found), "Exact KMG identity/name pair required.");
                foreach (string bad in new[] { null, "", SerpentineVisualPolicy.ClubShieldPrefab,
                    SerpentineVisualPolicy.TwoHandPrefab, SerpentineVisualPolicy.WormPrefab.ToUpperInvariant() })
                    Assertions.False(SerpentineVisualPolicy.TryProductionSnake(true, guid, name, bad, out found),
                        "Reject an unreviewed replacement prefab, even on our identity.");
                Assertions.True(found == null, "Rejected dispatch must not leak the prior key.");
                float multiplier;
                Assertions.False(SummonViewScaleCatalog.TryGetMultiplier(name, out multiplier),
                    "The owned hook applies scale once; the shared hook must not multiply it again.");
                Assertions.Equal("Medium", ExpandedSummoningNaturalProfiles.For(key).Size,
                    "Visual binding does not change mechanical size.");
            }
            Assertions.Equal(.2f, SerpentineVisualPolicy.SnakeViewMultiplier,
                "Same view-only scale as the measured native snake bite research, pending production proof.");
            Assertions.Equal(17, SummonViewScaleCatalog.All.Count,
                "All prior production view steps remain unchanged.");
        }

        internal static void LedgerAppendPreservesEveryHistoricalEntry()
        {
            JArray entries = (JArray)JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json")))["entries"];
            var masterTraits = new System.Collections.Generic.HashSet<string>(
                KingmakerGunslinger.ElementalRaces.ElementalCharacterTraitCatalog.Nodes()
                    .Select(value => value.Symbol), StringComparer.Ordinal);
            Assertions.Equal(2836 + AppendedLedgerIdentities +
                PrimateRulesTests.AppendedLedgerIdentities +
                Sprint19RulesTests.AppendedLedgerIdentities +
                Sprint20RulesTests.AppendedLedgerIdentities + masterTraits.Count,
                entries.Count,
                "Qualified Phase2B identities, the Sprint 18 appends and all eleven released master trait identities coexist.");
            // The current release preserves master's entire prefix. Project
            // only its exact eleven independently appended trait nodes out
            // when checking the historical Phase2B order/hash; never remove
            // arbitrary entries or weaken the historical identity contract.
            JArray historical = new JArray(entries.Where(value =>
                !masterTraits.Contains((string)value["symbol"])));
            string digest;
            using (var sha = SHA256.Create())
                digest = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(
                    new JArray(historical.Take(2836)).ToString(Newtonsoft.Json.Formatting.None))))
                    .Replace("-", "").ToLowerInvariant();
            Assertions.Equal("bf46e4e3d2d709935f2a27fa32f2bd8ad640098801174680c755c89287ea0570",
                digest, "Every prior symbol, GUID, type, status and historical note stays intact.");
            foreach (string[] row in new[] {
                new[] { "KMG.Summoning.Unit.Viper", "d8be82543ab64dc988c33e9f13608bad" },
                new[] { "KMG.Summoning.Unit.ConstrictorSnake", "f1a2eadf588e4c3b9fb670724d706364" } })
                Assertions.Equal(row[1], (string)entries.Single(e => (string)e["symbol"] == row[0])["guid"],
                    "Allocated snake identity is permanent even while withheld.");
        }
        internal static void PrintedProfilesAreMediumAndCreatureOwned()
        {
            var viper = ExpandedSummoningNaturalProfiles.For("viper");
            var snake = ExpandedSummoningNaturalProfiles.For("constrictor-snake");
            foreach (var profile in new[] { viper, snake })
            {
                Assertions.Equal("Medium", profile.Size, "Frozen contract uses Medium snakes.");
                Assertions.Equal("Animal", profile.HitDieClass, "No Purple Worm magical-beast progression.");
                Assertions.Equal(20, profile.SpeedFeet, "Land speed is printed 20 feet.");
                Assertions.Equal("Bite1d4", profile.PrimaryWeapon, "One printed bite.");
                Assertions.Equal(0, profile.AdditionalWeapons.Count, "No donor sting.");
                Assertions.Equal(0, profile.AdditionalSecondaryWeapons.Count, "No donor tail or sting.");
                Assertions.True(profile.Facts.Contains("TripImmune"), "Legless trip defense.");
                Assertions.False(profile.Facts.Contains("PurpleWormPoison"), "No worm state leakage.");
            }
            Assertions.Equal("2|8|13|14|1|13|2|3", Row(viper), "Printed Viper chassis.");
            Assertions.Equal("3|17|17|12|1|12|2|2", Row(snake), "Printed Constrictor chassis.");
            Assertions.True(viper.Facts.Contains("ImprovedInitiative") &&
                viper.Facts.Contains("WeaponFinesse"), "Printed Viper feats, including bonus finesse.");
            Assertions.True(snake.Facts.Contains("SkillFocusPerception") &&
                snake.Facts.Contains("Toughness"), "Printed Constrictor feats.");
        }

        private static string Row(NaturalSummonProfile p)
        { return string.Join("|", new[] { p.HitDice, p.Strength, p.Dexterity,
            p.Constitution, p.Intelligence, p.Wisdom, p.Charisma, p.NaturalArmor }); }

        internal static void ExactRanksPreserveNativeContributions()
        {
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                int m = 0, p = 0, s = 0;
                SerpentineRulesPolicy.AllocateLandRanks(key, ref m, ref p, ref s);
                bool viper = key == "viper";
                int dex = viper ? 1 : 3, wis = 1, focus = viper ? 0 : 3;
                Assertions.Equal(viper ? 9 : 15,
                    m + (m > 0 ? 3 : 0) + dex + SerpentineRulesPolicy.MobilityRacialBonus,
                    "Mobility is ranks + native class + Dexterity + racial.");
                Assertions.Equal(viper ? 9 : 12,
                    p + 3 + wis + focus + SerpentineRulesPolicy.PerceptionRacialBonus,
                    "Perception includes only the printed feat.");
                Assertions.Equal(viper ? 9 : 11,
                    s + 3 + dex + SerpentineRulesPolicy.StealthRacialBonus,
                    "Land Stealth; no aquatic bonus.");
                Assertions.Equal(viper ? 13 : 19,
                    SerpentineRulesPolicy.For(key).BaseHitPoints +
                    (viper ? 2 * 2 : 3 * 1 + 3), "Native Constitution/Toughness complete printed HP.");
            }
        }

        internal static void RankAllocationRejectsDonorOrRepeatRanks()
        {
            foreach (string key in new[] { "viper", "constrictor-snake" })
            foreach (int dirtyIndex in new[] { 0, 1, 2 })
            foreach (int donorRanks in new[] { 1, 5 })
            {
                int m = dirtyIndex == 0 ? donorRanks : 0, p = dirtyIndex == 1 ? donorRanks : 0,
                    s = dirtyIndex == 2 ? donorRanks : 0;
                Assertions.Throws<InvalidOperationException>(() =>
                    SerpentineRulesPolicy.AllocateLandRanks(key, ref m, ref p, ref s),
                    "Unexpected ranks must fail, not be overwritten silently.");
                Assertions.Equal(donorRanks, m + p + s,
                    "The observed donor five-rank seed is rejected atomically, not canceled with a hidden bonus.");
            }
            foreach (string key in new[] { null, "", "salamander", "purple-worm" })
                Assertions.Throws<ArgumentException>(() => SerpentineRulesPolicy.For(key),
                    "Policy must not bleed into a donor or hybrid.");
        }

        internal static void PoisonAndDamageUseLiveModifiers()
        {
            Assertions.Equal(13, SerpentineRulesPolicy.ViperPoisonDifficultyClass(2), "Printed DC.");
            Assertions.Equal(15, SerpentineRulesPolicy.ViperPoisonDifficultyClass(4), "Live Con increase.");
            Assertions.Equal(10, SerpentineRulesPolicy.ViperPoisonDifficultyClass(-1), "Live Con penalty.");
            Assertions.Equal(6, SerpentineRulesPolicy.ViperPoisonExposures, "Six total exposures.");
            Assertions.Equal(1, SerpentineRulesPolicy.ViperPoisonSavesToCure, "One native cure save.");
            foreach (int[] row in new[] { new[] { -3, -3 }, new[] { -1, -1 },
                new[] { 0, 0 }, new[] { 1, 1 }, new[] { 3, 4 }, new[] { 5, 7 } })
                Assertions.Equal(row[1], SerpentineRulesPolicy.SingleNaturalDamageBonus(row[0]),
                    "Only a positive Strength bonus gains an extra half.");
        }

        internal static void WeaponSizeDropsDonorSizeButKeepsLiveShift()
        {
            foreach (int donorSize in new[] { 2, 4, 7, 8 })
            foreach (int bodySize in new[] { 3, 4, 5 })
            {
                Assertions.Equal(bodySize, SerpentineRulesPolicy.LiveWeaponSize(bodySize, donorSize, donorSize),
                    "Donor size is not a second body-size multiplier.");
                Assertions.Equal(bodySize + 1, SerpentineRulesPolicy.LiveWeaponSize(bodySize, donorSize, donorSize + 1),
                    "Legitimate native weapon-size shifts survive.");
            }
            Assertions.Equal(2, SerpentineRulesPolicy.LiveWeaponSize(2, 8, 2), "Native lower bound.");
            Assertions.Equal(8, SerpentineRulesPolicy.LiveWeaponSize(8, 2, 8), "Native upper bound.");
        }

        internal static void QualifiedSnakesPublishWithoutMovingExistingChoices()
        {
            var all = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)).ToArray();
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                var rows = all.Where(v => v.Creature.Key == key).ToArray();
                Assertions.Equal(key == "viper" ? 18 : 14, rows.Length, "Exact registered placement count.");
                Assertions.True(rows.All(SummonVisibilityCatalog.IsPublished), "Only exact independently qualified roots publish.");
                Assertions.True(rows.Select(v => v.Multiplicity).Distinct().Count() == 3,
                    "Direct, 1d3 and 1d4+1 routes retain their allocated identities.");
            }
            Assertions.Equal(1044, all.Count(SummonVisibilityCatalog.IsPublished), "Exactly 32 newly published roots.");
            Assertions.Equal(1073, all.Count(SummonVisibilityCatalog.IsPublished) +
                SummonNativeExpansionCatalog.All.Count, "Published surface is source-derived.");
            Assertions.True(all.Where(v => !SerpentineRulesPolicy.IsSnake(v.Creature.Key) &&
                    !PrimateRulesPolicy.IsPrimate(v.Creature.Key) &&
                    !KingmakerGunslinger.RuntimeTesting.Sprint19ReviewPolicy
                        .IsSprint19Creature(v.Creature.Key) &&
                    v.Creature.Key != GiantScorpionRulesPolicy.GiantScorpionKey)
                .All(SummonVisibilityCatalog.IsPublished), "No old publication or Salamander root moved.");
        }

        internal static void SnakeArtworkHasExactConsumersAndProvenance()
        {
            JObject manifest = JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "assets-source", "original-icons", "expanded-summoning", "icon-manifest.json")));
            var icons = (JArray)manifest["icons"];
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                JToken row = icons.Single(v => (string)v["key"] == key);
                string token = key == "viper" ? "Viper" : "ConstrictorSnake";
                string[] consumers = ((JArray)row["blueprintSymbols"]).Values<string>().ToArray();
                Assertions.True(consumers.Contains("KMG.Summoning.Unit." + token), "Owned unit painting.");
                Assertions.True(consumers.Contains("KMG.Summoning.Natural." + token + ".UnitType"), "Owned inspection painting.");
                Assertions.Equal(key == "viper" ? 38 : 31, consumers.Length,
                    "Unit/type plus exact logical and SM template consumers.");
                if (key == "constrictor-snake")
                    Assertions.True(consumers.Contains(SummonIconCatalog.ConstrictorTraitsSymbol),
                        "Inspectable passive trait has a cataloged original icon.");
                Assertions.Equal(128, (int)row["width"], "Native-sized export.");
                Assertions.Equal(128, (int)row["height"], "Native-sized export.");
                Assertions.True(icons.All(other => ReferenceEquals(other, row) ||
                    (string)other["outputSha256"] != (string)row["outputSha256"]), "No unrelated art reuse.");
            }
        }
    }
}
