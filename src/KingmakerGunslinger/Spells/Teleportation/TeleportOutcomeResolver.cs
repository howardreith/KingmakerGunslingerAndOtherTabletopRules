using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal enum TeleportExecutionStatus { Arrived, NoLegalAlternate, DefensiveMishapLimit }
    internal sealed class TeleportExecutionResult
    {
        internal TeleportExecutionResult(TeleportExecutionStatus status, TeleportOutcomeKind outcome,
            string destinationId, int d100, int mishaps, TeleportAlternateDecision alternate)
        { Status = status; Outcome = outcome; DestinationId = destinationId; D100 = d100; Mishaps = mishaps; Alternate = alternate; }
        internal TeleportExecutionStatus Status { get; private set; }
        internal TeleportOutcomeKind Outcome { get; private set; }
        internal string DestinationId { get; private set; }
        internal int D100 { get; private set; }
        internal int Mishaps { get; private set; }
        internal TeleportAlternateDecision Alternate { get; private set; }
    }
    internal sealed class TeleportTravelerSnapshot
    {
        internal TeleportTravelerSnapshot(string id, bool living, bool traveling)
        { Id = id; Living = living; Traveling = traveling; }
        internal string Id { get; private set; }
        internal bool Living { get; private set; }
        internal bool Traveling { get; private set; }
    }
    internal interface ITeleportOutcomeWorld
    {
        int RollD100();
        int RollD10();
        IReadOnlyList<TeleportTravelerSnapshot> CaptureTravelers();
        void DamageTraveler(string id, int amount);
        TeleportAlternateDecision SelectAlternate(string intendedId, string originId, double severity);
        void Relocate(string destinationId, Action materialEffectStarting);
    }
    internal static class TeleportOutcomeResolver
    {
        internal const int DefensiveMishapLimit = 1024;
        internal static TeleportExecutionResult Resolve(TeleportSpellKind spell, TeleportFamiliarity familiarity,
            string intendedId, string originId, ITeleportOutcomeWorld world, Action materialEffectStarting)
        {
            if (world == null) throw new ArgumentNullException("world");
            if (materialEffectStarting == null) throw new ArgumentNullException("materialEffectStarting");
            if (!TeleportDestinationPolicy.IsStableId(intendedId) || !TeleportDestinationPolicy.IsStableId(originId) || intendedId == originId)
                throw new ArgumentException("Distinct stable origin and destination required.");
            if (!Enum.IsDefined(typeof(TeleportSpellKind), spell)) throw new ArgumentOutOfRangeException("spell");
            int roll = 0, mishaps = 0;
            TeleportOutcomeKind kind = TeleportOutcomeKind.OnTarget;
            TeleportAlternateDecision alternate = null;
            string final = intendedId;
            TeleportExecutionStatus status = TeleportExecutionStatus.Arrived;
            if (spell == TeleportSpellKind.Teleport)
            {
                TeleportOutcomeDecision table = TeleportRollTable.For(familiarity);
                while (true)
                {
                    roll = world.RollD100();
                    kind = table.Resolve(roll);
                    if (kind != TeleportOutcomeKind.Mishap) break;
                    int damage = world.RollD10();
                    if (damage < 1 || damage > 10) throw new InvalidOperationException("Canonical/injected d10 must be in 1..10.");
                    IReadOnlyList<TeleportTravelerSnapshot> travelers = world.CaptureTravelers();
                    if (travelers == null || travelers.Any(value => value == null || string.IsNullOrWhiteSpace(value.Id)) ||
                        travelers.Select(value => value.Id).Distinct(StringComparer.Ordinal).Count() != travelers.Count)
                        throw new InvalidOperationException("Canonical traveler identities are missing or ambiguous.");
                    foreach (var traveler in travelers.Where(value => value.Living && value.Traveling))
                    {
                        materialEffectStarting();
                        world.DamageTraveler(traveler.Id, damage);
                    }
                    mishaps++;
                    if (mishaps >= DefensiveMishapLimit)
                    {
                        status = TeleportExecutionStatus.DefensiveMishapLimit;
                        final = originId;
                        break;
                    }
                }
                if (kind == TeleportOutcomeKind.OffTarget || kind == TeleportOutcomeKind.SimilarLocation)
                {
                    alternate = world.SelectAlternate(intendedId, originId,
                        TeleportFailureSeverityPolicy.Calculate(roll, table.OnTargetPercent));
                    if (alternate == null) throw new InvalidOperationException("Alternate selection returned no decision.");
                    if (!alternate.Found) { status = TeleportExecutionStatus.NoLegalAlternate; final = originId; }
                    else
                    {
                        if (!TeleportDestinationPolicy.IsStableId(alternate.Id) || alternate.Id == intendedId || alternate.Id == originId)
                            throw new InvalidOperationException("Alternate violates distinct stable destination contract.");
                        final = alternate.Id;
                    }
                }
            }
            if (status == TeleportExecutionStatus.Arrived) world.Relocate(final, materialEffectStarting);
            var result = new TeleportExecutionResult(status, kind, final, roll, mishaps, alternate);
            return result;
        }
    }
}
