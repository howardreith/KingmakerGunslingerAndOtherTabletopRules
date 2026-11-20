using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// S5 T5 / POL-06, in the cleric phase's native Default fixture: Call of the
    /// Wild's metamagic conversion rows (its "metamagic for spontaneous spell
    /// conversion" setting) projected through the creature menu.
    /// <list type="bullet">
    /// <item>Herald (Summon Monster) and Druid (Summon Nature's Ally) popups
    /// over an unrelated level 4 spell prepared with Extend Spell in a level 5
    /// slot: the ordinary level 5 rows unchanged, CotW's level 4 rows projected
    /// into the permitted level 4 creatures, each carrying that row's Extend at
    /// spell level 5, none left raw;</item>
    /// <item>a projected cast (the Black Widow on the Herald route): Extend's
    /// doubled duration from the engine's summon rule, the quantity row's count,
    /// the level 5 slot spent exactly once;</item>
    /// <item>a retained projected choice after that slot is spent, and a
    /// forbidden-alignment creature under the same row, refused with nothing
    /// spent;</item>
    /// <item>the setting off (CotW's in-memory settings value, restored): the
    /// same popup has no additional rows and the ordinary rows unchanged.</item>
    /// </list>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const string Pol06Projected = "pol06-cotw-additional-projected";
        private const string Pol06Raw = "pol06-cotw-additional-raw";

        /// <summary>Classifies a popup row against one prepared source: a child
        /// projected from a Call of the Wild metamagic conversion row, such a row
        /// left raw, or neither (null).</summary>
        private static Func<AbilityData, string> Pol06Classify(AbilityData source)
        {
            return row =>
            {
                if (row == null || row.Blueprint == null) return null;
                AbilityData conversion = row.ConvertedFrom;
                if (conversion != null && ReferenceEquals(conversion.ConvertedFrom, source) && Pol06HasMetamagic(conversion) &&
                    SummonNativeOptionCatalog.All.Any(value => value.Guid == conversion.Blueprint?.AssetGuid)) return Pol06Projected;
                if (ReferenceEquals(conversion, source) && Pol06HasMetamagic(row) &&
                    SummonNativeOptionCatalog.All.Any(value => value.Guid == row.Blueprint.AssetGuid)) return Pol06Raw;
                return null;
            };
        }

        private static bool Pol06HasMetamagic(AbilityData data)
        { return data?.MetamagicData != null && data.MetamagicData.MetamagicMask != 0; }

        /// <summary>Call of the Wild's live settings object and the backing field
        /// of its "metamagic for spontaneous spell conversion" value, or null.</summary>
        private static Tuple<object, FieldInfo> Pol06CotwSetting()
        {
            Assembly cotw = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.GetName().Name == "CallOfTheWild");
            Type main = cotw?.GetType("CallOfTheWild.Main");
            FieldInfo settingsField = main?.GetField("settings", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            object settings = settingsField?.GetValue(null);
            FieldInfo field = settings?.GetType().GetField("<metamagic_for_spontaneous_spell_conversion>k__BackingField",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return settings == null || field == null || field.FieldType != typeof(bool) ? null : Tuple.Create(settings, field);
        }

        /// <summary>Structural verdict for the additional children of one popup.</summary>
        private static JObject Pol06AdditionalVerdict(AlphaMenu menu, AbilityData source, int sourceLevel, int additionalTier,
            BlueprintAbility[] ordinaryExpected, BlueprintAbility[] additionalExpected, out bool exact)
        {
            Func<AbilityData, string> classify = Pol06Classify(source);
            AbilityData[] data = menu == null ? new AbilityData[0] : menu.Data;
            int[] projectedIndices = Enumerable.Range(0, data.Length).Where(index => classify(data[index]) == Pol06Projected).ToArray();
            int[] rawIndices = Enumerable.Range(0, data.Length).Where(index => classify(data[index]) == Pol06Raw).ToArray();
            int[] ordinaryIndices = Enumerable.Range(0, data.Length).Where(index => data[index] != null && classify(data[index]) == null).ToArray();
            AbilityData[] projected = projectedIndices.Select(index => data[index]).ToArray();
            var problems = new JArray();
            foreach (AbilityData child in projected)
            {
                var receipt = DruidSummonConversionVariantPatch.For(child);
                int tier; SummonMultiplicity quantity;
                bool resolved = receipt != null && DruidSummonConversionPolicy.TryResolve(receipt.Route, child.ConvertedFrom.Blueprint.AssetGuid, out tier, out quantity) && tier == additionalTier;
                bool ok = receipt != null && receipt.Current && receipt.Additional && receipt.AdditionalTier == additionalTier && resolved &&
                    ReferenceEquals(receipt.Source, source) && ReferenceEquals(child.ConvertedFrom, receipt.Conversion) &&
                    ReferenceEquals(child.MetamagicData, child.ConvertedFrom.MetamagicData) && child.MetamagicData.Has(Kingmaker.UnitLogic.Abilities.Metamagic.Extend) &&
                    child.SpellLevel == sourceLevel && ReferenceEquals(child.Spellbook, source.Spellbook) && ReferenceEquals(child.Caster, source.Caster) &&
                    receipt.ProjectionVariants.Any(value => ReferenceEquals(value, child.Blueprint));
                if (!ok) problems.Add(new JObject { ["child"] = child.Blueprint.AssetGuid, ["name"] = child.Blueprint.Name, ["receipt"] = receipt != null,
                    ["current"] = receipt?.Current, ["additionalTier"] = receipt?.AdditionalTier, ["spellLevel"] = child.SpellLevel,
                    ["metamagic"] = child.MetamagicData?.MetamagicMask.ToString(), ["sameMetamagicObject"] = ReferenceEquals(child.MetamagicData, child.ConvertedFrom?.MetamagicData) });
            }
            BlueprintAbility[] ordinary = ordinaryIndices.Select(index => data[index].Blueprint).ToArray();
            bool ordinaryExact = ordinary.SequenceEqual(ordinaryExpected);
            bool setExact = projected.Length == additionalExpected.Length && projected.Select(value => value.Blueprint).Distinct().Count() == projected.Length &&
                new HashSet<BlueprintAbility>(projected.Select(value => value.Blueprint)).SetEquals(additionalExpected);
            bool afterOrdinary = projectedIndices.Length == 0 || ordinaryIndices.Length == 0 || projectedIndices.Min() > ordinaryIndices.Max();
            exact = menu != null && rawIndices.Length == 0 && problems.Count == 0 && ordinaryExact && setExact && afterOrdinary && projected.Length > 0;
            return new JObject { ["rows"] = data.Length, ["ordinary"] = ordinary.Length, ["ordinaryExpected"] = ordinaryExpected.Length, ["ordinaryExact"] = ordinaryExact,
                ["projected"] = projected.Length, ["projectedExpected"] = additionalExpected.Length, ["projectedSetExact"] = setExact, ["raw"] = rawIndices.Length,
                ["projectedAfterOrdinary"] = afterOrdinary, ["problems"] = problems, ["sourceLevel"] = sourceLevel, ["additionalTier"] = additionalTier,
                ["quantityRows"] = new JArray(projected.Select(value => value.ConvertedFrom.Blueprint.name).Distinct()),
                ["missing"] = new JArray(additionalExpected.Except(projected.Select(value => value.Blueprint)).Select(value => value.Name)),
                ["unexpected"] = new JArray(projected.Select(value => value.Blueprint).Except(additionalExpected).Select(value => value.Name)),
                ["conversionAdapter"] = DruidSummonConversionVariantPatch.Outcome };
        }

        /// <summary>Prepared slots of the source's level whose spell the native
        /// spend would treat as this source (AbilityData equality).</summary>
        private static int Pol06EqualPreparedCopies(Spellbook book, AbilityData source)
        {
            return book.GetMemorizedSpellSlots(book.GetSpellLevel(source)).Count(slot => slot?.Spell != null && source.Equals(slot.Spell));
        }

        private static Func<int, bool> Pol06CountFor(AbilityData child, SummonConversionRoute route)
        {
            int tier; SummonMultiplicity quantity;
            if (child?.ConvertedFrom == null || !DruidSummonConversionPolicy.TryResolve(route, child.ConvertedFrom.Blueprint.AssetGuid, out tier, out quantity)) return count => false;
            if (quantity == SummonMultiplicity.One) return count => count == 1;
            if (quantity == SummonMultiplicity.OneD3) return count => count >= 1 && count <= 3;
            return count => count >= 2 && count <= 5;
        }

        private IEnumerable<int> Pol06HeraldCells(ExpandedSummoningCorrectionFixture fixture, Spellbook book, Cleanup2NativeDefaultScope mode,
            ActionBarGroupElement spellGroup, ActionBarSubGroupLevels levels, Func<int, BlueprintAbility> smParent, Func<SummonFamily, string, BlueprintAbility> original)
        {
            var watch = Stopwatch.StartNew();
            var setting = Pol06CotwSetting();
            bool settingOn = setting != null && (bool)setting.Item2.GetValue(setting.Item1);
            Sprint25Check("pol06-cotw-setting-present", setting != null && settingOn, new JObject { ["settingFound"] = setting != null, ["value"] = settingOn },
                "Call of the Wild is loaded in this profile and its metamagic-for-spontaneous-conversion setting is on.");
            if (setting == null || !settingOn) yield break;

            // A level 4 spell no earlier cell prepared: the native spend takes the
            // last available slot whose spell equals the source, so a second
            // identical extended copy (the extended-source cell's, index 0) would
            // be spent instead and keep this source's slot available.
            BlueprintAbility unrelatedFour = SpiderPlainSpell(book, 4, 1, true);
            AbilityData source = SpiderPrepareExtended(book, unrelatedFour, 4);
            book.Rest();
            int sourceLevel = book.GetSpellLevel(source);
            int equalCopies = Pol06EqualPreparedCopies(book, source);
            Sprint25Check("pol06-herald-source-unique", equalCopies == 1, new JObject { ["sourceSpell"] = unrelatedFour.name + ":" + unrelatedFour.AssetGuid,
                ["equalPreparedCopies"] = equalCopies, ["slotLevel"] = source.ParamSpellSlot?.SpellLevel },
                "The Herald POL-06 source is the only prepared slot whose spell equals it, so its debit and staleness are exact.");
            BlueprintAbility[] ordinaryExpected = SpiderPermitted(smParent(5), book);
            BlueprintAbility[] additionalExpected = SpiderPermitted(smParent(4), book);
            // Owned generated rows only: like the phase's forbidden selected child,
            // the gate hides them and refuses them (visible false, unavailable).
            BlueprintAbility[] forbidden = (smParent(4).Variants ?? new BlueprintAbility[0]).Where(value => value.name != null &&
                value.name.StartsWith("KMG_Summoning_Ability_", StringComparison.Ordinal) &&
                !SummonClericAlignmentRuntime.PermitsRow(value, book.Owner, book)).OrderBy(value => value.name, StringComparer.Ordinal).ToArray();

            AlphaMenu menu = null; ActionBarGroupSlot popupSource = null;
            foreach (int frame in R6Cell("pol06-herald-cotw-on-popup", () => ClericOpenMenu("pol06-herald-cotw-on-tier5", spellGroup, levels, 5, book, smParent(5), source, mode, watch, null,
                (value, slot) => { menu = value; popupSource = slot; }, false, ordinaryExpected, Pol06Classify(source)))) yield return frame;
            bool exact;
            JObject verdict = Pol06AdditionalVerdict(menu, source, sourceLevel, 4, ordinaryExpected, additionalExpected, out exact);
            verdict["sourceSpell"] = unrelatedFour.name + ":" + unrelatedFour.AssetGuid; verdict["sourceMetamagic"] = source.MetamagicData?.MetamagicMask.ToString();
            verdict["forbiddenForThisCaster"] = new JArray(forbidden.Select(value => value.Name));
            Sprint25Check("pol06-herald-additional-projection", exact && sourceLevel == 5, verdict,
                "Over an unrelated level 4 spell prepared with Extend Spell (level 5 slot), the real Herald popup keeps the ordinary Summon Monster V rows exactly and projects Call of the Wild's " +
                "Summon Monster IV metamagic rows into exactly this caster's permitted Summon Monster IV creatures, after them, each carrying that row's own Extend at spell level 5; none stays raw.");

            AbilityData[] projected = menu == null ? new AbilityData[0] : menu.Data.Where(value => Pol06Classify(source)(value) == Pol06Projected).ToArray();
            BlueprintAbility widow = original(SummonFamily.Monster, "giant-black-widow");
            AbilityData chosen = projected.FirstOrDefault(value => ReferenceEquals(value.Blueprint, widow)) ?? projected.FirstOrDefault();
            AbilityData retained = projected.FirstOrDefault(value => !ReferenceEquals(value, chosen) && ReferenceEquals(value.ConvertedFrom, chosen?.ConvertedFrom));
            int chosenIndex = chosen == null || menu == null ? -1 : Array.IndexOf(menu.Data, chosen);
            if (chosenIndex >= 0 && popupSource != null)
            {
                var debit = new Cleanup2SourceDebit(fixture.Caster); JArray rules = null; ActionBarGroupSlot close = popupSource;
                foreach (int frame in SpiderSummonRuleScope(() => R6Cell("pol06-herald-additional-cast", () => Cleanup2MenuAdapterDruidCast(fixture, chosen, source, smParent(4),
                    menu.Rows[chosenIndex], debit, mode, watch, () => Cleanup2MenuAdapterHide(close, source), "pol06-herald-additional",
                    Pol06CountFor(chosen, DruidSummonConversionPolicy.Herald))), value => rules = value)) yield return frame;
                popupSource = null;
                string problem = SpiderDurationProblem(rules, book.CasterLevel, true);
                Sprint25Check("pol06-herald-additional-extend-duration-debit", problem == null && book.CasterLevel == 20 && source.ParamSpellSlot != null &&
                    source.ParamSpellSlot.SpellLevel == 5 && !source.ParamSpellSlot.Available && chosen.MetamagicData.Has(Kingmaker.UnitLogic.Abilities.Metamagic.Extend),
                    new JObject { ["rules"] = rules, ["problem"] = problem, ["child"] = chosen.Blueprint.Name, ["spider"] = ReferenceEquals(chosen.Blueprint, widow),
                        ["quantityRow"] = chosen.ConvertedFrom.Blueprint.name, ["childMetamagic"] = chosen.MetamagicData?.MetamagicMask.ToString(), ["childSpellLevel"] = chosen.SpellLevel,
                        ["sourceSlotLevel"] = source.ParamSpellSlot?.SpellLevel, ["slotSpent"] = source.ParamSpellSlot != null && !source.ParamSpellSlot.Available },
                    "The projected Summon Monster IV choice (the Black Widow where offered) keeps Call of the Wild's Extend: the engine's summon rule gives 40 rounds at caster level 20, " +
                    "its quantity row's count is summoned, and the level 5 slot is the one spent.");

                if (retained != null)
                    foreach (int frame in R6Cell("pol06-herald-stale-refused", () => ClericRefusedAttempt(fixture, retained, source, mode, watch, "pol06-herald-stale-choice-refused", "pol06-stale",
                        true, "a projected choice retained from the popup, after its source slot was spent; native UnitUseAbility",
                        "A projected Call of the Wild choice retained after its source slot was spent has no cast left and is refused with nothing spent and nothing summoned.",
                        true))) yield return frame;
                else Sprint25Check("pol06-herald-stale-choice-present", false, new JObject { ["projected"] = projected.Length }, "A second projected choice of the same row exists to retain.");
            }
            else Sprint25Check("pol06-herald-additional-row-present", false, new JObject { ["rows"] = menu?.Rows.Length ?? 0, ["projected"] = projected.Length },
                "A projected Call of the Wild choice is offered.");
            if (popupSource != null) Cleanup2MenuAdapterHide(popupSource, source);
            book.Rest();

            // An evil Summon Monster IV creature this chaotic good Herald may not
            // call, built under the same Call of the Wild row as a saved shortcut would hold it.
            AbilityData row = chosen?.ConvertedFrom;
            if (row != null && forbidden.Length > 0)
            {
                var forbiddenChild = new AbilityData(forbidden[0], book) { ConvertedFrom = row };
                foreach (int frame in R6Cell("pol06-herald-forbidden-refused", () => ClericRefusedAttempt(fixture, forbiddenChild, source, mode, watch, "pol06-herald-forbidden-choice-refused",
                    "pol06-forbidden", false, "a forbidden-alignment Summon Monster IV creature under the Call of the Wild Extend row; native UnitUseAbility",
                    "A forbidden-alignment creature under a Call of the Wild metamagic row is refused before any slot is spent, and nothing is summoned."))) yield return frame;
                book.Rest();
            }
            else Sprint25Check("pol06-herald-forbidden-choice-present", false, new JObject { ["row"] = row != null, ["forbidden"] = forbidden.Length },
                "A forbidden-alignment Summon Monster IV creature exists for this caster.");

            // The setting off: CotW reads this value on every popup fill.
            setting.Item2.SetValue(setting.Item1, false);
            try
            {
                AlphaMenu offMenu = null; ActionBarGroupSlot offSource = null;
                foreach (int frame in R6Cell("pol06-herald-cotw-off-popup", () => ClericOpenMenu("pol06-herald-cotw-off-tier5", spellGroup, levels, 5, book, smParent(5), source, mode, watch, null,
                    (value, slot) => { offMenu = value; offSource = slot; }, false, ordinaryExpected, Pol06Classify(source)))) yield return frame;
                int additionalRows = offMenu == null ? -1 : offMenu.Data.Count(value => Pol06Classify(source)(value) != null);
                Sprint25Check("pol06-herald-cotw-off-ordinary-only", offMenu != null && additionalRows == 0 && offMenu.Data.Select(value => value?.Blueprint).SequenceEqual(ordinaryExpected),
                    new JObject { ["rows"] = offMenu?.Rows.Length ?? 0, ["additionalRows"] = additionalRows, ["ordinaryExpected"] = ordinaryExpected.Length,
                        ["conversionAdapter"] = DruidSummonConversionVariantPatch.Outcome, ["toggle"] = "CallOfTheWild.Main.settings in-memory value; settings file untouched" },
                    "With Call of the Wild's setting off, the same Herald popup lists exactly the ordinary Summon Monster V rows and no additional row.");
                if (offSource != null) Cleanup2MenuAdapterHide(offSource, source);
            }
            finally { setting.Item2.SetValue(setting.Item1, true); }
            book.Rest();
        }

        private IEnumerable<int> Pol06DruidCells(ExpandedSummoningCorrectionFixture fixture, SpiderInterfaceCasters casters, Cleanup2NativeDefaultScope mode,
            ActionBarGroupElement spellGroup, ActionBarSubGroupLevels levels, Func<int, BlueprintAbility> snaParent)
        {
            var watch = Stopwatch.StartNew();
            Spellbook book = casters.DruidBook;
            if (book == null || Pol06CotwSetting() == null) { Sprint25Check("pol06-druid-prerequisites", false, casters.Setup, "The Druid and Call of the Wild's setting exist."); yield break; }
            BlueprintAbility unrelatedFour = SpiderPlainSpell(book, 4, 0, true);
            AbilityData source = SpiderPrepareExtended(book, unrelatedFour, 4);
            book.Rest();
            int sourceLevel = book.GetSpellLevel(source);
            int equalCopies = Pol06EqualPreparedCopies(book, source);
            Sprint25Check("pol06-druid-source-unique", equalCopies == 1, new JObject { ["sourceSpell"] = unrelatedFour.name + ":" + unrelatedFour.AssetGuid,
                ["equalPreparedCopies"] = equalCopies }, "The Druid POL-06 source is the only prepared slot whose spell equals it.");
            BlueprintAbility[] ordinaryExpected = SpiderPermitted(snaParent(5), book);
            BlueprintAbility[] additionalExpected = SpiderPermitted(snaParent(4), book);
            foreach (int frame in SpiderInterfaceSelect(casters.Druid)) yield return frame;
            AlphaMenu menu = null; ActionBarGroupSlot popupSource = null;
            foreach (int frame in R6Cell("pol06-druid-cotw-on-popup", () => ClericOpenMenu("pol06-druid-cotw-on-tier5", spellGroup, levels, 5, book, snaParent(5), source, mode, watch, null,
                (value, slot) => { menu = value; popupSource = slot; }, false, ordinaryExpected, Pol06Classify(source)))) yield return frame;
            bool exact;
            JObject verdict = Pol06AdditionalVerdict(menu, source, sourceLevel, 4, ordinaryExpected, additionalExpected, out exact);
            verdict["sourceSpell"] = unrelatedFour.name + ":" + unrelatedFour.AssetGuid;
            Sprint25Check("pol06-druid-additional-projection", exact && sourceLevel == 5, verdict,
                "The Druid's popup over an unrelated level 4 spell prepared with Extend Spell keeps the ordinary Summon Nature's Ally V rows exactly and projects Call of the Wild's " +
                "Summon Nature's Ally IV metamagic rows into the published Summon Nature's Ally IV creatures, each carrying that row's Extend at spell level 5; none stays raw.");
            AbilityData chosen = menu == null ? null : menu.Data.FirstOrDefault(value => Pol06Classify(source)(value) == Pol06Projected);
            int index = chosen == null ? -1 : Array.IndexOf(menu.Data, chosen);
            if (index >= 0 && popupSource != null)
            {
                UnitEntityData fixtureCaster = fixture.Caster;
                var debit = new Cleanup2SourceDebit(casters.Druid); JArray rules = null; ActionBarGroupSlot close = popupSource;
                fixture.Caster = casters.Druid;
                try
                {
                    foreach (int frame in SpiderSummonRuleScope(() => R6Cell("pol06-druid-additional-cast", () => Cleanup2MenuAdapterDruidCast(fixture, chosen, source, snaParent(4), menu.Rows[index],
                        debit, mode, watch, () => Cleanup2MenuAdapterHide(close, source), "pol06-druid-additional", Pol06CountFor(chosen, DruidSummonConversionPolicy.Druid))), value => rules = value)) yield return frame;
                }
                finally { fixture.Caster = fixtureCaster; }
                popupSource = null;
                string problem = SpiderDurationProblem(rules, book.CasterLevel, true);
                Sprint25Check("pol06-druid-additional-extend-duration-debit", problem == null && source.ParamSpellSlot != null && source.ParamSpellSlot.SpellLevel == 5 && !source.ParamSpellSlot.Available,
                    new JObject { ["rules"] = rules, ["problem"] = problem, ["child"] = chosen.Blueprint.Name, ["quantityRow"] = chosen.ConvertedFrom.Blueprint.name,
                        ["casterLevel"] = book.CasterLevel, ["childSpellLevel"] = chosen.SpellLevel, ["slotSpent"] = source.ParamSpellSlot != null && !source.ParamSpellSlot.Available },
                    "The projected Summon Nature's Ally IV choice keeps Call of the Wild's Extend (twice the Druid's caster level in rounds), summons its quantity row's count and spends the level 5 slot.");
            }
            else Sprint25Check("pol06-druid-additional-row-present", false, new JObject { ["rows"] = menu?.Rows.Length ?? 0 }, "A projected Druid choice is offered.");
            if (popupSource != null) Cleanup2MenuAdapterHide(popupSource, source);
            book.Rest();
            foreach (int frame in SpiderInterfaceSelect(fixture.Caster)) yield return frame;
        }
    }
}
