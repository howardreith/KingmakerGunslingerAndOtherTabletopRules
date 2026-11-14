using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    // A presentation selection owns no spell, slot, resource or fallback rule.
    // The original native source is the authority on every commit.
    internal sealed class ElementalSourceSelection<T> where T : class
    {
        private readonly T[] _sources;
        private readonly Func<bool> _current;
        private readonly Func<T, string> _validate;
        private readonly Action<T> _dispatch;
        private T _selected;
        internal string Outcome { get; private set; }
        internal bool Closed { get; private set; }
        internal T Selected { get { return _selected; } }
        internal IReadOnlyList<T> Sources { get { return Array.AsReadOnly(_sources); } }

        internal ElementalSourceSelection(IEnumerable<T> sources, Func<bool> current,
            Func<T, string> validate, Action<T> dispatch)
        {
            _sources = sources.ToArray(); _current = current; _validate = validate; _dispatch = dispatch;
            if (_sources.Any(value => value == null)) throw new ArgumentException("Null source.");
            Outcome = "choose-source";
        }

        internal bool Select(T source)
        {
            if (Closed || !_current() || !_sources.Any(value => ReferenceEquals(value, source)))
            { Outcome = "stale-or-foreign-source"; return false; }
            _selected = source; Outcome = "source-selected-no-spend"; return true;
        }

        internal bool Cast()
        {
            if (Closed || !_current()) { Outcome = "stale-presentation"; return false; }
            if (_selected == null) { Outcome = "choose-source"; return false; }
            string reason = _validate(_selected);
            if (reason != null) { Outcome = reason; return false; }
            // Mark closed before dispatch: repeated submit cannot hand off twice.
            Closed = true; Outcome = "native-source-handed-off"; _dispatch(_selected); return true;
        }

        internal void ClearSelection()
        { if (!Closed) { _selected = null; Outcome = "choose-source"; } }

        internal void Cancel() { Closed = true; _selected = null; Outcome = "cancelled-no-spend"; }
    }

    internal static class ElementalSourceChooserPolicy
    {
        internal static bool RetainedPreparationAvailable<T>(T retained,T current,bool available) where T:class
        {return retained!=null && ReferenceEquals(retained,current) && available;}

        internal static T NativeDebitPreparation<T>(IEnumerable<T> nativeOrder,Func<T,bool> available,Func<T,bool> matches) where T:class
        {return nativeOrder.LastOrDefault(value=>value!=null && available(value) && matches(value));}

        internal static int SourceRank(string guid)
        {
            var spec=StandaloneElementalPresentationPolicy.All.FirstOrDefault(value=>value.IsParent &&
                (value.Guid==guid || value.MonsterParentGuid==guid || value.NaturesAllyParentGuid==guid));
            return spec==null?0:spec.CanonicalTier;
        }
        internal static readonly int[] Ranks = { 2, 4, 5, 6, 7, 8 };
        internal static readonly string[] Elements = { "air", "earth", "fire", "water" };

        internal static StandaloneElementalAbilitySpec Parent(string guid)
        { return StandaloneElementalPresentationPolicy.All.SingleOrDefault(value => value.IsParent && value.Guid == guid); }

        internal static StandaloneElementalAbilitySpec Rank(int tier)
        { return StandaloneElementalPresentationPolicy.All.SingleOrDefault(value => value.IsParent && value.CanonicalTier == tier); }

        internal static string NativeCreature(string parentGuid, string childGuid)
        {
            var child = StandaloneElementalPresentationPolicy.Find(childGuid);
            return child != null && !child.UniqueNpc && child.ParentGuid == parentGuid && child.CreatureKeys.Count == 1
                ? child.CreatureKeys[0] : null;
        }

        internal static string Title(string key)
        {
            foreach (int rank in Ranks)
            {
                var spec = Rank(rank);
                if (!spec.CreatureKeys.Contains(key)) continue;
                string element = Elements.Single(value => key.Contains("-" + value + "-"));
                string size = key.Substring(0, key.IndexOf('-'));
                return "Elemental, " + Capital(element) + ", " + Capital(size);
            }
            return null;
        }

        /// <summary>
        /// The desktop Spells tab's launcher staging, applied to the native
        /// per-level rows (m_LevelButtonPairs order) after FillSlots. Each
        /// consolidated original standalone Summon Elemental tile leaves its
        /// row, and one launcher takes the first one's place at the destination
        /// level (or the end of that row). Every other row, including Summon
        /// Monster and Summon Nature's Ally tiles with their native variant
        /// popups, keeps its order. Input rows are never written.
        /// Returns null, leaving the native rows untouched, when no original
        /// tile is consolidated: every collected source is then a Summon
        /// Monster/Nature's Ally row that already offers the same children,
        /// so a launcher would only add a duplicate entry.
        /// </summary>
        internal static List<T[]> StageLauncherRows<T>(IList<T[]> levels,int destination,
            ICollection<T> consolidated,Func<T> launcher) where T:class
        {
            if(levels==null)throw new ArgumentNullException("levels");
            if(consolidated==null)throw new ArgumentNullException("consolidated");
            if(launcher==null)throw new ArgumentNullException("launcher");
            if(consolidated.Count==0)return null;
            if(destination<0 || destination>=levels.Count)throw new InvalidOperationException("Exact native destination level is missing.");
            var staged=new List<T[]>(levels.Count);
            for(int index=0;index<levels.Count;index++)
            {
                T[] original=levels[index];
                var rows=original.Where(value=>!consolidated.Any(source=>ReferenceEquals(value,source))).ToList();
                if(index==destination)
                {
                    T tile=launcher();
                    if(tile==null)throw new InvalidOperationException("Launcher construction failed.");
                    int first=Array.FindIndex(original,value=>consolidated.Any(source=>ReferenceEquals(value,source)));
                    rows.Insert(first<0?rows.Count:Math.Min(first,rows.Count),tile);
                }
                staged.Add(rows.ToArray());
            }
            return staged;
        }

        internal static string RankTitle(int rank)
        { return Capital(Rank(rank).CreatureKeys[0].Split('-')[0]) + " (" + rank + ")"; }
        private static string Capital(string value)
        { return char.ToUpperInvariant(value[0]) + value.Substring(1); }
    }

    internal sealed class ElementalInvocationContextSnapshot
    {
        private readonly object[] _references;
        private readonly long[] _values;
        private readonly Func<object[]> _readReferences;
        private readonly Func<long[]> _readValues;
        internal ElementalInvocationContextSnapshot(Func<object[]> references,Func<long[]> values)
        {_readReferences=references;_readValues=values;_references=references().ToArray();_values=values().ToArray();}
        internal bool Current {get{return Matches(_readReferences(),_readValues());}}
        internal bool Matches(object[] references,long[] values)
        {return references.Length==_references.Length && values.SequenceEqual(_values) &&
            references.Select((value,index)=>ReferenceEquals(value,_references[index])).All(value=>value);}
    }
}
