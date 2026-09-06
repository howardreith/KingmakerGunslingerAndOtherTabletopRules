# Elemental Races 0.0.117 Crystalline Form core checkpoint

## Outcome and scope

Core native-mechanic qualification PASS in KMG-only and the highest-risk
combined profile. This is **not complete Crystalline Form qualification or a
Release C PASS**. Five other required mechanics remain unimplemented; full
trait persistence, lifecycle and release-wide gates remain open.

Authoritative starting master: `6874dc15a27ded132456dbdd480f47c794543a05`.
Branch: `codex/elemental-races-expansion`.
The qualified artifact embeds preceding commit `ad812bb06f34f1bc616ed8c8018870178e0b479a`.
Its then-uncommitted source fingerprint below identifies the exact built code;
subsequent curated documentation does not retroactively change that identity.

## Rules and implementation

[Oread Crystalline Form](https://aonprd.com/RacesDisplay.aspx?ItemName=Oread)
replaces affinity, grants +2 racial AC against rays, and allows one daily
deflection as [Deflect Arrows](https://aonprd.com/FeatDisplay.aspx?ItemName=Deflect%20Arrows).
The owned native activatable opts into the next eligible hit without spending
an action. Canceling it spends neither action nor use. Awareness, freedom from
flat-footedness, consciousness and a free hand are required; the use is spent
only when a ray is actually deflected.

The source uses 34 immutable, individually audited native/project ray identities,
the actual attack weapon, simple attack-roll delivery and exact parent traversal.
Neither the Ray weapon category nor shared projectile art is sufficient.
The local owned subscriber changes the native resolved attack result at
Projectile.OnHit, before ordinary native effect application. No global Harmony
rewrite or optional assembly dependency is added.

Independent racial bonuses retain the native stacking rule, matching the
[PRD bonus-type exception](https://legacy.aonprd.com/coreRulebook/magic.html).
The exact provider reconciler prevents duplicate trait instances. Three new
identities append to the unchanged marker/provider pair:

| Suffix under KMG.ElementalRaces.Traits.Oread.CrystallineForm | GUID |
| --- | --- |
| Resource | `e117e1e0a17a4acec001000000000070` |
| ArmedBuff | `e117e1e0a17a4acec001000000000071` |
| Mode | `e117e1e0a17a4acec001000000000072` |

Manifest: 1,856 total / 1,854 active / 2 reserved; 218 active Elemental
identities; 72 Release C identities. No legacy identity changed.

## Exact candidate and source gates

- Version: `0.0.117-elemental-traits`.
- Repository validation: PASS; complete domain/reflection suite: 1,423/1,423.
- Clean Release and strict standalone/package validation: PASS.
- Package: `KingmakerGunslinger-0.0.117-local-runtime.zip`;
  23,222,181 bytes, 135 entries.
- Source-state SHA-256: `1898ebb6f418806a5fa0d597222a75471e6c614c2e2f4e485e293277a1f3e48d`.
- ZIP SHA-256: `91bd0f3102e3825603ad01ffa09693cbd88a3d555e597394e95f6c6c0176f831`.
- DLL: 6,158,848 bytes.
- DLL SHA-256: `aa733beb731b0baa5718576c82d562215e4984f2bc84d52593a9fcdc39e19414`.
- DLL MVID: `29571794-fa42-4e14-a630-22d1a0def365`.
- Local ignored archive: `artifacts/qualification/0.0.117/crystalline-development-06`.

## Guarded native evidence

Both fresh Windows 10 processes used the documented request through Steam
App ID 640820, save-free named disposable units and guarded clean exit.
They each passed 5,084 assertions (10,168 total), including 185 Crystalline
assertions per process. No runtime-result warnings or native fixture
errors/exceptions occurred. Native initialization still logged the existing
four shader groups, four missing serialized-script messages and one lightmap
message; exact counts and raw-log hashes are retained in STATE, not hidden.

### gunslinger-only

Run: `20260906T1505159294610Z-793180e036eb432b9aefed602c190aa9`.
Result SHA-256: `37eab019456f2fe74c6435470ab3b1cf71a3306241d31ca34fba3092d09a0789`.
Evidence manifest SHA-256: `9b9d407928ca821a2f2f6752e6bbff7d2cdff958844dcad2df93749fa89a9a22`.
Mechanic evidence SHA-256: `d8c4c5dee6246fe00d5fd84380b0b9d7c5630ea54bccb7a1f9d5fe5c2f1bdc8d`.
Profile transaction SHA-256: `0efdd566da090dce2cb61b05f50445fd67dac6bcf7df8c17d70c3eff8390128c`.

### gunslinger-high-risk-combined-favored-class

Run: `20260906T1507128952013Z-c3b521139ea44095908e9942b7765d55`.
Result SHA-256: `31169aeada6fe472cb3410d002f3704cbaea70c4aca196bb326eaaf4f8ec0982`.
Evidence manifest SHA-256: `9fe959f15fa85474aa5c477e6e376b9b2e83a1da1d74949b5a874059de4464a8`.
Mechanic evidence SHA-256: `4b2fd3e8aa7b8bcef46b2a9c66cfaca8567d09c7d42061f7c633a2f5d5ec08cc`.
Profile transaction SHA-256: `f1a610f6d91720f4e679d5fafe220f20512f59f15e70918a32b4189956cf32ad`.


Each profile restored the original Mods manifest, settings and managed
SoundBank exactly. An independent post-run full manifest comparison also
passed. Restored Mods manifest SHA-256:
`5fd25324af892572107c960fc8189bc7de04352b20cc201dfb3de4e35b43cc13`.
Encoding is SHA256 of UTF-8 PowerShell
`originalManifest | ConvertTo-Json -Depth 6 -Compress`.
Full per-file evidence/log/transaction hashes are in
`releaseCCrystallineDevelopment.qualifiedCore` in the mission STATE.

Proven on all three Oread heritages: ray-only native AC and cleanup; current
owned provider/mode/resource; opt-in/cancel and unchanged standard/move/swift
budgets; accepted Ray of Frost deflection and zero damage; spent-use rejection;
miss and Snowball exclusion; ordinary rest; native fact deactivate/reactivate,
marker removal/re-add and exact cleanup; occupied hands and free-hand recovery;
blindness/paralysis exclusion and restored awareness.

Only request-local asynchronous projectile arrival was isolated. Actual native
commands, projectile creation, attack rolls, OnHit notifications and subsequent
native effects ran. The fixture never invokes the trait handler or damage
effect directly as its proof.

## Diagnostic history and limitations

The first three failed candidates remain in the development ledger. The next
KMG-only candidate passed 5,069 assertions, but its combined run stopped because
the foreign Shadow Evocation Greater Elemental Assessor identity
`e50e2db3d78b7ff4aa5c9699ba26febe` had an area-target action graph and no native
projectile delivery. The final test proves that pairing this context with a ray
weapon grants +0 AC; it does not reconstruct or alter that foreign spell.
KMG-only has 34 positive ray witnesses plus five negative controls per heritage;
combined has 33 positive ray witnesses plus six negative controls.
The foreign replacement's actual area-effect mechanic remains NOT-RUN here.

The next equipment test retained a detached activatable after actual native
respec removed/recreated it. Six assertions failed. Reacquiring the current
owned activatable resolved those failures in both final profiles; production
rules were not changed to satisfy the fixture.

Still required: four unresolved native story-effect classifications, remaining
two-handed/disabled-hand boundaries, native multi-ray/duplicate-event checks,
non-damage ray effect suppression, save-backed consent/spent-resource state,
module-OFF load, complete lifecycle and final compatibility matrices.
The older seven-trait save transaction and pinned 0.0.114 migration remain
historical proof of their actual scope, not proof of this new deflection graph.
Visual Adjustments is absent and NOT-RUN.

No campaign save was accessed by these runs. No package, proprietary asset,
raw runtime artifact or machine setting is committed. Nothing was merged,
tagged, publicly released or made into a PR.

