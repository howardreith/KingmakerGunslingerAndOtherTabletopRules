using System;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.AidAnotherCompatibility
{
    internal static class AidAnotherCompatibilityDiagnostics
    {
        internal static void Info(ModContext context, string code,
            string message)
        {
            if (context != null) context.Logger.Info("aid-another", code,
                message);
        }

        internal static void Warning(ModContext context, string code,
            string message)
        {
            if (context != null) context.Logger.Warning("aid-another", code,
                message);
        }

        internal static void Failure(ModContext context, string code,
            string message, Exception exception)
        {
            if (context != null) context.Logger.Failure("aid-another", code,
                message, exception);
        }
    }
}
