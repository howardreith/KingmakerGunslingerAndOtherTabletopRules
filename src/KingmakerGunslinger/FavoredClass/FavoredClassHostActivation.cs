namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// Whether the exact qualified host's owned publication is committed and
    /// live in this process. It starts inactive and becomes active only after
    /// a successful commit; an absent, disabled, unsupported or incompletely
    /// initialized host, a disabled integration, a failed publication and any
    /// rollback make it inactive again. Owned numerical effects require it;
    /// saved ranks always resolve, and the Mostly Human racial identity and
    /// its bridge do not depend on it.
    /// </summary>
    internal sealed class FavoredClassHostActivation
    {
        private readonly object _gate = new object();
        private bool _active;
        private string _reason = "not-resolved";

        internal bool IsActive
        {
            get { lock (_gate) return _active; }
        }

        /// <summary>Why the host is (in)active, for diagnostics.</summary>
        internal string Reason
        {
            get { lock (_gate) return _reason; }
        }

        /// <summary>The committed publication of the exact host is live.</summary>
        internal void Activate(string reason)
        {
            lock (_gate)
            {
                _active = true;
                _reason = reason ?? "published";
            }
        }

        /// <summary>Any state other than a live committed publication.</summary>
        internal void Deactivate(string reason)
        {
            lock (_gate)
            {
                _active = false;
                _reason = reason ?? "inactive";
            }
        }

        /// <summary>Owned numerical effects need the integration enabled and the host active.</summary>
        internal static bool MechanicsEnabled(bool integrationEnabled, bool hostActive)
        {
            return integrationEnabled && hostActive;
        }
    }
}
