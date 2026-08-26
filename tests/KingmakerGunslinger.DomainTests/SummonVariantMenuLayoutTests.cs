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
                Rect(20f, 900f, 64f, 64f), 320f, 800f,
                Rect(0f, 0f, 1920f, 1080f), 20f);
            Assertions.True(decision.SafeRect.Contains(decision.FinalRect, 0.01f) &&
                decision.OpeningDirection == SummonVariantMenuOpeningDirection.Down,
                "A large top-left variant list escaped the safe viewport.");
        }

        internal static void BottomLeftChoosesUp()
        {
            SummonVariantMenuLayoutDecision decision = Decide(
                Rect(20f, 30f, 64f, 64f), 320f, 600f,
                Rect(0f, 0f, 1920f, 1080f), 20f);
            Assertions.True(decision.OpeningDirection ==
                    SummonVariantMenuOpeningDirection.Up &&
                decision.FinalRect.YMin >= decision.AnchorlessSafeYMin(),
                "A bottom-left variant list did not use the available upper space.");
        }

        internal static void MiddlePlacementRemainsStable()
        {
            SummonVariantMenuLayoutDecision decision = Decide(
                Rect(20f, 500f, 64f, 64f), 320f, 300f,
                Rect(0f, 0f, 1920f, 1080f), 20f);
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
                "ConditionalWeakTable<ActionBarSpellsGroup",
                "new LayoutState(value)",
                "new GameObject(\n                    \"KMG Expanded Summoning Variant Viewport\"",
                "AddComponent<ScrollRect>()",
                "AddComponent<RectMask2D>()",
                "slot.transform.SetParent(_content, false)",
                "slot.transform.SetSiblingIndex(index)",
                "_scroll.verticalNormalizedPosition = 1f",
                "state.RestoreNative()" })
                Assertions.True(runtime.Contains(token),
                    "Variant-menu adapter lacks rendered-layout contract: " + token);
            Assertions.False(runtime.Contains("1920") ||
                runtime.Contains("1080") || runtime.Contains("Screen.height -"),
                "Variant-menu adapter must not assume one physical resolution.");
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
                "snapshot.SafeRect.Contains(snapshot.FinalRect",
                "snapshot.ScrollRectCount == 1",
                "snapshot.ViewportMarkerCount == 1",
                "snapshot.FirstEntryReachable",
                "snapshot.LastEntryReachable",
                "NearTopLeft(snapshot.AnchorRect, snapshot.SafeRect)" })
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
            return SummonVariantMenuLayoutPolicy.Decide(
                new SummonVariantMenuLayoutRequest(anchor, width, height,
                    safe, margin, SummonVariantMenuOpeningDirection.Down));
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

        private static float AnchorlessSafeYMin(
            this SummonVariantMenuLayoutDecision decision)
        {
            return decision.SafeRect.YMin;
        }
    }
}
