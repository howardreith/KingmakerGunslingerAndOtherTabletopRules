# Icon overhaul v2: pilot checkpoint

Status: **AWAITING_PILOT_APPROVAL**. The plan is approved; the ten new images are
not. This is a technically qualified presentation prototype and an isolated art
pilot, not the completed overhaul or a release.

## Review packet

- [Interactive pilot review](PILOT-REVIEW.html): enlarged originals, adjustable
  32/48/64/128px export previews, parchment/dark backgrounds, grayscale and hashes.
- [Contact sheet](pilot-contact-sheet.png): ten candidates at multiple sizes.
- [Source/export manifest](../../assets-source/original-icons/icon-overhaul-v2/pilot/pilot-manifest.json):
  exact originals, exports, dimensions, hashes and explicitly unapproved status.
- [Production prompts](../../assets-source/original-icons/icon-overhaul-v2/pilot/production-prompts.json):
  prompts, reference roles and preserved revision inputs. The first Ifrit prompt
  is honestly labeled a reconstructed brief, not a verbatim transcript.
- [Curated qualification](PILOT-QUALIFICATION.json): artifact identity, exact run
  IDs, assertion counts, restored installation and explicit evidence limits.

The requested decisions concern the three-role heritage/affinity/resistance
family, multi-element and hydraulic action language, the travel/recall family,
and the Rapid Reload loading emblem. Approval of these images establishes
direction for production; it does not approve unseen family members or unresolved
native UI surfaces.

The P/M/B prototype uses the real native text route in production UI-entry
objects. Its actual native-screen comparison is still pending. The review page
does not substitute a PC font or the retained blueprint PNGs for that evidence.

## Intake and protected baseline

Unpacked the supplied v2 ZIP without overwriting the historical staging directory.
All sixteen SHA256SUMS members matched. ZIP SHA-256:
`78a44c77861969fee778a574bdee9c3761cef323553a20951c5e9fbbb7765a8f`.
Only the accepted mission and plan were copied into
[planning/icon-overhaul](../../planning/icon-overhaul/Kingmaker_Icon_Overhaul_Codex_Mission.md).
Native screenshots remain local/reference-only.

Started from clean master at
`db1fccf648b25baa53667dd00ac566e2a0d7f6b9`, version 0.0.127. Relevant icon
source was unchanged from the pack's planning audit
`258decb8fc2dad58d7a096772b34ec4202d71bd5`.
The requested branch already existed at historical `7e77a970`; it was preserved.
This work uses `codex/icon-art-overhaul-v2`.

The canonical catalog protects 117 existing runtime images/bundles and the exact
ProjectAssetIcons, custom weapon selector and Shield Other assignment source.
Existing legacy manifests continue to own their unchanged export records. All
current runtime PNGs, accepted actions/items, native/eastern controls, bundles,
audio, appearances, GUIDs and serialized parameters remain unchanged.

## Permanent authoring standard and inventory

Root AGENTS and README now lead to:

- [Icon art guide](../../docs/ICON-ART-GUIDE.md).
- [Actual reference index](../../docs/art/ICON-REFERENCE-INDEX.md), including
  eleven inspected project files and ten indexed local native screenshots.
- [Canonical catalog](../../assets-source/original-icons/icon-catalog.json).

The catalog contains 284 exact registered blueprint consumers: 247 elemental and
strategic records, plus 37 firearm records. Its dispositions are 109
original-required, 47 intentional-family-share, 21 native-semantic-reuse,
44 protected-existing and 63 hidden-internal. A further fifteen exact UI entries
cover five native firearm feat families times three parameters. The existing
summoning manifest retains authority over its 77 art identities and placements.

There are 93 original/family identities requiring final resolution, including
three firearm fallback identities that may ultimately use native presentation.
Ten have isolated pilot candidates. **No final new raster identity is integrated
or visually approved.** Counts are source inventory, not a completed live census.

The inventory explicitly covers all 21 published alternate traits; ten selectors
and ten retain choices; held-touch delivery; hydraulic and wind children; Nereid
Shake Free, assistance and aura; visible buffs; and independent scroll-item
icons. Native spell reuse is distinguished from ancestry/trait donor defects.
Non-icon visual-resource records remain protected.

The bounded audit recorded Conjuration-donor use for Acadamae; Combat Reflexes
sharing for Bodyguard/In Harm's Way/Helpful; native donor use for Brown-Fur;
Fast Movement on Urban Barbarian Crowd Control; and existing original summoning
and Shield Other art. These are keep/review findings, not expanded art scope.

A read-only discovery traversal starting with root AGENTS reached the guide,
actual reference files and catalog for Magic Circle, Lunge and a new summon.
It selected original group-ward painting, an original mundane reach emblem,
and species-specific creature painting respectively. No other agent/service
was used and none of those gameplay examples was implemented.

## Pilot production and self-review

The available built-in image tool produced nine individual paintings. Sources
are preserved byte-for-byte, with the two superseded edit inputs retained.
Native images informed stylistic inspection; no extracted game pixels or fonts
were incorporated into the new assets. Sibling edits used project-original
generated images. There was no paid API fallback, credential access, installation
or third-party download.

Fire Affinity was revised because its hand disappeared at small size. Greater
Teleport was revised to remove four gold brackets that looked like UI selection
decoration. Rapid Reload is an original measured flat emblem; its initial
vertical-bore composition was replaced by a diagonal stock/barrel, ball and
ramrod. Loading clarity is a specific owner-review question. Elemental Strike's
stone element and foreground padding also merit native-size review.

Exports are nine 128x128 RGBA paintings and one 64x64 RGBA emblem. Repeat export
produced the identical manifest hash
`6061ddf726b3d318d046ee8b9b404af1339f1b8d6abcdec0d6613522d1f925ce`.
The script uses recorded GDI+ sampling and does not call a model during export.
All candidates remain outside runtime asset/package directories.

The retired firearm generator's All/Feat modes now fail with a guide-directed
explanation; its wrapper exports isolated pilot candidates. Item generation and
protected item pixels were not run or rewritten.

## Native firearm prototype

The only production behavior delta is at NativeFirearmFeatIntegration.Append:
the explicit FeatureUIData constructor receives null Icon and the single-letter
B/M/P acronym. The FeatureParam still points to the same owned blueprint.
Eligibility, ordering, categories, prerequisites and mechanics are unchanged.

Read-only installed-assembly inspection established that the explicit constructor
retains null, and CharBuildSelectorItem.SetIcon displays the native TMP acronym,
background and name-derived color. Selected-feature construction independently
reads the parameter blueprint sprite. Those historical non-null sprites remain
in place until each selected/character-sheet surface has a qualified replacement.

The catalog reconstructs the pre-change whole source file by reversing exactly
this constructor delta and verifies its normalized SHA-256. An old Round 2
whole-file hash was delegated to this stronger scoped check; every other
historical protected check remains. A negative fixture proves that even changing
sort direction outside the presentation site is rejected.

The runtime facsimile scenario now labels blueprint fallback sprites explicitly
and separately records the native acronym route. It never presents those PNGs
as proof of actual native typography.

## Qualification

The repository's Build-Local orchestrator passed:

- Repository validation, including existing icon gates and the new catalog gate.
- Nine focused corruption/consistency tests.
- Complete clean Release domain suite: **1,612 tests, zero failures**.
- Exact private-reference Release compilation and focused supply-icon checks.
- Build-output and strict standalone installable-package validation.

Two preliminary attempts stopped during validation: a Python import cache was
removed and bytecode generation disabled in the test runner; the historical
whole-file lock then identified the authorized constructor change and was
replaced by the exact-delta guard described above. Neither failure was ignored.

Tested version: 0.0.127.
Package SHA-256:
`ca964c53bf983f9afeb822f62e2c6dff67f101ccd33e3a46a6c1007f420f267c`.
DLL SHA-256:
`fb81a01e3ff56289f6e86db6a0a61fc66e5cbcb6b5ca06fbb767c5cf02ac4fc9`.
DLL MVID: `e35ff147-5fb4-4aa4-9d11-9612bf1a2f3c`.
Working-tree fingerprint at build:
`b1f9a1ed73aa10a962a988363159a9ddffe5ed70b462b78489774e9effa234aa`.
Post-qualification documentation/status/provenance changes do not claim that
old full-tree fingerprint. The tested C# files have not changed; their hashes
are in the curated record.

All four fresh game launches used the guarded request through Steam App ID
640820, the same immutable deployment, explicit version and automatic clean exit:

| Scenario | Run ID | Result |
|---|---|---|
| icon-overhaul-visual-evidence, all ON | 20260912T1803219775894Z-18aee0ff89d54749aaef3ad6d5469a99 | PASS, 9/9 |
| disposable-firearm-dependent-feats, all ON | 20260912T1806469792393Z-7ce4e8341bca47088d82486cb6c76c4a | PASS, 12/12 |
| observe-feature-module-settings, Gunslinger OFF / others ON | 20260912T1809242356406Z-de6d4240173748f89d3ac9f82342b334 | PASS, 34/34 |
| working-save-smoke, exact KMG_AUTOMATION_WORKING | 20260912T1812393075254Z-d668e794c5f44b79b280344beccd2111 | PASS, 11/11 |

The focused boundary is appropriate to a presentation-only constructor change:
the full-file delta check proves that selector publication/gating and mechanics
did not change. No new module mechanics or publication path was introduced,
so the generic 26-state mechanics/publication matrix was not rerun. The actual
unit-aware menus, parameter commit, feat effects/isolation and disabled
publication were exercised. No optional external-mod configuration was changed.

The working-save run verified exact descriptor/load correlation, completion
fingerprint and no save-writing API. It does not separately establish a saved
firearm parameter round trip. No new saves, campaign actions or UI navigation
by coordinates/OCR were used. The eleven legacy image renders are local
facsimiles; zero new native-menu screenshots have been claimed.

## Restoration and publication

The original 0.0.117 installation was restored through the exact backup/restore
scripts. Final verification matched **every path and SHA-256, 136/136 files**,
including original FeatureModules bytes:
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
Kingmaker was no longer running. New art was never installed.

The extra-file pre-restoration audit rejected FeatureModules.json.previous
before completing its backup check; restoration nevertheless ran in the same
orchestration cell. Subsequent source inspection confirmed that
FeatureModuleSettingsStore creates that file via File.Replace. Its temporary
contents were not independently hashed before restoration. The complete
post-restoration hash comparison is the evidence for the final installation;
the earlier extra-file check is not represented as PASS.

The exact mandated push wrapper refused the new branch because it is absent
from its allowlist. No policy edit, alternate push, history rewrite, merge, tag
or release was attempted. The owner has been asked to add
`codex/icon-art-overhaul-v2` to the wrapper's AllowedBranches.

## Remaining gates and exact next work

1. Obtain an explicit owner decision against the ten pilot hashes. Do not begin
   mass generation on silence or plan approval.
2. Arrange a bounded supervised native-screen session: new/disposable character
   firearm feat rows beside native/Katana/Nodachi controls, nested Rapid Reload,
   selected feat/character sheet, Wakizashi and Elven Branched Spear. A human
   navigates unsupported screens; the permitted PID-bound capture helper records
   images. No campaign save or write is needed for menu inspection.
3. Complete the guarded live elemental/strategic census and exact donor/protected
   assignment snapshots before final mappings. Resolve remaining native
   selected-feature and save-parameter-specific coverage through an authorized
   fixture; do not claim the generic smoke supplies that evidence.
4. After pilot approval, produce coherent small families from the 93 catalog
   identities, preserving exact native exceptions and distinct selectable
   actions. Resolve the three firearm fallback identities through qualified
   native routes or an explicitly approved isolated fallback.
5. Integrate exact owned mappings, including independent scroll icons; qualify
   final exports/package, live bindings and actual native UI; restore the
   installation; obtain separate final visual approval.
6. Publish the checkpoint using only the mandated wrapper once the branch is
   allowlisted. No merge or public release is authorized.
