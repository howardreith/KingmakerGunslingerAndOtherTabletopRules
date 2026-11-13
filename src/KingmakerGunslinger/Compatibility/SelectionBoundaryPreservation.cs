using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Compatibility
{
    /// <summary>
    /// GS-AUD-0034. Composes this project's own entries into a sequence that
    /// may also contain entries from other publishers, without discarding the
    /// ones it does not understand.
    ///
    /// Both of this project's postfixes on
    /// <c>BlueprintParametrizedFeature.ExtractSelectionItems</c> used to narrow
    /// the host result with <c>.OfType&lt;FeatureUIData&gt;()</c> and assign the
    /// narrowed sequence back, so any <c>IFeatureSelectionItem</c> supplied by
    /// another mod that is not a <c>FeatureUIData</c> was silently dropped. The
    /// failure mode would present to a player as "KMG broke mod X".
    ///
    /// The rules this enforces:
    ///
    /// - When this project contributes nothing, the host sequence is returned
    ///   completely untouched, preserving every entry and its exact order. That
    ///   is the common case, and the one the old code silently broke.
    /// - When this project does contribute, the understood entries keep the
    ///   composing caller's own contract - which for both current callers means
    ///   alphabetical ordering - and unfamiliar entries follow in their
    ///   original relative order, never reordered among themselves and never
    ///   removed.
    ///
    /// Unfamiliar entries are appended rather than interleaved. The original
    /// reason given here was that the interface exposes no display name to sort
    /// on; that is WRONG, and was found to be wrong while building the native
    /// boundary observation. IFeatureSelectionItem inherits Name, Description
    /// and Icon from IUIDataProvider, so a name is available.
    ///
    /// The actual reason stands on its own: sorting another publisher's rows in
    /// among ours would impose OUR ordering on THEIR entries, which is a
    /// presentation decision that is not this project's to make. Appending
    /// leaves their relative order exactly as the host produced it. That is a
    /// deliberate choice, not a limitation of the interface.
    ///
    /// This type is deliberately generic and free of engine dependencies so the
    /// boundary rule itself is directly testable.
    ///
    /// No installed mod was shown to be affected: across the eleven mods on the
    /// supported host no type implements that interface, and the only other mod
    /// patching the same method supplies only <c>FeatureUIData</c>. This is
    /// boundary preservation, not the repair of a demonstrated compatibility
    /// failure.
    /// </summary>
    internal static class SelectionBoundaryPreservation
    {
        /// <param name="hostResult">The sequence produced so far, which may
        /// contain entries from any publisher.</param>
        /// <param name="isUnderstood">Identifies the entries this project may
        /// filter, reorder and extend.</param>
        /// <param name="compose">Receives exactly the understood entries and
        /// returns the understood entries this project wants presented.</param>
        internal static T[] Merge<T>(IEnumerable<T> hostResult,
            Func<T, bool> isUnderstood, Func<T[], T[]> compose) where T : class
        {
            if (isUnderstood == null) throw new ArgumentNullException("isUnderstood");
            if (compose == null) throw new ArgumentNullException("compose");

            T[] original = (hostResult ?? Enumerable.Empty<T>())
                .Where(value => value != null).ToArray();
            T[] understood = original.Where(value => isUnderstood(value)).ToArray();
            T[] composed = compose(understood) ?? new T[0];

            // Nothing of ours was added: return the host sequence exactly as it
            // is, rather than a narrowed copy of it.
            bool contributed = composed.Any(value => !understood.Any(known =>
                ReferenceEquals(known, value)));
            if (!contributed) return original;

            return composed.Concat(original.Where(value => !isUnderstood(value)))
                .ToArray();
        }
    }
}
