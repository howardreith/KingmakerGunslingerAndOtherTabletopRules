using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.BodyguardFeats;
using KingmakerGunslinger.Bootstrap;
using UnityModManagerNet;

namespace KingmakerGunslinger.TurnUndead
{
    /// <summary>
    /// Self-contained Turn Undead module. It runs once on the first UMM update
    /// after the whole LoadDictionary postfix chain has returned, so every
    /// optional mod's channel wrappers already exist whatever the load order,
    /// and well before any save can be loaded.
    ///
    /// Save-referenced identities (feat, actions, flight buff) register in every
    /// module state. Only the feat's general-feat publication follows the
    /// module toggle; existing owners keep a working feat while it is off, as
    /// for the other dedicated feat modules.
    /// </summary>
    internal static class TurnUndeadModule
    {
        private static readonly object Gate = new object();
        private static ModContext _context;
        private static bool _installed;
        private static bool _attached;
        private static bool _completed;
        private static TurnUndeadBlueprintSet _blueprints;
        private static BodyguardFeatPublicationTransaction<BlueprintFeature> _publication;
        private static TurnUndeadResistancePublication _resistance;
        private static TurnUndeadCotwPublication _cotw;
        private static string _status = "pending";

        internal static TurnUndeadBlueprintSet Blueprints { get { lock (Gate) return _blueprints; } }
        internal static bool FeatPublished { get { lock (Gate) return _publication != null; } }
        internal static string Status { get { lock (Gate) return _status; } }
        internal static TurnUndeadCotwPublication CotwPublication { get { lock (Gate) return _cotw; } }
        internal static IReadOnlyList<string> ResistanceEvidence
        {
            get
            {
                lock (Gate) return _resistance == null ? new string[0] :
                    _resistance.Evidence.ToArray();
            }
        }

        internal static void Install(ModContext context)
        {
            if (context == null) return;
            lock (Gate)
            {
                _context = context;
                if (_installed) return;
                _installed = true;
                _attached = true;
            }
            context.ModEntry.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(UnityModManager.ModEntry entry, float delta)
        {
            ModContext context;
            lock (Gate)
            {
                if (_completed) return;
                context = _context;
            }
            if (context == null) return;
            switch (TurnUndeadLifecyclePolicy.Decide(BlueprintBootstrap.IsInitialized, BlueprintBootstrap.IsFailed))
            {
                case TurnUndeadStartupDecision.Wait:
                    return;
                case TurnUndeadStartupDecision.Blocked:
                    Finish(context, "blocked: core blueprint initialization failed");
                    TurnUndeadDiagnostics.Warning("initialize.blocked",
                        "Core blueprints failed to initialize; Turn Undead was not registered.");
                    return;
            }
            try
            {
                Initialize(context, BlueprintBootstrap.Library);
            }
            catch (Exception exception)
            {
                Finish(context, "failed: " + exception.GetType().Name);
                TurnUndeadDiagnostics.Failure("initialize.failed",
                    "Turn Undead could not start; its changes were rolled back and other modules continue.",
                    exception);
            }
        }

        private static void Finish(ModContext context, string status)
        {
            lock (Gate)
            {
                _completed = true;
                _status = status;
                if (_attached)
                {
                    _attached = false;
                    context.ModEntry.OnUpdate -= OnUpdate;
                }
            }
        }

        /// <summary>
        /// Phase 1 registers the save-referenced identities (feat, actions,
        /// flight effect) and their icons; a failure there rolls back the whole
        /// registration. Phase 2 publishes into foreign surfaces (native channel
        /// resistance, optional Call of the Wild, the general feat offer); a
        /// failure there rolls back only phase-2 changes, so saved identities
        /// stay resolvable. A Call of the Wild contract drift is a reported
        /// degraded state inside phase 2, not a phase failure.
        /// </summary>
        private static void Initialize(ModContext context, LibraryScriptableObject library)
        {
            string modDirectory = Path.GetDirectoryName(context.Assembly.Location);
            BlueprintManifest manifest = BlueprintManifest.Load(modDirectory);
            var registry = new BlueprintRegistry(library, manifest, context.Logger);
            TurnUndeadBlueprintSet set = null;
            TurnUndeadResistancePublication resistance = null;
            TurnUndeadCotwPublication cotw = null;
            string cotwFailure = null;
            BodyguardFeatPublicationTransaction<BlueprintFeature> publication = null;
            bool offer = context.FeatureModules.ActiveTurnUndead;

            TurnUndeadInstallOutcome outcome = TurnUndeadLifecyclePolicy.Install(
                () =>
                {
                    set = TurnUndeadBlueprints.Register(library, registry);
                    OwnedIconAssignments.ApplyTurnUndead(library, manifest);
                },
                () => { set = null; registry.RollbackAll(); },
                () =>
                {
                    resistance = TurnUndeadResistancePublication.Publish(library, set.AllAbilities.ToArray());
                    try
                    {
                        cotw = TurnUndeadCotwPublication.Publish(set, context.ModEntry);
                    }
                    catch (Exception exception)
                    {
                        // Publish restored its own changes: no submenu child, no CotW DC or
                        // resistance lists. Baseline Turn Undead with reduced CotW compatibility.
                        cotw = null;
                        cotwFailure = exception.GetType().Name + ": " + exception.Message;
                        TurnUndeadDiagnostics.Warning("cotw.unavailable",
                            "Call of the Wild integration was not applied (reduced compatibility: no channel " +
                            "submenu entry, no CotW DC bonuses or channel resistances); the direct actions remain. " +
                            cotwFailure);
                    }
                    if (offer) publication = PublishFeat(library, set.Feat);
                },
                () =>
                {
                    if (publication != null) publication.Rollback();
                    if (cotw != null) cotw.Rollback();
                    if (resistance != null) resistance.Rollback();
                    publication = null;
                    cotw = null;
                    resistance = null;
                });

            if (!outcome.Registered)
                throw new InvalidOperationException("Turn Undead registration failed and was rolled back.",
                    outcome.RegistrationFailure);
            lock (Gate)
            {
                _blueprints = set;
                _resistance = resistance;
                _cotw = cotw;
                _publication = publication;
            }
            if (!outcome.Published)
            {
                Finish(context, "registered; publication failed: " + outcome.PublicationFailure.GetType().Name);
                TurnUndeadDiagnostics.Failure("publication.failed",
                    "Turn Undead identities are registered (saves stay loadable) but its publication failed and " +
                    "was rolled back; the feat is not offered.", outcome.PublicationFailure);
                return;
            }
            Finish(context, offer ? "published" : "registered; module off (not offered)");
            TurnUndeadDiagnostics.Info("initialize.complete",
                "registered=" + registry.RegisteredCount + ";featPublished=" + offer +
                ";resistance=" + string.Join(",", resistance.Evidence.ToArray()) +
                (cotw == null ? ";cotw=failed" : ";cotw=" + cotw.Availability + ";menuChildren=" +
                    cotw.MenuChildren + (cotw.Evidence.Count == 0 ? string.Empty :
                        ";" + string.Join(",", cotw.Evidence.ToArray()))));
        }

        private static BodyguardFeatPublicationTransaction<BlueprintFeature> PublishFeat(
            LibraryScriptableObject library, BlueprintFeature feat)
        {
            BlueprintFeatureSelection basic = BlueprintLibraryLookup.RequireExact<
                BlueprintFeatureSelection>(library, TurnUndeadSourceCatalog.BasicFeatSelectionGuid,
                    "native basic feat selection");
            var surfaces = new[]
            {
                new BodyguardPublicationSurface<BlueprintFeature>("basic.Features",
                    () => basic.Features, value => basic.Features = value),
                new BodyguardPublicationSurface<BlueprintFeature>("basic.AllFeatures",
                    () => basic.AllFeatures, value => basic.AllFeatures = value)
            };
            return BodyguardFeatPublicationTransaction<BlueprintFeature>.Publish(surfaces,
                new[] { feat }, value => value.AssetGuid, value => value.Name ?? string.Empty);
        }
    }

    /// <summary>
    /// Adds the private Turn Undead identities to the native ability-specific
    /// channel resistance list. Existing entries, order and the Sun domain
    /// bypass are preserved; the append is idempotent and exactly reversible.
    /// </summary>
    internal sealed class TurnUndeadResistancePublication
    {
        private readonly List<KeyValuePair<SavingThrowBonusAgainstSpecificSpells, BlueprintAbility[]>> _records =
            new List<KeyValuePair<SavingThrowBonusAgainstSpecificSpells, BlueprintAbility[]>>();
        private readonly List<string> _evidence = new List<string>();

        internal IReadOnlyList<string> Evidence { get { return _evidence; } }

        internal static TurnUndeadResistancePublication Publish(LibraryScriptableObject library,
            BlueprintAbility[] abilities)
        {
            var result = new TurnUndeadResistancePublication();
            BlueprintFeature resistance = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                library, TurnUndeadSourceCatalog.ChannelResistanceFourGuid,
                "native ChannelResistance4");
            result.Append(resistance, abilities);
            return result;
        }

        internal void Append(BlueprintFeature feature, BlueprintAbility[] abilities)
        {
            SavingThrowBonusAgainstSpecificSpells[] components = (feature.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<SavingThrowBonusAgainstSpecificSpells>()
                .ToArray();
            if (components.Length == 0)
                throw new InvalidOperationException(feature.name +
                    " has no ability-specific channel resistance component.");
            foreach (SavingThrowBonusAgainstSpecificSpells component in components)
            {
                BlueprintAbility[] before = component.Spells ?? Array.Empty<BlueprintAbility>();
                BlueprintAbility[] after = TurnUndeadListPolicy.AppendMissing(before, abilities);
                if (after == null) continue;
                component.Spells = after;
                _records.Add(new KeyValuePair<SavingThrowBonusAgainstSpecificSpells,
                    BlueprintAbility[]>(component, before));
                _evidence.Add(feature.AssetGuid + ":" + before.Length + "+" +
                    (after.Length - before.Length));
            }
        }

        internal void Rollback()
        {
            for (int index = _records.Count - 1; index >= 0; index--)
                _records[index].Key.Spells = _records[index].Value;
            _records.Clear();
        }
    }
}
