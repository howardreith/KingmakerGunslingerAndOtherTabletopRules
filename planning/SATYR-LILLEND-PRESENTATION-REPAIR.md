# Satyr locomotion and Lillend presentation repair

**OWNER APPROVED; RELEASE 0.0.170 PREPARATION — October 29, 2026.** The owner
reviewed the repair and wrote: "Alright, this looks good. Please merge, push to
remote, and cut a new release." Art is accepted and normal integration/publication
is authorized. The selected publisher runs the full source/build/package gate
and two deterministic exact-reference builds (30-minute planning budget).
Affected cold save/reload remains NOT_RUN, with its disclosed limitation retained
in the public notes. No new native execution or whole-roster campaign is selected.

The original development checkpoint below retains its exact evidence. Satyr walking is
repaired and native-verified in bow/dagger modes against both native controls.
Lillend's replacement surfaces are implemented and native presentation is
verified. Final candidate passes53/53 native assertions,9/9 selected domain
cases and3/3 Blender export cases. Owner art review is now approved. Affected cold
save/reload remains explicitly deferred by the validation policy; this is not
full release qualification. Current installation/settings and protected save
intake are restored exactly. No merge or release was performed.

### Release preparation follow-up

PR80 merged normally as `6ae2eb273353d9adfe765638d61f98626f636ef8` and master
was verified through the guarded push wrapper. The first ordinary publisher
attempt ran from that exact clean source, 2026-10-29T12:14:54.5433641Z through
12:21:27.0852678Z (392.520 seconds). Repository validation, 1,166 offline runtime
preflight predicates, all2,759 domain cases (53.7 seconds), exact compilation,
37 native-reference eligibility assertions, output and SoundBank validation
passed. Packaging then FAILED: the newly included v170 notes raised the exact
payload from422 to423 files, but package.ps1 still declared422. No release/tag
was created. Retain the original transcript, summary and incomplete exact-build
archive under the original6ae2eb273353 paths; this attempt is not qualified.

The narrow follow-up changes only the declared non-icon count276 to277, preserving
strict complete path/hash validation. Repacking the retained staged payload with
the corrected423 count and strict package validation passed, alongside the
serializer's count/refusal/determinism fixture. This proof ZIP is diagnostic only;
no receipt or release qualification is fabricated. The ordinary publisher must
finish on a new clean merged commit because its immutable candidate/source guards
do not admit the failed attempt's incomplete output as a reusable release build.
Local records: `release170-publisher.log`, `release170-publisher-summary.json`,
`release170-package-count-proof.json` under `artifacts/repair-packet-0.0.169/`.

Local review gallery: `artifacts/repair-packet-0.0.169/review/index.html`;
portable packet: `artifacts/repair-packet-0.0.169/Satyr-Lillend-Repair-Review.zip`.
Editable private Blender scene: `artifacts/repair-packet-0.0.169/lillend-authored.blend`.
[Authored source/export provenance](SATYR-LILLEND-ART-PROVENANCE.json).

Owner packet: `Kingmaker_v0.0.169_Repair_Packet.zip`, received October 28,
2026; SHA256 `9d3ec37f20a4659b5fdb7b016ce5dd32ee5a7d3492eb11ff359b724020d29daa`.
Mission text SHA256 `1a0b9af1f0bd0b63e75acdd401dea7d8f0fd82af9545c89c886d53c8c4379491`.
The archive and three original reference images were extracted to ignored
`artifacts/repair-packet-0.0.169/`. They are reference inputs, not revised art.

Accepted baseline: master `71ea55cda5`, including published v0.0.169 source
`d4e1f656971bd2010110c331fa51f65cdf919f7b` and release records PR78.
Review branch: `codex/satyr-lillend-presentation-repair`, machine DATA.
The owner's October29 follow-up authorizes merge and the next stable release.
The Sprint32 release and its owner waiver remain historical facts.

## Selected work

1. Reproduce planted Satyr feet; compare native archer/melee and summoned
   bow/dagger, inspect the actual locomotion handle, repair only its exact owner.
2. Require bilateral pelvis-relative leg/hoof motion, native start/stop/turn,
   and sequential motion frames in the existing presentation observer.
3. Rebuild Lillend's visible anatomy, auburn hair, teal/ivory garment, layered
   wings and emerald serpent surfaces on the same 26 native + 10 original
   drivers. Preserve identities, mechanics, material route and tail timing
   (1.4 seconds, action at 0.60).
4. Focused compile/export checks and directly affected guarded native
   presentation/cold reconstruction; compact before/after review gallery.

Native saves and the current human installation are protected by the existing
transactional wrappers. A new capture uses the existing exact detached human
bind-palette observer; no native geometry or curves are exported or committed.
Authoring scenes containing those frames remain ignored and machine-local.

## Investigation checkpoint

The installed engine's `AnimationManager.AnimationSet` setter calls
`RuntimeInitializeAnimationSet`, which finishes and removes active handles.
`UnitAnimationManager.TickInternal` creates locomotion only when its cached
`m_LocoMotionHandle` is null. The released Satyr adapter sets a private set in
LateUpdate. Fresh native reproduction confirmed this cause on both summons:
before binding, started=True/finished=False/active=True; after binding and
during travel, started=True/finished=True/active=False. The first candidate
changed observation only, not playback. The old translation-only contract
still returned PASS47/47 overall (46 scenario assertions plus loaded version)
and therefore demonstrably did not validate walking.

Diagnostic source `2fa836efb56a5ed0677cdc1917caffa0c26d3552`, source state
`ed452c20f2e6316c65044363ee7342b0f8370c3964ec43d83a809f9bcb0b7baf`;
ZIP `b6d419adda4718fdc5d25aacbe4d0178561de7f2366b7b3d24e7d235f54d96bf`;
DLL `7c2eeed8f652da615b2457dcb9b15dc3f523039ec6955e1e187e79027445f864`;
MVID `dd8edc3b-1bb6-49c8-9680-5219ca9d8b72`.
Run `20261029T0406373080133Z-bc3d5a1c0f7847348eb4920c1400f0ac`, evidence
directory `20261029T0406372930148Z-disposable-expanded-summoning-sprint32-native-review`.
Profile transaction `compat-20261029T040518Z-1e028af3b038`, 240.290s, exact
restoration verified. No save was selected or written. The existing bind-only
observer reproduced the required private capture SHA256 exactly.

## Implemented repair (final native result below)

Only the exact Satyr adapter invalidates its own cached locomotion handle after
its set assignment has actually finished/removed that same handle. The next
ordinary native tick recreates playback. Restoration uses the same narrow
seam. A changed/foreign handle or foreign set is preserved; no controller tick,
clip/bone manipulation, donor mutation or global reset is introduced.

The existing presentation observer now compares unmodified native archer/melee
controls and fresh summoned bow/dagger moves. It samples both thighs, knees and
hooves relative to the pelvis, requires alternating strides and live native
locomotion, and records six sequential gameplay frames per move plus stops.
Eight selected Sprint32 domain cases pass, including rigid sliding, hopping,
one-leg, nonfinite, sparse and retired-handle refusals. This is offline evidence,
the initial offline checkpoint; final native gait acceptance is recorded below.

Lillend keeps all36 drivers and the exact released tail centreline, action
frames,1.4s duration and0.60s event. Original sculpted skin/face/hands, auburn
locks, garment shells/hem/sleeves/seams, layered feather vanes and revised
emerald/ivory paint replace the draft surfaces. Mesh23116vertices/45408triangles,
1024RGB albedo, at most2 weights per vertex. Both exports reproduce exactly:
mesh `11ad83a0858d95c72627f63c131a713de26c1e9bc83c3c79a84000b9816a8dd7`;
albedo `5148b79042d0e09010269155936c984613c4992b28302999f6caba7779a962d7`.
Three Blender export tests pass (4.315s). Final export/regeneration each took
about5s. Offline views were inspected and corrected before native packaging;
owner art acceptance remains separate.

Commands: `scripts/Invoke-KmgGate.ps1 -Filter expanded-summoning.sprint32`
(compile10.8s, selected tests1.9s); Blender4.5.10 background runs of
`paint_lillend.py`, `generate_lillend.py`, `test_lillend_export.py` with the
exact private capture, explicit output paths and `--python-exit-code 1`.
`scripts/Build-Local.ps1 -DevelopmentPurpose satyr-lillend-presentation-repair
-DevelopmentTestFilter expanded-summoning.sprint32` creates the guarded
candidate; `scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1`
selects gunslinger-only / Sprint32 technical-presentation, runtime600s and
that exact reused package/purpose. No broad Sprint/Tranche gate is selected.

The existing cold fey-celestial flow is documented as60-80min for its two trios,
exceeding the ordinary30-minute checkpoint ceiling. It is not automatically
launched. Exact remaining check: fresh prepare/cold verify-cleanup/fresh absence
for the affected Satyr/Lillend views, with the outer original-Working envelope
and exact restoration. No new cold-reconstruction claim is made here.

Candidate `0fe6c018caf7` passed the wrapper,8 selected domain cases, exact
compilation and37 native-contract assertions. Strict packaging correctly refused
its new Lillend assets against the retained draft pins in `validate-package.ps1`.
The failed build/package/log remain immutable, with no deployment or game run.
The two pins are updated to the reviewed exports; all other package guards stay
in force. Narrow `scripts/validate-package.ps1 -PackagePath <that exact ZIP>`
revalidation passes in9.6s. This reader correction does not create a build
receipt or retroactively qualify native behavior.

Candidate `198198cab65d79ed235359c481cf270eeb6cf285` then passed the guarded
build and strict package validation in222.726s, selected8/8 cases and37/37
native-contract assertions. ZIP `c92b4c0ca2df7b787891f2c098680a4ecde54c615e212b7c40a0ab702c98bf7c`,
DLL `cdbd2a612a531dc80e34097a214118ce56dd74ed99522dbadfb6b89444863a29`.
It was not deployed: review identified the packet's required cast/performance
pose as a missing presentation observation. The same exact phase now supplies
one ordinary self-cure from the real bard book (disclosed5 damage setup at an8m
enemy-clear lane), observes native healing/slot expenditure and active casting,
then captures a normally activated/running Courage performance and stops it
through its native toggle. No manual animation/controller ticks or spell effect
are supplied. Satyr returns to bow through a normal command before the unchanged
combat dagger/bow switch-cost checks. This observer change requires a new binary
and one fresh presentation run. Its initial compile caught a missing namespace
import (8.4s); corrected production compilation10.5s and8 selected cases0.4s PASS.

## Native gait checkpoint and narrowed bow-observer correction

Candidate `d1df29674e3f95d10dbe721974269e7b9f372c95` passed the guarded
build in221.958s,8 selected cases and strict packaging. ZIP
`9f87dcae7e4c09f8bde06483f427879e56113da01277ca9551552afced3d81e0`;
DLL `da2488aff974167fddbb1306a918e927687e97f9af8bd8ce3671f3587516fdd3`;
MVID `69a7b2e1-86f0-4e76-95e3-76186a284870`;
source state `1267b3ef54c4f593355510e46a4a67b5e0fc2806e20a1c1d270655af05adf5c4`.
Fresh native run `20261029T0442578794349Z-9b08336f0a7342699c008375c479bf70`
in `20261029T0442578654360Z-disposable-expanded-summoning-sprint32-native-review`
proved all six gait rows: native archer37/38 samples, native melee38/38,
summoned bow29 and dagger52. Actual frame strips were inspected: bilateral
stepping and ordinary turns are visible. Dagger/gore events and normal costs
passed. This is fresh gait proof for those exact bytes, not full acceptance.

The scenario then FAILED the bow release image's active-handle predicate.
Actual bow command completed Success, normal cost, real3 damage/projectile
impact; exact event/owner/target/manager/frame predicates were true. The native
handle was finished, un-interrupted and acted at1.70008s after the1.66667s
`attackWithBow_b` clip. Installed `UnitAnimationActionHandAttack.OnUpdate`
explicitly marks acted/releases when ActiveAnimation is null after0.1s.
Therefore this is a confirmed observer timing boundary, not evidence of failed
weapon mechanics. The failure is retained; Lillend was NOT_RUN after it.
Native duration110.689s; full transaction190.984s. Exact restoration verified
for `compat-20261029T044150Z-75bd3cecd076` at04:45:01UTC.

Next coherent correction selects only this failing bow observation and the
pending Lillend presentation, using the same bounded existing phase. The observer
now records actual live, weighted native bow playback during that exact command,
captures its visible pose, and permits a later finished/noninterrupted/acted
release only for the same handle and selected clip observed within two frames.
Original active=false facts remain visible; no clip inventory substitutes for
playback, and melee retains synchronous live-contact requirements. New focused
refusals cover missing/stale/future playback, different handle/clip and interrupt.
No gameplay or asset changes follow the successful gait checkpoint. The new
observer requires one affected fresh native run; no unchanged broad campaign,
cold-save trio, provider matrix or release qualification is selected.

## Final native checkpoint

Tested source `44281097448efa9236f20faa2dfd74ee4dd85072`, version0.0.169,
development purpose `satyr-lillend-presentation-repair`:

| Identity | SHA256 / value |
|---|---|
| Source state | `e28a372acb5080d4712fca7a52a8b10124b4c89abb439d7b8694006f0614224e` |
| Candidate ZIP | `e2c180632d5a49a133085e1f2107cb1fb40f0a42d3cc6963f558fc012167981d` |
| DLL | `80446d004a2e83c1224df66e66a9f0ccf5e4b7bba6e7e4cea623bf9c161a3ad5` |
| MVID | `70656ba0-577f-489c-8035-03d5626d411b` |
| Native result | `dd311c22447ee1de75a2a6699b24ac4c8ad976342c449af4f8c1ff451ceceff7` |
| Native progress | `866f9ccf75f51cb1442d66d45c797af0501915003199321f7dca5546735527b0` |

Canonical package/receipt: `artifacts/development-candidates/satyr-lillend-presentation-repair/44281097448e/KingmakerGunslinger-0.0.169-development-satyr-lillend-presentation-repair-44281097448e.zip`
and its `.development-receipt.json`. One guarded build passed in220.526s:
repository wrapper,9 selected domain cases, exact installed-reference compilation,
37 native-contract assertions, strict package validation. This is not a full
domain or release gate. Narrow pre-build compile10.7s/tests1.6s also passed.

Fresh run `20261029T0459315749870Z-aeb68db8ea80453faced5dbec35565fa`,
directory `20261029T0459315599875Z-disposable-expanded-summoning-sprint32-native-review`,
PASS53/53, native168.450s (04:59:38.338–05:02:26.789UTC), full guarded
transaction249.906s. Six live gait cases passed: native archer39/36 samples,
native melee37/39, summoned bow29/dagger56. Both weapon switches, native
dagger/gore/bow events and real action costs passed. The retired bow has actual
weighted `attackWithBow_b` playback at frame5063/time1.62408/weight0.0654,
followed by its exact native release at5064; its visible bow pose is captured
at5030. No event-time active-clip claim replaces that distinction.

Lillend passed independent body bindings, selection, two native moves/turns,
finite moving geometry, real self-cure and running performance poses, sword
and tail contacts, actual wound and native expiry. Return-to-rest, pose continuity,
native before/after views and Satyr motion strips were agent-inspected. No
obvious split limbs/waist or detached weapon grip was seen at the gameplay
camera. This is technical inspection, not owner aesthetic approval. Both native
donors expose no Prone action; death/expiry proof does not claim a played death
animation. Native scheduled removal retires all6 Satyr and62 Lillend owned
resources while58/316 borrowed resources survive; fixture disposals before
those verdicts are0. Final owned-world restoration passes.

Profile `compat-20261029T045822Z-024cebbbd60d` verifies exact restoration at
05:02:32.333UTC, including current Mods, feature settings and CotW settings.
No save was selected, loaded or written. Original failure records remain FAIL;
the success belongs only to the final candidate above.

Command: `scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1
-ProfileId gunslinger-only -Scenario disposable-expanded-summoning-sprint32-native-review
-Parameters @{reviewPhase='technical-presentation'} -RuntimeTimeoutSeconds 600
-ReuseInstalledArtifact -PackagePath <canonical ZIP above>
-DevelopmentPurpose satyr-lillend-presentation-repair -Confirm:$false`.
Local supervised log: `artifacts/repair-packet-0.0.169/repair-native-bow-observer.log`.

Records-only changes after tested source4428109 do not rebuild, requalify or
retag those bytes. Future guarded reuse needs that exact clean source commit
at the package's canonical worktree path; the receipt is not valid on a newer
records HEAD. Cold reconstruction, full profiles/roster/provider qualification
and release work are not selected. Stop at owner art review.

Previous translation-only presentation PASS remains bound to its original
artifact and is not stepping evidence. New results will be recorded below.
