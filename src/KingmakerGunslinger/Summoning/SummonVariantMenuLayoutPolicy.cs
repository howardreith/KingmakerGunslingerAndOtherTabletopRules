using System;

namespace KingmakerGunslinger.Summoning
{
    internal enum SummonVariantMenuOpeningDirection
    {
        Down = 0,
        Up = 1
    }

    internal struct SummonVariantMenuRect : IEquatable<SummonVariantMenuRect>
    {
        internal SummonVariantMenuRect(float x, float y, float width, float height)
        {
            if (!Finite(x) || !Finite(y) || !Finite(width) || !Finite(height) ||
                width < 0f || height < 0f)
            {
                throw new ArgumentOutOfRangeException("width",
                    "Menu rectangles require finite coordinates and non-negative dimensions.");
            }

            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        internal float X { get; private set; }
        internal float Y { get; private set; }
        internal float Width { get; private set; }
        internal float Height { get; private set; }
        internal float XMin { get { return X; } }
        internal float XMax { get { return X + Width; } }
        internal float YMin { get { return Y; } }
        internal float YMax { get { return Y + Height; } }

        internal bool Contains(SummonVariantMenuRect other, float tolerance)
        {
            return other.XMin >= XMin - tolerance &&
                other.XMax <= XMax + tolerance &&
                other.YMin >= YMin - tolerance &&
                other.YMax <= YMax + tolerance;
        }

        public bool Equals(SummonVariantMenuRect other)
        {
            return X == other.X && Y == other.Y &&
                Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is SummonVariantMenuRect &&
                Equals((SummonVariantMenuRect)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = X.GetHashCode();
                hash = (hash * 397) ^ Y.GetHashCode();
                hash = (hash * 397) ^ Width.GetHashCode();
                return (hash * 397) ^ Height.GetHashCode();
            }
        }

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    internal sealed class SummonVariantMenuLayoutRequest
    {
        internal SummonVariantMenuLayoutRequest(
            SummonVariantMenuRect anchor,
            float desiredWidth,
            float desiredHeight,
            SummonVariantMenuRect canvasSafeRect,
            float minimumMargin,
            SummonVariantMenuOpeningDirection preferredDirection)
        {
            if (!Finite(desiredWidth) || !Finite(desiredHeight) ||
                desiredWidth <= 0f || desiredHeight <= 0f)
            {
                throw new ArgumentOutOfRangeException("desiredWidth",
                    "Desired menu dimensions must be finite and positive.");
            }

            if (!Finite(minimumMargin) || minimumMargin < 0f ||
                (minimumMargin * 2f) >= canvasSafeRect.Width ||
                (minimumMargin * 2f) >= canvasSafeRect.Height)
            {
                throw new ArgumentOutOfRangeException("minimumMargin",
                    "The menu margin must leave a positive canvas-safe rectangle.");
            }

            if (!Enum.IsDefined(typeof(SummonVariantMenuOpeningDirection),
                preferredDirection))
            {
                throw new ArgumentOutOfRangeException("preferredDirection");
            }

            Anchor = anchor;
            DesiredWidth = desiredWidth;
            DesiredHeight = desiredHeight;
            CanvasSafeRect = canvasSafeRect;
            MinimumMargin = minimumMargin;
            PreferredDirection = preferredDirection;
        }

        internal SummonVariantMenuRect Anchor { get; private set; }
        internal float DesiredWidth { get; private set; }
        internal float DesiredHeight { get; private set; }
        internal SummonVariantMenuRect CanvasSafeRect { get; private set; }
        internal float MinimumMargin { get; private set; }
        internal SummonVariantMenuOpeningDirection PreferredDirection
        { get; private set; }

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    internal sealed class SummonVariantMenuLayoutDecision
    {
        internal SummonVariantMenuLayoutDecision(
            SummonVariantMenuRect safeRect,
            SummonVariantMenuRect finalRect,
            SummonVariantMenuOpeningDirection openingDirection,
            float desiredWidth,
            float desiredHeight,
            bool topClamped,
            bool bottomClamped)
        {
            SafeRect = safeRect;
            FinalRect = finalRect;
            OpeningDirection = openingDirection;
            DesiredWidth = desiredWidth;
            DesiredHeight = desiredHeight;
            TopClamped = topClamped;
            BottomClamped = bottomClamped;
        }

        internal SummonVariantMenuRect SafeRect { get; private set; }
        internal SummonVariantMenuRect FinalRect { get; private set; }
        internal SummonVariantMenuOpeningDirection OpeningDirection
        { get; private set; }
        internal float DesiredWidth { get; private set; }
        internal float DesiredHeight { get; private set; }
        internal bool TopClamped { get; private set; }
        internal bool BottomClamped { get; private set; }
        internal float ViewportHeight { get { return FinalRect.Height; } }
        internal float VerticalScrollExtent
        { get { return Math.Max(0f, DesiredHeight - FinalRect.Height); } }
        internal float HorizontalScrollExtent
        { get { return Math.Max(0f, DesiredWidth - FinalRect.Width); } }
        internal bool RequiresVerticalScrolling
        { get { return VerticalScrollExtent > SummonVariantMenuLayoutPolicy.Epsilon; } }
        internal bool RequiresHorizontalScrolling
        { get { return HorizontalScrollExtent > SummonVariantMenuLayoutPolicy.Epsilon; } }
        internal bool RequiresScrolling
        { get { return RequiresVerticalScrolling || RequiresHorizontalScrolling; } }

        internal float VerticalContentOffset(float normalizedPosition)
        {
            if (float.IsNaN(normalizedPosition) ||
                float.IsInfinity(normalizedPosition) ||
                normalizedPosition < 0f || normalizedPosition > 1f)
            {
                throw new ArgumentOutOfRangeException("normalizedPosition");
            }

            return (1f - normalizedPosition) * VerticalScrollExtent;
        }
    }

    internal sealed class SummonVariantMenuPlacementDecision
    {
        internal SummonVariantMenuPlacementDecision(float deltaX, float deltaY)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
        }

        internal float DeltaX { get; private set; }
        internal float DeltaY { get; private set; }

        internal SummonVariantMenuRect ApplyTo(
            SummonVariantMenuRect renderedRect)
        {
            return new SummonVariantMenuRect(renderedRect.X + DeltaX,
                renderedRect.Y + DeltaY, renderedRect.Width,
                renderedRect.Height);
        }
    }

    /// <summary>
    /// Converts a target canvas rectangle into a translation from the rectangle that
    /// Unity actually rendered. Measuring first makes the placement independent of
    /// the popup RectTransform's pivot, anchors, and parent-space origin.
    /// </summary>
    internal static class SummonVariantMenuPlacementPolicy
    {
        private const float RenderedTolerance = 0.5f;

        internal static SummonVariantMenuPlacementDecision Decide(
            SummonVariantMenuRect renderedRect,
            SummonVariantMenuLayoutDecision layout)
        {
            if (layout == null) throw new ArgumentNullException("layout");
            if (renderedRect.Width > layout.SafeRect.Width +
                    RenderedTolerance ||
                renderedRect.Height > layout.SafeRect.Height +
                    RenderedTolerance)
            {
                throw new InvalidOperationException(
                    "The rendered popup remains larger than the canvas-safe rectangle after sizing.");
            }

            float x = Clamp(layout.FinalRect.X, layout.SafeRect.XMin,
                layout.SafeRect.XMax - renderedRect.Width);
            float y;
            if (layout.TopClamped)
                y = layout.SafeRect.YMax - renderedRect.Height;
            else if (layout.BottomClamped)
                y = layout.SafeRect.YMin;
            else
                y = Clamp(layout.FinalRect.Y, layout.SafeRect.YMin,
                    layout.SafeRect.YMax - renderedRect.Height);

            return Decide(renderedRect, new SummonVariantMenuRect(x, y,
                renderedRect.Width, renderedRect.Height));
        }

        internal static SummonVariantMenuPlacementDecision Decide(
            SummonVariantMenuRect renderedRect,
            SummonVariantMenuRect targetRect)
        {
            if (Math.Abs(renderedRect.Width - targetRect.Width) >
                    SummonVariantMenuLayoutPolicy.Epsilon ||
                Math.Abs(renderedRect.Height - targetRect.Height) >
                    SummonVariantMenuLayoutPolicy.Epsilon)
            {
                throw new InvalidOperationException(
                    "Rendered popup size must match the target before translation.");
            }

            return new SummonVariantMenuPlacementDecision(
                targetRect.X - renderedRect.X,
                targetRect.Y - renderedRect.Y);
        }

        private static float Clamp(float value, float minimum, float maximum)
        {
            if (maximum < minimum) return minimum;
            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }

    /// <summary>
    /// Pure canvas-space geometry for the native PC action-bar variant menu. It uses
    /// the rendered anchor, content, and safe rectangle supplied by the UI adapter;
    /// no screen resolution, aspect ratio, option count, or UI scale is assumed.
    /// </summary>
    internal static class SummonVariantMenuLayoutPolicy
    {
        internal const float Epsilon = 0.01f;

        internal static SummonVariantMenuLayoutDecision Decide(
            SummonVariantMenuLayoutRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            SummonVariantMenuRect safe = Inset(request.CanvasSafeRect,
                request.MinimumMargin);
            float width = Math.Min(request.DesiredWidth, safe.Width);
            float height = Math.Min(request.DesiredHeight, safe.Height);
            SummonVariantMenuOpeningDirection direction =
                request.PreferredDirection;

            float x;
            if (request.Anchor.XMax + width <= safe.XMax + Epsilon)
                x = request.Anchor.XMax;
            else if (request.Anchor.XMin - width >= safe.XMin - Epsilon)
                x = request.Anchor.XMin - width;
            else
                x = Clamp(request.Anchor.XMin, safe.XMin, safe.XMax - width);

            float preferredY = direction == SummonVariantMenuOpeningDirection.Down
                ? request.Anchor.YMin - height
                : request.Anchor.YMax;
            float y = Clamp(preferredY, safe.YMin, safe.YMax - height);
            bool topClamped = preferredY + height > safe.YMax + Epsilon &&
                Math.Abs(y + height - safe.YMax) <= Epsilon;
            bool bottomClamped = preferredY < safe.YMin - Epsilon &&
                Math.Abs(y - safe.YMin) <= Epsilon;
            var finalRect = new SummonVariantMenuRect(x, y, width, height);
            if (!safe.Contains(finalRect, Epsilon))
            {
                throw new InvalidOperationException(
                    "Summon variant menu geometry escaped the canvas-safe rectangle.");
            }

            return new SummonVariantMenuLayoutDecision(safe, finalRect,
                direction, request.DesiredWidth, request.DesiredHeight,
                topClamped, bottomClamped);
        }

        private static SummonVariantMenuRect Inset(
            SummonVariantMenuRect rect, float margin)
        {
            return new SummonVariantMenuRect(rect.X + margin, rect.Y + margin,
                rect.Width - (margin * 2f), rect.Height - (margin * 2f));
        }

        private static float Clamp(float value, float minimum, float maximum)
        {
            if (maximum < minimum) return minimum;
            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }
}
