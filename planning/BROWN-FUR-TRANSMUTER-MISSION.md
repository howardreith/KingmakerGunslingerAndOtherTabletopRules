# Brown-Fur Transmuter mission

## Engineering base authorization

- Verified engineering base:
  `a8b19fe39285da44ac443b7bcbd217870ec6ffb6` from
  `codex/weapon-visual-variety-firearm-fit-cleanup`.
- Cleanup human acceptance: **PENDING / intentionally deferred**.
- Brown-Fur authorization: the user explicitly overrode the original
  human-accepted-cleanup-base prerequisite and authorized Brown-Fur development
  directly from this pre-human cleanup candidate.

This authorization is limited to selecting the Brown-Fur source baseline. It
does not accept or complete the cleanup sprint, does not change the cleanup
acceptance report, and does not resolve its outstanding human visual review.
That work remains deferred for later.

## Mission boundary

Implement Brown-Fur Transmuter as the independent, default-enabled
`brown-fur-transmuter` feature module in the combined package. Call of the Wild
is a hard runtime prerequisite for Brown-Fur only; every unrelated module must
remain loadable when that dependency is absent or incompatible.

The mission proceeds through investigation, implementation, focused source and
runtime qualification, the seven-module 16-state boundary matrix, packaging,
guarded deployment, and one immutable pre-human candidate. The Brown-Fur human
acceptance gate remains mandatory. The final 128-state exhaustive matrix must
not run until the exact installed Brown-Fur candidate receives explicit human
acceptance.

## Current status

- Branch: `codex/brown-fur-transmuter-cotw-extension`.
- Release base: `0.0.80`.
- Preferred candidate identity: `0.0.81-brown-fur-transmuter`.
- Base gate: satisfied by explicit user override on 2026-08-15.
- Cleanup acceptance: still pending and outside the current work cycle.
- Brown-Fur acceptance: not yet requested; implementation investigation is in
  progress.

The inspected dependency structure and fail-closed progression authority are
recorded in `planning/BROWN-FUR-COTW-CONTRACT.md`.

Permanent identities and the archetype shell are now source-qualified. The
shell has exact level 3/9/20 additions, dynamically planned 3/9 or 4/10 exploit
removals, level-20 Magical Supremacy removal, one six-variant Powerful Change
selector, and the Share Transmutation activatable. Its 19 manifest identities
remain `reserved`, the builder is not invoked, and no archetype is published
until the cast mechanics, complete spell classification, and isolated runtime
registration path are qualified.

The live modifier-provenance contract is qualified on commit
`2c18c84d44be6907d3d30dbdd5a42f7d8a1bcef1`: all 11 guarded assertions
passed, all six installed ability-bonus carrier families resolved, and the
engine exposes enough source, descriptor, destination-stat, and execution
context identity for a narrow cast-scoped Powerful Change adapter. Mechanical
stacking, recast, persistence, polymorph, and size behavior remain unqualified;
the archetype remains reserved and unpublished.

The next carrier checkpoint passed on commit
`7008c0e1067f47c713612c3d751f7519a9a0c62d`. Seven guarded assertions
proved exact runtime value and descriptor registration for five real installed
buffs covering `AddStatBonus`, `AddContextStatBonus`,
`AddStatBonusAbilityValue`, `Polymorph`, `AddGenericStatBonus`, and
`ChangeUnitSize`; exact caster/spell/target/caster-level provenance survived
the engine's child-context clone; every modifier was removed cleanly; every
stat returned to baseline; and the disposable units were removed. This narrows
Powerful Change to an execution-scoped value adjustment, but does not yet
qualify the adjustment, stacking/recast, dispel, or persistence. Brown-Fur
therefore remains reserved and unpublished.

The descriptor-preserving adjustment itself then passed on commit
`2a44f651d1fe6ca94bcae9cb7a9b945598bdbe04`. Five real installed carrier
cases changed `4 -> 6 Enhancement`, `2 -> 4 Enhancement`,
`6 -> 8 Enhancement`, `2 -> 4 Polymorph`, and `2 -> 4 Size` respectively.
Each transaction adjusted exactly once; wrong-stat scopes adjusted zero times;
scope release restored ordinary subsequent application; and removal restored
the baseline. Remaining Powerful Change work includes competition/recast,
dispel, level-20 `+4`, actual spell and reservoir commit integration,
cancellation/interruption, and persistence. Brown-Fur remains reserved and
unpublished.

The advanced carrier fixture passed on commit
`bc8f30c439c53d0ff4e00ae5bb39c1fef7608c71`. Native Enhancement competition
resolved weaker/equal/stronger cases correctly, ordinary-to-enhanced and
enhanced-to-ordinary recasts each left one correct buff, the enhanced value
survived transaction release, and the level-20 increase produced exactly
`+8 Enhancement` from the ordinary `+4`. Remaining Powerful Change carrier
gates are dispel and persistence; real cast/debit/cancellation integration is
still outstanding. Brown-Fur remains reserved and unpublished.

The CotW Shared Spells helper-body checkpoint passed on commit
`53306fb3367a94fdf2d60b535c7bb5fd72a678ea` with 12 guarded assertions. Exact
IL proves CotW's eligibility helper rejects `CannotBeShared`, non-spells,
casters without its two sharing facts, and null spellbooks. Its target helper
then supports only self, the `ac_share_spell` owner's pet, or a target carrying
`bonded_mind_feat` when the caster has `share_spells_feat`. These helpers are
not sufficient for Brown-Fur's authorized party, controlled-creature, summon,
and friendly-ally contract. Share Transmutation will use a separate
owner-scoped execution adapter with deterministic CotW patch ordering. No
player-facing archetype or targeting change is active yet.

The follow-up targeting-body probe passed all 13 guarded assertions on commit
`3f85642168d47998186860e36ac9ccff8d8de0fe`. CotW priority-400 prefixes fully
replace `AbilityData.TargetAnchor` and `AbilityData.CanTarget`, and their
Personal-spell branches call the same narrow Shared Spells helpers. Brown-Fur
will therefore compose through exact after-CotW postfix result overrides for
a validated per-cast identity, leaving every unmatched query unchanged. The
native approach-distance and delivery bodies remain the next investigation
gate; the archetype remains reserved and unpublished.

The delivery observer on `f12c33c9e5a13fb57f5505f27dfe6755b8104203`
captured the requested native bodies but failed one evidence assertion because
the inspected getters are inherited from `UnitCommand`, not declared on
`UnitUseAbility`. The corrected exact checkpoint
`29071bdbd059d09455b8d507eb8edf06d9ee6019` passed all 14 assertions. Native
Personal delivery is zero spell range plus caster/target corporeal radii and
uses ordinary command approach, establishing the Touch path. The exact
level-20 adapter can add 30 feet only to the matching cast's approach-distance
result while the policy rejects an over-30-foot target before commitment.
Targeting-scope implementation and live boundary qualification remain pending;
Brown-Fur is still reserved and unpublished.

The exact Share targeting scope and its after-CotW postfixes are implemented
but not yet wired to player intent. The disposable runtime fixture passed all
five assertions on commit `8f649db44b6a4c4e5cc980df08375f19085a2f0b`:
the installed Personal spell moved from CotW baseline `Owner/false` to scoped
`Unit/true`, a different target was rejected, Touch retained the native
1.0-meter contact radius, release restored baseline, and the capstone added
exactly 9.144 meters. The blueprint remained unchanged, scopes returned to
zero, and all disposable units were removed. Real ally effect delivery,
relationship classification, cast/debit integration, and interruption remain
pending; Brown-Fur remains reserved and unpublished.

The native cast-commit ordering checkpoint passed all 15 guarded assertions on
commit `38779fb6c5671d5bac7af5536b6f3a80c9d8a2a7`. Exact IL now proves that the
engine constructs and clones the execution context in the `RuleCastSpell`
constructor, performs spell-failure checks and submits successful execution in
`RuleCastSpell.OnTrigger`, and only then calls `AbilityData.Spend` from
`UnitUseAbility.OnAction`. Native spend is skipped for UMD failure but occurs
for ordinary and arcane spell failure. The exact package/DLL/MVID and contract
artifact identities are recorded in `planning/BROWN-FUR-COTW-CONTRACT.md`.
This establishes the pre-slot rejection and post-rule/pre-slot commit window;
the next gate is the exact `CreateExecutionContext` parameter path and
execution-process completion/interruption lifecycle. Brown-Fur remains
reserved and unpublished.

The refined lifecycle checkpoint then passed all 16 guarded assertions on
commit `475394e2216af64b547ebe1f79ed75e40abb61b4`. It proves that native
`CalculateParams` completes before the new execution context clones the
parameters; process `Tick` marks both ordinary completion and caught exceptions
terminal and invokes custom-logic cleanup; and command `OnEnded` remains the
separate no-process cancellation signal. This authorizes exact reference-keyed
command/rule/context/process retention and a context-local, non-stacking Extend
addition without shared blueprint or `AbilityData` mutation. Artifact
identities and the exact order are recorded in the CotW contract. Actual
resource-backed transaction wiring is now the next implementation checkpoint;
Brown-Fur remains reserved and unpublished.

The real CotW reservoir accounting fixture passed all seven guarded assertions
on commit `1c1fb1e3d4bf45d22437ab0547128c10f73edcf5`. A disposable owner acquired
the structurally resolved reservoir with three points; the combined two-point
cost changed it exactly from `3` to `1` once, exact restoration returned it to
`3`, a one-point balance rejected a two-point request without mutation, and a
removed resource rejected with `reservoir-not-owned`. The resource and unit
were removed on cleanup. This qualifies the narrow native resource adapter,
not its pending attachment to the live command/rule/process cast lifecycle.
That adapter is now attached to the inert production cast boundary described
below; native submitted-spell and player-owner proof remains pending.

The production cast transaction fixture passed all eight guarded assertions
on commit `4d5f599457a1b5eb3d012dbbcfc1fc89344585f0`. The live Harmony registry
proved the rule-commit prefix is ordered after CotW. A combined intent retained
one transaction/reservation plus Share and Supremacy scopes, added Extend to
the exact installed spell context, and debited the real reservoir exactly
`3 -> 1`. The exception path restored exactly to `3` and released all scopes.
A post-reservation shortage rejected at one point without mutation, suppressed
exactly one native `AbilityData.Spend`, and cleaned the suppression. A request
starting with one point failed before retaining any cast state. Exact package,
DLL, MVID, deployment, backup, structured-artifact, and runtime-result
identities are recorded in `planning/BROWN-FUR-COTW-CONTRACT.md`.

This closes production-boundary reservation, combined debit, rollback,
pre-slot suppression, and scope cleanup for an already validated intent. It
does not yet publish or discover player intent, submit a real spellbook cast,
measure the real slot delta, deliver the shared effect to an ally, qualify
process interruption, or prove save/reload persistence. Brown-Fur remains
reserved and unpublished.

The native CotW Arcanist slot fixture then passed all six guarded assertions
on commit `0fa4e97f89eb16e594e1475caff8b517d6c187e0`. A disposable native
level-five Arcanist owned both resolved CotW spellbooks. Its Beast Shape I
wrapper/wolf-variant invocation retained the exact casting book as source.
The combined boundary committed once, changed the real reservoir `4 -> 2`,
and changed real level-three slots `6 -> 5` through native
`AbilityData.Spend`. A rule-failure rollback restored the reservoir to `4`.
The post-reservation one-point race rejected, left the reservoir `1 -> 1`,
consumed the one-shot spend suppression, and preserved slots `6 -> 6`. Exact
package, DLL, MVID, deployment, backup, structured-artifact, and runtime-result
identities are recorded in `planning/BROWN-FUR-COTW-CONTRACT.md`.

This closes the submitted native source-spellbook and spell-slot boundary for
an already validated combined intent. Player intent discovery, real Brown-Fur
feature ownership, applied ally delivery, process completion/interruption,
and save/reload persistence remain publication gates. Brown-Fur remains
reserved and unpublished.

The guarded native combined-cast fixture then passed all six assertions on
commit `442963a26108fa41a89accabcc7c3209373aceae`. A real spellbook-backed
Beast Shape I wolf cast ran through native `UnitUseAbility`, completed its
execution process, and completed the production Brown-Fur transaction. The
real CotW reservoir changed exactly `4 -> 2`, available level-three Arcanist
slots changed exactly `6 -> 5`, and the installed wolf buff appeared once on
the disposable ally and not on the caster. Its Strength modifier was exactly
`+4 Polymorph`, with exact caster, target, and selected-spell provenance and a
positive duration. Every retained scope, reservation, buff, and disposable
unit cleaned up. Exact package, DLL, MVID, deployment, backup, and structured
evidence identities are recorded in
`planning/BROWN-FUR-COTW-CONTRACT.md`.

This closes native combined-cast completion and ally-effect delivery for an
already validated intent. Player intent discovery, real Brown-Fur feature
ownership, native post-submission interruption, dispel, and persistence remain
publication gates. Brown-Fur remains reserved and unpublished.

The first scoped Transmutation Supremacy fixture passed all five guarded
assertions on commit `49d785c43b2c389d63e3a5abebd49c9288191943`. One exact
execution scope added native `Extend` to one installed Personal Transmutation
spell context, duplicate retention was rejected, an already-Extended context
was not modified again, and scope release restored ordinary subsequent
context construction. No shared spell range, metamagic support, or spell level
changed, and all scopes and disposable runtime state cleaned up. The package,
DLL, MVID, and structured-result identities are recorded in the CotW contract.
Timed-duration behavior, slot and casting-time preservation, special duration
structures, and real owner/transaction wiring remain pending; Brown-Fur stays
reserved and unpublished.

The corrected timed-duration fixture passed all seven guarded assertions on
commit `2d91e4898761feaf46731a15c92900c84f40aef6`. A complete native
five-round duration calculated as `5` ordinarily, `10` in the exact Supremacy
scope, `10` when already Extended, and `5` after release. The installed
spell's action type remained `Standard`, blueprint range and metamagic support
were unchanged, and all runtime scope and disposable-unit state cleaned up.
The exact package, DLL, MVID, and result hashes are recorded in the CotW
contract. Prepared-slot and actual Metamixing inputs, instantaneous and
permanent spells, variant/converted spells, live concurrency, and real
owner/transaction wiring remain pending; Brown-Fur remains reserved and
unpublished.

The installed spell inventory exposed two timed exceptions that do not support
ordinary Extend: Resonating Word and Obsidian Flow. The initial exact fixture
on `4df227c4eeeecd06f42d2000bf44962cc2a6bc38` failed closed at `3 -> 3`
and `600 -> 600`, proving the generic scoped metamagic flag was insufficient.
The named, execution-reference-scoped, post-CotW adapter on
`f6048539d0e609ed4fd0787b3e18e73e478f5746` then passed all nine guarded
assertions at `3 -> 6` and `600 -> 1200`. It left the shared blueprint range,
metamagic mask, spell level, and action type unchanged and released every
scope. Exact artifact identities are recorded in the CotW contract.

This closes both exceptional timed duration carriers in the installed
inventory without a generic fallback. Instantaneous/permanent no-op proof,
deterministic classification of all 177 inventory rows, player intent, real
feature ownership, interruption, dispel, and persistence remain publication
gates. Brown-Fur remains reserved and unpublished.

The subsequent complete no-Extend scan found three hidden one-hour area
durations under Earth Tremor's blank-duration wrapper. A broad provisional
adapter correctly failed qualification when spread and cone doubled twice;
structured evidence showed those two CotW paths already honor scoped Extend,
while line needs the named adapter. The narrowed implementation on
`7f282f91b2fd8859274711e164b5de683ab261fd` passed all ten assertions:
spread, cone, and line each calculated exactly `600 -> 1200`, and no path
reached `2400`. Exact immutable identities are recorded in the CotW contract.

This closes every positive hidden-duration carrier discovered among installed
Transmutations lacking ordinary Extend support. Zero-duration and permanent
no-op proof still precedes final classification. Brown-Fur remains reserved
and unpublished.

The corrected instantaneous/permanent/selector fixture on
`92aa73a35f218ef6ae994b7015161cbc88c236e1` then passed all eleven guarded
Supremacy assertions. Jolt's one zero-round carrier, Baleful Polymorph's one
zero-round carrier and permanent application flag, Disintegrate's four
zero-round carriers, Stone to Flesh's absent duration graph, and Earth
Tremor's non-executable three-variant parent selector all remained unchanged.
The earlier fixture failure is retained as evidence that a variational parent
cannot be used as an executable spell context. Exact immutable identities are
recorded in the CotW contract.

This closes all identified non-Extend Supremacy duration structures in the
installed inventory. Deterministic classification of all 177 inventory rows,
player intent, real feature ownership, interruption, dispel, and persistence
remain publication gates. Brown-Fur remains reserved and unpublished.

The pure fail-closed inventory classifier then passed all six guarded
assertions on `b707fd5c3aa8773fb16ae3a3f968de1ab529b766`. All `86` installed
Arcanist Transmutation roots and `177` root/variant records were present with
complete required fields; `174` use generic contracts, the exact three true
Brown-Fur duration adapters are named, and `Unexplained = 0`. The classifier
rejects unfamiliar carrier families, hard-coded caster routing, and unknown
non-Extend duration structures. Earth Tremor cone and spread are recorded as
proven CotW-native hidden-duration paths, not mislabeled as Brown-Fur adapters.
Exact immutable identities are recorded in the inventory report.

This closes the installed static blueprint inventory classification gate.
Runtime `ConvertedFrom` canonicalization, player intent, real feature
ownership, restricted Personal-spell delivery, interruption, dispel, and
persistence remain publication gates. Brown-Fur remains reserved and
unpublished.

The isolated optional-registration checkpoint first failed closed on commit
`c0fbe1893115fc4fada88f6a14ac07990f1aab43` because all 19 permanent
Brown-Fur manifest entries were still marked `reserved`. Run
`20260815T2002264792969Z-a68800ceb56949cf821cbfd3c330ab9d` proved the
registry rejected the first entry before creating any Brown-Fur asset, the
archetype selector remained unchanged, and the package stayed ready. The
manifest and inherited validators were then generalized without changing any
GUID, making the optional identities active while retaining the core package's
independent 1,517-registration authority.

The corrected save-free contract observer passed all 13 assertions on commit
`dd35e276cac658f3976aaddbcf4f6f61cd7eae26`. It resolved the exact
balance-fixes CotW contract, registered all 19 manifest-backed Brown-Fur
identities, retained zero Brown-Fur references in CotW's Arcanist archetype
array, and left effective status Available / Not published. Exact immutable
artifact identities are recorded in the CotW contract.

This closes stable optional identity registration and proves that active
identity status is distinct from selector publication. Player intent, actual
feature ownership, interruption, dispel, persistence, and final publication
remain gates. Brown-Fur is registered but unpublished.
