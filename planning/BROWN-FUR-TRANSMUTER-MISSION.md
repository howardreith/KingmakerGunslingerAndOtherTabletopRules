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
