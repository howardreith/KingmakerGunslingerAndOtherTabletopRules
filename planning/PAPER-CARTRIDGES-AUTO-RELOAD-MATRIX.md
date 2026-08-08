# Paper Cartridges Acceptance Matrix

Status values: `TODO`, `PASS`, `FAIL`, `DEFER (evidenced)`.

| Area | Acceptance slice | Status | Evidence |
|---|---|---:|---|
| Intake | Clean exact remote baseline and required branch | PASS | 2026-08-08: local/remote master `759685077da0aed6d7ed1fda2cd43e5ad12d0bdb`; branch created |
| Inventory | Current source/tests/docs inventory and replacement matrix | TODO | Phase 0 journal |
| Profile/item | One paper profile/item; cost 12, stackable, inert, zero weight/resale | PASS (Phase 1 foundation; resale pending Phase 5) | `ReloadAmmunitionProfileCatalog`; `KMG.Ammunition.PaperCartridge`; 941/941 |
| Compatibility | Definition-driven Early Pistol/Musket/Blunderbuss; magic families included; advanced rejected | PASS (domain foundation) | `FirearmStateRules.CreateForDefinition`; no item whitelist |
| State | Two append-only paper tokens; old identities unchanged; codec/reconciliation/static enchantments | PASS (token/codec foundation; lifecycle runtime pending Phase 6) | Six-token catalog; old four IDs asserted exact; paper round trips |
| Action economy | Every required Pistol/Musket/Blunderbuss/Fast Musket/Rapid/Paper row | TODO | |
| Mode/grants | Per-unit off-default toggle; exact grants; no fallback/leak/duplicate/auto-disable | TODO | |
| Transaction | Atomic loose/paper sources, exact rollback, no mix/substitution/double-consume | TODO | |
| Manual/auto-use | Shared plan drives availability, UI, command, delivery, native continuation | TODO | |
| Full attack | Exact auto-use gate; Free normal or one Free Lightning fallback; fail closed | TODO | |
| Lightning | Swift/Free dynamic action/source; one chamber/round; True Grit; rollback/reset | TODO | |
| Misfire | Shared exact-weapon policy; paper before Reliable; threshold 0; ordinary/Dead Shot/Scatter | TODO | |
| Scatter/audio | One paper, all-roll rule, triple explosion, single transition/sound, no multi-cone | TODO | |
| Crafting | 20 for 120; existing kit/gates/shared marker; atomic rollback; basic unchanged | TODO | |
| Vendors | Smith 200; installed BTSL 200; bounded idempotent normalization/rollback | TODO | |
| Acquisition controls | No Jhod/starting/fixed-loot paper; roster preserved; Bokken exact or evidenced defer | TODO | |
| Presentation | Item/mode/reload/Lightning/Gunsmithing/help/icons/logs/build/changelog | TODO | |
| Runtime | Reload scenario | TODO | |
| Runtime | Native full-attack scenario | TODO | |
| Runtime | Ordinary/Dead Shot misfire scenario | TODO | |
| Runtime | Scatter scenario | TODO | |
| Runtime | Lightning Reload scenario | TODO | |
| Runtime | Crafting/vendor scenario | TODO | |
| Runtime | Two independent final comprehensive PASSes | TODO | |
| Compatibility | Standalone, A&A, soundpacks, combined, one bounded CotW; exact restoration | TODO | |
| Release | Version/pins, complete gates, clean package, hashes, docs, pushed remote equality | TODO | |

## Pre-implementation inventory and replacement/extension map

Completed against source at `759685077da0aed6d7ed1fda2cd43e5ad12d0bdb`
before production mechanics changed.

| Current authority | Observed contract | Narrow mission extension |
|---|---|---|
| `Firearms/AmmunitionId`, `FirearmState*` | Immutable validated string ID; DTO/codec already carry ammunition; state forbids empty+ID, loaded+null, wrecked+rounds | Add catalog-owned paper ID/profile and allow it only in compatible early-family rules; preserve DTO/schema |
| `FirearmStateTokenCatalog` | Absence is empty/normal; four Lead/Broken/Wrecked token meanings; diagnostic Lead ID is `kmg.debug.lead-ball`; finite encode/decode fails unknown | Append normal/broken paper definitions; do not rename old IDs; extend catalog factories/tests |
| `FirearmStateTokenBlueprints` and token store/repository | Inert weapon-enchantment tokens replace only project state tokens; merged runtime evidence preserves unrelated static enchantments | Append exactly two blueprint tokens and repeat exact Enhancement/Reliable/Seeking/Fey Bane preservation tests/runtime evidence |
| `BasicAmmunitionBlueprints` / `Ammunition/*` | Two inert cloned stack items; inventory/transaction shape is fixed powder+ball with snapshot/restore | Add Paper item and generalized immutable inventory-source snapshot/transaction while retaining loose API behavior |
| `ReloadActionEconomy` | Pure base + Fast Musket + matching Rapid reduction with clamp; no ammunition modifier | Accept an immutable modifier/profile and apply Paper after Rapid; test full mandated matrix |
| `ReloadTestMusketRuntime`, ability logic/blueprints, presentation patches | Exact equipped firearm resolution; ability availability/action and delivery currently reconstruct loose-only decisions at several seams | Replace reconstructions with one plan; keep Reload as sole native auto-use ability and bind manual UI/command/delivery to plan |
| `FullAttackAutoReloadPolicy` / `FreeActionFullAttackReloadPatch` | Inline reload only for a genuinely Free action after exact native attack conditions; patch directly creates loose inventory transaction | Add exact native auto-use requirement, selected-source plan, Free normal preference, and at-most-once Free Lightning fallback |
| `EmptyFirearmAttackCommandPatch` | Existing pending exact item/target/command auto-use continuation and cancellation safeguards | Supply/revalidate the same authoritative selected plan without weakening stale/turn safeguards |
| `LightningReload*` | Policy checks exact firearm/grit/once marker/basic stock; runtime adds marker then invokes loose-only transaction; action is presently static | Plan Swift/Free dynamically, support paper/no fallback, share atomic transaction, preserve True Grit and marker rollback/reset |
| `EffectiveFirearmMisfirePolicy` | One shared base + condition/training - exact-item Reliable, final 0..20 clamp used by ordinary, Dead Shot and Scatter | Add profile/ammunition modifier before Reliable; bind pre-discharge ammunition through all three callers |
| `FirearmMisfireRuntime` / discharge context | Ordinary path has exact-item scoped event/roll/duplicate protections and consumes chamber before final decision | Carry exact fired ammunition/threshold from pre-discharge state; preserve all existing event semantics |
| `DeadShotRuntime` | One custom chamber transaction and shared effective policy; merged threshold-zero support | Capture the one pre-discharge ID and compute one threshold for all probes |
| `ScatterShotRuntime` | Separate chamber transaction, same effective policy, all-target aggregation, single transition/explosion/sound guards | Capture paper ID and central modifier; retain one cone and all aggregation behavior |
| Proficiency blueprints and `PlayerFacingPresentation` | Full/scoped feature ownership and recursive presentation already grant/publish Reload/Scatter by scope | Add one activatable+hidden marker exactly once to every Reload-granting scope; no global selection |
| `GunsmithingCraftingBlueprints` / `CraftBasicAmmunitionAbilityLogic` | Basic recipe outputs 20/20 for 22 gp, uses exact shared feature marker and explicit rollback; rest removes marker | Extract shared transaction; add 20 Paper for 120 using the same marker and all existing gates |
| `BasicAmmunitionSaleValuePatch` | Project anti-arbitrage override covers crafted basic supply identities | Add exact Paper identity only |
| `CapitalVendorBlueprints` | Exact `SmithVendorTable`; bounded project-owned universe, normalized desired entries, exact snapshot and foreign-mutation rollback refusal | Add Paper to desired/owned sets at 200; preserve 11 existing entries and exclusions |
| `BeneathStolenLandsVendorBlueprints` | Four exact GUID/name optional tables; same bounded normalization and reverse rollback | Add Paper at 200 to every installed exact table |
| `RareFirearmCampaignLootBlueprints` | Five exact count-one fixed publications with isolated normalization | No production change; add preservation assertions only |
| Acquisition observers/docs | Installed graph already proves Smith, rejected Jhod, BTSL, fixed loot; Bokken is not resolved in the accepted inventory | Add one bounded Bokken exact table/owner/lifecycle observation; publish 100 only if unique/safe, else document defer |
| `BlueprintBootstrap` / manifests / project files | Expected 242 registrations; Basic ammunition, state tokens, crafting, vendors and loot initialize transactionally; ledger has 243 stable rows | Append item, two tokens, activatable, marker, and craft ability identities (exact count after implementation); update bootstrap rollback/order and validators |
| Runtime scenario catalog/runner | Typed allowlisted guarded scenarios with request/preflight/result and exact build/process controls | Add seven typed paper scenarios and host allowlist/preflight tests; require mod-load smoke and fresh processes |
| Release tooling | Source pins are exactly 0.0.73; current full deterministic total is 935; Release/build-output/SoundBank/package scripts are authoritative | Keep 0.0.73 during vertical work, then transactionally advance all discovered pins to 0.0.74 at Phase 7 |

Production replacement begins only after the unchanged baseline gates and the
durable intake commit are published with local/remote equality.

## Deterministic coverage checklist

Focused tests must cover every work-order section 7 slice: item/profile;
mandatory action matrix and clamping; inventory success/rejection/rollback/isolation;
toggle and proficiency ownership; old/new token/codec/reconciliation/static-enchantment
behavior; native full-attack controls and Lightning fallback; exact centralized
misfire order and boundaries; Scatter aggregation/explosion/audio; Lightning dynamic
action/marker/True Grit; shared-marker crafting/economy; exact Smith/BTSL/Jhod/fixed
loot/Bokken normalization and rollback.
