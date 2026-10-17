using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Harmony12;
using Kingmaker;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Compatibility
{
    // CotW's 1.14.4c spontaneous prefix accepts the unrelated memorized-slot
    // class. Correct only its three reads; retain its complete conversion helper,
    // settings, variants, native patch ownership and ordering.
    internal static class CotwSpontaneousSlotCompatibility
    {
        private const string CotwMvid = "8caab254-aacf-4811-8093-44b9184e6e53";
        private const string GameMvid = "07fa1e4d-8618-41b3-9b8d-faa17d3b26f7";
        private const string PrefixType = "CallOfTheWild.SpellManipulationMechanics." +
            "MechanicActionBarSlotSpontaneusSpell__GetConvertedAbilityData__Patch";
        private static ModContext _context;
        private static MethodInfo _patched;
        private static int _observations;
        private static int _pendingObservations;

        internal static void Install(ModContext context)
        {
            _context = context;
            if (_patched != null) return;
            try
            {
                Assembly[] matches = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(value => value.GetName().Name == "CallOfTheWild").ToArray();
                if (matches.Length == 0) return;
                if (matches.Length != 1) throw new InvalidOperationException("Ambiguous CotW assembly.");
                MethodInfo prefix = VerifyContract(matches[0]);
                MethodInfo native = typeof(MechanicActionBarSlotSpontaneusSpell)
                    .GetMethod("GetConvertedAbilityData");
                Log("patches.before", Describe(context.Harmony.GetPatchInfo(native)));
                Log("prefix-patches.before", Describe(context.Harmony.GetPatchInfo(prefix)));
                Log("assemblies", AssemblyIdentity(matches[0]) + ";game=" +
                    AssemblyIdentity(native.Module.Assembly) + ";kmg=" + AssemblyIdentity(context.Assembly));
                context.Harmony.Patch(prefix, null, null, new HarmonyMethod(
                    typeof(CotwSpontaneousSlotCompatibility).GetMethod("Transpile",
                        BindingFlags.Static | BindingFlags.NonPublic)));
                _patched = prefix;
                Log("installed", "cotw=" + matches[0].Location + ";mvid=" + CotwMvid +
                    ";gameMvid=" + GameMvid + ";correctedReads=3;nativeOwnersUnchanged=true");
            }
            catch (Exception exception)
            {
                context.Logger.Failure("cotw-actionbar", "contract.refused",
                    "Optional spontaneous-slot compatibility was not installed.", exception);
            }
        }

        internal static MethodInfo VerifyContract(Assembly cotw)
        {
            if (cotw.ManifestModule.ModuleVersionId.ToString() != CotwMvid ||
                typeof(MechanicActionBarSlotSpontaneusSpell).Module.ModuleVersionId.ToString() != GameMvid ||
                typeof(MechanicActionBarSlotMemorizedSpell).IsAssignableFrom(
                    typeof(MechanicActionBarSlotSpontaneusSpell)))
                throw new InvalidOperationException("Unverified CotW/game slot contract.");
            Type type = cotw.GetType(PrefixType, true);
            MethodInfo prefix = type.GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] { typeof(MechanicActionBarSlotMemorizedSpell),
                    typeof(List<AbilityData>).MakeByRefType() }, null);
            if (prefix == null || prefix.ReturnType != typeof(bool) ||
                prefix.GetMethodBody().GetILAsByteArray().Length != 115)
                throw new InvalidOperationException("CotW spontaneous prefix changed.");
            return prefix;
        }

        private static IEnumerable<CodeInstruction> Transpile(
            IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> body = instructions.ToList();
            MethodInfo wrongGetter = typeof(MechanicActionBarSlotMemorizedSpell)
                .GetProperty("Spell").GetGetMethod();
            CodeInstruction[] reads = body.Where(value =>
                (value.opcode == OpCodes.Call || value.opcode == OpCodes.Callvirt) &&
                Equals(value.operand, wrongGetter)).ToArray();
            if (reads.Length != 3)
                throw new InvalidOperationException("Expected exactly three CotW memorized-slot reads.");
            MethodInfo correctGetter = typeof(CotwSpontaneousSlotCompatibility)
                .GetMethod("ReadSpell", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (CodeInstruction read in reads)
            {
                read.opcode = OpCodes.Call;
                read.operand = correctGetter;
            }
            Label ready = generator.DefineLabel();
            body[0].labels.Add(ready);
            var result = new List<CodeInstruction> {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, typeof(CotwSpontaneousSlotCompatibility)
                    .GetMethod("Ready", BindingFlags.Static | BindingFlags.NonPublic)),
                new CodeInstruction(OpCodes.Brtrue, ready),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Ret)
            };
            result.AddRange(body);
            return result;
        }

        private static AbilityData ReadSpell(object value)
        {
            var slot = value as MechanicActionBarSlotSpontaneusSpell;
            return slot == null ? null : slot.Spell;
        }

        private static bool Ready(object value, ref List<AbilityData> result)
        {
            AbilityData spell = ReadSpell(value);
            // Do not obtain a book from another unit/book or construct substitute
            // AbilityData. An empty presentation has no conversion options. A later
            // native page/unit rebind calls this again against current context.
            bool ready = spell != null && spell.Blueprint != null && spell.Caster != null &&
                spell.Caster.Progression != null && spell.Caster.Progression.Features != null &&
                spell.Caster.Abilities != null;
            var book = ready ? spell.Spellbook : null;
            ready = ready && book != null && ReferenceEquals(book.Owner, spell.Caster);
            if ((ready && _observations++ < 12) || (!ready && _pendingObservations++ < 8))
                Observe(value, spell, book, ready);
            if (!ready) result = new List<AbilityData>();
            return ready;
        }

        private static void Observe(object slot, AbilityData spell,
            Kingmaker.UnitLogic.Spellbook book, bool ready)
        {
            if (_context == null) return;
            // Diagnostics must not turn an incomplete UI context into an exception.
            try
            {
                var cached = spell == null ? null : typeof(AbilityData).GetField(
                    "m_CachedSpellbook", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(spell)
                    as Kingmaker.UnitLogic.Spellbook;
                Log("slot.observed", "stage=GetConvertedAbilityData;slot=" + Identity(slot) +
                    ";spell=" + Identity(spell) + ";blueprint=" +
                    (spell == null || spell.Blueprint == null ? "<null>" : spell.Blueprint.AssetGuid) +
                    ";caster=" + Identity(spell == null ? null : spell.Caster) +
                    ";book=" + Identity(book) + ";owner=" + Identity(book == null ? null : book.Owner) +
                    ";cachedBook=" + Identity(cached) + ";cachedOwner=" + Identity(cached == null ? null : cached.Owner) +
                    ";progression=" + Identity(spell == null || spell.Caster == null ? null : spell.Caster.Progression) +
                    ";ready=" + ready + ";input=" + (Game.Instance == null ? "unavailable" :
                        (Game.Instance.IsControllerGamepad ? "controller" : "mouse-keyboard")));
            }
            catch (Exception exception) { Log("diagnostic.unavailable", exception.GetType().Name); }
        }

        private static string Identity(object value)
        {
            return value == null ? "<null>" : value.GetType().FullName + "#" +
                RuntimeHelpers.GetHashCode(value).ToString("X8");
        }

        private static string AssemblyIdentity(Assembly assembly)
        {
            using (var stream = File.OpenRead(assembly.Location))
            using (var sha = SHA256.Create())
                return assembly.Location + "/mvid=" + assembly.ManifestModule.ModuleVersionId +
                    "/sha256=" + BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }

        private static string Describe(Patches patches)
        {
            if (patches == null) return "none";
            return "prefixes=" + Describe(patches.Prefixes) + ";postfixes=" +
                Describe(patches.Postfixes) + ";transpilers=" + Describe(patches.Transpilers);
        }

        private static string Describe(IEnumerable<Patch> patches)
        {
            return string.Join("|", patches.Select(value => value.index + ":" + value.owner +
                "/" + value.priority + "/before=" + string.Join(",", value.before) +
                "/after=" + string.Join(",", value.after) + "/" + value.patch.DeclaringType.FullName +
                "." + value.patch.Name).ToArray());
        }

        private static void Log(string name, string message)
        {
            if (_context != null) _context.Logger.Info("cotw-actionbar", name, message);
        }
    }
}
