using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint11Tests
    {
        internal const int HiddenUngulateIdentityCount = 100;
        internal const int PowerfulChargeIdentityCount = 2;
        internal const int TrampleIdentityCount = 3;
        internal const int AppendedLedgerIdentities =
            HiddenUngulateIdentityCount + PowerfulChargeIdentityCount +
            TrampleIdentityCount;

        internal static void FourUngulatesRegisterAtPrintedTiersButRemainHidden()
        {
            var expected = new[] {
                new { Key = "aurochs", Tier = 3, Hd = 3, Strength = 23,
                    Speed = 40, Armor = 4, Weapon = "Gore1d8" },
                new { Key = "bison", Tier = 4, Hd = 5, Strength = 27,
                    Speed = 40, Armor = 8, Weapon = "Gore2d6" },
                new { Key = "rhinoceros", Tier = 4, Hd = 5, Strength = 22,
                    Speed = 40, Armor = 7, Weapon = "Gore2d6" },
                new { Key = "woolly-rhinoceros", Tier = 5, Hd = 8,
                    Strength = 28, Speed = 30, Armor = 10, Weapon = "Gore2d8" }
            };
            var all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            foreach (var value in expected)
            {
                SummonCreatureSpec creature = ExpandedSummoningCatalog.All
                    .Single(item => item.Key == value.Key);
                NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                    .For(value.Key);
                Assertions.True(creature.MonsterTier == value.Tier &&
                    creature.NaturesAllyTier == value.Tier &&
                    creature.MonsterTemplated && profile.HitDieClass == "Animal" &&
                    profile.HitDice == value.Hd && profile.Size == "Large" &&
                    profile.Strength == value.Strength &&
                    profile.SpeedFeet == value.Speed &&
                    profile.NaturalArmor == value.Armor &&
                    profile.PrimaryWeapon == value.Weapon &&
                    profile.Facts.Contains("ReducedReach"),
                    "The hidden " + value.Key + " has its own printed animal profile.");
                var variants = all.Where(item => item.Creature.Key == value.Key)
                    .ToArray();
                int perFamily = 10 - value.Tier;
                Assertions.True(variants.Length == perFamily * 2 &&
                    variants.All(item => !SummonVisibilityCatalog.IsPublished(item)),
                    "Every placement of " + value.Key +
                    " remains registered but hidden before qualification.");
            }
            Assertions.True(all.Length == 882 &&
                all.Count(SummonVisibilityCatalog.IsPublished) == 834,
                "The 48 new placements must not change the accepted menu.");
        }

        internal static void FourUngulatesHaveHiddenQuantityPlacementsInBothFamilies()
        {
            string[] keys = {
                "aurochs", "bison", "rhinoceros", "woolly-rhinoceros"
            };
            var variants = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            foreach (string key in keys)
                foreach (SummonFamily family in new[] {
                    SummonFamily.Monster, SummonFamily.NaturesAlly
                })
                    foreach (SummonMultiplicity quantity in new[] {
                        SummonMultiplicity.OneD3,
                        SummonMultiplicity.OneD4PlusOne
                    })
                    {
                        var placements = variants.Where(value =>
                            value.Creature.Key == key &&
                            value.Family == family &&
                            value.Multiplicity == quantity).ToArray();
                        Assertions.True(placements.Length > 0 &&
                            placements.All(value =>
                                !SummonVisibilityCatalog.IsPublished(value)),
                            key + " must have a hidden " + family + "/" +
                            quantity + " quantity route for live qualification.");
                    }
        }

        internal static void HiddenRhinosOwnDistinctPowerfulChargeFacts()
        {
            string[] symbols = {
                "KMG.Summoning.Special.Rhinoceros.PowerfulCharge",
                "KMG.Summoning.Special.WoollyRhinoceros.PowerfulCharge"
            };
            var identities = ExpandedSummoningIdentityCatalog.Build();
            Assertions.True(symbols.All(symbol => identities.Count(item =>
                item.Symbol == symbol && item.PlannedType ==
                    "BlueprintFeature") == 1),
                "Each hidden Rhino must own one distinct charge feature identity.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            Assertions.True(builder.Contains(
                    "ConfigureUngulatePowerfulCharge(bySymbol, RhinocerosUnitSymbol,") &&
                builder.Contains(
                    "ConfigureUngulatePowerfulCharge(bySymbol, WoollyRhinocerosUnitSymbol,") &&
                builder.Contains("UngulateRulesPolicy.For(creatureKey)") &&
                builder.Contains("charge.Gore = gore;") &&
                builder.Contains("charge.AdditionalDiceRolls = rules.ChargeDiceIncrement;") &&
                builder.Contains("charge.AdditionalDamageBonus = rules.ChargeBonusIncrement;") &&
                builder.Contains(".Concat(new BlueprintUnitFact[] { feature }).ToArray()"),
                "Both owned facts must use the printed per-species increment and exact primary gore.");
            string charge = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "UngulatePowerfulCharge.cs"));
            Assertions.True(charge.Contains("evt.DoNotScaleDamage = true;") &&
                charge.Contains("ReferenceEquals(evt.Weapon.Blueprint, Gore)"),
                "The exact gore's printed charge dice must bypass Kingmaker's second size scale.");
            string fixture = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.ExpandedSummoningCorrection.cs"));
            Assertions.True(fixture.Contains("Rulebook.Trigger(first);") &&
                fixture.Contains("realHit && realDamage > 0") &&
                fixture.Contains("fixture.Hostile.Descriptor.Damage = damageBefore;"),
                "The guarded charge fixture must land a real attack and restore its disposable target.");
            Assertions.True(fixture.Contains(
                    "BeginExpandedSummoningQueuedRhinoCharge();") &&
                fixture.Contains("value.AssetGuid == \"c78506dd0e14f7c45a599990e4e65038\"") &&
                fixture.Contains("_rulesChargeCommand = BeginExpandedSummoningDetachedAbility(") &&
                fixture.Contains("_rulesChargeRhino.View.MovementAgent.TickMovement(delta);") &&
                fixture.Contains("_rulesChargeRhino.Commands.Raw") &&
                fixture.Contains("_rulesChargeAttackCommand.Start();") &&
                fixture.Contains("_rulesChargeObserver.FirstChargeHitWithMarker") &&
                fixture.Contains("CleanupExpandedSummoningQueuedRhinoCharge();"),
                "The guarded command case must drive native travel, observe its real charged gore, and restore request-local state.");
        }

        internal static void HiddenTrampleAbilitiesUseNativePathWithSummonRules()
        {
            string[] keys = { "Aurochs", "Bison", "WoollyRhinoceros" };
            var identities = ExpandedSummoningIdentityCatalog.Build();
            Assertions.True(keys.All(key => identities.Count(item =>
                item.Symbol == "KMG.Summoning.Special." + key + ".Trample" &&
                item.PlannedType == "BlueprintAbility") == 1),
                "Only the three printed tramplers receive distinct hidden abilities.");
            Assertions.False(identities.Any(item => item.Symbol ==
                "KMG.Summoning.Special.Rhinoceros.Trample"),
                "Ordinary Rhinoceros must not gain an invented trample.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            string action = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ContextActionUngulateTrample.cs"));
            Assertions.True(builder.Contains("overrun.AutoSuccess = true;") &&
                builder.Contains("overrun.FirstTargetOnly = false;") &&
                builder.Contains("ability.SetIsFullRoundAction(true);") &&
                builder.Contains("UngulateTramplePathChecker") &&
                action.Contains("target.IsEnemy(caster)") &&
                action.Contains("ledger.TryClaim(round, target.UniqueId)") &&
                action.Contains("SavingThrowType.Reflex") &&
                action.Contains("damage.Half = save.IsPassed;") &&
                action.Contains("ReferenceEquals(caster.Blueprint, SourceUnit)"),
                "Native multi-contact movement must use a full-round, speed-bound action with exact owner, enemy, size, per-round, Reflex and half-damage gates.");
            string inventory = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            int whitelist = inventory.IndexOf(
                "private static bool ExpandedSummoningIsApprovedUngulateDirectFact(",
                StringComparison.Ordinal);
            int forbidden = inventory.IndexOf(
                "private static bool ExpandedSummoningIsForbiddenReference(",
                StringComparison.Ordinal);
            Assertions.True(whitelist >= 0 && forbidden > whitelist &&
                inventory.Substring(whitelist, forbidden - whitelist)
                    .Contains("owner == \"8c4a8e045ca844a8bd42f614707b8e74\"") &&
                inventory.Contains("!ExpandedSummoningIsApprovedUngulateDirectFact(") &&
                inventory.Contains("granted == \"0f12c70e9b264ae484fb720d0c6845aa\""),
                "The live inventory must allow only exact owner-to-trample fact pairs, leaving unrelated donor references prohibited.");
        }

        internal static void UngulateDonorSurveyRecordsNativeMechanicGraphs()
        {
            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.ExpandedSummoningNativeDonors.cs"));
            foreach (string name in new[] { "aurochs", "bison", "rhinoceros",
                "woolly", "mastodon", "elephant", "trample",
                "powerfulcharge", "TrampleFeature", "PowerfulChargeFeature",
                "FlyTrampleTest", "OverrunAbility", "MammothTrample",
                "PowerfulChargeSharedStrengthBuff" })
                Assertions.True(source.Contains("\"" + name + "\""),
                    "Native survey must cover " + name + " by exact search term.");
            Assertions.True(source.Contains("DescribeGraph(matches[0], 0,") &&
                source.Contains("native-donor-audit.json") &&
                source.Contains("DescribeNativeUnit(unit)"),
                "Survey records component graphs and physical donor profiles.");
        }

        internal static void UngulateProfilesMatchPrintedRoles()
        {
            UngulateRulesPolicy.Validate();
            UngulateRulesProfile aurochs = UngulateRulesPolicy.For("aurochs");
            UngulateRulesProfile bison = UngulateRulesPolicy.For("bison");
            UngulateRulesProfile rhino = UngulateRulesPolicy.For("rhinoceros");
            UngulateRulesProfile woolly = UngulateRulesPolicy.For("woolly-rhinoceros");
            Assertions.True(aurochs.HitDice == 3 && aurochs.Strength == 23 &&
                aurochs.GoreDiceCount == 1 && aurochs.GoreDieSides == 8 &&
                aurochs.GoreBonus == 9 && aurochs.TrampleDiceCount == 2 &&
                aurochs.TrampleDieSides == 6 && aurochs.TrampleBonus == 9 &&
                aurochs.TrampleDc == 17 && aurochs.Stampede,
                "Aurochs has the printed gore, trample and stampede profile.");
            Assertions.True(bison.HitDice == 5 && bison.Strength == 27 &&
                bison.GoreDiceCount == 2 && bison.GoreDieSides == 6 &&
                bison.GoreBonus == 12 && bison.TrampleBonus == 12 &&
                bison.TrampleDc == 20 && bison.Stampede,
                "Bison is the heavier printed herd profile.");
            Assertions.True(rhino.HitDice == 5 && rhino.Strength == 22 &&
                rhino.GoreBonus == 9 && !rhino.HasTrample &&
                rhino.ChargeDiceCount == 4 && rhino.ChargeDieSides == 6 &&
                rhino.ChargeBonus == 12 && rhino.ChargeDiceIncrement == 2 &&
                rhino.ChargeBonusIncrement == 3,
                "Ordinary Rhinoceros has charge but no invented trample.");
            Assertions.True(woolly.HitDice == 8 && woolly.Strength == 28 &&
                woolly.GoreDiceCount == 2 && woolly.GoreDieSides == 8 &&
                woolly.GoreBonus == 13 && woolly.ChargeDiceCount == 4 &&
                woolly.ChargeDieSides == 8 && woolly.ChargeBonus == 18 &&
                woolly.ChargeDiceIncrement == 2 &&
                woolly.ChargeBonusIncrement == 5 &&
                woolly.TrampleDiceCount == 2 && woolly.TrampleBonus == 13 &&
                woolly.TrampleDc == 23 && !woolly.Stampede,
                "Woolly Rhinoceros has its distinct charge and trample profile.");
            Assertions.True(rhino.AppliesPowerfulCharge(true, true, true, false),
                "A first gore charge uses the stronger stat-block damage.");
            Assertions.False(rhino.AppliesPowerfulCharge(true, false, true, false) ||
                rhino.AppliesPowerfulCharge(false, true, true, false) ||
                rhino.AppliesPowerfulCharge(true, true, false, false) ||
                rhino.AppliesPowerfulCharge(true, true, true, true) ||
                bison.AppliesPowerfulCharge(true, true, true, false),
                "Powerful charge never leaks to ordinary or unrelated attacks.");
        }

        internal static void TrampleTargetsAndRoundsAreBounded()
        {
            UngulateRulesProfile aurochs = UngulateRulesPolicy.For("aurochs");
            UngulateRulesProfile rhino = UngulateRulesPolicy.For("rhinoceros");
            Assertions.True(aurochs.CanTrample(4, 3, 1),
                "A Large aurochs can trample a Medium creature.");
            Assertions.False(aurochs.CanTrample(4, 4, 2),
                "A pair does not qualify for same-size stampede.");
            Assertions.True(aurochs.CanTrample(4, 4, 3) &&
                aurochs.TrampleSaveDc(3) == 19,
                "Three adjacent stampeding herd animals may trample Large foes at DC 19.");
            Assertions.False(aurochs.CanTrample(4, 5, 3) ||
                rhino.CanTrample(4, 3, 0),
                "Stampede does not reach larger creatures or grant Rhino trample.");
            TrampleRoundLedger ledger = new TrampleRoundLedger();
            Assertions.True(ledger.TryClaim(10, "target-a"),
                "First contact is eligible.");
            Assertions.False(ledger.TryClaim(10, "target-a"),
                "A path replay cannot damage the same target twice in a round.");
            Assertions.True(ledger.TryClaim(10, "target-b") &&
                ledger.TryClaim(11, "target-a"),
                "Another foe and a later round are eligible.");
            Assertions.False(ledger.TryClaim(10, "target-c") ||
                ledger.TryClaim(11, ""),
                "Time regression or missing target identity fails closed.");
            ledger.Clear();
            Assertions.True(ledger.TryClaim(10, "target-a"),
                "Cleanup releases the request-local contact ledger.");
        }

        internal static void DonorRigCaptureUsesPrivateBindFrame()
        {
            string root = Environment.CurrentDirectory;
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningCorrection.cs"));
            string project = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "KingmakerGunslinger.csproj"));
            Assertions.True(runner.Contains("CaptureExpandedSummoningUngulateDonorRig(_rulesTrampler,") &&
                runner.Contains("_rulesTrampleIndex == 0 ? \"horse\" : \"mastodon\"") &&
                runner.Contains("Matrix4x4 bind = poses[index].inverse;") &&
                runner.Contains("Path.Combine(_request.EvidenceDirectory, fileName)"),
                "Original-mesh authoring must measure both exact live donor bind frames in guarded local evidence.");
            Assertions.False(project.Contains("sprint11-horse-bind-rig.json") ||
                project.Contains("sprint11-mastodon-bind-rig.json"),
                "Measured native donor transforms must never enter the package.");
        }

        internal static void OriginalUngulateVisualsUseNativeBindFramesAndPackage()
        {
            string root = Environment.CurrentDirectory;
            string directory = Path.Combine(root, "assets", "ungulates");
            string[] kinds = { "aurochs", "bison", "rhinoceros",
                "woolly-rhinoceros" };
            string[] meshHashes = {
                "c699bf2f310faad1b27f5a8526d5ec62edd9d89de2d1b271e40c3e1dc5144857",
                "ded381caaad9bf5f350867d2f13391d90b5ad52b92c27360b4f1051ef2416dc7",
                "fc4196030a46cc7c71d9a2e7492b5c89a08543d8f08d3728bbf187ffe16c0723",
                "2a10b55256a9bdc578ea8eed02e8535b3348522de9b5905b48ab7a80efb5fe36"
            };
            for (int item = 0; item < kinds.Length; item++)
            {
                string kind = kinds[item];
                string meshPath = Path.Combine(directory, kind + "-mesh.json");
                using (var sha = SHA256.Create())
                {
                    string actual = string.Concat(sha.ComputeHash(
                        File.ReadAllBytes(meshPath))
                        .Select(value => value.ToString("x2")));
                    Assertions.Equal(meshHashes[item], actual,
                        kind + " mesh bytes match the LF-normalized reviewed export.");
                }
                JObject mesh = JObject.Parse(File.ReadAllText(Path.Combine(
                    directory, kind + "-mesh.json")));
                Assertions.Equal(2, (int)mesh["schemaVersion"],
                    kind + " uses the shared skinned-mesh schema.");
                string[] bones = ((JArray)mesh["bones"])
                    .Select(value => (string)value).ToArray();
                Assertions.True(bones.Length >= 25 && bones.Length <= 30 &&
                    bones.Distinct(StringComparer.Ordinal).Count() == bones.Length &&
                    bones.Contains("Head") && bones.Contains("LowerTorso"),
                    kind + " binds only an unambiguous measured native rig.");
                Assertions.True(((string)mesh["space"]).Contains("donor renderer local") &&
                    ((string)mesh["rigSha256"]).Length == 64,
                    kind + " records its captured frame without shipping transforms.");
                int vertices = (int)mesh["vertexCount"];
                int triangles = (int)mesh["triangleCount"];
                Assertions.True(vertices >= 500 && triangles >= 390 &&
                    Convert.FromBase64String((string)mesh["data"]).Length ==
                    vertices * 64 + triangles * 12,
                    kind + " carries complete original geometry and weights.");
                JObject albedo = (JObject)mesh["albedo"];
                Assertions.Equal(kind + "-albedo.png", (string)albedo["file"],
                    kind + " names its own painting.");
                using (var sha = SHA256.Create())
                {
                    string actual = string.Concat(sha.ComputeHash(File.ReadAllBytes(
                        Path.Combine(directory, (string)albedo["file"])))
                        .Select(value => value.ToString("x2")));
                    Assertions.Equal((string)albedo["sha256"], actual,
                        kind + " painting matches its geometry manifest.");
                }
            }
            string source = Path.Combine(root, "assets-source", "original-models",
                "ungulates");
            Assertions.True(File.Exists(Path.Combine(source, "generate_ungulates.py")) &&
                File.Exists(Path.Combine(source, "paint_ungulate_albedo.py")),
                "Original editable source and reproducible exporters remain available.");
            string loader = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Assets", "PteranodonAssetRuntime.cs"));
            string view = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningPteranodonViewPatch.cs"));
            Assertions.True(loader.Contains("ConfigureUngulates(context)") &&
                loader.Contains("AllowedHorseBones") &&
                loader.Contains("AllowedMastodonBones") &&
                loader.Contains("TryGetUngulateVisual") &&
                view.Contains("UngulateKeys.Contains(attachment.VisualKey)") &&
                view.Contains("TryResolveDonorBinding(donor, boneNames") &&
                view.Contains("Revert(attachment)") &&
                view.Contains("!UngulateKeys.Contains(attachment.VisualKey)") &&
                view.Contains("DestroyImmediate(attachment.Mesh)") &&
                view.Contains("DestroyImmediate(material)"),
                "Four original visuals use the validated native-bind instance swap and fallback.");
            string build = File.ReadAllText(Path.Combine(root, "scripts",
                "Build-Local.ps1"));
            string package = File.ReadAllText(Path.Combine(root, "scripts",
                "package.ps1"));
            Assertions.True(build.Contains("assets\\ungulates") &&
                package.Contains("assets\\ungulates") &&
                build.Contains("{ 274 } else { 272 }") &&
                package.Contains("{ 274 } else { 272 }"),
                "All eight ungulate asset files enter the strict standalone package.");
        }

        internal static void HiddenUngulatesUseGuardedCreatureViewReview()
        {
            string root = Environment.CurrentDirectory;
            string review = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningCreatureReview.cs"));
            string motion = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.PteranodonReview.cs"));
            Assertions.True(review.Contains("suppressedSprint11Candidate =") &&
                review.Contains("!suppressedSprint10Candidate && !suppressedSprint11Candidate") &&
                review.Contains("key == \"aurochs\" || key == \"bison\"") &&
                review.Contains("key == \"rhinoceros\" || key == \"woolly-rhinoceros\"") &&
                review.Contains("IsOriginalReviewKey(key)") &&
                review.Contains("OriginalReviewVisualName(key)") &&
                review.Contains("expanded-summoning-original-view-") &&
                review.Contains("Resources.FindObjectsOfTypeAll<Mesh>()") &&
                review.Contains(".FindObjectsOfTypeAll<Material>()") &&
                review.Contains("expanded-summoning-ungulate-travel-") &&
                review.Contains("MotionReviewTravelValid") &&
                motion.Contains("IsGuidedMotionReview(unit)") &&
                motion.Contains("PrepareSprint9FlightMovement(unit)") &&
                motion.Contains("unit.View.MovementAgent.TickMovement(delta)"),
                "The four hidden Sprint 11 units can enter only request-local visual review with exact attached-mesh and resource-cleanup checks.");
            float rhino, woolly, unused;
            Assertions.True(SummonViewScaleCatalog.TryGetMultiplier(
                    "KMG_Summoning_Unit_Rhinoceros", out rhino) &&
                SummonViewScaleCatalog.TryGetMultiplier(
                    "KMG_Summoning_Unit_WoollyRhinoceros", out woolly) &&
                rhino == 0.55f && woolly == 0.60f &&
                !SummonViewScaleCatalog.TryGetMultiplier(
                    "KMG_Summoning_Unit_Pony", out unused) &&
                !SummonViewScaleCatalog.TryGetMultiplier(
                    "KMG_Summoning_Unit_Horse", out unused),
                "The Huge donor is reduced only for the two Large original rhinoceroses; Pony and Horse remain native controls.");
        }
    }
}
