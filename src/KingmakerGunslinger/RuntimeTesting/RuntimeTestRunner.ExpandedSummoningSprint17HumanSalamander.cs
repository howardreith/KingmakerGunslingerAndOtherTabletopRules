using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private int _humanSalamanderCheckpointSequence;

        // Flushed request-local observations survive a native process crash.
        // They are never a final result, save-write audit or qualification.
        private void WriteHumanSalamanderCheckpoint(string stage, JObject row)
        {
            int sequence = ++_humanSalamanderCheckpointSequence;
            string file = "sprint17-human-stage-" + sequence.ToString("D2") + "-" + stage + ".json";
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, file),
                new JObject { ["schemaVersion"] = 1, ["runId"] = _request.RunId,
                    ["scenario"] = _request.Scenario, ["sequence"] = sequence, ["stage"] = stage,
                    ["recordedAtUtc"] = DateTime.UtcNow.ToString("o"), ["frame"] = Time.frameCount,
                    ["isFinalResult"] = false, ["qualifies"] = false, ["nativeSaveWriteAudit"] = "PENDING_FINAL",
                    ["assertionsSoFar"] = JArray.FromObject(_serpentineBodyAssertions),
                    ["observation"] = row.DeepClone() }.ToString(Formatting.Indented));
        }

        private static JArray HumanSalamanderRendererObservation(UnitEntityData owner)
        {
            return new JArray(owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(s => new JObject {
                ["name"] = s.name, ["mesh"] = s.sharedMesh == null ? null : s.sharedMesh.name,
                ["vertices"] = s.sharedMesh == null ? (int?)null : s.sharedMesh.vertexCount,
                ["palette"] = s.bones.Length, ["root"] = s.rootBone == null ? null : s.rootBone.name,
                ["enabled"] = s.enabled, ["activeSelf"] = s.gameObject.activeSelf,
                ["activeInHierarchy"] = s.gameObject.activeInHierarchy, ["updateWhenOffscreen"] = s.updateWhenOffscreen,
                ["components"] = new JArray(s.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().FullName)) }));
        }

        private void CheckHumanSalamander(string id, bool pass, JToken evidence, string expected)
        {
            _serpentineBodyAssertions.Add(Assertion("sprint17-human-salamander-" + id, expected,
                evidence == null ? "no observation" : evidence.ToString(Formatting.None), pass,
                "Request-local original hybrid research; not production, printed-profile or whole-Sprint qualification."));
        }

        private IEnumerable<int> ReviewSprint17HumanSalamander(ExpandedSummoningCorrectionFixture fixture)
        {
            Sprint16ManualSummonControl control;
            UnitEntityData owner = SummonSprint17BodyCarrier(fixture, "salamander", out control, humanTailResearch: true);
            SetExpandedSummoningBrainActive(owner, false);
            var resources = new List<UnityEngine.Object>();
            UnityEngine.Object[] borrowed = new UnityEngine.Object[0];
            var row = new JObject { ["key"] = "salamander-human-tail",
                ["scope"] = "closed human/original-tail integration research; published Salamander unchanged" };
            _serpentineBodyRows.Add(row);
            try
            {
                WriteHumanSalamanderCheckpoint("spawned-before-settlement", row);
                bool capital = Game.Instance.CurrentlyLoadedArea != null && Game.Instance.CurrentlyLoadedArea.IsCapital;
                if (capital) owner.Descriptor.Master = Game.Instance.Player.MainCharacter;
                else if (!owner.Faction.IsDirectlyControllable)
                    owner.Descriptor.SwitchFactions(Game.Instance.Player.MainCharacter.Value.Faction, false);
                string floorDetail;
                PlaceExpandedSummoningUnit(owner, FindExpandedSummoningArtPoint(fixture.Caster, out floorDetail));
                row["floorSurvey"] = floorDetail;
                if (!Game.Instance.State.AwakeUnits.Contains(owner)) Game.Instance.State.AwakeUnits.Add(owner);
                int settle = 0;
                while (++settle <= 600)
                {
                    if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                    yield return 0;
                    if (settle >= 60 && owner.IsDirectlyControllable && owner.Descriptor.State.CanAct &&
                        Sprint17BodyIntact(owner.View, SalamanderHumanBindingPolicy.BodyName)) break;
                }
                bool ready = control.Matched == 1 && owner.Get<UnitPartSummonedMonster>() != null &&
                    owner.Get<UnitPartSummonedMonster>().IsDirectlyControllable && owner.IsDirectlyControllable &&
                    Sprint17BodyIntact(owner.View, SalamanderHumanBindingPolicy.BodyName);
                row["nativeControl"] = Sprint16ControlObservation(owner); row["settlementFrames"] = settle;
                CheckHumanSalamander("native-settlement", ready, row["nativeControl"], "intact native human frame and one exact manual summon-part rule");
                WriteHumanSalamanderCheckpoint("native-settlement", row);
                if (!ready) throw new InvalidOperationException("Native human appearance/control did not settle.");
                var originalSet = owner.View.AnimationManager.AnimationSet;
                // Read before any attachment/rollback. The first live attempt
                // rejected one combined guard, so expose exact native vs
                // effective lookup identities and patch provenance separately.
                HumanSalamanderActionSetBoundary(owner, row);
                var nativeActions = originalSet.Actions.ToArray();
                var skins = owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(s =>
                    new { Skin = s, Mesh = s.sharedMesh, Bones = s.bones, Quality = s.quality,
                        Enabled = s.enabled, ActiveSelf = s.gameObject.activeSelf, Root = s.rootBone,
                        Offscreen = s.updateWhenOffscreen }).ToArray();
                var staticMeshes = owner.View.GetComponentsInChildren<MeshFilter>(true).Select(f =>
                    new { Filter = f, Mesh = f.sharedMesh }).ToArray();
                row["nativeRendererCensus"] = new JArray(skins.Select(s => new JObject {
                    ["name"] = s.Skin.name, ["palette"] = s.Bones.Length,
                    ["uniqueTransforms"] = s.Bones.Distinct().Count(), ["mesh"] = s.Mesh == null ? null : s.Mesh.name }));
                row["nativeHandActions"] = Sprint17NativeHandAttackCensus(owner);
                row["nativeRendererState"] = HumanSalamanderRendererObservation(owner);
                WriteHumanSalamanderCheckpoint("native-census-before-rollback", row);
                UnityEngine.Object[] rollback = new UnityEngine.Object[0];
                bool rejected; string outcome;
                try
                {
                    SalamanderHumanVisualAttachment.PostSwapFaultForTest = () => {
                        rollback = owner.View.GetComponent<SalamanderHumanVisualAttachment>().CaptureOwnedResources();
                        throw new InvalidOperationException("owned human/tail post-swap rollback drill");
                    };
                    rejected = !SalamanderHumanVisualAttachment.TryAttach(owner.View, _context, out outcome);
                }
                finally { SalamanderHumanVisualAttachment.PostSwapFaultForTest = null; }
                row["rollbackOutcome"] = outcome;
                WriteHumanSalamanderCheckpoint("rollback-returned-before-render", row);
                yield return 0; yield return 0;
                bool restored = skins.All(s => s.Skin != null && ReferenceEquals(s.Skin.sharedMesh, s.Mesh) &&
                    s.Skin.bones.SequenceEqual(s.Bones) && s.Skin.quality == s.Quality &&
                    s.Skin.enabled == s.Enabled && s.Skin.gameObject.activeSelf == s.ActiveSelf &&
                    ReferenceEquals(s.Skin.rootBone, s.Root) && s.Skin.updateWhenOffscreen == s.Offscreen) &&
                    staticMeshes.All(s => s.Filter != null && ReferenceEquals(s.Filter.sharedMesh, s.Mesh)) &&
                    ReferenceEquals(owner.View.AnimationManager.AnimationSet, originalSet) &&
                    originalSet.Actions.SequenceEqual(nativeActions);
                // 29 required owned objects: eight assets/components/root GO,
                // its Transform, and ten tail GO/Transform pairs. Controller
                // material clones add to this; no empty mesh is allocated.
                CheckHumanSalamander("rollback", rejected && rollback.Length >= 29 && rollback.All(v => v == null) && restored,
                    new JObject { ["resources"] = rollback.Length, ["remaining"] = rollback.Count(v => v != null),
                        ["restored"] = restored, ["outcome"] = outcome },
                    "post-swap fault restores all exact native body/static/action references and destroys every owned object");
                row["rollbackRendererState"] = HumanSalamanderRendererObservation(owner);
                WriteHumanSalamanderCheckpoint("rollback-settled-before-binding", row);
                if (!restored || rollback.Length == 0) throw new InvalidOperationException("Human binding could not reach or restore the rollback seam: " + outcome);
                bool attached = SalamanderHumanVisualAttachment.TryAttach(owner.View, _context, out outcome);
                row["attachmentOutcome"] = outcome;
                var attachment = owner.View.GetComponent<SalamanderHumanVisualAttachment>();
                CheckHumanSalamander("binding", attached && attachment != null && attachment.Live,
                    outcome, "one owned body/ten original drivers/one Tail action, all24 human actions preserved");
                row["attachedRendererState"] = HumanSalamanderRendererObservation(owner);
                WriteHumanSalamanderCheckpoint("binding-returned-before-render", row);
                if (!attached || attachment == null || !attachment.Live) throw new InvalidOperationException(outcome);
                resources.AddRange(attachment.CaptureOwnedResources());
                borrowed = attachment.BorrowedResources();
                // Do not force a camera render in the swap frame: the native
                // skinning/render loop must first consume the new palette.
                int bindingFrame = Time.frameCount;
                yield return 0;
                row["restFrameBoundary"] = new JObject { ["bindingFrame"] = bindingFrame,
                    ["observationFrame"] = Time.frameCount, ["advanced"] = Time.frameCount > bindingFrame };
                WriteHumanSalamanderCheckpoint("first-native-frame-after-binding", row);
                if (Time.frameCount <= bindingFrame)
                    throw new InvalidOperationException("No native frame boundary after human skin swap.");
                row["bodyMaterial"] = Sprint17BodyMaterialResearch(attachment.Body.sharedMaterial);
                row["restPose"] = Sprint17OriginalBodySample(owner, attachment.Body);
                WriteHumanSalamanderCheckpoint("rest-sampled-before-supporting-frame", row);
                row["supportingRestFrame"] = CaptureSprint17OriginalBodyFrame(owner, attachment.Body, "human-salamander-rest");
                CheckHumanSalamander("original-rest", (bool)row["restPose"]["finite"] && (bool)row["restPose"]["poseFinite"] &&
                    attachment.Body.bones.Length == 36 && attachment.Body.sharedMesh.vertexCount == 2198 &&
                    attachment.AuxiliaryGeometrySuppressed && skins.All(s => s.Skin.enabled == s.Enabled &&
                        s.Skin.gameObject.activeSelf == s.ActiveSelf && ReferenceEquals(s.Skin.rootBone, s.Root) &&
                        s.Skin.updateWhenOffscreen == s.Offscreen) &&
                    Sprint17BodyIntact(owner.View, SalamanderHumanBindingPolicy.BodyName),
                    row["restPose"], "finite intact original body; no native skin visible or visibility forcing");
                WriteHumanSalamanderCheckpoint("rest-frame-complete-before-movement", row);
                Vector3 origin = owner.Position;
                var move = new UnitMoveTo(Sprint17BodyMoveDestination(origin));
                move.Init(owner); owner.Commands.Run(move);
                WriteHumanSalamanderCheckpoint("movement-queued-before-render", row);
                DateTime deadline = DateTime.UtcNow.AddSeconds(18);
                while (!move.IsFinished && DateTime.UtcNow < deadline)
                {
                    yield return 0;
                    if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                }
                row["movement"] = new JObject { ["started"] = move.IsStarted, ["finished"] = move.IsFinished,
                    ["distance"] = Vector3.Distance(origin, owner.Position), ["pose"] = Sprint17OriginalBodySample(owner, attachment.Body) };
                CheckHumanSalamander("native-movement", move.IsStarted && move.IsFinished &&
                    Vector3.Distance(origin, owner.Position) > 1 && attachment.Live &&
                    (bool)row["movement"]["pose"]["finite"], row["movement"],
                    "native movement progresses with finite original skin and unchanged human action references");
                WriteHumanSalamanderCheckpoint("movement-complete-before-attack", row);
                foreach (int step in ReviewHumanSalamanderAttack(fixture, owner, attachment, row)) yield return step;
                resources.AddRange(attachment.CaptureOwnedResources());
                CheckHumanSalamander("native-reference-isolation", attachment.NativeActionsUnchanged &&
                    originalSet.Actions.SequenceEqual(nativeActions) &&
                    staticMeshes.All(s => s.Filter != null && ReferenceEquals(s.Filter.sharedMesh, s.Mesh)),
                    new JObject { ["borrowed"] = borrowed.Length, ["nativeActions"] = nativeActions.Length },
                    "human actions and native equipment meshes remain exact, with no spear remount/transplant");
            }
            finally
            {
                SalamanderHumanVisualAttachment.PostSwapFaultForTest = null;
                try { WriteHumanSalamanderCheckpoint("before-owner-destruction", row); }
                finally
                {
                    if (owner != null && !owner.Destroyed)
                    {
                        var owned = owner.View == null ? null : owner.View.GetComponent<SalamanderHumanVisualAttachment>();
                        if (owned != null) resources.AddRange(owned.CaptureOwnedResources());
                        InterruptExpandedSummoningFixtureCommands(owner);
                        owner.CombatState.LeaveCombat(); owner.Destroy(); Game.Instance.EntityDestroyer.Tick();
                        Game.Instance.Player.UpdateIsInCombat();
                    }
                }
                WriteHumanSalamanderCheckpoint("owner-destruction-returned", row);
            }
            yield return 0; yield return 0;
            CheckHumanSalamander("native-destruction", resources.Distinct().Count() >= 29 && resources.All(v => v == null) &&
                borrowed.Length >= 25 && borrowed.All(v => v != null),
                new JObject { ["captured"] = resources.Distinct().Count(), ["remaining"] = resources.Count(v => v != null),
                    ["borrowedAlive"] = borrowed.Count(v => v != null) },
                "native unit destruction releases only exact project-owned body/tail/clip/set/material resources");
            WriteHumanSalamanderCheckpoint("native-destruction-settled", row);
        }

        private IEnumerable<int> ReviewHumanSalamanderAttack(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, SalamanderHumanVisualAttachment attachment, JObject row)
        {
            string placement;
            var target = CreateSprint17ContactTarget(fixture, ExpandedSummoningOpenPoint(owner.Position, 1.5f,
                new List<int>(), out placement));
            var faction = owner.Faction; var enemies = owner.AttackFactions.ToArray(); string group = owner.GroupId;
            var contacts = new JArray(); var poses = new JArray();
            row["contacts"] = contacts; row["attackPoses"] = poses; row["targetPlacement"] = placement;
            var observer = new Sprint16RuleObserver { Owner = owner, Target = target };
            var attack = new UnitAttack(target) { ForceFullAttack = true };
            var privateFaction = UnityEngine.Object.Instantiate(faction);
            _serpentineContactPrototypes.Add(privateFaction);
            privateFaction.name = "KMG_Runtime_Sprint17_HumanTailContact";
            privateFaction.Peaceful = privateFaction.AlwaysEnemy = privateFaction.Neutral = false;
            privateFaction.Dummy = null; privateFaction.AttackFactions = new[] { target.Faction };
            owner.Descriptor.SwitchFactions(privateFaction, true); owner.AttackFactions.Match(new[] { target.Faction });
            owner.GroupId = "KMG_Runtime_Sprint17_" + owner.UniqueId;
            owner.Descriptor.Stats.AdditionalAttackBonus.BaseValue = 100;
            owner.Descriptor.Stats.HitPoints.BaseValue = 100000;
            owner.Memory.Add(target); target.Memory.Add(owner);
            if (!Game.Instance.State.AwakeUnits.Contains(target)) Game.Instance.State.AwakeUnits.Add(target);
            var probe = owner.View.gameObject.AddComponent<Sprint17SnakeContactFrameProbe>();
            observer.ObserveWeaponContact = rule => {
                if (!ReferenceEquals(rule.Target, target) || contacts.Count >= 8) return;
                JObject contact;
                try { contact = HumanSalamanderContact(owner, target, attachment, rule, attack); }
                catch (Exception error) { contact = new JObject { ["measurementFailure"] = error.ToString(), ["finite"] = false }; }
                contacts.Add(contact);
                var handle = attack.Animation; int frame = Time.frameCount;
                probe.Capture(() => {
                    try
                    {
                        var rendered = HumanSalamanderContact(owner, target, attachment, rule, attack);
                        rendered["sameFrameAndHandle"] = frame == Time.frameCount && ReferenceEquals(handle, attack.Animation);
                        contact["endOfFrame"] = rendered;
                    }
                    catch (Exception error) { contact["endOfFrameFailure"] = error.ToString(); }
                });
            };
            EventBus.Subscribe(observer);
            try
            {
                WriteHumanSalamanderCheckpoint("before-attack-combat-join", row);
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                for (int frame = 0; frame < 90; frame++)
                { if (Game.Instance.IsPaused) Game.Instance.IsPaused = false; yield return 0; }
                if (!Sprint17ContactPairIsolated(fixture, owner, target)) throw new InvalidOperationException("Owned human/tail enemy pair not isolated.");
                attack.Init(owner); row["attackBefore"] = Sprint17NativeCommandState(owner, target, attack);
                WriteHumanSalamanderCheckpoint("before-native-attack-command", row);
                owner.Commands.Run(attack);
                DateTime deadline = DateTime.UtcNow.AddSeconds(45); int frames = 0;
                while (!attack.IsFinished && DateTime.UtcNow < deadline && frames++ < 1800)
                {
                    if (Game.Instance.IsPaused) Game.Instance.IsPaused = false;
                    yield return 0;
                    if (frames % 10 == 0 && poses.Count < 180)
                        poses.Add(new JObject { ["pose"] = Sprint17OriginalBodySample(owner, attachment.Body),
                            ["command"] = Sprint17NativeCommandState(owner, target, attack),
                            ["tailStarted"] = attachment.TailAction.StartedHandles,
                            ["tailActEvents"] = attachment.TailAction.ActEvents, ["tailFailure"] = attachment.TailAction.Failure,
                            ["tailClipTime"] = attachment.TailAction.LastClipTime });
                }
                yield return 0; // finish same-frame paired evidence, not arbitrary peak selection
                row["attackAfter"] = Sprint17NativeCommandState(owner, target, attack);
                JObject[] rows = contacts.OfType<JObject>().ToArray();
                JObject[] spear = rows.Where(c => (string)c["weapon"] == SalamanderTailAnimationPolicy.Spear).ToArray();
                JObject[] tail = rows.Where(c => (bool?)c["tail"] == true).ToArray();
                CheckHumanSalamander("native-full-attack", attack.IsStarted && attack.IsFinished && spear.Length == 2 &&
                    tail.Length == 1 && rows.All(c => (bool?)c["executing"] == true && (bool?)c["finite"] == true &&
                        (bool?)c["opportunity"] == false && (bool?)c["ownedPair"] == true) &&
                    Sprint17ContactPairIsolated(fixture, owner, target),
                    new JObject { ["spearRules"] = spear.Length, ["tailRules"] = tail.Length, ["contacts"] = contacts },
                    "one real full native UnitAttack resolves two spear iteratives and one secondary Tail; isolated owned pair");
                CheckHumanSalamander("native-spear-playback", spear.Length == 2 && spear.All(c => (bool?)c["nativeSpearClip"] == true),
                    new JArray(spear), "each spear rule has its native human hand-attack clip, not a null-animation fallback");
                CheckHumanSalamander("original-tail-playback", tail.Length == 1 && (bool?)tail[0]["ownedTailClip"] == true &&
                    (float?)tail[0]["tailMovementMeters"] > .3f && attachment.TailAction.ActEvents == 1 &&
                    attachment.TailAction.StartedHandles == 1 && string.IsNullOrEmpty(attachment.TailAction.Failure),
                    new JArray(tail), "exact owned clip actually moves original tail bones at its one authored event; no native null-clip fallback");
                bool contactPass = spear.Length == 2 && tail.Length == 1 && rows.All(c =>
                    (float?)c["gapMeters"] <= .25f) && spear.All(c =>
                    (float?)c["LHandGapMeters"] <= .08f && (float?)c["RHandGapMeters"] <= .08f);
                CheckHumanSalamander("attack-contact", contactPass, contacts,
                    "actual rule-frame spear tip and weighted striking tail within quarter metre; both native hands within8cm of shaft");
                CheckHumanSalamander("finite-attack-skin", poses.Count >= 3 && poses.OfType<JObject>().All(p =>
                    (bool)p["pose"]["finite"] && (bool)p["pose"]["poseFinite"]) && attachment.Live, poses.Count,
                    "original skin stays finite and native human actions remain unchanged throughout the real attack");
                WriteHumanSalamanderCheckpoint("native-attack-observations-complete", row);
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                InterruptExpandedSummoningFixtureCommands(owner);
                owner.CombatState.LeaveCombat(); target.CombatState.LeaveCombat();
                owner.Descriptor.SwitchFactions(faction, false); owner.AttackFactions.Match(enemies); owner.GroupId = group;
                if (probe != null) UnityEngine.Object.Destroy(probe);
                target.Destroy(); Game.Instance.EntityDestroyer.Tick(); Game.Instance.Player.UpdateIsInCombat();
            }
        }

        private void HumanSalamanderActionSetBoundary(UnitEntityData owner, JObject row)
        {
            var manager = owner.View.AnimationManager;
            var set = manager.AnimationSet;
            var root = BlueprintRoot.Instance.HumanAnimationSet;
            var actions = set == null ? new Kingmaker.Visual.Animation.Actions.AnimationActionBase[0] : set.Actions.ToArray();
            if (actions.Length > 64) throw new InvalidOperationException("Unbounded human action-set metadata.");
            var effective = manager.GetAction(UnitAnimationSpecialAttackType.Tail);
            var specials = actions.OfType<UnitAnimationActionSpecialAttack>().ToArray();
            var patches = new JArray();
            // Retain the core native observations before consulting patch
            // metadata; a provenance failure must not erase the guard facts.
            var boundary = new JObject {
                ["scope"] = "read-only exact live actor and three getter patch registries; no direct patch invocation or action execution",
                ["manager"] = manager.name, ["set"] = set == null ? null : set.name,
                ["setId"] = set == null ? (int?)null : set.GetInstanceID(),
                ["rootSet"] = root == null ? null : root.name, ["rootSetId"] = root == null ? (int?)null : root.GetInstanceID(),
                ["referenceEqualToRoot"] = ReferenceEquals(set, root), ["unityEqualToRoot"] = set == root,
                ["actionCount"] = actions.Length, ["transitionCount"] = set == null ? (int?)null : set.Transitions.Count(),
                ["effectiveTail"] = effective == null ? null : effective.name,
                ["effectiveTailClass"] = effective == null ? null : effective.GetType().FullName,
                ["effectiveTailDeclaredType"] = effective is UnitAnimationActionSpecialAttack
                    ? ((UnitAnimationActionSpecialAttack)effective).AttackType.ToString() : null,
                ["effectiveTailIsRawMember"] = effective != null && actions.Any(a => ReferenceEquals(a, effective)),
                ["rawSpecials"] = new JArray(specials.Select(a => new JObject {
                    ["name"] = a.name, ["type"] = a.AttackType.ToString(),
                    ["clips"] = new JArray((a.Clips ?? new AnimationClip[0]).Select(Sprint17ClipMetadata)) })),
                ["getterPatches"] = patches, ["provenanceComplete"] = false
            };
            row["nativeActionSetBoundary"] = boundary;
            var methods = new MethodBase[] {
                typeof(UnitAnimationManager).GetMethod("GetAction", new[] { typeof(UnitAnimationSpecialAttackType) }),
                typeof(AnimationManager).GetProperty("AnimationSet").GetGetMethod(),
                typeof(UnitAnimationActionSpecialAttack).GetProperty("AttackType").GetGetMethod()
            };
            // Same native registry boundary used by the qualified teleportation
            // observer. Retain membership for only these three exact getters.
            var registered = new HashSet<MethodBase>(_context.Harmony.GetPatchedMethods()
                .Where(method => methods.Contains(method)));
            foreach (MethodBase method in methods)
            {
                if (method == null) throw new InvalidOperationException("Exact action-boundary method missing.");
                bool present = registered.Contains(method);
                Patches info = SalamanderHumanBindingPolicy.ReadRegisteredPatchMetadata(present,
                    () => _context.Harmony.GetPatchInfo(method));
                Func<IEnumerable<Patch>, JArray> describe = list => {
                    Patch[] entries = list.ToArray();
                    if (entries.Length > 64) throw new InvalidOperationException("Unbounded action-boundary patches.");
                    return new JArray(entries.Select(p => new JObject {
                        ["method"] = p.patch == null ? null : p.patch.ToString(),
                        ["type"] = p.patch == null || p.patch.DeclaringType == null ? null : p.patch.DeclaringType.FullName,
                        ["assembly"] = p.patch == null || p.patch.DeclaringType == null ? null :
                            p.patch.DeclaringType.Assembly.GetName().Name }));
                };
                patches.Add(new JObject { ["target"] = method.DeclaringType.FullName + "." + method.Name,
                    ["registered"] = present, ["metadataQueried"] = present,
                    ["prefixes"] = info == null ? new JArray() : describe(info.Prefixes),
                    ["postfixes"] = info == null ? new JArray() : describe(info.Postfixes),
                    ["transpilers"] = info == null ? new JArray() : describe(info.Transpilers) });
            }
            boundary["provenanceComplete"] = true;
            boundary["unchangedAfterRead"] = ReferenceEquals(manager.AnimationSet, set) &&
                ReferenceEquals(BlueprintRoot.Instance.HumanAnimationSet, root) &&
                (set == null || set.Actions.SequenceEqual(actions));
        }

        private static JObject HumanSalamanderContact(UnitEntityData owner, UnitEntityData target,
            SalamanderHumanVisualAttachment attachment, RuleAttackWithWeapon rule, UnitAttack issued)
        {
            bool spear = rule.Weapon.Blueprint.AssetGuid == SalamanderTailAnimationPolicy.Spear;
            bool tail = rule.Weapon.Blueprint.name == "KMG_Summoning_Special_Salamander_Tail";
            var handle = issued.Animation;
            var row = new JObject { ["frame"] = Time.frameCount, ["weapon"] = rule.Weapon.Blueprint.AssetGuid,
                ["tail"] = tail, ["executing"] = issued.IsStarted && !issued.IsFinished,
                ["ownedPair"] = ReferenceEquals(rule.Initiator, owner) && ReferenceEquals(rule.Target, target),
                ["opportunity"] = rule.IsAttackOfOpportunity, ["finite"] = false,
                ["issuedAnimation"] = Sprint17IssuedAnimationObservation(issued), ["nativeSpearClip"] = false,
                ["ownedTailClip"] = false };
            var renderer = target.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(s => s.enabled && s.sharedMesh != null && s.sharedMesh.vertexCount >= 100)
                .OrderByDescending(s => s.bones.Length).First();
            Bounds targetBounds = renderer.bounds;
            Vector3[] points; float uncertainty = 0;
            if (spear)
            {
                var model = rule.Weapon.Blueprint.VisualParameters.Model;
                Mesh source = model.GetComponentsInChildren<MeshFilter>(true).Single().sharedMesh;
                var filter = owner.View.GetComponentsInChildren<MeshFilter>(true)
                    .Single(f => ReferenceEquals(f.sharedMesh, source));
                var b = source.bounds;
                Vector3 a = filter.transform.TransformPoint(b.center - Vector3.up * b.extents.y);
                Vector3 end = filter.transform.TransformPoint(b.center + Vector3.up * b.extents.y);
                Vector3 axis = end - a;
                if (axis.sqrMagnitude == 0 || b.extents.y <= b.extents.x || b.extents.y <= b.extents.z)
                    throw new InvalidOperationException("Native spear shaft bounds changed.");
                foreach (string side in new[] { "L", "R" })
                {
                    var hand = attachment.Body.bones.Single(bone => bone.name == side + "_Hand");
                    row[side + "HandGapMeters"] = Vector3.Distance(hand.position,
                        a + axis * Mathf.Clamp01(Vector3.Dot(hand.position - a, axis) / axis.sqrMagnitude));
                }
                uncertainty = new[] { -1, 1 }.SelectMany(x => new[] { -1, 1 }.Select(z =>
                    filter.transform.TransformVector(new Vector3(x * b.extents.x, 0, z * b.extents.z)).magnitude)).Max();
                points = new[] { end };
                row["nativeSpearClip"] = (bool)row["issuedAnimation"]["observedActedClip"] &&
                    handle != null && attachment.BorrowedResources().Any(value =>
                        handle.ActiveAnimation != null && ReferenceEquals(value, handle.ActiveAnimation.GetPlayableClip()));
                row["measurement"] = "native positive-Y spear bound end plus transverse uncertainty; no weapon mount write";
                row["uncertaintyMeters"] = uncertainty;
            }
            else if (tail)
            {
                BoneWeight[] weights = attachment.Body.sharedMesh.boneWeights;
                Vector3[] all = Sprint17OriginalWorldVertices(attachment.Body);
                // Striking half only; proximal waist touching a target is not
                // evidence that the tail slap made contact.
                string[] distalNames = SalamanderTailAnimationPolicy.TailNames.Skip(5).ToArray();
                var distalSlots = new HashSet<int>(Enumerable.Range(0, attachment.Body.bones.Length).Where(i =>
                    distalNames.Contains(attachment.Body.bones[i].name, StringComparer.Ordinal)));
                points = all.Where((point, i) => distalSlots.Contains(weights[i].boneIndex0) && weights[i].weight0 >= .25f ||
                    distalSlots.Contains(weights[i].boneIndex1) && weights[i].weight1 >= .25f).ToArray();
                int[] tailSlots = SalamanderTailAnimationPolicy.TailNames.Select(name =>
                    Array.FindIndex(attachment.Body.bones, bone => bone.name == name)).ToArray();
                Transform[] bones = tailSlots.Select(i => attachment.Body.bones[i]).ToArray();
                Matrix4x4[] bind = tailSlots.Select(i => attachment.Body.sharedMesh.bindposes[i]).ToArray();
                row["tailMovementMeters"] = bones.Select((bone, i) =>
                    Vector3.Distance(bone.localPosition, bind[i].inverse.MultiplyPoint3x4(Vector3.zero))).Max();
                row["ownedTailClip"] = handle != null && ReferenceEquals(handle.Action, attachment.TailAction) &&
                    handle.IsStarted && handle.IsActed && attachment.TailAction.PlayingOwnedClip &&
                    attachment.TailAction.LastClipTime >= SalamanderTailAnimationPolicy.ActTime;
                row["tailClipTime"] = attachment.TailAction.LastClipTime;
                row["tailActFrame"] = attachment.TailAction.LastActFrame;
                row["measurement"] = "current weighted world vertices on original distal half; independent from IsActed";
            }
            else throw new InvalidOperationException("Unprinted weapon in Salamander full attack.");
            bool finite = points.Length > 0 && points.All(p => SalamanderTailAnimationPolicy.Finite(p.x) &&
                SalamanderTailAnimationPolicy.Finite(p.y) && SalamanderTailAnimationPolicy.Finite(p.z));
            row["finite"] = finite; row["points"] = points.Length;
            if (finite) row["gapMeters"] = points.Min(p => Vector3.Distance(p, targetBounds.ClosestPoint(p))) + uncertainty;
            return row;
        }
    }
}
