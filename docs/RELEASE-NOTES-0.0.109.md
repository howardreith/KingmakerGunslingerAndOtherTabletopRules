# Kingmaker Gunslinger 0.0.109

Release archive:
`KingmakerGunslinger-0.0.109-martial-performance-repair-notifications.zip`.

## Martial Performance custom weapons

- Call of the Wild's exact Martial Performance selection now receives the
  active Pistol, Musket, Blunderbuss, Elven Branched Spear, Wakizashi, Katana,
  and Nodachi choices exactly once.
- Native choices and their order remain intact. Enabled custom choices form a
  deterministic tail; disabled feature-module families do not leak into the
  selection.
- Each child mirrors the native Dagger donor's Weapon Focus effect and uses
  the authoritative weapon-proficiency rule. Native before-level-up and preview
  descriptors continue to control eligibility, so a genuinely proficient
  character can commit a choice and a non-proficient character cannot.
- The optional provider is discovered only by GUID
  `19d1ff4cf70845d094b0ec231473e97f`, exact blueprint type, and internal name.
  Absence is inert; a changed contract fails closed and rollback restores the
  original selector arrays exactly.

## Loaded firearm repair

- Ordinary repair now accepts a loaded Broken firearm. A successful repair
  consumes exactly one appropriate repair kit, changes the firearm to Normal,
  destroys all rounds held by that exact firearm, and clears its loaded-
  ammunition identity. No ammunition is refunded to inventory.
- Empty Broken firearms retain the established successful behavior. Rejected,
  failed, or rolled-back operations preserve the exact condition, round count,
  ammunition identity, and repair-kit count.
- Wrecked recovery remains Wrecked to Broken through overhaul, followed by
  ordinary repair to Normal. Successful overhaul is empty and does not emit a
  degradation notification.

## Broken and Wrecked feedback

- A committed Normal-to-Broken or Broken-to-Wrecked degradation now adds one
  concise native transient warning such as `Akasa's Pistol is now broken.`
  after the existing combat-log entry.
- The adapter calls the inspected Kingmaker 2.1.7b API
  `Kingmaker.UI.Common.UIUtility.SendWarning(System.String)`, whose native event
  route terminates at `Kingmaker.UI.WarningsText`.
- Prevented, ignored, unchanged, failed-commit, rollback, hydration,
  reconciliation, and recovery paths do not notify. A notification failure is
  logged softly and cannot undo the committed firearm state.

## Compatibility and qualification

The implementation has no compile-time dependency on Call of the Wild. It
retains the exact-item firearm state model, stable blueprint identities, all
native and foreign Martial Performance choices, the Wrecked recovery
progression, and the existing combat-log publication.

Optional Craft Magic Items compatibility remains reflection-only; the release
does not link or package `CraftMagicItems.dll`. The unchanged production
firearm SoundBank remains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

The complete deterministic suite contains 1,348 tests, including 11 focused
Martial Performance cases, 22 ordinary-repair cases, and 9 condition-
notification cases. This release also retains the 1,325-test 0.0.108 release
baseline and all inherited validators.

It retains the 1,288-test 0.0.103 baseline, the 1,307-test 0.0.104 summon
repair, the 1,315-test 0.0.105 presentation baseline, and the 1,323-test
0.0.106 fatigue-authority baseline.

The behavior passed guarded Steam runtime qualification on its implementation
artifact. Exact 0.0.109 metadata-promotion requalification is pending before
publication. Native level-up rendering, visually observed top-of-screen
placement, and a live loaded multi-round repair interaction remain supervised
presentation checks and are not represented as proven by automated tests.
