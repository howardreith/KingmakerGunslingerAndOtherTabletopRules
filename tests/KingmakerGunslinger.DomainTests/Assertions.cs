using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.DomainTests
{
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
