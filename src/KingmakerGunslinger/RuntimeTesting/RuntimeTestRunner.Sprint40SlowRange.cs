using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // The sole S39 gameplay carry-forward: no nimbus/spell campaign.
        private IEnumerable<int> ReviewNalfeshneeSlowRange40(ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>(); var trace = new InfernalEvents34();
            var cells = new JArray(); var receipt = new JObject {
                ["cells"] = cells, ["events"] = trace.Rows,
                ["scope"] = "Nalfeshnee-owned Slow close-range correction only" };
            Action<string, bool, JObject, string> check = (id, pass, row, why) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint40-" + id, pass, row, why);
                if (!pass) throw new InvalidOperationException("Nalfeshnee Slow boundary: " + id);
            };
            var recipient = fixture.Hostile;
            try
            {
                var owner = CastExpandedSummoningOwnTier(fixture, NalfeshneeVisualPolicy39.Nalfeshnee);
                owned.Add(owner); VrockQuiesce37(owner);
                foreach (int frame in Outsider30NativeAppearance(owner, owner.Blueprint.name)) yield return frame;
                var spell = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningSpecialBuilder.Nalfeshnee39Prefix + "Slow"];
                var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(spell));
                JObject hostility = Nalfeshnee39EstablishHostility(fixture, owner, recipient);
                check("nalfeshnee-slow-hostility", owner.IsEnemy(recipient) && recipient.IsEnemy(owner) &&
                    !ReferenceEquals(owner.Group, recipient.Group), hostility,
                    "Exact disposable recipient, supported native group setter and mutual hostility.");
                var probe = new UnitUseAbility(data, new TargetWrapper(recipient)); probe.Init(owner);
                float nativeRadius = probe.ApproachRadius;
                float printed = new Feet(NalfeshneeRules39Policy.SlowRangeFeet(
                    NalfeshneeRules39Policy.SpellLikeCasterLevel)).Meters;
                float footprint = owner.Corpulence + recipient.Corpulence;
                check("nalfeshnee-slow-native-range", spell.CustomRange.Value == 55 &&
                    Math.Abs(nativeRadius - printed) <= footprint + .1f,
                    new JObject { ["printedFeet"] = 55, ["nativeApproachRadius"] = nativeRadius,
                        ["ownerCorpulence"] = owner.Corpulence, ["targetCorpulence"] = recipient.Corpulence,
                        ["nativeRadiusMinusPrinted"] = nativeRadius - printed },
                    "Actual command approach radius is the printed CL12 close range with the engine's real footprint allowance, far below220 feet.");
                Vector3 outside; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, recipient,
                    nativeRadius + 3f, out outside, out lane, out direction,
                    originSearchRadiusMeters: 30);
                Vector3 axis = (outside - origin).normalized;
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(recipient, origin + axis * (nativeRadius - 1f));
                var insideProbe = new UnitUseAbility(data, new TargetWrapper(recipient)); insideProbe.Init(owner);
                bool insideLegal = data.IsAvailable && data.CanTarget(new TargetWrapper(recipient)) &&
                    insideProbe.IsUnitEnoughClose;
                check("nalfeshnee-slow-inside-targeting", insideLegal,
                    new JObject { ["lane"] = lane, ["centerDistance"] = Vector3.Distance(owner.Position, recipient.Position),
                        ["nativeEnoughClose"] = insideProbe.IsUnitEnoughClose,
                        ["nativeApproachRadius"] = insideProbe.ApproachRadius },
                    "Real native targeting accepts the recipient just inside its measured command boundary.");
                int start = trace.Rows.Count;
                var inside = new UnitUseAbility(data, new TargetWrapper(recipient)); owner.Commands.Run(inside);
                foreach (int frame in WaitInfernalCommand34(inside, () => true, 25)) yield return frame;
                var insideActs = trace.Rows.Skip(start).Where(x => (string)x["event"] == "command-act" &&
                    (string)x["owner"] == owner.UniqueId && (string)x["ability"] == spell.AssetGuid).ToArray();
                check("nalfeshnee-slow-inside-cast", inside.Result == UnitCommand.ResultType.Success &&
                    insideActs.Length == 1 && (bool)insideActs[0]["nativeEnoughClose"],
                    new JObject { ["command"] = Sprint27AiCommand(inside), ["acts"] = new JArray(insideActs) },
                    "A real owned Slow command completes inside the correct close range.");
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(recipient, outside);
                var outsideProbe = new UnitUseAbility(data, new TargetWrapper(recipient)); outsideProbe.Init(owner);
                check("nalfeshnee-slow-outside-needs-approach", !outsideProbe.IsUnitEnoughClose &&
                    data.CanTarget(new TargetWrapper(recipient)),
                    new JObject { ["centerDistance"] = Vector3.Distance(owner.Position, recipient.Position),
                        ["nativeEnoughClose"] = outsideProbe.IsUnitEnoughClose,
                        ["nativeApproachRadius"] = outsideProbe.ApproachRadius },
                    "The native command refuses immediate execution outside close range while retaining legal target identity.");
                Vector3 before = owner.Position; start = trace.Rows.Count;
                var approach = new UnitUseAbility(data, new TargetWrapper(recipient)); owner.Commands.Run(approach);
                foreach (int frame in WaitInfernalCommand34(approach, () => true, 30)) yield return frame;
                var acts = trace.Rows.Skip(start).Where(x => (string)x["event"] == "command-act" &&
                    (string)x["owner"] == owner.UniqueId && (string)x["ability"] == spell.AssetGuid).ToArray();
                check("nalfeshnee-slow-outside-native-approach", approach.Result == UnitCommand.ResultType.Success &&
                    Vector3.Distance(before, owner.Position) > 1f && acts.Length == 1 &&
                    (bool)acts[0]["nativeEnoughClose"],
                    new JObject { ["command"] = Sprint27AiCommand(approach),
                        ["travel"] = Vector3.Distance(before, owner.Position), ["acts"] = new JArray(acts) },
                    "Outside the native boundary, the actual Nalfeshnee walks into range before casting; no party actor is repositioned or range guard waived.");
            }
            finally
            {
                trace.Dispose(); DisposeExpandedSummoningUnits(fixture.Created, owned);
                ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                    "sprint40-nalfeshnee-slow-range.json"), receipt.ToString(Formatting.Indented));
            }
        }
    }
}
