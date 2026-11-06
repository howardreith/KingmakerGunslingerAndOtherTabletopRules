using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Commands;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // An ordinary native full attack by the Bone Devil on a creature
        // standing adjacent to it, inside the installed ProperFlanking2 dead
        // zone it computes for reach weapons ((size reach + 1) ft x 0.5 beyond
        // both bodies). Natural reach threatens adjacent squares, so the
        // natural attacks must resolve where the target stands: the Bone
        // Devil must not back off (ProperFlanking2 moves the approach point of
        // a reach weapon away) and its natural attacks must land. The target
        // is the stilled hostile tail witness; the observer forces no roll
        // and writes no position while the command runs.
        private IEnumerable<int> InfernalAdjacentNatural35(UnitEntityData bone, UnitEntityData target,
            InfernalEvents34 trace, Action<string, bool, JObject, string> check)
        {
            var row = new JObject { ["owner"] = bone.UniqueId, ["target"] = target.UniqueId };
            var profile = InfernalMartialProfile35.For(bone);
            bone.Commands.InterruptAll(true); bone.Commands.RemoveFinishedAndUpdateQueue();
            InfernalWallPrepareActor35(target);
            for (int frame = 0; frame < 6; frame++) { Game.Instance.IsPaused = false; yield return 0; }
            Vector3 away = target.Position - bone.Position; away.y = 0f;
            if (away.sqrMagnitude < .01f) away = bone.OrientationDirection;
            away.y = 0f; away.Normalize();
            float bodies = bone.View.Corpulence + target.View.Corpulence;
            var spot = AstarPath.active.GetNearest(bone.Position + away * (bodies + .15f));
            if (spot.node != null) PlaceExpandedSummoningUnit(target, spot.clampedPosition);
            InfernalWallPrepareActor35(target);
            for (int frame = 0; frame < 6; frame++) { Game.Instance.IsPaused = false; yield return 0; }
            bool properFlanking2 = AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
                assembly.GetName().Name == "ProperFlanking20");
            int sizeReach = bone.Descriptor.State.Size.GetModifiers().Reach;
            float deadRange = (sizeReach + 1) * .3048f * .5f;
            float centreDistance = (bone.Position - target.Position).magnitude;
            float deadZoneOuter = bone.View.Corpulence + deadRange + target.View.Corpulence;
            var weapons = new[] { profile?.Bite, profile?.Claw, profile?.Sting }.Where(value => value != null).ToArray();
            row["properFlanking2Loaded"] = properFlanking2;
            row["sizeReachFeet"] = sizeReach;
            row["deadRangeMeters"] = deadRange;
            row["centreDistance"] = centreDistance;
            row["edgeGap"] = centreDistance - bodies;
            row["deadZoneOuter"] = deadZoneOuter;
            row["insideDeadZone"] = centreDistance < deadZoneOuter;
            row["reachRangeFeet"] = bone.Stats.GetStat(Kingmaker.EntitySystem.Stats.StatType.Reach).ModifiedValue;
            row["naturalWeapons"] = new JArray(weapons.Select(weapon => new JObject { ["weapon"] = weapon.AssetGuid,
                ["typeRangeFeet"] = weapon.Type.AttackRange.Value,
                ["unitWeaponRangeFeet"] = bone.Descriptor.GetWeaponRange(weapon).Value,
                ["properFlanking2ReachWeapon"] = InfernalRules35.ProperFlanking2TreatsAsReachWeapon(
                    weapon.IsMelee, (int)Math.Round((double)weapon.Type.AttackRange.Value)) }));
            bool threatens = false;
            try { threatens = bone.IsReach(target, bone.Body.PrimaryHand); }
            catch (Exception error) { row["isReachError"] = error.GetType().Name; }
            row["primaryHandThreatensTarget"] = threatens;
            Vector3 boneStart = bone.Position, targetStart = target.Position;
            var command = new UnitAttack(target) { IsSingleAttack = false, ForceFullAttack = true };
            int reference = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(command);
            bone.Commands.Run(command);
            float boneTravel = 0f, targetTravel = 0f;
            DateTime deadline = DateTime.UtcNow.AddSeconds(15);
            int settled = 0;
            while (DateTime.UtcNow < deadline && settled < 30)
            {
                Game.Instance.IsPaused = false; yield return 0;
                boneTravel = Math.Max(boneTravel, Vector3.Distance(boneStart, bone.Position));
                targetTravel = Math.Max(targetTravel, Vector3.Distance(targetStart, target.Position));
                settled = command.IsFinished ? settled + 1 : 0;
            }
            var guids = new HashSet<string>(weapons.Select(weapon => weapon.AssetGuid));
            var attackRows = trace.Rows.Where(value => (string)value["event"] == "attack" &&
                (string)value["owner"] == bone.UniqueId && (string)value["target"] == target.UniqueId &&
                (int?)value["attackCommand"] == reference).ToArray();
            row["command"] = Sprint27AiCommand(command);
            row["commandReference"] = reference;
            row["attackRows"] = new JArray(attackRows);
            row["naturalAttacks"] = attackRows.Count(value => guids.Contains((string)value["weapon"]));
            row["naturalHits"] = attackRows.Count(value => guids.Contains((string)value["weapon"]) && (bool?)value["hit"] == true);
            row["boneTravel"] = boneTravel;
            row["targetTravel"] = targetTravel;
            row["targetDeadAfter"] = target.Descriptor.State.IsDead;
            row["forcedRolls"] = 0;
            bool pass = properFlanking2 && centreDistance < deadZoneOuter && threatens &&
                command.IsFinished && attackRows.Length > 0 &&
                attackRows.All(value => guids.Contains((string)value["weapon"])) &&
                (int)row["naturalHits"] > 0 && boneTravel < .25f && targetTravel < .25f;
            check("bone-adjacent-natural-attack", pass, row,
                "With the installed ProperFlanking2 dead zone active, an ordinary native full attack by the Bone Devil on an adjacent creature (inside the dead zone ProperFlanking2 computes for reach weapons) resolves its natural attacks where the target stands: the Bone Devil threatens the target, does not back off, and at least one natural attack hits.");
        }
    }
}
