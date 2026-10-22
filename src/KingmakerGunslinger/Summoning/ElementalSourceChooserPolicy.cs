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
        internal static List<T> ProjectOriginalSources<T>(IEnumerable<T> original,IEnumerable<T> sources,T header,bool expanded,Func<T,bool> selected) where T:class
        {
            var exact=sources.ToArray();var rows=new List<T>();bool inserted=false;
            foreach(var row in original)
            {
                bool source=exact.Any(value=>ReferenceEquals(value,row));
                if(source && !inserted){rows.Add(header);inserted=true;}
                if(!source || expanded || selected(row))rows.Add(row);
            }
            return rows;
        }
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

        internal static string RankTitle(int rank)
        { return Capital(Rank(rank).CreatureKeys[0].Split('-')[0]) + " (" + rank + ")"; }
        private static string Capital(string value)
        { return char.ToUpperInvariant(value[0]) + value.Substring(1); }
    }

    // Source-aware transient visibility ownership, shared by native learning
    // and preparation disclosures. The row object is never replaced.
    internal sealed class ElementalRowVisibilityLease<T> where T:class
    {
        internal sealed class Row
        {
            internal T Native;
            internal string ParentGuid;
            internal Func<bool> Current,Selected,Visible;
            internal Action<bool> SetVisible;
            internal bool Original,Last,Written;
        }
        private readonly Row[] _rows;
        internal int Count { get { return _rows.Length; } }
        internal ElementalRowVisibilityLease(IEnumerable<Row> rows)
        {
            _rows=rows.Where(value=>value.Native!=null && ElementalSourceChooserPolicy.Parent(value.ParentGuid)!=null &&
                value.Current() && value.Visible()).ToArray();
            foreach(var row in _rows)row.Original=row.Last=row.Visible();
        }
        internal void Apply(bool expanded)
        {
            if(_rows.Any(value=>!value.Current()))throw new InvalidOperationException("Native source row generation changed.");
            try
            {
                foreach(var row in _rows)
                {
                    bool visible=expanded || row.Selected();
                    if(row.Visible()==visible)continue;
                    // A later writer changing our last value is not erased.
                    if(row.Written && row.Visible()!=row.Last)throw new InvalidOperationException("Native source visibility has another owner.");
                    row.Last=visible;row.Written=true;row.SetVisible(visible);
                }
            }
            catch { Release();throw; }
        }
        internal void Release()
        {
            var errors=new List<Exception>();
            foreach(var row in _rows)
            {
                try
                {
                    if(row.Written && row.Current() && row.Visible()==row.Last)
                    {row.SetVisible(row.Original);row.Written=false;}
                }
                catch(Exception error){errors.Add(error);}
            }
            if(errors.Count!=0)throw new AggregateException("Native row visibility restoration failed.",errors);
        }
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
