using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Deterministic one-line formatting for correlated firearm traces.
    /// </summary>
    internal static class CombatTraceFormatter
    {
        internal static string FormatBegin(long traceId, CombatTraceObservation observation)
        {
            if (observation == null)
            {
                throw new ArgumentNullException("observation");
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "trace={0}; rootStage={1}; rootEvent={2}; weapon={3}; weaponType={4}; definition={5}",
                FormatTraceId(traceId),
                observation.Stage,
                observation.EventIdentity,
                ReadField(observation.Fields, "weapon", "<unavailable>"),
                ReadField(observation.Fields, "weaponType", "<unavailable>"),
                ReadField(observation.Fields, "firearmDefinition", "<unavailable>"));
        }

        internal static string FormatRecord(long traceId, CombatTraceRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            CombatTraceObservation observation = record.Observation;
            var builder = new StringBuilder();
            builder.Append("trace=");
            builder.Append(FormatTraceId(traceId));
            builder.Append("; stage=");
            builder.Append(observation.Stage);
            builder.Append("; phase=");
            builder.Append(observation.Phase);
            builder.Append("; event=");
            builder.Append(observation.EventIdentity.ToString(CultureInfo.InvariantCulture));
            builder.Append("; parent=");
            builder.Append(
                observation.ParentEventIdentity.HasValue
                    ? observation.ParentEventIdentity.Value.ToString(CultureInfo.InvariantCulture)
                    : "<none>");
            builder.Append("; callback=");
            builder.Append(record.CallbackOrdinal.ToString(CultureInfo.InvariantCulture));
            builder.Append("; markerCount=");
            builder.Append(observation.MarkerCount.ToString(CultureInfo.InvariantCulture));

            foreach (KeyValuePair<string, string> pair in observation.Fields)
            {
                builder.Append("; ");
                builder.Append(Sanitize(pair.Key));
                builder.Append('=');
                builder.Append(Sanitize(pair.Value));
            }

            return builder.ToString();
        }

        internal static string FormatComplete(CombatTraceSnapshot trace)
        {
            if (trace == null)
            {
                throw new ArgumentNullException("trace");
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "trace={0}; rootStage={1}; rootEvent={2}; observations={3}; duplicateCallbacks={4}",
                FormatTraceId(trace.TraceId),
                trace.RootStage,
                trace.RootEventIdentity,
                trace.Records.Count,
                trace.DuplicateCallbackCount);
        }

        private static string FormatTraceId(long traceId)
        {
            return "KMG-" + traceId.ToString("D6", CultureInfo.InvariantCulture);
        }

        private static string ReadField(
            IReadOnlyDictionary<string, string> fields,
            string key,
            string fallback)
        {
            string value;
            return fields != null && fields.TryGetValue(key, out value)
                ? Sanitize(value)
                : fallback;
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "<empty>";
            }

            return value
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Replace(";", ",")
                .Trim();
        }
    }
}
