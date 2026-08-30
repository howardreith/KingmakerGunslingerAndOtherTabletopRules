using System;
using System.Reflection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Localization;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Exact private-field adapter for custom unit-fact display text and icon.
    /// </summary>
    internal sealed class BlueprintUnitFactAccess
    {
        private const BindingFlags Fields =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly FieldInfo _displayName;
        private readonly FieldInfo _description;
        private readonly FieldInfo _icon;

        private BlueprintUnitFactAccess()
        {
            Type type = typeof(BlueprintUnitFact);
            _displayName = Require(type, "m_DisplayName", typeof(LocalizedString));
            _description = Require(type, "m_Description", typeof(LocalizedString));
            _icon = Require(type, "m_Icon", typeof(Sprite));
        }

        internal static BlueprintUnitFactAccess Resolve()
        {
            return new BlueprintUnitFactAccess();
        }

        internal void Configure(
            BlueprintUnitFact fact,
            LocalizedString displayName,
            LocalizedString description,
            Sprite icon)
        {
            if (fact == null)
            {
                throw new ArgumentNullException("fact");
            }

            if (displayName == null)
            {
                throw new ArgumentNullException("displayName");
            }

            if (description == null)
            {
                throw new ArgumentNullException("description");
            }

            _displayName.SetValue(fact, displayName);
            _description.SetValue(fact, description);
            _icon.SetValue(fact, icon);

            if (string.IsNullOrWhiteSpace(fact.Name) ||
                string.IsNullOrWhiteSpace(fact.Description))
            {
                throw new InvalidOperationException(
                    "The unit-fact text was not readable after assignment.");
            }
        }

        internal void SetIconIfMissing(BlueprintUnitFact fact, Sprite icon)
        {
            if (fact == null) throw new ArgumentNullException("fact");
            if (icon == null) throw new ArgumentNullException("icon");
            if (fact.Icon == null) _icon.SetValue(fact, icon);
            if (fact.Icon == null)
                throw new InvalidOperationException(
                    "The unit-fact icon was not readable after assignment.");
        }

        internal void SetIcon(BlueprintUnitFact fact, Sprite icon)
        {
            if (fact == null) throw new ArgumentNullException("fact");
            if (icon == null) throw new ArgumentNullException("icon");
            _icon.SetValue(fact, icon);
            if (!ReferenceEquals(fact.Icon, icon))
                throw new InvalidOperationException("The unit-fact icon assignment did not verify.");
        }

        internal LocalizedString GetDescription(BlueprintUnitFact fact)
        {
            if (fact == null) throw new ArgumentNullException("fact");
            return (LocalizedString)_description.GetValue(fact);
        }

        internal void SetDescription(BlueprintUnitFact fact,
            LocalizedString description)
        {
            if (fact == null) throw new ArgumentNullException("fact");
            if (description == null)
                throw new ArgumentNullException("description");
            _description.SetValue(fact, description);
            if (!ReferenceEquals(_description.GetValue(fact), description) ||
                string.IsNullOrWhiteSpace(fact.Description))
                throw new InvalidOperationException(
                    "The unit-fact description assignment did not verify.");
        }

        private static FieldInfo Require(
            Type type,
            string name,
            Type expectedType)
        {
            FieldInfo field = type.GetField(name, Fields);
            if (field == null || field.FieldType != expectedType)
            {
                throw new MissingFieldException(type.FullName, name);
            }

            return field;
        }
    }
}
