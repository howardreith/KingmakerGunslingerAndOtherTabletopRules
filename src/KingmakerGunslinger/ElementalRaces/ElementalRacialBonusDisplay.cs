using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UI._ConsoleUI.CharacterInfo;
using Kingmaker.UI.Common;
using Kingmaker.UI.ServiceWindow.CharacterScreen;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.ElementalRaces
{
    /// <summary>
    /// Racial-bonus labels for elemental-race ability scores.
    /// Kingmaker 2.1.7b (Assembly-CSharp MVID 07fa1e4d-8618-41b3-9b8d-faa17d3b26f7)
    /// labels an ability with the first Racial modifier it enumerates
    /// (CharSScoreEntry.SetData and the console CharInfoStatVM.OnStatUpdated).
    /// Each descriptor list is kept sorted ascending by value, so for an alternate
    /// heritage (General race modifier plus the marker's net adjustment) the label
    /// shows the most negative entry, e.g. Stormsoul Int "-2" for an effective 0.
    /// Racial is a default-stacking descriptor, so the actual scores are correct.
    /// The label here is the engine's own effective Racial aggregate.
    /// </summary>
    internal static class ElementalRacialBonusDisplay
    {
        // ApplyModifiersFiltered clamps its result to MinValueRaw, which is 5
        // for player-faction attributes outside True Death. GetDescriptorBonus
        // starts from 0 and is therefore clamped; start well above any clamp.
        private const int AggregateOffset = 1 << 20;

        private static readonly MethodInfo ApplyModifiersFiltered =
            typeof(ModifiableValue).GetMethod("ApplyModifiersFiltered",
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new[] { typeof(int), typeof(Func<ModifiableValue.Modifier, bool>) },
                null);

        private static readonly Func<ModifiableValue.Modifier, bool> IsRacial =
            modifier => modifier != null &&
                modifier.ModDescriptor == ModifierDescriptor.Racial;

        private static int _failureReported;

        /// <summary>True only for a unit whose race is a registered elemental race.</summary>
        internal static bool AppliesTo(UnitDescriptor owner)
        {
            ElementalRaceBlueprintSet races = BlueprintBootstrap.ElementalRaces;
            var race = owner?.Progression?.Race;
            return races != null && race != null &&
                races.OrderedRaces().Any(value => ReferenceEquals(value, race));
        }

        /// <summary>
        /// The engine's effective Racial contribution to <paramref name="stat"/>,
        /// computed by the native stacking algorithm over Racial modifiers only.
        /// </summary>
        internal static int EffectiveRacialBonus(ModifiableValue stat)
        {
            if (stat == null) throw new ArgumentNullException("stat");
            if (ApplyModifiersFiltered == null)
                throw new MissingMethodException(
                    "ModifiableValue.ApplyModifiersFiltered(int, Func<Modifier,bool>) is unavailable.");
            int value = (int)ApplyModifiersFiltered.Invoke(stat,
                new object[] { AggregateOffset, IsRacial });
            return value - AggregateOffset;
        }

        /// <summary>Native label text: "-" when there is no effective racial bonus.</summary>
        internal static string Label(int effective)
        {
            return effective == 0 ? "-" : UIUtility.AddSign(effective);
        }

        internal static void ReportFailure(Exception exception)
        {
            if (System.Threading.Interlocked.Exchange(ref _failureReported, 1) != 0) return;
            string message = "Elemental racial-bonus label kept the native value: " +
                exception.Message;
            var logger = BlueprintBootstrap.ElementalRaces?.Visuals?.Registry?.Logger;
            if (logger != null)
                logger.Warning("elemental-races", "racial-bonus-label.isolated", message);
            else UnityEngine.Debug.LogWarning("[KMG][elemental-races] " + message);
        }
    }

    // CharBScoresEntry (creator and level-up ability panel) calls this base
    // method, so one postfix covers the creator and the character sheet.
    [HarmonyPatch(typeof(CharSScoreEntry), "SetData")]
    internal static class ElementalRacialBonusLabelPatch
    {
        private static void Postfix(CharSScoreEntry __instance, UnitDescriptor descriptor)
        {
            try
            {
                if (__instance == null || __instance.RaceBonus == null) return;
                ModifiableValueAttributeStat stat = __instance.Stat;
                if (stat == null || !stat.Enabled) return;
                if (!ElementalRacialBonusDisplay.AppliesTo(descriptor)) return;
                __instance.RaceBonus.text = ElementalRacialBonusDisplay.Label(
                    ElementalRacialBonusDisplay.EffectiveRacialBonus(stat));
            }
            catch (Exception exception)
            {
                ElementalRacialBonusDisplay.ReportFailure(exception);
            }
        }
    }

    [HarmonyPatch(typeof(CharInfoStatVM), "OnStatUpdated")]
    internal static class ElementalRacialBonusConsoleLabelPatch
    {
        private static void Postfix(CharInfoStatVM __instance)
        {
            try
            {
                var stat = __instance?.Stat?.Value as ModifiableValueAttributeStat;
                if (stat == null || !ElementalRacialBonusDisplay.AppliesTo(stat.Owner)) return;
                int effective = ElementalRacialBonusDisplay.EffectiveRacialBonus(stat);
                __instance.RaceBonus.Value = effective == 0 ? (int?)null : effective;
            }
            catch (Exception exception)
            {
                ElementalRacialBonusDisplay.ReportFailure(exception);
            }
        }
    }
}
