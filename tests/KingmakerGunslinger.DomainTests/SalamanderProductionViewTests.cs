using System;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SalamanderProductionViewTests
    {
        internal static void ProductionBindingIsExactAndSeparateFromPrototype()
        {
            string[] row = { SalamanderRulesPolicy.UnitGuid, SalamanderRulesPolicy.UnitName,
                SalamanderTailAnimationPolicy.Prefab, SalamanderTailAnimationPolicy.Spear };
            Func<string[], bool, bool, bool> permits = (r, enabled, polymorphed) =>
                SalamanderProductionViewPolicy.Permits(enabled, r[0], r[1], r[2], r[3], polymorphed);
            Assertions.True(permits(row, true, false), "Exact registered production Salamander.");
            Assertions.False(permits(row, false, false), "Disabled module.");
            Assertions.False(permits(row, true, true), "Polymorph keeps native visual behavior.");
            for (int index = 0; index < row.Length; index++)
            foreach (string value in new[] { null, "", "foreign", row[index].ToUpperInvariant(), SalamanderTailAnimationPolicy.PrototypeName })
            {
                var changed = (string[])row.Clone(); changed[index] = value;
                Assertions.False(permits(changed, true, false), "No native human, old donor, snake, prototype or partial identity match.");
            }
            foreach (string name in new[] { SalamanderRulesPolicy.UnitName, SalamanderTailAnimationPolicy.PrototypeName })
            {
                Assertions.True(SalamanderTailAnimationPolicy.PermitsBinding(true, row[0], name, row[2], row[3], true, true),
                    "Only the two closed owners share the reviewed action adapter.");
                Assertions.False(SalamanderTailAnimationPolicy.PermitsBinding(true, row[0], name, row[2], row[3], false, true), "Player must be owned.");
                Assertions.False(SalamanderTailAnimationPolicy.PermitsBinding(true, row[0], name, row[2], row[3], true, false), "Bones must be owned.");
            }
        }

        internal static void BindingRequiresConsecutiveSameNativeMesh()
        {
            var gate = new SalamanderViewSettlement(); var mesh = new object();
            Assertions.Equal(SalamanderViewSettlementResult.Waiting, gate.Observe(10, mesh, true), "First ready frame is not stability.");
            Assertions.Equal(SalamanderViewSettlementResult.Waiting, gate.Observe(10, mesh, true), "Duplicate callback is not another frame.");
            Assertions.Equal(SalamanderViewSettlementResult.Ready, gate.Observe(11, mesh, true), "Same native mesh on next ready LateUpdate.");
            Assertions.Equal(SalamanderViewSettlementResult.Closed, gate.Observe(12, mesh, true), "Only one attachment attempt.");
        }

        internal static void UnreadyChangingAndSkippedFramesCannotBind()
        {
            var gate = new SalamanderViewSettlement(); var mesh = new object(); var other = new object();
            Assertions.Equal(SalamanderViewSettlementResult.Waiting, gate.Observe(1, null, true), "Missing native mesh is never ready.");
            Assertions.Equal(SalamanderViewSettlementResult.Waiting, gate.Observe(2, mesh, true), "First real mesh.");
            Assertions.Equal(SalamanderViewSettlementResult.Waiting, gate.Observe(3, other, true), "Native outfit rebuild resets stability.");
            Assertions.Equal(SalamanderViewSettlementResult.Waiting, gate.Observe(4, other, false), "Incomplete palette/material/weapon resets stability.");
            Assertions.Equal(SalamanderViewSettlementResult.Waiting, gate.Observe(5, other, true), "Recovered first frame.");
            Assertions.Equal(SalamanderViewSettlementResult.Waiting, gate.Observe(8, other, true), "Skipped observation cannot infer stability.");
            Assertions.Equal(SalamanderViewSettlementResult.Ready, gate.Observe(9, other, true), "Actual consecutive ready frames.");
        }

        internal static void SettlementIsBoundedAndInvalidClockFailsClosed()
        {
            var gate = new SalamanderViewSettlement();
            for (int frame = 0; frame < SalamanderViewSettlement.MaximumObservations; frame++)
                Assertions.Equal(SalamanderViewSettlementResult.Waiting, gate.Observe(frame, null, false), "Observe without mutating native readiness.");
            Assertions.Equal(SalamanderViewSettlementResult.Expired, gate.Observe(600, null, false), "Bounded timeout leaves donor fallback.");
            Assertions.Equal(SalamanderViewSettlementResult.Closed, gate.Observe(601, new object(), true), "No late automatic retry.");
            foreach (int frame in new[] { -1, 9 })
            {
                var invalid = new SalamanderViewSettlement(); invalid.Observe(10, new object(), true);
                Assertions.Equal(SalamanderViewSettlementResult.Expired, invalid.Observe(frame, new object(), true), "Invalid/rewound native frame fails closed.");
            }
        }
    }
}
