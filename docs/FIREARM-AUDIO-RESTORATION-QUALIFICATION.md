# Firearm audio restoration qualification

Date: 2026-08-24

Status: automated implementation and routing qualified on the implementation
checkpoint; human auditory acceptance pending. This record does not claim that
audio reached the speakers.

## Proven root cause

The defect was not a schema migration, malformed source asset, encoding issue,
package mutation, stale live copy, alternate manifest path, or SoundBank-byte
failure. The old production loader called
`JsonConvert.DeserializeObject<FirearmSoundBankManifest>(json)` and therefore
inherited process-global `JsonConvert.DefaultSettings`.

The exact old installed loader and canonical live JSON were exercised twice:

- With neutral global defaults, the object parsed as schema 1 and validated.
- With a deliberately hostile but valid global contract resolver, the same
  bytes produced `SchemaVersion=0`, null/default remaining members, and the
  exact `Unsupported manifest schema.` exception.

That test fails against the old loader and passes against the repaired loader.
It proves the KMG loader's global-state dependency was causal. Retained logs do
not identify which full-stack component set the global defaults, so no
third-party attribution is made and no third-party file was changed.

## Representation trace

The repository source, Release staging copy, standalone ZIP entry, live
installed manifest, runtime text, and repaired production object agreed:

| Property | Observed value |
| --- | --- |
| Byte length | 610 |
| SHA-256 | `BF57981AD5EC2CBF3149ECAFC3EF737D87BC9035B14BCCC7D254DCA8F991C62E` |
| Encoding/BOM | UTF-8 / none |
| Raw schema token | `1` / JSON Integer |
| Parsed schema | 1 |
| Bank name/file | `KMG_Firearms` / `KMG_Firearms.bnk` |
| Platform | `Windows` |
| Wwise | `2016.2.6.6153` |
| Bank SHA-256 | `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18` |
| Embedded media | `true` |
| Events | 5 canonical mappings |

Source, package, live, and staged `KMG_Firearms.bnk` copies were byte-identical:
999,390 bytes and the bank SHA-256 above. `Init.bnk`, additional `.bnk`, and
external `.wem` files were absent from the strict package.

## Production repair

`FirearmSoundBankManifestLoader` now uses a private exact `JsonTextReader`
boundary compatible with the game's Newtonsoft.Json version. It does not call
`JsonConvert.DeserializeObject` and does not inherit
`JsonConvert.DefaultSettings`. It requires the exact canonical names and token
types, rejects null/malformed/trailing JSON, absent/null/zero/future/string/
duplicate schema values, unknown or duplicate properties, missing required
fields, wrong identities/mappings, malformed or lowercase hashes,
non-embedded media, and extra/missing/duplicate Events.

The existing semantic bank/platform/Wwise/hash/media/Event validation remains
intact. Failure stages distinguish `manifest.read`, `manifest.json-parsing`,
`manifest.schema-extraction`, `manifest.semantic-validation`,
`bank.validation`, `bank.staging`, `bank.loading`, and PostEvent input,
not-ready, rejection, or exception states. Configuration faults cannot report
`Ready`, and the development retry re-enters the repaired production parser.

Successful startup emits one record per stage:

```text
[audio][manifest.read] path=<live manifest>;byteLength=610;manifestSha256=BF57981A...991C62E;rawSchemaToken=1;schemaTokenType=Integer;encoding=UTF-8;bom=None
[audio][manifest.validated] schemaVersion=1;bankName=KMG_Firearms;bankSha256=0E9F88C5...92EDF18;eventCount=5
[audio][bank.staged] decision=Skipped;source=<live bank>;destination=<game bank>;sourceSha256=0E9F88C5...92EDF18;destinationSha256=0E9F88C5...92EDF18;hashParity=True
[audio][bank.ready] wwiseInitialized=True;bankName=KMG_Firearms;loadAttempts=1
```

The fresh UMM/output logs contained each once and contained zero
`configuration.disabled` records.

## Tests and artifact qualification

The dependency-free suite now exercises the checked-in production JSON through
the real loader, a deterministic copied package/live representation, hostile
global serializer defaults restored in `finally`, every required strict
failure, source/package/bank parity, package allowlists, transactional installed
parity, one stage/load, idempotent readiness, nonzero/zero PostEvent handling,
fault state, and production-parser retry.

Results at implementation checkpoint
`ea51bd3732fd7313e92bcc2edac9560008f6c9ac`:

- Repository validation: PASS.
- Complete domain/reflection suite: 1,224/1,224 PASS.
- Authored Wwise object validation: PASS.
- Deterministic source/polish validation: PASS.
- Production SoundBank validation: PASS.
- Clean Release build/output: PASS.
- Strict standalone package and production-loader artifact validation: PASS.
- Transactional deployment dry run and deployment: PASS; feature-module
  settings preserved.
- `git diff --check`: PASS.
- Bank/source-audio disposition: preserved byte-for-byte.

Checkpoint artifact identity:

| Artifact | Identity |
| --- | --- |
| Source commit | `ea51bd3732fd7313e92bcc2edac9560008f6c9ac` |
| Package SHA-256 | `9D2569E4F5F2238947DC1B6F63F171FA95AF791B59C651564458D9A79FD59325` |
| DLL SHA-256 | `6028FE4B4D20688D0C7E8E44C1AB4EF9B024DA7F385A46C54278EBAF95A10A40` |
| DLL MVID | `4fe2a77e-7be7-4744-9b6f-2076ba22987b` |
| Deployment evidence | `20260824T0444034836709Z/deployment.json` |

## Guarded runtime qualification

The repository-owned harness launched through Steam App ID 640820 and ran only
`disposable-firearm-wwise-audio`. Run
`20260824T0444147375477Z-849bb57337ad44538df0c8582d5038f7` passed all focused
assertions and exited automatically. Result SHA-256:
`55E251B7BFEE529917DB869B0421C79F5DD76BE3D66D9C668217F5720942311B`.

| Route | Observed result |
| --- | --- |
| Global Pistol | `KMG_Firearm_Pistol_Shot`, playing ID 2 |
| Global Musket | `KMG_Firearm_Musket_Shot`, playing ID 3 |
| Global Blunderbuss | `KMG_Firearm_Blunderbuss_Shot`, playing ID 4 |
| Global Revolver | `KMG_Firearm_Revolver_Shot`, playing ID 5 |
| Global Rifle | `KMG_Firearm_Rifle_Shot`, playing ID 6 |
| Live unit emitter | Blunderbuss, playing ID 7 |
| Ordinary committed shot | Blunderbuss, playing ID 8, exactly once |
| Native committed miss | Blunderbuss, playing ID 10, exactly once; `IsHit=False` |
| True misfire | no ordinary custom post |
| Empty/Wrecked/canceled | no ordinary custom post |
| Native crossbow | no custom firearm post |
| Scatter | one notification per committed volley; all-misfire volley silent |
| Existing deed routes | Dead Shot +1, Startling Shot +1, Menacing Shot +2, Stop Bleeding +2 |

A valid playing ID establishes Event acceptance only. The twelve-step owner
listening gate in `FIREARM-WWISE-MANUAL-AUDITORY-ACCEPTANCE.md` remains open.

## Boundaries and remaining blocker

No third-party mod, proprietary game binary, `Init.bnk`, unrelated feature,
blueprint GUID, save-owned content, release version, master branch, tag, pull
request, or public release was changed. The mandated guarded push helper was
used after each coherent commit, but its external allowlist omits the exact
required `codex/firearm-audio-restoration` branch. The helper was not modified
or bypassed and no raw push was used.
