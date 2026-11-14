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

    internal enum ElementalLauncherRoute { Stale, Chooser, NativeRows }

    /// <summary>
    /// The desktop chooser's admission and lifetime decisions. The runtime's
    /// FillSlots prefix (Begin), the launcher click (Open) and the open view's
    /// Update read the live game state and decide only through these gates.
    /// </summary>
    internal static class ElementalChooserGate
    {
        /// <summary>
        /// Native KeyboardAccess GameModesGroup.World. Game.SetupPublicControllers
        /// installs ClickWithSelectedAbilityHandler only in these two modes, and
        /// native DoStartMode starts Pause only over Default, so a paused game
        /// keeps native targeting. Every other mode (dialog, cutscene, rest,
        /// full-screen UI, global map, kingdom, Esc menu, game over) is refused.
        /// </summary>
        internal static readonly string[] WorldModes = { "Default", "Pause" };

        internal static bool WorldMode(string mode) { return mode != null && Array.IndexOf(WorldModes, mode) >= 0; }

        /// <summary>
        /// FillSlots prefix. Null: this fill may consolidate original tiles
        /// behind a launcher. Otherwise the reason the native rows are kept,
        /// so every original spell keeps its own native tile in this fill.
        /// The keyboard is probed last, only for a fill that could otherwise
        /// consolidate: a launcher is never built that could not open.
        /// </summary>
        internal static string Consolidation(bool active, bool gamepad, string mode, bool spellTab,
            string nativeRowsRequest, Func<string> keyboard)
        {
            if (!active) return "module-off";
            if (gamepad) return "controller-input";
            if (!spellTab) return "not-spell-tab";
            if (!WorldMode(mode)) return "unsupported-game-mode:" + mode;
            if (nativeRowsRequest != null) return "native-rows-requested:" + nativeRowsRequest;
            string refusal = keyboard();
            return refusal == null ? null : "chooser-input-unavailable:" + refusal;
        }

        /// <summary>
        /// Launcher click. A stale launcher belongs to a tab that is being
        /// rebuilt and does nothing. Any other refusal restores the native
        /// rows, so the original spells the launcher replaced become
        /// clickable again in their own native tiles.
        /// </summary>
        internal static ElementalLauncherRoute Launcher(bool current, bool gamepad, string mode,
            bool surfaceReady, string keyboardRefusal, out string reason)
        {
            reason = null;
            if (!current) { reason = "stale-launcher"; return ElementalLauncherRoute.Stale; }
            if (gamepad) { reason = "controller-input"; return ElementalLauncherRoute.Stale; }
            if (!WorldMode(mode)) reason = "unsupported-game-mode:" + mode;
            else if (!surfaceReady) reason = "chooser-surface-not-ready";
            else if (keyboardRefusal != null) reason = keyboardRefusal;
            return reason == null ? ElementalLauncherRoute.Chooser : ElementalLauncherRoute.NativeRows;
        }

        /// <summary>
        /// Open view, once per frame. Default and Pause transitions keep the
        /// same view and session; any other change closes it with no spend.
        /// </summary>
        internal static bool KeepOpen(bool current, bool gamepad, string mode, bool ownerActive)
        { return current && !gamepad && WorldMode(mode) && ownerActive; }
    }

    /// <summary>
    /// One live native keyboard context: Game.Keyboard and its Disabled
    /// CountingGuard, read and written only through these delegates.
    /// </summary>
    internal sealed class ElementalKeyboardContext
    {
        internal readonly object Keyboard, Guard;
        private readonly Func<int> _count;
        private readonly Action _acquire, _release;
        internal ElementalKeyboardContext(object keyboard, object guard, Func<int> count, Action acquire, Action release)
        { Keyboard = keyboard; Guard = guard; _count = count; _acquire = acquire; _release = release; }
        internal int Count { get { return _count(); } }
        internal void AcquireOne() { _acquire(); }
        internal void ReleaseOne() { _release(); }
        internal bool Same(ElementalKeyboardContext other)
        { return other != null && ReferenceEquals(Keyboard, other.Keyboard) && ReferenceEquals(Guard, other.Guard); }
    }

    /// <summary>
    /// The chooser's single keyboard count, with ownership-aware recovery.
    /// The native guard cannot go negative, so a live count of zero proves
    /// that nobody (this chooser included) holds the keyboard. A release that
    /// could not be verified is remembered with its exact context. The lease
    /// never resets the guard, never releases a count it does not hold, and
    /// never acquires while any count is present.
    /// </summary>
    internal sealed class ElementalKeyboardLease
    {
        private ElementalKeyboardContext _held, _unresolved;
        private bool _unresolvedCountMayRemain;
        internal string Outcome { get; private set; }
        internal bool Held { get { return _held != null; } }
        internal bool Unresolved { get { return _unresolved != null; } }
        internal bool Restored { get { return _held == null && _unresolved == null; } }

        internal ElementalKeyboardLease() { Outcome = "not-acquired"; }

        /// <summary>Null when one fresh count may be acquired on the live context.</summary>
        internal string Admit(ElementalKeyboardContext live)
        {
            if (live == null || live.Guard == null) return Outcome = "refused:native-keyboard-absent";
            if (_held != null) return Outcome = "refused:keyboard-already-held-by-chooser";
            if (_unresolved != null)
            {
                if (!_unresolved.Same(live))
                {
                    // The unverified count, if any, lives on a guard that no
                    // longer gates input. It is left untouched.
                    _unresolved = null;
                    Outcome = "retired-unresolved-release:keyboard-context-replaced-no-write";
                }
                else if (live.Count == 0)
                {
                    _unresolved = null;
                    Outcome = "retired-unresolved-release:same-context-provably-free-no-write";
                }
                else return Outcome = "refused:unresolved-release-on-same-keyboard-context;count=" + live.Count +
                    (_unresolvedCountMayRemain ? ";chooser-count-may-remain" : ";foreign-count");
            }
            if (live.Count != 0) return Outcome = "refused:keyboard-owned-by-another-ui;count=" + live.Count;
            return null;
        }

        internal bool Acquire(ElementalKeyboardContext live)
        {
            if (Admit(live) != null) return false;
            live.AcquireOne(); _held = live;
            Outcome = "owned-native-keyboard-count-acquired";
            if (live.Count == 1) return true;
            Release(live);
            Outcome = "refused:acquisition-did-not-add-exactly-one-count;" + Outcome;
            return false;
        }

        /// <summary>True when nothing is held or exactly the held count was released.</summary>
        internal bool Release(ElementalKeyboardContext live)
        {
            if (_held == null) return true;
            var held = _held; _held = null;
            if (!held.Same(live))
            {
                _unresolved = held; _unresolvedCountMayRemain = true;
                Outcome = "keyboard-context-changed-no-foreign-write"; return false;
            }
            int count = live.Count;
            if (count < 1)
            {
                // Something else cleared the guard; the chooser's count is gone.
                _unresolved = held; _unresolvedCountMayRemain = false;
                Outcome = "owned-count-already-cleared-no-foreign-write"; return false;
            }
            live.ReleaseOne();
            if (live.Count == count - 1) { Outcome = "owned-native-keyboard-count-released"; return true; }
            // One release was issued but the count did not drop by one.
            _unresolved = held; _unresolvedCountMayRemain = true;
            Outcome = "native-keyboard-release-unverified"; return false;
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
