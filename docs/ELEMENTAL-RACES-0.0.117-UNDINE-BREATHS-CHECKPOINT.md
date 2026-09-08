# Elemental Races 0.0.117 Undine breath native checkpoint

## Outcome and limits

PASS for the focused native Acid Breath/Ooze Breath checkpoint only.
Repository validation, all 1,427 domain/reflection tests, clean exact-reference
Release build and strict package validation pass. Two guarded Steam App ID
640820 processes pass 10,936 assertions, including 652 breath observations,
with zero runtime-result warnings and exact restoration.

Release C is incomplete. Full native action-cooldown commitment controls,
breath-bearing save/OFF/ON persistence, complete lifecycle and final release
profiles remain pending. The current command tests prove actual cast effects
and the Standard-action blueprint contract, not the full cooldown-controller
boundary. Three traits are still unimplemented: Treacherous Earth,
Breeze-Kissed and Nereid Fascination.

## Rules and implementation

The [authoritative Undine rules](https://aonprd.com/RacesDisplay.aspx?ItemName=Undine)
specify five-foot acid cones, a daily use, Constitution-based half-level DCs
and capped damage dice. Ooze Breath additionally sickens on a failed save.
The [round-down convention](https://legacy.aonprd.com/corerulebook/gettingStarted.html)
and absence of a minimum die produce zero damage dice at level one; Ooze's
failed-save condition still applies.

Each trait consumes the racial-SLA replacement slot and has its own fixed
ability/resource pair. Fresh owned native components deliver the cone,
Reflex save, acid damage and, for Ooze, three-round native non-poison Sickened.
The existing daily-resource memory and exact owned-provider reconciler retain
spent amounts through provider reconstruction. Native racial-SLA parameters
remain unchanged for Efreeti Magic; an optional owned parameter component
provides the breaths' half-level/current-Constitution contract.

No donor array/component is mutated. The native Earth mephit acid cone supplies native icon/animation precedent;
its existing acid-cone projectile supplies native-only art.
Mechanical cone length is five feet; no new visual asset or exact visual-scale
qualification is claimed. Native Sickened has no poison descriptor.
Native damage clamps a zero-dice packet to one damage, so a local native
conditional skips only that packet. There is no global damage rewrite.

Actual native commands across General, Mistsoul and Rimesoul prove levels
1/2/5/10/11/20 with a Fighter/Wizard class split, current temporary Constitution,
exact UI/execution DC/CL, capped dice but uncapped DC, matching-affinity
exclusion, Reflex half with identical seeded rolls, resistance/immunity,
poison-immunity exclusion, allied targets, outside/behind/side cone exclusions,
three-round native expiry, queued cancellation, one-use commitment, zero-use
availability, native level-up, ordinary rest and same-day provider re-add.
Only asynchronous projectile arrival is completed by the request-local driver;
native target enumeration and all downstream effects remain native.
No direct effect application or manual resource-spend fallback is used.

## Artifact and identity ledger

Starting master: `6874dc15a27ded132456dbdd480f47c794543a05`.
Branch: `codex/elemental-races-expansion`.
Embedded parent: `0007d7c97f11cca70dd682bb2e006059cfd6e0c1`.
Version: `0.0.117-elemental-traits`.
Source-state SHA-256: `38e071a8092fce13ae3bd1fddca4e73dcf2b8c447eae9cf73b9d690996068228`.

Archive: `artifacts/qualification/0.0.117/undine-breaths-01` (ignored).

- ZIP: 23,239,729 bytes, 135 entries;
  SHA-256 `2338fa0775887259f8fb2d7f9216ec10947c04ccb507afde13de51c74d83c7c4`.
- DLL: 6,202,880 bytes;
  SHA-256 `b379a40cf41beeec79f84260d36fb2eac6411ac727fb48b829d8b175a6f6c41a`;
  MVID `8f780d13-64ec-4fa0-b9d9-bbbc44721377`.
- Deployment `20260906T1939194648237Z`;
  SHA-256 `759f3d795b342d5c98b6ad24808501d449f2f97b248d54cf852f632d2160ee22`.

Four identities append, without replacing any prior GUID:

| Symbol under KMG.ElementalRaces.Traits.Undine | Stable GUID |
| --- | --- |
| AcidBreath.Resource | `e117e1e0a17a4acec001000000000073` |
| AcidBreath.Ability | `e117e1e0a17a4acec001000000000074` |
| OozeBreath.Resource | `e117e1e0a17a4acec001000000000075` |
| OozeBreath.Ability | `e117e1e0a17a4acec001000000000076` |

Manifest: 1,860 total / 1,858 active / two reserved. Elemental Races has
222 active identities: 194 blueprints and 28 visual proxies. Release C
contributes 76. Registration remains unconditional; no new module or schema.

| Profile | Run ID | Assertions | Breath observations |
| --- | --- | ---: | ---: |
| gunslinger-only | `20260906T1939501436030Z-24217250a716424a94083602917f0171` | 5468 | 326 |
| gunslinger-high-risk-combined-favored-class | `20260906T1942096421654Z-f8b892e87c754aefa5786559d9102e70` | 5468 | 326 |

All result/evidence/mechanic/log/summary and transaction hashes are in
`releaseCUndineBreathNativeQualification` in STATE. Both full 968-entry
restorations independently match the live original manifest:
`8498aca7ab2755dd41fa813112cb2abecde7dc09bb79f21133a4dbae7f085c96`.
Encoding: UTF-8 PowerShell `originalManifest | ConvertTo-Json -Depth 6 -Compress`.
Settings SHA-256:
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
No game process remains; no campaign save or protected baseline was accessed.

## Diagnostics and next work

Preflight found stale aggregate identity counters; clean compilation then
found two missing imports and an absent named zero Metamagic enum value.
Those failures are retained in the journal. Both actual native attempts PASS.
A read-only collector quoting error changed no evidence and caused no game rerun.

Native shader, missing-script and lightmap diagnostics remain; combined has
four inherited KeyNotFoundException occurrences. No new elemental ERROR,
transient-restoration or Fact.PostLoad signature appears. No blanket clean-log
claim. Visual Adjustments is absent and NOT-RUN.

Next: native cooldown commitment, the ten-trait breath-bearing save matrix,
three remaining mechanics, complete trait lifecycle and final release gates.
Earlier 0.0.114 migration remains historical evidence, not a new breath-save
test. No package, raw artifact, save or proprietary assembly is committed.
Nothing was merged, tagged, publicly released or made into a PR.
