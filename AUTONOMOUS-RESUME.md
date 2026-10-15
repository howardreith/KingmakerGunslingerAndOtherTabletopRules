# Gunslinger animal-companion class eligibility — v0.0.154 RELEASED / STOP

The owner accepted the focused checkpoint on 2026-10-15 ("This is acceptable.
Please merge and cut a release."), including its stated limits. Product PR #46
merged normally as `342c8a80c7099b9b18d88de6abd46d9f3e918473` (tree identical
to gated branch head `8f1223933d`). The exact-reference `Publish-Release.ps1`
then ran once from clean, pushed `master` in 143s: its first build passed
2,596/2,596 domain tests, the 37-assertion harness and strict package
validation, and both clean builds produced identical bytes.

[Stable v0.0.154](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.154)
is published and Latest, titled "Tabletop Added Rules v0.0.154". Annotated tag
object `4ca022374ec38a91fe435cd2a07078028653be11` peels to the merge commit.
The public assets' SHA-256 values match GitHub's digests:

- `KingmakerGunslinger-0.0.154-gunslinger-animal-companion-eligibility.zip`:
  `c94f1b39ff9d7187110b42fc395fae7ed117ce64fa5b4bcc95518c5316f666e1`
  (365 files; DLL
  `9253342600110445ff091022fb9fdc257f33d3226f48d5a6f73358e4a7d806be`);
- `SHA256SUMS.txt`:
  `8368338b6e9defe776d263da6a5e5fb7488b115e86bc6ace444c3a197fb13d75`;
- `release-manifest.json`:
  `13d4d58e6e90f47d5f67c41610c1d26c0fad3d6f188f10ffd0b29358f7063ef6`.

The downloaded ZIP passed strict package validation, and its DLL embeds the
merge commit. The exact-reference build stamps `AssemblyMetadata("GitCommit")`,
so these bytes differ from the gate candidate below. Between those commits only
`Publish-Release.ps1` and this record changed, neither compiled nor packaged;
the publisher's own run qualified the released bytes. Nothing was deployed to the
live installation and no save was touched.

Root cause: installed Kingmaker 2.1.7b applies a class
(`LevelUpController.SelectClass` and level plans through `SelectClass.Check`)
only when `BlueprintCharacterClass.MeetsPrerequisites` passes, and the class
selector (`CharBSelectorLayer.FillData`, console `CharGenClassPhaseVM`) enables
an entry on the same predicate. It reads only the class's `Prerequisite`
components. Native player classes in the 2.1.4 blueprint dump carry
`PrerequisiteNoClassLevel(AnimalClass 4cd1757a0eea7694ba5c933729a53920)`; the
runtime-created Gunslinger had none, so a unit with Animal levels passed.
Vanilla pets auto-level through `AddClassLevels`; the selector exists because
the installed CallOfTheWild 1.14.4c levels Animal/Eidolon/Phantom/Dragon pets
through it and adds Animal to the class catalog.

Fix: `GunslingerClassBlueprints.CreateClass` adds that one component before
registration, and `Validate` fails bootstrap closed without it.
`HideIfRestricted` stays false as on Fighter: a selector that lists classes a
pet cannot take shows Gunslinger unavailable beside Fighter. GUID, symbol,
manifest, publication order and the Animal blueprint are unchanged.

Validated: the production compile, the focused domain selection and
`scripts\Test-GunslingerAnimalCompanionEligibility.ps1` (37 assertions, native
`SelectClass.Check`/`MeetsPrerequisites` on synthetic units and classes,
Assembly-CSharp SHA-256
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`).
`Build-Local.ps1` runs that script on the exact-reference DLL.

Pre-merge candidate gate: `Invoke-KmgGate.ps1 -Level Sprint` on `cc62cb1d0` passed in
91.8s (2,596/2,596 domain tests, repository wrapper, exact-reference build,
the 37-assertion harness and strict package validation; 365 package files).
Package SHA-256
`0f333f0988c1b2151334ebd634b45b79b1b55e4c47910862d936b29da81da586`, DLL
SHA-256 `c02b2113ddffd2ecd0bfda6df14a37d607ba779a5836842e632397687ebe0f37`.
This machine's Sep 6 private reference bundle lacks
`UnityEngine.PhysicsModule.dll`, required since Oct 6; the gate used a fresh
`export-private-build-references.ps1` bundle of the installed Steam 2.1.7b
files at `C:\Dev\KingmakerGunslingerLab\private\refresh-2026-10-15`.

Not performed: in-game observation of a real companion's level-up or respec
selector. Open follow-up: CotW adds its Eidolon/Phantom/Dragon exclusions in its
own `LoadDictionary` postfix, before KMG publishes Gunslinger, so CotW-only pets
are not covered. A pet that already took a Gunslinger level is not repaired; it
can no longer gain one.

# Sprint 23 Powerful Charge rounding follow-up — RELEASED / STOP

The owner selected and completed a bounded correction and patch-release
campaign for the released Triceratops Powerful Charge live-Strength
calculation. Product PR #43 merged normally as
`e122013edce9ceb88bd9aa0b40c9762cc717ed8d`; stable v0.0.153 is published and
Latest. Sprint 24 was not started.

## Preserved released baseline

Sprint 23 remains historically **RELEASED / STOP** at v0.0.152: product PR
#41 merged as `1d41388adb6aaa6e4e102e744abb392104cedbba`, final-records
PR #42 produced master `15d6c0d82bf14fa4fd66c72c0f780f86abf44794`, and the
tested source was `e0d7cb863b23925cfbf1721e3965c05e811a0bd7`. Its exact
ZIP SHA-256 is
`e417ad95e796bac7c312ed60e859b888b1dd2f83818ef211027e35d2c1c663db`.
The published surface remains 105 project creatures,
**1,075 generated placements**, 29 retained native wrappers, and
**1,104 visible choices**.
That release's mechanics, art, route, lifecycle, persistence, compatibility
and restoration results remain unchanged in
[the Sprint 23 coverage record](docs/SPRINT23-COVERAGE.md) and
[ledger](planning/SPRINT23-TRICERATOPS-STEGOSAURUS-LEDGER.json).

The released helper separately truncated the additional half-Strength term.
A fresh focused reproduction selected one test and failed at modifier +7:
ordinary +10 plus the released +3 correction produced +13 instead of the
documented +14 target. Installed Kingmaker 2.1.7b IL confirms that a positive
single-natural-attack contribution is truncated after multiplication by 1.5,
while a zero or negative modifier is applied once. The source correction now
computes the difference between that already-applied native term and the
Triceratops's existing positive 2x target; zero and penalties receive no extra
correction. The full Sprint 23 focused selection passes 12/12 after this edit.

The frozen candidate is
`8f30cf72c916aaee840c18b373be82274729520e`, source fingerprint
`5167ff01981bb3a6d4109ebc0dde5646db258902e4c8dea74f2d8414626046ef`.
Its one full Sprint gate passed 2,594/2,594 tests plus strict package validation
in 181.8 seconds. The exact ZIP SHA-256 is
`cca7c0d2fe902e79aab090250912e0aa03a135c5680a926ab4e4d8749dfd366b`;
the DLL SHA-256 is
`bab22f7fa8a10c4f9adc2410b4e2fd4ec98b56d3a5291bbd36cf0b47fbf333b0`
with MVID `dd3797aa-a482-4411-b3a5-f574204a320a`.

Fresh guarded native qualification passed on those exact bytes:

- standalone RTWP: profile `compat-20261014T025926Z-f7acd4897101`, runtime
  `20261014T0301019306544Z-7c896449d20e4e3d9335f1e4dde928e9`;
- standalone turn-based: profile `compat-20261014T030403Z-28aa83f5aa3f`, runtime
  `20261014T0305412649861Z-3ce64fad9ff94f36a5fde7986fb999a5`;
- highest-risk-combined RTWP: profile
  `compat-20261014T030800Z-b3091a8a321a`, runtime
  `20261014T0309454568154Z-c3d6c46905f1401c9d59ce7fb8f7d427`.

Each profile passed 13/13 selected assertions and exact restoration. Real native
charge hits at modifiers +7/+8/+9/+10 recorded 2d10 ordinary Strength
contributions +10/+12/+13/+15 and 4d10 charge totals +14/+16/+18/+20, with
Power Attack off, zero unrelated additive damage and no critical. Separate
Power Attack-on and critical checks passed, as did ordinary, aborted, later,
opportunity and released Rhinoceros/Woolly Rhinoceros controls. Zero and
negative Strength retain the native once-applied convention.

The post-native audit exactly matches the actual intake: 968 Mods manifest
entries, installed KMG 0.0.117 with 136 files, 83 game-setting values, 118 save
files and both protected fixture hashes. No Kingmaker process, compatibility
lock, transaction staging or owned diagnostic temp remains. The observer-only
faction handoff is request-local: it preserves the original monster weapon-stat
path, uses the installed Power Attack watcher's player-faction exemption only
at the attack-roll boundary, and restores the original faction before damage.

The exact qualified ZIP was promoted without rebuilding as
[stable v0.0.153](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.153).
The annotated tag object is
`586d42b70b30569f88266c73ea7afc3696e911be` and peels to product merge
`e122013edce9ceb88bd9aa0b40c9762cc717ed8d`. The public release has exactly the
ZIP, `SHA256SUMS.txt` and `release-manifest.json`; their SHA-256 values are
respectively
`cca7c0d2fe902e79aab090250912e0aa03a135c5680a926ab4e4d8749dfd366b`,
`c9e33dceafd4f125b58604bd62adb19001a71e673d3bce0a9348442d3e2febcd`
and `690e3dbb597d586fc41d9ef8989fb4dc0f443b4b3305d72177c6d727791b0a39`.
The downloaded ZIP passed strict validation and contained the exact candidate
DLL/MVID. GitHub's asset digests match, the release is stable/non-draft/
non-prerelease, and Latest points to v0.0.153.

The post-release audit again matches the actual intake exactly: 968 Mods
entries, installed KMG 0.0.117 with 136 files, 83 settings, 118 saves, both
protected fixture hashes, the sound bank and installed/private references.
There is no Kingmaker process, compatibility lock, game staging, publisher
checkout or owned diagnostic temp. Documentation-only PR #44 synchronizes that
verified released state to master. The endpoint is achieved: stop; do not begin
Sprint 24.

The authoritative active contract and evidence ledger are
[the rounding mission](docs/SPRINT23-TRICERATOPS-POWERFUL-CHARGE-ROUNDING-MISSION.md)
and [its machine-readable ledger](planning/SPRINT23-TRICERATOPS-POWERFUL-CHARGE-ROUNDING-LEDGER.json).
Do not alter dinosaur art, identities, routes, trample/trip behavior or old
release records. Do not begin Sprint 24.
