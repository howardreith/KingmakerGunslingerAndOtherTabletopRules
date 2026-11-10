using System;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Pure verdicts for Spider Summoner web-escape observations. An attempt is
    /// a JSON record of the escape rules one escape triggered. Every missing,
    /// null, malformed, refused or unsupported observation is a named problem,
    /// never an exception that hides the original reason.
    /// </summary>
    internal static class SpiderEscapeObservationVerdict
    {
        /// <summary>A JSON value for an optional string: a real Null token when
        /// absent. A null string stored directly becomes a String token.</summary>
        internal static JToken Optional(string value)
        { return value == null ? JValue.CreateNull() : (JToken)value; }

        /// <summary>Null when the one rule triggered is the one this escape
        /// implementation's branch must trigger; otherwise why not.</summary>
        internal static string RuleProblem(string implementation, string branch, JArray rules)
        {
            if (implementation == null || implementation.StartsWith("unsupported", StringComparison.Ordinal)) return "unsupported-implementation:" + (implementation ?? "null");
            if (branch != "skill" && branch != "cmb") return "unknown-branch:" + (branch ?? "null");
            if (rules == null || rules.Count != 1) return "rules=" + (rules == null ? 0 : rules.Count);
            JObject only = rules[0] as JObject;
            string rule = only == null ? null : Text(only["rule"]), discriminator = only == null ? null : Text(only["discriminator"]);
            bool skillStat = discriminator == "SkillMobility" || discriminator == "SkillAthletics";
            bool ok = branch == "skill" ? rule == "RuleSkillCheck" && skillStat :
                implementation == "native" ? rule == "RuleCombatManeuver" && discriminator == "Grapple" :
                implementation == "cotw" ? rule == "RuleSkillCheck" && discriminator == "AdditionalCMB" : false;
            return ok ? null : "unexpected:" + (rule ?? "null") + ":" + (discriminator ?? "null");
        }

        /// <summary>Null and the target number when the attempt is a complete,
        /// supported observation; otherwise its original reason.</summary>
        internal static string TargetNumber(JObject attempt, out int value)
        {
            value = 0;
            if (attempt == null) return "no-observation";
            string error = Text(attempt["error"]);
            if (error != null) return "error:" + error;
            JToken problem = attempt["ruleProblem"];
            if (problem == null) return "missing-rule-verdict";
            // Read by value: a String token holding null also means no problem.
            string ruleProblem = Text(problem);
            if (ruleProblem != null) return "rule:" + ruleProblem;
            JToken number = attempt["targetNumber"];
            if (number == null) return "missing-target-number";
            if (number.Type != JTokenType.Integer) return "target-number-" + number.Type.ToString().ToLowerInvariant();
            value = (int)number;
            return null;
        }

        /// <summary>Null when the marked attempt is exactly the unmarked one
        /// plus the bonus, through the same rule; otherwise why not.</summary>
        internal static string PairProblem(JObject marked, JObject unmarked, int bonus)
        {
            int with, without;
            string a = TargetNumber(marked, out with), b = TargetNumber(unmarked, out without);
            if (a != null) return "marked:" + a;
            if (b != null) return "unmarked:" + b;
            if (Text(marked["rule"]) != Text(unmarked["rule"]) || Text(marked["discriminator"]) != Text(unmarked["discriminator"])) return "different-rule";
            return with == without + bonus ? null : "target-numbers:" + without + "->" + with;
        }

        private static string Text(JToken token)
        { return token == null || token.Type == JTokenType.Null ? null : token.Type == JTokenType.String ? (string)token : token.ToString(); }
    }
}
