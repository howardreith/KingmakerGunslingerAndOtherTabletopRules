# Expanded Summoning first-playtest repair completion audit

## Final immutable-source addendum (2026-08-13)

Status: complete and release-qualified. This addendum supersedes older source,
artifact-hash, settings-restoration, working-save-restoration, and final-run
values below; the older rows remain as qualification chronology.

- Immutable runtime/artifact source:
  `fea6b60786aaabc41d1e276e524d8c14803a0e65`; release `0.0.78`.
- Runtime PASS: visual 13/13
  (`20260813T1903398091052Z-8599cd14aebd435f8e674d46b39c1613`),
  structural/menu/icon 38/38
  (`20260813T1907030054323Z-c3c2c5fe47a745689167c9a0e9fc89e2`),
  player path 10/10
  (`20260813T1911396823000Z-7e1b7d94c1664c839176eeb3189f58ee`),
  and mechanical 12/12
  (`20260813T1919096622832Z-7e5087b74fae4490a40a533b4957dd7b`).
- All 16 settings states passed 7/7 per fresh launch at constant registration
  1,438. Enabled and disabled persistence each passed prepare, cleanup, and
  fresh-process absence, 9/9 per stage. Exact directories/run IDs are in
  `EXPANDED-SUMMONING-STATE.json` and the implementation report.
- Standalone x2, Call of the Wild x2, Arms and Armor, Toggle Custom
  Soundpacks, and highest-risk combined x2 all passed both required observers;
  every transaction restored. Focused regressions passed Shield Other 23/23,
  Acadamae 13/13, paper/firearm 6/6, Cord 8/8, and vendors 14/14.
- Two clean final cycles each passed repository/manifest validation,
  1,018/1,018 tests, exact-reference Release, SoundBank validation, and strict
  package validation. Byte-identical SHA-256 values: DLL
  `f56e285107b237569eba0a8aee44d0f8afeb0ee19791a131a86ba4c8625db6e9`;
  package
  `a0089e9e0ab54ec06baf85c0b4e8272d2524d0087e647d3e28d39f14622fd72c`;
  source archive
  `9b912094c264c92f04e00d88cebad5b20050ff795853b37fa94f8bb019ce9bd1`.
- Restored settings SHA-256 is
  `32a0d33f57357ad0e4e7ae872a7c76e6276e3febcd1c9b5b27ea3735fc98f500`;
  restored disposable working-save SHA-256 is
  `50421a09bb874419e4f2f794ec8e926db1cefe8fc18ba7644fb6f4aa045029a0`.
  Protected baseline remains
  `cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07`
  and was never written. No compatibility quarantine or lock remains.
- Draft PR #2 remains draft, open, unmerged, and targets `master`. The only
  remaining work is the documented subjective human visual review.

## Final presentation addendum (2026-08-13)

- [x] 77/77 visible creature concepts have distinct project-owned original
  128x128 RGBA icons; 77/77 source and production hashes validate.
- [x] All 371 SM and 322 SNA visible child placements resolve to manifest keys;
  runtime fallback count is zero and same-creature placements share a sprite.
- [x] SNA I publishes no generic parent-name child or semantic duplicate; SNA
  II quantity choices use their summoned creature's original icon.
- [x] Smilodon naming is fixed without changing the `dire-tiger` key or GUIDs.
- [x] Eagle uses view scale 0.30 and measures 1.360 high versus a 1.926 Medium
  humanoid while retaining mechanical Small size and animation contracts.
- [x] Focused player-path, combat, visual, persistence, 16-state, compatibility,
  and existing-feature regression evidence is recorded in the report/state.


Audit date: 2026-08-12

Status: complete and release-qualified. This audit supersedes the original
pre-playtest completion map. PASS means the cited current evidence covers the
repair requirement; the old direct-child/153-command test is retained only as
lower-layer historical evidence and is not used as the player-path gate.

## Authoritative state

- Repository: `howardreith/KingmakerGunslingerAndOtherTabletopRules`.
- Branch: `codex/expanded-summoning`.
- Selected baseline: `2894d9fcce250708e354894ffd8e1be9c7493b9b`,
  containing required ancestor
  `e4d560f8dd2909518614e3a20e77ba4d70dadeb8`.
- Reviewed repair baseline:
  `e9f251c584607dd45a45a2414e2aaffabff4c44b`.
- Immutable repaired runtime source:
  `11dabc92f4a84b226bab8df03196da98b9ca5f28`.
- Deterministic artifact source:
  `a951f7e4958f77244ef38e9df4507acf41c62b59`. Its only changes from the
  runtime source are the implementation report, journal, and state document;
  no gameplay, blueprint, manifest, asset, or runtime-harness source differs.
- Release: `0.0.78`.
- Draft PR #2 targets `master`, remains open/draft/unmerged, and is refreshed
  after every final documentation push.

## Reproduction, root causes, and repairs

| Requirement | Authoritative evidence | Audit |
|---|---|---|
| Reproduce through the same real spellbook/parent path as the player | Pre-repair discriminating run `20260812T1729340453662Z-0ebb1d302b9045389454b13fbe812a06` records native Dog, KMG Dog logical root, direct Celestial/Fiendish children, SNA Dog, Giant Spider, Small Earth Elemental, and Erinyes. The KMG SM logical roots fail before `RuleCastSpell` while direct children and SNA work. | PASS |
| Prove template-wrapper root cause before rewriting units | The log reports `Can't cast variational ability` for the nested KMG logical wrapper. Direct execution children and the same unit through SNA survive queued creation, isolating the two-level `AbilityVariants` path rather than the natural unit chassis. | PASS |
| Direct executable logical placements | All 182 templated SM logical roots own the proven native spawn action directly and apply one caster-selected post-spawn template. No player-facing KMG logical root contains nested `AbilityVariants`. The 364 old execution identities remain registered and unpublished. | PASS |
| Neutral template choice | A mutually exclusive, save-safe per-character mode defaults deterministically to Celestial and toggles to Fiendish. Good forces Celestial; evil forces Fiendish; neutral honors the mode. Runtime good/neutral-celestial/neutral-fiendish/evil cases each carry one exact KMG template and smite marker and zero native template buffs. | PASS |
| Natural/proxy chassis | SNA Dog/Giant Spider controls and the final 681-root player-path matrix prove the shared natural units survive `EntityCreator.Tick`, remain in the live game-state collection, own descriptor/view/faction/pool/duration context, accept commands, and clean up. Donor-isolation and sanitizer assertions remain zero. No mass natural-unit rewrite was required. | PASS |
| Erinyes independent failure and repair | Erinyes inherited campaign `BuffOnEntityCreated`/`AppearFromFog` state. The repaired unit uses a fresh 9-HD outsider component chassis while retaining the proven ranged body, brain, facts, and view. Structural, player-path, ranged-origin, live-view, and cleanup cases pass. | PASS |

## Player-facing behavior and presentation

| Requirement | Authoritative evidence | Audit |
|---|---|---|
| Every approved logical entry through its native parent | Final run `20260812T2133229099710Z-1da4fc8df4544d46851dc73f93672363` on `11dabc9` resolves and casts all 681/681 logical roots through their actual native spellbook parents. It never grants an internal template child. All nine aggregate assertions pass. | PASS |
| Slot semantics | Every successful player-path case spends exactly one correct-level slot. A deliberately distant command is rejected before cast/spawn rules and preserves slots `7 -> 7`. | PASS |
| Live-world and cleanup semantics | The player-path run advances full-round casting, execution, queued entity creation, and world ticks, then proves exact kind/quantity, non-destroyed live collection membership, descriptor, native faction/pool/duration/caster context, command surface, and exact cleanup. | PASS |
| Required human-path matrix | Native Dog; KMG Dog, Eagle, Giant Spider, Wolf, Lion, Dire Tiger, Mastodon, Roc, Erinyes; Small Earth Elemental; Lantern Archon; Salamander; Succubus; Bebelith; Ghaele; natural 1d3/1d4+1; same-unit SNA; Movanic Deva; and Frost Giant are explicit matrix cases before the complete 681-root sweep. | PASS |
| Native semantic reconciliation | A frozen 48-GUID map suppresses only exact native semantic duplicates from displayed parent arrays. Original blueprint objects remain unchanged and registered. The structural observer reports mapped visible duplicates `0`, unique native missing `0`, and frozen native registration missing `0`. Disabling the feature restores exact originals. | PASS |
| Unique native/third-party preservation | Movanic Deva, Frost Giant, all other unique native choices, and structurally discovered third-party entries remain once; unclassified third-party entries retain stable relative order. Call of the Wild and combined profiles pass. | PASS |
| Deterministic display order | Current-tier KMG singles, unique native singles, 1d3, 1d4+1, then stable unclassified foreign tail. Final observer reports `misordered=0`. | PASS |
| Exact before/after counts | Structural run `20260812T2140370704350Z-7827f7dbd7804905be85087d908026fc` records all 18 original-parent counts and repaired One/1d3/1d4+1 splits. The exact table is in `EXPANDED-SUMMONING-IMPLEMENTATION-REPORT.md`. Final totals are SM `3,13,21,35,44,56,64,68,69` and SNA `5,14,21,33,40,47,53,57,59`. | PASS |
| Icon usability | The immutable icon catalog prefers exact donor icons, then canine, feline, bear, flying, reptile/dinosaur, vermin, per-element elemental/mephit, celestial, or fiend fallbacks. Quantity remains explicit in names. Observer reports no missing icons/bad quantity names and distinct categories. | PASS |
| Invisible Stalker | Uses the Medium Air Elemental view with Medium footprint/silhouette. Final visual contracts cover geometry, navigation, locomotion, attack, hit, death, invisibility, selection bounds, and cleanup. The true Huge Air Elemental is unchanged. | PASS |
| Elementals and standalone Summon Elemental | Elementals remain in SM. The six standalone Summon Elemental roots each retain their exact four original non-KMG children and zero KMG mutation. | PASS |
| Sparse SM VIII/IX | Four Elder Elementals and Ghaele remain the frozen KMG additions; native unique VIII/IX options remain visible. No engine limitation is claimed. A bounded review found no safe dedicated Astral Deva, Trumpet Archon, or additional high-tier fiend donor, so speculative additions are deferred. | PASS |

## Roster, identities, and feature module

| Requirement | Authoritative evidence | Audit |
|---|---|---|
| Frozen roster and quantities | Catalog/generator/domain/runtime evidence agrees on 66 SM entries, 57 SNA entries, 67 unique creatures, 361 SM placements, 320 SNA placements, and 681 total same-kind placements. Formula and dice-boundary tests prohibit mixed packs. | PASS |
| Blueprint identities | Expanded Summoning owns 1,158 active identities and no feature-local reserved identity. Repository ledger is 1,412 active plus one historical reserved, 1,413 total. Runtime registration is exactly 1,412 in every module state. GUID/type/collision validation passes. | PASS |
| Fourth independent module | Settings schema 3, default ON, restart-bound active/pending snapshot, atomic settings, legacy migration, malformed quarantine, UI/state/equality/hash/publication plan, and all 16 configurations are tested. | PASS |
| Disabled/save-safe behavior | Disabled mode publishes zero KMG parent variants while every identity remains registered. The six-stage persistence sequence proves active summons load/expire safely with enabled and disabled publication. | PASS |
| Additive/idempotent/transactional publication | Pure merge and runtime transaction tests preserve reference/order, prevent GUID/reference duplicates, roll back exact originals, refuse unsafe rollback, and fail closed only on ambiguous optional parents. | PASS |

## Static, build, runtime, persistence, and profiles

| Gate | Authoritative evidence | Audit |
|---|---|---|
| Repository/static validation | Current `validate.ps1`/0.0.78 validator and deterministic roster/manifest validator pass with exact counts. `git diff --check` passes. | PASS |
| Complete domain/reflection suite | `1013/1013` PASS, including repaired player-path source contracts, reconciliation/order/icons, template roots, cancellation, standalone elemental isolation, donor/sanitizer, all 16 settings states, Shield Other, Acadamae, Cord, Gunslinger/firearms/vendors/settings, and package contracts. | PASS |
| Clean Release and package | Two clean exact-reference Release/package builds from `a951f7e` pass. Strict standalone UMM package validation passes. Both repetitions are byte-identical. | PASS |
| Player-path runtime | `20260812T2133229099710Z-1da4fc8df4544d46851dc73f93672363`: PASS, 681/681 actual roots plus successful/cancelled slot semantics. | PASS |
| Structural runtime | `20260812T2140370704350Z-7827f7dbd7804905be85087d908026fc`: PASS 37/37, including parents, roots, templates, units, reconciliation, menus, icons, high tiers, standalone elementals, donors, sanitizer, and registration. | PASS |
| Visual-contract runtime | `20260812T2144335858882Z-472c7a98d606437bac9a34d5815608ac`: PASS 10/10 for 67/67 unique units. | PASS |
| Enabled persistence | Prepare `20260812T1929470099833Z-bb3292f38b3644b6923940b52fc491e4`, cleanup `20260812T1932384938123Z-38502fdc53534e7bab8b9ff1f0c9737b`, and absence `20260812T1935299471647Z-9e6533fcb51d421ea31e545fdd28aa51`: PASS. | PASS |
| Disabled persistence | Prepare `20260812T1938060304326Z-4f4f1d75920e45fdb86513c0bb48c61d`, cleanup `20260812T1941047028065Z-e853f261c5e64fe9bd172692d7538997`, and absence `20260812T1943438977236Z-003950a75a8d4c448b8865c651413c33`: PASS with zero KMG publication. | PASS |
| Feature matrix | All 16 fresh launches on `9cb5f54` pass with constant 1,412 registration, exact publication isolation, and byte-identical settings restoration. | PASS |
| Standalone | `compat-20260812T214659Z-a8408890bf7c` and `compat-20260812T214914Z-2bf507fe07aa`: PASS/restored. | PASS |
| Call of the Wild | `compat-20260812T215130Z-3fbac165ac9e` and `compat-20260812T215520Z-067b42b7677a`: PASS/restored. | PASS |
| Highest-risk combined | `compat-20260812T215909Z-2f991684eb43` and `compat-20260812T220300Z-b0dc049fc8b2`: PASS/restored. | PASS |
| Arms and Armor | `compat-20260812T220654Z-a2e8eabf9736`: PASS/restored. | PASS |
| Toggle Custom Soundpacks | `compat-20260812T220911Z-976210443a94`: PASS/restored. | PASS |
| Existing-feature runtime regressions | Shield Other `20260812T2101121022865Z-89f2bb1c40ea4d43abba943f1ce05b8e` passes 23/23. Acadamae/Cord `20260812T2058347680701Z-8cdc06e4886a41cba7ac7592462864b5` passes 13/13 using the exact KMG Dog root. Paper/firearm/vendor comprehensive transaction `compat-20260812T212522Z-f2d2def65434` passes 6/6. | PASS |
| Gunslinger comprehensive fixture disposition | The broad save-free fixture passes 184 assertions; its detached Gunslinger's Dodge command lacks a Swift-action controller and interrupts before `Start`. Production Dodge source was untouched, focused Dodge/domain coverage passes, and no runtime behavior or resource spend occurred. This is an inherited auxiliary-fixture limitation, not a product regression, and is disclosed in `KNOWN-ISSUES.md`. | PASS with disclosed harness limitation |

## Hashes, restoration, documentation, and publication

| Requirement | Authoritative evidence | Audit |
|---|---|---|
| DLL hash | `0bab4618881b616516cfe51f28ab1857ecd7e1a5598e28125a175f651e45201b`. | PASS |
| Package hash | `35fa5f9347794156a6eeb763818333efce83111fe9a0dd8fc71e861e75f137a4`. | PASS |
| Source archive hash | Two independent `git archive` outputs match at `572f9349767af56e41d15147ecafb14cd796d7530042d6c60705be9c340c3ecf`. | PASS |
| Settings restoration | Current settings hash is `424da4573acb5dc9e3c7ca3546da688a1405702858fb3b28aea5cbae28c4ba3e`. | PASS |
| Mods restoration | Every final profile transaction reports `Restored`; no live `Mods.kmg-compat-*` backup/quarantine remains. | PASS |
| Working save restoration | `KMG_AUTOMATION_WORKING` was restored from its verified pre-test backup to `3595a41873f62ef2e28762abb6dd757418b239f2e5c9441f6f027214fc99a997`. | PASS |
| Protected baseline | `KMG_AUTOMATION_BASELINE` remains `cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07` and was never written. | PASS |
| Documentation | Mission, roster, fidelity matrix, inventory, journal, state, resume, changelog, known issues, testing, and implementation report distinguish the lower-layer test from the decisive player path and record root causes, reconciliation, exact menu counts, icons, Stalker, slot evidence, profiles, hashes, deviations, and deferred high-tier candidates. | PASS |
| Git/publication | Dedicated branch only; ordinary non-force push helper used after each coherent commit; local/origin equality verified; no merge, rebase, reset, clean, force-push, or release-branch mutation. Draft PR #2 is updated and unmerged. | PASS |

## Conservative adaptations and known limitations

- Lantern Archon Aura of Menace is present only when an exact compatible
  optional carrier exists; greater teleport, gestalt, truespeech, and separate
  darkvision modeling are omitted.
- Mephits retain Owlcat's unconditional Fast Healing 2 because no safe
  environment predicate exists.
- Shadow Demon possession/shadow blend/sprint and Succubus profane gift are
  omitted; their bounded combat identities remain.
- Bebelith permanent armor destruction is a one-round `-2 AC` dismantle;
  rot/climb are omitted.
- Pixie sleep uses a bounded zero-damage non-inventory bow; dance is a frozen
  bounded state.
- Per-creature proxy omissions are recorded in the fidelity matrix, chiefly
  unsupported movement modes, grab/trample/sprint/rage, and unproven donor
  feats/senses.
- Kingmaker exposes spell descriptors only on shared `BlueprintAbility`
  objects, not per caster invocation. Direct roots retain `Summoning`; spawned
  template, alignment, and smite are caster-correct. Unsafe shared mutation was
  rejected. Supported profiles contain no Sacred Summons surface, so optional
  integration fails closed.
- No safe dedicated high-tier donor justified Astral Deva, Trumpet Archon, or
  extra fiend additions. They remain deferred rather than implemented with
  poor campaign proxies.

## Residual human-only checklist

No engineering gate remains. Human review is limited to subjective proxy
silhouette/scale, selection circles and camera comfort, projectile/socket
appearance, and locomotion/attack/hit/cast/invisibility/death aesthetics,
especially Invisible Stalker, Bebelith, Pixie, Roc/Mastodon, and flying
proxies. Draft PR #2 must remain unmerged until review is complete.
