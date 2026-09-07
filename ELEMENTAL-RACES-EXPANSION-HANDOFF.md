# Elemental Races expansion: manual-test handoff

## Outcome

The owner requested that this segment conclude for local manual testing.
Autonomous implementation is stopped. The tested `0.0.117-elemental-traits`
candidate is installed and enabled in Unity Mod Manager on this machine.
All 135 packaged files independently match the immutable runtime-tested ZIP.
Existing FeatureModules.json bytes are preserved; `elemental-races` is enabled.
No game was launched and no save was accessed by the handoff installation.

This is a development test build, not a completed Release C or production-save
recommendation. Foundation and Releases A/B retain their qualified local
checkpoints. Release C has nineteen traits with incremental native proof and
ten with save-backed proof, but its final gates are incomplete.

The latest public master Brown-Fur/Buff Planner compatibility fix is included.
Use the currently installed 117 candidate, not either historical 115 ZIP.
Direct Buff Planner Instant consumer gameplay still needs the owner's test.

## What is not implemented

| Required trait | Missing work |
| --- | --- |
| Oread: Treacherous Earth | Ground/material eligibility and a faithful native difficult-terrain area, duration and cleanup. The audited sound-surface grid alone cannot establish valid earth/unworked stone/sand at the actual target height. |
| Undine: Nereid Fascination | Native humanoid fascination aura with exact saves/duration and condition/threat behavior. The Bard donor is not yet qualified as a faithful implementation of this trait. |

These two choices currently exist as selection/provider scaffolding.
**Do not select them for gameplay:** they can consume the racial SLA
replacement slot without delivering the advertised benefit. Retain the base
trait or choose a working alternative. No compensatory bonus was invented.

The nineteen-trait persistence extension was inspected only before the
owner changed the stopping instruction. No partial source implementation or
fixture from that extension is included in this build.

## Implemented, but not fully qualified

Nine implemented traits still need their fresh-process save/OFF/ON,
rest/level/respec and cleanup qualification:

- Ifrit: Wildfire Heart, Brazen Flame, Forge-Hardened.
- Oread: Granite Skin.
- Sylph: Breeze-Kissed, Like the Wind, Secretive, Thunderous Resilience,
  Whispering Wind.

The two unimplemented traits will also need those tests, for eleven remaining
traits in the final persistence matrix. Existing ten-trait save proof covers
Fire/Earth/Air Insight; Fire/Stone/Storm in the Blood; Efreeti Magic;
Crystalline Form; Acid Breath; and Ooze Breath.

Other open gates:

- Breeze-Kissed's broader attack-source boundary: the tested core covers
  physical nonmagical ranged weapons, masterwork and temporary enhancement.
  Ability-sourced and nonphysical attacks currently fail closed; ordinary
  extraordinary weapon-ability/alchemical cases need native controls.
- Native turn-based action costs for Breeze-Kissed and both Undine breaths.
- Complete Crystalline Form semantic ray catalog and full trait lifecycle.
- The complete all-trait death/resurrection, polymorph/return, equipment/body
  and fresh-process persistence matrix, with exact spent-resource preservation.
- Final Release C six-profile/module-ON/OFF regression and complete package
  qualification. The isolated combined profile has save-free evidence, not a
  successful working-fixture load; the restored installed configuration did
  load the same fixture in earlier guarded transactions.
- Subjective appearance review. Visual Adjustments is absent/NOT-RUN, not PASS.
- Direct Buff Planner Instant consumer gameplay; native provider/command
  integration checks are not a substitute for that user-facing workflow.

## Available content to test

Heritages remain three choices on each existing parent race, not new races:

| Race | Heritage choices | Implemented alternate traits |
| --- | --- | --- |
| Ifrit | General, Lavasoul, Sunsoul | Wildfire Heart, Brazen Flame, Fire in the Blood, Efreeti Magic, Forge-Hardened, Fire Insight |
| Oread | General, Gemsoul, Ironsoul | Crystalline Form, Earth Insight, Granite Skin, Stone in the Blood |
| Sylph | General, Smokesoul, Stormsoul | Air Insight, Breeze-Kissed, Like the Wind, Secretive, Storm in the Blood, Thunderous Resilience, Whispering Wind |
| Undine | General, Mistsoul, Rimesoul | Acid Breath, Ooze Breath |

Implemented feats: Elemental Strike; Scorching Weapons; Inner Flame;
Blazing Aura; Firesight; Airy Step; Wings of Air; Cloud Gazer; Inner Breath;
Hydraulic Maneuver; Triton Portal. Their historical Release B gate passes;
this does not promote the unfinished Release C to PASS.

Replacement choices consume their printed resistance, affinity and/or SLA
slots. Overlapping choices are excluded; retain-base remains valid. Native
race identities and General SLA/resource GUIDs are unchanged.

Start Kingmaker normally through Steam and use a new disposable test character
or a separate disposable save copy, for example `KMG_ELEMENTAL_USER_TEST`.
Keep `KMG_AUTOMATION_BASELINE` untouched. Useful manual checks are creation and
respec presentation, active abilities/tooltips, normal/turn-based action costs,
save/reload/rest, clothing/equipment/appearance, and the Buff Planner workflow.
No further autonomous game launch or normal-save UI automation is planned.

## Intentionally deferred or omitted material

These remain outside this segment's implementation backlog, as originally
requested: Elemental Jaunt, Oread Burrower, Oread Earth Glider, Stony Step,
Echoes of Stone, Murmurs of Earth, Aquatic Ancestry, Steam Caster, Water
Skinned and Blistering Feint. Favored-class bonuses are wholly out of scope.

Environmental swimming, burrowing, planar/underground travel, language,
Disguise/Craft and GM-adjudicated traits remain excluded. Existing explicit
engine omissions include grapple, ordinary falling damage and Dirty Trick
(dazzle); native Dirty Trick (blind) is implemented. Triton Portal offers
Water Elementals only. No darkvision, global creature-type rewrite or custom
heritage meshes were added. See the
[deviation matrix](ELEMENTAL-RACES-DEVIATION-MATRIX.md) for exact printed
omissions and approved SLA, concealment and flight adaptations.

## Qualification and artifact identity

The installed candidate passes complete repository validation, all 1,432
domain/reflection tests, a clean exact-reference Release build and strict
package validation. Two guarded Steam App ID 640820 processes pass 11,376
assertions, including 380 Breeze observations, with exact independent
968-entry mod/settings restorations. Earlier failed/mixed candidates remain
recorded. Native shader/script/lightmap diagnostics and four combined-profile
ZFavoredClass KeyNotFound signatures are retained; no blanket clean-log claim.

| Profile | Run ID | Assertions |
| --- | --- | ---: |
| KMG only | `20260907T0150166123561Z-4c2126abd9594fd5818b03ca95f9189a` | 5688 |
| Highest-risk combined | `20260907T0152582879337Z-716236caed0c4b7d92682033ba4e440a` | 5688 |

ZIP: 23,259,329 bytes, 135 entries;
SHA-256 `acc04aa24697170f00336198d67f0ef4aa98dedc5a7884acf263d0d713de3fe4`.
DLL: 6,264,320 bytes;
SHA-256 `b3839a63fb83a5894169fa7ea2cbc1ef6e15f01229081b51d0f74b0d88984f05`;
MVID `0a84af89-794b-46b5-b55e-13bf9959bcd6`.
Source-state SHA-256:
`955ab659e3cad5f4aa414614c6d65bab89e970b311f87eaedf2655d422f560ed`.

Installed folder:
`C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger`.
Deployment `20260907T0212038949733Z`, SHA-256
`d9248e45be84c8f86bf0dcf432470b3023136bdf7e3a35edd79469e7ec9892b9`.
Preserved settings SHA-256:
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
Recoverable pre-install mod backup:
`C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260907T0212002253044Z`.

The install replaced only the exact KMG mod folder through the backup-first
installer. All other mod folders were outside its write target. No personal
save was overwritten, deleted or converted. UMM's existing enabled entry was
verified, not edited. The game is left stopped for the owner to launch.

Current manifest: 1,867 total / 1,865 active / two reserved. Elemental Races:
229 active identities (201 blueprints and 28 visual proxies), including 53
heritage additions, 25 feat additions and 83 Release C additions. Exact new
Breeze IDs and complete per-run evidence hashes are in the
[native core report](docs/ELEMENTAL-RACES-0.0.117-BREEZE-KISSED-CORE-CHECKPOINT.md)
and `releaseCBreezeKissedNativeQualification` in STATE.

Historical 0.0.114 migration preserves markerless General identity, exact
stats/appearance data and spent SLA uses. The
[ten-trait save report](docs/ELEMENTAL-RACES-0.0.117-BREATH-PERSISTENCE-CHECKPOINT.md)
retains the exact producer/consumer/absence evidence; it does not cover
Breeze or the other newly pending traits. A/B qualification, packages,
adaptations and compatibility records remain in the
[implementation report](ELEMENTAL-RACES-EXPANSION-IMPLEMENTATION-REPORT.md),
[115 release records](docs/RELEASE-NOTES-0.0.115.md) and
[116 qualification](docs/ELEMENTAL-RACES-0.0.116-QUALIFICATION.md).

## Git and continuation

Branch: `codex/elemental-races-expansion`.
Original starting master: `6874dc15a27ded132456dbdd480f47c794543a05`.
Integrated authoritative master: `dfd551080a1aad38cdd0b19714fbcb12c81ca4ca`.
Owner-authorized master-to-feature merge:
`0d9cd38144132a94acac997b82409f84c54d2b94`.
Code checkpoint: `efc9d54ec29dbdd84ffe328183139f517c2f3350`.

The installed runtime artifact embeds the pre-checkpoint parent plus its exact
dirty source fingerprint. Only evidence/documentation changed after those
runtime runs. The final handoff commit is documentation-only, and its exact
SHA/clean push outcome is reported in the closing response.

No feature-to-master merge, tag, GitHub release, PR, generated ZIP, raw runtime
artifact, save or proprietary assembly is published by this handoff.
The retained recovery stash is not applied again or dropped. Wait for owner
testing and explicit follow-up before merging or implementing more material.

Feature-segment commit ledger through the code checkpoint (newest first,
excluding commits already on authoritative master):

- `efc9d54ec29dbdd84ffe328183139f517c2f3350` — Implement and qualify Breeze-Kissed native core
- `0d9cd38144132a94acac997b82409f84c54d2b94` — Merge master 0.0.115 Brown-Fur fix into elemental expansion
- `14dea6215927517a9cb7c91e6553fa5592abe44a` — Qualify breath action costs and ten-trait save persistence
- `cf2426ac092b6bed33ff721fca722be9486f5e89` — Implement and qualify native Undine breath mechanics
- `0007d7c97f11cca70dd682bb2e006059cfd6e0c1` — Preserve elemental feat state in native level-up previews
- `686cd42efeb10d3c979e6ca4d951449d157998ca` — Qualify Crystalline multi-ray and native hand controls
- `d178ec275e4ecc7694cc1cf6ac26b2ef0abf7c55` — Preserve elemental providers through native save suspension
- `6af2a44776570affb0b4bc2251f6c15749d99175` — Implement native Crystalline Form core ray defense
- `ad812bb06f34f1bc616ed8c8018870178e0b479a` — Record successful elemental expansion checkpoint publication
- `e8879007f0c788b0bb494f7747c35bab730d59dc` — Audit native ray delivery for Crystalline Form
- `c93aafb4ab7ed5c8977797787553ade9ab6504ec` — Implement and qualify Efreeti Magic and combined-trait persistence
- `c7249576dd79d565039b635dab46fcae1e5a1d8a` — Implement and qualify elemental Insight and blood traits
- `6e5744490b783bed941b70763d03f76c1f3dd557` — Record initial elemental trait qualification checkpoint
- `530eff1ebe6814fc17a5fc39c1ac50bb215bfbbf` — Implement and qualify initial elemental trait mechanics
- `fa7900289286bc326014057c275de97a30b7d1ae` — Implement and qualify elemental trait replacement framework
- `e2986654246bf0081ba6965ae5fc90318635d059` — Record exact Elemental Feats qualification
- `d8ad16f7c10670ed5b200214738322195213c392` — Qualify Elemental Feats release
- `c4b9f8fbc40a21ecc9775deab66a40b2ec9b24f3` — Harden elemental feat selector reconciliation
- `ce4818b2992ffaaa4621d55515775d492f0f044e` — Record elemental feat persistence checkpoint
- `a4a377cc73585776ed24d40d77d3cebbe20ba72b` — Qualify elemental feat persistence
- `a41d535d0f255fe3a4efc6300dd0999a6cc4305f` — Record Undine feat qualification
- `b70ad97d25ff2c25a41dd544f2e6a7870c6bd12d` — Implement Undine elemental feats
- `ec6dc368075ae7462bc5ca2e741341367a323f34` — Record Sylph feat qualification
- `f514c3dbb31c8f2f705a5e3dea1237e8d9eeebc5` — Implement Sylph elemental feats
- `01c0c25a23da013a571695adda2ab5df8f6969bb` — Record advanced Ifrit feat qualification
- `7aee2740f7c08f0f9eec1b3efee4eff8e526ce51` — Implement Blazing Aura and Firesight
- `0fddd8a6793be7f38c3fbcd33f49c1a493e6488c` — Record Ifrit feat checkpoint publication status
- `768d8c94a4ec6658b71085fb0446243dae2d8d66` — Implement Scorching Weapons and Inner Flame
- `4adb34cc72e4a8ac7ccb0b5313f329a9a2f0f8fa` — Record Elemental Feat mechanics checkpoint
- `bacc7a0da6400fa4538db6092ee29f3ae28bd514` — Implement Elemental Strike and Wings of Air
- `84e36dd13229195fb984d50508b30f28e458c7b1` — Record Elemental Feat blueprint checkpoint
- `ecc65faad142960de6f5b1ea523feaa9ed83dac7` — Register Elemental Feat blueprints
- `99339e894420a33b335eec7883aeb95b33f4f646` — Define Elemental Feat behavior policies
- `74365a2c41e57d4fa76546cb69d5543101a94086` — Audit native Elemental Feat contracts
- `0350f7d75871fa739c4d9d8e821a20a13cf529fd` — Qualify Elemental Heritages release
- `1613cf8a766f680e28d201341327feb25b52dc5a` — Qualify elemental race publication with module off
- `61c4f40ce40fd811ef132826ab2bbd40d07c52c7` — Add isolated Tweak or Treat compatibility profile
- `68ac68b0c69779dc61b10caff9b6e43acc852ba6` — Harden legacy runtime evidence verification
- `72ce278eba040ee5b8bf1cce0e73537cc652e3f7` — Permit exact legacy migration producer preflight
- `e4a33171fd548d07c8b1994de3cad51850906965` — Add exact Elemental Race legacy migration qualification
- `52692129804a484f49ecadcca6c6fbfbe077594d` — Qualify Elemental Heritage persistence
- `17fe55d886ff3f75ebd96f5f231e9884d3a15842` — Qualify alternate heritage SLA commands
- `aca9aece0933d4713d5eae5cd98e1097fca52325` — Qualify heritage selection reconciliation
- `543ccdfc91bf2d31916176336985baef6d0720b8` — Implement Elemental Races heritages
- `9c0b7d7bdfe39dd54947c7a37d601cd91db98027` — Harden Elemental Races foundation mechanics
