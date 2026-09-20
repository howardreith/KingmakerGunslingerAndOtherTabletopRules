using System;
using System.Collections.Generic;
using KingmakerGunslinger.Spells.ShieldOther;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Array-backed publication transaction for the optional Favored Class
    // per-level feature variants. The production reconciliation executes
    // exactly this transaction against the live blueprint, and the complete
    // behavioral contract — already-correct retention, foreign preservation,
    // singular publication, failure rollback and foreign-mutation refusal —
    // is exercised by the deterministic domain suite against this class.
    internal static class FavoredClassVariantsPublication
    {
        // Pure publication decision for the spell entry array. Returns the
        // exact same array instance when the canonical spell is already
        // present exactly once by reference and GUID; otherwise returns a
        // new array with every foreign entry preserved in order and the
        // canonical spell appended exactly once. Null entries and a null or
        // empty spell GUID fail closed before any mutation.
        internal static TSpell[] Publish<TSpell>(TSpell[] current,
            TSpell spell, Func<TSpell, string> guid) where TSpell : class
        {
            if (spell == null) throw new ArgumentNullException("spell");
            if (guid == null) throw new ArgumentNullException("guid");
            string spellGuid = guid(spell);
            if (string.IsNullOrWhiteSpace(spellGuid))
                throw new InvalidOperationException(
                    "The canonical spell has no stable GUID.");
            if (current != null)
            {
                int references = 0;
                int guids = 0;
                foreach (TSpell value in current)
                {
                    if (value == null)
                        throw new InvalidOperationException(
                            "The variants array contains a null entry.");
                    if (ReferenceEquals(value, spell)) references++;
                    if (string.Equals(guid(value), spellGuid,
                        StringComparison.Ordinal)) guids++;
                }
                if (references == 1 && guids == 1) return current;
            }
            var result = new List<TSpell>();
            if (current != null)
                foreach (TSpell value in current)
                {
                    if (value == null)
                        throw new InvalidOperationException(
                            "The variants array contains a null entry.");
                    if (!ReferenceEquals(value, spell) && !string.Equals(
                        guid(value), spellGuid, StringComparison.Ordinal))
                        result.Add(value);
                }
            result.Add(spell);
            return result.ToArray();
        }
    }

    // Executes one publication against a target field plus its item cache.
    // The exact assigned array is retained so rollback can prove ownership;
    // the prior variants array and the prior cache value are captured before
    // any mutation so a failed publication or validation restores exactly
    // the owned state. Each owned write is marked as attempted BEFORE its
    // delegate runs, because a writer may change its target and then throw;
    // the rollback decision therefore reads the live field instead of
    // trusting completion flags. A field that no longer references either
    // the prior array or the exact assigned array is a foreign mutation and
    // is never overwritten; a restoration whose own writes fail rethrows
    // with the original failure preserved inside an AggregateException.
    internal sealed class FavoredClassVariantsTransaction<TSpell>
        where TSpell : class
    {
        private readonly Func<TSpell[]> _read;
        private readonly Action<TSpell[]> _write;
        private readonly Func<object> _readCache;
        private readonly Action<object> _writeCache;
        private TSpell[] _before;
        private object _beforeCache;
        private TSpell[] _assigned;
        private bool _fieldWriteAttempted;
        private bool _mutated;

        internal FavoredClassVariantsTransaction(Func<TSpell[]> read,
            Action<TSpell[]> write, Func<object> readCache,
            Action<object> writeCache)
        {
            if (read == null || write == null || readCache == null ||
                writeCache == null)
                throw new ArgumentNullException();
            _read = read; _write = write; _readCache = readCache;
            _writeCache = writeCache;
        }

        internal bool Mutated { get { return _mutated; } }

        // Publishes and then runs the caller's validation under one failure
        // contract: a failure during publication (including a writer that
        // writes and then throws, or a cache writer that throws after the
        // array write) or during validation rolls the owned writes back and
        // rethrows the original exception with its information intact.
        internal TSpell[] PublishAndValidate(TSpell spell,
            Func<TSpell, string> guid, Action validate)
        {
            TSpell[] published;
            try { published = Publish(spell, guid); }
            catch (Exception exception)
            {
                RollbackOwnedWrites(exception);
                throw;
            }
            Validate(validate);
            return published;
        }

        // Runs the caller's validation against an already-executed
        // publication; a failure rolls back the owned mutation and rethrows
        // the original exception with its information intact.
        internal void Validate(Action validate)
        {
            if (validate == null) throw new ArgumentNullException("validate");
            try { validate(); }
            catch (Exception exception)
            {
                RollbackOwnedWrites(exception);
                throw;
            }
        }

        internal TSpell[] Publish(TSpell spell, Func<TSpell, string> guid)
        {
            _before = _read();
            _beforeCache = _readCache();
            TSpell[] published = FavoredClassVariantsPublication.Publish(
                _before, spell, guid);
            if (ReferenceEquals(published, _before)) return published;
            _assigned = published;
            // Attempted-marks precede the delegates: a writer may change
            // its target and then throw, leaving an owned write installed.
            _fieldWriteAttempted = true;
            _write(published);
            _writeCache(null);
            _mutated = true;
            return published;
        }

        internal void Rollback(Exception cause)
        { RollbackOwnedWrites(cause); }

        private void RollbackOwnedWrites(Exception cause)
        {
            if (!_fieldWriteAttempted) return;
            TSpell[] field = _read();
            if (ReferenceEquals(field, _before)) return;
            if (!ReferenceEquals(field, _assigned))
                throw new InvalidOperationException(
                    "Favored Class variants changed during rollback; restoration refused.",
                    cause);
            try
            {
                _write(_before);
                _writeCache(_beforeCache);
            }
            catch (Exception restorationFailure)
            {
                throw new InvalidOperationException(
                    "Favored Class variants restoration failed; owned state may remain partially published.",
                    new AggregateException(cause, restorationFailure));
            }
        }
    }
}
