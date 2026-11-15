using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.Grit
{
    /// <summary>How an ability shows the shared grit pool on the action bar.</summary>
    internal enum GritUiContract
    {
        /// <summary>
        /// Receives the shared indicator: a count of remaining grit and a disabled
        /// button at zero. The indicator never spends; the deed's own transaction
        /// does, on activation or later through its marker or handler.
        /// </summary>
        SharedIndicator,

        /// <summary>
        /// Built with its own grit resource logic, which shows and spends through
        /// Kingmaker's resource path. It must not receive a second component.
        /// </summary>
        OwnResourceLogic
    }

    internal sealed class GritUiRecipient
    {
        internal GritUiRecipient(string deed, BlueprintAbility ability,
            GritUiContract contract)
        {
            if (string.IsNullOrEmpty(deed)) throw new ArgumentException("deed");
            Deed = deed; Ability = ability; Contract = contract;
        }

        /// <summary>The audited deed label; safe to print when the ability is missing.</summary>
        internal string Deed { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal GritUiContract Contract { get; private set; }
    }

    internal static class GritAbilityUiIntegration
    {
        internal const string SharedComponentName = "$KMG_SharedGritUi";

        /// <summary>
        /// Every ability whose button shows grit, by blueprint identity. Membership
        /// is decided here and nowhere else: no ability joins because of its
        /// displayed text or because a component happens to reference the pool.
        /// Deeds that only need grit to remain (Quick Clear as a standard action,
        /// Utility Shot, Startling Shot, Lightning Reload) are gated by their own
        /// logic and are deliberately absent, as in every released version. Every
        /// entry is required; the three archetypes are always registered before
        /// this runs.
        /// </summary>
        internal static GritUiRecipient[] Recipients(GunslingerClassBlueprintSet set)
        {
            if (set == null) throw new ArgumentNullException("set");
            MysteriousStrangerBlueprintSet stranger = set.MysteriousStranger ??
                throw Missing("the Mysterious Stranger archetype");
            PistoleroBlueprintSet pistolero = set.Pistolero ??
                throw Missing("the Pistolero archetype");
            MusketMasterBlueprintSet musketMaster = set.MusketMaster ??
                throw Missing("the Musket Master archetype");
            BlueprintAbility[] bleeding = set.BleedingWound.Abilities;
            if (bleeding == null || bleeding.Length != 4)
                throw new InvalidOperationException(
                    "Grit UI recipient Bleeding Wound exposes " +
                    (bleeding == null ? "no" : bleeding.Length.ToString()) +
                    " arming abilities; four are required.");
            return new[]
            {
                // Spend through Kingmaker's resource path with their own component.
                Own("Deadeye", set.Deadeye.Ability),
                Own("Gunslinger's Dodge", set.Dodge.ProneAbility),
                // Spend when used, through the deed transaction.
                Shared("Quick Clear (move action)", set.QuickClear.MoveAbility),
                Shared("Pistol Whip", set.PistolWhip.Ability),
                Shared("Dead Shot", set.DeadShot.Ability),
                Shared("Targeting (Arms)", set.TargetingArms.Ability),
                Shared("Targeting (Head)", set.TargetingHead.Ability),
                Shared("Targeting (Torso)", set.TargetingTorso.Ability),
                Shared("Targeting (Legs)", set.TargetingLegs.Ability),
                Shared("Menacing Shot", set.MenacingShot.Ability),
                // Arm a marker; the marker's handler spends when it triggers.
                Shared("Bleeding Wound (Hit Points)", bleeding[0]),
                Shared("Bleeding Wound (Strength)", bleeding[1]),
                Shared("Bleeding Wound (Dexterity)", bleeding[2]),
                Shared("Bleeding Wound (Constitution)", bleeding[3]),
                Shared("Expert Loading", set.ExpertLoading.Ability),
                Shared("Slinger's Luck (saving throw)", set.SlingersLuck.SavingAbility),
                Shared("Slinger's Luck (skill check)", set.SlingersLuck.SkillAbility),
                Shared("Death's Shot", set.DeathsShot.Ability),
                Shared("Stunning Shot", set.StunningShot.Ability),
                // Archetype deeds.
                Shared("Focused Aim", stranger.FocusedAimAbility),
                Shared("Clipping Shot", stranger.ClippingShotAbility),
                Shared("Up Close and Deadly", pistolero.UpCloseAbility),
                Shared("Twin Shot Knockdown", pistolero.TwinShotAbility),
                // Spends nothing but needs grit; has always carried the indicator.
                Shared("Steady Aim", musketMaster.SteadyAimAbility)
            };
        }

        internal static int Apply(GunslingerClassBlueprintSet set)
        {
            if (set == null) throw new ArgumentNullException("set");
            return Apply(set.Grit.Resource, Recipients(set));
        }

        /// <summary>
        /// Checks every recipient before changing any, adds the indicator where it
        /// is absent, then verifies the whole set. A repeat on a satisfied set
        /// changes nothing. Returns how many abilities carry the shared indicator.
        /// </summary>
        internal static int Apply(BlueprintAbilityResource grit,
            GritUiRecipient[] recipients)
        {
            if (grit == null) throw new ArgumentNullException("grit");
            if (recipients == null || recipients.Length == 0)
                throw new InvalidOperationException(
                    "The shared grit UI contract has no recipients.");
            var seen = new List<BlueprintAbility>();
            var unbound = new List<BlueprintAbility>();
            foreach (GritUiRecipient recipient in recipients)
            {
                if (recipient == null)
                    throw new InvalidOperationException(
                        "The shared grit UI contract lists a null recipient.");
                BlueprintAbility ability = recipient.Ability;
                if (ability == null)
                    throw new InvalidOperationException("Grit UI recipient " +
                        recipient.Deed + " is missing: no ability blueprint was " +
                        "constructed for it.");
                if (seen.Any(value => ReferenceEquals(value, ability)))
                    throw Violation(recipient, "it is listed more than once");
                seen.Add(ability);
                AbilityResourceLogic[] existing = GritLogic(ability, grit);
                if (recipient.Contract == GritUiContract.OwnResourceLogic)
                {
                    if (existing.Length != 1 || IsSharedIndicator(existing[0], grit))
                        throw Violation(recipient, "it must carry exactly one grit " +
                            "resource component of its own; found " + Describe(existing));
                }
                else if (existing.Length == 0) unbound.Add(ability);
                else if (existing.Length != 1 || !IsSharedIndicator(existing[0], grit))
                    throw Violation(recipient, "it already carries grit resource " +
                        "logic other than one shared indicator (" + Describe(existing) +
                        "); adding the indicator would count or charge grit twice");
            }

            foreach (BlueprintAbility ability in unbound)
                ability.ComponentsArray = (ability.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).Concat(
                        new BlueprintComponent[] { CreateIndicator(grit) }).ToArray();

            int shared = 0;
            foreach (GritUiRecipient recipient in recipients)
            {
                if (recipient.Contract != GritUiContract.SharedIndicator) continue;
                AbilityResourceLogic[] bound = GritLogic(recipient.Ability, grit);
                if (bound.Length != 1 || !IsSharedIndicator(bound[0], grit))
                    throw Violation(recipient, "the shared indicator did not verify " +
                        "after integration; found " + Describe(bound));
                shared++;
            }
            return shared;
        }

        private static GritAbilityResourceUiLogic CreateIndicator(
            BlueprintAbilityResource grit)
        {
            var ui = UnityEngine.ScriptableObject.CreateInstance<
                GritAbilityResourceUiLogic>();
            ui.name = SharedComponentName;
            ui.RequiredResource = grit;
            // Kingmaker's action bar reports a resource count only when this
            // flag is true. The derived component keeps native availability
            // and count behavior while its virtual Spend remains a no-op;
            // request-local deed transactions stay the atomic authority.
            ui.IsSpendResource = true;
            ui.CostIsCustom = false;
            ui.Amount = 1;
            return ui;
        }

        private static bool IsSharedIndicator(AbilityResourceLogic logic,
            BlueprintAbilityResource grit)
        {
            return logic is GritAbilityResourceUiLogic &&
                logic.name == SharedComponentName &&
                ReferenceEquals(logic.RequiredResource, grit) &&
                logic.IsSpendResource && !logic.CostIsCustom && logic.Amount == 1;
        }

        private static AbilityResourceLogic[] GritLogic(BlueprintAbility ability,
            BlueprintAbilityResource grit)
        {
            return (ability.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AbilityResourceLogic>().Where(value =>
                    ReferenceEquals(value.RequiredResource, grit)).ToArray();
        }

        private static GritUiRecipient Shared(string deed, BlueprintAbility ability)
        {
            return new GritUiRecipient(deed, ability, GritUiContract.SharedIndicator);
        }

        private static GritUiRecipient Own(string deed, BlueprintAbility ability)
        {
            return new GritUiRecipient(deed, ability, GritUiContract.OwnResourceLogic);
        }

        private static string Describe(AbilityResourceLogic[] logic)
        {
            return logic.Length == 0 ? "none" : string.Join(", ", logic.Select(value =>
                value.GetType().Name + " '" + value.name + "'").ToArray());
        }

        private static InvalidOperationException Missing(string what)
        {
            return new InvalidOperationException("Grit UI recipients are missing: " +
                what + " was not registered before the shared grit UI pass.");
        }

        // Internal identity only: the blueprint's object name and asset GUID. A
        // displayed name or description can itself be what is broken.
        private static InvalidOperationException Violation(GritUiRecipient recipient,
            string reason)
        {
            return new InvalidOperationException("Grit UI recipient " + recipient.Deed +
                " (" + recipient.Ability.name + ", " + recipient.Ability.AssetGuid +
                ") violates the shared grit UI contract: " + reason + ".");
        }
    }

    internal sealed class GritAbilityResourceUiLogic : AbilityResourceLogic
    {
        public override void Spend(Kingmaker.UnitLogic.Abilities.AbilityData ability)
        {
            // The deed's production transaction spends after all deed-specific
            // gates pass. Spending here would double-charge or charge reactions
            // that never trigger.
        }
    }
}
