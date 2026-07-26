using System;
using System.Runtime.CompilerServices;

namespace KingmakerGunslinger.Firing
{
    /// <summary>
    /// Weak reference-identity stamp preventing one Kingmaker rule-event object from
    /// consuming more than one loaded round if a Harmony callback is observed twice.
    /// </summary>
    internal sealed class ReferenceEventGate
    {
        private readonly object _gate = new object();
        private readonly ConditionalWeakTable<object, Marker> _seen =
            new ConditionalWeakTable<object, Marker>();

        internal bool TryMark(object eventInstance)
        {
            if (eventInstance == null)
            {
                throw new ArgumentNullException("eventInstance");
            }

            if (eventInstance.GetType().IsValueType)
            {
                throw new ArgumentException(
                    "A rule-event identity must be a reference type.",
                    "eventInstance");
            }

            lock (_gate)
            {
                Marker ignored;
                if (_seen.TryGetValue(eventInstance, out ignored))
                {
                    return false;
                }

                _seen.Add(eventInstance, new Marker());
                return true;
            }
        }

        private sealed class Marker
        {
        }
    }
}
