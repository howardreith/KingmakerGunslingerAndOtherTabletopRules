using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.View;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.RuntimeTesting;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    // Request-only experiment. Borrowing Bite changes native act timing from
    // the missing-Claw ScheduleAct(1) fallback. Never install this with PatchAll
    // or use it as a production visual repair.
    internal static class CrabSummonAnimationBindingPatch
    {
        private sealed class ReviewLease : IDisposable
        {
            internal RuntimeTestRequest Request;
            internal string RunId;
            internal UnitEntityData Actor, Caster;
            internal UnitAnimationManager Manager;
            internal HarmonyInstance Hooks;
            internal int Thread;

            internal bool Permits(UnitAnimationManager manager)
            {
                return ReferenceEquals(Review, this) && Thread == System.Threading.Thread.CurrentThread.ManagedThreadId &&
                    ExactRequest(Request) && Request.RunId == RunId && ReferenceEquals(Manager, manager) &&
                    Actor != null && !Actor.Destroyed && Caster != null && !Caster.Destroyed &&
                    Actor.View != null && ReferenceEquals(Actor.View.AnimationManager, manager);
            }

            public void Dispose()
            {
                // Close admission before attempting unpatch; cleanup failure
                // cannot leave a timing-changing callback active.
                if (ReferenceEquals(Review, this)) Review = null;
                if (Hooks != null) { Hooks.UnpatchAll(Hooks.Id); Hooks = null; }
            }
        }

        private static ReviewLease Review;

        private static bool ExactRequest(RuntimeTestRequest request)
        {
            return request != null && request.Enabled && request.ExitAfterCompletion &&
                request.Scenario == "disposable-expanded-summoning-cleanup2-native-review" &&
                !string.IsNullOrWhiteSpace(request.RunId) && request.Parameters != null && request.Parameters.Count == 1 &&
                (string)request.Parameters["reviewPhase"] == "crab-attack";
        }

        internal static IDisposable BeginReview(RuntimeTestRequest request, UnitEntityData actor, UnitEntityData caster)
        {
            if (!ExactRequest(request) || Review != null || actor == null || actor.Destroyed || actor.View == null ||
                actor.View.AnimationManager == null || caster == null || caster.Destroyed || ReferenceEquals(actor, caster))
                throw new InvalidOperationException("An exclusive exact Cleanup2 Crab attack request and live owned actor/caster are required.");
            var lease = new ReviewLease { Request = request, RunId = request.RunId, Actor = actor, Caster = caster,
                Manager = actor.View.AnimationManager, Thread = System.Threading.Thread.CurrentThread.ManagedThreadId,
                Hooks = HarmonyInstance.Create("kmg.cleanup2.crab-binding-experiment." + request.RunId) };
            Review = lease;
            try
            {
                lease.Hooks.Patch(typeof(UnitAnimationManager).GetMethod("GetAction", new[] { typeof(UnitAnimationSpecialAttackType) }), null,
                    new HarmonyMethod(typeof(CrabSummonAnimationBindingPatch).GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic)), null);
                return lease;
            }
            catch { lease.Dispose(); throw; }
        }

        private sealed class Receipt
        {
            internal UnitEntityData Actor;
            internal UnitAnimationManager Manager, PrefabManager;
            internal AnimationSet Set;
            internal List<AnimationActionBase> ActionList;
            internal AnimationActionBase[] Actions;
            internal UnitAnimationActionSpecialAttack Bite;
            internal UnitAnimationActionSpecialAttack.Entry[] Entries;
            internal UnitAnimationActionSpecialAttack.Entry[] EntryObjects;
            internal AnimationClip[][] VariantArrays;
            internal AnimationClip[][] Variants;
            internal AnimationClip[] Rends;
            internal bool[] Blends;
            internal float[][] Ranges;
            internal float[][] RangeArrays;
            internal float TransitionIn, TransitionOut;
            internal ExecutionMode Execution;
            internal bool EmptyMask;
            internal AvatarMask[] Masks;
            internal int Lookups;
            internal string Rejection;
        }
        private static readonly FieldInfo Attacks = typeof(UnitAnimationActionSpecialAttack)
            .GetField("m_Attacks", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly ConditionalWeakTable<UnitAnimationManager, Receipt> Receipts = new ConditionalWeakTable<UnitAnimationManager, Receipt>();

        private static void Postfix(UnitAnimationManager __instance, UnitAnimationSpecialAttackType __0, ref UnitAnimationAction __result)
        { __result = Resolve(__instance, __0, __result); }

        internal static UnitAnimationAction Resolve(UnitAnimationManager manager, UnitAnimationSpecialAttackType requested, UnitAnimationAction current)
        {
            if (current != null || requested != UnitAnimationSpecialAttackType.Claw || manager == null ||
                Review == null || !Review.Permits(manager) || !ExpandedSummoningVisualVariantPatch.IsModuleActive) return current;
            try
            {
                UnitEntityView view = manager.GetComponentInParent<UnitEntityView>();
                UnitEntityData actor = view == null ? null : view.EntityData;
                if (actor == null || !ReferenceEquals(actor, Review.Actor) || actor.Destroyed || !actor.IsInState || actor.Descriptor.State.IsDead || actor.Blueprint == null || actor.Blueprint.AssetGuid != CrabSummonAnimationBindingPolicy.UnitGuid ||
                    actor.Blueprint.name != CrabSummonAnimationBindingPolicy.UnitName || actor.Blueprint.Prefab == null ||
                    actor.Blueprint.Prefab.AssetId != CrabSummonAnimationBindingPolicy.PrefabGuid || !ReferenceEquals(view.AnimationManager, manager) ||
                    !ExpandedSummoningPteranodonViewPatch.HasLiveOriginalVisual(view, Sprint14BonePolicy.CrabKey)) return current;
                var markers = actor.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => value.Blueprint == BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff).ToArray();
                if (markers.Length != 1 || !(markers[0].TimeLeft.TotalSeconds > 0d)) return current;
                var source = markers[0].MaybeContext?.SourceAbility;
                var caster = markers[0].MaybeContext?.MaybeCaster;
                if (source == null || caster == null || !ReferenceEquals(caster, Review.Caster) || caster.Destroyed ||
                    !CrabSummonAnimationBindingPolicy.SourceGuids.Contains(source.AssetGuid)) return current;
                bool registered = ReferenceEquals(BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                    CrabSummonAnimationBindingPolicy.UnitGuid, "registered Crab animation owner"), actor.Blueprint);
                bool registeredSource = ReferenceEquals(BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    source.AssetGuid, "registered Crab summon source"), source);
                // The released routine is primary claw plus one additional
                // primary limb, not the equipment off-hand slot.
                var weapons = new[] { actor.Body.PrimaryHand.MaybeWeapon }
                    .Concat(actor.Body.AdditionalLimbs.Select(value => value.MaybeWeapon)).ToArray();
                bool exactWeapons = weapons.Length == 2 && weapons.Distinct().Count() == 2 &&
                    weapons.All(value => value != null && value.Blueprint.AssetGuid == CrabSummonAnimationBindingPolicy.ClawGuid);
                Receipt receipt;
                if (!Receipts.TryGetValue(manager, out receipt))
                {
                    if (!registered || !registeredSource || !exactWeapons) return current;
                    try { receipt = Capture(actor, manager); }
                    catch (Exception error) { receipt = new Receipt { Actor = actor, Manager = manager, Rejection = error.GetType().Name + ":" + error.Message }; }
                    Receipts.Add(manager, receipt);
                }
                var scope = new CrabAnimationBindingScope { RequestActive = Review.Permits(manager), ModuleActive = true, MissingResult = current == null, RequestedClaw = true,
                    RegisteredUnit = registered, RegisteredSource = registeredSource, ExactManagerView = ReferenceEquals(actor, receipt.Actor),
                    LiveOriginalBody = true, LivePositiveSummon = true, ExactClawWeapons = exactWeapons,
                    NativeReceiptUnchanged = Unchanged(receipt), UnitGuid = actor.Blueprint.AssetGuid, UnitName = actor.Blueprint.name,
                    PrefabGuid = actor.Blueprint.Prefab.AssetId, SourceGuid = source.AssetGuid };
                if (!CrabSummonAnimationBindingPolicy.Permits(scope)) return current;
                receipt.Lookups++;
                return receipt.Bite;
            }
            catch (Exception) { return current; } // Presentation fails closed; native behavior continues.
        }

        private static Receipt Capture(UnitEntityData actor, UnitAnimationManager manager)
        {
            BlueprintUnit spider = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                CrabSummonAnimationBindingPolicy.SpiderGuid, "unchanged native spider animation source");
            if (spider.Prefab == null || spider.Prefab.AssetId != CrabSummonAnimationBindingPolicy.PrefabGuid || Attacks == null)
                throw new NotSupportedException("Exact native spider prefab/action field unavailable.");
            UnitEntityView prefab = spider.Prefab.Load(false); // Fixed shared prefab, read only; never instantiate an actor.
            var donor = prefab == null ? null : prefab.GetComponentsInChildren<UnitAnimationManager>(true).SingleOrDefault();
            var set = manager.AnimationSet;
            if (donor == null || set == null || !ReferenceEquals(set, donor.AnimationSet) || set.name != CrabSummonAnimationBindingPolicy.NativeSet || set.Actions.Count != 8)
                throw new InvalidOperationException("Current Crab does not borrow the exact native spider set.");
            var bite = set.Actions.OfType<UnitAnimationActionSpecialAttack>().Single(value => value.AttackType == UnitAnimationSpecialAttackType.Bite);
            var entries = Attacks.GetValue(bite) as UnitAnimationActionSpecialAttack.Entry[];
            var clips = bite.Clips == null ? new AnimationClip[0] : bite.Clips.Where(value => value != null).Distinct().ToArray();
            if (bite.name != CrabSummonAnimationBindingPolicy.NativeAction || clips.Length != 1 ||
                clips[0].name != CrabSummonAnimationBindingPolicy.NativeClip || !(Mathf.Abs(clips[0].length - 7f / 6f) <= .002f) ||
                entries == null || entries.Length == 0 || entries.Length > 16 || entries.Any(entry => entry == null ||
                    entry.Variants == null || entry.Variants.Length == 0 || entry.Variants.Length > 16 ||
                    entry.Variants.Any(clip => !ReferenceEquals(clip, clips[0])) ||
                    (entry.Rend != null && !ReferenceEquals(entry.Rend, clips[0])) ||
                    (entry.HasRangeBlend && (entry.BlendRanges == null || entry.BlendRanges.Length != entry.Variants.Length)) ||
                    (entry.BlendRanges != null && (entry.BlendRanges.Length > 16 || entry.BlendRanges.Any(range => float.IsNaN(range) || float.IsInfinity(range))))))
                throw new InvalidOperationException("Unreviewed native spider Bite Entry/clip contract.");
            return new Receipt { Actor = actor, Manager = manager, PrefabManager = donor, Set = set, ActionList = set.Actions, Actions = set.Actions.ToArray(), Bite = bite,
                Entries = entries, EntryObjects = entries.ToArray(), VariantArrays = entries.Select(value => value.Variants).ToArray(),
                Variants = entries.Select(value => value.Variants.ToArray()).ToArray(), Rends = entries.Select(value => value.Rend).ToArray(),
                Blends = entries.Select(value => value.HasRangeBlend).ToArray(), Ranges = entries.Select(value => value.BlendRanges == null ? null : value.BlendRanges.ToArray()).ToArray(),
                RangeArrays = entries.Select(value => value.BlendRanges).ToArray(),
                TransitionIn = bite.TransitionIn, TransitionOut = bite.TransitionOut, Execution = bite.ExecutionMode,
                EmptyMask = bite.UseEmptyAvatarMask, Masks = bite.AvatarMasks == null ? null : bite.AvatarMasks.ToArray() };
        }

        private static bool Unchanged(Receipt value)
        {
            if (value.Rejection != null || value.Actor == null || value.Actor.Destroyed || value.Actor.View == null || value.Manager == null || value.PrefabManager == null || value.Set == null ||
                !ReferenceEquals(value.Actor.View.AnimationManager, value.Manager) || !ReferenceEquals(value.Manager.AnimationSet, value.Set) ||
                !ReferenceEquals(value.PrefabManager.AnimationSet, value.Set) || !ReferenceEquals(value.Set.Actions, value.ActionList) || !value.Set.Actions.SequenceEqual(value.Actions) ||
                value.Bite == null || value.Bite.AttackType != UnitAnimationSpecialAttackType.Bite || value.Bite.name != CrabSummonAnimationBindingPolicy.NativeAction ||
                value.Bite.TransitionIn != value.TransitionIn || value.Bite.TransitionOut != value.TransitionOut || value.Bite.ExecutionMode != value.Execution ||
                value.Bite.UseEmptyAvatarMask != value.EmptyMask || (value.Masks == null ? value.Bite.AvatarMasks != null : value.Bite.AvatarMasks == null || !value.Bite.AvatarMasks.SequenceEqual(value.Masks)) ||
                !ReferenceEquals(Attacks.GetValue(value.Bite), value.Entries)) return false;
            for (int i = 0; i < value.Entries.Length; i++)
            {
                var entry = value.Entries[i];
                if (entry == null || !ReferenceEquals(entry, value.EntryObjects[i]) || !ReferenceEquals(entry.Variants, value.VariantArrays[i]) ||
                    !ReferenceEquals(entry.BlendRanges, value.RangeArrays[i]) || entry.Variants == null || !entry.Variants.SequenceEqual(value.Variants[i]) || entry.Rend != value.Rends[i] || entry.HasRangeBlend != value.Blends[i] ||
                    (entry.BlendRanges == null ? value.Ranges[i] != null : value.Ranges[i] == null || !entry.BlendRanges.SequenceEqual(value.Ranges[i]))) return false;
                foreach (var clip in entry.Variants)
                    if (clip == null || clip.name != CrabSummonAnimationBindingPolicy.NativeClip || !(Mathf.Abs(clip.length - 7f / 6f) <= .002f)) return false;
            }
            return true;
        }

        internal static bool NativeReceiptUnchanged(UnitAnimationManager manager)
        { Receipt value; return manager != null && Receipts.TryGetValue(manager, out value) && Unchanged(value); }
        internal static int BoundLookups(UnitAnimationManager manager)
        { Receipt value; return manager != null && Receipts.TryGetValue(manager, out value) ? value.Lookups : 0; }
        internal static string Describe(UnitAnimationManager manager)
        { Receipt value; return manager == null || !Receipts.TryGetValue(manager, out value) ? "not-bound" : value.Rejection ?? (Unchanged(value) ? "exact-native-borrowed-bite;lookups=" + value.Lookups : "refused-changed-native-receipt"); }
    }
}
