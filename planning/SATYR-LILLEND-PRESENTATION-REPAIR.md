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
LateUpdate. This is a contract-supported hypothesis pending live reproduction.
The first observer-only candidate records pre/post binding handle state and
pelvis-relative joints during actual queued movement. It does not change
production playback or claim gait acceptance.

Previous translation-only presentation PASS remains bound to its original
artifact and is not stepping evidence. New results will be recorded below.
