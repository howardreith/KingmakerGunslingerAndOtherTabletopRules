# Satyr locomotion and Lillend presentation repair

Owner packet: `Kingmaker_v0.0.169_Repair_Packet.zip`, received October 28,
2026; SHA256 `9d3ec37f20a4659b5fdb7b016ce5dd32ee5a7d3492eb11ff359b724020d29daa`.
Mission text SHA256 `1a0b9af1f0bd0b63e75acdd401dea7d8f0fd82af9545c89c886d53c8c4379491`.
The archive and three original reference images were extracted to ignored
`artifacts/repair-packet-0.0.169/`. They are reference inputs, not revised art.

Accepted baseline: master `71ea55cda5`, including published v0.0.169 source
`d4e1f656971bd2010110c331fa51f65cdf919f7b` and release records PR78.
Review branch: `codex/satyr-lillend-presentation-repair`, machine DATA.
No merge or release is authorized by this follow-up. The Sprint32 release
and its owner waiver remain historical facts.

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
still returned PASS46/46 and therefore demonstrably did not validate walking.

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

## Implemented repair, pending final native execution

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
not yet a native gait PASS.

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

Previous translation-only presentation PASS remains bound to its original
artifact and is not stepping evidence. New results will be recorded below.
