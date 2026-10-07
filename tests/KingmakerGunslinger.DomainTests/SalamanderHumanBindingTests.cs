using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SalamanderHumanBindingTests
    {
        private sealed class Palette
        {
            internal string[] Names, Parents;
            internal int[] Ids;
            internal float[][] Binds;
            internal Palette()
            {
                Names = SalamanderHumanBindingPolicy.NativeNames.Concat(SalamanderHumanBindingPolicy.NativeNames)
                    .Concat(new[] { "Weapons", "Weapons" }).ToArray();
                Parents = Names.Select(SalamanderHumanBindingPolicy.Parent).ToArray();
                Ids = Enumerable.Range(0, Names.Length).Select(i => i < 52 ? i % 26 + 1 : 99).ToArray();
                Binds = Enumerable.Range(0, Names.Length).Select(_ => Enumerable.Range(0, 16)
                    .Select(i => i % 5 == 0 ? 1f : 0f).ToArray()).ToArray();
                // Unselected storage bindings may disagree; never used as a
                // driver, and never collapse their transforms into anatomy.
                Binds[53][12] = 2.8f;
            }
            internal bool Resolve(out int[] slots)
            { return SalamanderHumanBindingPolicy.TrySlots(Names, Ids, Parents, Binds, out slots); }
        }

        internal static void CompleteSelectedPaletteKeepsAllNativeReferences()
        {
            var row = new Palette(); int[] slots;
            Assertions.True(row.Resolve(out slots), "Both copies of every exact anatomical bind agree.");
            Assertions.True(slots.SequenceEqual(Enumerable.Range(0, 26)), "First slot selected only after checking every duplicate.");
            Assertions.Equal(36, SalamanderHumanBindingPolicy.Names.Length, "26 native plus 10 original; no storage/cape/leg.");
            string[] logical = SalamanderHumanBindingPolicy.NativeNames.Concat(SalamanderTailAnimationPolicy.TailNames).ToArray();
            string[] exported = SalamanderHumanBindingPolicy.Names;
            Assertions.True(exported.SequenceEqual(logical.OrderBy(name => name, StringComparer.Ordinal)),
                "The exact exporter sorts the skin palette.");
            Assertions.True(exported.Select(name => logical[SalamanderHumanBindingPolicy.BindingIndex(name)]).SequenceEqual(exported),
                "Every exported weight index resolves to its correct anatomical/original bind, regardless of authoring order.");
            Assertions.True(exported.Select(SalamanderHumanBindingPolicy.BindingIndex).OrderBy(i => i)
                .SequenceEqual(Enumerable.Range(0, 36)), "No lost or duplicate driver mapping.");
            Assertions.False(SalamanderHumanBindingPolicy.Names.Contains("Weapons"), "Storage is excluded.");
            Assertions.True(SalamanderHumanBindingPolicy.NativeNames.All(name =>
                SalamanderHumanBindingPolicy.Parent(name) != null), "Every exact driver has a reviewed parent.");
            row.Names[0] = "foreign";
            Assertions.True(row.Resolve(out slots) && slots[0] == 26, "One remaining complete exact driver is valid.");
        }

        internal static void DisagreeingOrAmbiguousSelectedDuplicatesFailClosed()
        {
            for (int i = 0; i < 26; i++)
            {
                int[] slots;
                var bind = new Palette(); bind.Binds[i + 26][12] = .01f;
                Assertions.False(bind.Resolve(out slots), "Any disagreeing selected duplicate rejects the whole binding.");
                var identity = new Palette(); identity.Ids[i + 26] = 501;
                Assertions.False(identity.Resolve(out slots), "Name equality cannot replace live Transform identity.");
                var parent = new Palette(); parent.Parents[i + 26] = "Weapons";
                Assertions.False(parent.Resolve(out slots), "Wrong anatomical parent rejected.");
                var missing = new Palette(); missing.Names[i] = missing.Names[i + 26] = "unselected";
                Assertions.False(missing.Resolve(out slots), "Missing exact driver rejected.");
            }
        }

        internal static void MalformedPaletteNeverCreatesPartialBindings()
        {
            foreach (float value in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                var bad = new Palette(); bad.Binds[0][0] = value; int[] slots;
                Assertions.False(bad.Resolve(out slots), "Nonfinite selected bind fails.");
                Assertions.True(slots == null, "No partially resolved slots escape.");
            }
            var row = new Palette(); int[] result;
            row.Binds[0] = new float[15];
            Assertions.False(row.Resolve(out result), "Matrix must have sixteen cells.");
            row = new Palette(); row.Binds[0] = null;
            Assertions.False(row.Resolve(out result), "Null matrix rejected.");
            row = new Palette(); row.Ids[0] = row.Ids[26] = 0;
            Assertions.False(row.Resolve(out result), "No null Transform identity.");
            row = new Palette(); row.Ids[1] = row.Ids[27] = row.Ids[0];
            Assertions.False(row.Resolve(out result), "Two anatomical drivers cannot be one Transform.");
            Assertions.False(SalamanderHumanBindingPolicy.TrySlots(null, null, null, null, out result), "No missing palette.");
        }

        internal static void NativeSetGuardReportsBothOperandsWithoutRelaxation()
        {
            Assertions.True(SalamanderHumanBindingPolicy.NativeSetRejection(true, false) == null,
                "Only exact native human set and absent effective Tail can proceed.");
            Assertions.Equal("existing-effective-tail-action",
                SalamanderHumanBindingPolicy.NativeSetRejection(true, true), "Identify the rejected effective lookup.");
            foreach (bool tail in new[] { false, true })
                Assertions.Equal("not-exact-native-human-set",
                    SalamanderHumanBindingPolicy.NativeSetRejection(false, tail), "Wrong set still fails before considering Tail.");
        }

        internal static void OneTailAppendDoesNotReplaceOrMutateHumanActions()
        {
            object[] native = Enumerable.Range(0, 24).Select(_ => new object()).ToArray();
            object[] original = (object[])native.Clone(); object tail = new object();
            object[] owned = SalamanderHumanBindingPolicy.AppendOneTail(native, tail);
            Assertions.Equal(25, owned.Length, "Exactly one tail action appended.");
            Assertions.True(owned.Take(24).SequenceEqual(native) && ReferenceEquals(owned[24], tail), "Native ordering/references unchanged.");
            owned[0] = tail;
            Assertions.True(native.SequenceEqual(original), "Owned container cannot mutate shared action list.");
            foreach (object[] invalid in new[] { native.Take(23).ToArray(),
                native.Concat(new[] { new object() }).ToArray(),
                native.Select((value, i) => i == 3 ? null : value).ToArray(),
                native.Select((value, i) => i == 3 ? native[0] : value).ToArray() })
            {
                bool rejected = false;
                try { SalamanderHumanBindingPolicy.AppendOneTail(invalid, tail); }
                catch (ArgumentException) { rejected = true; }
                Assertions.True(rejected, "Incomplete/duplicate/null native action list rejected.");
            }
            bool alias = false;
            try { SalamanderHumanBindingPolicy.AppendOneTail(native, native[0]); }
            catch (ArgumentException) { alias = true; }
            Assertions.True(alias, "A native action cannot be relabeled as our tail.");
        }
    }
}
