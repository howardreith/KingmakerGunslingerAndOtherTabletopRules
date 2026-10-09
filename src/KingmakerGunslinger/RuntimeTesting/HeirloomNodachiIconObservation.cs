using System;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Request-local read-only observer inputs. A selection derives from feature,
    // but RequireExact deliberately does not accept derived blueprint types.
    internal static class HeirloomNodachiIconObservation
    {
        internal static TFeature[] ReadConsumers<TFeature, TSelection>(
            Func<string, TSelection> readSelection, Func<string, TFeature> readFeature)
            where TSelection : TFeature
        {
            if (readSelection == null) throw new ArgumentNullException("readSelection");
            if (readFeature == null) throw new ArgumentNullException("readFeature");
            return new TFeature[] {
                readSelection("5ae9f898e45846d19d3802caf91e06b6"),
                readFeature("af205733f7fe49838edb37cdf1b90cbb"),
                readFeature("4caf60ed8b264701a3965288a65eebc2"),
                readFeature("e17fafa6f75641f8a2e3fe4b6f71da78")
            };
        }
    }
}
