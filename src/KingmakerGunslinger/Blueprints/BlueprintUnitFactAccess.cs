using System;
using System.Reflection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Presentation;
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

        internal LocalizedString GetDisplayName(BlueprintUnitFact fact)
        {
            if (fact == null) throw new ArgumentNullException("fact");
            return (LocalizedString)_displayName.GetValue(fact);
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

        /// <summary>
        /// Puts back a description reference captured before a publication,
        /// exactly as captured. The captured value was the installation's own
        /// state, so it is not validated as new display text: it may be null,
        /// or resolve to nothing in the active localization pack.
        /// </summary>
        internal void RestoreDescription(BlueprintUnitFact fact,
            LocalizedString captured)
        {
            if (fact == null) throw new ArgumentNullException("fact");
            _description.SetValue(fact, captured);
            if (!ReferenceEquals(_description.GetValue(fact), captured))
                throw new InvalidOperationException(
                    "The unit-fact description restoration did not verify.");
        }

        internal PlayerFacingTextState GetNameState(BlueprintUnitFact fact)
        {
            if (fact == null) throw new ArgumentNullException("fact");
            return PlayerFacingTextAssignment.Classify(
                IsStored(GetDisplayName(fact)), fact.Name,
                LocalizationService.ResolveUnassigned());
        }

        internal PlayerFacingTextState GetDescriptionState(BlueprintUnitFact fact)
        {
            if (fact == null) throw new ArgumentNullException("fact");
            return PlayerFacingTextAssignment.Classify(
                IsStored(GetDescription(fact)), fact.Description,
                LocalizationService.ResolveUnassigned());
        }

        /// <summary>
        /// Sets Kingmaker's own HiddenInUi buff flag, keeping every other
        /// flag, and confirms the engine now reports the buff as hidden.
        /// </summary>
        internal static void HideBuffInUi(BlueprintBuff buff)
        {
            if (buff == null) throw new ArgumentNullException("buff");
            FieldInfo flags = typeof(BlueprintBuff).GetField("m_Flags", Fields);
            if (flags == null || !flags.FieldType.IsEnum ||
                !Enum.IsDefined(flags.FieldType, "HiddenInUi"))
                throw new MissingFieldException(typeof(BlueprintBuff).FullName,
                    "m_Flags");
            int hidden = Convert.ToInt32(Enum.Parse(flags.FieldType, "HiddenInUi"));
            int current = Convert.ToInt32(flags.GetValue(buff));
            flags.SetValue(buff, Enum.ToObject(flags.FieldType, current | hidden));
            if (!buff.IsHiddenInUI)
                throw new InvalidOperationException(
                    "The buff UI-visibility flag did not verify.");
        }

        /// <summary>
        /// Gives a buff that has no visual effect the empty effect links
        /// Kingmaker expects. Buff activation loads FxOnStart without a null
        /// check; on a buff built without the link it throws after the buff
        /// is already active, the rule that applied it swallows the error,
        /// and the "buff added" notification and expiry scheduling never
        /// run, so the buff works but is missing from the party bar. An
        /// empty link loads nothing and spawns nothing.
        /// </summary>
        internal static void UseEmptyBuffEffects(BlueprintBuff buff)
        {
            if (buff == null) throw new ArgumentNullException("buff");
            if (buff.FxOnStart == null) buff.FxOnStart = new PrefabLink();
            if (buff.FxOnRemove == null) buff.FxOnRemove = new PrefabLink();
            if (buff.ResourceAssetIds == null)
                buff.ResourceAssetIds = Array.Empty<string>();
        }

        private static bool IsStored(LocalizedString value)
        {
            return value != null && !value.IsEmpty();
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
