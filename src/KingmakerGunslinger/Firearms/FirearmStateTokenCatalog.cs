using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Strict finite codec for the Sprint 12 capacity-one persistence experiment.
    /// Absence of a token represents the canonical empty/normal state.
    /// </summary>
    internal sealed class FirearmStateTokenCatalog
    {
        internal const string LoadedNormalTokenId = "kmg.state.v1.loaded-normal.lead-ball";
        internal const string BrokenEmptyTokenId = "kmg.state.v1.broken-empty";
        internal const string BrokenLoadedTokenId = "kmg.state.v1.broken-loaded.lead-ball";
        internal const string WreckedTokenId = "kmg.state.v1.wrecked";

        internal static readonly AmmunitionId DiagnosticLeadBall =
            new AmmunitionId("kmg.debug.lead-ball");

        private readonly Dictionary<string, FirearmStateTokenDefinition> _byToken;
        private readonly Dictionary<FirearmState, FirearmStateTokenDefinition> _byState;
        private readonly FirearmStateTokenDefinition[] _definitions;

        internal FirearmStateTokenCatalog(
            IEnumerable<FirearmStateTokenDefinition> definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException("definitions");
            }

            _byToken = new Dictionary<string, FirearmStateTokenDefinition>(StringComparer.Ordinal);
            _byState = new Dictionary<FirearmState, FirearmStateTokenDefinition>();
            foreach (FirearmStateTokenDefinition definition in definitions)
            {
                if (definition == null)
                {
                    throw new ArgumentException(
                        "A state-token catalog cannot contain a null definition.",
                        "definitions");
                }

                if (definition.State == FirearmState.CreateEmpty())
                {
                    throw new ArgumentException(
                        "The canonical empty/normal state is represented by token absence and must not have a token.",
                        "definitions");
                }

                if (_byToken.ContainsKey(definition.TokenId))
                {
                    throw new ArgumentException(
                        "Duplicate state-token ID '" + definition.TokenId + "'.",
                        "definitions");
                }

                if (_byState.ContainsKey(definition.State))
                {
                    throw new ArgumentException(
                        "Two state-token definitions encode the same firearm state.",
                        "definitions");
                }

                _byToken.Add(definition.TokenId, definition);
                _byState.Add(definition.State, definition);
            }

            if (_byToken.Count == 0)
            {
                throw new ArgumentException(
                    "At least one non-default state-token definition is required.",
                    "definitions");
            }

            _definitions = _byToken.Values
                .OrderBy(definition => definition.TokenId, StringComparer.Ordinal)
                .ToArray();
        }

        internal IReadOnlyList<FirearmStateTokenDefinition> Definitions
        {
            get
            {
                var copy = new FirearmStateTokenDefinition[_definitions.Length];
                Array.Copy(_definitions, copy, _definitions.Length);
                return copy;
            }
        }

        internal static FirearmStateTokenCatalog CreateCapacityOneDiagnostic()
        {
            return new FirearmStateTokenCatalog(
                new[]
                {
                    new FirearmStateTokenDefinition(
                        LoadedNormalTokenId,
                        new FirearmState(
                            FirearmState.CurrentSchemaVersion,
                            1,
                            DiagnosticLeadBall,
                            FirearmCondition.Normal)),
                    new FirearmStateTokenDefinition(
                        BrokenEmptyTokenId,
                        new FirearmState(
                            FirearmState.CurrentSchemaVersion,
                            0,
                            null,
                            FirearmCondition.Broken)),
                    new FirearmStateTokenDefinition(
                        BrokenLoadedTokenId,
                        new FirearmState(
                            FirearmState.CurrentSchemaVersion,
                            1,
                            DiagnosticLeadBall,
                            FirearmCondition.Broken)),
                    new FirearmStateTokenDefinition(
                        WreckedTokenId,
                        new FirearmState(
                            FirearmState.CurrentSchemaVersion,
                            0,
                            null,
                            FirearmCondition.Wrecked))
                });
        }

        internal FirearmState Decode(IEnumerable<string> tokenIds)
        {
            if (tokenIds == null)
            {
                throw new ArgumentNullException("tokenIds");
            }

            string[] materialized = tokenIds.ToArray();
            if (materialized.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidDataException(
                    "A persisted firearm-state token ID was null or empty.");
            }

            if (materialized.Length == 0)
            {
                return FirearmState.CreateEmpty();
            }

            if (materialized.Length != 1)
            {
                throw new InvalidDataException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Exactly zero or one firearm-state token is valid; observed {0}.",
                        materialized.Length));
            }

            FirearmStateTokenDefinition definition;
            if (!_byToken.TryGetValue(materialized[0], out definition))
            {
                throw new NotSupportedException(
                    "The persisted firearm-state token '" + materialized[0] + "' is unknown to this build.");
            }

            return definition.State;
        }

        internal string Encode(FirearmState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            if (state == FirearmState.CreateEmpty())
            {
                return null;
            }

            FirearmStateTokenDefinition definition;
            if (!_byState.TryGetValue(state, out definition))
            {
                throw new NotSupportedException(
                    "The firearm state is not representable by the Sprint 12 finite token catalog: " + state);
            }

            return definition.TokenId;
        }

        internal FirearmStateTokenDefinition RequireDefinition(string tokenId)
        {
            FirearmStateTokenDefinition definition;
            if (string.IsNullOrWhiteSpace(tokenId) || !_byToken.TryGetValue(tokenId, out definition))
            {
                throw new KeyNotFoundException(
                    "No firearm-state token definition exists for '" + (tokenId ?? "<null>") + "'.");
            }

            return definition;
        }

        internal bool ContainsToken(string tokenId)
        {
            return tokenId != null && _byToken.ContainsKey(tokenId);
        }
    }
}
