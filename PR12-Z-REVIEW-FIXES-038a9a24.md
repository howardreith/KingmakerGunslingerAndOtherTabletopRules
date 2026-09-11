# PR #12 continuation review (head 038a9a24) — R1–R4

Persisted verbatim summary of the owner's continuation prompt. Full text is in
the conversation of record; the four acceptance items are:

- R1: Confine native activation to a valid destination request. Ordinary
  inventory/equipment use must not consume strategic scrolls (refusal before
  activation, not an inert success). Narrow request-bound authorization around
  the exact native activation; always released; no process-global boolean.
  Observer must attribute to the right reader/item/activation. Reproduce the
  boundary case in a disposable fixture first.
- R2: Preserve material variant identity. Explicit equivalence contract
  (canonical activated spell, caster level, item spell level, other
  activation-relevant properties). Snapshot carries caster level; rows and
  confirmation display it; Resolve binds the selected variant exactly;
  AssociatedSpell must not let a teaching-only association authorize an
  unrelated activated spell. Mismatched teaching/activation rejected before
  spending.
- R3: Complete fallback availability and saved-stock integration. Publication
  and migration must share one supplier decision; Migrate must recognize the
  fallback table and record fallback grants; no unconditional fallback stock
  while primary is active; priest independent; already-materialized fallback
  merchant gets stock exactly once; native shared-table diff one-time behavior
  proven where it applies.
- R4: Real gameplay persistence. Extend the existing bounded fresh-process
  harness: purchase/copy/prepare/rest/activate with real paths in phase A,
  record supplier identity/grant/stock/gold/items/learned spell/preparations/
  arrival + actual first-arrow action; fresh-process verify before any fixture
  can reconstruct; module OFF/ON without refill/item loss/free restoration.

Final: focused regressions that fail before and pass after; full validation,
domain tests, clean build/package, affected native regressions; correct final
artifact qualified and installed with the approved backup/rollback workflow;
state/handoff updated honestly (R1–R4 fixed/proven or precisely blocked);
commit and push through the approved wrapper; no merge or public release.


## Resolution state at this checkpoint (2026-09-10, head = OFF-preserving publication)

- R1 SOLVED and native-verified: request-bound activation gate; ordinary
  TryUseFromInventory refused before any roll/consumption with the destination
  guidance; observer attributes to exact reader+SourceItem; casting/specialist/
  arrows regressions PASS.
- R2 SOLVED and native-verified: full equivalence contract (spell, caster level,
  item spell level, charge model) in grouping+binding; snapshot carries
  CasterLevel; rows/confirmation show CL; same-CL different-spell-level variant
  separate; teaching/activation mismatch rejected; standard teaching intact.
- R3 SOLVED at the shared-decision level and native-verified for: fallback
  identity, decision seam with primary genuinely absent (fallback selected,
  priest independent), fallback batch-once under its own grant identity,
  fallback inactive while primary active. NOT RUN: in-area genuine Zarcie
  absence and kingdom auto-management acquisition (environment cannot
  establish them at the main menu).
- R4 PARTIALLY proven, one REAL defect open: phase A now performs the real
  purchase/copy/prepare/rest/activation/first-arrow chain and phase B
  verifies the full lifecycle state byte-exact on a fresh process. Phases C/D
  exposed a genuine contract violation: with the module OFF the process
  rebuilds blueprints WITHOUT our stock rows (publication plan off), the
  native shared-table diff then wipes the materialized shelf AND its purchase
  memory, and re-enabling refills bought-out stock to the full batch.
  The in-process OFF no-op fix (preserve rows when publish=false) is
  implemented but cannot fire because Publish is not called at all under OFF.
  Full fix needs a bootstrap-order-aware design (e.g., publishing the finite
  rows as campaign-granted data whenever the identities load, gating purchase/
  activation instead) — a design decision requiring owner review, not a
  last-minute change. Until then phases C/D of the acquisition lifecycle are
  NOT RUN/PASS; the earlier serializer-only persistence qualification
  (transaction 20260910T2038273365319Z) remains valid for its own scope.


## C1/C2 closeout (2026-09-10, final commit 0282049)

- C2 FIXED and native-verified: TeleportationScrollActivationGate
  .HasStrategicScrollFact now classifies through the SAME shared contract as
  discovery (TeleportationScrollAdapter.AssociatedSpell — canonical activated
  ability), so every supported variant is guarded. disposable-teleportation-
  scrolls 36/36 twice: ordinary native use of BOTH the standard scroll AND the
  distinct crafted variant is refused through the actual TryUseFromInventory
  boundary (no rulebook event, no consumption, gate closed); contextual use of
  the variant performs one native activation, consumes only the selected
  variant, relocates, spends no slot; spellbook casting and unrelated scrolls
  unaffected (specialist/casting/travelers/gamepad/coexistence regressions
  PASS).
- C1 FIXED and native-verified: bootstrap now publishes the finite vendor stock
  rows whenever the scroll identities load — the rows are save-compatible
  campaign DEFINITIONS — independently of gameplay enablement; teleport
  activation, migration and spell-list publication remain module-gated. The
  full fresh-process acquisition lifecycle qualification PASSED A/B/C/D
  (transaction 20260910T2328349730573Z_e6f6f244594e420aa9febe777507d087,
  distinct PIDs, assertions 15/12/8/4): phase A performs the real purchase/
  copy/prepare/rest/activation/first-arrow chain; B (fresh ON reload) verifies
  the exact lifecycle state; C (fresh OFF load/save) preserves it; D (fresh ON
  reload) PASSES module-on-no-refill — shelf restored to the batch minus
  remembered purchases (3/3), owned items, learned spell and spent/ready
  preparations intact, no duplicated grant.
- Final candidate: commit 028204931c51e35de16da716b4137f45ef5a026d, package
  sha256 24df8242e0a442bf…, installed DLL sha256 a90ff24e2c8386aa… == deployed
  (deployment 20260910T2352590936980Z); rollback backup
  runtime-backups/live-mod (latest under that root). The persistence evidence
  was collected on commit 65262a9 whose PRODUCTION code is identical to
  0282049 (the later commits changed only guarded test scenarios); the final
  artifact additionally passed scrolls 36/36, interaction (camera-settle fix
  in the test), arrows, specialist, casting, travelers, gamepad, coexistence,
  observer, repository validation, 1,561 domain tests and 464-check preflight.
- Remaining explicit limitations (follow-up, not passes): in-area genuine
  Zarcie absence and kingdom auto-management acquisition; broader display/
  aspect-ratio and optional-mod coverage beyond the qualified profiles.
