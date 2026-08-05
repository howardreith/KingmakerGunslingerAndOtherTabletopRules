using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using UnityEngine;

namespace KingmakerGunslinger.Diagnostics
{
    internal static class DodgeBuffLifecycleForensics
    {
        internal const string MarkerName = "dodge-forensics.enabled";
        internal const string DodgeGuid = "bbd7d42117cc4c23b3e22af3a71621d9";
        internal const string DodgeName = "KMG_GunslingerDodge_ArmorClass_Buff";
        private static readonly JsonSerializerSettings JsonLineSettings =
            new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                NullValueHandling = NullValueHandling.Include
            };
        private static readonly object Gate = new object();
        private static StreamWriter _writer;
        private static long _sequence;
        private static int _gameThreadId;
        private static bool _enabled;
        private static bool _writeFailureLogged;
        private static ModLogger _logger;
        private static readonly PropertyInfo NextTick = typeof(Buff).GetProperty(
            "NextTickTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo NextEvent = typeof(BuffCollection).GetField(
            "m_NextEvent", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static bool Enabled { get { return _enabled; } }

        internal static void Initialize(ModContext context)
        {
            if (context == null) return;
            string marker = Path.Combine(context.ModEntry.Path, MarkerName);
            if (!File.Exists(marker)) return;
            try
            {
                _logger = context.Logger;
                _writeFailureLogged = false;
                _gameThreadId = Thread.CurrentThread.ManagedThreadId;
                string directory = Path.Combine(Application.persistentDataPath,
                    "KingmakerGunslinger", "Diagnostics");
                Directory.CreateDirectory(directory);
                string path = Path.Combine(directory, "dodge-buff-lifecycle-" +
                    DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffffffZ",
                        CultureInfo.InvariantCulture) + ".jsonl");
                _writer = new StreamWriter(new FileStream(path, FileMode.CreateNew,
                    FileAccess.Write, FileShare.Read), new System.Text.UTF8Encoding(false));
                _writer.AutoFlush = true;
                _enabled = true;
                context.Logger.Info("diagnostics", "dodge-forensics.enabled",
                    "Manual Dodge forensics is enabled; JSONL=" + path);
                DodgeBuffLifecycleForensicsSampler.Attach(context);
                Record("forensics-enabled", null, null, null, null, null);
            }
            catch (Exception exception)
            {
                _enabled = false;
                if (_writer != null) { _writer.Dispose(); _writer = null; }
                context.Logger.Failure("diagnostics", "dodge-forensics.failed",
                    "Manual Dodge forensics could not start.", exception);
            }
        }

        internal static bool IsExact(BlueprintBuff blueprint)
        {
            return blueprint != null && blueprint.AssetGuid == DodgeGuid &&
                string.Equals(blueprint.name, DodgeName, StringComparison.Ordinal);
        }

        internal static bool Relevant(Buff buff)
        {
            return _enabled && buff != null &&
                IsExact(buff.Blueprint);
        }

        internal static bool Relevant(BuffCollection collection)
        {
            return _enabled && collection != null && collection.Enumerable.Any(Relevant);
        }

        internal static void Record(string eventName, Buff buff,
            BuffCollection collection, TimeSpan? requestedDuration,
            Exception exception, string stack)
        {
            if (!_enabled) return;
            try
            {
                if (collection == null && buff != null && buff.Owner != null)
                    collection = buff.Owner.Buffs;
                if (buff != null && !Relevant(buff) && !Relevant(collection)) return;
                if (buff == null && collection != null && !Relevant(collection)) return;
                DodgeBuffLifecycleRecord record = Snapshot(eventName, buff, collection,
                    requestedDuration, exception, stack);
                Write(record);
            }
            catch { }
        }

        internal static void RecordCreation(string eventName, BlueprintBuff blueprint,
            MechanicsContext context, TimeSpan? duration, Buff result, bool? allowed,
            Exception exception)
        {
            if (!_enabled || !IsExact(blueprint)) return;
            string detail = "allowed=" + (allowed.HasValue ? allowed.Value.ToString() : "unknown") +
                ";sourceAbility=" + BlueprintIdentity(ReadMember(context, "SourceAbility"));
            try
            {
                DodgeBuffLifecycleRecord record = Snapshot(eventName, result,
                    result == null ? null : result.Owner.Buffs, duration, exception,
                    new StackTrace(true) + Environment.NewLine + detail);
                record.blueprintGuid = blueprint.AssetGuid;
                record.blueprintInternalName = blueprint.name;
                record.sourceAbilityGuid = BlueprintGuid(ReadMember(context, "SourceAbility"));
                record.sourceAbilityInternalName = BlueprintName(ReadMember(context, "SourceAbility"));
                record.contextIdentity = context == null ? null :
                    RuntimeHelpers.GetHashCode(context).ToString("X8");
                Write(record);
            }
            catch { }
        }

        private static void Write(DodgeBuffLifecycleRecord record)
        {
            lock (Gate)
            {
                if (!_enabled || _writer == null) return;
                try
                {
                    record.sequence = ++_sequence;
                    string line = JsonConvert.SerializeObject(record,
                        Formatting.None, JsonLineSettings);
                    _writer.WriteLine(line);
                    _writer.Flush();
                    _writer.BaseStream.Flush();
                }
                catch (Exception exception)
                {
                    _enabled = false;
                    try { _writer.Dispose(); } catch { }
                    _writer = null;
                    if (!_writeFailureLogged && _logger != null)
                    {
                        _writeFailureLogged = true;
                        _logger.Failure("diagnostics", "dodge-forensics.write-failed",
                            "Manual Dodge forensics disabled after a JSONL write failure.",
                            exception);
                    }
                }
            }
        }

        private static DodgeBuffLifecycleRecord Snapshot(string eventName, Buff buff,
            BuffCollection collection, TimeSpan? duration, Exception exception, string stack)
        {
            TimeSpan gameTime = Game.Instance == null || Game.Instance.TimeController == null
                ? TimeSpan.MinValue : Game.Instance.TimeController.GameTime;
            Buff next = NextEvent == null || collection == null ? null :
                NextEvent.GetValue(collection) as Buff;
            UnitDescriptor owner = buff == null ? ReadOwner(collection) : buff.Owner;
            Buff[] dodge = collection == null ? new Buff[0] : collection.Enumerable
                .Where(value => value != null && IsExact(value.Blueprint)).ToArray();
            var record = new DodgeBuffLifecycleRecord
            {
                utcTimestamp = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                eventName = eventName,
                threadId = Thread.CurrentThread.ManagedThreadId,
                onGameThread = Thread.CurrentThread.ManagedThreadId == _gameThreadId,
                isGameThread = Thread.CurrentThread.ManagedThreadId == _gameThreadId,
                gameTimeTicks = gameTime.Ticks,
                gameTimeSeconds = gameTime == TimeSpan.MinValue ? (double?)null : gameTime.TotalSeconds,
                turnBasedCombatActive = IsTurnBased(), turnBasedState = IsTurnBased(),
                currentTurnUnitIdentity = CurrentTurnIdentity(),
                ownerIdentity = UnitIdentity(owner), ownerCharacterName = OwnerName(owner),
                buffRuntimeReferenceId = buff == null ? null : RuntimeHelpers.GetHashCode(buff).ToString("X8"),
                buffRuntimeUniqueId = Value(ReadMember(buff, "UniqueId")),
                blueprintGuid = buff == null ? null : buff.Blueprint.AssetGuid,
                blueprintInternalName = buff == null ? null : buff.Blueprint.name,
                casterIdentity = UnitIdentity(ReadMember(buff == null ? null : buff.Context, "MaybeCaster") as UnitDescriptor),
                sourceAbilityGuid = BlueprintGuid(ReadMember(buff == null ? null : buff.Context, "SourceAbility")),
                sourceAbilityInternalName = BlueprintName(ReadMember(buff == null ? null : buff.Context, "SourceAbility")),
                contextIdentity = buff == null || buff.Context == null ? null :
                    RuntimeHelpers.GetHashCode(buff.Context).ToString("X8"),
                dodgeCollectionCount = dodge.Length,
                dodgeInstanceIdentities = dodge.Select(InstanceIdentity).ToArray(),
                endTimeTicks = buff == null ? (long?)null : buff.EndTime.Ticks,
                endTimeSeconds = buff == null ? (double?)null : buff.EndTime.TotalSeconds,
                timeLeftTicks = buff == null ? (long?)null : buff.TimeLeft.Ticks,
                timeLeftSeconds = buff == null ? (double?)null : buff.TimeLeft.TotalSeconds,
                nextTickTimeTicks = buff == null ? (long?)null : ReadTime(NextTick, buff).Ticks,
                nextTickTimeSeconds = buff == null ? (double?)null : ReadTime(NextTick, buff).TotalSeconds,
                nextEventTimeTicks = buff == null ? (long?)null : buff.NextEventTime.Ticks,
                nextEventTimeSeconds = buff == null ? (double?)null : buff.NextEventTime.TotalSeconds,
                isPermanent = buff == null ? (bool?)null : buff.IsPermanent,
                isActive = buff == null ? (bool?)null : buff.Active,
                isDisposed = buff == null ? (bool?)null : buff.IsDisposed,
                rank = buff == null ? (int?)null : buff.GetRank(),
                collectionNextEventRuntimeIdentity = next == null ? null : InstanceIdentity(next),
                collectionNextEventBlueprintGuid = next == null ? null : next.Blueprint.AssetGuid,
                collectionNextEventBlueprintInternalName = next == null ? null : next.Blueprint.name,
                armorClassModifiedValue = owner == null || owner.Stats == null ? (int?)null : owner.Stats.AC.ModifiedValue,
                gritAmount = ReadGrit(owner), requestedDurationTicks = duration.HasValue ? duration.Value.Ticks : (long?)null,
                requestedDurationSeconds = duration.HasValue ? duration.Value.TotalSeconds : (double?)null,
                exceptionType = exception == null ? null : exception.GetType().FullName,
                exceptionMessage = exception == null ? null : exception.Message,
                managedStackTrace = stack
            };
            return record;
        }

        private static UnitDescriptor ReadOwner(BuffCollection collection)
        { return ReadMember(collection, "Owner") as UnitDescriptor; }
        private static object ReadMember(object value, string name)
        {
            if (value == null) return null;
            Type type = value.GetType();
            PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null) return property.GetValue(value, null);
            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(value);
        }
        private static TimeSpan ReadTime(PropertyInfo property, object value)
        { object result = property == null ? null : property.GetValue(value, null); return result is TimeSpan ? (TimeSpan)result : TimeSpan.MinValue; }
        private static string Value(object value) { return value == null ? null : Convert.ToString(value, CultureInfo.InvariantCulture); }
        private static string InstanceIdentity(Buff value) { return RuntimeHelpers.GetHashCode(value).ToString("X8") + ":" + Value(ReadMember(value, "UniqueId")); }
        private static string BlueprintIdentity(object value) { return BlueprintName(value) + "@" + BlueprintGuid(value); }
        private static string BlueprintGuid(object value) { BlueprintScriptableObject b = value as BlueprintScriptableObject; return b == null ? null : b.AssetGuid; }
        private static string BlueprintName(object value) { BlueprintScriptableObject b = value as BlueprintScriptableObject; return b == null ? null : b.name; }
        private static string UnitIdentity(UnitDescriptor owner) { return owner == null || owner.Unit == null ? null : owner.Unit.UniqueId; }
        private static string OwnerName(UnitDescriptor owner) { return owner == null || owner.Unit == null ? null : owner.Unit.CharacterName; }
        private static bool IsTurnBased() { try { return Game.Instance != null && Game.Instance.TurnBasedCombatController != null && TurnBased.Controllers.CombatController.IsInTurnBasedCombat(); } catch { return false; } }
        private static string CurrentTurnIdentity() { try { var c = Game.Instance.TurnBasedCombatController; return c == null || c.CurrentTurn == null || c.CurrentTurn.Unit == null ? null : c.CurrentTurn.Unit.UniqueId; } catch { return null; } }
        private static int? ReadGrit(UnitDescriptor owner) { try { var set = KingmakerGunslinger.Bootstrap.BlueprintBootstrap.GunslingerClass; return owner == null || set == null ? (int?)null : owner.Resources.GetResourceAmount(set.Grit.Resource); } catch { return null; } }
    }

    internal sealed class DodgeBuffLifecycleRecord
    {
        public long sequence; public string utcTimestamp; public string eventName;
        public int threadId; public bool onGameThread; public bool isGameThread;
        public long gameTimeTicks;
        public double? gameTimeSeconds; public bool turnBasedCombatActive;
        public bool turnBasedState;
        public string currentTurnUnitIdentity; public string ownerIdentity;
        public string ownerCharacterName; public string buffRuntimeReferenceId;
        public string buffRuntimeUniqueId; public string blueprintGuid;
        public string blueprintInternalName; public string casterIdentity;
        public string sourceAbilityGuid; public string sourceAbilityInternalName;
        public string contextIdentity; public int dodgeCollectionCount;
        public string[] dodgeInstanceIdentities; public long? endTimeTicks;
        public double? endTimeSeconds; public long? timeLeftTicks;
        public double? timeLeftSeconds; public long? nextTickTimeTicks;
        public double? nextTickTimeSeconds; public long? nextEventTimeTicks;
        public double? nextEventTimeSeconds; public bool? isPermanent;
        public bool? isActive; public bool? isDisposed; public int? rank;
        public string collectionNextEventRuntimeIdentity;
        public string collectionNextEventBlueprintGuid;
        public string collectionNextEventBlueprintInternalName;
        public int? armorClassModifiedValue; public int? gritAmount;
        public long? requestedDurationTicks; public double? requestedDurationSeconds;
        public string exceptionType; public string exceptionMessage;
        public string managedStackTrace;
    }
}
