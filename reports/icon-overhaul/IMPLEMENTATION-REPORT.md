# Icon overhaul v2 implementation record

The branch includes master `e5f1426a` (0.0.129), preserving published history.
The latest [native buff qualification](NATIVE-RACIAL-BUFF-QUALIFICATION.json)
passed **five guarded Steam runs, 63 assertions and 393 native captures**.
All thirteen buff identities were inspected in native character sheets; twelve
disposable mercenaries completed their regressions. Exact icons, full titles,
native timers/control, inactive buff state and original context were retained.
Read-only capture avoids scrolling already-visible rows. All cleanup and
same-artifact working-save smoke pass. No save writes occurred.

Repository validation, 23 catalog, 25 capture, six paired, eleven request and all
1,639 domain tests, clean Release and the strict 224-file package passed. All 136
original installation files/settings were restored at `2026-09-14T00:17:14.376986+00:00`.
Package `30030682eda80a3a41f65dc0a0c42993e0eb3f5dfd45ec034088eeaeebcdd994`; DLL `804f0b30ab8e2dd417e1c6fd267c9bb23c8efa675889e87b3bff198eff2906f3`;
MVID `ff0b4a9c-7daa-4a94-8f82-f9ca3bb47693`; source `7e15cd13cbcf0917c9eaefab78a90d52e89467708c1b6fa93c3eac1572a0eb62`
based on `a20b0d236a1259f4c92521ab3a7780956095631e`. Post-run documentation curation changes no compiled code or pixels.

The v0.0.129 integration separately passed 208 assertions / 30 native captures,
including Oracle Recall learning and updated scroll behavior. Earlier exact
artifacts qualify strategic text controls, racial feats, scroll
inventory/descriptions/merchant, native Total/sheet P/M/B, eastern controls,
creator/heritage/alternate choices and strategic spell learning/books. Individual
reports retain exact hashes and evidence limits; they are not new captures on
this DLL. See [mission state](../../planning/ICON-OVERHAUL-STATE.md) and the
[native UI guide](../../docs/ICON-NATIVE-UI-EVIDENCE.md).

All 89 paintings plus Rapid Reload have preserved sources/exports: 90 runtime
exports, 137 painted assignments and 284 consumer dispositions. Ten pilot images
and family direction are approved. Eighty production images and final native UI
await owner approval. Protected artwork and blueprint-based firearm parameters
remain preserved.

Remaining: supported racial actions/variants and applicable higher dependent
feat rendering. The working save lacks supported firearm parameters; new save
creation/writes require separate authorization. Inactive buff presentation does
not qualify active action bars or combat effects. Unrelated fixture header/doll
and repeated native Weapon Focus label findings remain recorded. This is a
technical checkpoint; no feature-to-master merge or release is claimed.

## Review packet

- [Full production review](PRODUCTION-REVIEW.html): all 90 art identities, including
  the ten approved pilot images and 80 production candidates.
- [Production manifest](../../assets-source/original-icons/icon-overhaul-v2/production/production-manifest.json):
  individual sources/exports, exact hashes, and complete implementation-informed
  briefs. Three revised production drafts remain preserved with their prompts.
- [Interactive pilot review](PILOT-REVIEW.html): enlarged originals, adjustable
  32/48/64/128px export previews, parchment/dark backgrounds, grayscale and hashes.
- [Contact sheet](pilot-contact-sheet.png): ten candidates at multiple sizes.
- [Source/export manifest](../../assets-source/original-icons/icon-overhaul-v2/pilot/pilot-manifest.json):
  exact originals, exports, dimensions, hashes and the recorded pilot approval.
- [Production prompts](../../assets-source/original-icons/icon-overhaul-v2/pilot/production-prompts.json):
  prompts, reference roles and preserved revision inputs. The first Ifrit prompt
  is honestly labeled a reconstructed brief, not a verbatim transcript.
- [Curated qualification](PILOT-QUALIFICATION.json): artifact identity, exact run
  IDs, assertion counts, restored installation and explicit evidence limits.

The recorded pilot decision establishes the heritage/affinity/resistance,
elemental/hydraulic, strategic travel and mundane emblem family direction. It does
not approve the additional 80 images or unresolved native UI surfaces.

P/M/B uses the real native text route in production UI-entry objects. Rapid Reload
rendering passed; the remaining native parameter/chosen/sheet comparison is pending. The review page
does not substitute a PC font or the retained blueprint PNGs for that evidence.
Weapon Focus P/M/B viewport inspection has since passed. The actual native
`CharSComponentAbilitySlot.SetFeature(Feature)` reads Fact.Icon independently of
the data constructor; its below-fold selected facts and character-sheet rendering
are the next bounded qualification target.

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
protected installed PNGs, accepted actions/items, native/eastern controls,
bundles, audio, appearances, GUIDs and serialized parameters remain unchanged.
The integration adds 89 runtime exports; their catalog entries retain baseline
donors separately from the current explicit owned mappings.

## Permanent authoring standard and inventory

Root AGENTS and README now lead to:

- [Icon art guide](../../docs/ICON-ART-GUIDE.md).
- [Actual reference index](../../docs/art/ICON-REFERENCE-INDEX.md), including
  eleven inspected project files and ten indexed local native screenshots.
- [Canonical catalog](../../assets-source/original-icons/icon-catalog.json).

The catalog contains 284 exact identities: 247 elemental/strategic and 37 firearm
records. Live registration comprises 255 blueprints, 28 separate appearance
resources and one reserved diagnostic absence. Its current dispositions are 91
original-required, 47 intentional-family-share, 21 native-semantic-reuse,
56 protected-existing, six native-monogram and 63 hidden-internal. A further fifteen exact UI entries
cover five native firearm feat families times three parameters. The existing
summoning manifest retains authority over its 77 art identities and placements.

There are 93 original/family identities requiring final resolution, including
three firearm fallback identities that may ultimately use native presentation.
Ninety have preserved originals and exports; the three firearm lettering
identities use a native presentation prototype. **Ten pilot images are approved;
137 painted consumers are mapped and passed the paired live census.**
Native UI and final visual acceptance remain separate.

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

## Historical pilot production and self-review

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
At this historical pilot checkpoint, all candidates remained outside runtime
asset/package directories. The later painted integration is qualified above.

The retired firearm generator's All/Feat modes now fail with a guide-directed
explanation; its wrapper exports isolated pilot candidates. Item generation and
protected item pixels were not run or rewritten.

## Historical native firearm prototype

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

## Historical pilot qualification

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

The wrapper initially refused the new branch. The owner subsequently added
`codex/icon-art-overhaul-v2` to its allowlist. The unchanged mandated wrapper
successfully published pilot commit `4f141921a830042442b617f2788f1cd0b34a7cb6`
and production-foundations commit `4914a48409175610b8ecde73ff388d36a1101edd`.
No policy edit, alternate push, history rewrite, merge, tag or release occurred.

## Complete production art checkpoint

The 80 production paintings complete the scoped heritage, resistance, affinity,
racial feat, alternate trait, support-action and selector art. Shared retain
choices and feature/action/buff identities remain explicit catalog relationships.
Independent hydraulic, wind and held-weapon children have distinct compositions.
Native spell exceptions and protected art remain unchanged.

Reviewed every family at 32/48/64px and in grayscale using twelve labeled contact
sheets. Metal Affinity was revised to remove fire, Inner Flame to retain the full
weapon silhouette, and Breeze-Kissed to show a straight arrow redirected by wind.
The exact prior originals and prompts remain in production/revisions. Chill Touch
uses negative-energy imagery; Triton Portal shows summoned water elementals;
selectors communicate a static choice category. No mechanics were invented.

All 80 briefs record implemented behavior, actual UI surfaces, focal subjects,
confusable identities, forbidden interpretations, references and generation
provenance. An initial Acid Breath request produced no image after a tool safety
rejection; the exact failure and successful benign symbolic retry are recorded
in its brief. No fallback service or credentials were used.

The canonical validator now rejects incomplete production coverage or missing
creative-brief surfaces. Sixteen focused tests cover those failures alongside
protected files, identities, approval hashes and exact constructor scope.
The gallery is generated from verified manifests and does not generate pixels.
The complete checkpoint passed `Build-Local.ps1` with exit 0: repository
validation, 16 focused tests, all 1,612 domain tests, clean Release compilation,
build-output validation and strict installable-package validation. Repeat export
left all 187 checked PNG/manifest files unchanged. Gallery links, 90 unique
records, approval counts and JavaScript syntax also passed. See the
[exact checkpoint evidence](PRODUCTION-QUALIFICATION.json). No game deployment or
runtime claim applies to this isolated art checkpoint; all runtime PNGs remain
unchanged. Post-build documentation records the result and does not claim the
earlier full-tree build fingerprint.

## Earlier native qualification and remaining gates

The 0.0.128 integration, viewport, selected-fact, scroll inventory and merchant
reports record separate exact artifacts and successful guarded checks. The
latest [racial feat qualification](NATIVE-RACIAL-FEAT-QUALIFICATION.json) adds
four real creator cases plus same-artifact smoke: 59 assertions and fourteen
inspected target captures covering all eleven racial feats. Native sprites,
titles, eligibility and controls were exact; filter/scroll/selection/facts and
the original installation were restored. The creators reached final review and
were canceled; no committed character or new save. See the durable
[mission state](../../planning/ICON-OVERHAUL-STATE.md) for exact hashes, counts
and earlier qualified firearm/creator/learning/protected evidence.

The latest strategic text-control qualification adds six inspected actual source
buttons and same-artifact smoke (53 assertions); full cleanup and restoration
passed. Remaining work is racial action/variant/buff views and applicable higher
dependent feat rendering. The existing
working save lacks saved firearm parameters; any new save/write fixture requires
separate authorization. All eighty later paintings and final native UI still
need owner approval. The ten pilot images and family direction are approved.
Publish qualified checkpoints through the mandated wrapper; no feature-to-master
merge or release is authorized.
