using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Newtonsoft.Json;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>Factory-only finalization of exclusively project-owned components.
    /// No registry/native blueprint sweep, live fact mutation, or Harmony rewrite.</summary>
    internal static class ElementalComponentIdentity
    {
        internal static T Prepare<T>(T blueprint) where T : BlueprintScriptableObject
        {
            if (blueprint == null || !blueprint.name.StartsWith("KMG_ElementalRaces_",
                    StringComparison.Ordinal))
                throw new ArgumentException("Only a factory-owned Elemental blueprint may be finalized.");
            GameLogicComponent[] components = (blueprint.ComponentsArray ??
                new BlueprintComponent[0]).OfType<GameLogicComponent>().ToArray();
            string[] keys = components.Select(value => value.GetType().FullName +
                (value is AddStatBonus ? "." + ((AddStatBonus)value).Stat : string.Empty)).ToArray();
            string[] names = ElementalComponentIdentityPolicy.Plan(
                components.Select(value => value.name).ToArray(), keys,
                components.Select(value => HasSavedFields(value.GetType())).ToArray());
            for (int index = 0; index < components.Length; index++)
                if (!string.Equals(components[index].name, names[index], StringComparison.Ordinal))
                    components[index].name = names[index];
            return blueprint;
        }

        private static bool HasSavedFields(Type type)
        {
            for (Type current = type; current != null; current = current.BaseType)
                if (current.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly).Any(field =>
                        field.GetCustomAttributes(typeof(JsonPropertyAttribute), false).Any()))
                    return true;
            return false;
        }
    }
}
