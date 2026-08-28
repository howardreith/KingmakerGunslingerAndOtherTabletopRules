using System;

namespace KingmakerGunslinger.Fatigue
{
    /// <summary>
    /// Carries one explicit fatigue intent to one exact synchronous native
    /// application. Arbitrary applications have native passthrough semantics.
    /// </summary>
    internal static class CanonicalFatigueApplicationIntentScope
    {
        [ThreadStatic] private static Request _active;

        internal static Request EnterAcadamaeEscalation(
            object buffCollection, object expectedBlueprint)
        {
            if (buffCollection == null)
                throw new ArgumentNullException("buffCollection");
            if (expectedBlueprint == null)
                throw new ArgumentNullException("expectedBlueprint");

            var request = new Request(buffCollection, expectedBlueprint,
                _active);
            _active = request;
            return request;
        }

        internal static CanonicalFatigueApplicationIntent Claim(
            object buffCollection, object blueprint)
        {
            Request request = _active;
            if (request == null || request.Disposed || request.Claimed ||
                !ReferenceEquals(request.BuffCollection, buffCollection) ||
                !ReferenceEquals(request.ExpectedBlueprint, blueprint))
            {
                return CanonicalFatigueApplicationIntent.NativePassthrough;
            }

            request.Claimed = true;
            return CanonicalFatigueApplicationIntent
                .EscalateIfAlreadyFatigued;
        }

        internal sealed class Request : IDisposable
        {
            internal Request(object buffCollection, object expectedBlueprint,
                Request parent)
            {
                BuffCollection = buffCollection;
                ExpectedBlueprint = expectedBlueprint;
                Parent = parent;
            }

            internal object BuffCollection { get; private set; }
            internal object ExpectedBlueprint { get; private set; }
            internal Request Parent { get; set; }
            internal bool Claimed { get; set; }
            internal bool Disposed { get; private set; }

            public void Dispose()
            {
                if (Disposed) return;
                Disposed = true;
                if (ReferenceEquals(_active, this))
                {
                    _active = Parent;
                    return;
                }

                for (Request current = _active; current != null;
                    current = current.Parent)
                {
                    if (ReferenceEquals(current.Parent, this))
                    {
                        current.Parent = Parent;
                        return;
                    }
                }
            }
        }
    }
}
