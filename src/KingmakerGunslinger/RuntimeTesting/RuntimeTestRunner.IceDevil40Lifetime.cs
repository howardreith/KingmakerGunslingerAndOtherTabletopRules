using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewIceDevilLifetime40(ExpandedSummoningCorrectionFixture fixture,
            Kingmaker.EntitySystem.Entities.UnitEntityData owner,
            Kingmaker.EntitySystem.Entities.UnitEntityData sibling, Mesh cache, Texture2D albedo,
            InfernalEvents34 trace, JObject receipt)
        {
            var party = Game.Instance.Player.Inventory.Items.ToArray();
            var partyCounts = party.Select(x => x.Count).ToArray();
            var items = owner.Descriptor.Inventory.Items.Concat(new[] { owner.Body.PrimaryHand.MaybeItem })
                .Where(x => x != null).Distinct().ToArray();
            var siblingItems = sibling.Descriptor.Inventory.Items.ToArray();
            var siblingCounts = siblingItems.Select(x => x.Count).ToArray();
            var resources = ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(owner.View);
            var siblingResources = ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(sibling.View);
            var cells = new JArray(); receipt["nativeLifetime"] = cells;
            Action<string, bool, JObject, string> check = (id, pass, row, why) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint40-" + id, pass, row, why);
                if (!pass) throw new InvalidOperationException("Sprint40 native lifetime: " + id);
            };
            check("lifetime-private-pair", items.Length > 0 && resources.Length > 0 && siblingResources.Length > 0 &&
                !resources.Intersect(siblingResources).Any() && items.All(x =>
                    ReferenceEquals(x.Owner, owner.Descriptor) && !x.IsLootable && x.IsNonRemovable),
                new JObject { ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId,
                    ["items"] = items.Length, ["privateResources"] = resources.Length },
                "Live owners have nonempty exact owned gear and independent private resources before native retirement.");
            Game.Instance.IsPaused = true;
            int first = trace.Rows.Count;
            var costs = new JArray();
            using (var economy = new FirearmReviewCosts(owner, costs))
            {
                var pending = new UnitAttack(fixture.Hostile) { IsSingleAttack = true };
                owner.Commands.Run(pending);
                bool queued = owner.Commands.Raw.Contains(pending) && !pending.IsStarted && !pending.IsFinished;
                // Existing scheduler helper supplies an expired finite marker
                // precondition. The native summon-pool consumer does the work;
                // no fixture detach/dispose or post-expiry interrupt precedes proof.
                var expiry = ExpireSprint16OwnedBuff(owner,
                    owner.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
                DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                while (DateTime.UtcNow < deadline && (!Sprints3031SourceAbsent(owner) || resources.Any(x => x != null)))
                { Game.Instance.IsPaused = false; yield return 0; }
                var live = Game.Instance.State.Units.All.Where(x => x != null && !x.Destroyed && x.IsInState).ToArray();
                var gear = new JArray(items.Select(x => OutsiderEscapeItemState(x, owner, live)));
                bool sourceAbsent = Sprints3031SourceAbsent(owner);
                int pendingCosts = economy.Count(pending);
                bool resourcesAbsent = resources.All(x => x == null);
                bool gearSafe = gear.All(x => (bool)x["safe"]);
                bool siblingResourcesAlive = siblingResources.All(x => x != null);
                bool siblingInventoryExact = Sprint28InventoryExact(
                    sibling.Descriptor.Inventory, siblingItems, siblingCounts);
                bool partyInventoryExact = Sprint28PartyInventoryExact(party, partyCounts);
                bool ownerEventsAbsent = !trace.Rows.Skip(first).Any(x =>
                    (string)x["owner"] == owner.UniqueId &&
                    ((string)x["event"] == "attack" || (string)x["event"] == "save" ||
                     (string)x["event"] == "damage"));
                check("native-expiry-gear-and-resources", queued && sourceAbsent &&
                    pending.IsFinished && pendingCosts == 0 && resourcesAbsent &&
                    gearSafe && siblingResourcesAlive && siblingInventoryExact &&
                    partyInventoryExact && ownerEventsAbsent,
                    new JObject { ["expiry"] = expiry, ["queued"] = queued,
                        ["command"] = Sprint27AiCommand(pending), ["costs"] = costs,
                        ["sourceAbsent"] = sourceAbsent,
                        ["pendingFinished"] = pending.IsFinished,
                        ["pendingCostCount"] = pendingCosts,
                        ["privateResourcesAbsent"] = resourcesAbsent,
                        ["gearSafe"] = gearSafe,
                        ["siblingResourcesAlive"] = siblingResourcesAlive,
                        ["siblingInventoryExact"] = siblingInventoryExact,
                        ["partyInventoryExact"] = partyInventoryExact,
                        ["ownerAttackSaveDamageEventsAbsent"] = ownerEventsAbsent,
                        ["gear"] = gear, ["fixtureDisposalsBeforeAssertion"] = 0,
                        ["fixtureInterruptsAfterExpiry"] = 0 },
                    "Native expiry cancels the real queued command without spending, retires exact gear/private resources without escaped loot, and preserves the other owner and party inventory.");
            }
            var deathItems = siblingItems.Concat(new[] { sibling.Body.PrimaryHand.MaybeItem })
                .Where(x => x != null).Distinct().ToArray();
            var good = new PhysicalDamage(new DiceFormula(0, DiceType.Zero), PhysicalDamageForm.Slashing);
            good.AddBonus(1000); good.AddAlignment(DamageAlignment.Good);
            var fatal = Rulebook.Trigger(new RuleDealDamage(fixture.Hostile, sibling, new DamageBundle(good)));
            bool died = sibling.Descriptor.State.IsDead;
            string capture = sibling.View == null ? "native immediate view retirement" :
                WriteExpandedSummoningPartyCameraCapture(sibling, _request.EvidenceDirectory,
                    "s40-ice-devil-native-death.png", KingmakerGunslinger.Summoning.TheropodVisualPolicy.BodyRendererName);
            DateTime deathDeadline = DateTime.UtcNow.AddSeconds(20);
            while (DateTime.UtcNow < deathDeadline && (!Sprints3031SourceAbsent(sibling) || siblingResources.Any(x => x != null)))
            { Game.Instance.IsPaused = false; yield return 0; }
            var remaining = Game.Instance.State.Units.All.Where(x => x != null && !x.Destroyed && x.IsInState).ToArray();
            var deathGear = new JArray(deathItems.Select(x => OutsiderEscapeItemState(x, sibling, remaining)));
            check("native-death-gear-and-resources", died && fatal.Damage > 0 &&
                Sprints3031SourceAbsent(sibling) && siblingResources.All(x => x == null) &&
                deathGear.Count > 0 && deathGear.All(x => (bool)x["safe"]) &&
                cache != null && albedo != null && Sprint28PartyInventoryExact(party, partyCounts),
                new JObject { ["nativeFatalGoodDamage"] = fatal.Damage, ["nativeDead"] = died,
                    ["capture"] = capture, ["gear"] = deathGear,
                    ["fixtureDisposalsBeforeAssertion"] = 0, ["nativeDamageConsumerNotAttackProof"] = true },
                "Actual good-damage death retires the original body and its private gear/resources without fixture disposal or escaped stock; borrowed/cache assets remain alive.");
        }
    }
}
