using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.DomainTests
{
    // A case that has nothing to assert in this run (for example no bound artifact in an unfiltered run, or deferred to
    // a later run): the runner reports NOT APPLICABLE, never PASS, and does not count it as a failure.
    internal sealed class NotApplicableException : Exception
    {
        internal NotApplicableException(string message) : base(message) { }
    }

    // How the runner selected this run's cases: Focused is true when KMG_TEST_FILTER or --test-prefix chose them.
    internal static class RunSelection
    {
        internal static bool Focused;
    }

    internal static class Assertions
    {
        internal static void True(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        internal static void False(bool condition, string message)
        {
            True(!condition, message);
        }

        internal static void Equal<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException(
                    string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "{0} Expected={1}; Actual={2}.",
                        message,
                        expected,
                        actual));
            }
        }

        internal static TException Throws<TException>(Action action, string message)
            where TException : Exception
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            try
            {
                action();
            }
            catch (TException exception)
            {
                return exception;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "{0} Expected {1}, but received {2}.",
                        message,
                        typeof(TException).FullName,
                        exception.GetType().FullName),
                    exception);
            }

            throw new InvalidOperationException(
                string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{0} Expected {1}, but no exception was thrown.",
                    message,
                    typeof(TException).FullName));
        }
    }
}
