using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal sealed class TeleportationCastExecution : ITeleportCastExecution
    {
        private readonly ITeleportationRolls _rolls;
        private TeleportationNativeCastSource _source;
        private TeleportationScrollCastSource _scrollSource;
        private TeleportationWorldSnapshot _before;
        private TeleportationOutcomeWorld _world;
        internal ITeleportCastResource Resource { get; private set; }
        internal TeleportFamiliarity Familiarity { get; private set; }
        internal object LastEvidence { get; private set; }
        internal TeleportationCastExecution() : this(new TeleportationCanonicalRolls()) { }
        // Request-local seam: normal UI always constructs the parameterless adapter.
        // Guarded runtime fixtures bind their sequence to the selected UI request.
        internal TeleportationCastExecution(ITeleportationRolls rolls)
        { _rolls = rolls ?? throw new ArgumentNullException("rolls"); }

        public ITeleportCastResource RevalidateAndCapture(WorldMapPointSpellAction action, out string diagnostic)
        {
            var context = TeleportationWorldMapAdapter.Capture(false);
            diagnostic = context.Diagnostic;
            if (!context.Usable || context.OriginId != action.OriginId) return null;
            BlueprintLocation destination = ResourcesLibrary.TryGetBlueprint<BlueprintLocation>(action.Destination.Id);
            var current = TeleportationWorldMapAdapter.Compose(context, destination).SingleOrDefault(value => value.Key == action.Key);
            if (current == null || current.Source.Kind != action.Source.Kind || current.Source.SpellLevel != action.Source.SpellLevel ||
                current.Destination.OrdinaryArrivals != action.Destination.OrdinaryArrivals)
            { diagnostic = "The selected destination or spellbook source changed before confirmation."; return null; }
            if (current.Source.Kind == TeleportCastSourceKind.Scroll)
            {
                _scrollSource = TeleportationScrollAdapter.Resolve(current.Source);
                if (_scrollSource == null) { diagnostic = "No current exact scroll resource."; return null; }
                if (_scrollSource.Reader == null) { diagnostic = "The scroll reader is no longer available."; return null; }
                // The real reader and the real scroll ability back the outcome
                // attribution; the native activation itself runs inside the
                // resource lease below.
                _source = new TeleportationNativeCastSource(current.Source, null,
                    new Kingmaker.UnitLogic.Abilities.AbilityData(_scrollSource.Scroll.Ability, _scrollSource.Reader.Descriptor), _scrollSource.Reader);
            }
            else
            {
                _source = TeleportationSpellbookAdapter.Resolve(current.Source);
                if (_source == null) { diagnostic = "No current exact spellbook resource."; return null; }
                _scrollSource = null;
            }
            Familiarity = FamiliarityFor(context, action.Destination.Id);
            if (action.Source.Spell == TeleportSpellKind.Teleport && TeleportRollTable.For(Familiarity).MishapPercent > 0 &&
                !TeleportationMishapDamageTarget.CanApply(TeleportationTravelers.Read(context.Player)))
            { diagnostic = "Native life-state update is unavailable for a living traveler."; return null; }
            _before = new TeleportationWorldSnapshot(context);
            Resource = _scrollSource != null ? _scrollSource.Capture() : _source.Capture();
            _world = new TeleportationOutcomeWorld(_before, _source, context.OriginId, _rolls);
            Record("cast.before", new { action = ActionEvidence(action), world = _before.State, resource = Resource.Evidence() });
            return Resource;
        }
        public TeleportExecutionResult Execute(WorldMapPointSpellAction action, Action materialEffectStarting)
        {
            if (_world == null || Resource == null || Resource.ObserveExpenditure() != TeleportExpenditure.ExactlyOne)
                throw new InvalidOperationException("A proven single spellbook expenditure is required before any effect.");
            var result = TeleportOutcomeResolver.Resolve(action.Source.Spell, Familiarity,
                action.Destination.Id, action.OriginId, _world, materialEffectStarting);
            if (result.Status != TeleportExecutionStatus.Arrived) _world.VerifyRulesFailure();
            return result;
        }
        public void RecordResult(WorldMapPointSpellAction action, TeleportExecutionResult result)
        {
            LastEvidence = new { action = ActionEvidence(action), familiarity = Familiarity.ToString(),
                status = result.Status.ToString(), outcome = result.Outcome.ToString(), d100 = result.D100,
                mishaps = result.Mishaps, destinationId = result.DestinationId, events = _world.Events.ToArray(),
                before = _before.State, after = _world.After == null ? null : _world.After.State, resource = Resource.Evidence() };
            Record(result.Status == TeleportExecutionStatus.DefensiveMishapLimit ? "critical.mishap-limit" : "cast.result", LastEvidence);
        }
        internal void RecordTransaction(WorldMapPointSpellAction action, TeleportCastTransaction transaction)
        {
            Record("cast.transaction", new { action = ActionEvidence(action), state = transaction.State.ToString(),
                diagnostic = transaction.Diagnostic, materialEffectStarted = transaction.MaterialEffectStarted,
                events = _world == null ? null : _world.Events.ToArray(), resource = Resource == null ? null : Resource.Evidence() });
        }
        internal static TeleportFamiliarity FamiliarityFor(TeleportationWorldMapContext context, string id)
        {
            return context.Familiarity.Familiarity(id, id == WordOfRecallDestinationPolicy.OlegId,
                context.Recall.Known && context.Recall.Established && context.Recall.DestinationId == id);
        }
        private static object ActionEvidence(WorldMapPointSpellAction action)
        {
            return new { sourcePointId = action.OriginId, selectedTargetId = action.Destination.Id,
                casterId = action.Source.CasterId, spellbookId = action.Source.BookId, spell = action.Source.Spell.ToString(),
                sourceKind = action.Source.Kind.ToString(), level = action.Source.SpellLevel,
                availableUses = action.Source.Uses, ordinaryArrivals = action.Destination.OrdinaryArrivals };
        }
        private static void Record(string code, object value)
        {
            ModContext context;
            if (ModContext.TryGet(out context)) context.Logger.Info("teleportation", code, TeleportationDiagnosticJson.Serialize(value));
        }
    }
}
