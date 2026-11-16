using System;
using System.Collections.Generic;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Presentation
{
    /// <summary>What kind of thing could not be read from a source.</summary>
    internal enum SourceFaultKind
    {
        /// <summary>One entry of a source that was otherwise read.</summary>
        MalformedEntry,
        /// <summary>The source itself could not be obtained; nothing was read from it.</summary>
        SourceUnavailable,
        /// <summary>The source stopped part-way; the entries before the stop were read.</summary>
        EnumerationStopped
    }

    /// <summary>
    /// One thing that could not be read while collecting observations. An
    /// entry fault names its source and position; a source fault names its
    /// source only. The two are never reported under the same heading.
    /// </summary>
    internal sealed class SourceFault
    {
        internal SourceFault(SourceFaultKind kind, string source, int index,
            string detail)
        {
            Kind = kind; Source = source ?? "<unnamed source>"; Index = index;
            Detail = detail ?? "no detail";
        }

        internal SourceFaultKind Kind { get; private set; }
        internal string Source { get; private set; }
        /// <summary>The entry's position in its source; -1 for a source fault.</summary>
        internal int Index { get; private set; }
        internal string Detail { get; private set; }
        internal bool AboutEntry { get { return Kind == SourceFaultKind.MalformedEntry; } }

        /// <summary>Where the fault is: <c>source[index]</c> for an entry, the source otherwise.</summary>
        internal string Locator
        {
            get { return AboutEntry ? SourceReader.Locate(Source, Index) : Source; }
        }

        internal string Describe()
        {
            return Locator + "(" + Detail + ")";
        }
    }

    /// <summary>
    /// One position of a source: the entry that was there, or the reason
    /// there was none.
    /// </summary>
    internal sealed class SourceSlot<TEntry> where TEntry : class
    {
        internal SourceSlot(int index, TEntry value, SourceFault fault)
        {
            Index = index; Value = value; Fault = fault;
        }

        internal int Index { get; private set; }
        internal TEntry Value { get; private set; }
        /// <summary>Set when the position held no entry.</summary>
        internal SourceFault Fault { get; private set; }
    }

    /// <summary>
    /// Everything that could be read from one source, position by position,
    /// and the source's own fault if it had one. A source fault does not
    /// discard the positions read before it.
    /// </summary>
    internal sealed class SourceRead<TEntry> where TEntry : class
    {
        internal SourceRead(string source, IList<SourceSlot<TEntry>> slots,
            SourceFault sourceFault)
        {
            Source = source; Slots = slots; SourceFault = sourceFault;
        }

        internal string Source { get; private set; }
        internal IList<SourceSlot<TEntry>> Slots { get; private set; }
        /// <summary>Null when the source was obtained and read to its end.</summary>
        internal SourceFault SourceFault { get; private set; }
    }

    /// <summary>
    /// Reads a source one position at a time. Obtaining the source, stepping
    /// it and reading a field of an entry are separate failure boundaries, so
    /// one bad entry costs that entry and nothing else.
    /// </summary>
    internal static class SourceReader
    {
        internal static string Locate(string source, int index)
        {
            return (source ?? "<unnamed source>") + "[" + index + "]";
        }

        internal static string Reason(Exception error)
        {
            return error == null ? "unknown" :
                error.GetType().Name + ": " + error.Message;
        }

        /// <summary>
        /// Reads one field. Returns false with the reason when reading raised;
        /// a null or default value is a successful read of that value.
        /// </summary>
        internal static bool TryRead<TValue>(Func<TValue> read, out TValue value,
            out string fault)
        {
            try
            {
                value = read();
                fault = null;
                return true;
            }
            catch (Exception error)
            {
                value = default(TValue);
                fault = Reason(error);
                return false;
            }
        }

        /// <summary>Never throws.</summary>
        internal static SourceRead<TEntry> Read<TEntry>(string source,
            Func<IEnumerable<TEntry>> acquire) where TEntry : class
        {
            var slots = new List<SourceSlot<TEntry>>();
            IEnumerable<TEntry> sequence;
            IEnumerator<TEntry> enumerator = null;
            try
            {
                sequence = acquire == null ? null : acquire();
                if (sequence != null) enumerator = sequence.GetEnumerator();
            }
            catch (Exception error)
            {
                return new SourceRead<TEntry>(source, slots, new SourceFault(
                    SourceFaultKind.SourceUnavailable, source, -1, Reason(error)));
            }
            if (enumerator == null)
                return new SourceRead<TEntry>(source, slots, new SourceFault(
                    SourceFaultKind.SourceUnavailable, source, -1,
                    "the source is null"));
            SourceFault stopped = null;
            try
            {
                while (true)
                {
                    TEntry entry;
                    try
                    {
                        if (!enumerator.MoveNext()) break;
                        entry = enumerator.Current;
                    }
                    catch (Exception error)
                    {
                        stopped = new SourceFault(SourceFaultKind.EnumerationStopped,
                            source, -1, "enumeration stopped after " + slots.Count +
                                " entries: " + Reason(error));
                        break;
                    }
                    int index = slots.Count;
                    slots.Add(new SourceSlot<TEntry>(index, entry, entry != null
                        ? null : new SourceFault(SourceFaultKind.MalformedEntry,
                            source, index, "the entry is null")));
                }
            }
            finally
            {
                try { enumerator.Dispose(); }
                catch (Exception) { }
            }
            return new SourceRead<TEntry>(source, slots, stopped);
        }
    }

    /// <summary>
    /// Collects what the firearm item visual check reads. Each item, and each
    /// entry of a catalog, is its own failure boundary: whatever could be
    /// read of an entry is kept, an entry whose metadata could not be read is
    /// recorded as malformed at its position, and later entries are read as
    /// usual. Only a catalog that cannot be obtained or stepped is a source
    /// fault.
    /// </summary>
    internal static class FirearmVisualCollection
    {
        /// <summary>One item whose symbol and kind are known in advance.</summary>
        internal static FirearmVisualBindingObservation ReadItem<TItem>(
            string source, string symbol, FirearmKind kind, Func<TItem> item,
            Func<TItem, string, FirearmKind, FirearmVisualBindingObservation> observe)
        {
            TItem value;
            string fault;
            FirearmVisualBindingObservation observation;
            if (!SourceReader.TryRead(item, out value, out fault))
                observation = new FirearmVisualBindingObservation
                {
                    Symbol = symbol, Kind = kind, Fault = source + ": " + fault
                };
            else observation = Observe(observe, value, symbol, kind);
            observation.Source = source;
            return observation;
        }

        /// <summary>
        /// Every entry of one catalog, in order. Appends one observation per
        /// position, and one more for the catalog itself if it could not be
        /// obtained or stopped part-way.
        /// </summary>
        internal static void ReadEntries<TEntry, TSpec, TItem>(
            ICollection<FirearmVisualBindingObservation> into, string source,
            Func<IEnumerable<TEntry>> acquire, Func<TEntry, TSpec> spec,
            Func<TSpec, string> symbol, Func<TSpec, FirearmKind> kind,
            Func<TEntry, TItem> item,
            Func<TItem, string, FirearmKind, FirearmVisualBindingObservation> observe)
            where TEntry : class where TSpec : class
        {
            if (into == null) throw new ArgumentNullException("into");
            SourceRead<TEntry> read = SourceReader.Read(source, acquire);
            foreach (SourceSlot<TEntry> slot in read.Slots)
            {
                FirearmVisualBindingObservation observation;
                if (slot.Fault != null)
                    observation = new FirearmVisualBindingObservation
                    {
                        Malformed = true, Fault = slot.Fault.Detail
                    };
                else observation = ReadEntry(slot.Value, spec, symbol, kind, item,
                    observe);
                observation.Source = source;
                observation.EntryIndex = slot.Index;
                into.Add(observation);
            }
            if (read.SourceFault != null)
                into.Add(new FirearmVisualBindingObservation
                {
                    Source = source, SourceUnavailable = true,
                    Fault = read.SourceFault.Detail
                });
        }

        private static FirearmVisualBindingObservation ReadEntry<TEntry, TSpec, TItem>(
            TEntry entry, Func<TEntry, TSpec> spec, Func<TSpec, string> symbol,
            Func<TSpec, FirearmKind> kind, Func<TEntry, TItem> item,
            Func<TItem, string, FirearmKind, FirearmVisualBindingObservation> observe)
            where TEntry : class where TSpec : class
        {
            // The metadata, field by field. Each field that could be read is
            // kept even when another could not.
            var problems = new List<string>();
            string fault;
            TSpec metadata;
            string entrySymbol = null;
            FirearmKind entryKind = default(FirearmKind);
            if (!SourceReader.TryRead(() => spec(entry), out metadata, out fault))
                problems.Add("the entry's Spec could not be read: " + fault);
            else if (metadata == null) problems.Add("the entry has no Spec");
            else
            {
                if (!SourceReader.TryRead(() => symbol(metadata), out entrySymbol,
                    out fault))
                    problems.Add("the Spec's symbol could not be read: " + fault);
                else if (string.IsNullOrEmpty(entrySymbol))
                {
                    entrySymbol = null;
                    problems.Add("the Spec has no symbol");
                }
                if (!SourceReader.TryRead(() => kind(metadata), out entryKind,
                    out fault))
                    problems.Add("the Spec's kind could not be read: " + fault);
            }

            // The item is read whatever happened to the metadata.
            TItem value;
            string itemFault;
            bool itemRead = SourceReader.TryRead(() => item(entry), out value,
                out itemFault);
            FirearmVisualBindingObservation observation = itemRead
                ? Observe(observe, value, entrySymbol, entryKind)
                : new FirearmVisualBindingObservation
                {
                    Symbol = entrySymbol, Kind = entryKind, Fault = itemFault
                };
            if (problems.Count == 0) return observation;

            // Malformed metadata: no identity is invented. The symbol stays
            // absent if it was not read, and what was read of the item stays.
            observation.Symbol = entrySymbol;
            observation.Kind = entryKind;
            observation.Malformed = true;
            observation.Fault = string.Join("; ", problems.ToArray()) +
                (observation.Fault == null ? string.Empty :
                    "; the item could not be read: " + observation.Fault);
            return observation;
        }

        private static FirearmVisualBindingObservation Observe<TItem>(
            Func<TItem, string, FirearmKind, FirearmVisualBindingObservation> observe,
            TItem item, string symbol, FirearmKind kind)
        {
            FirearmVisualBindingObservation observation;
            string fault;
            if (!SourceReader.TryRead(() => observe(item, symbol, kind),
                out observation, out fault) || observation == null)
                observation = new FirearmVisualBindingObservation
                {
                    Symbol = symbol, Kind = kind, Fault = fault ?? "no observation"
                };
            return observation;
        }
    }

    /// <summary>
    /// The items read from the project's item sources: the readable items
    /// themselves, for the checks that follow, and one census identity per
    /// position read. Malformed entries and unavailable sources are in the
    /// identities and in <see cref="Faults"/>, never among the items.
    /// </summary>
    internal sealed class ProjectItemObservations<TItem> where TItem : class
    {
        internal ProjectItemObservations()
        {
            Items = new List<TItem>();
            Identities = new List<AuditIdentity>();
            Faults = new List<SourceFault>();
        }

        /// <summary>Every readable item, in source order.</summary>
        internal IList<TItem> Items { get; private set; }
        /// <summary>What the census compares against the expectation.</summary>
        internal IList<AuditIdentity> Identities { get; private set; }
        internal IList<SourceFault> Faults { get; private set; }
    }

    /// <summary>
    /// Collects the project item presentation check's items source by source
    /// and entry by entry. A malformed entry costs that entry; the readable
    /// entries before and after it are kept. A source that cannot be obtained
    /// or stops part-way is reported as that, keeps what was read, and does
    /// not stop the other sources.
    /// </summary>
    internal static class ProjectItemCollection
    {
        /// <summary>Never throws.</summary>
        internal static void ReadSource<TEntry, TItem>(
            ProjectItemObservations<TItem> into, string source,
            Func<IEnumerable<TEntry>> acquire, Func<TEntry, TItem> item,
            Func<TItem, AuditIdentity> identify)
            where TEntry : class where TItem : class
        {
            if (into == null) throw new ArgumentNullException("into");
            SourceRead<TEntry> read = SourceReader.Read(source, acquire);
            foreach (SourceSlot<TEntry> slot in read.Slots)
            {
                if (slot.Fault != null)
                {
                    Malformed(into, slot.Fault);
                    continue;
                }
                TItem value;
                string fault;
                TEntry entry = slot.Value;
                if (!SourceReader.TryRead(() => item(entry), out value, out fault))
                {
                    Malformed(into, new SourceFault(SourceFaultKind.MalformedEntry,
                        source, slot.Index, "the entry's item could not be read: " +
                            fault));
                    continue;
                }
                if (value == null)
                {
                    Malformed(into, new SourceFault(SourceFaultKind.MalformedEntry,
                        source, slot.Index, "the entry has no item"));
                    continue;
                }

                // The item is readable: it goes on to the later checks even
                // if its identity cannot be established.
                into.Items.Add(value);
                AuditIdentity identity;
                TItem readable = value;
                if (!SourceReader.TryRead(() => identify(readable), out identity,
                    out fault) || identity == null)
                    Malformed(into, new SourceFault(SourceFaultKind.MalformedEntry,
                        source, slot.Index, "the item's identity could not be read: " +
                            (fault ?? "no identity")));
                else into.Identities.Add(identity);
            }
            if (read.SourceFault != null)
            {
                into.Faults.Add(read.SourceFault);
                into.Identities.Add(AuditIdentity.UnavailableSource(
                    read.SourceFault.Source, read.SourceFault.Detail));
            }
        }

        private static void Malformed<TItem>(ProjectItemObservations<TItem> into,
            SourceFault fault) where TItem : class
        {
            into.Faults.Add(fault);
            into.Identities.Add(new AuditIdentity(null, fault.Locator, fault.Detail));
        }
    }

    /// <summary>The player-facing text rule the presentation check applies to one field.</summary>
    internal static class PlayerTextAudit
    {
        internal static void Append(ICollection<string> issues, string owner,
            string field, string text, bool required)
        {
            if (issues == null) throw new ArgumentNullException("issues");
            if (required && string.IsNullOrWhiteSpace(text))
            {
                issues.Add(owner + ":" + field + "=blank");
                return;
            }
            string issue = PlayerFacingTextPolicy.FindIssue(text);
            if (issue != null) issues.Add(owner + ":" + field + "=" + issue);
        }
    }
}
