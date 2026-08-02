using KingmakerGunslinger.Deeds;
namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void DeathsShotCritical()
        {
            DeathsShotDecision value = new DeathsShotService().Evaluate(19, 5,
                1, true, true, true, true, false, true);
            Assertions.True(value.ShouldSave, "Confirmed critical must save.");
            Assertions.Equal(24, value.DifficultyClass, "Death's Shot DC changed.");
            Assertions.Equal(1, value.GritCost, "Death's Shot cost changed.");
        }
        private static void DeathsShotNoncritical()
        {
            DeathsShotDecision value = new DeathsShotService().Evaluate(19, 5,
                1, true, true, true, false, false, true);
            Assertions.True(value.ConsumeMarker && !value.ShouldSave,
                "Noncritical hit must consume without saving.");
        }
        private static void DeathsShotGates()
        {
            var service = new DeathsShotService();
            Assertions.False(service.Evaluate(19, 5, 0, true, true, true,
                true, false, true).ShouldSave, "Zero grit must reject rider.");
            Assertions.False(service.Evaluate(19, 5, 1, true, true, true,
                true, true, true).ShouldSave, "Critical immunity must reject rider.");
        }
    }
}
