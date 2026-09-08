# Elemental Races 0.0.117 Crystalline persistence checkpoint

## Outcome and scope

Incremental mechanical PASS, not complete Crystalline Form or Release C
qualification. All 1,424 domain/reflection tests, repository validation,
clean Release and strict package checks pass. Nine final guarded processes
pass 10,490 assertions: two native profiles (10,180), four eight-trait
save processes (282), and three renewed 0.0.114 migration processes (28).
All 60 runtime-result visual/DollData warnings remain recorded.

Starting master: `6874dc15a27ded132456dbdd480f47c794543a05`.
Branch: `codex/elemental-races-expansion`.
The candidate embeds preceding commit
`6af2a44776570affb0b4bc2251f6c15749d99175`; its source fingerprint below
identifies the then-uncommitted code, before this curated documentation.

## Defect and narrow correction

The first save test failed: female Ironsoul had one use and an armed native
activatable/buff immediately before saving, but OFF loading found it unarmed.
Read-only inspection of the exact disposable working archive proved that the
mode was already OFF in serialized data. This was a production save defect,
not a weakened test expectation.

Installed native IL shows that SaveManager suspends the entire unit before
PreSave; UnitDescriptor sets IsTurnedOn=false before turning its facts off.
Four owned race, heritage, trait-marker and retain-marker OnTurnOff callbacks
mistook that suspension for permanent fact removal and recreated providers,
losing AddFacts-owned consent. They now leave providers unchanged while the
owner is suspended. Active-unit fact removal still reconciles normally.

New native regressions preserve exact mode and buff references during real
TurnOff/TurnOn for all three Oread heritages. Existing marker-removal/respec
tests still pass. No global patch, new ledger, donor mutation, save rewrite,
dynamic identity or new dependency was introduced.

## Actual persistence coverage

The new matrix keeps 24 fixtures: all four races, both sexes and all twelve
heritages. Eighteen carry native-selected traits; six Ifrits have legal
two-trait combinations. All seven earlier trait types remain represented.
Eight fixtures retain partially spent active blood healing; male and female
Ironsouls add Crystalline coverage.

Male Ironsoul starts spent and unarmed; female starts unspent and armed.
The setup uses the native resource-spend and activatable boundaries, not
saved-ledger assignment. Actual ray-hit expenditure remains separately proved
by the command scenario. OFF load, native level-up, rest, re-spending, ON load,
native respec to base traits, saved cleanup and fresh absence pass.
All 168 trait observations and 42 Crystalline observations are exact. Native
rest restored the male use without arming it and retained female consent.
Respec removed the exact resource, mode and buff. Final absence found none of
the 24 fixtures in any of the five tracked membership collections and wrote
no save.

Only `KMG_AUTOMATION_WORKING` was accessed through guarded requests and Steam
App ID 640820 on Windows 10. The protected baseline was not accessed.
The renewed pinned 0.0.114 producer/migration/absence cycle preserves exact
race/facts, stats, spent General SLAs and appearance data for all eight
original race/sex fixtures. No image or UI automation is mechanical proof.

## Immutable candidate

- Version: `0.0.117-elemental-traits`.
- Archive: `artifacts/qualification/0.0.117/crystalline-persistence-02`.
- ZIP: `KingmakerGunslinger-0.0.117-local-runtime.zip`, 23,223,953 bytes,
  135 entries.
- Source SHA-256: `5b67a9e58a085363a3314d00626d83783c9cf17b4f1cdee128fa2379f864b9ab`.
- ZIP SHA-256: `cb33bf0ed2c3b121ee205fbc874b5c9ea9500b5299913c523202dd2ce8e9c1b7`.
- DLL: 6,163,968 bytes.
- DLL SHA-256: `352b9df553c197831eb9e929d113872cb5278a634978b8ae941907ee141f0d97`.
- DLL MVID: `7d63807e-6da5-4130-b853-e6b57f7d58ff`.
- No new identities. Manifest remains 1,856 total / 1,854 active / 2 reserved;
  218 active Elemental identities, including 190 blueprints and 28 proxies;
  72 Release C identities. All legacy GUIDs remain unchanged.

## Runs and restoration

| Gate | Exact run ID | Assertions |
| --- | --- | --- |
| KMG only | 20260906T1616209644253Z-c09a0c87e38449afa718f1a2df1cf72c | 5,090 |
| High-risk combined | 20260906T1618265504293Z-25e2eb6dc5324fb6a7b512e9488b5544 | 5,090 |
| Prepare | 20260906T1628055592247Z-43c348f4a0514354acc287374cad466a | 58 |
| OFF | 20260906T1631561861916Z-f2dff3549ef042938d2947fc82ac4a55 | 146 |
| ON/respec/cleanup | 20260906T1635180492146Z-b2a782634af048709def8926ea616439 | 71 |
| Fresh absence | 20260906T1639236542905Z-e89c39f7f91049d7b7f76c4d6493c143 | 7 |
| Pinned 0.0.114 prepare | 20260906T1651070224708Z-c0a8a742e34843a2986760171db1beef | 11 |
| 0.0.117 migration/cleanup | 20260906T1654552561150Z-f7ad1bc87bb24cd4b2b3c18f993cac14 | 10 |
| Fresh legacy absence | 20260906T1658246678702Z-c7cb030bdcc84fe3a8e956a7a0de5545 | 7 |

Native checks use KMG-only and the established highest-risk combined profile.
Save-backed checks use an exact copy of the installed configuration, not the
separately unqualified isolated-combined working-save load environment.
Each final profile transaction restored the complete original mod tree,
settings and managed SoundBank. Independent final comparison agrees for all
968 manifest entries, including timestamps. Full per-process result, evidence,
mechanic, index, native-log and restoration hashes are in
`releaseCCrystallinePersistenceQualification` in the mission STATE.

Restored Mods manifest SHA-256:
`ccf49739800e26f33a9c78bce559b97cf0cdf1e73dd1801f2431cda1fac413f7`.
Encoding: SHA256 of UTF-8
`originalManifest | ConvertTo-Json -Depth 6 -Compress`.
Original/restored settings SHA-256:
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.

## Retained failures and open gates

Candidate 01 OFF persistence remains FAIL (123 assertions, seven failures).
Its cleanup was saved exactly. A recovery absence process passed; an ignored
wrapper parser error then stopped before a second launch and restored folders.
A repeat prepare confirmed that consent was lost during serialization itself.
An explicitly diagnostic level-one fixture recovery later remained FAIL
against the level-two respec contract, saved exact cleanup once, and was
followed by fresh absence before the final passing transaction.

The first unwrapped attempt restored settings bytes but did not restore the
full Mods metadata manifest: native startup/exit changed settings/output-file
timestamps and generated cache/previous files. No user data was deleted or
timestamps backdated. That exact-restoration gate remains FAIL for that attempt.
Every subsequent diagnostic and final run used the existing reversible full
folder transaction; none of those later PASS records erase the initial failure.

The final OFF native log retains four pre-existing
`feat-transient.reconcile.failed` ERROR entries during level-up of the
Scorching Weapons/Elemental Strike fixtures. The same units and messages
appear in the earlier seven-trait Efreeti OFF run. Exact final transient
state assertions pass, but this separate preview/hydration boundary still
requires investigation before final Release C acceptance. Zero Fact.PostLoad
signatures does not mean zero native errors. Existing shader, missing-script
and lightmap diagnostics also remain.

Still open: complete ray classification, multi-ray/duplicate-event and
non-damage-ray controls, remaining equipment and lifecycle boundaries;
Treacherous Earth, Breeze-Kissed, Acid Breath, Nereid Fascination and Ooze
Breath; the other thirteen traits' persistence; full trait lifecycle,
compatibility and release gates. Visual Adjustments is absent and NOT-RUN.
Prior reports retain their original historical scope.

Nothing was merged, tagged or publicly released; no PR was created.
Generated packages, raw artifacts, saves and machine settings are not committed.

