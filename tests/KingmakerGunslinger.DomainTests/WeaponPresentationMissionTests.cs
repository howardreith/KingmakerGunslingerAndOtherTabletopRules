using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class WeaponPresentationMissionTests
    {
        internal static void SemanticFrameContractIsCompleteAndShared()
        {
            string contract = Read("src", "KingmakerGunslinger", "Assets",
                "WeaponPresentationSemanticFrame.cs");
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            string firearmBuilder = Read("tools", "unity",
                "BuildFirearmBundles.cs");
            string spearBuilder = Read("tools", "unity",
                "BuildElvenBranchedSpearBundle.cs");
            string easternBuilder = Read("tools", "unity",
                "BuildEasternWeaponsBundle.cs");
            string firearmRuntime = Read("src", "KingmakerGunslinger",
                "Assets", "FirearmAssetRuntime.cs");
            string spearRuntime = Read("src", "KingmakerGunslinger",
                "Assets", "ElvenBranchedSpearAssetRuntime.cs");
            string easternRuntime = Read("src", "KingmakerGunslinger",
                "Assets", "EasternWeaponAssetRuntime.cs");

            foreach (string token in new[] { "GripMarker", "ButtMarker",
                "SupportMarker", "WeaponUpMarker", "HeadUpMarker",
                "WeaponForwardMarker", "BladeNormalMarker",
                "RequireWithForwardMarker", "semantic frame is degenerate",
                "RequireWithForwardMarkerAndButtSupport",
                "forward and secondary axes are collinear",
                "tip/butt polarity is reversed",
                "support-hand target is outside the grip-to-tip interval",
                "support-hand target is outside the butt-to-grip handle interval",
                "reflected, zero, or non-finite local scale",
                "renderer-bound forward end", "renderer-bound rear end",
                "Quaternion.LookRotation", "targetBasis * Quaternion.Inverse(sourceBasis)",
                "targetGrip - rotation * (sourceGrip * scale)" })
                Assertions.True(contract.Contains(token),
                    "Shared semantic-frame contract omitted " + token + ".");

            Assertions.True(project.Contains(
                "Assets\\WeaponPresentationSemanticFrame.cs"),
                "The runtime project does not compile the shared frame contract.");
            foreach (string script in new[] { "Prepare-UnityAssets.ps1",
                "Prepare-ElvenBranchedSpearAssets.ps1",
                "Prepare-EasternWeaponAssets.ps1" })
                Assertions.True(Read("scripts", script).Contains(
                    "WeaponPresentationSemanticFrame.cs"),
                    script + " does not stage the shared contract into Unity.");

            Assertions.True(firearmBuilder.Contains("WeaponUpMarker") &&
                firearmBuilder.Contains("ValidateRendererEndpoints") &&
                firearmBuilder.Contains("KMG_FIREARM_SEMANTIC_FRAME") &&
                firearmRuntime.Contains("weapon-up-missing") &&
                firearmRuntime.Contains("ValidateIndependentHeldAndStored"),
                "Firearm authoring/runtime does not enforce a complete independent frame.");
            Assertions.True(spearBuilder.Contains("KMG_Grip") &&
                spearBuilder.Contains("KMG_Support") &&
                spearBuilder.Contains("KMG_Tip") &&
                spearBuilder.Contains("KMG_Butt") &&
                spearBuilder.Contains("KMG_HeadUp") &&
                spearBuilder.Contains("KMG_Back") &&
                spearBuilder.Contains("NativeLongspearHeldEuler") &&
                spearBuilder.Contains("NativeLongspearStoredEuler") &&
                spearBuilder.Contains("SolveRotation") &&
                spearBuilder.Contains("SolveTranslation") &&
                spearBuilder.Contains("HeadUpMarker") &&
                spearBuilder.Contains("ValidateSecondaryAsPlaneNormal") &&
                spearBuilder.Contains("KMG_SPEAR_SEMANTIC_FRAME") &&
                spearRuntime.Contains("HeadUpMarker") &&
                spearRuntime.Contains("EquipmentOffsets") &&
                spearRuntime.Contains("IkTargetLeftHand") &&
                spearRuntime.Contains("HasCalibratedDonorFrame") &&
                spearRuntime.Contains(
                    "held and stored presentations share an incompatible transform"),
                "Spear authoring/runtime does not enforce mesh-authored polarity, donor-derived roll, held IK, and independent stored presentation.");
            Assertions.True(easternBuilder.Contains("BladeNormalMarker") &&
                easternBuilder.Contains("KMG_Grip") &&
                easternBuilder.Contains("KMG_Tip") &&
                easternBuilder.Contains("KMG_Butt") &&
                easternBuilder.Contains("KMG_Forward") &&
                easternBuilder.Contains("KMG_BladeNormal") &&
                easternBuilder.Contains("KMG_Edge") &&
                easternBuilder.Contains("KMG_Stored") &&
                easternBuilder.Contains("SolveRotation") &&
                easternBuilder.Contains("SolveTranslation") &&
                easternBuilder.Contains("StoredMount") &&
                easternBuilder.Contains("native-Scimitar") &&
                easternBuilder.Contains("native-BastardSword") &&
                easternBuilder.Contains("native-Greatsword") &&
                easternBuilder.Contains("ValidateSecondaryAsPlaneNormal") &&
                easternBuilder.Contains("KMG_EASTERN_SEMANTIC_FRAME") &&
                easternRuntime.Contains("BladeNormalMarker") &&
                easternRuntime.Contains("ValidateRendererEndpoints") &&
                easternRuntime.Contains("StoredPrefabs") &&
                easternRuntime.Contains("m_WeaponBeltModel") &&
                easternRuntime.Contains("m_WeaponSheathModel") &&
                easternRuntime.Contains(".SetValue(visual, null)") &&
                easternRuntime.Contains("visual.SheathModel == null") &&
                easternRuntime.Contains("PreservesUnreplacedDonorFields") &&
                easternRuntime.Contains("EquipmentOffsets") &&
                easternRuntime.Contains("IkTargetLeftHand") &&
                easternRuntime.Contains("HasCalibratedDonorFrame") &&
                easternRuntime.Contains(
                    "held and stored presentations share an incompatible transform"),
                "Eastern authoring/runtime does not enforce mesh-authored blade polarity, donor-derived roll, held-only Nodachi IK, independent stored presentation, and custom-clone-only sheath replacement.");
        }

        internal static void EvidenceScenarioIsGuardedAndStateLabelled()
        {
            string scenario = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "WeaponPresentationEvidenceScenario.cs");
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string request = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRequest.cs");
            string automation = Read("scripts", "RuntimeAutomation.Common.ps1");
            string preflight = Read("scripts",
                "Test-RuntimeScenarioPreflight.ps1");

            const string identity = "weapon-presentation-evidence";
            const string motionIdentity =
                "weapon-presentation-motion-evidence";
            const string spearMotionIdentity =
                "weapon-presentation-spear-motion-evidence";
            const string easternMotionIdentity =
                "weapon-presentation-eastern-motion-evidence";
            const string transitionMotionIdentity =
                "weapon-presentation-transition-motion-evidence";
            int workingSaveCompletion = runner.IndexOf(
                "if (_workingSaveSmoke.Complete)", StringComparison.Ordinal);
            int evidenceExecution = runner.IndexOf(
                "WeaponPresentationEvidenceScenario.Begin(",
                StringComparison.Ordinal);
            int motionEvidenceExecution = runner.IndexOf(
                "WeaponPresentationEvidenceScenario.BeginMotion(",
                StringComparison.Ordinal);
            int transitionMotionEvidenceExecution = runner.IndexOf(
                ".BeginTransitionMotion(_context, _request)",
                StringComparison.Ordinal);
            Assertions.True(catalog.Contains(identity) &&
                catalog.Contains(motionIdentity) &&
                catalog.Contains(spearMotionIdentity) &&
                catalog.Contains(easternMotionIdentity) &&
                catalog.Contains(transitionMotionIdentity) &&
                runner.Contains("WeaponPresentationEvidenceScenario.Begin(") &&
                runner.Contains("_weaponPresentationEvidence.Poll()") &&
                runner.Contains("if (_weaponPresentationEvidence.Complete)") &&
                runner.Contains(
                    "WeaponPresentationEvidenceScenario.BeginMotion(") &&
                runner.Contains("_weaponPresentationMotionEvidence.Poll()") &&
                runner.Contains(
                    "if (_weaponPresentationMotionEvidence.Complete)") &&
                runner.Contains(
                    "_weaponPresentationTransitionMotionEvidence.Poll()") &&
                runner.Contains(
                    "if (_weaponPresentationTransitionMotionEvidence.Complete)") &&
                workingSaveCompletion >= 0 &&
                evidenceExecution > workingSaveCompletion &&
                motionEvidenceExecution > workingSaveCompletion &&
                transitionMotionEvidenceExecution > workingSaveCompletion &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationEvidence ||") &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationMotionEvidence ||") &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationSpearMotionEvidence ||") &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationEasternMotionEvidence ||") &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationTransitionMotionEvidence ||") &&
                automation.Contains("'" + identity + "' = [pscustomobject]") &&
                automation.Contains("'" + motionIdentity +
                    "' = [pscustomobject]") &&
                automation.Contains("'" + spearMotionIdentity +
                    "' = [pscustomobject]") &&
                automation.Contains("'" + easternMotionIdentity +
                    "' = [pscustomobject]") &&
                automation.Contains("'" + transitionMotionIdentity +
                    "' = [pscustomobject]") &&
                preflight.Contains("'" + identity + "'") &&
                preflight.Contains("'" + motionIdentity + "'") &&
                preflight.Contains("'" + spearMotionIdentity + "'") &&
                preflight.Contains("'" + easternMotionIdentity + "'") &&
                preflight.Contains("'" + transitionMotionIdentity + "'") &&
                automation.Contains(
                    "PermittedSaveName = 'KMG_AUTOMATION_WORKING'") &&
                automation.Contains(
                    "ReadinessBehavior = 'autonomous-working-save'"),
                "Weapon presentation evidence must be an allowlisted autonomous working-save scenario.");

            foreach (string token in new[] {
                "SpearMotionVariants", "Native.Longspear",
                "TryResolveSpearPhysicalEndpoints",
                "authored-renderer-bound-Tip/Butt",
                "native-TH_LongspearKnight1-renderer-positive-Y-head",
                "physicalTipLeadsTargetDirection",
                "physicalTipTargetProjectionMeters",
                "actedTipLeadingRecords ==",
                "actedEndpointRecords.Length",
                "weapon-presentation-branched-spear-motion-index.json",
                "weapon-presentation-spear-physical-endpoint-evidence" })
                Assertions.True(scenario.Contains(token),
                    "Spear motion evidence omitted " + token + ".");

            foreach (string token in new[] {
                "EasternMotionVariants", "Native.Scimitar",
                "Native.BastardSword", "Native.Greatsword",
                "TryResolveEasternBladeFrame",
                "authored-renderer-bound-Tip/Butt+WeaponForward/BladeNormal/CuttingEdge",
                "native-renderer-local-+Y-forward/+X-blade-normal/-Z-cutting-edge",
                "physicalBladeLengthMeters",
                "physicalTipAheadAlongBladeForward",
                "bladeNormalForwardAbsDot", "cuttingEdgeForwardAbsDot",
                "cuttingEdgeBladeNormalAbsDot", "cuttingEdgePolarityDot",
                "actedVariants == _motionVariants.Length",
                "weapon-presentation-eastern-motion-index.json",
                "weapon-presentation-eastern-physical-blade-frame" })
                Assertions.True(scenario.Contains(token),
                    "Eastern motion evidence omitted " + token + ".");

            foreach (string token in new[] {
                "BeginTransitionMotion", "TransitionMotionSession",
                "MainHandEquip", "MainHandUnequip",
                "CombatStateTransitionAnimating", "m_Coroutine",
                "AreHandsBusyWithAnimation.Value", "UnitMoveTo",
                "_actor.Commands.Contains(_moveCommand)",
                "_moveCommand.Executor", "Pathfinding.ForcedPath",
                "AgentASP.ForcePath(path, 0.1f)",
                "candidate.node.Area == _movementStartArea",
                "candidate.node.GraphIndex == _movementGraphIndex",
                "MovementAgent.TickMovement(",
                "Game.Instance.TimeController.DeltaTime",
                "MovementAgent.IsReallyMoving", "MovementAgent.WantsToMove",
                "_actor.View.IsMoving()", "MovementAgent.Velocity",
                "_movementVelocityObserved", "m_ShoudBeInCombat",
                "without UnitCombatState.JoinCombat",
                "Game.Instance.Player.IsInCombat",
                "TurnBased.Controllers.CombatController",
                "IsActorCurrentTurn()", "IsPreventingMovement",
                "IsCommandsPreventMovement",
                "UnitAnimationType.LocoMotion", "ForceLookAt",
                "equip-transition", "unequip-transition", "turned-right",
                "weapon-presentation-transition-motion-index.json",
                "weapon-presentation-native-equip-unequip-transitions",
                "weapon-presentation-eastern-custom-sheath-replacement",
                "weapon-presentation-native-locomotion",
                "weapon-presentation-body-relative-turn",
                "weapon-presentation-transition-motion-request-cleanup" })
                Assertions.True(scenario.Contains(token),
                    "Transition/movement evidence omitted " + token + ".");

            foreach (string token in new[] {
                "PistolService", "PistolDuelist", "PistolLastWord",
                "RevolverService", "MusketService", "BlunderbussService",
                "RifleService", "SpearClassic", "SpearThorn", "SpearCrown",
                "WakizashiClassic", "WakizashiPetal", "WakizashiMoon",
                "WakizashiCapstone", "KatanaClassic", "KatanaReed",
                "KatanaRegal", "KatanaCapstone", "NodachiClassic",
                "NodachiCleaver", "NodachiTitan", "NodachiCapstone" })
                Assertions.True(scenario.Contains(token),
                    "Evidence catalog omitted production variant " + token + ".");

            foreach (string token in new[] { "LightCrossbow",
                "HeavyCrossbow", "Longspear", "Scimitar", "BastardSword",
                "Greatsword" })
                Assertions.True(scenario.Contains(
                    "new NativeControlSpec(\"" + token + "\""),
                    "Evidence catalog omitted native control " + token + ".");

            Assertions.True(scenario.Contains("_cases.Length != 28") &&
                scenario.Contains("production.Length != 22") &&
                scenario.Contains("controls.Length != 6") &&
                scenario.Contains("SequenceEqual(ProductionVariants)") &&
                scenario.Contains("BuildNativeControl") &&
                scenario.Contains("ReferenceEquals(item.VisualParameters.Model,") &&
                scenario.Contains("type.VisualParameters.Model") &&
                scenario.Contains("HandsEquipment.UpdateAll()") &&
                scenario.Contains("HandsEquipment.ForceSwitch(false)") &&
                scenario.Contains("HandsEquipment.ForceSwitch(true)") &&
                scenario.Contains("GetWeaponModel(false)") &&
                scenario.Contains("HandsEquipment.InCombat") &&
                scenario.Contains("_fixtureBodyRenderers") &&
                scenario.Contains("empty-handed disposable humanoid") &&
                scenario.Contains("Game.Instance.State.Units.All") &&
                scenario.Contains("Game.Instance.Player.Party") &&
                scenario.Contains("MaximumSettleUpdates") &&
                scenario.Contains("PollMaterialization()") &&
                scenario.Contains("PollRemoval()") &&
                scenario.Contains("_presentationState + \"-default-medium-\"") &&
                scenario.Contains("front-right-three-quarter") &&
                scenario.Contains("aabbOverlapVolume") &&
                scenario.Contains("modelLocalRendererBoundsSize") &&
                scenario.Contains("modelLocalRendererBoundsCenterComponents") &&
                scenario.Contains("modelLocalRendererBoundsSizeComponents") &&
                scenario.Contains("modelLocalMajorAxis") &&
                scenario.Contains("modelLocalMinorAxis") &&
                scenario.Contains("modelLocalBoundsSourceCount") &&
                scenario.Contains("filter.sharedMesh.bounds") &&
                scenario.Contains("skinned.localBounds") &&
                scenario.Contains("weapon-presentation-native-local-geometry-invariant") &&
                scenario.Contains("SameComponents") &&
                scenario.Contains("0.00001") &&
                scenario.Contains("semanticLocators") &&
                scenario.Contains("presentationRole") &&
                scenario.Contains("weapon-presentation-native-donor-controls") &&
                scenario.Contains("no attack, reload, or movement claim") &&
                scenario.Contains("body-centered-capped") &&
                scenario.Contains("SameReferences(_unitsBefore") &&
                scenario.Contains("SameReferences(_partyBefore") &&
                scenario.Contains("File.WriteAllBytes(pngPath, png)"),
                "Evidence must settle exact native stored and held models across game updates, capture four labelled views with outlier-safe framing, retain clipping diagnostics and honest claim limits, and prove exact cleanup.");

            foreach (string token in new[] {
                "LongGunMotionVariants", "AttackCaptureUpdates",
                "BeginMotion", "MotionSession", "combat-ready",
                "UnitAttack.CreateAttackCommand", "_actor.Commands.Run(",
                "_attackCommand.Start()",
                "AstarPath.active.GetNearest", "NearestNavigable",
                "_attackCommand.CanStart",
                "_attackCommand.IsUnitEnoughClose",
                "_attackCommand.ApproachRadius",
                "_attackCommand.NeedLoS", "PrepareAttackStart",
                "commandTargetPlacement", "commandTargetAttempts",
                "_attackCommand.IsSingleAttack = true",
                "_attackCommand.Tick()", "commandExplicitTickCount",
                "AnimationActedObserved",
                "FirearmDischargeRuntimeDiagnostics.Fired",
                "LoadedRoundsAfter", "weapon-presentation-native-attack-command",
                "weapon-presentation-firearm-discharge-nonregression",
                "_target.Descriptor.Stats.HitPoints.BaseValue = 10000",
                "_target.Descriptor.Damage = 0", "targetHitPoints",
                "targetDamage", "rigContacts", "dominantHandToGrip",
                "supportHandToTarget", "dominantClavicleToButt",
                "supportTargetSource", "supportTargetPath",
                "EquipmentOffsets.IkTargetLeftHand",
                "R_WeaponBone", "L_Hand", "R_Clavicle",
                "SameReferences(_unitsBefore", "SameReferences(_partyBefore",
                "modelWorldForward", "modelWorldUp", "modelWorldRight" })
                Assertions.True(scenario.Contains(token),
                    "Long-gun motion evidence omitted " + token + ".");

            Assertions.False(scenario.Contains("SaveGame") ||
                scenario.Contains("QuickSave") || scenario.Contains("LoadGame") ||
                scenario.Contains("KMG_AUTOMATION_BASELINE") ||
                scenario.Contains("Camera.main.transform.rotation =") ||
                scenario.Contains("actor.View.transform.position ="),
                "The evidence fixture may not save, load, target the protected baseline, or camera-relative-correct a weapon/actor.");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
