# Firearm audio restoration qualification

Date: 2026-08-24

Status: automated implementation and routing qualified on the final installed
candidate. The repository owner subsequently reported that the sound effect was
working and explicitly approved commit, merge, and release. That human report,
not the technical playing IDs, closes the auditory release gate.

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

Results at final implementation commit
`d9a51132a39369d6393b7fe90b7a6ffc3ee243bf`:

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

Installed candidate identity:

| Artifact | Identity |
| --- | --- |
| Source commit | `d9a51132a39369d6393b7fe90b7a6ffc3ee243bf` |
| Package SHA-256 | `DFBEDB0CB3CF7ADDB38E5D794D49555B7CEF141F17139922DC8A08F65B163A51` |
| DLL SHA-256 | `E93B7BC51558E9D141B6E516EE7B3D93F1AC26A87E7169495F74FBB4CCF9CD43` |
| DLL MVID | `29aa9f64-4fd8-4716-b0c5-abf484ea45d4` |
| Deployment evidence | `20260824T0452286831396Z/deployment.json` |

## Guarded runtime qualification

The repository-owned harness launched through Steam App ID 640820 and ran only
`disposable-firearm-wwise-audio`. Final run
`20260824T0452404103662Z-eb0f3eb1afc8403ea015951c45bdc0cf` passed all 13
focused assertions and exited automatically. Result SHA-256:
`32FAB2E279F9DB9C5C12EF9897BCD8E51D98113DBDF07949E10771EC659C7729`.

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

A valid playing ID establishes Event acceptance only. On 2026-08-24, after the
exact candidate above was installed, the repository owner reported, "Sound
effect sounds working to me," and explicitly requested release. The owner did
not provide separate observations for every checklist line, audio-device
identity, or mixer setting; this record does not fabricate those details.

## Boundaries and release authorization

The implementation changed no third-party mod, proprietary game binary,
`Init.bnk`, unrelated feature, blueprint GUID, or save-owned content. The owner
authorized advancing the release identity to `0.0.96`, merging the qualified
branch, and publishing a new immutable release after the release pipeline
passes. The guarded publication helper remains authoritative; it must not be
bypassed if its external allowlist still rejects the exact branch.
