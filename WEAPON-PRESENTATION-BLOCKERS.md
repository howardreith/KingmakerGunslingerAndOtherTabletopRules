# Weapon Presentation Calibration Blockers

## Hard blockers

None as of 2026-08-21.

## Open evidence gaps (not hard blockers)

- The unchanged baseline has exact stored and held-idle imagery for all 22
  production variants. V5/V6 supersede the V4 long-gun defects with accepted
  held, stored, combat-ready, and acted-fire evidence for all three production
  long guns on one default Medium male. V8/V9 supersede V7's branched-spear
  defects with accepted held, stored, combat-ready, and sampled-thrust evidence
  for all three variants on the same fixture. All 15 acted samples lead with
  the physical tip, custom support-hand averages match native Longspear, and
  grip is exactly on `R_WeaponBone`. V10/V11 now supersede the baseline Eastern
  defects with accepted held, independently stored, combat-ready, and sampled-
  attack evidence for all 12 production variants; E3 passes all 30 Eastern item
  identities and protected mechanics on the same exact artifact. V12 adds
  accepted locomotion, 90-degree body-relative turning, and active native
  equip/unequip transitions for firearms and spears, but its clean review also
  exposed detached inherited donor scabbards on custom Eastern clones. The
  clone-only sheath repair is now qualified by E4/V13/V14: all 30 custom items
  are sheath-free, all native family donors retain their sheaths, every custom
  stored prefab remains visible, and all 12 Eastern variants are accepted in
  the captured default-Medium-male stored/motion/transition states. Clean
  exact-commit E5/V15/V16 reruns now supersede those dirty-source repair runs.
  Firearm reload, handgun ready/fire and valid dual wield, armor or cloak
  interaction, female, Small, and Enlarged coverage remain ordinary open work.
- Native donor axes have been measured by exact live controls (`V2`), and all
  production authored assets now expose complete secondary-axis markers under
  a shared fail-closed contract. Branched-spears additionally have
  mesh-grounded FBX markers, donor-derived held/stored bases, and held-only IK.
  Eastern weapons now additionally have renderer-grounded FBX grip/tip/butt,
  blade-normal, cutting-edge, and stored anchors; full donor-basis conversion;
  exact held/stored pairs; and held-only Nodachi IK at the native Greatsword
  butt-side station.
- The stale firearm donor readiness assertion is repaired and the guarded
  observer passes. Crossbow donor forward/up and support-locator positions are
  explicit in `V2`; V6 supplies native-control combat-ready and acted-attack
  observation. Across all ten ready/attack samples, Musket's support-hand
  average is `0.131895 m` versus the native control's `0.132578 m`; V12 accepts
  locomotion/turning/transitions on the default Medium male, while reload
  remains open.
- Service Pistol and Revolver source frames have renderer/component proof and
  basis-derived transforms. Musket, Blunderbuss, and Rifle now use deterministic
  canonical source frames, measured trigger-wrist grips, renderer-bound ends,
  identity equipment roots, the measured Heavy Crossbow held basis, and
  independent BackMount prefabs. V5/V6 accept their captured default-Medium-
  male states without changing projectile semantics.
- Medium female, Small, and Enlarged cosmetic fixtures may require narrower
  request-local character construction. Lack of first-pass coverage is not a
  stop condition.

## Rejected stopping rationales

The initial Eastern observer timeout, stale firearm observer expectation, the
first Nodachi null slot-offset failure, a post-build artifact-timestamp race,
the first static wrapper deadline race, and the transition matrix's generic
wrapper deadline were ordinary engineering tasks.
The first Nodachi failure was isolated to native sheath recreation and corrected
at that checkpoint by initializing an empty custom slot-offset collection
without moving the equipment root. A later clean transition matrix proved that
retaining the donor sheath on custom clones could render it detached. Because
all 12 variants have complete independent stored prefabs, the new narrow repair
clears the sheath only on custom clones; native donors remain unchanged.
Post-repair E4/V13/V14 pass structurally and visually; clean exact-commit
E5/V15/V16 now supersede them. The original transition matrix continued responsively
after its wrapper deadline, flushed a complete structured PASS, and exited
automatically; the later clean run supersedes that wrapper race.
No protected save was selected or overwritten, and no launch-environment safety
condition was encountered.
