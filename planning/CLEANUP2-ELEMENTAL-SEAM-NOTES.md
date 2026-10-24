# Cleanup iteration 2: standalone elemental presentation contracts

MENU-02 is **incomplete**. This record describes actual installed engine seams,
initial native observations, exact-identity policy and implemented PC/console
learning-list copies. Native proof of actual suppression remains pending.
No ability remapping has been implemented. The initial inventory has 29
explicitly incomplete owners; it cannot authorize a complete-consumer claim.

## Exact inspected inputs

Read-only inspection on 2026-10-21 used the actual installed Managed assemblies
under `C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker`.
The private refresh/extracted copies of `Assembly-CSharp.dll` have the same hash.

| Assembly | Version / MVID | SHA-256 |
| --- | --- | --- |
| Assembly-CSharp | 0.0.0.0 / `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7` | `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb` |
| UnityModManager | 0.33.0.0 / `54059519-2754-445a-b5ee-fdb9326336e2` | `63e5baf7b1738e4091b5fd17ccb738ecdb4d1dbf246061dfd55dc52835d52691` |
| 0Harmony12 | 1.2.0.1 / `918c071f-383e-46dc-a374-6879300cbe15` | `aa1cd48317254985d8b700cc74953477d1b40c3022ce9aa4c95ed2b8327e1292` |

Method bodies were read from the private disassembly
`C:/Dev/KingmakerGunslingerLab/private/charvis-native-il/Assembly-CSharp.il`,
whose MVID matches the installed DLL. The main worktree's private
`artifacts/inspection/bodyguard-native/Assembly-CSharp.il` also has that MVID.
No private disassembly or assembly is added to Git.

The integrator's guarded baseline run produced
`artifacts/cleanup2-installed-elemental-inventory.txt`, SHA-256
`1a29acebde261915b3ccd46e46489005b3af756f951d57f7d9d333a6bd8bef3c`.
Its 31 candidates comprise six ordinary parents, 24 ordinary element children
and Wriggling Man's unique infested action. The overall run remains FAIL due to
the integrator's retained prohibited-reference observer finding; extracting
useful observations does not turn it into PASS. The integrator reports exact
installation/settings restoration and unchanged saves.

## Verified native contracts

The following signatures and behavior were inspected in the exact-reference IL.
`AbilityData`, `BlueprintAbility`, `Spellbook`, `SpellSlot` and
`BlueprintSpellList` below are their ordinary Kingmaker runtime types.

| Surface | Verified signature | Implication |
| --- | --- | --- |
| Cached class-list entries | `SpellLevelList.SpellsFiltered : List<BlueprintAbility>` getter; public static `bool SpellIsNotLocked(BlueprintAbility)` | Filters DLC entitlement only. `BlueprintAbility.Hidden` does not suppress spell discovery here. The getter returns its cached list, not a fresh UI list. |
| Class list | `BlueprintSpellList.GetSpells(int) : List<BlueprintAbility>` | Returns `SpellsByLevel[level].SpellsFiltered`, so globally changing this affects more than UI. |
| Actual class membership | `BlueprintAbility.IsInSpellList(BlueprintSpellList) : bool` | Reads each level's raw `Spells` list. Removing raw entries changes membership used by other consumers, including item/class rules. |
| Actual known spells | `Spellbook.GetKnownSpells(int) : IEnumerable<AbilityData>` | Returns `m_KnownSpells[level]` itself. It is a core rules/serialized-state seam and must not be used as a global presentation filter. |
| Actual preparations | `Spellbook.GetMemorizedSpells(int) : IEnumerable<SpellSlot>` | Filters the stored `m_MemorizedSpells[level]` slots. Existing preparations should keep these exact objects. |
| Saved spell level | `Spellbook.GetSpellLevel(BlueprintAbility) : int`; overload for `AbilityData` | The former reads `m_KnownSpellLevels`; the latter adds metamagic cost. Retaining old definitions/known data retains this identity mapping; changing display alone does not grant canonical access. |
| PC spellbook presentation | `Kingmaker.UI.ServiceWindow.SpellBookView.GetSpellsForLevel(int) : List<AbilityData>` | Builds a fresh result from known + special + custom spells. A context-aware result filter/grouping can leave all underlying known/prepared state intact. |
| Console spellbook presentation | public static `Kingmaker.UI.Common.UIUtilityUnit.GetKnownSpellsForLevel(int, Spellbook) : List<AbilityData>` | Builds a fresh known + special + custom result; used by `SpellBookKnownSpellsVM` and the memorizing panel. Any filter must retain access to old preparations. |
| PC learning | `Kingmaker.Assets.UI.LevelUp.CharBSelectorLayer.FillSpellLevel(SpellSelectionData, int, bool, CharBFeatureSelector) : void`; `FillSpellLightUpdate() : void` | The first reads `CurrentSpellSelectionData.SpellList.GetSpells(level)`; the second updates visibility/interactability. A UI-scoped copy at the first call site avoids changing the global list. |
| Console learning | private `Kingmaker.UI._ConsoleUI.CharGen.Phases.Spells.CharGenSpellsPhaseVM.GetSpellsCollection(LevelUpController, BlueprintSpellList) : List<CharGenSpellSelectorItemVM>` | Builds fresh selection VMs from the actual list. PC filtering alone will leave this discovery path visible. |
| Learning availability | `SpellSelectionData.CanSelectAnything(UnitDescriptor) : bool`; `CanSpendSlot(int spellLevel, int slotIndex) : bool`; `SpendSlot(int spellLevel, BlueprintAbility spell, int slotIndex) : void` | UI hiding must also prove a valid selection budget. A hidden duplicate cannot remain the last apparently selectable spell after the canonical spell is already known. |
| PC action-bar spell selector | private `Kingmaker.UI.ActionBar.ActionBarGroupElement.FillSlots() : void`; public `ActionBarSubGroupLevels.AddSlot(int, MechanicActionBarSlot) : void` | Builds fresh spontaneous/memorized presentation slots. A narrowly scoped insertion filter/grouping is possible without touching saved hotbar arrays. |
| Console action-bar selector | private `Kingmaker.UI._ConsoleUI.ActionBar.ActionBarSelectorVm.CollectSpells() : void`; private `AddSlotToCollection(MechanicActionBarSlot) : void` | Separate collection path. It preserves original spell/slot references when constructing its display slots. |
| Variant popups | virtual `GetConvertedAbilityData() : List<AbilityData>` on `MechanicActionBarSlotAbility`, `MechanicActionBarSlotMemorizedSpell`, and `MechanicActionBarSlotSpontaneusSpell` | Native slots read `BlueprintAbility.Variants` and create invocation children. `AbilityData.Variants` returns null and `InitVariants()` is a no-op in this exact installed binary; they are unsuitable seams. |
| Saved slot presentation | virtual `GetTitle() : string`, `GetDescription() : string`, `GetIcon() : Sprite`, `GetContentData() : object` on those three slot types | Slot-specific presentation can be adapted while retaining its original execution data. Tooltips/content need consistent treatment; a title-only change is insufficient. |
| PC spellbook item | `Kingmaker.UI.ServiceWindow.SpellItem.SetUp(AbilityData, SpellSlotType, bool, bool, bool, Spellbook) : void` | Provides the actual invocation and book to an item-specific presentation adapter. |

Related repository examples are `ExpandedSummoningVariantMenuPatch`,
`AlternateSummoningAdapterBuilder`, `CotwSpontaneousSlotCompatibility`,
`AcadamaeCastingPatches`, `MagicCircleSpellListPublication` and
`ShieldOtherSpellListPublication`. These demonstrate installed ABI use and
transaction/caching patterns; their former scope is not an elemental allowlist.

## Invocation preservation: useful seam and hard limit

The native memorized and spontaneous variant popups create each child with
`new AbilityData(childBlueprint, originalSpellbook)` and assign
`ConvertedFrom = originalSpell`. The original `SpellSlot` and its stored spell
remain intact. `AbilityData.SpendFromSpellbook()` recurses into `ConvertedFrom`
before calling the original book's `Spend(AbilityData, bool)`. This is the
verified native mechanism for retaining the original slot debit.

However, `AbilityData.Spend()` also spends item charges through `SourceItem` and
reads `AbilityResourceLogic` from the **execution child's blueprint**. Copying an
ordinary SM/SNA child and assigning `ConvertedFrom` does not preserve a special
class resource, item charges, original action/duration/ranks or restrictions.
The `ConvertedFrom` setter also validates actual conversion/variant lineage;
it is not an arbitrary remapping permission. Its copy constructor
`AbilityData(AbilityData other, BlueprintAbility replaceBlueprint = null)`
copies caster/fact/book/metamagic and links conversion, but does not prove the
remaining invocation properties or a special consumer's equivalence.

The compatible direction is to keep the original serialized spell and source
slot, and supply UI-only family presentation or source-owned execution children
derived from that consumer's original graph, as the alternate adapters already
do. Each such route still needs actual native slot/resource/action/casting and
save/reload tests. A player whose sole access is a domain, Flamewarden grant or
item must retain it even when an ordinary wizard has the same elemental choice.
Legacy preparations that are hidden without an accessible way to spend their
exact old slots are an incomplete repair, even if an already pinned hotbar
button still casts them.

## Initial consumer findings and explicit protected dispositions

The 720 observed references include duplicated raw/cache references; they are
not 720 distinct capabilities. Grouped references: 84 spell-list, 38 feature,
450 parametrized-feature, 52 usable-item, 48 ability parent/child, two buff,
39 unit, three AI-action and four weapon references. Full owner scanning is not
complete, and component header `action=Standard` alone omits effective full-round
casting and other invocation fields. It cannot prove equivalence.

The ten observed ordinary class lists are candidates for **future context-aware
discovery** filtering only after access, level and full contract proof: Bard,
Cleric, Druid, Wizard, Thassilonian Abjuration/Conjuration/Enchantment/Necromancy/
Transmutation and Wizard Conjuration. They are identified by exact GUID in the
new policy. No route is currently authorized to hide.

| Known special consumer | Exact owner identity | Current disposition |
| --- | --- | --- |
| Fire Domain spell list | `d8f30625d1b1f9d41a24446cbf7ac52e` | Preserve Huge Fire at VI and Elder Fire at VIII; ordinary canonical access is not proven for its domain slot. |
| Travel Domain spell list | `ab90308db82342f47bf0d636fe941434` | Preserve Greater Air at VII; retain domain access and slot semantics. |
| Weather Domain spell list | `eba577470b8ee8443bb4552433451990` | Preserve Small parent at II; retain domain access. |
| Enemy of the Fey | `b08a858f2e4887f45aca4c09893a0adc` | Preserve its four Elder element choices; special source graph needs inspection. |
| Flamewarden Spellcasting 1 | `2cb57c0d74d143f4da1ef6704012b0f4` | Preserve Small Fire grant; canonical replacement access at the grant's actual level is unproved. |
| Flamewarden Spellcasting 3 | `0dc70999341cdd644be0b972dd83900b` | Preserve Medium Fire grant. |
| Flamewarden Spellcasting 4 | `891441bcd187d4d43abc46d3202e07a0` | Preserve Large Fire grant. |
| Tristian Angel buff | `f16954c5c8cb0834baace64a167aa3cb` | Preserve its two ability references; special consumer, not ordinary learning. |
| Fireborn Warhammer | `854e566488509a04994101344c3c4e2a` | Preserve Small Fire item action and charges/resources. |
| Lava-Forged Axe | `c93fc085584f26d49a0a0d0f5f876a0e` | Preserve Huge Fire item action. |
| Whirlwind Composite Longbow | `1f546ab76bb0e77478ad08248795f7d7` | Preserve Medium Air item action. |
| Eye of the Tornado bow | `b6c71cc97303b0f4f9f0a28e14bbb66e` | Preserve Medium Air item action. |
| Wriggling Man infested summon | ability `03b02f96bf14a7d4290eb2605bd2c15e`; unit `baf4cbd8e093b314ea73b9315551af33`; AI `99b64ef562a89cf479ed6c312adbf695` | Explicit unique exception: summons `96aa2542f4fd17d4f901f2379e723fd4` Infested Elder Air. Never classify as an ordinary redundant spell. |
| Castle of Knives Wriggling Man | `8799ba5ac241abe49b6240ce1c07a6a5` | Preserve original selected spells and facts. |
| Nereid summon AI | `06823bad9fe04e249a44a707a56880f9` | Preserve Huge Water native AI route. |
| Nereid lair boss AI | `1847ac733955b7c4384a537df0b70ab6` | Preserve Huge Water native AI route. |
| Nereid lair metamagic feature | `d26acdac8ded95b44b787c9700634fc9` | Preserve its Huge Water metamagic association. |

All 24 element scrolls retain their original Ability/learned-parent links. All
four Medium-element wands retain their original item actions. The four Belt of
Perfect Components features, Feyspeaker Magic, Mystic Theurge parameters,
Arcane New Arcana/Magus parameters, Spell Specialization parameters and existing
prebuilt selected spells remain protected pending exact source-specific proof;
removing a parameter can alter a character's only legal choice. Nereid/Evindra
campaign unit variants retain their AddFacts/AI behavior. Nothing here authorizes
global `Hidden`, deleting definitions, rewriting saved blueprints, scrubbing
known spells or clearing preparations/pinned hotbar slots.

The 29 incomplete owners remain individually identifiable in the original
inventory. They include BlueprintRoot and player Druid/Saken pregen feature/unit
graphs, so they cannot all be waived as irrelevant NPCs. Root is narrowing that
read-only observation rather than silently increasing a bound or declaring it
complete. Any new exact consumer must receive a disposition before suppression.

## Minimal proposed implementation boundary after proof

1. Keep all native definitions, list membership and serialized invocation
   objects. Build an exact ability/consumer disposition catalog from the
   completed installed-stack inventory.
2. For ordinary learning, filter a fresh list at the PC/console UI call sites
   only when the same character/book can select an equivalent canonical parent
   at the same spell level, all four required element choices are published,
   and level-up selection budgets still complete. Do not mutate global
   `SpellsFiltered` or raw list membership, which also serves NPC grants/items.
3. For known/prepared/pinned routes, implement compatibility presentation around
   the original invocation and original slot budget. Grouping/labels must make
   any remaining distinct resource access understandable without creating a
   second ordinary standalone discovery row. Keep unresolved source contracts
   individually visible as incomplete coverage.
4. Use one immutable publication/proof state enabled only after successful
   Expanded Summoning publication. UI hooks consult it and otherwise return
   original results. Failed initialization clears that state; cold module-off
   allocates no presentation state and exposes original lists. Any necessary
   in-memory publication mutation needs recorded reference ownership and exact
   cache restoration; source-owned saved objects remain untouched.

The installed Harmony12 facade supports the verified signatures
`DynamicMethod Patch(MethodBase, HarmonyMethod, HarmonyMethod, HarmonyMethod)`,
`void Unpatch(MethodBase, HarmonyPatchType, string)`,
`void Unpatch(MethodBase, MethodInfo)`, `Patches GetPatchInfo(MethodBase)`, and
`IEnumerable<MethodBase> GetPatchedMethods()`. Installed UMM's `ModEntry.OnUpdate`
is `Action<ModEntry,float>`. These are available integration surfaces. The
learning hooks below use ordinary repository Harmony attribute discovery.

## Pure policy foundation and checks

`StandaloneElementalPresentationPolicy.cs` contains the exact six observed
parent and 24 child identities plus the explicit unique Wriggling action. It
records canonical species/tier/SM/SNA parent metadata and protected consumer
route dispositions. Its positive discovery predicate requires a complete
inventory, exact same ordinary list/book/level, correct canonical parent,
complete published creature coverage, full invocation-contract equivalence,
intact old references, original resource/slot retention and reachable old
invocations. Learning additionally requires a verified selection budget.
Every proof flag defaults false. The production learning helper now constructs
scoped evidence from the actual list/book/publication/graphs as described below.

Four focused tests in `Cleanup2ElementalPresentationTests.cs` cover identity and
canonical metadata, incomplete inventory/saved invocation preservation, protected
domain/item/NPC/unique routes, and each missing access/coverage/compatibility
proof. The positive test uses explicitly synthetic evidence only. Integrated
compilation/test execution remains with the integrator.

Read-only source consistency found all 31 observed native candidate GUIDs in the
explicit policy mapping, with none missing. This does not qualify equivalence,
native UI behavior, old-save casts or MENU-02 completion. No new game launch,
save write, installed-file mutation, assembly export or release action occurred
in this lane.

## Implemented learning-only presentation, awaiting native behavior evidence

The integrator authorized actual UI implementation after confirming that the
ten ordinary class lists are complete within the initial observer's bounds.
The incomplete pregens, BlueprintRoot and deeper NPC owners retain their
separate protected dispositions. This is scoped evidence for one ordinary
learning call; it is not a waiver of the remaining consumer inventory.

`StandaloneElementalDiscoveryPatch.cs` replaces exactly one native
`BlueprintSpellList.GetSpells(int)` call in each verified learning method:

| Native method | Original body | Exact call | Replacement arguments |
| --- | --- | --- | --- |
| PC `CharBSelectorLayer.FillSpellLevel` | 511 bytes | IL `00c3` | original list/level, actual `SpellSelectionData`, actual layer |
| Console `CharGenSpellsPhaseVM.GetSpellsCollection` | 298 bytes | IL `001f` | original list/level, actual VM, actual `LevelUpController` |

Installation requires the exact observed game MVID, signatures, body lengths,
console selection/book field types and exactly one matching call with no
exception-block boundary. An unrecognized seam is preserved. Existing labels
on the replaced instruction transfer to the first inserted context load.
The console constructor was also inspected: it assigns `m_SelectionData` at
IL `0082` and `PreviewSpellbook` at `00a5` before invoking selection collection.
These fields are available at the installed call site.

Both helpers return a fresh list and retain the native continuation. The shared
`FilterLearningSpells` requires successful blueprint initialization, active
Expanded Summoning, the exact observed ordinary list, exact selection list,
the same selected/preview blueprint spellbook and that book's actual ordinary
list. Missing/ambiguous context and stat-dependent extra learning preserve all
rows. Independently listed elemental children are distinct grant routes and
remain visible; only the six exact observed parents can qualify.

For each parent, proof requires a single exact source reference on this level,
one same-level canonical SM or SNA parent on the same list, an actual published
Expanded Summoning variants component, all four exact source native children,
and all four exact registered canonical single-creature children currently
published exactly once. Ambiguous same-GUID object replacements are retained.

`StandaloneElementalInvocationContract.cs` compares actual parent invocation
fields, including private full-round/target flags, action type, targets, range,
metamagic, material cost and cast-animation fields. It then compares every
non-membership/non-variant component in order, recursively reading public and
private payload fields. This includes resources, checkers, rank configuration,
all action/condition branches, full nested AfterSpawn buff payloads, summon
pool/count/link/control and duration extendability. Blueprint references remain
exact GUID leaves. Only the native elemental unit donor and the matching
registered canonical unit receive the same explicit species identity, after
every spawn was verified against its appropriate exact unit reference. This
uses the released canonical species mapping; it does not infer equivalence by
name. Other differences, cycles, unsupported object types or traversal limits
preserve the row and report the differing field path/reason. No alignment or
AfterSpawn difference has been waived. Whether the installed native graph
actually qualifies is an outstanding native observation, not an asserted PASS.

Every spell currently selected anywhere in `LevelCount[].SpellSelections` or
`ExtraSelected`, and every already-known legacy invocation, is retained. The
helper writes neither those arrays nor any ability/slot object. Before filtering
it verifies that the remaining distinct unknown, unselected selectable rows
can satisfy free per-level slots plus all applicable free extra slots. PC
`SetupSpellSelectorView` omits `UniqueSpellComponent` rows, so these do not
count toward either UI's conservative budget. Extra slots are charged fully to
this level, even where other levels could supply them; this can retain a
duplicate but cannot consume a mandatory choice. An already-known canonical
parent cannot make the last legal legacy learning row disappear.

The actual shared copy/filter function is covered by three additional focused
tests: a positive removal retaining selected/known/foreign references and order,
distinct availability plus ordinary/extra learning budgets, and unverified
context fallback. Existing four identity/access/protected-route tests remain.
The integrator reported production compilation against the actual installed
references PASS in 9.4 seconds. Focused execution/native behavior evidence is
owned and recorded by the integrator; this lane has not independently run it.

Native observation entry points are `FilterLearningSpells` (the actual UI
decision helper), `SnapshotDispositions()`, `PcSeamState` and `ConsoleSeamState`.
Dispositions retain at most 192 distinct entries of at most 768 characters,
including exact list, level, source and contract/budget reason. They distinguish
an installed hook from an eligible proof and actual collection filtering.
No global `GetSpells`, `Hidden`, raw list, known/prepared/hotbar/domain/item/NPC
surface is patched. Fresh module-off/uninitialized calls return original rows
in a fresh copy. This lane adds no persistent publication state requiring
rollback. Known/prepared/hotbar compatibility presentation and remaining
discovery routes remain explicit MENU-02 work; preserving them is not closure.

## Added bounded native elemental diagnostic phase

The integrator requested the additive
`RuntimeTestRunner.SummonCleanup2ElementalReview.cs` partial only; shared request
phase registration and execution remain integrator-owned. Its entry point is
`IEnumerable<int> ReviewSummonCleanup2Elementals(ExpandedSummoningCorrectionFixture)`.
It requires the exact automatic-exit Cleanup 2 `elementals` request, the exact
request-owned disposable caster clone and a zero-write observation. It does
not commit progression, open a human save, replace any blueprint/list or create
synthetic candidate rows.

The observer records the following separately:

- Actual installed PC and console transpiler ownership, contract state, native
  method body sizes and exact console selection/book field types.
- All 60 actual source/canonical contract pairs: six ranks times two families
  times parent plus four children. Every equality rejection remains a verdict,
  with native signatures and up to 16 scalar field differences from the
  production strict reader. A completed diagnostic census is not an equivalence
  PASS; zero qualifying rows is useful retained evidence.
- All ten observed ordinary lists and six ranks per list. Absent membership,
  ambiguous source membership and lists without an exact existing native
  blueprint spellbook receive explicit exceptions. Where a released book
  exists, the phase uses its actual attached native `Spellbook` and native
  `SpellSelectionData(BlueprintSpellbook, BlueprintSpellList)` with
  `SetLevelSpells(int level, int count)`. It invokes the exact production
  `FilterLearningSpells` and separately records its actual live proof reason.
  Known spellbooks that already know the legacy source remain protected; the
  contract census still diagnoses their underlying pairs.
- Current selection is allocated using native
  `SpendSlot(int level, BlueprintAbility spell, int slotIndex)` and checked with
  `CanSpendSlot(int level, int slotIndex)`. The same selected legacy reference
  must remain present and allocated. This corrects an earlier notes typo that
  gave SpendSlot only the two availability arguments.
- Native known `AbilityData`, prepared `SpellSlot`/spell and action-bar object
  references are compared around the actual helper. All watched raw/cached
  ordinary and special lists, component arrays, variant arrays and graph
  object references must remain exact. Every independently listed elemental
  child and every special-list source is individually recorded without hiding;
  the unique Wriggling action remains explicit.
- One native legacy Small Air invocation and one actual canonical SM II Small
  Air invocation execute through the existing qualified native
  `UnitUseAbility`/`RuleCastSpell`/spawn seam on the disposable Wizard. Each must
  retain its original known/prepared references before execution, debit its
  original prepared slot once, preserve all resource objects/amounts (ordinary
  controls must have zero extra resource components), and create its own exact
  expected unit with caster provenance. They are distinct invocations; no slot
  or resource remapping is introduced. Cast acceleration is the established
  disposable harness and does not certify natural input timing.

Additional real books are owned and removed from the disposable caster only,
preserving its original book references. Fixture-granted known/prepared spells
are destroyed with that caster in the existing shared teardown. Shared
blueprint/list identities and no-save-write status are checked again.

Safety review of the actual native IL found that `Spellbook.CasterLevel` clamps
to `SpellsPerDay.Levels.Length - 1`. New-book setup therefore selects only actual
class-bound books with at least 21 daily table entries and uses at most 20
`AddCasterLevel` calls, refusing an unexpected final level. It never waits
indefinitely for a clamped or modified book. The actual
`BlueprintSpellList.GetSpells(int)` returns its native empty list for an
out-of-range rank, so missing higher rank membership remains a normal explicit
exception. All command execution loops are bounded by the existing 5000-tick
qualified fixture limit; the review yields between lists and casts.

The integrator reported the initial new partial's production compile PASS in
11 seconds and the focused selection PASS 29/29, including the three shared
learning-filter tests. The subsequent explicit book-setup bound is a source
safety refinement and awaits the integrator's final compiled artifact. No
elemental native result has yet been reported.

This first phase deliberately makes no visible PC/console learning-UI claim:
the shared fixture has already advanced a Wizard to CL20 and does not establish
an actual eligible active level-up preview. Creating a selection allocation for
the actual helper qualifies that lower seam only. Natural learning UI capture,
old-save serialization, domain/item/NPC execution and complete MENU-02 closure
remain outside this bounded diagnostic. No native execution result is recorded
here until the integrator runs this exact observer artifact.
