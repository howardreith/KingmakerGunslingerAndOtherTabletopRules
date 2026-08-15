using System;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurDiagnostics
    {
        internal static void Info(ModContext context, string code, string message)
        {
            if (context != null) context.Logger.Info("brown-fur", code, message);
        }

        internal static void Warning(ModContext context, string code, string message)
        {
            if (context != null) context.Logger.Warning("brown-fur", code, message);
        }

        internal static void Failure(ModContext context, string code,
            string message, Exception exception)
        {
            if (context != null) context.Logger.Failure("brown-fur", code,
                message, exception);
        }
    }
}
