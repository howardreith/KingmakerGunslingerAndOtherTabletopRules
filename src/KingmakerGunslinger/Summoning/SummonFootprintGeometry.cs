using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Footprint relations between two live units, measured from the native
    /// positions and corpulences rather than from a size table or a creature's
    /// weapon reach.
    ///
    /// <para><b>Why this is not the same thing as reach.</b> Adjacency and
    /// melee reach are different quantities and happen to coincide for some
    /// creatures and not others. The released
    /// <c>ContextActionUngulateTrample</c> already measures adjacency exactly
    /// this way — edge to edge, both corpulences subtracted — and this is that
    /// same contract named once so a second consumer cannot drift from it.</para>
    ///
    /// <para><b>Why a caller cannot just hand a radius to the native area
    /// collector.</b> <c>AbilityTargetsAround.Select</c> anchors on the
    /// caster's <c>EyePosition</c> and hands that point to
    /// <c>GameHelper.GetTargetsAround</c>, which admits a unit when
    /// <c>unit.DistanceTo(point) &lt;= radius + unit.View.Corpulence</c>. Only
    /// the *target's* corpulence is added; the caster's own half-footprint is
    /// not subtracted. So a radius measured from the caster's centre reaches a
    /// different distance for a Large creature than for a Medium one, and no
    /// single scalar expresses "adjacent" for both. The supported shape is to
    /// over-collect with a generous radius and then apply this exact relation.</para>
    /// </summary>
    internal static class SummonFootprintGeometry
    {
        /// <summary>
        /// One five-foot square of slack beyond the touching footprints. Held
        /// in <see cref="SummonFootprintGeometryPolicy"/> so the threshold a
        /// test observes is the threshold production applies.
        /// </summary>
        internal static float AdjacencyToleranceMeters
        { get { return SummonFootprintGeometryPolicy.AdjacencyToleranceMeters; } }

        internal static float EdgeDistanceMeters(UnitEntityData left,
            UnitEntityData right)
        {
            if (left == null || right == null) return float.PositiveInfinity;
            return SummonFootprintGeometryPolicy.EdgeDistanceMeters(
                Vector3.Distance(left.Position, right.Position),
                left.Corpulence, right.Corpulence);
        }

        internal static bool AreAdjacent(UnitEntityData left,
            UnitEntityData right)
        {
            if (left == null || right == null) return false;
            return SummonFootprintGeometryPolicy.IsAdjacentDistance(
                Vector3.Distance(left.Position, right.Position),
                left.Corpulence, right.Corpulence);
        }
    }
}
