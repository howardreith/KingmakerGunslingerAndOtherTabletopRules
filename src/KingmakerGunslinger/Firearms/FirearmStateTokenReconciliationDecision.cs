using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Immutable comparison of known state-token IDs before and after one native
    /// enchantment reconciliation call.
    /// </summary>
    internal sealed class FirearmStateTokenReconciliationDecision
    {
        private readonly string[] _before;
        private readonly string[] _after;

        internal FirearmStateTokenReconciliationDecision(
            FirearmStateTokenReconciliationAction action,
            IEnumerable<string> before,
            IEnumerable<string> after,
            string tokenToRestore,
            string reason)
        {
            if (!Enum.IsDefined(typeof(FirearmStateTokenReconciliationAction), action))
            {
                throw new ArgumentOutOfRangeException("action");
            }

            _before = Copy(before, "before");
            _after = Copy(after, "after");
            Action = action;
            TokenToRestore = NormalizeOptional(tokenToRestore);
            Reason = string.IsNullOrWhiteSpace(reason)
                ? throw new ArgumentException("A reconciliation reason is required.", "reason")
                : reason.Trim();
            Validate();
        }

        internal FirearmStateTokenReconciliationAction Action { get; private set; }

        internal IReadOnlyList<string> Before
        {
            get { return _before.ToArray(); }
        }

        internal IReadOnlyList<string> After
        {
            get { return _after.ToArray(); }
        }

        internal string TokenToRestore { get; private set; }

        internal string Reason { get; private set; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "action={0}; before=[{1}]; after=[{2}]; restore={3}; reason={4}",
                Action,
                string.Join(",", _before),
                string.Join(",", _after),
                TokenToRestore ?? "<none>",
                Reason);
        }

        private void Validate()
        {
            switch (Action)
            {
                case FirearmStateTokenReconciliationAction.NoToken:
                    if (_before.Length != 0 || _after.Length != 0 || TokenToRestore != null)
                    {
                        throw new ArgumentException(
                            "A no-token decision requires no token before or after and no restoration token.");
                    }

                    return;

                case FirearmStateTokenReconciliationAction.Preserved:
                    if (_before.Length != 1 || _after.Length != 1 ||
                        !string.Equals(_before[0], _after[0], StringComparison.Ordinal) ||
                        TokenToRestore != null)
                    {
                        throw new ArgumentException(
                            "A preserved decision requires the same single token before and after.");
                    }

                    return;

                case FirearmStateTokenReconciliationAction.RestoreMissing:
                    if (_before.Length != 1 || _after.Length != 0 ||
                        !string.Equals(_before[0], TokenToRestore, StringComparison.Ordinal))
                    {
                        throw new ArgumentException(
                            "A restore decision requires one before token, no after token, and that exact restoration token.");
                    }

                    return;

                case FirearmStateTokenReconciliationAction.Conflict:
                    if (TokenToRestore != null)
                    {
                        throw new ArgumentException(
                            "A conflicting token set must never request implicit restoration.");
                    }

                    return;

                default:
                    throw new ArgumentOutOfRangeException("Action");
            }
        }

        private static string[] Copy(IEnumerable<string> values, string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            string[] result = values.ToArray();
            if (result.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException(
                    "A reconciliation token collection cannot contain null or empty IDs.",
                    parameterName);
            }

            return result.Select(value => value.Trim()).ToArray();
        }

        private static string NormalizeOptional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
