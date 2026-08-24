# Firearm Wwise audio implementation report

Status: automated implementation, strict release package, and guarded runtime
Event-acceptance qualification complete; human auditory qualification pending.
Automated `PostEvent` acceptance is not proof of audible output.

Implemented source-complete architecture now includes strict validation and
staging, one-time readiness/load state, Wwise posting and diagnostics, global
and selected-unit preview controls, all six required discharge routes, Unity
playback removal, source-only/release package gates, deterministic WAV staging,
an authoring project, authentic embedded-media bank, and a save-free guarded
runtime scenario. Two consecutive fresh-launch scenario passes and all six
mechanical discharge-path scenarios are recorded below. Listening remains.

Wwise 2016.2.6.6153 is verified at
`C:\Audiokinetic\Wwise_2016.2.6.6153`. The repository now contains the curated
Owlcat.Templates 1.14.4 Kingmaker authoring project, including the exact Master
Mixer and native `WEAPONS` bus identity. Its template Work Units are
the preserved Kingmaker seed identities. Wwise generated the new sound, event,
and bank GUIDs. `SoundbanksInfo.xml` and `KMG_Firearms.txt` independently show
the five canonical events and five in-memory media files, with no streamed or
external `.wem` media.

Current authentic bank after deterministic Blunderbuss repair: 999,390 bytes;
SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Production manifest SHA-256:
`DAEC8B174E3586ED20DD31C4146C651AEDFB79E76F74EB3FAEC4687F870935A9`.
Strict release package SHA-256:
`CA1A41CD2A787B45D967C3464097DB78141208A75A9AE3BD121EA234C5A121D4`.
Release DLL SHA-256:
`B549CC7605123443F60DE8A131E6026244FD96F22F1561EFB81C43E6CFFEDED6`.

Previous source-complete commit: `3cbfe4a`; documentation checkpoint:
`e34c1a0`. Both are present on `origin/codex/firearm-wwise-audio`. Mod version
remains `0.0.70`; no release version bump has been made because human auditory
qualification is still pending.

Owlcat authoring-project checkpoint:
`4b8cf93afa8d2797815eb26df4325936229abe68`, successfully pushed and verified
through the repository-authorized policy script.

## Guarded runtime qualification

The final runtime-qualified build at commit
`147c412485215cccb9da655b85293d7e7d24c3cc` has local-runtime package SHA-256
`24E2AC4CBF468B19143C361C220AD1EE90F343DE4E85AA2D2A4BB593E5973AC1`, DLL
SHA-256 `8BC7C6264629715050D1D57B87E9EEB5E2AB73D22C3BB1981A35C8DE1F6158F4`,
and strict release-package SHA-256
`CA93B06093DDCA57A4811562CB3AB6E2FC23E69854500E647A20A6D816C534E3`.
Strict validation requires exactly one firearm bank plus its manifest, no
`Init.bnk`, no other bank, no `.wem`, and no authoring/cache content.

The save-free scenario passed twice on consecutive fresh Steam launches. The
second run ID was
`20260806T2235554614335Z-d28d626bce5449f398c1adc4145168a4` (result SHA-256
`AE112B7524E48153EB292D1ACABFF715A7B8E68CB13A1B634A7213AB724A47E4`).
It recorded state `Ready`, one bank-load attempt, exact expected/observed bank
hashes, global pistol Event acceptance on `Canvas` with playing ID 2,
live-unit acceptance on `Human_Fighter_Baron(Clone)` with playing ID 3, and an
ordinary committed pistol discharge with playing ID 4. A forced misfire left
attempts and accepted posts unchanged at three.

The same commit passed guarded Scatter Shot, Dead Shot, Startling Shot,
Menacing Shot, Stop Bleeding, and firearm presentation/fallback scenarios.
Those deed scenarios qualify their mechanical commit/rollback behavior;
exact once/zero audio routing for every path is enforced by the 898-test
domain/reflection suite. They do not claim a valid playing ID where a detached
fixture lacked a usable live view.

Native crossbow combat-sound suppression remains deliberately unimplemented.
Exact local inspection shows the sound getters fall back through
`WeaponVisualParameters.Prototype` when local fields are empty. Blindly
clearing `m_SoundType`, `m_WhooshSound`, or `m_MissSoundType` would therefore
not prove suppression and could risk the qualified presentation chain. The
fallback scenario confirmed all five models, projectiles, icons, and native
presentation fallbacks. Layered crossbow sound is a human listening question.

## Auditory-polish repair checkpoint

Human listening confirmed Pistol and Musket and exposed three focused defects.
The approved processed Blunderbuss parent remains unchanged. Preparation now
drops exactly 2.180 seconds (104,640 frames at 48 kHz), producing a 174,764-byte
derivative with SHA-256
`F3F1E94701C86D946679DAD5F1AE4577553D0DED23404D356E9ADC71ED9488E3`.
Wwise 2016.2.6.6153 regenerated the bank without changing object, Event,
SoundBank, or native `WEAPONS` routing identities.

Firearm presentation now materializes protected resolved values into its owned
instance, severs Prototype, and clears only the release/whoosh string. Scatter
retains native cone geometry but uses the firearm projectile instead of Burning
Hands projectile media. The guarded Wwise scenario now uses a live Blunderbuss
for selected preview, ordinary success, and forced misfire.

Pre-runtime qualification passed: focused tests, authored-project and bank
validation, repository validation, 898/898 domain/reflection tests,
exact-reference Release build, build-output validation, strict release package
and validation, and `git diff --check`. Pre-commit strict package SHA-256:
`4DB98D3E7F36126C58C0F772B769BF459687912F6C9FF823808CAC0A3C9E4435`;
manifest SHA-256:
`20908FBB97AE465075B53491D5C7103E5C5520B5A481CDCC0CB2B8399A61F517`.

Post-checkpoint guarded qualification on commit `896ec38b1af5142967348f11935cca86bd36f2f7`
passed twice on consecutive fresh launches. The final run ID was
`20260807T0210021249437Z-259c1bd4abac47fbb5aa138846c1a0c6` (result SHA-256
`81DE5F3DE21F47B98C98BB315A322AAC23EC353E338DFFD5AD85F1501FB47ED9`).
It observed `Ready`, exact new bank hashes, one load attempt, global Pistol ID
2, live-unit Blunderbuss preview ID 3, ordinary Blunderbuss ID 4, and no post
increment for the forced Blunderbuss misfire. Scatter transaction and complete
firearm presentation/fallback scenarios also passed on the same commit.

Final local-runtime package SHA-256:
`66FCA06C862A41FC0E3E42A8ECC3C9DBBE605EBEC1CAC4E92B72180FE9D7FBE5`;
DLL SHA-256:
`649E5E7DFA739E610E28D0BA2B2124BB8EA831B30FC451CCEBF391EF8944B9BA`;
last strict release-package SHA-256:
`94AFF3D386BDE6111EA06E8340C751DE3C7CA9EFDDB617D9F10398C982F0B1B6`.
Mechanical/runtime work is complete. Fresh human listening must decide timing,
release-twang suppression, Scatter spell-audio removal, and Scatter visuals.

## 2026-08-20 integrated regression requalification

Fresh playtest evidence reports Pistol and possibly other firearm reports absent, so the earlier auditory result is stale until repeated. The integrated audit found the production bank unchanged at SHA-256 `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`, with five unique nonempty embedded media entries and nonzero PCM in all five approved source recordings. Staging now validates the exact binary media contract before copying the allow-listed bank.

Commit `d8fd4ad1836f3ad4d9b54dec908d7818725c64d1` expanded the guarded preview from Pistol-only to all five exact firearm Event families. Fresh-process run `20260820T0635323959656Z-88cfa04a0deb4595bfbc2ee8d4284e31` passed 6/6, accepting Pistol/Musket/Blunderbuss/Revolver/Rifle as playing IDs 2-6, live Blunderbuss preview as ID 7, ordinary committed Blunderbuss as ID 8, and no additional normal post for forced misfire.

This excludes the currently representable bank/package/load/routing failures; it does not prove audio reached the listener. No bank or source-audio bytes were changed without evidence. Fresh five-family auditory acceptance remains required on the final integrated candidate.

## 2026-08-24 manifest-boundary restoration

The 0.0.95 full-stack warning was reproduced without changing the canonical
manifest or bank. Source, Release staging, standalone package, and live manifest
copies were byte-identical: 610-byte UTF-8 without BOM, SHA-256
`BF57981AD5EC2CBF3149ECAFC3EF737D87BC9035B14BCCC7D254DCA8F991C62E`,
integer `schemaVersion` 1, and the exact five-Event catalog. Source, package,
live, and game-staged `KMG_Firearms.bnk` copies were also byte-identical at
999,390 bytes and SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

The old production `JsonConvert.DeserializeObject` path parsed correctly under
neutral defaults, but a deliberately hostile valid
`JsonConvert.DefaultSettings` contract resolver caused the exact installed
manifest to deserialize as `SchemaVersion=0` with defaulted members and then
throw `Unsupported manifest schema.` This proves the defect was the loader's
dependence on process-global Json.NET state. Available retained logs do not
identify which loaded component set that state, so no third-party attribution
is claimed.

The production loader now uses its own exact `JsonTextReader` boundary. It binds
only canonical case-sensitive names, verifies token types, rejects unknown or
duplicate properties, requires every field, and retains the exact schema,
identity, platform, Wwise, hash, embedded-media, and Event-catalog validators.
Failures identify JSON parsing, schema extraction, semantic validation, bank
validation, staging, loading, or PostEvent rejection. Startup emits exactly one
concise `manifest.read`, `manifest.validated`, `bank.staged`, and `bank.ready`
record on the successful path.

Commit `ea51bd3732fd7313e92bcc2edac9560008f6c9ac` passed repository
validation, all 1,224 dependency-free tests, Wwise authored-object validation,
deterministic audio-source/polish validation, production SoundBank validation,
clean Release build, build-output validation, strict packaging, production C#
artifact parsing, transactional deployment, and `git diff --check`. Its
runtime package SHA-256 was
`9D2569E4F5F2238947DC1B6F63F171FA95AF791B59C651564458D9A79FD59325`;
DLL SHA-256 was
`6028FE4B4D20688D0C7E8E44C1AB4EF9B024DA7F385A46C54278EBAF95A10A40`.

Guarded Steam run
`20260824T0444147375477Z-849bb57337ad44538df0c8582d5038f7`
passed every focused assertion. State reached `Ready`; configuration, staging,
and load attempts were each exactly one. Global Pistol, Musket, Blunderbuss,
Revolver, and Rifle previews returned playing IDs 2, 3, 4, 5, and 6. The live
Blunderbuss unit emitter returned 7; an ordinary committed shot returned 8;
and a deterministic native miss returned 10. Misfire, Empty, Wrecked,
canceled, and native-crossbow paths did not increment the accepted custom-post
count. Scatter posted once per committed volley, and the existing Dead Shot,
Startling Shot, Menacing Shot, and Stop Bleeding discharge contracts retained
their exact attempt counts.

No SoundBank or source-audio bytes changed. A nonzero playing ID proves Wwise
Event acceptance, not speaker output. The focused twelve-step human listening
pass remains mandatory.
