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

            foreach (string token in new[] {
                "WeaponPresentationDonorFrames",
                "PiercingOneHandedHeldPosition",
                "PiercingOneHandedHeldEuler",
                "PiercingOneHandedHeldRotation * Vector3.up",
                "PiercingOneHandedHeldRotation * Vector3.forward",
                "PiercingOneHandedFirearmForward",
                "forward - 0.468f * up + 0.184f * right",
                "PiercingOneHandedFirearmUp" })
                Assertions.True(contract.Contains(token),
                    "Shared native-donor contract omitted " + token + ".");

            Assertions.True(firearmBuilder.Contains(
                    "PiercingOneHandedHeld(Anchored(Spec(\"Pistol\"") &&
                firearmBuilder.Contains(
                    "PiercingOneHandedHeld(MarkerAuthored(Anchored(Spec(\"PistolDuelist\"") &&
                firearmBuilder.Contains(
                    "PiercingOneHandedHeld(MarkerAuthored(Anchored(Spec(\"PistolLastWord\"") &&
                firearmBuilder.Contains(
                    "PiercingOneHandedHeld(BasisCalibrated(Anchored(Spec(\"Revolver\"") &&
                firearmBuilder.Contains("native-shortspear-held-basis") &&
                firearmBuilder.Contains("spec.TargetAnchorPosition") &&
                firearmBuilder.Contains("SolveRotation"),
                "Every production handgun must derive its held frame from the native PiercingOneHanded donor.");

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
            const string handgunMotionIdentity =
                "weapon-presentation-handgun-motion-evidence";
            const string spearMotionIdentity =
                "weapon-presentation-spear-motion-evidence";
            const string easternMotionIdentity =
                "weapon-presentation-eastern-motion-evidence";
            const string transitionMotionIdentity =
                "weapon-presentation-transition-motion-evidence";
            const string reloadIdentity =
                "weapon-presentation-reload-evidence";
            const string bodyMatrixIdentity =
                "weapon-presentation-body-matrix-evidence";
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
            int reloadEvidenceExecution = runner.IndexOf(
                "WeaponPresentationEvidenceScenario.BeginReload(",
                StringComparison.Ordinal);
            int bodyMatrixEvidenceExecution = runner.IndexOf(
                "WeaponPresentationEvidenceScenario.BeginBodyMatrix(",
                StringComparison.Ordinal);
            Assertions.True(catalog.Contains(identity) &&
                catalog.Contains(motionIdentity) &&
                catalog.Contains(handgunMotionIdentity) &&
                catalog.Contains(spearMotionIdentity) &&
                catalog.Contains(easternMotionIdentity) &&
                catalog.Contains(transitionMotionIdentity) &&
                catalog.Contains(reloadIdentity) &&
                catalog.Contains(bodyMatrixIdentity) &&
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
                runner.Contains(
                    "WeaponPresentationEvidenceScenario.BeginReload(") &&
                runner.Contains("_weaponPresentationReloadEvidence.Poll()") &&
                runner.Contains(
                    "if (_weaponPresentationReloadEvidence.Complete)") &&
                runner.Contains(
                    "WeaponPresentationEvidenceScenario.BeginBodyMatrix(") &&
                runner.Contains("_weaponPresentationBodyMatrixEvidence.Poll()") &&
                runner.Contains(
                    "if (_weaponPresentationBodyMatrixEvidence.Complete)") &&
                workingSaveCompletion >= 0 &&
                evidenceExecution > workingSaveCompletion &&
                motionEvidenceExecution > workingSaveCompletion &&
                transitionMotionEvidenceExecution > workingSaveCompletion &&
                reloadEvidenceExecution > workingSaveCompletion &&
                bodyMatrixEvidenceExecution > workingSaveCompletion &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationEvidence ||") &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationMotionEvidence ||") &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationHandgunMotionEvidence ||") &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationSpearMotionEvidence ||") &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationEasternMotionEvidence ||") &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationTransitionMotionEvidence ||") &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationReloadEvidence ||") &&
                request.Contains(
                    "RuntimeTestScenarioCatalog.WeaponPresentationBodyMatrixEvidence ||") &&
                automation.Contains("'" + identity + "' = [pscustomobject]") &&
                automation.Contains("'" + motionIdentity +
                    "' = [pscustomobject]") &&
                automation.Contains("'" + handgunMotionIdentity +
                    "' = [pscustomobject]") &&
                automation.Contains("'" + spearMotionIdentity +
                    "' = [pscustomobject]") &&
                automation.Contains("'" + easternMotionIdentity +
                    "' = [pscustomobject]") &&
                automation.Contains("'" + transitionMotionIdentity +
                    "' = [pscustomobject]") &&
                automation.Contains("'" + reloadIdentity +
                    "' = [pscustomobject]") &&
                automation.Contains("'" + bodyMatrixIdentity +
                    "' = [pscustomobject]") &&
                preflight.Contains("'" + identity + "'") &&
                preflight.Contains("'" + motionIdentity + "'") &&
                preflight.Contains("'" + handgunMotionIdentity + "'") &&
                preflight.Contains("'" + spearMotionIdentity + "'") &&
                preflight.Contains("'" + easternMotionIdentity + "'") &&
                preflight.Contains("'" + transitionMotionIdentity + "'") &&
                preflight.Contains("'" + reloadIdentity + "'") &&
                preflight.Contains("'" + bodyMatrixIdentity + "'") &&
                automation.Contains(
                    "PermittedSaveName = 'KMG_AUTOMATION_WORKING'") &&
                automation.Contains(
                    "ReadinessBehavior = 'autonomous-working-save'"),
                "Weapon presentation evidence must be an allowlisted autonomous working-save scenario.");

            Assertions.True(runner.Split(new[] {
                    "WeaponPresentationBodyMatrixEvidence"
                }, StringSplitOptions.None).Length - 1 >= 4,
                "Body-matrix evidence must be wired through timeout exclusion, autonomous working-save routing, exception classification, and post-load dispatch.");

            foreach (string token in new[] {
                "BeginBodyMatrix", "BodyMatrixSession",
                "male-medium-light", "female-medium-light", "small-light",
                "male-medium-enlarged", "male-medium-heavy-armor",
                "male-medium-cloak", "EnlargePersonSpellGuid",
                "EnlargePersonBuffGuid", "ArmorProficiencyGroup.Heavy",
                "BlueprintItemEquipmentShoulders", "HasEquipmentLinks",
                "EquipmentEntityAlternatives.EmptyIfNull()",
                "NativeFullPlateItemGuid",
                "559b0b6f194656c428c403a000ceee78",
                "NativeCloakItemGuid",
                "04dff7841c5f499478c91487d9bbdcef",
                "NativeFemaleMediumBodyDonorGuid",
                "f9161aa0b3f519c47acbce01f53ee217",
                "NativeSmallBodyDonorGuid",
                "77c11edb92ce0fd408ad96b40fd27121",
                "ResolvePartyBodyDonor",
                "progression race",
                "actor.Descriptor.Progression.Race.RaceId",
                "raceGuid", "raceId",
                "DonorSource=exact-native-guid",
                "did not materialize a complete native unit view",
                "_fixtureInitialized",
                "weapon-presentation-body-matrix-progress.json",
                "final-case.remove-equipped.begin",
                "fixture.retire.begin", "dispose.all.begin",
                "dispose.entities.complete", "dispose.blueprints.complete",
                "weapon-presentation-body-matrix-cleanup-exception",
                "weapon-presentation-body-matrix-index.json",
                "weapon-presentation-native-body-contracts",
                "weapon-presentation-body-matrix-grips",
                "weapon-presentation-body-matrix-hidden-handguns",
                "weapon-presentation-body-matrix-request-cleanup",
                "336 PNG/JSON pairs", "1,344 labelled views" })
                Assertions.True(scenario.Contains(token),
                    "Body-matrix evidence omitted " + token + ".");

            int donorStart = scenario.IndexOf(
                "private BlueprintUnit ResolveBodyDonor",
                StringComparison.Ordinal);
            int donorEnd = donorStart < 0 ? -1 : scenario.IndexOf(
                "private static bool IsBodyDonor", donorStart,
                StringComparison.Ordinal);
            string donorSource = donorStart < 0 || donorEnd <= donorStart
                ? "" : scenario.Substring(donorStart, donorEnd - donorStart);
            Assertions.True(donorSource.Length > 0 &&
                !donorSource.Contains("GetAllBlueprints"),
                "Body-matrix donor resolution must use exact native identities instead of traversing the complete blueprint library on the game thread.");
            int fixtureEquipmentStart = scenario.IndexOf(
                "private void EquipHeavyArmor", StringComparison.Ordinal);
            int fixtureEquipmentEnd = fixtureEquipmentStart < 0 ? -1 :
                scenario.IndexOf("private void PollBodyReadiness",
                    fixtureEquipmentStart, StringComparison.Ordinal);
            string fixtureEquipmentSource = fixtureEquipmentStart < 0 ||
                fixtureEquipmentEnd <= fixtureEquipmentStart ? "" :
                scenario.Substring(fixtureEquipmentStart,
                    fixtureEquipmentEnd - fixtureEquipmentStart);
            Assertions.True(fixtureEquipmentSource.Length > 0 &&
                !fixtureEquipmentSource.Contains("GetAllBlueprints") &&
                !scenario.Contains("ResolveCompatibleHeavyArmorForProbe") &&
                fixtureEquipmentSource.Contains("RequireExact<BlueprintItemArmor>") &&
                fixtureEquipmentSource.Contains("catch (NullReferenceException)") &&
                fixtureEquipmentSource.Contains("HasEquipmentLinks(blueprint, _actor)") &&
                fixtureEquipmentSource.Contains(
                    "BlueprintItemEquipmentShoulders>("),
                "Body-matrix armor and cloak fixtures must use exact native identities instead of traversing the complete blueprint library on the game thread.");
            int spawnFixtureStart = scenario.IndexOf(
                "private bool SpawnFixture", StringComparison.Ordinal);
            int spawnFixtureEnd = spawnFixtureStart < 0 ? -1 :
                scenario.IndexOf("private void ApplyEnlargePerson",
                    spawnFixtureStart, StringComparison.Ordinal);
            string spawnFixtureSource = spawnFixtureStart < 0 ||
                spawnFixtureEnd <= spawnFixtureStart ? "" :
                scenario.Substring(spawnFixtureStart,
                    spawnFixtureEnd - spawnFixtureStart);
            Assertions.True(spawnFixtureSource.Length > 0 &&
                spawnFixtureSource.Contains(
                    "UnityEngine.Object.Instantiate(fixture.Source)") &&
                spawnFixtureSource.Contains(
                    "_actorBlueprint.Race = fixture.Source.Race") &&
                spawnFixtureSource.Contains(
                    "fixture.Source.Prefab.Load(false)") &&
                spawnFixtureSource.Contains(
                    "if (_settleUpdates < MaximumSettleUpdates) return false") &&
                spawnFixtureSource.Contains(
                    "SpawnUnit(_actorBlueprint,") &&
                spawnFixtureSource.Contains("prefab,") &&
                !spawnFixtureSource.Contains("ApplyVisualBodyDonor") &&
                !spawnFixtureSource.Contains(".Brain = null") &&
                !spawnFixtureSource.Contains("HandsEquipment == null") &&
                scenario.Contains("bool completeView = _actor != null") &&
                scenario.Contains("if (!_fixtureInitialized)"),
                "Body-matrix fixtures must clone each complete native donor contract and defer view-dependent initialization until native spawning has materialized the complete Unity view.");
            int bodyCleanupStart = scenario.IndexOf(
                "private void PollCleanup", spawnFixtureStart,
                StringComparison.Ordinal);
            int bodyCleanupEnd = bodyCleanupStart < 0 ? -1 :
                scenario.IndexOf("private void Finish", bodyCleanupStart,
                    StringComparison.Ordinal);
            string bodyCleanupSource = bodyCleanupStart < 0 ||
                bodyCleanupEnd <= bodyCleanupStart ? "" :
                scenario.Substring(bodyCleanupStart,
                    bodyCleanupEnd - bodyCleanupStart);
            Assertions.True(scenario.Contains(
                    "private void RetireActor()") &&
                scenario.Contains("_retiredActors.Add(_actor)") &&
                scenario.Contains("_retiredBlueprints.Add(_actorBlueprint)") &&
                scenario.Contains("_actor.View.gameObject.SetActive(false)") &&
                scenario.Contains("foreach (UnitEntityData actor in " +
                    "_retiredActors)") &&
                scenario.Contains("foreach (BlueprintUnit blueprint in " +
                    "_retiredBlueprints)") &&
                bodyCleanupSource.Length > 0 &&
                !bodyCleanupSource.Contains("EntityCreator.Tick"),
                "Body-matrix actors must retire between fixtures and final cleanup must not re-enter Unity's invalidated entity-creation queue after disposal.");
            int bodyContractStart = scenario.IndexOf(
                "private static bool IsBodyDonor", StringComparison.Ordinal);
            int bodyContractEnd = bodyContractStart < 0 ? -1 :
                scenario.IndexOf("private static string DescribeBlueprint",
                    bodyContractStart, StringComparison.Ordinal);
            string bodyContractSource = bodyContractStart < 0 ||
                bodyContractEnd <= bodyContractStart ? "" :
                scenario.Substring(bodyContractStart,
                    bodyContractEnd - bodyContractStart);
            Assertions.True(bodyContractSource.Contains("value.Prefab != null") &&
                bodyContractSource.Contains("!value.Body.DisableHands") &&
                !bodyContractSource.Contains("PrimaryHand") &&
                !bodyContractSource.Contains("SecondaryHand"),
                "Visual body donors must not be rejected merely because their default equipment hands are empty.");

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
                "IsIntentionallyHiddenStored",
                "intentionally-hidden-weapon-model",
                "weapon-presentation-handgun-hidden-stored-contract",
                "weapon-presentation-handgun-hidden-stored-transition",
                "weapon-presentation-eastern-custom-sheath-replacement",
                "weapon-presentation-native-locomotion",
                "weapon-presentation-body-relative-turn",
                "weapon-presentation-transition-motion-request-cleanup" })
                Assertions.True(scenario.Contains(token),
                    "Transition/movement evidence omitted " + token + ".");

            foreach (string token in new[] {
                "BeginReload", "ReloadSession", "ReloadCaptureUpdates",
                "120, 160, 200, 220, 240",
                "BuildCases().Where(IsFirearm)",
                "ProductionVariants.Take(7)",
                "BlueprintBootstrap.ReloadTestMusketAbility",
                "ReloadTestMusketRuntime.Evaluate", "new AbilityData(",
                "new UnitUseAbility(", "_reloadCommand.IgnoreCooldown(",
                "_actor.Commands.Run(_reloadCommand)",
                "_reloadCommand.Start()", "_reloadCommand.Tick()",
                "_reloadCommand.ExecutionProcess.Tick()",
                "reload-acted-update-", "ActedCaptureTaken",
                "ReloadRuntimeDiagnostics.Loaded",
                "FirearmDischargeRuntimeDiagnostics.Fired",
                "ResolveFirearmDefinition(value).Reload.RoundsPerAction",
                "RestoreInventoryCount", "reload-ready",
                "reload-update-", "weapon-presentation-reload-index.json",
                "weapon-presentation-native-reload-command",
                "weapon-presentation-reload-action-contract",
                "weapon-presentation-reload-transaction-nonregression",
                "weapon-presentation-reload-request-cleanup" })
                Assertions.True(scenario.Contains(token),
                    "Reload visual evidence omitted " + token + ".");

            foreach (string token in new[] {
                "HandgunMotionVariants", "Native.LightCrossbow",
                "HandgunPiercingDonor", "Native.Shortspear",
                "NativeShortswordItemGuid", "HandgunDualOutcome",
                "RecordPiercingOneHandedDonors",
                "handgunPiercingOneHandedDonors=",
                "WeaponPresentationHandgunMotionEvidence",
                "attack-acted-update-", "DescribeFirearmMuzzleFrame",
                "physicalMuzzleDistanceMeters", "boreWeaponUpAbsDot",
                "physicalMuzzleOffsetWorld",
                "readyRecords", "actedRecords",
                "boreTargetDirectionDot\"] > 0.95f",
                "MinimumRuntimeFrameVectorSquared",
                "physicalWeaponUpDistanceMeters",
                "actorForwardAvailable",
                "actor.View.transform.forward",
                "actorTargetDirectionDot\"] > 0.99f",
                "logicalActorTargetDirectionDot",
                "boreTargetDirectionDot",
                "physicalMuzzleLeadsGripTowardTarget",
                "targetFacingReadyDot=", "readyBoreDot=",
                "did not reach a target-facing live ready frame",
                "_attackTargetPrepared",
                "The installed probe supplies the real ranged approach",
                "finalIssued",
                "lost exact native start readiness",
                "DescribePiercingOneHandedDonorFrame",
                "piercingDonorFrameSource",
                "piercingDonorNegativeYTargetDot",
                "piercingDonorRendererGeometry",
                "dual-wield-firearm-main-combat-ready",
                "dual-wield-firearm-offhand-combat-ready",
                "GetWeaponModel(true)", "ExactEquippedFirearmResolver",
                "resolverSelectedFirearm", "RemoveDualEquipped",
                "TryWriteMotionFailureEvidence",
                "weapon-presentation-handgun-motion-failure.json",
                "_handgunMotionFailureCount",
                "failureSequence",
                "<no-records>",
                "TryInterruptHandEquipmentAnimation",
                "m_Coroutine", "InterruptAnimation",
                "_actor.CombatState.LeaveCombat()",
                "primarySlotEmpty", "secondarySlotEmpty",
                "removedPrimaryRenderable", "removedSecondaryRenderable",
                "weapon-presentation-handgun-motion-index.json",
                "weapon-presentation-handgun-muzzle-frame",
                "weapon-presentation-handgun-valid-dual-wield",
                "weapon-presentation-handgun-motion-contact-sheets" })
                Assertions.True(scenario.Contains(token),
                    "Handgun motion evidence omitted " + token + ".");

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

            int nativeControlStart = scenario.IndexOf(
                "private static EvidenceCase BuildNativeControl",
                StringComparison.Ordinal);
            int nativeControlEnd = nativeControlStart < 0 ? -1 :
                scenario.IndexOf("private static string FamilyFor",
                    nativeControlStart, StringComparison.Ordinal);
            string nativeControlSource = nativeControlStart < 0 ||
                nativeControlEnd <= nativeControlStart ? "" :
                scenario.Substring(nativeControlStart,
                    nativeControlEnd - nativeControlStart);
            Assertions.True(
                scenario.Contains("926d02c8af0352b46874791d4de9764f") &&
                scenario.Contains("2ca0329871f14a27922370f17ea4d15d") &&
                scenario.Contains("0782c8ca4b6c4634a0f6dabbed796211") &&
                nativeControlSource.Length > 0 &&
                !nativeControlSource.Contains("GetAllBlueprints"),
                "Native presentation controls must use exact previously observed item identities without game-thread blueprint scans.");

            Assertions.True(scenario.Contains("_cases.Length != 28") &&
                scenario.Contains("production.Length != 22") &&
                scenario.Contains("controls.Length != 6") &&
                scenario.Contains("SequenceEqual(ProductionVariants)") &&
                scenario.Contains("BuildNativeControl") &&
                scenario.Contains("ReferenceEquals(preferred.VisualParameters.Model,") &&
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
