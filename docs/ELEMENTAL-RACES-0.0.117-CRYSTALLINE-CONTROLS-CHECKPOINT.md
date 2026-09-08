# Elemental Races 0.0.117 Crystalline controls checkpoint

## Outcome

PASS for this incremental native-control and read-only audit checkpoint only.
All 1,424 domain/reflection tests, repository validation, clean exact-reference
Release build and strict package validation pass. Two guarded Steam App ID
640820 processes pass 10,242 assertions, including 404 Crystalline observations.
Release C remains incomplete. No production mechanic or save-bearing identity
changes in this checkpoint.

For all three Oread heritages, actual commands now prove:

- Native greatsword two-handed use and disabled hands prevent deflection;
  restoring an available hand restores the benefit without rest or trait re-add.
- Prepared Wizard Scorching Ray at native spellbook CL11 creates three distinct
  attack rolls. Exactly one ray is deflected; two hit; the prepared slot is spent.
  Six native impact notifications cannot spend the daily use twice.
- Ray of Enfeeblement has a positive native buff/Strength-loss control.
  Deflection skips its effect entirely, rather than applying and removing it.
- Fixture units/projectiles and the original clock are restored exactly.

Only projectile transport completion and request-local delayed-ray scheduling
are isolated. The native command, spellbook, projectile creation/delay, attack
rolls, OnHit notifications and downstream effects remain native. The driver
advances 0.7 seconds in the three-ray witness and restores that exact clock in
finally. No campaign save is opened or changed.

## Artifact and evidence

Authoritative starting master:
`6874dc15a27ded132456dbdd480f47c794543a05`.
Branch: `codex/elemental-races-expansion`.
Embedded parent: `d178ec275e4ecc7694cc1cf6ac26b2ef0abf7c55`.
Version: `0.0.117-elemental-traits`.
Source-state SHA-256: `ab2730d364637d541d36ffa0756d3576f7b267a79edd934a4a1f0d7dd5eb8bb2`.

Immutable ignored archive: `artifacts/qualification/0.0.117/crystalline-controls-04`.

- ZIP: 23,227,761 bytes / 135 entries;
  SHA-256 `12eacc2b69cd5c5b56a9a3e096d6efecd40f2b9d18623205a6526275c4941781`.
- DLL: 6,174,208 bytes;
  SHA-256 `4d568ecea5e18bb604452b5487707bbc78da1eba5bf97dd501bc04c56897132c`;
  MVID `8a7575cd-b0ec-4349-9cf1-1b5289e93e3b`.
- Deployment `20260906T1752324911913Z`;
  SHA-256 `a3cda8854a6254a95aa4dcb847019143894090ce41b847ebfeaeff0cd2f0f699`.

Manifest unchanged: 1,856 total, 1,854 active, two reserved; 218 active
Elemental identities (190 blueprints and 28 visual proxies), including 72
Release C identities. This checkpoint adds zero.

| Profile | Run ID | Assertions | Read-only contracts |
| --- | --- | ---: | ---: |
| gunslinger-only | `20260906T1753031449687Z-f0ca8625d78a4485bec04c557eadc12f` | 5121 | 171 |
| gunslinger-high-risk-combined-favored-class | `20260906T1755326621405Z-e1e292c173f74d3c9c6eda0bea065ab9` | 5121 | 257 |

Both exact 968-entry Mods manifests independently match the restored live tree,
including contents, timestamps and order. Manifest SHA-256:
`a8e9a07154989e99ead411ece9274884da35db3a746491e6918adc47b9334c6d`.
Encoding: UTF-8 PowerShell `originalManifest | ConvertTo-Json -Depth 6 -Compress`.
Feature-module settings were preserved:
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`. No Kingmaker process remained after restoration.

Per-run result, actual `runtime-evidence.json`, mechanic/audit, native-log,
attribution-summary and transaction hashes are recorded in
`releaseCCrystallineControlsQualification` in the mission STATE.

Zero runtime-result warnings. Each native log retains four shader groups,
four missing serialized-script messages and one lightmap-mode diagnostic.
Combined retains four KeyNotFoundException occurrences, the same count as
earlier combined checkpoints. No blanket error-free-log claim. Zero transient
or Fact.PostLoad signatures in these save-free runs does not resolve the
separate previously observed save-backed level-up preview errors.

## Remaining-trait audit

The read-only audit preserves every library entry/component-array reference and
order; a serialized-contract completeness assertion prevents empty DTO output
from counting as evidence. Its 171/257 inspected contracts are not mechanical
qualification.

The [Undine rules](https://aonprd.com/RacesDisplay.aspx?ItemName=Undine)
require acid cones and a humanoid fascination effect. Native Earth mephit breath
provides actual acid cone delivery. The native Water mephit ability instead
contains electricity damage despite cold text/descriptors, so it is not a safe
acid donor. Native Bard Fascinate uses Dazed plus damage-triggered removal as
its engine abstraction; its bard-specific immunity and descriptors cannot be
imported without review. Neither remaining trait is implemented by this audit.

The printed breath dice have no minimum-one-die clause.
The [core rounding rule](https://legacy.aonprd.com/corerulebook/gettingStarted.html)
rounds down unless stated otherwise; level-one zero-dice behavior needs explicit
native testing, not an invented minimum. Nereid duration does state minimum one.

[Breeze-Kissed](https://aonprd.com/RacesDisplay.aspx?ItemName=Sylph)
does not provide a special maneuver formula; ordinary native CMB is the
candidate, pending actual rule tests.
[Treacherous Earth](https://aonprd.com/RacesDisplay.aspx?ItemName=Oread)
restricts eligible ground material. The sound surface catalog alone does not
prove a reliable earth/unworked-stone/sand predicate. No broad ground acceptance
or substitute benefit has been introduced.

## Failures and remaining gates

Candidates 01-03 remain FAIL: an equipment-asset/item identity error; incomplete
native spellbook fixture setup; and a synchronous driver which did not advance
the native between-ray clock. All failed KMG-only profiles restored exactly;
combined was not attempted after those failures. Their exact results and
corrections remain in STATE. A read-only evidence collector also initially
encountered inherited StrictMode while reading absent save fields; isolating
its existing null-tolerant scope corrected collection, without another game run.

The earlier [eight-trait save and pinned 0.0.114 migration checkpoint](ELEMENTAL-RACES-0.0.117-CRYSTALLINE-PERSISTENCE-CHECKPOINT.md)
remains historical proof of its exact production scope. No new persistence
claim follows from these test-only changes.

Still open: semantic story-effect classification, full trait lifecycle, native
feat-transient preview diagnostics, five unfinished traits, all-trait
persistence and final release compatibility. Visual Adjustments is absent and
NOT-RUN. No package, raw artifact, save or proprietary assembly is committed.
Nothing was merged, tagged, publicly released or made into a PR.
