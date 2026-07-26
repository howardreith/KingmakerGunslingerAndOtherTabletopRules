using System;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Executes a one-time, non-gameplay runtime proof that the immutable domain
    /// object can be copied into a real Kingmaker BlueprintComponent and read back.
    /// The probe component is deliberately not attached to or registered as a blueprint.
    /// </summary>
    internal static class FirearmDomainProbe
    {
        private static readonly object Gate = new object();
        private static FirearmDefinitionComponent _component;
        private static FirearmDefinition _definition;

        internal static FirearmDefinitionComponent MarkerComponent
        {
            get
            {
                lock (Gate)
                {
                    return _component;
                }
            }
        }

        internal static FirearmDefinition VerifyMarkerRoundTrip()
        {
            lock (Gate)
            {
                if (_definition != null)
                {
                    if (_component == null)
                    {
                        throw new InvalidOperationException(
                            "The retained firearm marker was unexpectedly lost.");
                    }

                    return _definition;
                }

                FirearmDefinition expected = FirearmDefinitions.CreateEarlyMusket();

                FirearmDefinitionComponent component =
                    FirearmDefinitionComponent.Create(expected);
                if (component == null)
                {
                    throw new InvalidOperationException(
                        "Unity returned null while creating the firearm definition marker component.");
                }

                FirearmDefinition actual = component.Definition;
                if (!expected.Equals(actual))
                {
                    throw new InvalidOperationException(
                        "The firearm definition marker did not preserve its immutable configuration.");
                }

                // Keep the single diagnostic ScriptableObject alive for the process.
                // It is neither registered nor attached to a blueprint, unit, item, or save.
                _component = component;
                _definition = actual;
                return _definition;
            }
        }
    }
}
