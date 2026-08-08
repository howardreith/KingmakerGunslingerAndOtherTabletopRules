# Pistolero and Musket Master Journal

## 2026-08-07 - Mission intake and unchanged baseline

- Authority: user-issued Revision 2 autonomous work order; durable operational
  copy: `planning/PISTOLERO-MUSKET-MASTER-MISSION.md`.
- Exact base branch/commit: `master` at
  `10b792735db5d685b46749dc08ea819f31fa8052`.
- Feature branch: `codex/pistolero-musket-master-archetypes`.
- Initial status: clean; `Info.json` version `0.0.72`.
- Required compatibility merge is the exact base commit. The repository contains
  the merged profile transaction framework and
  `EvasiveBlueprints.PreservesCurrentComponentContract` current-donor repair.
- Content search found no user-provided local Pistolero or Musket Master rules
  `.txt`/`.md` file. The work order explicitly authorizes its reproduced rules
  when those files are absent.
- Inherited deterministic baseline: 911 tests. First two sandboxed runs failed
  only `audio.staging-lifecycle` because its temp-directory `File.Replace` was
  denied; the identical unchanged suite passed 911/911 with authorized temp
  access. Repository validation passed.
- Unchanged exact-reference Release, build-output, SoundBank, and strict package
  gates passed. Fresh local-runtime package SHA-256:
  `C9EC17E87805D3E1C93DC1879FBAC300E3BE0493AB422CE93B2445556D0BC4FE`;
  DLL SHA-256:
  `895D0EA7F1D4CB7658CA9C81B3F478D75C29A5FCEA8839E44908BE6E13F525FF`.
- Unchanged guarded baseline PASS:
  - mod load `20260808T0332364552630Z-mod-load-smoke`;
  - class contracts `20260808T0334429458961Z-observe-class-blueprint-contracts`;
  - presentation `20260808T0336494740671Z-observe-gunslinger-presentation`.
- Inherited standalone status: profile `gunslinger-only` remains
  `GUNSLINGER-REPAIR-REQUIRED` because detached Gunslinger's Dodge finishes
  `Interrupt` without its timed buff; exact inherited diagnostic
  `20260807T2057209416590Z-a48e33c01d6f48f2b407eb08fe361035`.
- Call of the Wild facts remain distinct: public exact profile
  `gunslinger-call-of-the-wild` is `CONFLICT-CONFIRMED` from human chargen
  omission; later dependency-free current-donor repair passed load
  `20260807T2146571019519Z-mod-load-smoke` and observer
  `20260807T2149121927539Z-a37fb450a1164ec9b664812be3073704`, retaining all
  46 helper classes and observing Gunslinger once in root/chargen input. Human
  confirmation of the repaired candidate remains pending.

## Committed compatibility profiles inherited unchanged

| Profile ID | Exact local identity/disposition | Allowed scenario boundary |
|---|---|---|
| gunslinger-only | standalone; `GUNSLINGER-REPAIR-REQUIRED` | load, optional observer, class contracts, presentation; working save permitted by profile but blocked by Dodge baseline |
| gunslinger-call-of-the-wild | CotW 1.14.4c-2.1 DLL `4EBF8E1E...B26915`; `CONFLICT-CONFIRMED` | committed observer matrix; no working save |
| gunslinger-craft-magic-items | no compiled root; `STATIC-AUDITED-ONLY` | no runtime scenarios |
| gunslinger-arms-armor | Arms & Armor 1.0.10 DLL `CEC7C177...E33733`; `RUNTIME-QUALIFIED-EXACT` | load/observer/presentation/rig/switching; no working save |
| gunslinger-toggle-custom-soundpacks | Toggle Custom Soundpacks 1.0.1 DLL `A2582533...0C9434`; `RUNTIME-QUALIFIED-EXACT` | load/observer/presentation/Wwise; no working save |
| gunslinger-call-of-the-wild-craft-magic-items | CotW plus source-only CMI; `STATIC-AUDITED-ONLY` | no runtime scenarios |
| gunslinger-high-risk-combined | CotW+A&A+Toggle; `CONFLICT-OBSERVED` | committed high-risk matrix; working save permitted but not qualified |
| gunslinger-all-loadable-local | all runtime-capable local references; `CONFLICT-OBSERVED` | committed high-risk matrix; no working save |
| gunslinger-qualified-combined | A&A+Toggle; `GUNSLINGER-REPAIR-REQUIRED` | passing targeted matrix; comprehensive/working save blocked by inherited Dodge |

## Exact next action

Checkpoint `c962e33` committed the five durable mission documents. The exact
required push helper was invoked and refused the branch before network access:

`Refusing to push non-allowlisted branch
'codex/pistolero-musket-master-archetypes'. Allowed branches:
codex/firearm-wwise-audio, codex/firearm-native-weapon-rigs,
codex/postbase-archetypes-compatibility`.

This is a workstation-policy hard stop under section 17 of the work order. Raw
push, helper modification, policy workaround, reuse of the obsolete compatibility
branch, or further unpublishable source commits are not authorized.

Exact next action: a human must add
`codex/pistolero-musket-master-archetypes` to the approved push helper allowlist.
Then rerun the exact helper, verify origin points to `c962e33`, and resume with
the mandatory pre-implementation source and installed-contract inventory.

## 2026-08-07 - Publication restored and inventory completed

- Human updated the external helper policy and published exact clean checkpoint
  `8ade461eab25f8fc2b068d8a739aa8ee1044f850`.
- Verified branch, local HEAD, and origin branch all equal that SHA; worktree was
  clean; the approved helper returned `Everything up-to-date` and explicitly
  confirmed publication without merge, force-push, reset, clean, rebase, or
  history rewrite.
- Completed the mandatory source/test/runtime-harness inventory in
  `planning/PISTOLERO-MUSKET-MASTER-INVENTORY.md`.
- Read-only exact installed IL proves public archetype
  `ReplaceStartingEquipment`/`StartingItems` fields and native
  `LevelUpHelper.AddStartingItems(UnitDescriptor)` selection/grant behavior.
  The native contract directly supports the exact Musket Master array and
  explains the existing detached no-inventory-delta path.

## Exact next action

Commit and publish the pre-implementation inventory, then implement the
canonical handedness policy and scoped proficiency foundations with focused
tests before generalizing the starting-firearm observer.

## 2026-08-08 - Canonical firearm handedness

- Added one project-owned `FirearmHandedness` enum and fail-closed
  `FirearmHandednessPolicy` mapping all five current production kinds exactly
  once: Pistol/Revolver one-handed; Musket/Blunderbuss/Rifle two-handed.
- `ProductionFirearmWeaponSpec` now validates its `IsTwoHanded` contract through
  the canonical policy instead of a second embedded kind list.
- Added three focused cases covering the complete catalog, both family scopes,
  cross-family rejection, unknown scope, unknown kind, and undefined kind.
- Repository validation, complete 914/914 deterministic suite, clean exact-
  reference Release, build-output, SoundBank, package creation, and strict
  package validation pass.
- Candidate package SHA-256:
  `E7D01B712448B85CC8693135CA362C608547BB853E26B40AC040615F9A3CA7FF`;
  DLL SHA-256:
  `E1600DC86BAEAE09CE7352A30D7B2309851DB430DAD7A93AD23110CDA10EACC0`.

## Exact next action

Commit/publish canonical handedness, then add stable one-handed/two-handed
proficiency blueprints and a pure full-or-matching-scope policy. Transactionally
rewire production firearm restrictions and focused tests before adding EWP.

## 2026-08-08 - Scoped firearm proficiency foundation

- Added stable manifest-backed one-handed and two-handed proficiency facts; the
  existing full `KMG.Firearms.FirearmProficiency` GUID and behavior are unchanged.
- Added one pure `FirearmProficiencyPolicy`: exactly one known marker is required;
  full proficiency permits all five kinds; one-handed permits Pistol/Revolver;
  two-handed permits Musket/Blunderbuss/Rifle; missing facts, unknown kinds, and
  ambiguous marker counts fail closed.
- Each production firearm restriction now stores its exact project kind plus
  exact full/one-/two-handed facts. The development Test Musket retains its
  historical full-only overload and cannot leak into archetype starter logic.
- Scoped action grants are exact: one-handed grants the existing Reload action;
  two-handed grants the same Reload plus existing Scatter Shot. No duplicate
  abilities were created.
- Appended two new GUIDs without changing existing entries. Bootstrap count is
  208 active; manifest ledger is 209 stable IDs including one reserved.
- Repository validation, complete 919/919 suite, clean exact-reference Release,
  build-output, SoundBank, package creation, and strict validation pass.
- Candidate package SHA-256:
  `D3627BBCFDC818D2D25E0CB5795B21A87B91C5B2EB73F04AA8A6CA072648E17F`;
  DLL SHA-256:
  `2A8A2846464A339342F6E8A18E8CB7415D66581F4FB9077FA9FF4379D58A05AB`.
- Guarded mod load requires a clean Git state, so the first dirty-tree launch
  was correctly rejected before deployment. Commit/publish this source-qualified
  phase, then run fresh mod load against the exact commit.
- Exact published source commit `c89cf29badf45fed4193d058046ebfd828451715`
  passed guarded fresh-process mod load
  `20260808T0408526345980Z-mod-load-smoke`; structured result and evidence
  manifests are under the matching machine-local runtime-evidence directory.
  Bootstrap registered the 208-active identity set without rollback.

## Exact next action

Commit/publish this runtime evidence record, then inspect exact BAB/duplicate
prerequisite APIs and implement the single EWP
(Firearms) publication plus scoped firearm-feat prerequisites.

## 2026-08-08 - Firearm EWP and scoped feat prerequisites

- Exact installed prerequisite contracts were inspected before use. Added one
  combat feat, Exotic Weapon Proficiency (Firearms), with BAB +1, a duplicate
  guard, and an exact grant of the preserved full firearm-proficiency identity.
- Added one serializable prerequisite driven by the shared full-or-matching-scope
  policy. Custom Weapon Focus, Rapid Reload, and dependent choices now reject
  the opposite family without relying on donor crossbow categories.
- Native parametrized level-up menus filter appended firearm parameters against
  the preview unit's exact full/scoped facts; presentation enumeration remains
  complete.
- `FirearmKind` is public because the public prerequisite serializes it; the
  exact-reference compiler caught and proved this boundary.
- Bootstrap count is 209 active; the manifest ledger has 210 stable IDs including
  one reserved. No existing GUID changed.
- Repository validation, complete 920/920 suite, exact-reference Release,
  build-output, SoundBank, package creation, and strict package validation pass.
- Candidate package SHA-256:
  `FB6B71147286BD90C4082BD91D1199FB3DCB2EA345799DACA36289FB9D1EC8AC`;
  DLL SHA-256:
  `D5AFDDC74A551625E9E690A49B849A4FC974AFCB9B2BEC630E9BC7AF7B07415E`.

## Exact next action

Commit and publish the EWP/scoped-feat phase, run the guarded native firearm-feat
and presentation observers against that exact commit, record their evidence,
then generalize the starting-firearm resolver and ownership transaction.

## 2026-08-08 - EWP/scoped-feat runtime evidence

- Exact published commit `f2667c2f9cc6dae003a0aaf355d770899121af01`
  passed guarded fresh-process native feat observation
  `20260808T0420061481380Z-observe-native-weapon-feat-contracts`.
- The same exact commit passed guarded fresh-process presentation observation
  `20260808T0422069409940Z-observe-gunslinger-presentation`.
- Both runs used the strict local-runtime package through Steam App ID 640820,
  produced structured PASS results, and completed the harness restoration path.

## Exact next action

Commit and publish this evidence record, then generalize the expected starter
resolver and `GunslingerStartingFirearmOwnershipPatch` transaction with focused
inventory-delta, detached-chargen, idempotence, and rollback tests.

## 2026-08-08 - Generalized starting-firearm transaction foundation

- Added an exact committed-class-data resolver with required precedence:
  Musket Master, Pistolero, future explicit base choice, then backward-compatible
  base/Mysterious Stranger Pistol default. The archetype references remain
  intentionally unconfigured until their stable blueprints are registered.
- The native `AddStartingItems` observer now recognizes every production firearm
  by exact blueprint reference, requires exactly one newly added expected item,
  rejects a wrong or duplicate production starter, preserves detached no-delta
  behavior, verifies native +1/+1 ammunition, tops only to 20/20, and binds the
  exact observed item to the exact receiving unit.
- A repeated callback for a receiver already owning the exact battered starter
  suppresses the native grant before inventory mutation, preventing firearm and
  ammunition duplication. Rollback remains limited to the project-added 19/19.
- Added pure precedence/kind tests and updated focused starting-item source
  contracts. All three focused scripts pass; the complete suite passes 922/922.
- Exact-reference Release, build-output, SoundBank, package creation, and strict
  package validation pass. Candidate package SHA-256:
  `256067A457C85F40C49B26E6ECD31AB71CCB314660C34E74D008A79E28E36486`;
  DLL SHA-256:
  `1DA4F52B3FB15A5516FDB523CF24B9160A20C1FDBA6E4ABE6360106F0FF6CB96`.

## Exact next action

Commit and publish the generalized starter foundation, run the existing guarded
starting-items scenario against the exact commit to prove unchanged base-Pistol
behavior, then implement the shared firearm-training service and reload policy.

## 2026-08-08 - Starting-items fixture repair

- Guarded run `20260808T0429233486715Z-gunslinger-starting-items` reached an
  exact working-save load with no save write, then exposed an inherited fixture
  mismatch: it required an obsolete three-item class array although the current
  established contract is Pistol, powder, ball, and gunsmith kit. Its cleanup
  also called `SequenceEqual` with an uninitialized quantity array, masking the
  primary assertion.
- Updated only this request-local runtime fixture to require four exact distinct
  items, assert one exact gunsmith kit, and guard cleanup comparison after early
  fixture rejection. No game mechanic or starting-item blueprint changed.
- The repaired fixture passes the complete 922-test and Release/package gate.

## Exact next action

Commit/publish the fixture repair and rerun `gunslinger-starting-items` once. If
it passes, record the exact evidence and continue to the shared training service.

## 2026-08-08 - Base starter runtime preserved

- Exact published fixture/transaction commit
  `20c6c7136177f500589e7b8977f4526924caa0d1` passed guarded working-save run
  `20260808T0433173615244Z-gunslinger-starting-items`.
- The structured PASS proves the unchanged base Gunslinger exact Pistol grant,
  20/20 ammunition, one gunsmith kit, exact battered-owner binding and transfer
  persistence, exact rollback, stable money/class state, and no save write.

## Exact next action

Commit and publish this evidence record, then implement the authoritative shared
firearm-training entitlement service and refactor ordinary attacks, Dead Shot,
Scatter Shot, and Broken-misfire callers to consume it.

## 2026-08-08 - Shared firearm-training entitlement

- Added one pure entitlement policy covering exact-kind base Gun Training plus
  one-/two-handed archetype ranks. It applies Dexterity at most once, selects the
  highest valid total rather than summing overlap, preserves negative Dexterity,
  and exposes the shared Broken +2 entitlement.
- Added one runtime service used by ordinary `RuleCalculateWeaponStats`, natural
  misfire registration, Dead Shot, and Scatter Shot. A weak per-event stamp makes
  damage mutation idempotent even if stale overlapping source components observe
  the same rule event.
- Existing base choice identities and progression remain unchanged. Archetype
  training fact references are intentionally configurable and will be wired when
  their stable rank features are registered.
- Updated the historical Sprint 42 validator from the removed duplicate helper
  name to the authoritative service call. Complete suite passes 924/924.
- Exact-reference Release, build-output, SoundBank, package creation, and strict
  validation pass. Candidate package SHA-256:
  `B789946329E57FC006EB0608FDA8B17C82E0E1C131660A53253D3D8B48071834`;
  DLL SHA-256:
  `463BD07670E59F0255C0B9F73DB1D835F0C37C31D0D719D856C06D370405BDDB`.

## Exact next action

Commit and publish the shared training service, then run the existing Gun
Training, Dead Shot, and Scatter Shot guarded scenarios against the exact commit
before extending the central reload-action policy for Fast Musket.

## 2026-08-08 - Shared training runtime evidence

- Exact published commit `3abaecb42975fc9a5f334be0cf79630fe3354ae5`
  passed three guarded fresh-process runtime regressions:
  `20260808T0439425292781Z-disposable-gunslinger-gun-training`,
  `20260808T0441466553851Z-disposable-gunslinger-dead-shot`, and
  `20260808T0443494220099Z-disposable-gunslinger-scatter-shot`.
- The exact-kind base training behavior and both centralized Broken-misfire
  callers remain runtime-correct. All runs produced structured PASS results.

## Exact next action

Commit/publish this evidence record, then extend the existing central reload
policy with Fast Musket state and the complete required Musket/Blunderbuss/Rifle
matrix, preserving current base and Lightning Reload behavior.

## 2026-08-08 - Fast Musket reload-policy foundation

- Extended the single existing reload-action policy with a Fast Musket input.
  It first maps eligible two-handed FullRound profiles to Standard, then applies
  matching Rapid Reload one step. Already-Move advanced rifles remain Move until
  Rapid Reload reduces them to Free; one-handed firearms are unaffected.
- Added the complete Musket/Blunderbuss/Rifle matrix plus one-handed regression
  coverage. Existing two-argument callers retain their exact behavior.
- Added one configurable state service that requires the exact Fast Musket fact
  and either positive grit or its exact future True Grit choice. Presentation,
  command construction, ordinary availability, and full-attack auto-reload all
  re-read this state; until the archetype fact is registered it fails closed.
- Complete suite passes 925/925; exact-reference Release, build-output,
  SoundBank, package creation, and strict validation pass.
- Candidate package SHA-256:
  `46A419FF6E779115CA67D1A7DDA5611679433B8038E4FDA77009DA8E3D4CFAB5`;
  DLL SHA-256:
  `46355F9241789C796D8C1ADD8593A4C28405F5C88472A3520623D4A6375A012C`.

## Exact next action

Commit/publish the Fast Musket policy foundation, run existing reload and
full-attack reload guarded regressions, record evidence, then inspect and add
the per-attack effective-range context required by Steady Aim.

## 2026-08-08 - Reload-policy runtime evidence

- Exact published commit `877c25df447e0f039f497e72608843eb780d1871`
  passed guarded fresh-process run
  `20260808T0448213280657Z-disposable-reload-autocast`.
- Existing Reload availability, action presentation, command execution, and
  autocast behavior remain runtime-correct while unconfigured Fast Musket fails
  closed. Full-attack-specific archetype coverage will be added with its exact
  feature fixture rather than inferred from this base regression.

## Exact next action

Commit/publish this evidence record, then inspect the exact native range penalty
and maximum-range signatures and implement the isolated per-attack effective-
range context before creating Steady Aim blueprints.

## 2026-08-08 - Per-attack effective-range foundation

- Read-only installed IL confirms native weapon target range flows through
  `UnitDescriptor.GetWeaponRange(BlueprintItemWeapon)` and attack command data;
  it reads weapon metadata rather than a project deed context. No shared
  blueprint mutation is acceptable, so a narrow exact-event adapter remains
  required when the armed Steady Aim fixture exists.
- Added an immutable pure effective-increment policy and a weak exact
  `RuleAttackRoll` context. Registration is one-time per attack and cannot add
  +20 feet through duplicate callbacks.
- Touch-AC selection and Deadeye increment/cost now accept the same per-attack
  bonus. Existing constructors default to zero, preserving all base callers.
- Focused boundary coverage proves a 40-foot Musket reaches 50 feet exactly,
  changes 45 feet from second to first increment, and makes both touch-AC and
  Deadeye consume the same effective range. Complete suite passes 926/926.
- Exact-reference Release/package gate passes. Candidate package SHA-256:
  `DEA7A7DCEFC16B2FB79BCAAB2B6FA7022BFB99794F6C5A92BED9C925F1529B2D`;
  DLL SHA-256:
  `F2BA8F1B61F30CB02C539AF6926201586B6C9F2D995DAB31FE16AF39F67C5EEA`.

## Exact next action

Commit/publish the effective-range foundation, run Deadeye and firearm-AC
regressions, then create the stable Pistolero/Musket Master training and
proficiency blueprint facts before wiring full archetype replacement rows.

## 2026-08-08 - Effective-range base runtime evidence

- Exact published commit `efab35f0fcff185d0900a0c3f0ce2be0e0b64c6c`
  passed guarded fresh-process run
  `20260808T0453598757440Z-disposable-gunslinger-deadeye`.
- The zero-bonus Deadeye and firearm touch-AC stack remains runtime-correct.
  Steady Aim's +10 runtime registration and native range adapter still require
  its exact armed feature and are not claimed by this base regression.

## Exact next action

Commit/publish this evidence record, then add stable Pistol/Musket Training facts
and visible scoped-proficiency grants before constructing the exact archetypes.

## 2026-08-08 - Rankable archetype training facts

- Added stable four-rank Pistol Training and Musket Training feature identities.
  Each uses the authoritative shared event component and carries exact family,
  rank-scaling, and Broken-misfire player text.
- Registered both transactionally before class/archetype wiring and configured
  the shared runtime service with their exact facts. Existing Mysterious Stranger
  validation now scopes its historical 17-asset assertion to its own identities.
- Bootstrap count is 211 active; manifest ledger is 212 stable IDs including one
  reserved. No established GUID changed.
- Complete suite remains 926/926; exact-reference Release, build-output,
  SoundBank, package creation, and strict validation pass.
- Candidate package SHA-256:
  `EC15842222B00C37AECEC00DDDA2B6FE302B41F1210BBC5B85690A0E910C19CC`;
  DLL SHA-256:
  `EC7F7EAB030159780265997CB497CAFA4EB2FD4A0CE3FBA7313066F298E48D91`.

## Exact next action

Commit/publish the rankable training facts, then create visible Pistolero/Musket
Master proficiency features from the exact native simple/martial/light facts and
the matching scoped firearm fact before archetype progression wiring.

## 2026-08-08 - Visible scoped archetype proficiencies

- Added stable visible Pistolero and Musket Master proficiency features. Both
  reuse the exact native simple/martial/light facts; Pistolero grants only the
  one-handed scoped firearm fact and Musket Master only the two-handed fact.
- Registered these facts inside the existing class transaction where the exact
  native proficiency donors are already resolved. The class set exposes them for
  upcoming archetype replacement wiring.
- Bootstrap count is 213 active; manifest ledger is 214 stable IDs including one
  reserved. Complete suite remains 926/926 and the full Release/package gate
  passes. Candidate package SHA-256:
  `D4BC6DABE21206866BA3D7AC7065D7BD9D6AEFF43A3675B56918FB7A51EC9EFA`;
  DLL SHA-256:
  `4166E62E0967FDEFAB78CB4455468B5D9B7F710939888865D23312A199AEBE93`.

## Exact next action

Commit/publish the visible proficiency layer, then implement the mandatory
Musket Master archetype skeleton first: exact parent/replacement rows, exact
native four-item Musket starting array, catalog append, and starter-resolver
wiring, before adding its deed mechanics.

## 2026-08-08 - Mandatory Musket Master native skeleton

- Added the stable Musket Master archetype identity and wired its exact parent,
  replacement rows, scoped proficiency, exact existing Rapid Reload (Musket),
  rankable Musket Training, and passive Fast Musket ownership fact.
- Set `ReplaceStartingEquipment = true` and the native archetype starting array
  to exactly production Musket, black powder, lead ball, and gunsmith kit. The
  generalized starter resolver now recognizes the exact Musket Master archetype
  before the backward-compatible base Pistol default.
- Appended the archetype without replacing unrelated catalog entries and added
  stable Steady Aim/Fast Musket identities. Steady Aim remains a deliberately
  incomplete placeholder fact in this skeleton; no runtime-qualification claim
  is made for its action or per-shot mechanics yet.
- Bootstrap count is 216 active; manifest ledger is 217 stable IDs including one
  reserved. Repository validation and all 927 domain/reflection tests pass. Two
  sandboxed attempts failed only because the sandbox denied the pre-existing
  audio test's atomic `File.Replace`; the identical suite passed 927/927 when
  run outside that filesystem restriction.
- Exact-reference Release, build-output, SoundBank, package creation, and strict
  package validation pass. Candidate package SHA-256:
  `42D006889DA8AC5941E62DB6E542435F829C3B500B1380191CB65E5CFA6AFD82`;
  DLL SHA-256:
  `3A49B22FB93AD5A02897D70996A1DDBA2229E5D3C75CA6199ABF6033D892D98C`.

## Exact next action

Commit/publish the mandatory Musket Master native skeleton, then extend the
guarded class/archetype observer to prove the exact parent, replacement rows,
four-item starting array, preserved unrelated catalog entries, and exact
starter-resolver identity in a fresh guarded Kingmaker process before adding
Steady Aim mechanics.

## 2026-08-08 - Guarded Musket Master blueprint observer

- Extended the existing read-only class-blueprint scenario with exact Musket
  Master parent/catalog membership, six remove/add rows, existing Rapid Reload
  (Musket) identity, ordered four-item starting array, and starter-resolver
  reference assertions.
- The observer compares current live blueprint references only and performs no
  registry, catalog, inventory, save, or optional-mod mutation.
- The first clean build exposed and rejected a compile-only accessor mistake;
  the exact already-registered Musket Training reference is now carried by the
  Musket Master set without changing blueprint registration. The corrected
  source passes all 927 tests and the full Release/package gate.
- Candidate package SHA-256:
  `2DAA1387E5A1A147D053548EBCDD3D6A08DC2DE7DED484FFC20280F82AE7E7CA`;
  DLL SHA-256:
  `183B8FF5A5A4BF4E5CC0E5705141720A98AC7C80D9AC8E0132C29FEB27E597A5`.

## Exact next action

Commit/publish the guarded Musket Master observer, then run a fresh guarded
`observe-class-blueprint-contracts` process against that exact commit/package;
inspect every structured Musket assertion and record the run before beginning
Steady Aim mechanics.

## 2026-08-08 - Observer registration correction

- Guarded run directory
  `20260808T0514160336987Z-observe-class-blueprint-contracts` returned PASS for
  the inherited three class assertions, but structured-result inspection proved
  the new Musket assertions were absent. A broad source anchor had registered
  them in the feature-acceptance path instead of the exact class observer.
- This run is explicitly insufficient and is not counted as Musket Master
  evidence. Moved the call into the exact class-observer method, removed the
  unintended feature-acceptance call, and hardened the source test to verify
  method-local registration.
- Corrected source passes all 927 tests and the full Release/package gate.
  Candidate package SHA-256:
  `4DAC4B3FF3B1EB318C99FF30490E01B24B75C59294E8A08702458B3F83E7306E`;
  DLL SHA-256:
  `EC55839DDF0EA342858FF15C812E101C633D8E5B6E1F1ADE48089142A48FC351`.

## Exact next action

Commit/publish the exact observer registration correction, rerun fresh guarded
`observe-class-blueprint-contracts`, and require all four named Musket Master
assertions in the structured result before recording runtime evidence.
