using System;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// The footprint arithmetic on its own, in metres, with no engine types so
    /// it can be exercised directly with representative footprints.
    /// <see cref="SummonFootprintGeometry"/> reads the live positions and
    /// corpulences and calls straight into this; it is the same code path, not
    /// a parallel copy, so a passing observation here is an observation about
    /// production.
    /// </summary>
    internal static class SummonFootprintGeometryPolicy
    {
        /// <summary>
        /// Installed 2.1.7b exposes this as the public literal
        /// <c>Kingmaker.Utility.Feet::FeetToMetersRatio = float32(0.3048)</c>,
        /// and <c>Feet::get_Meters</c> is exactly <c>m_Value * 0.3048</c>.
        /// </summary>
        internal const float FeetToMeters = 0.3048f;

        /// <summary>
        /// One five-foot square of slack beyond the touching footprints, which
        /// is what "in an adjacent square" means once the square grid is
        /// approximated by the circular corpulence the engine actually stores.
        /// The hundredth of a metre absorbs float noise in a position that is
        /// meant to be exactly one square away.
        /// </summary>
        internal const float AdjacencyToleranceMeters =
            (5f * FeetToMeters) + 0.01f;

        /// <summary>
        /// Gap between two footprint edges. Both corpulences are subtracted —
        /// that is the whole point, and the reason one radius constant cannot
        /// stand in for this across differing sizes.
        /// </summary>
        internal static float EdgeDistanceMeters(float centreDistanceMeters,
            float leftCorpulenceMeters, float rightCorpulenceMeters)
        {
            return centreDistanceMeters - Math.Max(0f, leftCorpulenceMeters) -
                Math.Max(0f, rightCorpulenceMeters);
        }

        internal static bool IsAdjacentDistance(float centreDistanceMeters,
            float leftCorpulenceMeters, float rightCorpulenceMeters)
        {
            return EdgeDistanceMeters(centreDistanceMeters,
                leftCorpulenceMeters, rightCorpulenceMeters) <=
                AdjacencyToleranceMeters;
        }

        /// <summary>
        /// Whether a broad-phase collection radius, measured from the caster's
        /// centre, still admits the furthest target that this relation would
        /// call adjacent.
        ///
        /// <para><c>GameHelper.GetTargetsAround</c> admits a unit when
        /// <c>unit.DistanceTo(point) &lt;= radius + unit.View.Corpulence</c>:
        /// only the target's own corpulence is added, and the caster's
        /// half-footprint is not subtracted. So the bound has to clear the
        /// caster's own corpulence plus the tolerance, and nothing else.</para>
        /// </summary>
        internal static bool CollectionRadiusAdmitsAdjacent(
            float radiusMeters, float casterCorpulenceMeters,
            float targetCorpulenceMeters)
        {
            float furthestAdjacentCentres = casterCorpulenceMeters +
                targetCorpulenceMeters + AdjacencyToleranceMeters;
            return furthestAdjacentCentres <=
                radiusMeters + targetCorpulenceMeters;
        }
    }
}
