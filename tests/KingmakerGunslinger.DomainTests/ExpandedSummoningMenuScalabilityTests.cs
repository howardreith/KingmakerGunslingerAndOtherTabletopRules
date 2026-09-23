using System;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 1 scalability gate for the Expanded Summoning charter.
    ///
    /// The charter's 1,232 figure is an aggregate over eighteen parent spells,
    /// never a single list. The real question is whether the shipped bounded
    /// presentation still behaves when the worst single parent spell grows from
    /// today's 69 visible choices to the projected 124.
    ///
    /// Acceptance rubric, fixed before measuring, relative to the baseline:
    ///   R1 the menu never escapes the canvas-safe rectangle;
    ///   R2 when content exceeds the viewport, scrolling is offered;
    ///   R3 every option stays reachable - the scroll offset spans exactly the
    ///      overflow, with no unreachable head or tail;
    ///   R4 the viewport never collapses below the baseline viewport, so growth
    ///      costs scroll extent and never usable height;
    ///   R5 behaviour at projected scale is the same shape as at baseline scale,
    ///      so nothing new is required of the player beyond scrolling further.
    /// A rubric failure is a real regression and must not be waived.
    /// </summary>
    internal static class ExpandedSummoningMenuScalabilityTests
    {
        // Worst observed parent today: Summon Monster VIII and IX, 69 choices.
        private const int BaselineWorstParent = 69;

        // Projected worst parent: 120 generated Summon Monster IX placements
        // plus the four native wrappers already preserved at that tier.
        private const int ProjectedWorstParent = 124;

        // Stretch probe only. Adding the five variant elemental families to the
        // ninth-level parent is a capacity question, not authorisation to build
        // them; Sprints 42-44 own that work.
        private const int StretchProbeWorstParent = 154;

        private const float RowHeight = 34f;
        private const float MenuWidth = 320f;

        internal static void ProjectedScaleMatchesTheMeasuredBaseline()
        {
            Assertions.Equal(69, BaselineWorstParent,
                "The baseline worst parent must match the Sprint 0 census.");
            Assertions.Equal(124, ProjectedWorstParent,
                "The projected worst parent must match the ideal manifest.");

            // Tie the projection to the manifest rather than a typed constant.
            int projectedGenerated = ExpandedSummoningIdealRosterCatalog
                .Placements(SummonFamily.Monster, 9).Count;
            int nativeWrappers = SummonNativeExpansionCatalog
                .For(SummonFamily.Monster, 9).Count;
            Assertions.Equal(ProjectedWorstParent,
                projectedGenerated + nativeWrappers,
                "Projected Summon Monster IX choices drifted from the manifest.");

            // Growth is under 2x on the only axis that matters to the player.
            Assertions.True(ProjectedWorstParent < BaselineWorstParent * 2,
                "The worst single menu must not double against the baseline.");
        }

        internal static void BaselineAndProjectedScalesSatisfyTheRubric()
        {
            foreach (Viewport viewport in Viewports())
            {
                Outcome baseline = Evaluate(viewport, BaselineWorstParent);
                Outcome projected = Evaluate(viewport, ProjectedWorstParent);

                // R1 / R2 / R3 hold independently at both scales.
                AssertRubric(baseline, viewport, "baseline");
                AssertRubric(projected, viewport, "projected");

                // R4: growth must cost scroll extent, never viewport height.
                Assertions.True(projected.ViewportHeight >= baseline.ViewportHeight - 0.01f,
                    "Projected scale shrank the usable viewport on " + viewport.Name + ".");

                // R5: same shape. Once the baseline already scrolls, the larger
                // list may only scroll further; it may not change behaviour.
                if (baseline.RequiresScrolling)
                {
                    Assertions.True(projected.RequiresScrolling,
                        "Projected scale stopped scrolling on " + viewport.Name + ".");
                    Assertions.True(projected.ScrollExtent > baseline.ScrollExtent,
                        "A larger list must scroll further on " + viewport.Name + ".");
                }
            }
        }

        /// <summary>
        /// Capacity probe for the full charter surface. It is evidence about
        /// headroom only and authorises no later-sprint content.
        /// </summary>
        internal static void StretchProbeStaysBounded()
        {
            foreach (Viewport viewport in Viewports())
            {
                AssertRubric(Evaluate(viewport, StretchProbeWorstParent),
                    viewport, "stretch probe");
            }
        }

        /// <summary>
        /// The presentation must stay option-count agnostic. A list one row
        /// longer may never flip a clamped menu outside the safe rectangle or
        /// strand a row, at any count from empty to well past the projection.
        /// </summary>
        internal static void EveryOptionCountRemainsBounded()
        {
            Viewport viewport = Viewports()[0];
            for (int options = 1; options <= 200; options++)
            {
                Outcome outcome = Evaluate(viewport, options);
                AssertRubric(outcome, viewport, "option count " + options);
            }
        }

        private static void AssertRubric(Outcome outcome, Viewport viewport,
            string label)
        {
            // R1: never escapes the canvas-safe rectangle.
            Assertions.True(outcome.WithinSafeRect,
                "R1 failed: the menu escaped the safe rectangle at " + label +
                " on " + viewport.Name + ".");

            // R2: overflow always offers scrolling.
            Assertions.Equal(outcome.Overflows, outcome.RequiresScrolling,
                "R2 failed: overflow and scrolling disagree at " + label +
                " on " + viewport.Name + ".");

            // R3: the reachable range is exactly the overflow - no stranded rows.
            Assertions.True(Math.Abs(outcome.OffsetAtTop) < 0.01f,
                "R3 failed: the list head is unreachable at " + label +
                " on " + viewport.Name + ".");
            Assertions.True(
                Math.Abs(outcome.OffsetAtBottom - outcome.ScrollExtent) < 0.01f,
                "R3 failed: the list tail is unreachable at " + label +
                " on " + viewport.Name + ".");
            Assertions.True(outcome.ScrollExtent >= 0f,
                "R3 failed: negative scroll extent at " + label + ".");
        }

        private sealed class Viewport
        {
            internal Viewport(string name, float width, float height, float margin,
                SummonVariantMenuRect anchor,
                SummonVariantMenuOpeningDirection direction)
            {
                Name = name; Width = width; Height = height; Margin = margin;
                Anchor = anchor; Direction = direction;
            }

            internal string Name { get; private set; }
            internal float Width { get; private set; }
            internal float Height { get; private set; }
            internal float Margin { get; private set; }
            internal SummonVariantMenuRect Anchor { get; private set; }
            internal SummonVariantMenuOpeningDirection Direction { get; private set; }
        }

        private sealed class Outcome
        {
            internal bool WithinSafeRect;
            internal bool Overflows;
            internal bool RequiresScrolling;
            internal float ScrollExtent;
            internal float ViewportHeight;
            internal float OffsetAtTop;
            internal float OffsetAtBottom;
        }

        private static Viewport[] Viewports()
        {
            // The resolutions the shipped menu-layout suite already covers, at
            // the action-bar slot positions the popup actually opens from.
            return new[] {
                new Viewport("1920x1080 bottom-left slot", 1920f, 1080f, 20f,
                    new SummonVariantMenuRect(20f, 30f, 64f, 64f),
                    SummonVariantMenuOpeningDirection.Up),
                new Viewport("1600x900 first sidebar slot", 1600f, 900f, 9f,
                    new SummonVariantMenuRect(20f, 820f, 64f, 64f),
                    SummonVariantMenuOpeningDirection.Up),
                new Viewport("1280x720 bottom-left slot", 1280f, 720f, 10f,
                    new SummonVariantMenuRect(18f, 26f, 48f, 48f),
                    SummonVariantMenuOpeningDirection.Up),
                new Viewport("3440x1440 ultrawide", 3440f, 1440f, 24f,
                    new SummonVariantMenuRect(40f, 40f, 64f, 64f),
                    SummonVariantMenuOpeningDirection.Up),
            };
        }

        private static Outcome Evaluate(Viewport viewport, int optionCount)
        {
            float desiredHeight = optionCount * RowHeight;
            SummonVariantMenuLayoutDecision decision =
                SummonVariantMenuLayoutPolicy.Decide(
                    new SummonVariantMenuLayoutRequest(viewport.Anchor,
                        MenuWidth, desiredHeight,
                        new SummonVariantMenuRect(0f, 0f, viewport.Width,
                            viewport.Height),
                        viewport.Margin, viewport.Direction));

            return new Outcome
            {
                WithinSafeRect = decision.SafeRect.Contains(decision.FinalRect,
                    SummonVariantMenuLayoutPolicy.Epsilon),
                Overflows = desiredHeight > decision.ViewportHeight +
                    SummonVariantMenuLayoutPolicy.Epsilon,
                RequiresScrolling = decision.RequiresVerticalScrolling,
                ScrollExtent = decision.VerticalScrollExtent,
                ViewportHeight = decision.ViewportHeight,
                OffsetAtTop = decision.VerticalContentOffset(1f),
                OffsetAtBottom = decision.VerticalContentOffset(0f),
            };
        }
    }
}
