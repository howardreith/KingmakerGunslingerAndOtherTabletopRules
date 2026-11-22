using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Checkpoint B (S6 T3 / RISK-02): does a saved Powerful Change increase survive when its caster cannot be
    /// resolved at load? Three Brown-Fur Arcanist casters are removable companions created by the engine's own
    /// persistent-party path (Game.AddUnitToPersistentSate with the default player blueprint, CanRemoveFromParty,
    /// Player.AddCompanion), so they live in the cross-scene party state like a player-made mercenary. Targets are the
    /// main character and a fourth companion, so their buffs load through party.json (cross-scene), not area state.
    /// - present: caster P stays in the party; P casts Bull's Strength (armed) and Cat's Grace (unarmed) on target T;
    /// - remote: caster R casts Cat's Grace (armed) on the main character, then is swapped out (Player.RemoveCompanion);
    /// - dismissed: caster D casts Bull's Strength (armed) on the main character, then is dismissed
    ///   (Player.DismissCompanion, which destroys the unit), the supported way a custom companion leaves for good.
    /// Expectations are the correct behavior: an effect the engine keeps keeps its exact increase.
    /// </summary>
    internal sealed partial class BrownFurProbe
    {
        private const string RestoreFile = "brown-fur-restoration-fixture.json";

        private IEnumerable<object> RestoreCreate()
        {
            foreach (object step in AwaitBrownFur()) yield return step;
            if (!ResolveBrownFur()) yield break;
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            var units = new JObject { ["main"] = main.UniqueId };
            var companions = new Dictionary<string, UnitEntityData>();
            string[] names = { "presentCaster", "remoteCaster", "dismissedCaster", "companionTarget" };
            for (int index = 0; index < names.Length; index++)
            {
                UnitEntityData unit = CreateCompanion(new Vector3(1.5f + index, 0f, -1.5f));
                if (unit == null) { problems.Add("the engine did not create custom companion " + names[index]); yield break; }
                companions[names[index]] = unit;
                units[names[index]] = unit.UniqueId;
            }
            result["units"] = units;
            yield return null;
            foreach (string caster in new[] { "presentCaster", "remoteCaster", "dismissedCaster" })
                if (!PrepareArcanist(companions[caster])) yield break;
            result["party"] = new JArray(Game.Instance.Player.Party.Select(u => (object)u.UniqueId).ToArray());
            yield return null;

            UnitEntityData p = companions["presentCaster"], r = companions["remoteCaster"], d = companions["dismissedCaster"];
            UnitEntityData t = companions["companionTarget"];
            var casts = new JObject
            {
                ["present.bullsStrength.onCompanion"] = CastAbilityBonus(p, t, BullsStrength, StatType.Strength, 0, 6),
                ["present.catsGraceUnarmed.onCompanion"] = CastAbilityBonus(p, t, CatsGrace, StatType.Dexterity, 1, 4, false),
                ["remote.catsGrace.onMain"] = CastAbilityBonus(r, main, CatsGrace, StatType.Dexterity, 1, 6),
                ["dismissed.bullsStrength.onMain"] = CastAbilityBonus(d, main, BullsStrength, StatType.Strength, 0, 6)
            };
            result["casts"] = casts;
            if (problems.Count != 0) yield break;
            result["effects.cast"] = Effects(main, t);

            // Supported lifecycle operations.
            Game.Instance.Player.RemoveCompanion(r);
            Game.Instance.Player.DismissCompanion(d);
            yield return null;
            result["lifecycle"] = new JObject
            {
                ["remote.inParty"] = Game.Instance.Player.Party.Contains(r), ["remote.isInGame"] = r.IsInGame,
                ["remote.inRemoteCompanions"] = Game.Instance.Player.RemoteCompanions.Any(u => u.UniqueId == r.UniqueId),
                ["remote.resolvable"] = Game.Instance.State.Units[r.UniqueId] != null,
                ["dismissed.destroyed"] = d.Destroyed,
                ["dismissed.resolvable"] = Game.Instance.State.Units[d.UniqueId] != null
            };
            JObject effects = Effects(main, t);
            result["effects.afterLifecycle"] = effects;
            // The engine keeps both buffs on the main character after its casters leave, with their increases, in-session.
            ExpectEffect(effects, "main.strength", 1, 6, d.UniqueId);
            ExpectEffect(effects, "main.dexterity", 1, 6, r.UniqueId);
            ExpectEffect(effects, "companion.strength", 1, 6, p.UniqueId);
            ExpectEffect(effects, "companion.dexterity", 1, 4, p.UniqueId);
            if ((bool)result["lifecycle"]["dismissed.resolvable"] || !(bool)result["lifecycle"]["dismissed.destroyed"])
                problems.Add("the dismissed caster is still resolvable or not destroyed");
            if (!(bool)result["lifecycle"]["remote.inRemoteCompanions"] || (bool)result["lifecycle"]["remote.inParty"])
                problems.Add("the remote caster is not a remote companion");
            ExpectRecords(main, "main.created", 2);
            ExpectRecords(t, "companion.created", 1);
            File.WriteAllText(Path.Combine(profileDir, RestoreFile), new JObject { ["units"] = units, ["effects"] = effects }.ToString());
            if (problems.Count != 0) yield break;
            foreach (object step in Save()) yield return step;
        }

        private IEnumerable<object> RestoreReload(bool mutate)
        {
            foreach (object step in AwaitBrownFur()) yield return step;
            if (!ResolveBrownFur()) yield break;
            string path = Path.Combine(profileDir, RestoreFile);
            if (!File.Exists(path)) { problems.Add("no restoration fixture at " + path); yield break; }
            JObject fixture = JObject.Parse(File.ReadAllText(path));
            JObject units = (JObject)fixture["units"];
            result["units"] = units;
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            UnitEntityData t = Game.Instance.State.Units[(string)units["companionTarget"]];
            UnitEntityData p = Game.Instance.State.Units[(string)units["presentCaster"]];
            if (main == null || t == null || p == null) { problems.Add("a party unit of the fixture is not loaded"); yield break; }
            result["lifecycle"] = new JObject
            {
                ["remote.resolvable"] = Game.Instance.State.Units[(string)units["remoteCaster"]] != null,
                ["dismissed.resolvable"] = Game.Instance.State.Units[(string)units["dismissedCaster"]] != null
            };
            result["restorationCalls"] = new JArray(BrownFurObserver.Restorations.Cast<object>().ToArray());
            foreach (UnitEntityData target in new[] { main, t })
            {
                object part = Part(target);
                int id = part == null ? -1 : RuntimeHelpers.GetHashCode(part);
                JObject[] postLoads = BrownFurObserver.Calls.Where(c => (int)c["part"] == id && (string)c["kind"] == "PostLoad").ToArray();
                if (postLoads.Length != 1 || !(bool)postLoads[0]["completed"])
                    problems.Add(target.UniqueId + ": " + postLoads.Length + " native PostLoad calls on its persistence part, expected exactly one completed");
            }
            JObject effects = Effects(main, t);
            result["effects.loaded"] = effects;
            ExpectRestored(effects, units, mutate ? "loaded" : "final");
            if (!mutate)
            {
                ExpectMutatedEffects(effects, units);
                ExpectRecords(main, "main.final", 1);
                ExpectRecords(t, "companion.final", 0);
                yield break;
            }

            // The engine's own repeated callback: Fact.Recalculate removes and re-adds each modifier.
            foreach (Buff buff in main.Descriptor.Buffs.RawFacts.OfType<Buff>().Concat(t.Descriptor.Buffs.RawFacts.OfType<Buff>()).ToArray())
                for (int index = 0; index < 3; index++) buff.Recalculate();
            JObject recalculated = Effects(main, t);
            result["effects.recalculated"] = recalculated;
            if (!JToken.DeepEquals(Values(effects), Values(recalculated)))
                problems.Add("repeated recalculation changed modifiers: " + Values(effects).ToString(Formatting.None) + " -> " + Values(recalculated).ToString(Formatting.None));

            // Removal: the dismissed caster's Bull's Strength on the main character; its record must go, Dexterity stays.
            Buff strength = main.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(b => b.Blueprint.AssetGuid == BullsStrengthBuff);
            if (strength == null) problems.Add("main: no Bull's Strength buff to remove");
            else strength.Remove();
            yield return null;
            // Ordinary (unarmed) recast by the present caster on the companion: the enhanced modifier returns to +4.
            JObject recast = CastAbilityBonus(p, t, BullsStrength, StatType.Strength, 0, 4, false);
            result["recastUnarmed"] = recast;
            JObject mutated = Effects(main, t);
            result["effects.mutated"] = mutated;
            ExpectMutatedEffects(mutated, units);
            // The removed buff's record is forgotten (main keeps the remote caster's Dexterity record); the ordinary
            // recast forgets the companion's enhanced Strength record.
            ExpectRecords(main, "main.mutated", 1);
            ExpectRecords(t, "companion.mutated", 0);
        }

        private void ExpectRestored(JObject effects, JObject units, string phase)
        {
            if (phase != "loaded") return;
            ExpectEffect(effects, "main.strength", 1, 6, (string)units["dismissedCaster"]);
            ExpectEffect(effects, "main.dexterity", 1, 6, (string)units["remoteCaster"]);
            ExpectEffect(effects, "companion.strength", 1, 6, (string)units["presentCaster"]);
            ExpectEffect(effects, "companion.dexterity", 1, 4, (string)units["presentCaster"]);
        }

        private void ExpectMutatedEffects(JObject effects, JObject units)
        {
            ExpectEffect(effects, "main.strength", 0, 0, null);
            ExpectEffect(effects, "main.dexterity", 1, 6, (string)units["remoteCaster"]);
            ExpectEffect(effects, "companion.strength", 1, 4, (string)units["presentCaster"]);
            ExpectEffect(effects, "companion.dexterity", 1, 4, (string)units["presentCaster"]);
        }

        private void ExpectEffect(JObject effects, string name, int count, int value, string caster)
        {
            JObject e = (JObject)effects[name];
            bool ok = (int)e["count"] == count && (count == 0 || ((int)e["value"] == value && (string)e["descriptor"] == "Enhancement" &&
                (string)e["savedCasterId"] == caster));
            if (!ok) problems.Add(name + ": " + e.ToString(Formatting.None) + ", expected " + count + " modifier(s)" +
                (count == 0 ? "" : " of +" + value + " Enhancement from caster " + caster));
        }

        private void ExpectRecords(UnitEntityData unit, string name, int active)
        {
            int count = RecordCount(unit);
            result["records." + name] = count;
            if (count != active) problems.Add(name + ": " + count + " active Powerful Change records, expected " + active);
        }

        private static int RecordCount(UnitEntityData unit)
        {
            object part = Part(unit);
            return part == null ? 0 : (int)BrownFurObserver.PartType.GetProperty("Count", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(part, null);
        }

        private static JObject Values(JObject effects) =>
            new JObject(effects.Properties().Select(pr => new JProperty(pr.Name, new JObject { ["count"] = pr.Value["count"], ["value"] = pr.Value["value"] })));

        /// <summary>Each target stat's spell modifiers with the owning buff, its saved caster id and its lifetime.</summary>
        private JObject Effects(UnitEntityData main, UnitEntityData companion)
        {
            return new JObject
            {
                ["main.strength"] = Effect(main, StatType.Strength, BullsStrength),
                ["main.dexterity"] = Effect(main, StatType.Dexterity, CatsGrace),
                ["companion.strength"] = Effect(companion, StatType.Strength, BullsStrength),
                ["companion.dexterity"] = Effect(companion, StatType.Dexterity, CatsGrace)
            };
        }

        private static JObject Effect(UnitEntityData unit, StatType stat, string spellGuid)
        {
            JObject observed = Modifiers(unit, stat, spellGuid);
            ModifiableValue value = unit.Descriptor.Stats.GetStat(stat);
            Buff buff = value.Modifiers.Select(m => m.Source as Buff).FirstOrDefault(b => b != null && b.Context != null &&
                b.Context.SourceAbility != null && b.Context.SourceAbility.AssetGuid == spellGuid);
            if (buff != null)
            {
                observed["savedCasterId"] = SavedCasterId(buff.Context);
                observed["casterResolved"] = buff.Context.MaybeCaster != null;
                observed["timeLeftSeconds"] = buff.TimeLeft.TotalSeconds;
            }
            return observed;
        }

        /// <summary>The caster id the engine saved in the context (its private UnitReference), read for evidence only.</summary>
        internal static string SavedCasterId(MechanicsContext context)
        {
            FieldInfo field = typeof(MechanicsContext).GetField("m_CasterReference", BindingFlags.Instance | BindingFlags.NonPublic);
            object reference = field == null ? null : field.GetValue(context);
            return reference == null ? null : ((UnitReference)reference).UniqueId;
        }

        /// <summary>The engine's persistent-party creation of a custom companion (CreateImportedCompanion without UI).</summary>
        private UnitEntityData CreateCompanion(Vector3 offset)
        {
            MethodInfo add = typeof(Game).GetMethod("AddUnitToPersistentSate", BindingFlags.Instance | BindingFlags.NonPublic);
            // The engine's CustomCompanion blueprint has no race until chargen sets one (a save holding it fails
            // PreloadClothes on load, session 20261122T183136Z); the default player blueprint, which the engine's own
            // persistent-party path uses for the main character, has one.
            var unit = (UnitEntityData)add.Invoke(Game.Instance, new object[] { BlueprintRoot.Instance.DefaultPlayerCharacter });
            unit.Descriptor.State.CanRemoveFromParty = true;
            unit.AttachToViewOnLoad(null);
            Game.Instance.Player.AddCompanion(unit);
            unit.IsInGame = true;
            unit.Position = Game.Instance.Player.MainCharacter.Value.Position + offset;
            unit.Descriptor.Stats.HitPoints.BaseValue = 10000;
            return unit.View == null ? null : unit;
        }

        private bool PrepareArcanist(UnitEntityData caster)
        {
            caster.Descriptor.Stats.Intelligence.BaseValue = 30;
            Advance(caster.Descriptor, (BlueprintCharacterClass)C("ArcanistClass"), 5);
            Spellbook casting = Book(caster, "CastingSpellbook"), preparation = Book(caster, "MemorizationSpellbook");
            if (casting == null || preparation == null) { problems.Add(caster.UniqueId + ": the Arcanist did not own both resolved spellbooks"); return false; }
            int level = caster.Descriptor.Progression.GetClassLevel((BlueprintCharacterClass)C("ArcanistClass"));
            foreach (Spellbook book in new[] { casting, preparation })
            {
                while (book.CasterLevel < level) book.AddCasterLevel();
                book.UpdateAllSlotsSize(false);
                book.Rest();
            }
            caster.Descriptor.Resources.Add((Kingmaker.Blueprints.BlueprintAbilityResource)C("Reservoir"), true);
            foreach (string feature in new[] { "PowerfulChange", "ShareTransmutation" })
            {
                var blueprint = (Kingmaker.Blueprints.Facts.BlueprintUnitFact)B(feature);
                if (!caster.Descriptor.HasFact(blueprint) && caster.Descriptor.AddFact(blueprint) == null)
                { problems.Add(caster.UniqueId + ": Brown-Fur feature " + feature + " could not be granted"); return false; }
            }
            return true;
        }
    }
}
