using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // An ordinary native full attack by the Bone Devil on a creature
        // standing adjacent to it, inside the installed ProperFlanking2 dead
        // zone it computes for reach weapons ((size reach + 1) ft x 0.5 beyond
        // both bodies). Natural reach threatens adjacent squares, so the whole
        // natural routine (bite, two claws, sting) must resolve where the
        // target stands and reach its native terminal boundary; the Bone Devil
        // must not back off. The verdict is InfernalAdjacentRoutineVerdict35
        // over this record. The observer forces no roll, extends no deadline
        // and writes no position while the command runs.
        private IEnumerable<int> InfernalAdjacentNatural35(UnitEntityData bone, UnitEntityData target,
            InfernalEvents34 trace, Action<string, bool, JObject, string> check, JObject row)
        {
            row["owner"] = bone.UniqueId; row["target"] = target.UniqueId;
            var profile = InfernalMartialProfile35.For(bone);
            row["profile"] = new JObject { ["bite"] = profile?.Bite?.AssetGuid, ["claw"] = profile?.Claw?.AssetGuid,
                ["sting"] = profile?.Sting?.AssetGuid };
            bone.Commands.InterruptAll(true); bone.Commands.RemoveFinishedAndUpdateQueue();
            InfernalWallPrepareActor35(target);
            for (int frame = 0; frame < 6; frame++) { Game.Instance.IsPaused = false; yield return 0; }
            Vector3 away = target.Position - bone.Position; away.y = 0f;
            if (away.sqrMagnitude < .01f) away = bone.OrientationDirection;
            away.y = 0f; away.Normalize();
            float bodies = bone.View.Corpulence + target.View.Corpulence;
            var spot = AstarPath.active.GetNearest(bone.Position + away * (bodies + .15f));
            row["placementNodeFound"] = spot.node != null;
            if (spot.node != null) PlaceExpandedSummoningUnit(target, spot.clampedPosition);
            InfernalWallPrepareActor35(target);
            // Settle before measuring: positions are read only after six
            // unpaused frames, and again just before the command starts.
            for (int frame = 0; frame < 6; frame++) { Game.Instance.IsPaused = false; yield return 0; }
            row["properFlanking2"] = InfernalProperFlanking2State35();
            int sizeReach = bone.Descriptor.State.Size.GetModifiers().Reach;
            float deadRange = (sizeReach + 1) * .3048f * .5f;
            float centreDistance = (bone.Position - target.Position).magnitude;
            float deadZoneOuter = bone.View.Corpulence + deadRange + target.View.Corpulence;
            var weapons = new[] { profile?.Bite, profile?.Claw, profile?.Sting }.Where(value => value != null).ToArray();
            row["prerequisites"] = InfernalAdjacentActors35(bone, target);
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
            row["threatens"] = threatens;
            Vector3 boneStart = bone.Position, targetStart = target.Position;
            var command = new UnitAttack(target) { IsSingleAttack = false, ForceFullAttack = true };
            int reference = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(command);
            row["commandReference"] = reference;
            row["commandIssuedGameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds;
            bone.Commands.Run(command);
            float boneTravel = 0f, targetTravel = 0f;
            // The wait bounds observation only; it never shortens or extends
            // the native routine. An unfinished command is recorded as such.
            DateTime deadline = DateTime.UtcNow.AddSeconds(20);
            int settled = 0;
            while (DateTime.UtcNow < deadline && settled < 30)
            {
                Game.Instance.IsPaused = false; yield return 0;
                boneTravel = Math.Max(boneTravel, Vector3.Distance(boneStart, bone.Position));
                targetTravel = Math.Max(targetTravel, Vector3.Distance(targetStart, target.Position));
                settled = command.IsFinished ? settled + 1 : 0;
            }
            var commandRows = trace.Rows.OfType<JObject>().Where(value => (string)value["owner"] == bone.UniqueId &&
                (int?)value["commandReference"] == reference).ToArray();
            var ends = commandRows.Where(value => (string)value["event"] == "command-end").ToArray();
            JObject planRow = ends.LastOrDefault() ?? commandRows.LastOrDefault(value => value["plan"] != null);
            var attackRows = trace.Rows.OfType<JObject>().Where(value => (string)value["event"] == "attack" &&
                (string)value["owner"] == bone.UniqueId && (int?)value["attackCommand"] == reference).ToArray();
            string bite = profile?.Bite?.AssetGuid, claw = profile?.Claw?.AssetGuid, sting = profile?.Sting?.AssetGuid;
            Func<string, string> role = guid => guid == null ? "none" : guid == bite ? "bite" : guid == claw ? "claw" :
                guid == sting ? "sting" : "other";
            row["command"] = Sprint27AiCommand(command);
            row["commandFinished"] = command.IsFinished;
            row["commandResult"] = command.Result.ToString();
            row["nativeAllAttacks"] = command.AllAttacks.Count;
            row["nativeExhaustedInterrupt"] = Sprint40NativeExhaustedAttack(command, ends, 4);
            row["plan"] = planRow?["plan"] is JArray ? new JArray(((JArray)planRow["plan"]).Select(slot => (string)slot["weapon"])) : new JArray();
            row["planRoles"] = new JArray(((JArray)row["plan"]).Select(slot => role((string)slot)));
            row["commandEnds"] = new JArray(ends.Select(value => new JObject { ["owner"] = value["owner"],
                ["target"] = value["target"], ["commandReference"] = value["commandReference"],
                ["gameSeconds"] = value["gameSeconds"], ["attackIndex"] = value["attackIndex"],
                ["plannedAttackPresent"] = value["plannedAttackPresent"],
                ["interruptionCaller"] = value["interruptionCaller"] }));
            row["commandEvents"] = new JArray(commandRows.Select(value => new JObject { ["event"] = value["event"],
                ["gameSeconds"] = value["gameSeconds"], ["attackIndex"] = value["attackIndex"],
                ["plannedAttackPresent"] = value["plannedAttackPresent"] }));
            row["attacks"] = new JArray(attackRows.Select(value => new JObject { ["owner"] = value["owner"],
                ["target"] = value["target"], ["attackCommand"] = value["attackCommand"],
                ["gameSeconds"] = value["gameSeconds"], ["weapon"] = value["weapon"],
                ["role"] = role((string)value["weapon"]), ["hit"] = value["hit"], ["hpDamage"] = value["hpDamage"],
                ["rangeFeet"] = value["rangeFeet"] }));
            row["attackCount"] = attackRows.Length;
            row["hits"] = attackRows.Count(value => (bool?)value["hit"] == true);
            row["misses"] = attackRows.Count(value => (bool?)value["hit"] != true);
            row["attacksByRole"] = new JObject(attackRows.GroupBy(value => role((string)value["weapon"]))
                .Select(group => new JProperty(group.Key, group.Count())));
            row["ownerTravel"] = boneTravel;
            row["targetTravel"] = targetTravel;
            Buff lifetime = bone.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            row["ownerLifetimePresentAfter"] = lifetime != null;
            row["ownerAliveAfter"] = !bone.Destroyed && bone.IsInGame && !bone.Descriptor.State.IsDead;
            row["after"] = InfernalAdjacentActors35(bone, target);
            row["forcedRolls"] = 0;
            row["observerPositionWritesDuringCommand"] = 0;
            row["previousAcceptance"] = "s3a/s3b accepted IsFinished plus one natural hit; superseded by InfernalAdjacentRoutineVerdict35.";
            string failure = InfernalAdjacentRoutineVerdict35.Failure(row);
            row["verdictFailure"] = failure;
            check("bone-adjacent-natural-attack", failure == null, row,
                "With ProperFlanking2 loaded, active, patched and its reach_weapons_dead_zone option enabled, an ordinary native full attack by the Bone Devil on an adjacent hostile creature inside that dead zone plans bite, two claws and sting, resolves all four against that target under the measured command, reaches its native terminal boundary (Success, or the exhausted-routine Interrupt from UnitAttack.OnTick at the final index), keeps the Bone Devil's summon lifetime, and neither unit moves.");
        }

        // Identity, hostility, lifetime, health and position of both actors.
        private static JObject InfernalAdjacentActors35(UnitEntityData bone, UnitEntityData target)
        {
            Func<UnitEntityData, JObject> actor = unit => {
                Buff lifetime = unit.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
                double now = Game.Instance.TimeController.GameTime.TotalSeconds;
                return new JObject { ["unit"] = unit.UniqueId, ["blueprint"] = unit.Blueprint.AssetGuid,
                    ["name"] = unit.Blueprint.name, ["faction"] = unit.Descriptor.Faction?.name,
                    ["summoner"] = unit.Get<Kingmaker.UnitLogic.Parts.UnitPartSummonedMonster>()?.Summoner?.UniqueId,
                    ["summonLifetimePresent"] = lifetime != null,
                    ["summonLifetimeRemainingSeconds"] = lifetime == null ? (double?)null : lifetime.EndTime.TotalSeconds - now,
                    ["hitPointsLeft"] = unit.HPLeft, ["maxHitPoints"] = unit.Stats.HitPoints.ModifiedValue,
                    ["dead"] = unit.Descriptor.State.IsDead, ["inGame"] = unit.IsInGame,
                    ["canAct"] = unit.Descriptor.State.CanAct, ["inFog"] = unit.IsInFogOfWar,
                    ["position"] = SurveyVector(unit.Position), ["corpulence"] = unit.View.Corpulence,
                    ["size"] = unit.Descriptor.State.Size.ToString(),
                    ["buffs"] = new JArray(unit.Buffs.Enumerable.Select(buff => buff.Blueprint.name)) };
            };
            float centre = (bone.Position - target.Position).magnitude;
            return new JObject { ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds,
                ["attacker"] = actor(bone), ["target"] = actor(target),
                ["attackerIsEnemyOfTarget"] = bone.IsEnemy(target), ["targetIsEnemyOfAttacker"] = target.IsEnemy(bone),
                ["centreDistance"] = centre, ["edgeGap"] = centre - bone.View.Corpulence - target.View.Corpulence };
        }

        // The installed ProperFlanking2 configuration, read only. Assembly
        // presence alone is not the dead zone: the UMM entry must be loaded and
        // active from that assembly, its live reach_weapons_dead_zone option
        // true, and its postfixes installed on UnitCommand.GetTargetPoint and
        // UnitEngagementExtension.IsReach.
        private JObject InfernalProperFlanking2State35()
        {
            var state = new JObject();
            Assembly[] matches = AppDomain.CurrentDomain.GetAssemblies()
                .Where(value => value.GetName().Name == "ProperFlanking20").ToArray();
            Assembly assembly = matches.Length == 1 ? matches[0] : null;
            state["assemblyCount"] = matches.Length;
            state["assemblyLoaded"] = assembly != null;
            if (assembly != null)
            {
                state["assemblyLocation"] = assembly.Location;
                state["assemblyMvid"] = assembly.ManifestModule.ModuleVersionId.ToString();
                try { state["assemblySha256"] = InfernalSha256File35(assembly.Location); }
                catch (Exception error) { state["assemblySha256Error"] = error.GetType().Name; }
            }
            bool entryActive = false;
            try
            {
                FieldInfo field = typeof(UnityModManager).GetField("modEntries",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var entry = (field?.GetValue(null) as IEnumerable)?.Cast<object>().OfType<UnityModManager.ModEntry>()
                    .FirstOrDefault(value => value.Info != null && value.Info.Id == "ProperFlanking2");
                state["modEntryFound"] = entry != null;
                if (entry != null)
                {
                    state["modVersion"] = entry.Info.Version;
                    state["modLoaded"] = entry.Loaded; state["modEntryActive"] = entry.Active;
                    state["modHasAssembly"] = entry.HasAssembly; state["modErrorOnLoading"] = entry.ErrorOnLoading;
                    state["modEntryAssemblyIsLoadedAssembly"] = assembly != null && ReferenceEquals(entry.Assembly, assembly);
                    entryActive = entry.Loaded && entry.Active && entry.HasAssembly && !entry.ErrorOnLoading &&
                        assembly != null && ReferenceEquals(entry.Assembly, assembly);
                    string file = Path.Combine(entry.Path ?? "", "settings.json");
                    state["settingsFile"] = file;
                    state["settingsFileExists"] = File.Exists(file);
                    if (File.Exists(file))
                    {
                        state["settingsFileSha256"] = InfernalSha256File35(file);
                        try { state["settingsFileReachWeaponsDeadZone"] = JObject.Parse(File.ReadAllText(file))["reach_weapons_dead_zone"]; }
                        catch (Exception error) { state["settingsFileParseError"] = error.GetType().Name; }
                    }
                }
            }
            catch (Exception error) { state["modEntryError"] = error.GetType().Name + ": " + error.Message; }
            state["modActive"] = entryActive;
            try
            {
                Type main = assembly?.GetType("ProperFlanking20.Main", false);
                object settings = main?.GetField("settings", BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic)?.GetValue(null);
                PropertyInfo option = settings?.GetType().GetProperty("reach_weapons_dead_zone",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                object value = option == null ? null : option.GetValue(settings, null);
                state["settingsType"] = settings?.GetType().FullName;
                state["reachWeaponsDeadZone"] = value is bool ? new JValue((bool)value) : JValue.CreateNull();
            }
            catch (Exception error) { state["settingsError"] = error.GetType().Name + ": " + error.Message; }
            bool targetPoint = false, isReach = false;
            var postfixes = new JArray();
            try
            {
                foreach (MethodBase method in _context.Harmony.GetPatchedMethods().ToArray())
                {
                    Patches patches = _context.Harmony.GetPatchInfo(method);
                    if (patches == null) continue;
                    foreach (Patch patch in patches.Postfixes.Where(value => assembly != null &&
                            value.patch?.DeclaringType?.Assembly == assembly))
                    {
                        postfixes.Add(new JObject { ["target"] = method.DeclaringType?.FullName + "." + method.Name,
                            ["parameters"] = new JArray(method.GetParameters().Select(value => value.ParameterType.Name)),
                            ["owner"] = patch.owner, ["patch"] = patch.patch.DeclaringType.FullName });
                        if (method.DeclaringType == typeof(UnitCommand) && method.Name == "GetTargetPoint") targetPoint = true;
                        if (method.DeclaringType == typeof(UnitEngagementExtension) && method.Name == "IsReach") isReach = true;
                    }
                }
            }
            catch (Exception error) { state["patchError"] = error.GetType().Name + ": " + error.Message; }
            state["postfixes"] = postfixes;
            state["getTargetPointPatched"] = targetPoint;
            state["isReachPatched"] = isReach;
            return state;
        }

        private static string InfernalSha256File35(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
    }
}
