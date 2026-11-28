using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
using UnityModManagerNet;

namespace KingmakerGunslinger.TurnUndead
{
    /// <summary>
    /// Narrow optional Call of the Wild integration, applied once after every
    /// LoadDictionary postfix has run. No CotW type is referenced at compile
    /// time and no CotW channel generator is called, so no quick, swift, smite,
    /// line, cone or other channel transformation is created for Turn Undead.
    ///
    ///  * Menu: each source's own positive-harm variant wrapper (the ability
    ///    granted by the source feature whose AbilityVariants contains that
    ///    source's native harm channel) gains that source's never-granted Turn
    ///    Undead child, appended once; the child's Parent is that wrapper. The
    ///    directly granted action of a source whose child was placed is hidden
    ///    for this process so the option appears once.
    ///  * DC: Improved Channel, Sacred Conduit and Consecrate add their exact
    ///    IncreaseSpecifiedSpellsDC bonus to the Turn Undead identities.
    ///  * Resistance: every CotW-registered channel resistance fact receives
    ///    the identities in its ability-specific save-bonus lists.
    /// Every change is an idempotent append, recorded and exactly reversible.
    /// </summary>
    internal sealed class TurnUndeadCotwPublication
    {
        internal const string ModId = "CallOfTheWild";
        internal const string EngineTypeName = "CallOfTheWild.ChannelEnergyEngine";
        private const BindingFlags Statics = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags Instances = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        internal static readonly string[] DcBonusFields = { "improved_channel", "sacred_conduit", "consecrate_buff" };

        private readonly List<Action> _undo = new List<Action>();
        private readonly List<string> _evidence = new List<string>();

        internal IReadOnlyList<string> Evidence { get { return _evidence; } }
        internal string Availability { get; private set; }
        internal int MenuChildren { get; private set; }

        internal static TurnUndeadCotwPublication Publish(TurnUndeadBlueprintSet set,
            UnityModManager.ModEntry self)
        {
            if (set == null) throw new ArgumentNullException("set");
            var result = new TurnUndeadCotwPublication();
            UnityModManager.ModEntry[] entries = ReadEntries(self).Where(entry =>
                entry.Info != null && string.Equals(entry.Info.Id, ModId, StringComparison.Ordinal)).ToArray();
            if (entries.Length == 0) { result.Availability = "absent"; return result; }
            if (entries.Length != 1)
                throw new InvalidOperationException("Expected at most one live Call of the Wild entry.");
            UnityModManager.ModEntry cotw = entries[0];
            if (!cotw.Loaded || !cotw.Active || !cotw.HasAssembly || cotw.ErrorOnLoading ||
                cotw.Assembly == null)
            {
                result.Availability = "inactive";
                return result;
            }
            Type engine = cotw.Assembly.GetType(EngineTypeName, false, false);
            if (engine == null)
                throw new InvalidOperationException("Call of the Wild " + (cotw.Info.Version ?? "?") +
                    " has no " + EngineTypeName + "; contract drift.");
            result.Availability = "present " + (cotw.Info.Version ?? "?");
            try
            {
                foreach (TurnUndeadSourceBlueprints source in set.Sources) result.PlaceMenuChild(source);
                foreach (string name in DcBonusFields) result.AppendDcBonus(engine, name, set);
                result.AppendResistances(engine, set);
            }
            catch
            {
                result.Rollback();
                throw;
            }
            return result;
        }

        private void PlaceMenuChild(TurnUndeadSourceBlueprints source)
        {
            BlueprintAbility[] wrappers = (source.SourceFeature.ComponentsArray ?? new BlueprintComponent[0])
                .OfType<AddFacts>().Where(grant => grant.Facts != null)
                .SelectMany(grant => grant.Facts).OfType<BlueprintAbility>()
                .Where(ability =>
                {
                    AbilityVariants variants = ability.GetComponent<AbilityVariants>();
                    return variants != null && variants.Variants != null &&
                        variants.Variants.Contains(source.NativeHarm);
                })
                .Distinct().ToArray();
            if (wrappers.Length == 0)
            {
                _evidence.Add(source.Contract.Key + ":menu=none;direct-visible");
                return;
            }
            if (wrappers.Length != 1)
                throw new InvalidOperationException("Ambiguous positive-harm wrapper for " +
                    source.Contract.Key + ".");
            BlueprintAbility wrapper = wrappers[0];
            AbilityVariants component = wrapper.GetComponent<AbilityVariants>();
            BlueprintAbility child = source.MenuChild;
            if (child.Parent != null && !ReferenceEquals(child.Parent, wrapper))
                throw new InvalidOperationException("The Turn Undead child of " + source.Contract.Key +
                    " already belongs to another wrapper.");
            BlueprintAbility[] before = component.Variants;
            BlueprintAbility[] after = TurnUndeadListPolicy.AppendMissing(before, new[] { child });
            BlueprintAbility priorParent = child.Parent;
            bool priorHidden = source.Direct.Hidden;
            if (after != null) component.Variants = after;
            child.Parent = wrapper;
            source.Direct.Hidden = true;
            _undo.Add(() =>
            {
                if (after != null) component.Variants = before;
                child.Parent = priorParent;
                source.Direct.Hidden = priorHidden;
            });
            MenuChildren++;
            _evidence.Add(source.Contract.Key + ":menu=" + wrapper.AssetGuid + ":" + wrapper.name +
                ";variants=" + (before == null ? 0 : before.Length) + "->" + component.Variants.Length +
                ";direct-hidden");
        }

        private void AppendDcBonus(Type engine, string fieldName, TurnUndeadBlueprintSet set)
        {
            FieldInfo field = engine.GetField(fieldName, Statics);
            bool typeMatches = field != null && typeof(BlueprintUnitFact).IsAssignableFrom(field.FieldType);
            BlueprintUnitFact holder = typeMatches ? field.GetValue(null) as BlueprintUnitFact : null;
            BlueprintComponent[] bonuses = holder == null ? new BlueprintComponent[0] :
                (holder.ComponentsArray ?? new BlueprintComponent[0]).Where(component =>
                    component != null && component.GetType().Name == "IncreaseSpecifiedSpellsDC").ToArray();
            switch (TurnUndeadCotwSeamPolicy.Classify(field != null, typeMatches, holder != null, bonuses.Length))
            {
                case TurnUndeadSeamState.Drift:
                    throw new InvalidOperationException("Call of the Wild DC seam " + fieldName + " drifted (" +
                        (field == null ? "field missing" : !typeMatches ? "field type " + field.FieldType.Name :
                            "no IncreaseSpecifiedSpellsDC on " + holder.name) + "): its DC bonus would not apply to Turn Undead.");
                case TurnUndeadSeamState.OptionalAbsent:
                    _evidence.Add(fieldName + "=absent");
                    return;
            }
            int changed = 0;
            foreach (BlueprintComponent component in bonuses) changed += AppendAbilities(component, "spells", set);
            _evidence.Add(fieldName + "=" + holder.AssetGuid + ":+" + changed);
        }

        private void AppendResistances(Type engine, TurnUndeadBlueprintSet set)
        {
            FieldInfo field = engine.GetField("channel_resistances", Statics);
            bool typeMatches = field != null && typeof(IEnumerable<BlueprintUnitFact>).IsAssignableFrom(field.FieldType);
            IEnumerable<BlueprintUnitFact> facts = typeMatches ? field.GetValue(null) as IEnumerable<BlueprintUnitFact> : null;
            if (TurnUndeadCotwSeamPolicy.Classify(field != null, typeMatches, facts != null, 1) == TurnUndeadSeamState.Drift)
                throw new InvalidOperationException("Call of the Wild seam channel_resistances drifted (" +
                    (field == null ? "field missing" : "field type " + field.FieldType.Name) +
                    "): CotW channel resistance would not apply to Turn Undead.");
            if (facts == null) { _evidence.Add("channel_resistances=absent"); return; }
            foreach (BlueprintUnitFact fact in facts.Where(value => value != null))
            {
                BlueprintComponent[] lists = (fact.ComponentsArray ?? new BlueprintComponent[0]).Where(component =>
                    component is SavingThrowBonusAgainstSpecificSpells ||
                    component != null && component.GetType().Name == "ContextSavingThrowBonusAgainstSpecificSpells").ToArray();
                if (TurnUndeadCotwSeamPolicy.Classify(true, true, true, lists.Length) == TurnUndeadSeamState.Drift)
                    throw new InvalidOperationException("Call of the Wild channel resistance " + fact.name + " (" +
                        fact.AssetGuid + ") has no ability-specific save-bonus list: its resistance would not apply to Turn Undead.");
                int changed = 0;
                foreach (BlueprintComponent component in lists) changed += AppendAbilities(component, "Spells", set);
                _evidence.Add("resistance=" + fact.AssetGuid + ":+" + changed);
            }
        }

        private int AppendAbilities(object component, string fieldName, TurnUndeadBlueprintSet set)
        {
            FieldInfo field = component.GetType().GetField(fieldName, Instances);
            if (field == null || field.FieldType != typeof(BlueprintAbility[]))
                throw new InvalidOperationException("Call of the Wild component " + component.GetType().FullName +
                    " lacks its BlueprintAbility[] " + fieldName + " list (contract drift).");
            var before = (BlueprintAbility[])field.GetValue(component);
            BlueprintAbility[] after = TurnUndeadListPolicy.AppendMissing(before, set.AllAbilities.ToArray());
            if (after == null) return 0;
            field.SetValue(component, after);
            _undo.Add(() => field.SetValue(component, before));
            return after.Length - (before == null ? 0 : before.Length);
        }

        internal void Rollback()
        {
            for (int index = _undo.Count - 1; index >= 0; index--) _undo[index]();
            _undo.Clear();
        }

        private static IEnumerable<UnityModManager.ModEntry> ReadEntries(UnityModManager.ModEntry self)
        {
            Type manager = self == null ? null : self.GetType().DeclaringType;
            FieldInfo field = manager == null ? null : manager.GetField("modEntries", Statics);
            IEnumerable values = field == null ? null : field.GetValue(null) as IEnumerable;
            if (values == null)
                throw new InvalidOperationException("The live UMM modEntries collection was unavailable.");
            return values.Cast<object>().Select(value => value as UnityModManager.ModEntry)
                .Where(value => value != null).ToArray();
        }
    }
}
