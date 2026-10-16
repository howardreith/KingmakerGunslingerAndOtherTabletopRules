using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>A context value as the engine evaluates it: kind, constant,
    /// rank slot, shared slot and properties.</summary>
    internal sealed class SummonValue
    {
        internal SummonValue(string kind, int value, string rank, string shared,
            string property, string customProperty)
        {
            Kind = kind ?? ""; Value = value; Rank = rank ?? ""; Shared = shared ?? "";
            Property = property ?? ""; CustomProperty = customProperty ?? "";
        }

        internal string Kind { get; private set; }
        internal int Value { get; private set; }
        internal string Rank { get; private set; }
        internal string Shared { get; private set; }
        internal string Property { get; private set; }
        internal string CustomProperty { get; private set; }

        internal static SummonValue Simple(int value)
        { return new SummonValue("Simple", value, "Default", "Damage", "None", ""); }

        internal static SummonValue RankOf(string rank)
        { return new SummonValue("Rank", 0, rank, "Damage", "None", ""); }

        /// <summary>Only the field the engine reads for this kind, so a
        /// default left in an unread slot is never a difference.</summary>
        public override string ToString()
        {
            switch (Kind)
            {
                case "Simple": return "Simple(" + Value + ")";
                case "Rank": return "Rank(" + Rank + ")";
                case "Shared": return "Shared(" + Shared + ")";
                default:
                    return Kind + "(" + Value + "," + Rank + "," + Shared + "," +
                        Property + "," + CustomProperty + ")";
            }
        }
    }

    /// <summary>The one rendering of a component or condition's public
    /// fields, used by both the reader and the contracts.</summary>
    internal static class SummonSignatureFormat
    {
        internal static string Component(string type,
            IEnumerable<KeyValuePair<string, string>> fields)
        {
            return type + "{" + string.Join(";", (fields ??
                Enumerable.Empty<KeyValuePair<string, string>>())
                .OrderBy(value => value.Key, StringComparer.Ordinal)
                .Select(value => value.Key + "=" + value.Value).ToArray()) + "}";
        }

        internal static KeyValuePair<string, string> Field(string name, string value)
        { return new KeyValuePair<string, string>(name, value); }

        internal static string List(IEnumerable<string> values)
        { return "[" + string.Join(",", values.ToArray()) + "]"; }
    }

    /// <summary>One spawn's significant fields. Its unit is the only one an
    /// adapter replaces.</summary>
    internal sealed class SummonSpawn
    {
        internal string Unit;
        internal string Pool;
        internal string CountDice;
        internal SummonValue CountDiceCount;
        internal SummonValue CountBonus;
        internal string DurationRate;
        internal string DurationDice;
        internal SummonValue DurationDiceCount;
        internal SummonValue DurationBonus;
        internal bool DurationExtendable;
        internal SummonValue Level;
        internal bool DoNotLinkToCaster = false;
        internal bool DirectlyControllable = false;
        internal List<string> AfterSpawn = new List<string>();

        internal SummonSpawn Clone()
        {
            var copy = (SummonSpawn)MemberwiseClone();
            copy.AfterSpawn = new List<string>(AfterSpawn);
            return copy;
        }

        /// <summary>Creature and quantity alone: what the native lineage of a
        /// tier and quantity fixes.</summary>
        internal string Quantity()
        {
            return "unit=" + Unit + ";count=" + CountDice + "x" + CountDiceCount +
                "+" + CountBonus + ";level=" + Level + ";link=" + !DoNotLinkToCaster +
                ";control=" + DirectlyControllable;
        }

        public override string ToString()
        {
            return Quantity() + ";pool=" + Pool + ";duration=" + DurationRate + ":" +
                DurationDice + "x" + DurationDiceCount + "+" + DurationBonus +
                ";extendable=" + DurationExtendable + ";after=[" +
                string.Join(",", AfterSpawn.ToArray()) + "]";
        }
    }

    internal enum SummonNodeKind { Spawn, Clear, Conditional, Other }

    /// <summary>A node of a summon ability's run action: spawns, pool clears
    /// and conditionals are significant; anything else is kept as its exact
    /// type so an added or removed action is still a difference.</summary>
    internal sealed class SummonNode
    {
        internal SummonNodeKind Kind;
        internal SummonSpawn Spawn;
        internal string Pool;
        internal string Operation;
        internal List<string> Conditions = new List<string>();
        internal List<SummonNode> IfTrue = new List<SummonNode>();
        internal List<SummonNode> IfFalse = new List<SummonNode>();
        internal string Type;

        internal static SummonNode ForSpawn(SummonSpawn spawn)
        { return new SummonNode { Kind = SummonNodeKind.Spawn, Spawn = spawn }; }

        internal static SummonNode ForClear(string pool)
        { return new SummonNode { Kind = SummonNodeKind.Clear, Pool = pool }; }

        internal static SummonNode ForConditional(string operation,
            IEnumerable<string> conditions, IEnumerable<SummonNode> ifTrue,
            IEnumerable<SummonNode> ifFalse)
        {
            return new SummonNode { Kind = SummonNodeKind.Conditional,
                Operation = operation, Conditions = conditions.ToList(),
                IfTrue = ifTrue.ToList(), IfFalse = ifFalse.ToList() };
        }

        internal static SummonNode ForOther(string type)
        { return new SummonNode { Kind = SummonNodeKind.Other, Type = type }; }

        internal SummonNode Clone()
        {
            return new SummonNode {
                Kind = Kind, Spawn = Spawn == null ? null : Spawn.Clone(),
                Pool = Pool, Operation = Operation,
                Conditions = new List<string>(Conditions),
                IfTrue = IfTrue.Select(value => value.Clone()).ToList(),
                IfFalse = IfFalse.Select(value => value.Clone()).ToList(),
                Type = Type };
        }

        internal void Render(StringBuilder text, string indent)
        {
            switch (Kind)
            {
                case SummonNodeKind.Spawn:
                    text.Append(indent).Append("spawn ").Append(Spawn).Append('\n');
                    break;
                case SummonNodeKind.Clear:
                    text.Append(indent).Append("clear pool=").Append(Pool).Append('\n');
                    break;
                case SummonNodeKind.Conditional:
                    text.Append(indent).Append("if ").Append(Operation).Append(" [")
                        .Append(string.Join(" ; ", Conditions.ToArray())).Append("]\n");
                    foreach (SummonNode node in IfTrue) node.Render(text, indent + "  T ");
                    foreach (SummonNode node in IfFalse) node.Render(text, indent + "  F ");
                    break;
                default:
                    text.Append(indent).Append("other ").Append(Type).Append('\n');
                    break;
            }
        }
    }

    internal sealed class SummonResource
    {
        internal string Resource;
        internal bool Spend;
        internal int Amount;
        internal bool CustomCost;
        internal List<string> IncreasingFacts = new List<string>();

        internal SummonResource Clone()
        {
            var copy = (SummonResource)MemberwiseClone();
            copy.IncreasingFacts = new List<string>(IncreasingFacts);
            return copy;
        }

        public override string ToString()
        {
            return "resource=" + Resource + ";spend=" + Spend + ";amount=" + Amount +
                ";custom=" + CustomCost + ";increasing=[" +
                string.Join(",", IncreasingFacts.ToArray()) + "]";
        }
    }

    internal sealed class SummonRank
    {
        internal string Type;
        internal string BaseValueType;
        internal string Progression;
        internal List<string> Classes = new List<string>();
        internal string Archetype;
        internal string Feature;
        internal List<string> FeatureList = new List<string>();
        internal string Stat;
        internal int StartLevel;
        internal int StepLevel;
        internal bool UseMin;
        internal int Min;
        internal bool UseMax;
        internal int Max;
        internal bool ExceptClasses;
        internal string CustomProperty;
        internal string CustomProgression;
        internal bool BasedOnClassLevel;
        internal bool FeatureListBased;

        internal SummonRank Clone()
        {
            var copy = (SummonRank)MemberwiseClone();
            copy.Classes = new List<string>(Classes);
            copy.FeatureList = new List<string>(FeatureList);
            return copy;
        }

        public override string ToString()
        {
            return "rank " + Type + " base=" + BaseValueType + " progression=" +
                Progression + " classes=[" + string.Join(",", Classes.ToArray()) +
                "] archetype=" + Archetype + " feature=" + Feature + " features=[" +
                string.Join(",", FeatureList.ToArray()) + "] stat=" + Stat +
                " start=" + StartLevel + " step=" + StepLevel + " min=" + UseMin + ":" +
                Min + " max=" + UseMax + ":" + Max + " except=" + ExceptClasses +
                " property=" + CustomProperty + " custom=" + CustomProgression;
        }
    }

    /// <summary>
    /// The significant mechanics of one summon ability, as comparable data:
    /// its type and action, resource and spend, caster conditions, custom
    /// cost, every rank expression, spell traits and the run action's spawn,
    /// pool-clear and conditional structure in order. Incidental engine fields
    /// are not recorded, so equality means the same behavior, not the same
    /// bytes.
    /// </summary>
    internal sealed class SummonSignature
    {
        internal string AbilityType;
        internal string ActionType;
        internal bool FullRoundField;
        internal string Range;
        internal List<SummonResource> Resources = new List<SummonResource>();
        internal List<string> CasterCheckers = new List<string>();
        internal List<string> CostCalculators = new List<string>();
        internal List<SummonRank> Ranks = new List<SummonRank>();
        internal string School;
        internal string Descriptor;
        internal int SpellLists;
        internal int RunActions;
        internal int Variants;
        /// <summary>Spawns the reader found only inside an action container
        /// it does not model. Any such spawn makes a graph unsupported.</summary>
        internal int HiddenSpawns;
        internal List<SummonNode> Actions = new List<SummonNode>();

        internal SummonSignature Clone()
        {
            var copy = (SummonSignature)MemberwiseClone();
            copy.Resources = Resources.Select(value => value.Clone()).ToList();
            copy.CasterCheckers = new List<string>(CasterCheckers);
            copy.CostCalculators = new List<string>(CostCalculators);
            copy.Ranks = Ranks.Select(value => value.Clone()).ToList();
            copy.Actions = Actions.Select(value => value.Clone()).ToList();
            return copy;
        }

        internal IEnumerable<SummonSpawn> Spawns()
        {
            return Walk(Actions).Where(value => value.Kind == SummonNodeKind.Spawn)
                .Select(value => value.Spawn);
        }

        internal IEnumerable<SummonNode> Nodes() { return Walk(Actions); }

        private static IEnumerable<SummonNode> Walk(IEnumerable<SummonNode> nodes)
        {
            foreach (SummonNode node in nodes)
            {
                yield return node;
                foreach (SummonNode child in Walk(node.IfTrue)) yield return child;
                foreach (SummonNode child in Walk(node.IfFalse)) yield return child;
            }
        }

        internal string Render()
        {
            var text = new StringBuilder();
            text.Append("ability type=").Append(AbilityType).Append(" action=")
                .Append(ActionType).Append(" fullRound=").Append(FullRoundField)
                .Append(" range=").Append(Range).Append(" school=").Append(School)
                .Append(" descriptor=").Append(Descriptor).Append(" spellLists=")
                .Append(SpellLists).Append(" runActions=").Append(RunActions)
                .Append(" variants=").Append(Variants).Append(" hiddenSpawns=")
                .Append(HiddenSpawns).Append('\n');
            foreach (SummonResource value in Resources) text.Append(value).Append('\n');
            foreach (string value in CasterCheckers.OrderBy(value => value,
                StringComparer.Ordinal)) text.Append("checker ").Append(value).Append('\n');
            foreach (string value in CostCalculators.OrderBy(value => value,
                StringComparer.Ordinal)) text.Append("cost ").Append(value).Append('\n');
            foreach (string value in Ranks.Select(value => value.ToString()).OrderBy(
                value => value, StringComparer.Ordinal)) text.Append(value).Append('\n');
            foreach (SummonNode node in Actions) node.Render(text, "");
            return text.ToString();
        }

        /// <summary>The first differing rendered line, or null when equal.</summary>
        internal static string Difference(SummonSignature expected,
            SummonSignature actual)
        {
            string[] left = expected.Render().Split('\n');
            string[] right = actual.Render().Split('\n');
            for (int index = 0; index < Math.Max(left.Length, right.Length); index++)
            {
                string a = index < left.Length ? left[index] : "<none>";
                string b = index < right.Length ? right[index] : "<none>";
                if (!string.Equals(a, b, StringComparison.Ordinal))
                    return "expected[" + a + "] actual[" + b + "]";
            }
            return null;
        }
    }
}
