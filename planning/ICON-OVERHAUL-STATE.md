# Icon overhaul v2 state

Status: **NATIVE_VIEWPORT_QUALIFIED; NATIVE_UI_REVIEW_IN_PROGRESS**. This is a technical
checkpoint. Ten exact pilot images and family direction are approved; eighty later
paintings, complete native UI and final acceptance remain unapproved.

- Branch: `codex/icon-art-overhaul-v2`. At the owner's explicit request, current
  master `9dc2b6301d97bc83540845240544d34b6fad4b48` (PR #18, `0.0.128`)
  is integrated with published icon parent `84c2ed9ed25e244e9cdb3e9e689ae72db3619a9f`.
  The ordinary feature-branch merge preserves published history and the original
  `codex/icon-art-overhaul` branch. This authorizes no feature-to-master merge or release.
  The owner allowlisted this branch; the mandated push wrapper works.
- Published merge checkpoint: `75017b3e039a16dbfd44b9212e4983de53be8249`.
  The current bounded extension captures existing native weapon and learning rows
  inside their scroll viewport, restoring its position and velocity afterward.
  Fighter cases learn each exotic proficiency through their normal feat choices
  before selecting Weapon Focus. No production pixels, assignments, firearm
  adapter, prerequisites or gameplay code changed in this extension.
- Exact current qualification: [viewport record](../reports/icon-overhaul/NATIVE-VIEWPORT-QUALIFICATION.json).
  Build used the working tree based on `75017b3`, source SHA-256
  `600da30a7d2636ffd427e3f7df6efab527383bf0da1ecc4532c653b4fd6a5696`.
  Six guarded Steam runs passed **86 assertions**: four ordinary creator cases
  (Ifrit/Gunslinger, Ifrit/Fighter, Oread/Fighter, Sylph/Fighter), learning preview
  (31) and final working-save smoke (11). No save write was observed.
  Documentation/catalog curation afterward changes neither compiled code nor pixels.
- Required build gates passed: repository validation, 22 catalog cases, six paired
  evidence cases, ten native screenshot corruption cases, eleven request cases,
  all **1,629 domain tests**, clean Release and strict **224-file** package validation.
  The preceding merge DLL separately passed ten synthetic compiled input-wrapper
  cases; that result is not mislabeled as a repeat on the new capture DLL.
- The unchanged production integration retains its [nine-run record](../reports/icon-overhaul/NATIVE-128-QUALIFICATION.json): candidate/control **27/27**;
  dependent firearm feats **12**; Gunslinger OFF **42**; Eastern Weapons OFF **43**;
  Undine creator **11**; real Ifrit mercenary regression **12**; spellbook **31**;
  final working-save smoke **11**. The paired census preserves all **35,756** other
  icon transitions and **255** owned blueprint graphs. No save write was observed.
- Firearm presentation: native full/level-up entries and selected/sheet data keep
  exact P/M/B blueprint parameters, names and fallback sprites. Rapid Reload's
  real static `Items` path now adapts only its existing three official rows.
  The current Undine capture verifies all three actual native glyphs, without
  clipping. All-ON/Gunslinger-OFF have 426 exact data/control rows; Eastern-OFF has 411.
  Complete chosen-feat/sheet rendering and saved-parameter evidence remain pending.
- Current native evidence: **138 unmodified framebuffer captures**, including ten
  targeted weapon rows and four Wizard/Sorcerer learning rows. Provenance validators
  passed all five capture runs. P/M/B, NO, KA and the spear's native EB are clear.
  WK is complete at 1280x720, with native Saber_Dist32 size 32 and no overflow or
  truncation; the reported clipping was not reproduced at this scale, so no lettering
  correction was made. All four learning targets are clear and centered. The
  [viewport review](../artifacts/icon-overhaul-v2/NATIVE-VIEWPORT-REVIEW.html) links originals.
- Inherited native evidence: 116 unmodified framebuffer captures across Undine,
  mercenary and spellbook runs, with validated run/DLL/hash/frame/overlay provenance.
  Inspected heritage, Rapid Reload and strategic descriptions show their intended
  art. The local [native review](../artifacts/icon-overhaul-v2/NATIVE-REVIEW.html)
  links originals. Some creator frames retain an unrelated hover panel and their
  central doll is empty; neither is claimed as full UI/appearance qualification.
  Active-in-hierarchy row metadata alone does not prove viewport visibility.
- Current package SHA-256:
  `f10fbb0db787bbf23d3ef23f825e5d98ca661db95155ff3fcb646413fcd500e6`.
  DLL: `b5ebd760f91f8e8657705b3403fbdf107ab5e3ad539f65ce1c106c9ba39b8df7`;
  MVID: `d6adc4c1-20ed-4435-baab-e863f39d1fa6`.
  Immutable copies are retained under ignored `artifacts/icon-overhaul-v2/native-viewport-qualified-128-*`.
- Installation: the game exited normally. All 224 temporary package files, three
  known runtime additions and 136 backup files were audited. At 2026-09-13 14:59:42 UTC,
  independent verification matched **all 136 original 0.0.117 paths and hashes**,
  including original schema-10 settings. The original installation is restored.
- Art is complete as candidates: 89 paintings plus Rapid Reload, individual sources,
  deterministic exports, briefs/prompts and preserved revisions. The
  [production review](../reports/icon-overhaul/PRODUCTION-REVIEW.html) shows all 90;
  [pilot approval](../reports/icon-overhaul/PILOT-APPROVAL.md) binds only ten exact hashes.
  Runtime uses 90 exports and 137 explicit painted assignments through the existing cache.
- Catalog: 284 identities (255 library blueprints, 28 native appearance resources,
  one reserved absence), 107 concepts and fifteen parametrized firearm UI entries.
  Dispositions: 91 original-required, 47 intentional-family-share, 21 native-semantic-reuse,
  56 protected-existing, six native-monogram and 63 hidden-internal. Twelve retained
  obsolete firearm wrappers preserve fallback art; current equivalents use native roots.
  All 117 protected-file checks and native/eastern assignment boundaries remain intact.
- Permanent policy is discoverable through root AGENTS, the
  [guide](../docs/ICON-ART-GUIDE.md), [actual references](../docs/art/ICON-REFERENCE-INDEX.md)
  and [catalog](../assets-source/original-icons/icon-catalog.json). Future examples
  document the policy without adding gameplay. No paid API, install or asset download was used.
- Intake is preserved at `C:/Dev/KingmakerGunslingerLab/incoming-assets/icon-overhaul`.
  Archive SHA-256 `78a44c77861969fee778a574bdee9c3761cef323553a20951c5e9fbbb7765a8f`;
  sixteen members and ten supplied references were verified. Raw native material stays local.
- Remaining: chosen feats and character sheet, full racial feat menus,
  racial action/variant menus and visible buffs,
  strategic destination controls, and scroll inventory/tooltip/merchant views. Then final
  owner review. The exact working save contains no P/M/B parameter GUID in its eight JSON
  members; mission section 8 requires separate authority for a new save/write fixture.
  No such write is authorized or performed. Continue independent native UI work first.
- Next bounded work: qualify the actual selected-fact/character-sheet component.
  Installed native `CharSComponentAbilitySlot.SetFeature(Feature)` reads `Fact.Icon`
  directly, bypassing the already qualified `FeatureUIData` constructor. Inspect
  its below-fold Total rows and apply only an exact P/M/B native-text correction
  if required; preserve facts, parameters, fallbacks and all other rows. See the
  [native workflow](../docs/ICON-NATIVE-UI-EVIDENCE.md).
  Historical failures, earlier artifacts and restoration evidence remain in the journal.

Intermediate reports are checkpoints. Continue the mission until acceptance or an
explicit approval/prerequisite gate; do not claim completion from technical PASS.
