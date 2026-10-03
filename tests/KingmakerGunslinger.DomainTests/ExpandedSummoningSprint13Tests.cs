using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint13Tests
    {
        /// <summary>
        /// Sprint 13 appends the Poison Frog's flat-1 bite, the three
        /// Wolverine rage blueprints, and the Shadow Mastiff: its unit, its
        /// four Summon Monster placements, the 1d6 tail slap its printed stat
        /// block needs, and its six bay and shadow-blend identities.
        /// </summary>
        internal const int AppendedLedgerIdentities = 16;

        private static string Source(params string[] parts)
        {
            string[] all = new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger" }.Concat(parts).ToArray();
            return File.ReadAllText(Path.Combine(all));
        }

        /// <summary>
        /// The printed rage triggers on taking damage, and only on damage that
        /// actually landed. A miss, a fully resisted hit and a zero-damage
        /// effect are all "was attacked" rather than "takes damage".
        /// </summary>
        internal static void RageSchedulesOnlyOnRealDamageToTheWolverine()
        {
            Assertions.True(SummonRagePolicy.ShouldScheduleRage(1, true, true,
                false, false),
                "One point of real damage must schedule the printed rage.");
            Assertions.False(SummonRagePolicy.ShouldScheduleRage(0, true, true,
                false, false),
                "Zero damage is not taking damage.");
            Assertions.False(SummonRagePolicy.ShouldScheduleRage(-3, true, true,
                false, false),
                "Negative damage is not taking damage.");
            Assertions.False(SummonRagePolicy.ShouldScheduleRage(7, false, true,
                false, false),
                "Damage dealt to someone else must not enrage the wolverine.");
            Assertions.False(SummonRagePolicy.ShouldScheduleRage(7, true, false,
                false, false),
                "A destroyed or dead wolverine must not be scheduled.");
            Assertions.False(SummonRagePolicy.ShouldScheduleRage(7, true, true,
                true, false),
                "The printed rage does not restack on an already raging creature.");
            Assertions.False(SummonRagePolicy.ShouldScheduleRage(7, true, true,
                false, true),
                "A second hit must not restart a rage that is already coming.");
        }

        /// <summary>
        /// "flies into a rage on its next turn". No amount or arrangement of
        /// damage may start the rage before a round boundary has passed.
        /// </summary>
        internal static void RageNeverBeginsOnTheTurnTheDamageLanded()
        {
            Assertions.False(SummonRagePolicy.ShouldBeginRage(0, true, false),
                "The rage must not begin on the turn the damage landed.");
            Assertions.True(SummonRagePolicy.ShouldBeginRage(1, true, false),
                "The rage must begin once its printed round boundary passed.");
            Assertions.True(SummonRagePolicy.ShouldBeginRage(4, true, false),
                "A longer wait must still begin the rage.");
            Assertions.False(SummonRagePolicy.ShouldBeginRage(1, false, false),
                "A gone wolverine must not begin raging.");
            Assertions.False(SummonRagePolicy.ShouldBeginRage(1, true, true),
                "An already raging wolverine must not begin a second rage.");
            Assertions.Equal(1, SummonRagePolicy.WolverineRageOnsetRounds,
                "The printed onset is one round boundary.");
        }

        /// <summary>
        /// The printed effect is "+4 to Strength, +4 to Constitution, and -2 to
        /// AC", and the rage "cannot end voluntarily". A rage that kept the
        /// bonuses and dropped the penalty would be stronger than the stat
        /// block, which is the exact class of defect the owner rejected in
        /// Phase 1.
        /// </summary>
        internal static void RageCarriesThePrintedArmourPenaltyAndNoVoluntaryEnd()
        {
            Assertions.Equal(4, SummonRagePolicy.WolverineRageAbilityBonus,
                "Printed rage ability bonus changed.");
            Assertions.Equal(-2, SummonRagePolicy.WolverineRageArmorClassPenalty,
                "Printed rage armour-class penalty changed or lost its sign.");
            Assertions.False(SummonRagePolicy.MayEndVoluntarily(),
                "The printed rage cannot be ended voluntarily.");
            SummonRagePolicy.Validate();

            string builder = Source("Blueprints",
                "ExpandedSummoningNaturalBuilder.cs");
            Assertions.True(builder.Contains("armourClass.Stat = StatType.AC") &&
                builder.Contains(
                    "armourClass.Value = SummonRagePolicy.WolverineRageArmorClassPenalty"),
                "The rage buff must carry the printed AC term from the policy constant.");
            Assertions.True(
                builder.Contains("strength.Descriptor = ModifierDescriptor.Morale") &&
                builder.Contains(
                    "constitution.Descriptor = ModifierDescriptor.Morale"),
                "The printed rage bonuses are morale bonuses.");
        }

        /// <summary>
        /// The delay must be structural rather than a convention. The trigger
        /// component may apply only the onset marker, and the rage state may be
        /// reached only from the marker's round-boundary action list, so no
        /// code path exists from a damage event to the +4/+4/-2.
        /// </summary>
        internal static void RageReachesItsStateOnlyThroughARoundBoundary()
        {
            string component = Source("Summoning",
                "ExpandedSummoningSprint13CombatComponents.cs");
            // The claim is about the damage trigger specifically, so its own
            // class body is what gets examined: the onset component below it
            // does apply the rage, and must.
            int trigger = component.IndexOf(
                "public sealed class SummonRageOnDamageComponent",
                StringComparison.Ordinal);
            int afterTrigger = component.IndexOf(
                "public sealed class SummonRageOnsetComponent",
                StringComparison.Ordinal);
            Assertions.True(trigger > 0 && afterTrigger > trigger,
                "Both rage components must be present, trigger first.");
            string triggerBody = component.Substring(trigger,
                afterTrigger - trigger);
            Assertions.True(triggerBody.Contains("RuleApplyBuff(owner, OnsetBuff"),
                "The damage trigger must apply the onset marker.");
            Assertions.False(triggerBody.Contains("RuleApplyBuff(owner, RageBuff"),
                "The damage trigger must never apply the rage state directly.");

            // The rage is reachable only from a round boundary, which the
            // engine dispatches through Buff.TickMechanics.
            string onsetBody = component.Substring(afterTrigger);
            Assertions.True(onsetBody.Contains("ITickEachRound") &&
                onsetBody.Contains("public void OnNewRound()") &&
                onsetBody.Contains("RuleApplyBuff(owner, RageBuff"),
                "The rage state must be applied from the onset marker's round boundary.");
            Assertions.True(onsetBody.Contains(
                "SummonRagePolicy.ShouldBeginRage(1, available, alreadyRaging)"),
                "The onset must ask the printed policy whether the rage may begin.");

            string builder = Source("Blueprints",
                "ExpandedSummoningNaturalBuilder.cs");
            Assertions.True(builder.Contains("onsetTick.RageBuff = state") &&
                builder.Contains("onset.Frequency = DurationRate.Rounds"),
                "The onset marker must carry the round-boundary component and tick by rounds.");
        }

        /// <summary>
        /// The rage belongs to one animal. Both buffs are applied to the
        /// component's own carrier and to no other unit, so no caster, ally or
        /// other summon can receive them.
        /// </summary>
        internal static void RageStaysOnTheOneWolverine()
        {
            string component = Source("Summoning",
                "ExpandedSummoningSprint13CombatComponents.cs");
            Assertions.True(component.Contains(
                "ReferenceEquals(evt.Target, owner)"),
                "The trigger must only fire for damage dealt to its own carrier.");
            Assertions.False(component.Contains("ToCaster"),
                "Nothing in the summon-local rage may travel to the caster.");

            string builder = Source("Blueprints",
                "ExpandedSummoningNaturalBuilder.cs");
            Assertions.False(builder.Contains("apply.ToCaster = true"),
                "The rage state must not be applied to the caster.");
        }

        /// <summary>
        /// Printed: "bite +3 (1 plus poison)". A flat point, which
        /// <c>DiceType.One</c> expresses exactly, so the previous 1d3 - which
        /// averages 2 and can roll 3 - was simply wrong rather than a necessary
        /// adaptation.
        /// </summary>
        internal static void PoisonFrogBiteIsThePrintedFlatPoint()
        {
            NaturalSummonProfile frog =
                ExpandedSummoningNaturalProfiles.For("poisonous-frog");
            Assertions.Equal("Bite1", frog.PrimaryWeapon,
                "The Poison Frog's printed flat-1 bite changed.");
            Assertions.Equal(0, frog.AdditionalWeapons.Count,
                "The Poison Frog has one printed attack.");
            Assertions.True(frog.Facts.Contains("PoisonFrog"),
                "The accepted Poison Frog poison contract must be preserved.");

            string builder = Source("Blueprints",
                "ExpandedSummoningNaturalBuilder.cs");
            Assertions.True(builder.Contains("Bite1Symbol), Bite1Symbol, 1, DiceType.One"),
                "The flat-1 bite must be minted at exactly one roll of DiceType.One.");
            Assertions.True(builder.Contains(
                "\"KMG.Summoning.Natural.Bite1\""),
                "The flat-1 bite needs its own project-owned identity.");
        }


        /// <summary>
        /// Printed: "The save DC is Charisma-based and includes a +2 racial
        /// bonus", and the stat block prints 16. Encoding the components
        /// rather than the literal is what lets a buffed Charisma move the DC,
        /// so the derivation is asserted rather than the number.
        /// </summary>
        internal static void BayDcDerivesToThePrintedSixteen()
        {
            Assertions.Equal(16, SummonShadowMastiffPolicy.BayWillDc(6, 13),
                "The printed Shadow Mastiff bay DC no longer derives to 16.");
            // Charisma moves it, which is the whole point of deriving it.
            Assertions.Equal(18, SummonShadowMastiffPolicy.BayWillDc(6, 17),
                "A buffed Charisma must move the bay DC.");
            Assertions.Equal(15, SummonShadowMastiffPolicy.BayWillDc(6, 11),
                "A drained Charisma must move the bay DC.");
            // Hit dice move it by halves, as the save-DC formula does.
            Assertions.Equal(17, SummonShadowMastiffPolicy.BayWillDc(8, 13),
                "Hit dice must move the bay DC by halves.");
            SummonShadowMastiffPolicy.Validate();
        }

        /// <summary>
        /// Printed: "all creatures within a 300-foot spread except evil
        /// outsiders ... A creature that successfully saves cannot be affected
        /// by the same mastiff's bay for 24 hours."
        /// </summary>
        internal static void BaySparesEvilOutsidersAndHonoursItsOwnImmunity()
        {
            Assertions.True(SummonShadowMastiffPolicy.ShouldRollBay(true, false,
                false), "An ordinary creature in the spread must save.");
            Assertions.False(SummonShadowMastiffPolicy.ShouldRollBay(true, true,
                false), "The printed rule exempts evil outsiders.");
            Assertions.False(SummonShadowMastiffPolicy.ShouldRollBay(true, false,
                true), "A creature inside its 24-hour window must not save again.");
            Assertions.False(SummonShadowMastiffPolicy.ShouldRollBay(false,
                false, false), "A dead or destroyed creature must not save.");
            Assertions.True(SummonShadowMastiffPolicy.AppliesPanicOnFailedSave(
                false), "A failed save must panic.");
            Assertions.False(SummonShadowMastiffPolicy.AppliesPanicOnFailedSave(
                true), "A successful save must not panic.");
            Assertions.True(SummonShadowMastiffPolicy.GrantsImmunityOnSave(true),
                "A successful save must open the printed 24-hour window.");
            Assertions.False(SummonShadowMastiffPolicy.GrantsImmunityOnSave(
                false), "A failed save must not grant immunity.");
            Assertions.Equal(24, ExpandedSummoningSpecialProfiles
                .ShadowMastiffBayImmunityHours,
                "The printed bay immunity window changed.");
            Assertions.Equal(300, ExpandedSummoningSpecialProfiles
                .ShadowMastiffBayRadiusFeet,
                "The printed bay spread changed.");
        }

        /// <summary>
        /// Printed: "Artificial illumination, even a light or continual flame
        /// spell, does not negate this ability; a daylight spell, however,
        /// does." Exactly two things switch it off and artificial light is not
        /// one of them.
        /// </summary>
        internal static void ShadowBlendHasExactlyThePrintedTwoNegations()
        {
            Assertions.True(SummonShadowMastiffPolicy.GrantsShadowConcealment(
                false, false), "Outside full daylight shadow blend applies.");
            Assertions.False(SummonShadowMastiffPolicy.GrantsShadowConcealment(
                true, false), "Full daylight negates shadow blend.");
            Assertions.False(SummonShadowMastiffPolicy.GrantsShadowConcealment(
                false, true), "A daylight effect negates shadow blend.");

            string builder = Source("Blueprints",
                "ExpandedSummoningSpecialBuilder.cs");
            // The printed 50% miss chance is Concealment.Total; Partial is the
            // 20% grade and would halve the ability.
            Assertions.True(builder.Contains("gate.Grade = Concealment.Total"),
                "Shadow blend must grant the printed 50% miss chance.");
            Assertions.True(builder.Contains(
                "gate.Descriptor = ConcealmentDescriptor.Blur"),
                "Shadow blend must not be filed as invisibility or as fog.");
            // Negation is matched against the audited native Daylight identity.
            Assertions.True(builder.Contains("2b877386976817a429002e8bb10bb3fc"),
                "Shadow blend must be negated by the exact native Daylight identity.");
            string component = Source("Summoning",
                "ExpandedSummoningSprint13CombatComponents.cs");
            Assertions.True(component.Contains("IsSingleLightScene") &&
                component.Contains("TimeOfDay.Day"),
                "Full daylight must be read from the engine's own day state and area lighting.");
            // Buff.IsSuppressed is a plain field that gates only the per-round
            // mechanics tick, so it can never be what switches the concealment
            // off; relying on it would have left the creature concealed in
            // full daylight and stronger than its own stat block.
            // The word appears in the comment that explains why suppression
            // is the wrong mechanism; what must not appear is a write to it.
            Assertions.False(component.Contains("IsSuppressed =") ||
                component.Contains("IsSuppressed="),
                "Shadow blend must not rely on buff suppression to negate itself.");
            // The component owns the entry, so it must also release it.
            Assertions.True(component.Contains("AddConcealment(Entry())") &&
                component.Contains("RemoveConcealement(Entry())") &&
                component.Contains("public override void OnTurnOff()"),
                "Shadow blend must own its concealment entry and remove it explicitly.");
            // Re-decided per concealment check, not on a round cadence, which
            // is what "in any condition of illumination other than full
            // daylight" actually says.
            Assertions.True(component.Contains(
                "OnEventAboutToTrigger(RuleConcealmentCheck evt)"),
                "Shadow blend must re-read its condition at each concealment check.");
        }

        /// <summary>
        /// Printed: "bite +10 (1d8+4 plus trip), tail slap +5 (1d6+2)". Base
        /// attack 6 plus Strength 4 with full Strength damage is a primary
        /// natural weapon; 6 + 4 - 5 with half Strength damage is a secondary
        /// limb. The donor's convenient limb layout may not promote either.
        /// </summary>
        internal static void ShadowMastiffAttacksFollowItsPrintedNumbers()
        {
            string builder = Source("Blueprints",
                "ExpandedSummoningSpecialBuilder.cs");
            Assertions.True(builder.Contains(
                "unit.Body = NaturalBody(bite, Array.Empty<BlueprintItemWeapon>(),\n                new[] { tail });"),
                "The bite must be primary and the tail slap the only secondary limb.");
            Assertions.True(builder.Contains(
                "SetField(tail, \"m_DamageDice\", new DiceFormula(1, DiceType.D6));"),
                "The printed tail slap is 1d6.");
            Assertions.True(builder.Contains("TrippingBiteGuid") &&
                builder.Contains("printed trip on the bite"),
                "The printed bite carries trip.");
            Assertions.True(builder.Contains("NaturalArmor6Guid"),
                "The printed +6 natural armor is missing.");
            Assertions.True(builder.Contains("IronWillGuid") &&
                builder.Contains("PowerAttackGuid") &&
                builder.Contains("ImprovedInitiativeGuid"),
                "The three printed feats are missing.");
            Assertions.Equal(6, ExpandedSummoningSpecialProfiles
                .ShadowMastiffHitDice, "The printed hit dice changed.");
            Assertions.Equal(50, ExpandedSummoningSpecialProfiles
                .ShadowMastiffSpeedFeet, "The printed speed changed.");
        }

        /// <summary>
        /// The charter asks for bay that is useful without repeatedly harming
        /// allies or stalling AI. The resolution keeps the printed spread -
        /// every creature in range, which includes the summoner's party - and
        /// bounds the decision instead: bay is a player-activated standard
        /// action and nothing wires it into a brain, so no AI can select it and
        /// no friendly-fire loop is possible.
        /// </summary>
        internal static void BayIsPlayerActivatedAndNeverAnAiChoice()
        {
            string builder = Source("Blueprints",
                "ExpandedSummoningSpecialBuilder.cs");
            int bay = builder.IndexOf("private static void ConfigureShadowMastiffBay(",
                StringComparison.Ordinal);
            Assertions.True(bay > 0, "The bay builder is missing.");
            int end = builder.IndexOf("private static void ConfigureShadowMastiffShadowBlendState(",
                StringComparison.Ordinal);
            Assertions.True(end > bay, "The bay builder could not be bounded.");
            string body = builder.Substring(bay, end - bay);
            // The printed spread does not spare allies.
            Assertions.True(body.Contains("TargetType.Any"),
                "The printed bay catches every creature in the spread, not only enemies.");
            Assertions.True(body.Contains("ShadowMastiffBayRadiusFeet"),
                "The bay radius must come from the printed profile.");
            Assertions.True(body.Contains("UnitCommand.CommandType.Standard"),
                "Bay is a standard action the player spends.");
            // Nothing may teach a brain to cast it.
            Assertions.False(body.Contains("BlueprintAiCastSpell"),
                "No AI action may select bay.");
            Assertions.False(builder.Contains("ShadowMastiffBrain"),
                "The Shadow Mastiff must not carry a bay-casting brain.");
        }

        /// <summary>
        /// The four Sprint 13 identities are declared in the frozen catalog so
        /// the append-only ledger allocates them, and none replaces an existing
        /// symbol.
        /// </summary>
        internal static void Sprint13IdentitiesAreDeclaredAndAppendOnly()
        {
            string catalog = Source("Summoning",
                "ExpandedSummoningIdentityCatalog.cs");
            string[] symbols = {
                "KMG.Summoning.Natural.Bite1",
                "KMG.Summoning.Natural.Wolverine.Rage",
                "KMG.Summoning.Natural.Wolverine.RageOnset",
                "KMG.Summoning.Natural.Wolverine.RageState",
                "KMG.Summoning.Natural.Tail1d6",
                "KMG.Summoning.Special.ShadowMastiff.Traits",
                "KMG.Summoning.Special.ShadowMastiff.Bay",
                "KMG.Summoning.Special.ShadowMastiff.BayPanic",
                "KMG.Summoning.Special.ShadowMastiff.BayImmunity",
                "KMG.Summoning.Special.ShadowMastiff.ShadowBlend",
                "KMG.Summoning.Special.ShadowMastiff.ShadowBlendState" };
            foreach (string symbol in symbols)
                Assertions.True(catalog.Contains("\"" + symbol + "\""),
                    "Sprint 13 identity is not declared: " + symbol);
            // The declared symbols above are the eleven that are written
            // down by hand; the Shadow Mastiff's unit and its four placements
            // are generated from the catalog entry, which is why the sprint's
            // ledger append is five larger.
            Assertions.Equal(AppendedLedgerIdentities, symbols.Length + 5,
                "Sprint 13 ledger count must match its declared identities.");
            Assertions.True(catalog.Contains("UnitCount = 94"),
                "The Shadow Mastiff unit identity is not registered.");
        }

        /// <summary>
        /// The three original visuals are deterministic, carry only reviewed
        /// donor controls, and reach the installed mod through the project,
        /// the local build and the strict package. A mesh that never arrives
        /// is a creature wearing somebody else's body.
        /// </summary>
        internal static void Sprint13OriginalVisualsAreDeterministicAndDelivered()
        {
            string root = Environment.CurrentDirectory;
            string directory = Path.Combine(root, "assets", "sprint13-creatures");
            string[] kinds = { "wolverine", "shadow-mastiff", "poisonous-frog" };
            string[] meshHashes = {
                "e1e97db7ab271c3705472a78262fc3306a1515ef174c39e7d6899c28bad54fc6",
                "61755b4c1853942b73e0f2666ac246bf5d77ef9b7c6800ca30ea2f7e56a13bd3",
                "2b37e38a4e6fb7613a06ab69f161273221a05c50105691a9dcdfd58899f58ed6"
            };
            // The Wolverine and the Shadow Mastiff ride the Worg, so both must
            // bind exactly the reviewed Worg controls the Goblin Dog already
            // binds. The Poison Frog's donor is a different rig entirely.
            string[] worgBones = {
                "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Foot0",
                "L_Leg0_Lower", "L_Leg0_Lower2", "L_Leg0_Upper", "L_Palm",
                "R_Arm_Lower", "R_Arm_Upper", "R_Foot0", "R_Leg0_Lower",
                "R_Leg0_Lower2", "R_Leg0_Upper", "R_Palm", "Torso_Lower",
                "Torso_Upper", "ear_L", "ear_R", "front_paw__tip_R",
                "front_paw_tip_L", "hindpaw_tip_L", "hindpaw_tip_R", "jaw",
                "jaw_woo_down", "jaw_woo_up", "jaw_woo_up_add", "neck",
                "spine_0", "tail_01", "tail_02", "tail_03", "tail_04",
                "withers"
            };
            string[] frogBones = {
                "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Foot0",
                "L_Leg0_Lower", "L_Leg0_Upper", "L_Palm", "LowerTorso",
                "R_Arm_Lower", "R_Arm_Upper", "R_Foot0", "R_Leg0_Lower",
                "R_Leg0_Upper", "R_Palm", "chest_joint", "jaw_endJoint",
                "jaw_skinJoint1", "l_t_eyeLid_joint", "l_toeTip_joint",
                "l_toe_joint", "r_t_eyeLid_joint", "r_toeTip_joint",
                "r_toe_joint", "spine_3_joint", "stomach"
            };
            string[][] allowedBones = { worgBones, worgBones, frogBones };
            for (int index = 0; index < kinds.Length; index++)
            {
                string kind = kinds[index];
                string meshPath = Path.Combine(directory, kind + "-mesh.json");
                Assertions.Equal(meshHashes[index], Sha256(meshPath),
                    kind + " mesh matches the reviewed deterministic export.");
                JObject mesh = JObject.Parse(File.ReadAllText(meshPath));
                Assertions.Equal(2, (int)mesh["schemaVersion"],
                    kind + " uses the audited skinned-mesh schema.");
                Assertions.True(((string)mesh["space"]).Contains(
                        "donor renderer local") &&
                    ((string)mesh["rigSha256"]).Length == 64,
                    kind + " records only its private captured-frame hash.");
                string[] bones = ((JArray)mesh["bones"])
                    .Select(value => (string)value).ToArray();
                Assertions.True(bones.SequenceEqual(allowedBones[index]),
                    kind + " binds only the exact reviewed donor controls.");
                int vertices = (int)mesh["vertexCount"];
                int triangles = (int)mesh["triangleCount"];
                Assertions.True(vertices >= 400 && triangles >= 900 &&
                    Convert.FromBase64String((string)mesh["data"]).Length ==
                    vertices * 64 + triangles * 12,
                    kind + " carries complete original geometry, UVs and weights.");
                JObject albedo = (JObject)mesh["albedo"];
                Assertions.Equal(kind + "-albedo.png", (string)albedo["file"],
                    kind + " names its own painting.");
                Assertions.True((int)albedo["width"] == 1024 &&
                    (int)albedo["height"] == 1024 &&
                    Sha256(Path.Combine(directory, (string)albedo["file"])) ==
                    (string)albedo["sha256"],
                    kind + " painting matches its mesh manifest.");
            }

            string source = Path.Combine(root, "assets-source",
                "original-models", "sprint13-creatures");
            string generator = File.ReadAllText(Path.Combine(source,
                "generate_sprint13_creatures.py"));
            Assertions.True(File.Exists(Path.Combine(source,
                    "paint_sprint13_albedo.py")) &&
                File.Exists(Path.Combine(source,
                    "render_sprint13_review.py")) &&
                File.Exists(Path.Combine(source, "SOURCE.md")) &&
                generator.Contains("if len(renderers) != 1") &&
                generator.Contains("donor bind frame is incomplete") &&
                generator.Contains("donor bind frame repeats a bone name") &&
                generator.Contains("wrong donor bind frame for"),
                "Editable source must retain deterministic review and strict private-capture rejection.");
            // The atlas regions tile edge to edge, so a ring landing on a
            // region edge samples its neighbour. That cost the Shadow Mastiff
            // a blue ring around its neck, and the inset is what prevents it.
            Assertions.True(generator.Contains("inset_region_uvs(uvs)") &&
                generator.Contains("UV_REGION_INSET = 0.02"),
                "Every Sprint 13 coordinate must be inset inside its own atlas region.");

            string project = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "KingmakerGunslinger.csproj"));
            string loader = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Assets", "PteranodonAssetRuntime.cs"));
            string view = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningPteranodonViewPatch.cs"));
            string runtime = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            Assertions.True(project.Contains("assets\\sprint13-creatures\\*-mesh.json") &&
                project.Contains("assets\\sprint13-creatures\\*-albedo.png") &&
                loader.Contains("ConfigureSprint13Creatures(context)") &&
                loader.Contains("AllowedGiantFrogBones") &&
                loader.Contains("TryGetSprint13CreatureVisual") &&
                view.Contains("Sprint13CreatureKeys.Contains(attachment.VisualKey)") &&
                view.Contains("KMG_Summoning_Unit_Wolverine") &&
                view.Contains("KMG_Summoning_Unit_ShadowMastiff") &&
                view.Contains("KMG_Summoning_Unit_PoisonousFrog") &&
                !project.Contains("sprint13-giant-poisonous-frog-bind-rig.json"),
                "The three originals use the instance-local swap and no private capture is packaged.");
            // The Wolverine and the Shadow Mastiff reuse the Worg list rather
            // than getting one of their own; a second copy would be a place
            // for the two to drift apart.
            Assertions.True(loader.Contains(
                    "? AllowedGiantFrogBones : AllowedWolfWorgBones"),
                "The two Worg riders must reuse the reviewed Worg bone list.");
            Assertions.True(runtime.Contains("int sprint13VisualChecked = 0;") &&
                runtime.Contains("variant.Creature.Key == \"shadow-mastiff\"") &&
                runtime.Contains("sprint13VisualAttached == sprint13VisualChecked") &&
                runtime.Contains("sprint13VisualChecked &&"),
                "The shared visual-patch lifecycle assertion must account for every Sprint 13 coverage view.");

            string build = File.ReadAllText(Path.Combine(root, "scripts",
                "Build-Local.ps1"));
            string package = File.ReadAllText(Path.Combine(root, "scripts",
                "package.ps1"));
            string buildOutput = File.ReadAllText(Path.Combine(root, "scripts",
                "validate-build-output.ps1"));
            string packageCheck = File.ReadAllText(Path.Combine(root, "scripts",
                "validate-package.ps1"));
            Assertions.True(build.Contains("assets\\sprint13-creatures") &&
                package.Contains("assets\\sprint13-creatures") &&
                buildOutput.Contains("assets\\sprint13-creatures\\wolverine-mesh.json") &&
                packageCheck.Contains("assets\\sprint13-creatures\\poisonous-frog-albedo.png") &&
                build.Contains("{ 300 } else { 298 }") &&
                package.Contains("{ 300 } else { 298 }"),
                "All six Sprint 13 asset files enter the strict standalone package.");
        }

        /// <summary>
        /// A Tiny creature wearing a Medium donor's prefab has to be scaled
        /// down for the view alone; mechanical size, reach and footprint are
        /// untouched. The multiplier itself is confirmed by the live
        /// party-camera review, which records the applied scale.
        /// </summary>
        internal static void PoisonFrogIsViewScaledWithoutTouchingItsMechanics()
        {
            SummonViewScaleSpec frog = SummonViewScaleCatalog.All.Single(
                value => value.CreatureKey == "poisonous-frog");
            Assertions.True(frog.Multiplier > 0f && frog.Multiplier < 1f,
                "The Poison Frog's view must be smaller than its donor's.");
            Assertions.Equal("KMG_Summoning_Unit_PoisonousFrog",
                frog.BlueprintName,
                "The view scale must address the Poison Frog's own identity.");
            Assertions.Equal("Tiny",
                ExpandedSummoningNaturalProfiles.For("poisonous-frog").Size,
                "The Poison Frog's mechanical size stays as printed.");
        }

        private static string Sha256(string path)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
                return string.Concat(sha.ComputeHash(File.ReadAllBytes(path))
                    .Select(value => value.ToString("x2")));
        }
    }
}
