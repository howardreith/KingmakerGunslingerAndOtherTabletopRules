# 0.0.128-firearm-postrelease-hotfix candidate

Unpublished candidate with passing native mechanical qualification. No public release is authorized.

Repair Firearm uses one brief contextual rejection. Active combat returns exactly
`Cannot repair firearms during combat.` Field repair remains Broken-only,
outside combat, with a reusable Gunsmith's Kit and the same exact firearm.

New native desktop and controller attack orders own their exact actor, firearm,
target and degradation generation. Authority is accepted after native command
submission and survives reload continuation and pause. A committed degradation
cancels the old order and its pending reload. Recovery alone grants no new order.

Review revisions preserve legal coexisting native actions and full-attack
retargeting, including running attack/reload survivors. Hover prediction cannot
prematurely cancel reload continuation. Actual native action rejection and turn
end revoke terminal orders. A TB Move reload can attack in the same turn;
Standard/FullRound reloads cannot borrow an action or promise an automatic
next-turn attack. Normal costs and firearm balance are unchanged.

Validation: 1,622 domain cases, 10 compiled input-wrapper cases, clean Release,
repository validators, strict 135-file package, and 413 native assertions PASS.
See [qualification and evidence boundaries](FIREARM-POSTRELEASE-HOTFIX-QUALIFICATION.md).
Human visual, physical-device and save-backed acceptance remain NOT RUN.
Earlier release waivers do not apply; final committed artifact hashes are in the PR.

Owner smoke: misfire to Broken; verify the old sequence stops; explicitly attack
again with the same firearm, including an empty reload-first attempt and a paused
order; cause another applicable misfire to Wrecked; verify firing and reloading
are rejected; complete a full rest with a participating gunsmith and reusable kit
and verify the same firearm returns to Normal.

The Kingmaker Gunslinger 0.0.128 candidate archive is
`KingmakerGunslinger-0.0.128-firearm-postrelease-hotfix.zip`.
The retained firearm SoundBank (`KMG_Firearms.bnk`) is unchanged, with SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

Retained historical suite checkpoints include 1,288 and 1,325 cases. The active
candidate inventory is 1,622 cases; the complete suite passed with zero failures.
`CraftMagicItems.dll` remains optional external software and is never bundled.

Final review candidate: compiled source `fc6c5cb220f7501fa20fc1709b085fd10b58b7a6`; fresh guarded native run
`20260913T0544024994395Z-62c3a21ad3864b25b953d8da52c67568` passed 413/413 assertions. ZIP SHA-256
`c0c2c0c83db19f2b0bd71cd4393f5a0b09b1a8adadb29a9725d8541d257960e6`; DLL SHA-256
`ce65373e73f40573fc9f715d30ef181da0d5b07db4be38d9be8261b6ce59b8b8`. DLL MVID `2a1e57fe-63ef-49ce-b69c-3cbf7de95372`.
Later evidence-only documentation does not change this qualified artifact.

Legacy batch-reload behavior remains outside this native qualification; the
running-merge proof uses a pistol with a real native paper reload. The earlier
failed exploratory probe is documented in the qualification evidence.
