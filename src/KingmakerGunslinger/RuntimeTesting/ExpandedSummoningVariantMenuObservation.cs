using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UI.ActionBar;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Read-only supervised observation of an actual rendered expanded summon
    /// variant menu. It never invokes Toggle, clicks a slot, or casts a spell.
    /// </summary>
    internal sealed class ExpandedSummoningVariantMenuObservation
    {
        private const int StableSamplesRequired = 3;
        private readonly int _largestPublishedVariantCount;
        private string _candidateKey = string.Empty;
        private int _candidateSamples;
        private ExpandedSummoningVariantMenuSnapshot _candidate;
        private string _candidateReason = "no active published menu observed";

        internal ExpandedSummoningVariantMenuObservation()
        {
            BlueprintAbility[] parents = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintAbility>()
                .Where(ExpandedSummoningPublisher.IsPublishedExpandedParent)
                .ToArray();
            if (parents.Length == 0)
                throw new InvalidOperationException(
                    "No published Expanded Summoning parent is available.");
            _largestPublishedVariantCount = parents.Max(value =>
                value.Variants == null ? 0 : value.Variants.Length);
            if (_largestPublishedVariantCount <= 0)
                throw new InvalidOperationException(
                    "Published Expanded Summoning parents expose no variants.");
        }

        internal bool Completed { get; private set; }
        internal bool Passed { get; private set; }
        internal string Reason { get; private set; }
        internal ExpandedSummoningVariantMenuSnapshot Snapshot
        { get; private set; }
        internal int LargestPublishedVariantCount
        { get { return _largestPublishedVariantCount; } }

        internal List<string> HookIdentifiers
        {
            get
            {
                return new List<string>
                {
                    "Kingmaker.UI.ActionBar.ActionBarSpellsGroup.Toggle(UnitEntityData,IEnumerable<AbilityData>,AbilityData)",
                    "Kingmaker.UI.ActionBar.ActionBarSpellsGroup.Hide(Boolean)",
                    "UnityEngine.Resources.FindObjectsOfTypeAll<ActionBarSpellsGroup>()"
                };
            }
        }

        internal void Poll()
        {
            if (Completed) return;
            ActionBarSpellsGroup[] groups = Resources.FindObjectsOfTypeAll<
                ActionBarSpellsGroup>();
            foreach (ActionBarSpellsGroup group in groups ??
                new ActionBarSpellsGroup[0])
            {
                if (group == null || group.gameObject == null ||
                    !group.gameObject.activeInHierarchy)
                    continue;
                ExpandedSummoningVariantMenuSnapshot snapshot;
                if (!ExpandedSummoningVariantMenuRuntime.TryGetSnapshot(group,
                        out snapshot) || snapshot == null)
                    continue;
                if (snapshot.ParentVariantCount <
                        _largestPublishedVariantCount ||
                    snapshot.SlotCount < _largestPublishedVariantCount)
                {
                    ObserveCandidate(snapshot,
                        "opened published menu is not the largest runtime list");
                    continue;
                }
                if (!NearTopLeft(snapshot.AnchorRect, snapshot.SafeRect))
                {
                    ObserveCandidate(snapshot,
                        "largest menu anchor is not near the top-left sidebar boundary");
                    continue;
                }

                bool navigation = ExpandedSummoningVariantMenuRuntime
                    .TryValidateNavigation(group, out snapshot);
                bool bounded = snapshot.SafeRect.Contains(snapshot.FinalRect,
                    SummonVariantMenuLayoutPolicy.Epsilon);
                bool needsVerticalViewport = snapshot.DesiredHeight >
                    snapshot.SafeRect.Height +
                    SummonVariantMenuLayoutPolicy.Epsilon;
                bool viewportExact = !needsVerticalViewport ||
                    snapshot.ScrollingRequired &&
                    snapshot.VerticalScrolling &&
                    snapshot.ScrollRectCount == 1 &&
                    snapshot.ViewportMarkerCount == 1;
                bool valid = bounded && viewportExact && navigation &&
                    snapshot.NavigationVerified &&
                    snapshot.FirstEntryReachable &&
                    snapshot.MiddleEntryReachable &&
                    snapshot.LastEntryReachable;
                string reason = valid ?
                    "largest top-left menu is bounded and fully navigable" :
                    "stable largest top-left menu failed bounds, viewport, or navigation checks";
                ObserveCandidate(snapshot, reason);
                if (_candidateSamples < StableSamplesRequired) continue;
                Completed = true;
                Passed = valid;
                Reason = reason;
                Snapshot = snapshot;
                return;
            }
        }

        internal string DescribeLatest()
        {
            return _candidate == null ? _candidateReason :
                _candidateReason + ";samples=" + _candidateSamples + ";" +
                _candidate;
        }

        private void ObserveCandidate(
            ExpandedSummoningVariantMenuSnapshot snapshot, string reason)
        {
            string key = snapshot.ParentGuid + ":" + snapshot.SlotCount +
                ":" + snapshot.AnchorRect.X + ":" + snapshot.AnchorRect.Y +
                ":" + reason;
            if (string.Equals(key, _candidateKey,
                    StringComparison.Ordinal))
                _candidateSamples++;
            else
            {
                _candidateKey = key;
                _candidateSamples = 1;
            }
            _candidate = snapshot;
            _candidateReason = reason;
        }

        private static bool NearTopLeft(SummonVariantMenuRect anchor,
            SummonVariantMenuRect safe)
        {
            float topBand = safe.YMax - safe.Height * 0.3f;
            float leftBand = safe.XMin + safe.Width * 0.3f;
            return anchor.YMax >= topBand && anchor.XMin <= leftBand;
        }
    }
}
