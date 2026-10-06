# DATA content stack final status — 2026-10-06

This is the cumulative authority for the finite DATA content stack. It supersedes current-status language in earlier handoffs without deleting or rewriting their historical observations, failed attempts or artifact records. The four new character race traits have qualified **unpublished mechanics foundations**. They do not ship or become selectable through this stack.

Repository: `howardreith/KingmakerGunslingerAndOtherTabletopRules`.
Frozen cumulative source: `fc352c24fcbcc5f6e1c3be7b8c72bd9c21720300`, the direct documentation-only child of qualified Whiteout mechanics commit `db3725d7eb665731c3b8c238217cd39f2d0a65b8`.
The exact ref and clean source worktree were verified before creating `codex/elemental-race-traits-publication-readiness-2026-10-06` in its own worktree. A fresh unfiltered baseline run passed **2,188/2,188** tests; no source was changed for consolidation.

## Cumulative disposition

| Work item | Current status | Qualified source commit / authority | Publication status | Remaining work |
|---|---|---|---|---|
| Nodachi Heirloom Weapon | COMPLETE before Z; verified, no duplicate implementation | Existing `431bef614` lineage; content suite at `c092a060a5c01f963e5954d6e6f1882bd16b6114` | Existing conditional Equipment Trait publication | None in this finite stack |
| Firearm descriptions | COMPLETE, technically qualified | `c092a060a5c01f963e5954d6e6f1882bd16b6114` | Existing items; normalized prose | Human tooltip appearance |
| Model D vendors | COMPLETE, technically qualified | `c092a060a5c01f963e5954d6e6f1882bd16b6114` | Existing merchant-table paths | Human merchant presentation and existing-stock acceptance |
| Fiery Glare | MECHANICS QUALIFIED; ADAPTED success-only take-10 | `2c5bbcaf428b3e0ac017ba5eb5a7969674bcf56e` | Unpublished; fixture-only transient graph | Dormant stable graph/copy/publication plan complete in the 2026-10-06 readiness branch; original icon and later player qualification |
| Stoic Dignity | MECHANICS QUALIFIED; exact same-effect/source-lineage suppression | `2c5bbcaf428b3e0ac017ba5eb5a7969674bcf56e` | Unpublished; fixture-only transient graph | Dormant stable graph/copy/publication plan complete in the 2026-10-06 readiness branch; original icon and later player qualification |
| Aerial Observer | MECHANICS QUALIFIED; ADAPTED exact Wings of Air effect | `a0992df35a28d0049ad25b3f746f0d465ed5426a` | Unpublished; fixture-only transient provider | Dormant stable graph/copy/publication plan complete in the 2026-10-06 readiness branch; original icon and later player qualification |
| Whiteout | MECHANICS QUALIFIED; ADAPTED outdoors-only native Rain/Snow Light+ | `db3725d7eb665731c3b8c238217cd39f2d0a65b8` | Unpublished; dormant exact patch and transient provider | Dormant stable graph/copy/publication plan complete in the 2026-10-06 readiness branch; original icon and later player qualification |
| Earthsense | OMITTED — NO FAITHFUL ENGINE CARRIER | Corrected finding `f1155cb158188a1264f8b53ac41a64f514fe099a` | None | No substitute or new subsystem |
| Lunge | BLOCKED — no faithful frozen RTwP turn/attack-attempt seam | [engine contract](docs/research/LUNGE-ENGINE-CONTRACT.md), closed in `87e42d2252192d66a9aeab56b86b9d4d81b62c69` | None; no production reach/AC patch | No implementation or adaptation |
| Human tooltip acceptance | NOT PERFORMED | [manual matrix](docs/RARE-FIREARMS-MANUAL-ACCEPTANCE.md#content-followup-acceptance---2026-10-05) | No owner approval inferred | Howie's rendered Pistol, Blunderbuss and Last Word review |
| Human merchant-screen acceptance | NOT PERFORMED | Same manual matrix | No owner approval inferred | New/materialized stock, ordering/quantities, early supply and persistence decision |
| Later laptop Phase 2B integration | NOT PERFORMED | [forecast](docs/integration/DATA-CONTENT-STACK-INTEGRATION-FORECAST-2026-10-06.md) | No merged/released artifact | Separate owner-authorized integration after laptop closure |

The Lunge range and threat functions themselves are separable. Its blocker is the combined own-turn/full-attack/no-attack penalty/accepted-attempt timing contract in RTwP. Earthsense is not replaced by another sense.

## Frozen mechanics and content boundaries

- Fiery Glare uses native `Take10ForSuccess.Skill = CheckIntimidate`, off by default, immediate/free/no resource. Ten is used only when it succeeds; otherwise the check rolls. Combat works; Bluff, Diplomacy and general Persuasion are unchanged.
- Stoic Dignity grants the conscious holder +1 Trait and other allies within 10 feet +1 Morale. Canonical effect-fact identity takes precedence over exact source ability and explicit parent lineage. Distinct concrete effects remain distinct; siblings sharing only a generic parent remain unrelated; missing/ambiguous correlation grants. Caster alone never identifies an effect. The aura excludes its holder/enemies, rechecks consciousness/distance, respects native stacking and once-per-rule delivery, and never cleanses or immunizes.
- Aerial Observer grants +2 Trait Perception only while the exact active, nonsuppressed KMG Wings of Air buff (`e116e1e0a17a4aceb001000000000019`) is present. Event-driven native modifiers cover active/cached rules and raw-stat passive consumers. Visual wings, hover, height, Feather Step and foreign flight are excluded.
- Whiteout requires its exact provider, actual Rain/Snow Light+, a known native map/area point outside, and valid module/native contracts. Native failure short-circuits; success permits one independent d100, 1–10 miss. Native 20% plus independent 10% is **28%**, not 30%. IgnoreConcealment and exact project Seeking bypass it. Only qualified native-stage paths are covered; automatic-hit/miss branches, nonattack damage/saves and unproven maneuvers are excluded.
- Model D generates Oleg's three mundane firearms ×1, powder/balls ×50; capital's three +1 firearms ×1, powder/balls/cartridges ×200, kit ×1; Bokken's three ammunition rows ×100. Capital generic Eastern/spear rows are removed while established non-capital, BTSL, Better Vendors, named-loot and salesman paths remain. Existing serialized stock is not rewritten; fixed stock can deplete and regeneration is not promised.

The original Whiteout round-trip reference-restoration failure remains accurate history. The later owner accepted native non-party scene replacement only in an automatically exiting disposable **no-save** process. Per-area weather and owned state were restored; no claim is made that unloaded NPC references were resurrected.

## Exact specialized qualification ledger

Each row qualifies its exact historical artifact. An older specialized run does **not** qualify a later cumulative DLL. Documentation-only descendants did not change packaged inputs. Current readiness builds/tests are recorded separately in the readiness handoff; they do not imply published-trait gameplay acceptance.

| Artifact | Embedded source | DLL SHA-256 | DLL MVID | ZIP SHA-256 | Source fingerprint |
|---|---|---|---|---|---|
| Fiery / Stoic | `2c5bbcaf428b3e0ac017ba5eb5a7969674bcf56e` | `508025890be9b9cf682f1fc70377dd11ab1f9b87d2a1d6b132cdca1bead4d046` | `922d085c-1755-4595-b16e-da0d43a9343e` | `ab0edb84d065a003415cf802d5c1d086ea244320cdecbf8a5c4676e5e475b993` | `c8dff63bb1a9148de4f0eee8e14b2b0cf536089b5abbd37551fda3754cc42f91` |
| Aerial Observer, passive lifecycle included | `a0992df35a28d0049ad25b3f746f0d465ed5426a` | `edfc34fd21a97e1f0742355ab31b665310b973c9f2d65fae97b6bb8604a55501` | `e9f9e98e-27ff-4b84-a36f-808f7c3d45bf` | `67504ce3fc110118d5bb555d04a70e8c98ba831fb49b7565221a13cb7b351386` | `d2d4641000a0ff39d1fbc73997db6007eb8a3cab5bbd25588d632fd66745d9a2` |
| Whiteout | `db3725d7eb665731c3b8c238217cd39f2d0a65b8` | `366c518d0b252ec4eb7335746587b4b2c4250eb5da5fa6c75de8f440a6d1d004` | `569ce324-53f5-4c36-9406-a16e983ac229` | `9e1e320da34daa12e04a5581b0dd1e6bc4e02f36327e2d829a4019a71bccc7e0` | `7420a0bdb981b4c9912105374a9a3829efe5b8404e4f5b4c3826519ad0c2ee41` |

Content clean review candidate: source `c092a060a5c01f963e5954d6e6f1882bd16b6114`, ZIP `2971a4f167f3943e580934ad302ec5b8acb420ee68310e21a50a01c64a67c006`. Prefer that already-qualified package for pending human review; reconstruct and requalify separately if unavailable.

| Scope | Final unfiltered tests | Exact specialized runtime | Exact-artifact smoke | Restoration |
|---|---:|---|---|---|
| Content followup | 2,040 | Resolved prose 8/8; Model D 26/26; read-only weather/catalog 6/6 and 2/2 at their recorded checkpoints | 11/11 at clean review candidate | Exact owned live snapshot; leases completed |
| Fiery / Stoic | 2,093 | Two fresh processes 49/49 each, PIDs 20856 / 38432 | 11/11, PID 33156 | Acquired 254-file snapshot exact; lease Completed |
| Aerial Observer | 2,127 | Two fresh processes 29/29 each, PIDs 34872 / 22224 | 11/11, PID 35456 | Acquired 254-file / 7-directory snapshot exact; lease Completed |
| Whiteout | 2,188 | Two fresh processes 65/65 each, PIDs 26792 / 27500 | 11/11, PID 39572 | Both weather scopes and owned state; acquired 254-file live snapshot exact; lease Completed |

All final game processes used guarded requests, Steam App ID 640820, automatic exit and zero observed save-writing APIs. Machine-local results/proprietary inspection remain outside Git. No human aesthetic acceptance follows from assertions.

Detailed evidence remains in [content](CODEX-ALL-NIGHT-GUNSLINGER-FOLLOWUP-HANDOFF-2026-10-04.md), [Fiery/Stoic](CODEX-ELEMENTAL-RACE-TRAIT-FOUNDATIONS-HANDOFF-2026-10-05.md), [Lunge/Aerial](CODEX-LUNGE-AERIAL-OBSERVER-FOUNDATIONS-HANDOFF-2026-10-05.md), [historical Whiteout blocker](CODEX-WHITEOUT-UNPUBLISHED-FOUNDATION-HANDOFF-2026-10-05.md), and [Whiteout continuation](CODEX-WHITEOUT-DISPOSABLE-PROCESS-CONTINUATION-HANDOFF-2026-10-05.md).

## Branch preservation

Remote existence/tips were checked read only on 2026-10-06. Preserve every branch below until integration is complete. None was merged or deleted.

| Preserved remote branch | Verified tip | Role |
|---|---|---|
| `codex/z-weekend-traits-content-2026-10-03` | `1b7bfa06ae897e2359d8ba6a0ec5522bc1183a57` | Historical Z evidence |
| `codex/gunslinger-content-followup-2026-10-04` | `b28b5786d10a94f3257fafa5cf02bbd50471d182` | Reconciled content / manual acceptance |
| `codex/elemental-race-trait-foundations-2026-10-05` | `6a4dc1b26350c1045171b42099ef2ac5584e303d` | Fiery / Stoic closeout |
| `codex/lunge-aerial-observer-foundations-2026-10-05` | `87e42d2252192d66a9aeab56b86b9d4d81b62c69` | Aerial / Lunge finding |
| `codex/whiteout-unpublished-foundation-2026-10-05` | `5c59150a748023d8bc7195df9bfd63ca4a2079b2` | Historical weather blocker |
| `codex/whiteout-disposable-process-foundation-2026-10-05` | `fc352c24fcbcc5f6e1c3be7b8c72bd9c21720300` | Frozen cumulative DATA tip |

The original Z handoff is absent from this cumulative tree; it was read as the historical Git object at `1b7bfa06…:Z-WEEKEND-GUNSLINGER-HANDOFF-2026-10-03.md`, without changing its branch/worktree. Corrected later contracts supersede its obsolete exactness/icon-only implementation claims.

Preserve the new readiness branch too. Its final commit, asset disposition and validation belong in `CODEX-ELEMENTAL-RACE-TRAIT-PUBLICATION-READINESS-HANDOFF-2026-10-06.md` and the post-push receipt. No integration/release/version change occurred. Expanded Summoning belongs exclusively to the laptop and was not investigated or changed here.

## Publication-readiness closeout — 2026-10-06

The four mechanics remain unchanged and unpublished. The new dedicated
readiness branch adds 11 planned stable identities, gated dormant native graphs,
exact race/self-duplicate prerequisites, staged final copy, an all-four
publication transaction plan, persistence/respec ownership contracts and four
original-art brief drafts. The live manifest, icon catalog/assignments,
localization, settings and selectors remain unchanged. No trait can be acquired.

[The readiness contract](docs/design/ELEMENTAL-CHARACTER-RACE-TRAIT-PUBLICATION-READINESS-2026-10-06.md)
and [mission handoff](CODEX-ELEMENTAL-RACE-TRAIT-PUBLICATION-READINESS-HANDOFF-2026-10-06.md)
record its separate qualification. The real asset gate finds all four originals
missing. The 75 new deterministic tests bring the registered domain total from
2,188 to 2,263; prior specialized game records do not qualify the new readiness
DLL. Final disposition is BLOCKED-ONLY-ON-ORIGINAL-ICONS, TraitsPublished: false.
No owner tooltip, merchant, icon or UI acceptance was inferred.

Next work is limited to four original icons, owner approval and the asset-qualified
player-publication pass. Earthsense and Lunge findings remain closed as recorded;
no implementation was reopened. Integration/release and the laptop's line remain
outside this mission.
