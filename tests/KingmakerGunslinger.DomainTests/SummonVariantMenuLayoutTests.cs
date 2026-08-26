using System;
using System.IO;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SummonVariantMenuLayoutTests
    {
        internal static void TopLeftLargeListIsBounded()
        {
            SummonVariantMenuLayoutDecision decision = Decide(
                Rect(20f, 820f, 64f, 64f), 320f, 520f,
                Rect(0f, 0f, 1600f, 900f), 10f,
                SummonVariantMenuOpeningDirection.Up);
            Assertions.True(decision.SafeRect.Contains(decision.FinalRect, 0.01f) &&
                decision.OpeningDirection == SummonVariantMenuOpeningDirection.Up &&
                decision.TopClamped && !decision.BottomClamped &&
                Close(decision.SafeRect.YMax, decision.FinalRect.YMax) &&
                Close(84f, decision.FinalRect.X),
                "A slightly tall top-left list was centered or moved away from the sidebar instead of top-clamped.");
        }

        internal static void TopLeftOversizedListUsesFullSafeHeight()
        {
            SummonVariantMenuLayoutDecision decision = Decide(
                Rect(20f, 820f, 64f, 64f), 320f, 1400f,
                Rect(0f, 0f, 1600f, 900f), 10f,
                SummonVariantMenuOpeningDirection.Up);
            Assertions.True(decision.RequiresVerticalScrolling &&
                decision.TopClamped &&
                Close(decision.SafeRect.YMin, decision.FinalRect.YMin) &&
                Close(decision.SafeRect.YMax, decision.FinalRect.YMax) &&
                Close(decision.SafeRect.Height, decision.ViewportHeight) &&
                Close(0f, decision.VerticalContentOffset(1f)) &&
                Close(decision.VerticalScrollExtent,
                    decision.VerticalContentOffset(0f)),
                "An oversized top-left list did not occupy the full safe vertical range with every scroll offset reachable.");
        }

        internal static void BottomLeftClampsWithoutDirectionFlip()
        {
            SummonVariantMenuLayoutDecision decision = Decide(
                Rect(20f, 30f, 64f, 64f), 320f, 600f,
                Rect(0f, 0f, 1920f, 1080f), 20f,
                SummonVariantMenuOpeningDirection.Down);
            Assertions.True(decision.OpeningDirection ==
                    SummonVariantMenuOpeningDirection.Down &&
                decision.BottomClamped && !decision.TopClamped &&
                Close(decision.SafeRect.YMin, decision.FinalRect.YMin) &&
                decision.FinalRect.YMax < decision.SafeRect.YMax,
                "A bottom-left list flipped or jumped to the top instead of clamping its bottom edge.");
        }

        internal static void MiddlePlacementRemainsStable()
        {
            SummonVariantMenuLayoutDecision decision = Decide(
                Rect(20f, 500f, 64f, 64f), 320f, 300f,
                Rect(0f, 0f, 1920f, 1080f), 20f,
                SummonVariantMenuOpeningDirection.Down);
            Assertions.True(decision.OpeningDirection ==
                    SummonVariantMenuOpeningDirection.Down &&
                Close(200f, decision.FinalRect.Y) &&
                Close(84f, decision.FinalRect.X),
                "Ordinary middle placement moved away from the native anchor edge.");
        }

        internal static void ShortContentRetainsNativeSize()
        {
            SummonVariantMenuLayoutDecision decision = Decide(
                Rect(20f, 700f, 64f, 64f), 240f, 160f,
                Rect(0f, 0f, 1920f, 1080f), 20f);
            Assertions.True(!decision.RequiresScrolling &&
                Close(240f, decision.FinalRect.Width) &&
                Close(160f, decision.FinalRect.Height),
                "A short native-sized list was unnecessarily viewported.");
        }

        internal static void OversizedContentUsesScrollableViewport()
        {
            SummonVariantMenuLayoutDecision decision = Decide(
                Rect(20f, 900f, 64f, 64f), 320f, 2000f,
                Rect(0f, 0f, 1920f, 1080f), 20f);
            Assertions.True(decision.RequiresVerticalScrolling &&
                Close(1040f, decision.ViewportHeight) &&
                Close(960f, decision.VerticalScrollExtent) &&
                decision.SafeRect.Contains(decision.FinalRect, 0.01f),
                "Content taller than the safe viewport was not bounded and scrollable.");
        }

        internal static void ScreenshotEquivalentGeometryTopClamps()
        {
            SummonVariantMenuLayoutDecision decision = Decide(
                Rect(14f, 824f, 58f, 58f), 340f, 1020f,
                Rect(0f, 0f, 1600f, 900f), 9f,
                SummonVariantMenuOpeningDirection.Up);
            Assertions.True(Close(9f, decision.SafeRect.YMin) &&
                Close(891f, decision.SafeRect.YMax) &&
                Close(72f, decision.FinalRect.X) &&
                Close(9f, decision.FinalRect.YMin) &&
                Close(891f, decision.FinalRect.YMax) &&
                decision.TopClamped && decision.RequiresVerticalScrolling,
                "The 1600x900 first-sidebar-slot geometry did not top-clamp beside the source icon.");
        }

        internal static void RenderedBoundsTranslationIgnoresPivotAndAnchors()
        {
            // The measured rectangle already incorporates arbitrary Unity pivot,
            // anchor, and parent transforms. Placement therefore translates its
            // rendered bounds, not a guessed pivot position.
            SummonVariantMenuRect rendered = Rect(468f, 211f, 320f, 500f);
            SummonVariantMenuRect target = Rect(84f, 380f, 320f, 500f);
            SummonVariantMenuPlacementDecision decision =
                SummonVariantMenuPlacementPolicy.Decide(rendered, target);
            Assertions.True(Close(-384f, decision.DeltaX) &&
                Close(169f, decision.DeltaY) &&
                decision.ApplyTo(rendered).Equals(target),
                "Rendered-bounds placement still depended on a RectTransform pivot or anchor origin.");
        }

        internal static void NarrowResolutionRespectsHorizontalBounds()
        {
            SummonVariantMenuLayoutDecision decision = Decide(
                Rect(730f, 300f, 50f, 50f), 500f, 300f,
                Rect(0f, 0f, 800f, 600f), 12f);
            Assertions.True(decision.SafeRect.Contains(decision.FinalRect, 0.01f) &&
                decision.FinalRect.XMax <= 788.01f,
                "A narrow viewport produced an out-of-bounds menu.");
        }

        internal static void UltrawideUsesActualSafeRectangle()
        {
            SummonVariantMenuLayoutDecision decision = Decide(
                Rect(24f, 1200f, 72f, 72f), 420f, 900f,
                Rect(0f, 0f, 3440f, 1440f), 24f);
            Assertions.True(decision.SafeRect.Contains(decision.FinalRect, 0.01f) &&
                Close(96f, decision.FinalRect.X) &&
                decision.FinalRect.XMax < 600f,
                "Ultrawide placement relied on a fixed 16:9 coordinate.");
        }

        internal static void CanvasScaleIsCoordinateInvariant()
        {
            SummonVariantMenuLayoutDecision logical = Decide(
                Rect(16f, 600f, 48f, 48f), 280f, 520f,
                Rect(0f, 0f, 1280f, 720f), 16f);
            SummonVariantMenuLayoutDecision physical = Decide(
                Rect(24f, 900f, 72f, 72f), 420f, 780f,
                Rect(0f, 0f, 1920f, 1080f), 24f);
            Assertions.True(Close(logical.FinalRect.X * 1.5f,
                    physical.FinalRect.X) &&
                Close(logical.FinalRect.Y * 1.5f, physical.FinalRect.Y) &&
                Close(logical.FinalRect.Width * 1.5f,
                    physical.FinalRect.Width) &&
                Close(logical.FinalRect.Height * 1.5f,
                    physical.FinalRect.Height),
                "Canvas-space decisions changed under equivalent UI scaling.");
        }

        internal static void RepeatedDecisionIsIdempotent()
        {
            var request = new SummonVariantMenuLayoutRequest(
                Rect(20f, 900f, 64f, 64f), 320f, 2000f,
                Rect(0f, 0f, 1920f, 1080f), 20f,
                SummonVariantMenuOpeningDirection.Down);
            SummonVariantMenuLayoutDecision first =
                SummonVariantMenuLayoutPolicy.Decide(request);
            SummonVariantMenuLayoutDecision second =
                SummonVariantMenuLayoutPolicy.Decide(request);
            Assertions.True(first.FinalRect.Equals(second.FinalRect) &&
                first.RequiresScrolling == second.RequiresScrolling &&
                Close(first.VerticalScrollExtent, second.VerticalScrollExtent),
                "Repeated open/close geometry retained stale layout state.");
        }

        internal static void ThirdPartyHeightUsesRenderedContent()
        {
            SummonVariantMenuLayoutDecision projectOnly = Decide(
                Rect(20f, 900f, 64f, 64f), 320f, 1400f,
                Rect(0f, 0f, 1920f, 1080f), 20f);
            SummonVariantMenuLayoutDecision thirdPartyExpanded = Decide(
                Rect(20f, 900f, 64f, 64f), 320f, 1880f,
                Rect(0f, 0f, 1920f, 1080f), 20f);
            Assertions.True(thirdPartyExpanded.VerticalScrollExtent >
                    projectOnly.VerticalScrollExtent &&
                Close(480f, thirdPartyExpanded.VerticalScrollExtent -
                    projectOnly.VerticalScrollExtent),
                "The policy behaved as if it used a hard-coded KMG option count.");
        }

        internal static void ShortNativeListNeedsNoCorrection()
        {
            SummonVariantMenuLayoutDecision decision = Decide(
                Rect(20f, 400f, 64f, 64f), 256f, 192f,
                Rect(0f, 0f, 1920f, 1080f), 20f);
            Assertions.True(!decision.RequiresScrolling &&
                Close(84f, decision.FinalRect.X) &&
                Close(208f, decision.FinalRect.Y) &&
                Close(256f, decision.FinalRect.Width) &&
                Close(192f, decision.FinalRect.Height),
                "A short native list received an avoidable visual correction.");
        }

        internal static void FirstMiddleAndLastOffsetsAreReachable()
        {
            SummonVariantMenuLayoutDecision decision = Decide(
                Rect(20f, 900f, 64f, 64f), 320f, 2000f,
                Rect(0f, 0f, 1920f, 1080f), 20f);
            float first = decision.VerticalContentOffset(1f);
            float middle = decision.VerticalContentOffset(0.5f);
            float last = decision.VerticalContentOffset(0f);
            Assertions.True(Close(0f, first) &&
                Close(decision.VerticalScrollExtent / 2f, middle) &&
                Close(decision.VerticalScrollExtent, last) &&
                first <= middle && middle <= last,
                "The bounded viewport cannot expose the first, middle, and final entries.");
        }

        internal static void ExactNativeViewAdapterIsNarrowAndReusable()
        {
            string patch = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningVariantMenuPatch.cs"));
            foreach (string token in new[] {
                "typeof(ActionBarGroupSlot), \"OnToggleGroupClick\"",
                "CaptureSourceSlot(",
                "ConsumeSourceSlot(",
                "typeof(ActionBarSpellsGroup), \"Toggle\"",
                "typeof(IEnumerable<AbilityData>)",
                "typeof(AbilityData)",
                "IsPublishedExpandedParent",
                "PrepareForNativeFill",
                "ExpandedSummoningVariantMenuRuntime.Apply",
                "typeof(ActionBarSpellsGroup), \"Hide\"",
                "RestoreNative" })
                Assertions.True(patch.Contains(token),
                    "Variant-menu patch lacks exact native guard: " + token);

            string runtime = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningVariantMenuRuntime.cs"));
            foreach (string token in new[] {
                "Canvas.ForceUpdateCanvases()",
                "LayoutRebuilder.ForceRebuildLayoutImmediate(root)",
                "Screen.safeArea",
                "canvas.pixelRect",
                "RectTransformUtility.ScreenPointToLocalPointInRectangle",
                "MeasureSlots(liveSlots",
                "MeasureRect(sourceSlotTransform",
                "TryMeasureFallbackAnchor(group",
                "InferNativeOpeningDirection(nativeRect, anchor)",
                "ConditionalWeakTable<ActionBarSpellsGroup",
                "new LayoutState(value)",
                "new GameObject(\n                    \"KMG Expanded Summoning Variant Viewport\"",
                "AddComponent<ScrollRect>()",
                "AddComponent<RectMask2D>()",
                "slot.transform.SetParent(_content, false)",
                "slot.transform.SetSiblingIndex(index)",
                "_scroll.verticalNormalizedPosition = 1f",
                "TopAligned(source.childAlignment)",
                "GridLayoutGroup.Corner.UpperLeft",
                "SummonVariantMenuPlacementPolicy.Decide(rendered, finalRect)",
                "snapshot.RenderedPopupRect",
                "variant-menu.layout-applied",
                "state.RestoreNative()" })
                Assertions.True(runtime.Contains(token),
                    "Variant-menu adapter lacks rendered-layout contract: " + token);
            Assertions.False(runtime.Contains("1920") ||
                runtime.Contains("1080") || runtime.Contains("Screen.height -"),
                "Variant-menu adapter must not assume one physical resolution.");
            Assertions.False(runtime.Contains(
                    "SummonVariantMenuRect anchor = MeasureAnchor(group"),
                "The adapter rediscovered the shared popup parent instead of using the exact clicked source slot.");
        }

        internal static void SourceSlotCaptureDoesNotRetainStaleParents()
        {
            string patch = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningVariantMenuPatch.cs"));
            string runtime = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningVariantMenuRuntime.cs"));
            Assertions.True(patch.Contains(
                    "ActionBarGroupSlot __state") &&
                patch.Contains("sourceSpell, __state") &&
                runtime.Contains("SourceSlots.Remove(group);") &&
                runtime.Contains("return capture.SourceSlot;") &&
                runtime.Contains("sourceSlot.GetInstanceID()") &&
                runtime.Contains("SourceSlotHierarchyPath"),
                "Opening different expanded parents could retain a prior source-slot anchor.");
        }

        internal static void RepeatedOpenUsesOneReusableViewport()
        {
            string runtime = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningVariantMenuRuntime.cs"));
            Assertions.True(runtime.Contains("if (_scroll != null) return;") &&
                runtime.Contains("AddComponent<ScrollRect>()") &&
                runtime.Contains("AddComponent<RectMask2D>()") &&
                runtime.Contains("ExpandedSummoningVariantMenuViewportMarker") &&
                runtime.Contains("_viewportObject.SetActive(false)") &&
                runtime.Contains("state.RestoreNative();"),
                "Repeated popup opens can accumulate a viewport, mask, or scroll component.");
        }

        internal static void GuardedRenderedMenuObservationIsReadOnly()
        {
            string observer = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting",
                "ExpandedSummoningVariantMenuObservation.cs"));
            foreach (string token in new[] {
                "Resources.FindObjectsOfTypeAll<",
                "ActionBarSpellsGroup>()",
                "TryGetSnapshot(group",
                "TryValidateNavigation(group",
                "snapshot.SafeRect.Contains(\n                    snapshot.RenderedPopupRect",
                "snapshot.ScrollRectCount == 1",
                "snapshot.ViewportMarkerCount == 1",
                "snapshot.FirstEntryReachable",
                "snapshot.LastEntryReachable",
                "NearTopLeft(snapshot.SourceSlotRect, snapshot.SafeRect)",
                "snapshot.ExactSourceSlotCaptured",
                "snapshot.ViewportRect.YMin",
                "snapshot.FirstSlotRect" })
                Assertions.True(observer.Contains(token),
                    "Rendered menu observer lacks exact read-only check: " +
                    token);
            Assertions.False(observer.Contains("group.Toggle(") ||
                observer.Contains(".OnClick.Invoke(") ||
                observer.Contains("new RuleCastSpell(") ||
                observer.Contains("new UnitUseAbility("),
                "Rendered menu observer must not open, click, or cast anything.");

            string runner = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            string automation = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "scripts",
                "RuntimeAutomation.Common.ps1"));
            string launcher = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "scripts",
                "Invoke-KingmakerRuntimeTest.ps1"));
            Assertions.True(runner.Contains(
                    "RunExpandedSummoningVariantMenuObservation") &&
                runner.Contains("new ManualSaveLoadObservation(") &&
                runner.Contains(
                    "runtime-expanded-summoning-menu-ready.json") &&
                automation.Contains(
                    "'observe-expanded-summoning-variant-menu'") &&
                automation.Contains("RequiresManualInteraction = $true") &&
                launcher.Contains(
                    "DO NOT CLICK AN OPTION OR CAST THE SPELL") &&
                launcher.Contains(
                    "No keyboard or mouse input will be sent by this script."),
                "The rendered-menu observation is not guarded as supervised read-only work.");
        }

        private static SummonVariantMenuLayoutDecision Decide(
            SummonVariantMenuRect anchor, float width, float height,
            SummonVariantMenuRect safe, float margin)
        {
            return Decide(anchor, width, height, safe, margin,
                SummonVariantMenuOpeningDirection.Down);
        }

        private static SummonVariantMenuLayoutDecision Decide(
            SummonVariantMenuRect anchor, float width, float height,
            SummonVariantMenuRect safe, float margin,
            SummonVariantMenuOpeningDirection direction)
        {
            return SummonVariantMenuLayoutPolicy.Decide(
                new SummonVariantMenuLayoutRequest(anchor, width, height,
                    safe, margin, direction));
        }

        private static SummonVariantMenuRect Rect(float x, float y,
            float width, float height)
        {
            return new SummonVariantMenuRect(x, y, width, height);
        }

        private static bool Close(float expected, float actual)
        {
            return Math.Abs(expected - actual) <= 0.01f;
        }

    }
}
