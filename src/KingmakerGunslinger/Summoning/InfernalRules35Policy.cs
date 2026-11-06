using System;

namespace KingmakerGunslinger.Summoning
{
    internal static class InfernalRules35
    {
        internal const string BarbedUnitSymbol =
            "KMG.Summoning.Unit.BarbedDevil";
        internal const string BoneUnitSymbol =
            "KMG.Summoning.Unit.BoneDevil";
        internal const string BarbedPrivateIdentityPrefix =
            "KMG.Summoning.Infernal.BarbedDevil.";
        internal const string BonePrivateIdentityPrefix =
            "KMG.Summoning.Infernal.BoneDevil.";
        internal const int AppendedCreatureIdentityCount = 2;
        internal const int AppendedRootIdentityCount = 5;
        internal const int AppendedPrivateIdentityCount = 47;
        internal const int AppendedCatalogIdentityCount =
            AppendedCreatureIdentityCount + AppendedRootIdentityCount +
            AppendedPrivateIdentityCount;
        internal const string WallSectionUnitSymbol =
            "KMG.Summoning.Infernal.BoneDevil.WallSectionUnit";
        internal const string WallSectionUnitGuid =
            "cc2c011c696e4d3e846815c60245465a";
        internal const string WallSectionUnitName =
            "KMG_Summoning_Infernal_BoneDevil_WallSectionUnit";
        internal const int BarbedHitDice = 12;
        internal const int BarbedBaseCriticalThreatMinimum = 20;
        internal const int BarbedEffectiveCriticalThreatMinimum = 19;
        internal const int BarbedFearRacialDcBonus = 0;
        internal const int BoneHitDice = 10;
        internal const int BoneFearRacialDcBonus = 0;
        internal const int BonePoisonDc = 20;
        internal const int SpellLikeCasterLevel = 12;
        internal const int UnholyBlightSpellLevel = 4;
        internal const int UnholyBlightRadiusFeet = 20;
        internal const int UnholyBlightUses = 1;
        internal const int UnholyBlightOrdinaryDice = 5;
        internal const int UnholyBlightGoodOutsiderDice = 10;
        internal const int WallOfIceSpellLevel = 4;
        internal const int WallOfIceDurationRounds = 120;
        internal const int WallOfIceSectionHitPoints = 36;
        internal const int WallOfIceResidualDiceCount = 1;
        internal const int WallOfIceResidualBonus = 12;
        internal const float WallOfIceSectionLengthMeters = 3.05f;
        internal const float WallOfIceSectionThicknessMeters = .75f;
        internal const float WallOfIceAdjacencyMeters = 1.524f;
        internal const string WallAbilityName =
            "KMG_Summoning_Infernal_BoneDevil_WallOfIce";
        // Installed 2.1.7b native Obscuring Mist area and its effect link,
        // which resolves to no prefab in this installation: the area spawns
        // nothing visible. Wall steam keeps this native area for mechanics
        // and draws the native 20-foot kineticist steam cloud instead.
        internal const string NativeObscuringMistAreaGuid =
            "e09010a73354a794293ebc7b33c2d130";
        internal const string NativeObscuringMistFxAssetId =
            "e19d6ede75196c244b1dc481aed96d70";
        internal const string WallSteamFxAssetId =
            "f5677ddc969727746af75ab89a26167e";
        internal const string WallSteamFxPrefabName =
            "Kinetic_SteamAoe00_20Feet_Aoe_Cycle";

        // Only the Wall's own steam area, and only when the native view
        // spawned no effect of its own.
        internal static bool WallSteamNeedsPresentation(
            bool nativeEffectSpawned, string areaGuid, string sourceAbilityName)
        {
            return !nativeEffectSpawned &&
                String.Equals(areaGuid, NativeObscuringMistAreaGuid,
                    StringComparison.Ordinal) &&
                String.Equals(sourceAbilityName, WallAbilityName,
                    StringComparison.Ordinal);
        }

        // The native kineticist steam cloud also draws burning ground
        // (GroundFireBig, Fire_Top, FireTiny) and its warm Glow. Wall steam
        // only conceals and deals no fire, so those layers are hidden; the
        // smoke, cold vapour, droplets and puddles stay.
        internal static bool WallSteamHidesLayer(string layerName)
        {
            if (layerName == null) return false;
            return layerName.StartsWith("GroundFire", StringComparison.Ordinal) ||
                layerName.StartsWith("Fire", StringComparison.Ordinal) ||
                layerName.StartsWith("Glow", StringComparison.Ordinal);
        }

        // A Wall of Ice section is a summoned unit linked to its Bone Devil.
        // Installed SummonedUnitsController.TickUnitFollowMaster walks any
        // linked summon that is out of combat, CanMove and over 11.7 m from
        // its summoner back toward it (nav4: a standing section walked about
        // 2.8 m, carrying its recast cut and leaving its grid claims behind).
        // CantAct alone leaves native CanMove true; this condition makes the
        // section stationary for every native mover (follow, fear).
        internal const string WallSectionStationaryCondition = "CantMove";

        // Bone Devil natural reach. Installed BlueprintWeaponType.AttackRange
        // returns max(2, raw - 4) feet for raw ranges at or below ten, and
        // UnitDescriptor.GetWeaponRange adds the unit's ReachRange stat. The
        // installed ProperFlanking2 treats a melee weapon whose type range
        // exceeds GameConsts.MinWeaponRange (2 ft) as a reach weapon with a
        // dead zone beside the wielder, which a manufactured reach weapon has
        // but natural reach does not (a Large creature's natural reach
        // threatens adjacent squares). The Bone Devil's 10-foot natural reach
        // is therefore expressed natively through ReachRange: the natural
        // types keep the ordinary raw five feet (two feet native, not a
        // reach weapon) and the unit gains three feet of ReachRange, exactly
        // the three feet the earlier raw nine-foot types contributed.
        internal const int BoneNaturalTypeStoredRangeFeet = 5;
        internal const int BoneNaturalReachBonusFeet = 3;
        internal const int PreviousBoneNaturalTypeStoredRangeFeet = 9;
        internal const int NativeMinWeaponRangeFeet = 2;

        internal static int NativeWeaponTypeRangeFeet(int storedRangeFeet)
        {
            if (storedRangeFeet < 0) throw new ArgumentOutOfRangeException("storedRangeFeet");
            return storedRangeFeet > 10 ? storedRangeFeet :
                Math.Max(NativeMinWeaponRangeFeet, storedRangeFeet - 4);
        }

        // ProperFlanking2's reach-weapon test (Patch_UnitCommand__GetTargetPoint,
        // Patch_UnitEngagementExtension__IsReach): melee and type range above
        // GameConsts.MinWeaponRange.
        internal static bool ProperFlanking2TreatsAsReachWeapon(bool melee, int nativeTypeRangeFeet)
        {
            return melee && nativeTypeRangeFeet > NativeMinWeaponRangeFeet;
        }

        // A grid node's two static walkability layers and its dynamic block
        // counter. Kingmaker's pathfinding fork reports Walkable as Static
        // and no DynamicBlocked counter.
        internal struct WallGridNodeLayers
        {
            internal bool Static;
            internal bool Erosion;
            internal int Dynamic;
            internal bool Walkable { get { return Static && Dynamic == 0; } }
        }

        // Installed GridGraph.UpdateArea for every node in a graph update's
        // erosion reach: static := pre-erosion flag, apply the update's
        // dynamic delta, then pre-erosion flag := static. A claim that holds
        // only the static flag is erased by any native grid update nearby
        // (a cut's combat grid block, a turn-based unit block).
        internal static WallGridNodeLayers NativeGridUpdate(WallGridNodeLayers node, int dynamicDelta)
        {
            node.Static = node.Erosion;
            node.Dynamic += dynamicDelta;
            node.Erosion = node.Static;
            return node;
        }

        // A Wall claim blocks both static layers, so native grid updates keep
        // the node blocked while the claim stands.
        internal static WallGridNodeLayers WallClaimNode(WallGridNodeLayers node)
        {
            node.Static = false;
            node.Erosion = false;
            return node;
        }

        // The last claim owner restores both static layers to what they were
        // before the first claim; the dynamic counter belongs to the cuts and
        // turn-based units.
        internal static WallGridNodeLayers WallRestoreNode(WallGridNodeLayers node,
            bool originalStatic, bool originalErosion)
        {
            node.Static = originalStatic;
            node.Erosion = originalErosion;
            return node;
        }

        internal static bool QualifiesForBarbedDefense(bool hit, bool melee,
            bool manufactured, float rawWeaponReachFeet,
            float ordinaryReachFeet)
        {
            return hit && melee && !(manufactured &&
                rawWeaponReachFeet > ordinaryReachFeet);
        }

        internal static bool QualifiesForClawFear(bool exactClaw, bool hit,
            int finalDamage)
        {
            if (finalDamage < 0)
                throw new ArgumentOutOfRangeException("finalDamage");
            return exactClaw && hit && finalDamage > 0;
        }

        internal static int FearDc(int hitDice, int abilityBonus,
            int racialBonus)
        {
            return 10 + hitDice / 2 + abilityBonus + racialBonus;
        }

        internal static int ImpaleStrengthBonus(int strengthBonus)
        {
            return strengthBonus > 0 ? strengthBonus + strengthBonus / 2 :
                strengthBonus;
        }

        internal static bool ShouldClearReleasedImpaleHold(bool exactOwner,
            bool exactInitiatorTarget, bool reciprocalTarget)
        {
            return exactOwner && exactInitiatorTarget && !reciprocalTarget;
        }

        internal static bool PoisonQualifies(bool exactSting, bool hit,
            int finalDamage)
        {
            return SummonInjuryPoisonPolicy.ShouldDeliver(exactSting, hit,
                finalDamage);
        }

        internal static bool ShouldBreakBoneInvisibility(bool exactOwner,
            bool factActive, bool ownerInGame, bool targetInGame, bool enemy)
        {
            return exactOwner && factActive && ownerInGame && targetInGame &&
                enemy;
        }

        internal static bool SpellAiCanAct(bool exactOwner, bool inCombat,
            bool hasActiveEffect, bool hasGrantedAbility,
            bool grantedAbilityAvailable)
        {
            return exactOwner && inCombat && !hasActiveEffect &&
                hasGrantedAbility && grantedAbilityAvailable;
        }

        internal static int AbilityModifier(int score)
        { return (int)Math.Floor((score - 10) / 2.0); }

        internal static int UnholyBlightWillDc(int liveCharismaScore)
        {
            return 10 + UnholyBlightSpellLevel +
                AbilityModifier(liveCharismaScore);
        }

        internal static int WallOfIceFormationDc(int liveCharismaScore)
        {
            return 10 + WallOfIceSpellLevel +
                AbilityModifier(liveCharismaScore);
        }

        internal static int WallSectionHitPoints(int casterLevel)
        {
            if (casterLevel < 1) throw new ArgumentOutOfRangeException("casterLevel");
            return 3 * casterLevel;
        }

        internal static int WallDurationRounds(int casterLevel)
        {
            if (casterLevel < 1) throw new ArgumentOutOfRangeException("casterLevel");
            return 10 * casterLevel;
        }

        internal static float WallDistanceToFootprint(float alongMeters,
            float acrossMeters, float halfLengthMeters,
            float halfThicknessMeters)
        {
            if (halfLengthMeters <= 0 || halfThicknessMeters <= 0 ||
                float.IsNaN(alongMeters) || float.IsNaN(acrossMeters) ||
                float.IsNaN(halfLengthMeters) ||
                float.IsNaN(halfThicknessMeters) ||
                float.IsInfinity(alongMeters) ||
                float.IsInfinity(acrossMeters) ||
                float.IsInfinity(halfLengthMeters) ||
                float.IsInfinity(halfThicknessMeters))
                throw new ArgumentOutOfRangeException("Wall footprint");
            float outsideAlong = Math.Max(0f,
                Math.Abs(alongMeters) - halfLengthMeters);
            float outsideAcross = Math.Max(0f,
                Math.Abs(acrossMeters) - halfThicknessMeters);
            return (float)Math.Sqrt(outsideAlong * outsideAlong +
                outsideAcross * outsideAcross);
        }

        internal static bool WallPlacementOccupied(float footprintDistance,
            float creatureRadius)
        {
            if (footprintDistance < 0 || creatureRadius < 0 ||
                float.IsNaN(footprintDistance) ||
                float.IsNaN(creatureRadius) ||
                float.IsInfinity(footprintDistance) ||
                float.IsInfinity(creatureRadius))
                throw new ArgumentOutOfRangeException("Wall placement");
            return footprintDistance <= creatureRadius;
        }

        // Wall of Ice: only a creature adjacent to the forming wall may save
        // to disrupt it. A standing Wall of Ice section is an object, not a
        // creature (natively it rolled Reflex, natural 20, and stopped an
        // adjacent second Wall from forming).
        internal static bool WallFormationSaveEligible(string unitBlueprintGuid)
        {
            // The shared formation rule serves the Bone Devil and Ice Devil
            // Walls alike; neither species' private section is a creature.
            return !IsWallSection(unitBlueprintGuid);
        }

        // A Wall of Ice section of either species: an object, not a creature.
        internal static bool IsWallSection(string unitBlueprintGuid)
        {
            return String.Equals(unitBlueprintGuid, WallSectionUnitGuid,
                    StringComparison.Ordinal) ||
                String.Equals(unitBlueprintGuid, IceDevilRules40Policy.WallSectionGuid,
                    StringComparison.Ordinal);
        }

        // Pathfinder bleed is a condition a creature takes at the start of its
        // turn. A Wall of Ice section is an object (Wall of Ice: hit points per
        // inch of ice; the section is not a creature, as for formation saves),
        // so an attacker's Infernal Wound persistent-bleed marker never
        // applies to it. The weapon damage itself still applies.
        internal static bool InfernalWoundExcludesTarget(string unitBlueprintGuid)
        {
            return IsWallSection(unitBlueprintGuid);
        }

        internal static bool WallFormationAdjacent(float footprintDistance,
            float creatureRadius)
        {
            if (footprintDistance < 0 || creatureRadius < 0 ||
                float.IsNaN(footprintDistance) ||
                float.IsNaN(creatureRadius) ||
                float.IsInfinity(footprintDistance) ||
                float.IsInfinity(creatureRadius))
                throw new ArgumentOutOfRangeException("Wall formation");
            return footprintDistance > creatureRadius &&
                footprintDistance <= creatureRadius +
                    WallOfIceAdjacencyMeters;
        }

        internal static bool WallDamageUsesObjectHalf(bool fireEnergy)
        { return !fireEnergy; }

        // Expanded summons carry the native Summoned faction, so a player's
        // Bone Devil is never IsPlayerFaction itself; its summoner decides.
        internal static bool WallFeedbackVisibleToPlayer(
            bool casterIsPlayerFaction, bool summonerIsPlayerFaction)
        { return casterIsPlayerFaction || summonerIsPlayerFaction; }

        // The section is drawn only by its private ice plane. Native body
        // meshes stay hidden; particle effects such as hit FX stay untouched.
        internal static bool WallSuppressesRenderer(bool wallVisual,
            bool bodyMesh)
        { return !wallVisual && bodyMesh; }

        // Ice does not bleed. Installed 2.1.7b BloodType: Common = 0 (red
        // blood), Stone = 9 (the chips native petrification uses). Hit FX,
        // death puddles and the bleeding face all read this per blueprint.
        internal const int NativeCommonBloodType = 0;
        internal const int NativeStoneBloodType = 9;
        internal const int WallSectionBloodType = NativeStoneBloodType;

        internal static bool WallSectionShowsNoBlood(int bloodType)
        { return bloodType != NativeCommonBloodType && bloodType == WallSectionBloodType; }

        // Consecutive native hits on one section: each continues from the last
        // recorded total and adds damage, and the final total reaches its HP.
        internal static bool WallDamageSequenceDestroys(int[] before,
            int[] after, int hitPoints)
        {
            if (before == null || after == null || hitPoints <= 0 ||
                before.Length != after.Length || before.Length == 0 ||
                before[0] != 0) return false;
            for (int index = 0; index < before.Length; index++)
            {
                if (after[index] <= before[index]) return false;
                if (index > 0 && before[index] != after[index - 1])
                    return false;
            }
            return after[after.Length - 1] >= hitPoints;
        }

        internal static int UnholyBlightDiceCount(bool goodOutsider)
        {
            return goodOutsider ? UnholyBlightGoodOutsiderDice :
                UnholyBlightOrdinaryDice;
        }

        internal static int UnholyBlightDieSides(bool goodOutsider)
        { return goodOutsider ? 6 : 8; }

        internal static int UnholyBlightDamage(int rolledDamage, bool good,
            bool evil, bool saved)
        {
            if (rolledDamage < 0)
                throw new ArgumentOutOfRangeException("rolledDamage");
            if (good && evil)
                throw new ArgumentException(
                    "A creature cannot be both good and evil.");
            if (evil) return 0;
            int divisor = good ? (saved ? 2 : 1) : (saved ? 4 : 2);
            return rolledDamage / divisor;
        }

        internal static bool UnholyBlightSickens(bool good, bool evil,
            bool saved)
        {
            if (good && evil)
                throw new ArgumentException(
                    "A creature cannot be both good and evil.");
            return good && !saved;
        }

        internal static string PrivateUnitDonorSymbol(string unitSymbol)
        {
            if (unitSymbol == null)
                throw new ArgumentNullException("unitSymbol");
            if (String.Equals(unitSymbol, IceDevilRules40Policy.WallSectionSymbol,
                StringComparison.Ordinal)) return BoneUnitSymbol;
            return String.Equals(unitSymbol, WallSectionUnitSymbol,
                StringComparison.Ordinal) ? BoneUnitSymbol : null;
        }

        internal static bool WallNodeIntersects(float alongMeters,
            float acrossMeters, float halfLengthMeters,
            float halfThicknessMeters, float nodeSizeMeters)
        {
            if (halfLengthMeters <= 0 || halfThicknessMeters <= 0 ||
                nodeSizeMeters <= 0 || float.IsNaN(alongMeters) ||
                float.IsNaN(acrossMeters) ||
                float.IsNaN(halfLengthMeters) ||
                float.IsNaN(halfThicknessMeters) ||
                float.IsNaN(nodeSizeMeters) ||
                float.IsInfinity(alongMeters) ||
                float.IsInfinity(acrossMeters) ||
                float.IsInfinity(halfLengthMeters) ||
                float.IsInfinity(halfThicknessMeters) ||
                float.IsInfinity(nodeSizeMeters))
                throw new ArgumentOutOfRangeException("Wall footprint");
            // A square grid cell can project by at most half its diagonal on
            // either arbitrarily rotated wall axis. Include intersecting cells,
            // not only cells whose centres happen to lie inside the thin wall.
            float cellProjection = nodeSizeMeters * .70710678f;
            return Math.Abs(alongMeters) <=
                    halfLengthMeters + cellProjection &&
                Math.Abs(acrossMeters) <=
                    halfThicknessMeters + cellProjection;
        }

        internal static bool WallNodeWalkableAfterOwnerRelease(
            bool originalWalkable, bool hasRemainingWallOwner)
        { return hasRemainingWallOwner ? false : originalWalkable; }

        /// <summary>What one wall-retirement attempt may do with the native
        /// grid cut this component applied.</summary>
        internal enum WallRetirementStep
        {
            /// <summary>Submit the paired revert object for the exact graph.</summary>
            RevertNativeCut,
            /// <summary>Complete retirement without a native revert, because
            /// nothing of ours survives on the captured graph.</summary>
            SkipNativeCut,
            /// <summary>Leave retirement pending and retry on a later frame.</summary>
            Defer
        }

        /// <summary>
        /// Decides one step of wall retirement.
        ///
        /// The installed engine makes exactly one of these states transient.
        /// <c>Pathfinding.GridGraph.UpdateArea</c> clears
        /// <c>GraphUpdateObject.updating</c> as its final act, so an update in
        /// flight is queued-not-yet-processed and the pathfinder resolves it
        /// itself: the correct response is to retry on a later frame, never to
        /// abandon retirement and never to block the Unity thread waiting.
        ///
        /// A replaced graph is terminal instead. The captured graph is gone, so
        /// no cut of ours survives on it, and its restoration data must never
        /// reach the replacement. Retirement completes without a native revert.
        ///
        /// Destruction and an exhausted defer budget both force completion:
        /// leaking owned navigation claims is worse than skipping a revert on
        /// a graph that is being torn down anyway.
        /// </summary>
        internal static WallRetirementStep PlanWallRetirement(
            bool gridCutApplied, bool graphExact, bool updateInFlight,
            bool forced, int deferredFrames, int deferFrameBudget)
        {
            if (deferredFrames < 0 || deferFrameBudget < 0)
                throw new ArgumentOutOfRangeException("deferredFrames");
            if (!gridCutApplied) return WallRetirementStep.SkipNativeCut;
            if (!graphExact) return WallRetirementStep.SkipNativeCut;
            if (!updateInFlight) return WallRetirementStep.RevertNativeCut;
            return forced || deferredFrames >= deferFrameBudget
                ? WallRetirementStep.SkipNativeCut
                : WallRetirementStep.Defer;
        }

        /// <summary>The recorded reason a retirement skipped its native
        /// revert, or null when it did not skip one.</summary>
        internal static string WallRetirementSkipReason(
            bool gridCutApplied, bool graphExact, bool updateInFlight,
            bool forced, int deferredFrames, int deferFrameBudget)
        {
            if (PlanWallRetirement(gridCutApplied, graphExact, updateInFlight,
                    forced, deferredFrames, deferFrameBudget) !=
                WallRetirementStep.SkipNativeCut) return null;
            if (!gridCutApplied) return "no-native-cut-applied";
            if (!graphExact) return "captured-graph-replaced";
            return forced ? "forced-while-update-in-flight"
                : "update-in-flight-defer-budget-exhausted";
        }

        internal static void Validate()
        {
            if (AppendedCatalogIdentityCount != 54 ||
                FearDc(BarbedHitDice, 4, BarbedFearRacialDcBonus) != 20 ||
                BarbedBaseCriticalThreatMinimum != 20 ||
                BarbedEffectiveCriticalThreatMinimum != 19 ||
                FearDc(BoneHitDice, 4, BoneFearRacialDcBonus) != 19 ||
                ImpaleStrengthBonus(6) != 9 ||
                ImpaleStrengthBonus(-2) != -2 ||
                !ShouldClearReleasedImpaleHold(true, true, false) ||
                ShouldClearReleasedImpaleHold(false, true, false) ||
                ShouldClearReleasedImpaleHold(true, false, false) ||
                ShouldClearReleasedImpaleHold(true, true, true) ||
                PrivateUnitDonorSymbol(WallSectionUnitSymbol) !=
                    BoneUnitSymbol ||
                !WallNodeIntersects(1.52f, 0f, 1.525f, .375f, .5f) ||
                !WallNodeIntersects(0f, .6f, 1.525f, .375f, .5f) ||
                WallNodeIntersects(2f, 0f, 1.525f, .375f, .5f) ||
                QualifiesForBarbedDefense(true, true, true, 10, 5) ||
                !QualifiesForBarbedDefense(true, true, false, 10, 5) ||
                QualifiesForClawFear(true, true, 0) ||
                !ShouldBreakBoneInvisibility(true, true, true, true, true) ||
                ShouldBreakBoneInvisibility(false, true, true, true, true) ||
                ShouldBreakBoneInvisibility(true, false, true, true, true) ||
                ShouldBreakBoneInvisibility(true, true, false, true, true) ||
                ShouldBreakBoneInvisibility(true, true, true, false, true) ||
                ShouldBreakBoneInvisibility(true, true, true, true, false) ||
                !SpellAiCanAct(true, true, false, true, true) ||
                SpellAiCanAct(false, true, false, true, true) ||
                SpellAiCanAct(true, false, false, true, true) ||
                SpellAiCanAct(true, true, true, true, true) ||
                SpellAiCanAct(true, true, false, false, true) ||
                SpellAiCanAct(true, true, false, true, false) ||
                SpellLikeCasterLevel != 12 ||
                UnholyBlightSpellLevel != 4 ||
                UnholyBlightRadiusFeet != 20 || UnholyBlightUses != 1 ||
                WallOfIceSpellLevel != 4 ||
                WallOfIceDurationRounds != 120 ||
                WallOfIceSectionHitPoints != 36 ||
                WallOfIceResidualDiceCount != 1 ||
                WallOfIceResidualBonus != 12 ||
                WallOfIceFormationDc(18) != 18 ||
                WallDistanceToFootprint(0, 0, 1.525f, .375f) != 0f ||
                Math.Abs(WallDistanceToFootprint(2f, 0, 1.525f,
                    .375f) - .475f) > .0001f ||
                !WallPlacementOccupied(.4f, .5f) ||
                WallPlacementOccupied(.6f, .5f) ||
                !WallFormationAdjacent(.6f, .5f) ||
                WallFormationAdjacent(.4f, .5f) ||
                !WallDamageUsesObjectHalf(false) ||
                WallDamageUsesObjectHalf(true) ||
                UnholyBlightWillDc(18) != 18 ||
                UnholyBlightDiceCount(false) != 5 ||
                UnholyBlightDieSides(false) != 8 ||
                UnholyBlightDiceCount(true) != 10 ||
                UnholyBlightDieSides(true) != 6 ||
                UnholyBlightDamage(40, true, false, false) != 40 ||
                UnholyBlightDamage(40, true, false, true) != 20 ||
                UnholyBlightDamage(40, false, false, false) != 20 ||
                UnholyBlightDamage(40, false, false, true) != 10 ||
                UnholyBlightDamage(40, false, true, false) != 0 ||
                !UnholyBlightSickens(true, false, false) ||
                UnholyBlightSickens(true, false, true) ||
                UnholyBlightSickens(false, false, false))
                throw new InvalidOperationException(
                    "Sprint 35 infernal rule contract changed.");
        }
    }
}
