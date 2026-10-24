# Cleanup 2: selected elemental invocation contract audit

Read-only audit on 2026-10-21 while frozen candidate
`827983eb74855bd5e1940c69d8b7cfec82b3e183` is being built. No production,
observer, frozen-worktree or shared registration file was changed. No game,
build or Git command was run for this audit. This file records engine/source
evidence for reviewing actual native diagnostic output; it does not relax the
current comparator or certify elemental equivalence.

## Exact references and inspection

Installed Kingmaker 2.1.7b `Assembly-CSharp.dll`:

- MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`.
- SHA-256 `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
- Installed location: `C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker/Kingmaker_Data/Managed`.
- Matching private disassembly:
  `C:/Dev/KingmakerGunslingerLab/private/charvis-native-il/Assembly-CSharp.il`.

Inspection used read-only PowerShell/.NET reflection-only type/member reads and
exact method-body extraction from that matching private IL. End-of-method
markers were matched completely, including newline, to distinguish `Spend`
from `SpendFromSpellbook`; constructor overloads and iterator owners were
matched explicitly. No proprietary assembly or disassembly is exported into
the repository. Method sizes and IL offsets below identify inspected contracts,
not a native-run receipt.

The retained installed-stack elemental inventory is
`artifacts/cleanup2-installed-elemental-inventory.txt`, SHA-256
`1a29acebde261915b3ccd46e46489005b3af756f951d57f7d9d333a6bd8bef3c`.
Its six exact standalone parent records contain spell-list components,
SpellDescriptorComponent, AbilityVariants and SpellComponent. Their recorded
own graphs contain no AbilityEffectRunAction or ContextRankConfig. That is an
observation about those six parent blueprints, not their four execution children.

## Selected child owns the actual execution context

The following sequence is verified in the installed engine:

1. Native memorized/spontaneous `GetConvertedAbilityData` reads the root's
   `BlueprintAbility.Variants`, creates each selected child with the original
   spellbook, and assigns `ConvertedFrom` to the original source spell.
   The root is a menu/invocation source; this does not execute it.
2. `UnitUseAbility.OnAction` (948 bytes) checks `Spell.IsAvailable` at IL `000e`
   and `Spell.CanTarget` at `0068`, then constructs
   `RuleCastSpell(Spell, Target)` at `018a`. Here Spell is the actual selected
   child AbilityData.
3. `RuleCastSpell(AbilityData, TargetWrapper)` (45 bytes) stores that same
   AbilityData as Spell and calls its `CreateExecutionContext` at `0022`.
4. `AbilityData.CreateExecutionContext` (19 bytes) calls `CalculateParams` on
   this AbilityData and constructs `AbilityExecutionContext(this, params,
   target, Rulebook.CurrentContext)`.
5. `AbilityExecutionContext` constructor (258 bytes) passes the selected
   `ability.Blueprint` to the base MechanicsContext constructor, with **null
   MechanicsContext parent** at IL `0013`, then stores the same AbilityData.
   `ConvertedFrom` is not converted into a MechanicsContext parent.
6. `AbilityExecutionContext.AbilityBlueprint` (12 bytes) returns
   `Ability.Blueprint`. `RuleCastSpell.OnTrigger` (415 bytes) ultimately calls
   that selected `Spell.Cast(Context)` at `0194`.
7. `AbilityExecutionProcess.<ProcessRoutine>d__15.MoveNext` (501 bytes) selects
   AbilityDeliverEffect at `0034`, AbilityApplyEffect at `0046` and
   AbilitySelectTarget at `005c` from **Context.AbilityBlueprint**. It does not
   enumerate ConvertedFrom's components.
8. `AbilityExecutionProcess.PrepareCast` (53 bytes) calls Context.Recalculate
   at `0013` and child IAbilityOnCastLogic at `002f`.
   `MechanicsContext.RecalculateRanks` (130 bytes) initializes m_RankSources
   from **AssociatedBlueprint.GetComponents<ContextRankConfig>** at `0011`.
   AssociatedBlueprint is the selected child, not the menu parent.
9. `AbilityEffectRunAction.Apply` (165 bytes) runs that selected component's
   Actions in the selected context at `0084`/`0089`. The effect stored on a
   different parent blueprint is not run alongside it.

`BlueprintExtenstions.GetComponents<T>`'s exact iterator
`<GetComponents>d__2<T>.MoveNext` (147 bytes) loops only the supplied
blueprint.ComponentsArray. `BlueprintScriptableObject.ComponentsArray` (29
bytes) returns that object's own component array, initializing an empty one if
needed. Neither routine follows BlueprintAbility.Parent or ConvertedFrom.

Both `AbilityData.Cast` and the AbilityExecutionContext constructor refuse a
blueprint that still HasVariants. Retained parent effects therefore do not
become a second direct cast through this selected-child path.

### Narrow, proved parent-comparison false negatives

For an exact native menu parent with a selected execution child:

| Parent component | Selected-child behavior | Audit conclusion |
| --- | --- | --- |
| Exact AbilityEffectRunAction, including nested parent spawn/clear actions | The execution process reads the child's AbilityApplyEffect | Parent effect payload is not executed through ConvertedFrom. Its presence/absence is not evidence that the child cast differs. |
| Exact ContextRankConfig | RecalculateRanks reads the selected context's AssociatedBlueprint | Parent rank configuration does not supply the selected child's context ranks. Comparing it against a standalone menu parent's missing rank is a false execution-equivalence requirement. |
| SpellListComponent | Actual book/level and membership have their own reads | Already independently verified by the learning helper; this is not authority to remove list membership. |
| AbilityVariants | Native menu creation and HasVariant/ConvertedFrom validation use it | Coverage and exact parent/child links remain required; it is not an execution action graph. |

This is a narrow conclusion about the exact known component types and this
variant path. It is not permission to ignore a subclass with added interfaces,
unknown parent logic, missing menu coverage, arbitrary parent fields, or the
**child's** action/rank components. The publisher retains unrelated native
parent components while replacing only the variants component; retaining
inactive parent effects is a publication property, not evidence they execute.

## What the engine reads from child versus original source

| Concern | Verified actual source | Preservation requirement |
| --- | --- | --- |
| Action category | AbilityData.ActionType (139 bytes) reads child ActionType after item, Quicken, free-action and spontaneous-metamagic rules. RuntimeActionType (90 bytes) applies cooldown handling to that result. UnitUseAbility's public constructor uses it. | Compare actual selected child action fields and metamagic/book conditions; do not declare a child action difference irrelevant because a root matches. |
| Full-round behavior | AbilityData.RequireFullRoundAction (70 bytes) checks the selected child's Blueprint.IsFullRoundAction, except spontaneous metamagic adds its own full-round rule. BlueprintAbility.IsFullRoundAction (57 bytes) reads the child's turn-based modifier component or private m_IsFullRoundAction. | Retain genuine child action/full-round or turn-based modifier differences. Parent availability can separately expose the parent's own requirements. |
| Caster and availability restrictions | AbilityData.IsAvailableForCast (552 bytes) reads child CasterCheckers and AvailabilityProviders, plus caster state, book restrictions, source-item, material and parameter requirements. The three BlueprintAbility checker/provider getters cache only that blueprint's own components. | No automatic inheritance of parent restrictions into the child. Keep every actual child checker/provider/resource difference; separately assess any root UI gating. |
| Targets and range | AbilityData.CanTarget (259 bytes) uses child TargetCheckers/target fields and child TargetAnchor. TargetAnchor (150 bytes) reads the child Range/target flags, with item/alchemist exceptions. | Actual selected-child target/range differences are capabilities. The direct group-toggle method does not itself prove that every caller/UI gate ignores root availability. |
| School and descriptor of execution | MechanicsContext constructor reads SpellComponent and SpellDescriptorComponent from AssociatedBlueprint. Its MechanicsContext parent is null here. | Child school/descriptor remains significant for rules, immunity and actor features. Parent descriptor can also matter to native AutoMetamagic, below. |
| Spell level | AbilityData.SpellLevel (117 bytes) gives OverrideSpellLevel and StickyTouch precedence, then recursively uses ConvertedFrom.SpellLevel, then actual book/item/list fallbacks. | Same actual list/book/level and unchanged source chains are essential; child identity alone does not establish slot equivalence. |
| Book availability | Private AbilityData.IsAvailableInSpellbook (66 bytes) recursively delegates to m_ConvertedFrom before actual Spellbook.CanSpend. It does **not** recursively call the parent's entire IsAvailableForCast. | Preserve the original source book, known/prepared identity and spell level. This is a different check from child availability. |
| Spell-slot spending | AbilityData.SpendFromSpellbook (46 bytes) recursively delegates to ConvertedFrom, then calls original Spellbook.Spend(this,false). SpendInternal (323 bytes) handles the actual book level, casting ability, spontaneous pool or available matching memorized spell. | Preserve original invocation and debit path. Native SpendInternal matches available slot spell data/blueprints; it does not select a slot by reading ParamSpellSlot directly. Do not claim ParamSpellSlot alone enforces an exact debit. |
| Material and ability resources | AbilityData.Spend (59 bytes) calls selected SpendMaterialComponent, inherited source item's SpendCharges, original-book SpendFromSpellbook, then selected Blueprint.GetComponent<AbilityResourceLogic>.Spend. Resource/ResourceCost getters also read the selected blueprint. | A root resource component is not automatically spent as an additional child resource; child resource logic and source-item charges remain actual costs. Item/special invocations must stay protected. |
| ParamSpellSlot | Native Get_HasRequiredParams reads it when the selected IAbilityParameterRequirement requires a slot. Native recall/restore components read it; KMG Acadamae also resolves it through ConvertedFrom. | Preserve it as real invocation metadata. Its limited role in ordinary native SpendInternal is not permission to drop it. |
| Fact and item provenance | ConvertedFrom setter copies the source Fact, validates conversion/variant relationship and copies metamagic for a variant. The copy constructor also carries original fact/book/metamagic and source chain. SourceItem reads Fact.SourceItem. | Fact overrides, source item, charges and selected ability parameters are meaningful. Ordinary book observations do not authorize replacing item/domain/NPC executions. |

The native memorized/spontaneous child creation and the repository's qualified
prepared-slot helper differ in construction details: native UI sets
ConvertedFrom explicitly; the helper uses the actual AbilityData copy
constructor and attaches ParamSpellSlot. Both preserve the original source
chain for ordinary casting, but this audit does not treat their bytes or every
special-source parameter as identical.

The three native checker/provider getters return an already-populated cached
array when present; they only enumerate current own components when that cache
is null. `ExpandedSummoningAbilityBuilder.CopyFields` copies declared ability
fields and clones arrays, then ConfigureOne replaces ComponentsArray. This
audit has not observed whether any relevant donor cache is populated at copy
time. Consequently component-payload equality alone is not an observation of
the actual cached checker/provider arrays. Their actual runtime contents and
availability remain native observations; no stale-cache bug is asserted here.

## Duration and actual child payloads remain significant

`ContextActionSpawnMonster.RunAction` (347 bytes) calculates DurationValue at
`002c`, CountValue at `003e`, and LevelValue at `0050` using its current context.
It passes that context to RuleSummonUnit at `00d1`, retains link/control flags,
registers the actual summon pool and runs AfterSpawn at `0139` under a target
scope for the new unit. That target scope does not swap the AssociatedBlueprint
to the menu parent. Native `MechanicsContext.SourceAbility` returns the
AssociatedBlueprint ability first; here this remains the selected child.

`ContextDurationValue.Calculate` (64 bytes) calculates dice/bonus from the child
context, doubles when IsExtendable and Extend apply, then converts Rate to
rounds. `IsExtendable` (54 bytes) reads private m_IsExtendable and whether the
dice/bonus expressions can be nonzero. Duration, Extend behavior, child ranks,
count, pool, control, links and AfterSpawn effects remain active semantics.
There is no blanket equivalence waiver for any of them.

The source builder adds `ContextActionSetSummonAlignment` to canonical SNA
AfterSpawn. Its actual RunAction sets the spawned target's alignment using the
caster context and chosen mode. That is an active child effect. It cannot be
removed from a comparison merely because source and canonical units share an
elemental species key. Whether a particular cast happens to produce the same
alignment is weaker than all-caster contract equivalence.

Two conditional unused-field conclusions are also verified, for possible
later review of actual mismatch paths; neither is implemented here:

- `ContextValue.Calculate` (192 bytes) switches on ValueType. Simple reads Value;
  Rank reads ValueRank; Shared reads ValueShared; property cases read their
  selected Property or CustomProperty. Inactive storage fields are not read by
  this method. The selected kind and its active payload remain mandatory.
- `AbilityData.RequireMaterialComponent` (35 bytes) is false when its actual
  MaterialComponent.Item is absent; SpendMaterialComponent (58 bytes) returns
  immediately in that case. A Count difference with both Item references absent
  cannot charge inventory in this ordinary native path. A real item reference,
  source item, null MaterialComponentData or special consumer needs its own
  treatment; this is not a material-cost waiver.

## Parent identity has real effects outside parent action execution

Dropping all parent comparison, or treating Parent and ConvertedFrom as mere
display data, would miss verified engine behavior:

- Native `SpellSpecializationParametrized.OnEventAboutToTrigger` (68 bytes)
  matches either the actual selected spell or its BlueprintAbility.Parent to
  the actor's parametrized fact, then adds +2 caster levels. A fact targeted at
  a standalone elemental root is not automatically targeted at canonical SM/SNA.
  For an actor with that source-specific benefit, canonical availability at the
  same level does not prove equivalent duration/parameters. Existing known
  invocations remain protected by the current helper; an unproved actor-specific
  selection situation must also preserve the discovery row.
- Native `AutoMetamagic.ShouldApplyTo` (167 bytes) and IsSuitableAbility (170
  bytes) explicitly inspect ConvertedFrom.Blueprint or BlueprintAbility.Parent
  and recurse. They can match exact ability lists and parent descriptors/types.
  Parent descriptor/type and source-specific metamagic eligibility are not
  generally irrelevant metadata.
- Native `MetamagicRodMechanics.IsSuitableAbility` (82 bytes) checks exact child
  or Parent whitelist membership, then ordinary spellbook/type/level rules.
  Native `IgnoreSpellResistanceForSpells` (64 bytes) also checks the exact child
  or Parent against an AbilityList. These are actual identity-sensitive seams;
  this audit has not asserted an affected active whitelist or actor.
- Repository `ElementalSummonInsight.IsNamedFamily` follows exact
  BlueprintAbility.Parent links. Its policy names the eighteen canonical SM/SNA
  roots and grants matching Fire/Earth/Air Insight summons +2 rounds when the
  cast is an ordinary book spell and the summon is temporary/linked. Standalone
  Elemental roots are outside that named set. This is a concrete source-level
  duration interaction when the owner has the trait and the child/unit links
  satisfy its checks. It is not an inactive parent RunAction difference.
- Repository `AcadamaeCastingPatches.ResolveCanonicalInvocation` reads the
  ConvertedFrom chain for the original book/level, Summoning descriptor and
  actual prepared slot. Same-turn summoning activation reads the selected
  ability's type/descriptor/full-round/action behavior; it does not key only on
  a canonical root name. These source paths reinforce preserving actual
  invocation chains and child metadata.

The strict comparator's explicit native-donor/canonical-unit species alias is
an identity mapping. It does not itself compare the two unit stat/fact graphs
or all actor-specific rules above. This audit provides no new unit-mechanics
equivalence receipt. Native canonical coverage, actual live child differences
and any demonstrated source-specific benefit remain separate evidence.

## Review boundary and next evidence

The current parent comparison can reject a canonical root solely because it
retains inactive own AbilityEffectRunAction/ContextRankConfig components that
the standalone menu parent lacks. The engine trace justifies identifying those
as false execution requirements, **without changing any current code**. A
future projection can distinguish exact menu/access metadata from selected
execution metadata, keeping child payloads and known identity-sensitive actor
contracts explicit. Unknown components must remain unproved.

Before any code relaxation, retain candidate one's actual 60-pair diagnostics
and per-list helper outcomes. Review the reported parent difference paths
against the narrow inactive components above, then review every remaining
selected-child difference. Actual action/full-round, duration/Extend/rank,
alignment, resources/material, restrictions, access level, unique child grants
and source-specific actor benefits must not be silently normalized.

No native Elemental result was available when this audit was written. No new
claim is made about visible PC/console learning rows, natural input, old-save
serialization, item/domain/NPC execution, whole-stack third-party patch
behavior or MENU-02 completion. Installed mods can patch the inspected native
methods; frozen-candidate hook ownership and native observations remain needed.
