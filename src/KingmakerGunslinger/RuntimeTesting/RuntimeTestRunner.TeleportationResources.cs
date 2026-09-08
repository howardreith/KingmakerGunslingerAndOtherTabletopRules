using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Globalmap;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void PollTeleportationResources()
        {
            if (_request.Scenario != RuntimeTestScenarioCatalog.DisposableTeleportationResources || !_request.ExitAfterCompletion ||
                _workingSaveSmoke == null || !_workingSaveSmoke.Complete || _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException("Resource probe requires its guarded named working save, automatic exit and intact write sentinels.");
            if (_teleportationMapLoad == null)
            {
                _teleportationMapLoad = Stopwatch.StartNew();
                Game.Instance.LoadArea(Game.Instance.BlueprintRoot.GlobalMap.GlobalMapEnterPoint, AutoSaveMode.None);
                return;
            }
            if (_teleportationMapLoad.Elapsed.TotalSeconds > _request.CompletionTimeoutSeconds)
                throw new InvalidOperationException("Resource probe world-map load timed out.");
            if (LoadingProcess.Instance.IsLoadingInProcess || GlobalMapRules.Instance == null || Game.Instance.CurrentMode != GameModeType.GlobalMap) return;
            Complete(RunTeleportationResources());
        }

        private RuntimeTestResult RunTeleportationResources()
        {
            if (!_context.FeatureModules.Active.TeleportationSpells || BlueprintBootstrap.TeleportationPublication == null)
                throw new InvalidOperationException("The resource probe requires the published module.");
            Player player = Game.Instance.Player;
            var map = GlobalMapRules.State;
            if (map.TravelData != null || map.CurrentEncounterData != null) throw new InvalidOperationException("Stationary encounter-free world map required.");
            BlueprintCharacterClass wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                "ba34257984f4c41408ce1dc2004e342e", "native Wizard resource fixture");
            BlueprintCharacterClass sorcerer = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                "b3a505fb61437dc4097f43c3f8f9a4cf", "native Sorcerer resource fixture");
            BlueprintCharacterClass cleric = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                "67819271767a9dd4fbfd4ae700befea0", "native Cleric resource fixture");
            if (wizard.Spellbook == null || sorcerer.Spellbook == null || cleric.Spellbook == null ||
                wizard.Spellbook.Spontaneous || !sorcerer.Spellbook.Spontaneous || cleric.Spellbook.Spontaneous ||
                wizard.Spellbook.SpellList.AssetGuid != TeleportationSpellListPublication.WizardListId ||
                sorcerer.Spellbook.SpellList.AssetGuid != TeleportationSpellListPublication.WizardListId ||
                cleric.Spellbook.SpellList.AssetGuid != TeleportationSpellListPublication.ClericListId)
                throw new InvalidOperationException("Native fixture class/book/list contract differs from the verified publication.");
            UnitEntityData[] owners = player.Party.Where(TeleportationSpellbookAdapter.CasterAvailable).Where(value =>
                new[] { wizard.Spellbook, sorcerer.Spellbook, cleric.Spellbook }.All(book => value.Descriptor.GetSpellbook(book) == null)).Take(2).ToArray();
            if (owners.Length != 2) throw new InvalidOperationException("Two available active-party casters with unused native fixture books are required.");
            string[] originalParty = player.Party.Select(value => value.UniqueId).ToArray();
            var originalTime = player.GameTime;
            var originalPosition = map.PartyPosition;
            string[] baselineSources = TeleportationSpellbookAdapter.Enumerate(player).Select(value => value.Snapshot.Key).ToArray();
            var fixtures = new List<TeleportResourceFixtureOwner>();
            var assertions = new List<RuntimeTestAssertion>();
            var cases = new List<object>();
            object sourceEvidence = null;
            string path = Path.Combine(_request.EvidenceDirectory, "teleportation-spell-resources.json");
            Exception failure = null;
            bool cleaned = false;
            try
            {
                foreach (UnitEntityData owner in owners) fixtures.Add(new TeleportResourceFixtureOwner(owner));
                Spellbook firstWizard = fixtures[0].AddBook(wizard.Spellbook);
                Spellbook spontaneous = fixtures[0].AddBook(sorcerer.Spellbook);
                Spellbook recallBook = fixtures[0].AddBook(cleric.Spellbook);
                Spellbook secondWizard = fixtures[1].AddBook(wizard.Spellbook);
                Spellbook[] fixtureBooks = fixtures.SelectMany(value => value.Books).ToArray();
                Func<TeleportationNativeCastSource[]> sources = () => TeleportationSpellbookAdapter.Enumerate(player)
                    .Where(value => fixtureBooks.Any(book => ReferenceEquals(book, value.Book))).ToArray();
                assertions.Add(Assertion("teleportation-resource-no-prepared-or-slot-source", "no fixture source before preparing/restoring native slots",
                    "sources=" + sources().Length, sources().Length == 0, path));
                firstWizard.OppositionSchools.Add(SpellSchool.Conjuration);
                foreach (Spellbook book in new[] { firstWizard, secondWizard, spontaneous })
                {
                    book.AddKnown(5, BlueprintBootstrap.Teleportation.Teleport, true);
                    book.AddKnown(7, BlueprintBootstrap.Teleportation.GreaterTeleport, true);
                }
                if (!recallBook.IsKnown(BlueprintBootstrap.Teleportation.WordOfRecall))
                    recallBook.AddKnown(6, BlueprintBootstrap.Teleportation.WordOfRecall, true);
                assertions.Add(Assertion("teleportation-resource-known-is-insufficient", "knowing project spells alone provides no usable source",
                    "sources=" + sources().Length, sources().Length == 0, path));
                foreach (Spellbook book in new[] { firstWizard, secondWizard })
                {
                    if (!book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.Teleport, book), null) ||
                        !book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.Teleport, book), null) ||
                        !book.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.GreaterTeleport, book), null))
                        throw new InvalidOperationException("Native preparation fixture could not allocate real slots.");
                }
                if (!recallBook.Memorize(new AbilityData(BlueprintBootstrap.Teleportation.WordOfRecall, recallBook), null))
                    throw new InvalidOperationException("Native Word of Recall preparation failed.");
                assertions.Add(Assertion("teleportation-resource-unrested-preparations-absent", "native newly assigned unavailable preparations are not cast sources",
                    "sources=" + sources().Length, sources().Length == 0, path));
                foreach (Spellbook book in fixtureBooks) book.Rest();
                TeleportationNativeCastSource[] initial = sources();
                sourceEvidence = initial.Select(value => new { casterId = value.Snapshot.CasterId, casterName = value.Snapshot.CasterName,
                    bookId = value.Snapshot.BookId, bookName = value.Snapshot.BookName, spell = value.Snapshot.Spell.ToString(),
                    kind = value.Snapshot.Kind.ToString(), uses = value.Snapshot.Uses, level = value.Snapshot.SpellLevel }).ToArray();
                bool expectedSources = initial.Length == 7 && initial.Select(value => value.Snapshot.Key).Distinct().Count() == 7 &&
                    initial.Count(value => value.Snapshot.CasterId == owners[0].UniqueId) == 5 &&
                    initial.Where(value => value.Snapshot.Kind == TeleportCastSourceKind.Prepared).All(value =>
                        value.Snapshot.Uses == (value.Snapshot.Spell == TeleportSpellKind.Teleport ? 2 : 1));
                assertions.Add(Assertion("teleportation-resource-distinct-current-sources", "seven real caster/book/spell sources; equivalent preparations grouped",
                    "sources=" + initial.Length, expectedSources, path));
                if (!expectedSources) throw new InvalidOperationException("Unexpected native source set; see structured sources.");
                string beforeMenus = string.Join("|", fixtureBooks.Select(TeleportResourceFingerprint));
                sources(); sources();
                assertions.Add(Assertion("teleportation-resource-enumeration-read-only", "repeated current source composition spends nothing",
                    "unchanged=" + (beforeMenus == string.Join("|", fixtureBooks.Select(TeleportResourceFingerprint))),
                    beforeMenus == string.Join("|", fixtureBooks.Select(TeleportResourceFingerprint)), path));
                foreach (TeleportationNativeCastSource initialSource in initial)
                {
                    TeleportationNativeCastSource current = TeleportationSpellbookAdapter.Resolve(initialSource.Snapshot);
                    if (current == null) throw new InvalidOperationException("A current fixture source did not re-resolve.");
                    TeleportationNativeCastResource resource = current.Capture();
                    object before = resource.Evidence();
                    resource.Spend();
                    TeleportExpenditure spent = resource.ObserveExpenditure();
                    object after = resource.Evidence();
                    TeleportationNativeCastSource remaining = TeleportationSpellbookAdapter.Resolve(current.Snapshot);
                    int remainingUses = remaining == null ? 0 : remaining.Snapshot.Uses;
                    resource.Spend();
                    bool duplicateSafe = resource.ObserveExpenditure() == spent;
                    bool restored = resource.RestoreAndVerifyExactResource();
                    bool duplicateRestoreRejected = !resource.RestoreAndVerifyExactResource();
                    object compensated = resource.Evidence();
                    cases.Add(new { source = current.Snapshot.Key, before, after, compensated,
                        expenditure = spent.ToString(), remainingUses, duplicateSafe, restored, duplicateRestoreRejected });
                    assertions.Add(Assertion("teleportation-resource-native-spend-" + cases.Count,
                        "exact one native use; duplicate spend inert; exact compensation once before any effect",
                        "kind=" + current.Snapshot.Kind + ";expenditure=" + spent + ";restored=" + restored,
                        resource.NativeSpendReturned == true && spent == TeleportExpenditure.ExactlyOne && remainingUses == current.Snapshot.Uses - 1 && duplicateSafe &&
                        restored && duplicateRestoreRejected && resource.ObserveExpenditure() == TeleportExpenditure.None, path));
                }
                TeleportCastSourceSnapshot stale = initial.First(value => ReferenceEquals(value.Book, firstWizard)).Snapshot;
                owners[0].IsDetached = true;
                try
                {
                    assertions.Add(Assertion("teleportation-resource-stale-caster-absent", "detached caster is absent with unchanged native resources",
                        "resolved=" + (TeleportationSpellbookAdapter.Resolve(stale) != null), TeleportationSpellbookAdapter.Resolve(stale) == null &&
                        beforeMenus == string.Join("|", fixtureBooks.Select(TeleportResourceFingerprint)), path));
                }
                finally { owners[0].IsDetached = false; }
                int intelligence = owners[0].Descriptor.Stats.Intelligence.BaseValue;
                owners[0].Descriptor.Stats.Intelligence.BaseValue = 9;
                try
                {
                    assertions.Add(Assertion("teleportation-resource-stale-casting-stat-absent", "native spellbook casting-stat gate suppresses the source",
                        "resolved=" + (TeleportationSpellbookAdapter.Resolve(stale) != null), TeleportationSpellbookAdapter.Resolve(stale) == null, path));
                }
                finally { owners[0].Descriptor.Stats.Intelligence.BaseValue = intelligence; }
                var exhausted = initial.First(value => ReferenceEquals(value.Book, secondWizard) && value.Snapshot.Spell == TeleportSpellKind.GreaterTeleport);
                TeleportationNativeCastResource lastUse = exhausted.Capture();
                lastUse.Spend();
                assertions.Add(Assertion("teleportation-resource-exhausted-source-absent", "last actual use disappears on fresh enumeration",
                    "resolved=" + (TeleportationSpellbookAdapter.Resolve(exhausted.Snapshot) != null),
                    lastUse.ObserveExpenditure() == TeleportExpenditure.ExactlyOne && TeleportationSpellbookAdapter.Resolve(exhausted.Snapshot) == null, path));
                if (!lastUse.RestoreAndVerifyExactResource()) throw new InvalidOperationException("Exhaustion control could not restore its captured native use.");
                var externalControl = TeleportationSpellbookAdapter.Resolve(initial.First(value =>
                    ReferenceEquals(value.Book, spontaneous) && value.Snapshot.Spell == TeleportSpellKind.Teleport).Snapshot);
                string externalBefore = TeleportResourceFingerprint(spontaneous);
                TeleportationNativeCastResource staleLease = externalControl.Capture();
                if (!spontaneous.Spend(externalControl.Ability, false)) throw new InvalidOperationException("External native resource control could not spend.");
                bool refused = false;
                try { staleLease.Spend(); } catch (InvalidOperationException) { refused = true; }
                bool attributionSafe = staleLease.ObserveExpenditure() == TeleportExpenditure.Ambiguous &&
                    !staleLease.RestoreAndVerifyExactResource() && staleLease.NativeSpendReturned == null;
                object externalEvidence = staleLease.Evidence();
                // Fixture cleanup of its separate control debit; production lease refused to refund it.
                spontaneous.RestoreSpontaneousSlots(5, 1);
                assertions.Add(Assertion("teleportation-resource-other-operation-not-refunded", "stale capture refuses to spend or compensate another operation's debit",
                    "refused=" + refused + ";attributionSafe=" + attributionSafe, refused && attributionSafe &&
                    externalBefore == TeleportResourceFingerprint(spontaneous), path));
                cases.Add(new { source = externalControl.Snapshot.Key, externalControl = true, evidence = externalEvidence, refused, attributionSafe });
                // Native special-known instances are a separate real list, not item
                // abilities or spell conversions. Invoke the exact native registration
                // only inside this explicit disposable fixture.
                spontaneous.RemoveSpell(BlueprintBootstrap.Teleportation.Teleport);
                MethodInfo addSpecial = typeof(Spellbook).GetMethod("AddSpecial", BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new[] { typeof(int), typeof(Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility) }, null);
                if (addSpecial == null) throw new MissingMethodException("Spellbook.AddSpecial(int, BlueprintAbility)");
                addSpecial.Invoke(spontaneous, new object[] { 5, BlueprintBootstrap.Teleportation.Teleport });
                var specialSource = TeleportationSpellbookAdapter.Resolve(externalControl.Snapshot);
                bool specialOnly = spontaneous.IsKnown(BlueprintBootstrap.Teleportation.Teleport) &&
                    !spontaneous.GetKnownSpells(5).Any(value => value.Blueprint == BlueprintBootstrap.Teleportation.Teleport) &&
                    spontaneous.GetSpecialSpells(5).Any(value => value.Blueprint == BlueprintBootstrap.Teleportation.Teleport);
                if (specialSource == null) throw new InvalidOperationException("A real native special-known source did not resolve.");
                TeleportationNativeCastResource specialLease = specialSource.Capture();
                specialLease.Spend();
                bool specialSpent = specialLease.ObserveExpenditure() == TeleportExpenditure.ExactlyOne;
                object specialEvidence = specialLease.Evidence();
                bool specialRestored = specialLease.RestoreAndVerifyExactResource();
                assertions.Add(Assertion("teleportation-resource-special-known-source", "native special-known instance spends one actual spontaneous slot",
                    "specialOnly=" + specialOnly + ";spent=" + specialSpent + ";restored=" + specialRestored,
                    specialOnly && specialSpent && specialRestored, path));
                cases.Add(new { source = specialSource.Snapshot.Key, specialKnownOnly = true, specialOnly, specialSpent, specialRestored, evidence = specialEvidence });
                var staleBook = initial.First(value => ReferenceEquals(value.Book, spontaneous));
                owners[0].Descriptor.DeleteSpellbook(spontaneous.Blueprint);
                assertions.Add(Assertion("teleportation-resource-stale-book-absent", "removed spellbook cannot re-resolve from a prior action snapshot",
                    "resolved=" + (TeleportationSpellbookAdapter.Resolve(staleBook.Snapshot) != null),
                    TeleportationSpellbookAdapter.Resolve(staleBook.Snapshot) == null, path));
                // No legitimate spell result occurred in this resource-only probe.
                // Cleanup below removes fixture books entirely; no slots are granted to a campaign character in normal operation.
            }
            catch (Exception exception) { failure = exception; }
            finally
            {
                foreach (TeleportResourceFixtureOwner fixture in fixtures.AsEnumerable().Reverse()) fixture.Restore();
                cleaned = fixtures.All(value => value.IsRestored()) && originalParty.SequenceEqual(player.Party.Select(value => value.UniqueId)) &&
                    player.GameTime == originalTime && ReferenceEquals(map.PartyPosition, originalPosition) &&
                    map.TravelData == null && map.CurrentEncounterData == null && !_workingSaveSmoke.WriteObserved &&
                    baselineSources.SequenceEqual(TeleportationSpellbookAdapter.Enumerate(player).Select(value => value.Snapshot.Key));
            }
            assertions.Add(Assertion("teleportation-resource-fixture-cleanup", "original spellbook instances/resources, caster stats, party, map and time restored; no save write",
                "cleaned=" + cleaned + ";saveWriteObserved=" + _workingSaveSmoke.WriteObserved, cleaned, path));
            WriteTeleportationForensicJson(path, new { schemaVersion = 1, runId = _request.RunId,
                claims = "Lower-layer real spellbook enumeration, native Spend and exact resource restoration. No contextual button, confirmation, rules outcome, relocation or completed magical cast is qualified by this probe.",
                sources = sourceEvidence, cases, cleaned, saveWriteObserved = _workingSaveSmoke.WriteObserved,
                error = failure == null ? null : failure.ToString(), assertions });
            return CreateResult(failure != null ? RuntimeTestStatuses.Error : assertions.All(value => value.Status == "PASS") ?
                RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, failure == null ? null : failure.ToString());
        }

        private static string TeleportResourceFingerprint(Spellbook book)
        {
            return TeleportationDiagnosticJson.Serialize(new { id = book.Blueprint.AssetGuid, book.CasterLevel,
                levels = Enumerable.Range(0, 10).Select(level => new { level,
                    spontaneous = book.GetSpontaneousSlots(level), capacity = book.GetSpellsPerDay(level),
                    known = (book.GetKnownSpells(level) ?? Enumerable.Empty<AbilityData>()).Select(value => value.Blueprint.AssetGuid).ToArray(),
                    prepared = (book.GetMemorizedSpellSlots(level) ?? Enumerable.Empty<SpellSlot>()).Select(value => new {
                        value.Index, type = value.Type.ToString(), value.Available, value.IsOpposition,
                        spell = value.Spell == null ? null : value.Spell.Blueprint.AssetGuid }).ToArray() }).ToArray() });
        }
        private sealed class TeleportResourceFixtureOwner
        {
            private readonly UnitEntityData _owner;
            private readonly Spellbook[] _originalBooks;
            private readonly string[] _originalResources;
            private readonly int _intelligence, _wisdom, _charisma;
            private readonly bool _detached;
            internal readonly List<Spellbook> Books = new List<Spellbook>();
            internal TeleportResourceFixtureOwner(UnitEntityData owner)
            {
                _owner = owner;
                _originalBooks = owner.Descriptor.Spellbooks.ToArray();
                _originalResources = _originalBooks.Select(TeleportResourceFingerprint).ToArray();
                _intelligence = owner.Descriptor.Stats.Intelligence.BaseValue;
                _wisdom = owner.Descriptor.Stats.Wisdom.BaseValue;
                _charisma = owner.Descriptor.Stats.Charisma.BaseValue;
                _detached = owner.IsDetached;
            }
            internal Spellbook AddBook(BlueprintSpellbook blueprint)
            {
                if (_owner.Descriptor.GetSpellbook(blueprint) != null) throw new InvalidOperationException("Fixture never replaces an existing spellbook.");
                _owner.Descriptor.Stats.Intelligence.BaseValue = 30;
                _owner.Descriptor.Stats.Wisdom.BaseValue = 30;
                _owner.Descriptor.Stats.Charisma.BaseValue = 30;
                Spellbook book = _owner.Descriptor.DemandSpellbook(blueprint);
                Books.Add(book); // Record ownership before subsequent fixture operations can throw.
                for (int level = 0; level < 20; level++) book.AddCasterLevel();
                book.UpdateAllSlotsSize(false);
                return book;
            }
            internal void Restore()
            {
                foreach (Spellbook book in Books.AsEnumerable().Reverse())
                {
                    if (ReferenceEquals(_owner.Descriptor.GetSpellbook(book.Blueprint), book)) _owner.Descriptor.DeleteSpellbook(book.Blueprint);
                    book.Dispose();
                }
                _owner.IsDetached = _detached;
                _owner.Descriptor.Stats.Intelligence.BaseValue = _intelligence;
                _owner.Descriptor.Stats.Wisdom.BaseValue = _wisdom;
                _owner.Descriptor.Stats.Charisma.BaseValue = _charisma;
            }
            internal bool IsRestored()
            {
                return _originalBooks.SequenceEqual(_owner.Descriptor.Spellbooks) &&
                    _originalResources.SequenceEqual(_originalBooks.Select(TeleportResourceFingerprint)) &&
                    _owner.IsDetached == _detached && _owner.Descriptor.Stats.Intelligence.BaseValue == _intelligence &&
                    _owner.Descriptor.Stats.Wisdom.BaseValue == _wisdom && _owner.Descriptor.Stats.Charisma.BaseValue == _charisma;
            }
        }
    }
}
