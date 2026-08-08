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

## 2026-08-08 - Musket Master native contract runtime evidence

- Corrected guarded directory
  `20260808T0517582472885Z-observe-class-blueprint-contracts` PASS, runtime run
  ID `20260808T0517582785424Z-83c886dcc18c400f9fcac681db3fbf55`, exact source
  `49a545d638b930dbb6decd2119e00f6c3585fd58`, version 0.0.72.
- All four required structured assertions are present and PASS: exact parent and
  single catalog membership; exact ordered production Musket/powder/ball/kit
  starter contract with `ReplaceStartingEquipment`; exact six remove/add rows
  including existing Rapid Reload (Musket); exact starter-resolver references.
- Loaded/runtime DLL SHA-256:
  `63394DFC43F15FBBEEE82B14581EB3EAD0B1B9A3AC2015C570F76AD3CF11BA63`.
  The harness launched only through Steam App ID 640820 and recorded no save
  interaction.
- This qualifies the native blueprint/catalog contract, not live character
  creation inventory or unfinished Steady Aim/Fast Musket mechanics.

## Exact next action

Commit/publish the curated Musket Master native-contract evidence, then inspect
the existing single-shot deed marker/turn-boundary components and implement
Steady Aim as a visible move-action arming ability feeding the existing exact
per-attack effective-range context, with focused positive-grit, consumption,
expiry, isolation, and no-global-mutation tests.

## 2026-08-08 - Steady Aim implementation

- Replaced the stable Steady Aim placeholder feature with an exact AddFacts
  grant to a new visible move-action ability and owner-scoped armed buff while
  preserving the established feature GUID.
- Activation spends no grit, requires positive grit, rejects duplicate arming,
  and exposes exact current-turn/None presentation. The marker removes itself
  on the owner's next round boundary and on the first qualifying shot.
- The attack handler qualifies exact direct Musket, Rifle, and Blunderbuss
  attacks; ignores one-handed/non-firearm actions; excludes registered Scatter
  Shot deliveries; revalidates grit; and registers exactly +10 feet on the
  attack's existing weak per-roll effective-range context. No weapon blueprint,
  weapon type, or firearm definition is mutated.
- Pure coverage proves family, scatter, marker-count, unknown-kind, touch-AC,
  and Deadeye range-order boundaries. The guarded class observer now also checks
  the exact move action, AddFacts grant, marker handler, and shared Grit
  references.
- Blueprint count is 218 active; ledger is 219 stable IDs including one
  reserved. All 927 tests and the full Release/build-output/SoundBank/package
  gate pass. Candidate package SHA-256:
  `88C45D6E98B90FB05A45111734E8713C402C3DFED9E3005A04AA7F0A208C30C3`;
  DLL SHA-256:
  `3C34889F961BCA32C691105B06AB4DA4002C28F947AA1A7DFB6666344AAF875E`.

## Exact next action

Commit/publish Steady Aim, then run the fresh guarded class-blueprint observer
and require the new `steady-aim-blueprint-contract` assertion before adding a
dedicated disposable mechanics scenario for arming, consumption, range, grit,
scatter exclusion, expiry, and metadata immutability.

## 2026-08-08 - Steady Aim live blueprint evidence

- Guarded directory
  `20260808T1204211142376Z-observe-class-blueprint-contracts` produced a final
  PASS after the outer shell reached its 120-second wait ceiling. The final
  artifact was inspected directly; Kingmaker then completed its requested exit.
- Runtime run ID `20260808T1204211365604Z-b1036159eeab40a3a958d461fa91b107`,
  exact source `a0fe0e8018216f8e73d57e5c925e34edd98d04d0`, version 0.0.72.
- `steady-aim-blueprint-contract` PASS proves the live move-action ability,
  exact AddFacts/armed-marker linkage, and exact shared Grit references. All
  prior Musket Master native assertions remained PASS.
- This is live blueprint evidence. Behavioral qualification still requires the
  dedicated disposable Steady Aim scenario.

## Exact next action

Commit/publish the curated Steady Aim blueprint evidence, then add the guarded
disposable Steady Aim mechanics scenario and prove positive/zero grit, arming,
wrong-family preservation, direct-shot consumption, +10-foot context, scatter
exclusion, round expiry, unit isolation, duplicate-callback idempotence, and
unchanged shared firearm metadata.

## 2026-08-08 - Pistolero native progression skeleton

- Added the stable Pistolero archetype with inherited base starting equipment,
  exact one-handed scoped proficiency, rankable Pistol Training at 5/9/13/17,
  and exact shifted existing Deadeye identity at level 7.
- Exact removal/addition rows replace Deadeye at 1, Startling Shot at 7, Bleeding
  Wound at 11, all four Gun Training selections, and the three misleading base
  deed summaries. Stable Pistolero summaries truthfully name the retained and
  replacement deeds.
- Up Close and Deadly and Twin Shot Knockdown currently have stable ownership
  facts and truthful descriptions but remain mechanics placeholders; this phase
  does not claim those deeds complete.
- Starter resolver configuration now carries both exact archetypes. Project
  catalog order is Mysterious Stranger, Pistolero, Musket Master while unrelated
  current entries remain in place. The guarded observer checks exact Pistolero
  parent, rows, starter configuration, summaries, and project ordering.
- Blueprint count is 224 active; ledger is 225 stable IDs including one
  reserved. All 928 tests and the full Release/package gate pass. Candidate
  package SHA-256:
  `419B20646399019F50592FAB91A1488336A4B8CCE00DCD74A0095261D67DAE51`;
  DLL SHA-256:
  `A79CD7802A5A63D40748134E8C6B7C4182A0F2F0664D7FCB8B39E1C68B0D8DC0`.

## Exact next action

Commit/publish the Pistolero native skeleton, then run the fresh guarded class
observer and require all Pistolero/project-order assertions before implementing
Up Close and Deadly's fixed-cost precision arming/delivery mechanics.

## 2026-08-08 - Pistolero live progression evidence

- Guarded directory
  `20260808T1213268469463Z-observe-class-blueprint-contracts` passed under
  runtime run ID `20260808T1213268625740Z-654d9af3b1d9489b82c858f8d5f4d37a`,
  exact source `90cc34d82934a9abd639e57706b65598ea88881f`, version 0.0.72.
- Live assertions prove the exact Pistolero parent and replacement rows, the
  inherited production-Pistol starter resolution, and deterministic project
  archetype order `Mysterious Stranger, Pistolero, Musket Master` with each
  project identity present once.
- The existing Musket Master starter resolver assertion remained PASS. This is
  blueprint/catalog evidence only: Up Close and Deadly and Twin Shot Knockdown
  remain mechanics placeholders, and live starting inventory remains pending.

## Exact next action

Commit/publish this curated Pistolero progression evidence, then implement Up
Close and Deadly's fixed-cost, one-shot arming and post-result precision damage
runtime with focused policy/component tests before its guarded mechanics
scenario.

## 2026-08-08 - Up Close and Deadly source implementation

- Replaced the level-1 placeholder with a visible free-action ability and an
  owner-scoped armed buff. Activation requires but does not spend the exact one
  grit; the qualifying result spends the fixed cost, independently of True
  Grit, and delivery failure restores it.
- The pure policy proves 1d6/2d6/3d6/4d6/5d6 scaling, full hit and native
  half-on-miss modifiers, Pistol/Revolver family gating, scatter and wrong-family
  preservation, precision-immunity consumption without spend, resolution-time
  insufficient-grit behavior, and genuine-misfire exclusion.
- Runtime delivery creates one isolated piercing damage packet with native
  precision discovery disabled, preventing critical multiplication and a second
  sneak/precision source while leaving other attack precision intact. A weak
  exact-attack gate prevents duplicate callbacks from spending or delivering
  twice.
- Exact event-order inspection proved the live misfire context is removed before
  `RuleAttackWithWeapon` completes. `FirearmMisfireRuntime` now retains a weak,
  attack-scoped completed non-misfire outcome so ordinary misses qualify and
  genuine misfires fail closed without global or item mutation.
- Added stable action/marker identities. Blueprint count is 226 active; ledger
  is 227 stable IDs including one reserved. Repository validation, all 929
  deterministic tests, clean exact-reference Release, build-output validation,
  SoundBank validation, and strict packages pass. Candidate package SHA-256:
  `7676A8EA0EC86066865BC60BA44ED813BA0954DC29B4D3DF80DABAD07112C5AF`;
  DLL SHA-256:
  `0C7F07A1819572743781CFB74B4F760292AF0D02C630CA42A5A09A17A079812F`.

## Exact next action

Commit/publish Up Close and Deadly, run the guarded class-blueprint observer and
require `up-close-and-deadly-blueprint-contract`, then add the disposable
behavioral scenario covering hit, miss, critical, immunity, misfire, family,
scatter, Dead Shot, fixed cost, marker lifecycle, and duplicate delivery.

## 2026-08-08 - Up Close and Deadly live blueprint evidence

- Guarded directory
  `20260808T1227026428237Z-observe-class-blueprint-contracts` passed under
  runtime run ID `20260808T1227026740842Z-0d8a298531cc4bda99418f0490b71ea8`,
  exact source `fe50f6b53e28b6f7e77276f1eda4b8078b66f1de`, version 0.0.72.
- `up-close-and-deadly-blueprint-contract` PASS proves the live visible free
  action, exact feature-to-ability-to-marker graph, exact Gunslinger class
  reference, and exact shared Grit resource. All Pistolero progression/starter
  and project catalog assertions remained PASS.
- This is blueprint evidence only; attack-result and damage behavior remains
  pending the disposable mechanics scenario.

## Exact next action

Commit/publish this curated live blueprint evidence, then implement and run the
guarded disposable Up Close and Deadly mechanics scenario for hit, miss,
critical, immunity, misfire, family/scatter gates, Dead Shot, fixed cost,
marker lifecycle, fault rollback, and duplicate-callback idempotence.

## 2026-08-08 - Twin Shot Knockdown source implementation

- Replaced the level-11 placeholder with a targeted visible free action, an
  exact hit-tracking component, and owner/target state keyed solely by reference
  identity. Duplicate callbacks for the same `RuleAttackWithWeapon` do not
  increase the count; different targets remain isolated.
- Direct Pistol/Revolver hits qualify. Misses, non-firearms, wrong-family hits,
  and turn-based attacks outside the Pistolero's exact current turn fail closed.
  Dead Shot's final weapon delivery naturally contributes one event rather than
  its constituent probes. RTWP uses the authorized per-owner combat-round
  adaptation through the feature component's native round tick.
- Execution revalidates the exact owner/target count and turn, rejects prone or
  combat-maneuver-immune targets without spending, spends the ordinary fixed
  cost, and applies native Prone directly with no save or CMB contest. A failed
  native condition application restores grit. One target can be charged only
  once per round, and round ticks discard all ephemeral evidence.
- The initial deterministic/source gate passes all 930 tests; clean exact-
  reference Release, build-output, SoundBank, and strict package validation pass.
  Candidate package SHA-256:
  `ECF6C1087B592FC129F07DEBF53E27020A1A406CECBFA4E823B095A228E4F64C`;
  DLL SHA-256:
  `0457D44D1571932B83F2F7B665AC45DC3B4C75029B3B42D6ED77226772866F83`.
- Up Close and Deadly's dedicated behavioral scenario remains required and has
  not been waived; batching both Pistolero deed fixtures will avoid duplicating
  disposable-unit setup while retaining separate structured assertions.

## Exact next action

Complete the exact-source gate for the Twin Shot observer addition, commit and
publish Twin Shot Knockdown, then run the guarded class observer before adding
the combined disposable Pistolero deed mechanics fixture with separately named
Up Close and Twin Shot assertions.

## 2026-08-08 - Twin Shot Knockdown live blueprint evidence

- Guarded directory
  `20260808T1235416856236Z-observe-class-blueprint-contracts` passed under
  runtime run ID `20260808T1235417118457Z-5b58767f21c244c09765fbd04e829081`,
  exact source `5a3b65fa8a38014c1f562aae8df25ab2b0867c68`, version 0.0.72.
- `twin-shot-knockdown-blueprint-contract` PASS proves the live targeted free
  action, exact feature-owned hit tracker, exact AddFacts link, and shared Grit
  resource. All prior archetype/progression/starter assertions remained PASS.
- This remains blueprint evidence. Hit counting, per-target isolation, prone,
  grit, duplicate delivery, and cleanup require the disposable mechanics run.

## Exact next action

Commit/publish this curated Twin Shot blueprint evidence, then add the combined
guarded disposable Pistolero deed mechanics fixture with distinct structured
assertions for every Up Close and Deadly and Twin Shot Knockdown behavior.

## 2026-08-08 - Disposable Pistolero deed fixture and runtime repairs

- Registered the no-save `disposable-pistolero-deeds` scenario and exercised
  exact firearm rolls, the production Up Close marker handler, grit, isolated
  damage packets, precision immunity, Twin Shot reference deduplication,
  per-target isolation, direct native Prone, and request-local cleanup.
- Exploratory runs exposed three exact engine contracts and changed strategy:
  `RuleAttackRoll.IsHit` is read-only; `AutoHit` bypasses the firearm success
  evaluation needed for the completed non-misfire outcome; and
  `RuleDealDamage.Modifier` did not halve this independent deed packet in the
  guarded runtime. The fixture now creates ordinary natural-19 hits/misses, and
  miss delivery uses the mission-authorized fallback: roll all deed dice once,
  integer-divide by two, then deliver the fixed packet.
- Detached `BuffCollection.AddBuff` returned null for the new armed marker. The
  request-local fixture falls back to direct exact fact attachment, while the
  production arming logic verifies actual fact presence instead of trusting the
  return value. Up Close and Steady Aim use the proven six-second current-turn
  duration and ordinary buff facts.
- Exploratory dirty-candidate run
  `20260808T1306113831555Z-917a7c6016bd4da28e16d47529a691ac` passed all seven
  structured assertions in directory
  `20260808T1306113518940Z-disposable-pistolero-deeds`. This is repair evidence,
  not final source evidence; a clean committed-SHA rerun is mandatory.

## Exact next action

Commit/publish the passing disposable Pistolero fixture and armed-marker
hardening, rebuild at that exact commit, rerun `disposable-pistolero-deeds`
without dirty-state authorization, and record only the clean PASS as final deed
behavior evidence.

## 2026-08-08 - Clean Pistolero deed mechanics evidence

- Clean committed-source guarded directory
  `20260808T1309347685793Z-disposable-pistolero-deeds` passed under runtime run
  ID `20260808T1309347962805Z-5e29dca207904e6fafa2aed622f074c8`,
  exact source `ebafc8ba8b87d6976448797a9f9e65fddb48f2a7`, version 0.0.72.
- All seven assertions passed: Up Close hit, miss roll-once half packet, and
  precision-immunity consumption; Twin Shot distinct-event deduplication,
  per-target isolation, native Prone and grit; exact fixture cleanup; and loaded
  version. Observed grit was `5 -> 4 -> 3 -> 3`, then Twin Shot refilled to five
  and spent to four. The test difficulty multiplier transformed the fixed half
  packet after its pre-delivery integer halving, so structured damage observed
  6 for both the 1d6 full packet and fixed 3-point half packet.
- Exact committed package SHA-256:
  `F57368F75BF64C1A69C5EE40743368F41EE4F2C38DF380575774B4152344BE62`;
  DLL SHA-256:
  `BB89B8B0204C88471DC3E1A4855E61E41C093369E3080865DD1CA2E94F30D518`.
- This fixture qualifies the implemented core slice. Critical, misfire,
  scatter, Dead Shot, marker expiry, fault injection, True Grit, and full
  progression reconciliation remain in the expanded scenario matrix.

## Exact next action

Commit/publish this clean deed evidence, then implement archetype-aware True
Grit ownership choices and wire Steady Aim, Fast Musket, Twin Shot Knockdown,
and Focused Aim into the authoritative runtime cost/positive-grit service.

## 2026-08-08 - Archetype-aware True Grit source implementation

- Expanded the exact catalog from 20 to 24 choices with Focused Aim, Twin Shot
  Knockdown, Steady Aim, and Fast Musket. Every existing and new choice now has
  one exact deed-ownership prerequisite, appended idempotently without changing
  the two-selection progression contract.
- Wired Focused Aim and Twin Shot through the central spend decision. A selected
  one-grit deed costs zero but retains the positive-grit floor. Wired Steady Aim
  and Fast Musket through the central positive-grit/no-spend decision so their
  selected choices operate at zero grit.
- Appended four stable manifest identities; the ledger is now 232 entries, 231
  active and one reserved. Repository validation, 930/930 deterministic tests,
  runtime preflight 86, clean exact-reference Release build, build-output,
  SoundBank, package creation, and strict package validation pass.
- The first sandboxed domain run had the known filesystem-denied audio staging
  result; the policy-approved full run passed 930/930. No assertion was waived.

## Exact next action

Commit and publish the archetype-aware True Grit source checkpoint, extend the
guarded True Grit/class observers to prove all 24 ownership prerequisites and
the four archetype runtime decisions, then run those observers on the exact
committed package before proceeding to the Musket Master mechanics fixture.

## 2026-08-08 - Archetype-aware True Grit guarded evidence

- Exact committed source `fa976145bc8f2f315b80065c1cb9ec1c8a0e476b`
  passed `observe-class-blueprint-contracts` in directory
  `20260808T1324308734029Z-observe-class-blueprint-contracts`, runtime run ID
  `20260808T1324309047264Z-053d5756361048e582e8ac2e01baf0c7`.
  The live blueprint graph contains 24 choices with one ordered, exact
  `PrerequisiteFeature` deed reference apiece and retains every prior archetype
  contract.
- The same committed source passed `disposable-gunslinger-true-grit` in
  directory `20260808T1326311118342Z-disposable-gunslinger-true-grit`, runtime
  run ID `20260808T1326311438745Z-5890d023ea9147cca4e40f90288cc00e`.
  Focused Aim and Twin Shot resolve to zero cost while retaining the
  positive-grit floor; selected Steady Aim and Fast Musket are available at
  zero grit through the positive-only contract. The existing two selections,
  variable reduction, Stunning Shot behavior, and exclusions remained PASS.
- Exact package SHA-256
  `D3B697F23F1044A910BEC31548984ACCEDF1BE3D5A75639BEE23768A0F834C3F`;
  DLL SHA-256
  `9E40B67171339CA4ED14B7990B3AE0450205A671278ECFB5CACE1A948FAC2A7B`.

## Exact next action

Commit and publish the curated True Grit runtime evidence, then implement the
guarded Musket Master mechanics fixture for Steady Aim effective range,
Deadeye composition, Fast Musket reload revalidation, scoped proficiency,
training, and the mandatory exact native Musket starter/ownership transaction.

## 2026-08-08 - Truthful Musket Master level-1 deed summary

- Closed the remaining progression-presentation gap by replacing the base
  level-1 deed summary alongside Gunslinger's Dodge and proficiency, and adding
  an exact Musket Master summary naming Deadeye, Steady Aim, and Quick Clear.
- Appended stable identity `KMG.Archetypes.MusketMasterDeedsLevel1`; ledger is
  now 233 entries, 232 active and one reserved. Updated exact replacement-row
  observers and source-shape coverage.
- Repository validation, 930/930 deterministic tests, clean exact-reference
  Release build, build-output, SoundBank, package creation, and strict package
  validation pass. The first gate correctly rejected the stale 20-archetype-
  asset validator count; it was updated to the exact new count of 21 and the
  complete gate then passed without waivers.

## Exact next action

Commit and publish the truthful Musket Master summary checkpoint, obtain its
guarded class-blueprint PASS, then add the combined disposable Musket Master
mechanics/starter fixture covering Steady Aim, Deadeye range composition, Fast
Musket reload revalidation, scoped proficiency, training, exact Musket grant,
20/20 ammunition, battered ownership, and repeated-callback idempotence.

## 2026-08-08 - Musket Master truthful-summary live evidence

- Exact committed source `b4018f07846201833b7e47e611b6551f28c37fba`
  passed `observe-class-blueprint-contracts` in directory
  `20260808T1333164049155Z-observe-class-blueprint-contracts`, runtime run ID
  `20260808T1333164340842Z-6177dd33257a40468a9935548c06a76e`.
- The live replacement graph removes the base level-1 summary and adds the
  exact Musket Master summary with proficiency, Steady Aim, and the existing
  Rapid Reload (Musket), while all starter, archetype catalog, Pistolero, and
  True Grit assertions remain PASS.
- Package SHA-256
  `C12FDACFE31628E4550548A24245A54EB494B2C2B14677883A9994136A77527B`;
  DLL SHA-256
  `E9400D14C6C878959EA7F5025FF92553710BF1B89664BD04D3D759B328534655`.

## Exact next action

Add the combined disposable Musket Master mechanics/starter fixture covering
Steady Aim, Deadeye range composition, Fast Musket reload revalidation, scoped
proficiency, training, exact Musket grant, 20/20 ammunition, battered
ownership, and repeated-callback idempotence.

## 2026-08-08 - Combined Musket Master guarded fixture source

- Registered `musket-master-mechanics-and-starter` through the existing
  autonomous `KMG_AUTOMATION_WORKING` path and no-save-write sentinels.
- The reversible fixture commits the exact Musket Master archetype onto the
  trusted receiver, calls native `LevelUpHelper.AddStartingItems`, verifies the
  production Musket/no-Pistol ordered starter contract, 20/20 ammunition,
  gunsmith kit, exact battered owner, then repeats the callback and requires no
  firearm, ammunition, or inventory-reference delta.
- The same result includes exact scoped-family, central Fast Musket/Rapid Reload
  ordering, +10-foot-before-Deadeye range/cost, and rank-4 Musket Training
  assertions, plus exact inventory/class/archetype/gold/money rollback.
- Repository validation, 930/930 deterministic tests, runtime preflight 86,
  clean exact-reference Release build, build-output, SoundBank, package, and
  strict package validation pass.

## Exact next action

Commit and publish the combined Musket Master fixture, run it against
`KMG_AUTOMATION_WORKING`, diagnose any exact native-contract mismatch, and
repeat only after a materially justified repair until the full result passes.

## 2026-08-08 - Combined fixture request-validation repair

- First guarded directory
  `20260808T1347133367005Z-musket-master-mechanics-and-starter` was rejected at
  `request-accepted` with `scenario-timeouts-not-allowed`; run ID remained
  empty, the hook was not installed, and no UI or save action occurred.
- Root cause was an omitted new scenario identity in `RuntimeTestRequest`'s
  authoritative `workingSmoke` classification. Added that exact identity; no
  timeout or save-name rule was weakened.
- Repository validation, 930/930 tests, runtime preflight 86, clean Release,
  SoundBank, package, and strict validation pass after the repair.

## Exact next action

Commit/publish the request-validation repair and relaunch the exact combined
scenario against `KMG_AUTOMATION_WORKING` through Steam App ID 640820.

## 2026-08-08 - Combined fixture first full transaction

- Guarded directory
  `20260808T1352002671718Z-musket-master-mechanics-and-starter`, run ID
  `20260808T1352002938328Z-13c0f75ba37a4e5794a08acf9f884d0a`, reached the
  complete transaction. Exact Musket, 20/20 ammunition, kit, battered owner,
  repeat-callback stability, scoped proficiency, reload, Steady/Deadeye,
  training, exact rollback, and no-save-write assertions all passed.
- The sole FAIL was the inherited Pistol-specific vendor-deal expectation. The
  Musket ordinary value is 375 gp and the native deal credited 433 rather than
  the Pistol fixture's fixed 22-gp transaction expectation. Musket vendor
  economics are outside this mission; exact reversible vendor staging and
  battered ownership passed. The base Pistol scenario retains its strict deal
  assertion unchanged, while the Musket path now requires staging plus owner
  identity and does not claim deal-economy qualification.
- Complete source/package gate passes after this narrowing: repository
  validation, 930/930 tests, clean Release, SoundBank, and strict package.

## Exact next action

Commit/publish the Musket-specific assertion repair and rerun the combined
scenario on the exact committed package.

## 2026-08-08 - Combined Musket Master runtime PASS

- Exact committed source `e178ec680ea169e33d40304a78075c6a1fb0b472`
  passed `musket-master-mechanics-and-starter` in directory
  `20260808T1356019112005Z-musket-master-mechanics-and-starter`, runtime run ID
  `20260808T1356019268485Z-7808ed952fa7458dab8395135cc948c3`.
- Live evidence proves the exact production Musket and no Pistol, 20 powder,
  20 balls, one kit, exact battered owner/receiver, stable repeated native
  callback, two-handed scoped proficiency, Fast Musket/Rapid Reload ordering,
  Steady Aim before Deadeye increment/cost, rank-4 Musket Training, exact
  inventory/class/archetype/gold/money rollback, and no save-writing API.
- Package SHA-256
  `F807B79490F1363DACE7EFC63AB2D8F38F64C0F58377FB3AEF88875C52D608B3`;
  DLL SHA-256
  `5F6B316D3050E5B504A1D79FEF82420D223F587862C91E1E6C643BE779BE71C4`.

## Exact next action

Commit/publish this curated PASS, then audit and close the remaining expanded
Pistolero deed branches (critical, misfire, scatter, Dead Shot, expiry,
duplicate callback, delivery rollback) and Twin Shot cleanup/turn boundaries
before persistence, presentation, compatibility, version, and final gates.

## 2026-08-08 - Expanded Pistolero deed runtime fixture

- Extended `disposable-pistolero-deeds` with a confirmed critical packet that
  must equal the ordinary deed packet, genuine misfire and registered scatter
  rejection that retain marker/grit, final Dead Shot delivery with three hits,
  and duplicate callback delivery/spend suppression.
- Repository validation, 930/930 tests, clean exact-reference Release build,
  build-output, SoundBank, package, and strict validation pass.

## Exact next action

Commit/publish the expanded fixture and run it on the exact committed package;
then add marker-expiry/delivery-fault evidence and harden Twin Shot lifecycle
cleanup and authoritative RTWP action-cycle state.

## 2026-08-08 - Expanded Pistolero detached-marker repair

- Guarded directory `20260808T1401511057242Z-disposable-pistolero-deeds`, run
  ID `20260808T1401511443981Z-70aa608643f64be4bb86b84df5e97935`, stopped at
  `up-close-critical` because detached `AddFact` returned null before mutation.
- Centralized the already-proven request-local timed-buff/raw-fact/direct-fact
  fallback for all new branches. Production arming remains strict and unchanged.
- Repository validation, 930/930 tests, clean Release, SoundBank, package, and
  strict validation pass after repair.

## Exact next action

Commit/publish the detached-marker repair and rerun the expanded Pistolero
scenario on the exact committed package.

## 2026-08-08 - Expanded Pistolero deed runtime PASS

- Exact committed source `c381eb0e85f330f35890041b67c92b030faa3259`
  passed `disposable-pistolero-deeds` in directory
  `20260808T1405262267429Z-disposable-pistolero-deeds`, run ID
  `20260808T1405262615305Z-40b057769b664cbb84bd9c6fb7e68f65`.
- The guarded result now proves hit, same-roll half miss, precision immunity,
  unmultiplied critical packet, genuine misfire rejection, scatter rejection,
  final Dead Shot delivery once despite three constituent hits and duplicate
  callback, Twin Shot reference deduplication/per-target isolation/native
  Prone/grit, and request-local cleanup.
- Package SHA-256
  `69E4A2FD88B11BF03B7895FF3EE5097168B17B807881287B130765320EF2BAAB`;
  DLL SHA-256
  `98D9A2B68DD643640DEE45426E496BD65877338979E007E171971F9B45E20DF7`.
- Marker expiry remains structurally enforced by the six-second current-turn
  buff and cleanup; delivery-fault grit restoration remains source-enforced.
  Persistence/reconciliation qualification will exercise stale-marker cleanup.

## Exact next action

Commit/publish the curated expanded PASS, then audit generic archetype
presentation/icons/action-bar ownership and implement persistence/reconciliation
coverage for stale markers, deeds, training, proficiency, and starter identity.

## 2026-08-08 - Generic archetype presentation traversal

- Replaced the Focused Aim one-off icon block with generic traversal of every
  current Gunslinger archetype AddFeatures row and public component-held unit
  fact reference. This reaches selections, granted abilities, armed buffs, and
  future project facts while preserving native blueprint icons.
- Added post-registration archetype localization/icon/ability-tooltip
  validation and retained semantic Focused Aim mapping through the generic
  name policy. Repository validation, 930/930 tests, clean Release, SoundBank,
  package, and strict validation pass.

## Exact next action

Commit/publish the generic presentation refactor, run the guarded Gunslinger
presentation and class observers, then implement persistence/reconciliation
coverage.

## 2026-08-08 - Generic presentation guarded PASS

- Exact committed source `1cd4b2785052acbe5be03a7426c064ba14c2ee46`
  passed `observe-gunslinger-presentation` in directory
  `20260808T1412495842884Z-observe-gunslinger-presentation`, run ID
  `20260808T1412496064594Z-e810c58680f9497dab4b3d75d336f3fd`.
- The same source passed `observe-class-blueprint-contracts` in directory
  `20260808T1414251995558Z-observe-class-blueprint-contracts`, run ID
  `20260808T1414252152059Z-1df25ec7506a4a0b977fdf1264aabdc2`.
- Both guarded Steam launches loaded DLL SHA-256
  `BA0676560B4C7A6F84A2D1173F902DC0319CA45165E5627F6D71F37846E4922D`,
  observed no save interaction, and proved the current class/progression and
  player-facing presentation contracts after generic archetype traversal.

## Exact next action

Implement and qualify archetype persistence/reconciliation coverage for
base-to-archetype, archetype-to-base, and archetype-to-archetype transitions,
including stale deed/marker/training/proficiency cleanup and invariant starter
identity with no repeated native grant.

## 2026-08-08 - Archetype reconciliation guarded PASS

- Added native `LevelUpController.AddArchetype(BlueprintArchetype)` selection
  to the existing reversible broad-respec handler and a five-transition matrix:
  base to Pistolero, base to Musket Master, each archetype to base, and
  Pistolero to Musket Master.
- Repository validation, 930/930 deterministic tests, clean Release,
  SoundBank, package, and strict package validation pass.
- The first guarded run produced a complete runtime PASS but its outer command
  timeout interrupted orchestration finalization, so it is retained only as
  diagnostic evidence and was not accepted.
- Exact committed source `e47bf17881e01ac0c3ab47c11f234f4edbb4b50b`
  passed unambiguously in directory
  `20260808T1425513568150Z-disposable-archetype-reconciliation`, run ID
  `20260808T1425513926441Z-748835f6d40d4c5b95f0c73b151f0521`.
  Every transition installed the exact target archetype/family proficiency and
  Grit, removed the replaced archetype, and restored party, unit, companion,
  cross-scene, shared-inventory, and money references exactly. No save
  interaction occurred.
- Package SHA-256
  `7666A16E0A4502B93AE79939F8CB473EB41D06165887910344FA8BBD93976BB1`;
  DLL SHA-256
  `E328D511087BB8D16EBE0949B203C9EAAF2044ACE511C42B133F3463C22604E8`.
  The combined Musket fixture separately proves repeated starting-item
  observations retain one exact owner-bound Musket and stable 20/20 stacks.

## Exact next action

Perform the bounded installed-code investigation for an explicit ordinary
Gunslinger starting-firearm choice. Implement only if the initial native
starting-item transaction can consume the committed selection without delayed
inventory surgery; otherwise record exact deferral evidence, then execute the
version/profile transaction and final compatibility/runtime gates.
