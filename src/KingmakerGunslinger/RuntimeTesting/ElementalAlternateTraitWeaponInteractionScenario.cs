using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Reuse the already-qualified native Scorching Weapons command driver.
    // Actual trait interactions stay in this feature-specific scenario file.
    internal static partial class ElementalIfritFeatScenario
    {
        internal static void ExerciseBrazenNonstacking(UnitEntityData attacker,
            UnitEntityData target, BlueprintFeature brazen,
            ICollection<ItemEntityWeapon> items,
            ICollection<RuntimeTestAssertion> assertions, JArray rows)
        {
            BlueprintFeature scorching = BlueprintBootstrap.ElementalFeats
                .RequireFeature(ElementalFeatId.ScorchingWeapons);
            BlueprintFeature inner = BlueprintBootstrap.ElementalFeats
                .RequireFeature(ElementalFeatId.InnerFlame);
            BlueprintAbility ability = GrantedAbility(scorching);
            var sword = new ItemEntityWeapon(BlueprintLibraryLookup
                .RequireExact<BlueprintItemWeapon>(BlueprintBootstrap.Library,
                    ShortSwordGuid, "Brazen/Scorching native shortsword"));
            items.Add(sword);
            attacker.Body.PrimaryHand.RemoveItem(false);
            attacker.Body.PrimaryHand.InsertItem(sword);
            EnsureFact(attacker.Descriptor, scorching);
            ElementalScorchingWeaponsAbilityLogic logic = ability
                .GetComponent<ElementalScorchingWeaponsAbilityLogic>();
            try
            {
                foreach (bool traitFirst in new[] { true, false })
                {
                    if (CountEnchantment(sword, logic.WeaponEnchantment) != 0 ||
                        ElementalFeatTransientRuntime.IsScorchingWeaponsActive(
                            attacker.Descriptor, sword))
                        throw new InvalidOperationException(
                            "The preceding request-local Scorching activation was not cleaned up.");
                    if (traitFirst) EnsureFact(attacker.Descriptor, brazen);
                    ExecuteEvidence command = Execute(attacker, ability);
                    if (!traitFirst) EnsureFact(attacker.Descriptor, brazen);
                    foreach (bool improved in new[] { false, true })
                    {
                        if (improved) EnsureFact(attacker.Descriptor, inner);
                        EnergyDamage[] packets = FirePackets(AutoHit(attacker,
                            target, sword));
                        bool pass = command.CommandResult == "Success" &&
                            command.ProcessPresent && command.ProcessEnded &&
                            CountEnchantment(sword, logic.WeaponEnchantment) == 1 &&
                            packets.Length == 1 && packets[0].PreRolledValue == 1;
                        string label = "brazen-scorching-order-" + traitFirst +
                            "-inner-" + improved;
                        string observed = "command=" + command.CommandResult +
                            ";processPresent=" + command.ProcessPresent +
                            ";processEnded=" + command.ProcessEnded +
                            ";enchantments=" + CountEnchantment(sword,
                                logic.WeaponEnchantment) +
                            ";firePackets=" + packets.Length +
                            ";firstPreRolled=" + (packets.Length == 0 ? "<none>" :
                                packets[0].PreRolledValue.ToString());
                        assertions.Add(new RuntimeTestAssertion
                        {
                            Name = label, Expected = "only Brazen Flame +1 fire",
                            Observed = observed,
                            Status = pass ? RuntimeTestStatuses.Pass :
                                RuntimeTestStatuses.Fail,
                            Evidence = "actual Scorching command and native weapon damage"
                        });
                        rows.Add(new JObject { { "name", label }, { "pass", pass },
                            { "observed", observed } });
                    }
                    attacker.Descriptor.RemoveFact(inner);
                    attacker.Descriptor.RemoveFact(brazen);
                    // Removing a buff alone intentionally retains unexpired
                    // feat state for save hydration. End the exact owned test
                    // activation, including its item snapshot, before replay.
                    ElementalFeatTransientRuntime.RemoveScorchingWeapons(
                        attacker.Descriptor);
                }
            }
            finally
            {
                attacker.Descriptor.RemoveFact(inner);
                attacker.Descriptor.RemoveFact(brazen);
                ElementalFeatTransientRuntime.RemoveScorchingWeapons(
                    attacker.Descriptor);
                attacker.Descriptor.RemoveFact(scorching);
            }
        }
    }
}
