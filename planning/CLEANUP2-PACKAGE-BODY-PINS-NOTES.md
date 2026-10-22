# Cleanup 2 current package export admission

2026-10-21. Technical integrity checkpoint. Owner visual approval remains
pending; package validation does not grant it. This changes only the current
`scripts/validate-package.ps1`, its new focused no-game test, and these notes.
Historical release validators and prior PASS records are unchanged.

## Exact admitted exports

Hashes were read directly from project-authored exports and matched against
`CLEANUP2-ARTHROPOD-HANDS-NOTES.md` and `CLEANUP2-LARGE-CREATURES-NOTES.md`.
Both mesh and albedo are pinned together. Girallon's painting is unchanged.

| Creature / asset directory | Mesh SHA-256 | Albedo SHA-256 |
| --- | --- | --- |
| Gorgon / `ungulates` | `ab559e49083e95085056bbc1588d9d2f3515758599a79f6ced4e983f2c5479a3` | `f6cf501d0f8817ed327e7275736cda78bb5684f3270adee1e7c6448c5a32c9f2` |
| Girallon / `sprint19-fourarmed` | `e5c4e59878ff6aae7b704f8fb49925e849f348817fc160e97a1f76907ee683cb` | `21e970e238983c79971709e05d562c3b04b5534a2019eeb55b491ff31fc0dfe6` |
| Giant Crab / `sprint21-arthropods` | `34833285f367258ed5d00127c72d86bb96897df94459740d3c9fd800e720893e` | `1182b139b3a8dc049eaa68d7db8d9bcf865d02e321af2e8d13b46688866409ce` |
| Griffon / `sprint27-griffon` | `e3db5d71b8f4c2bf4647bbcfdcea1b2438f4a060ad3d6c1ddc3ec6649af3eab9` | `1ccd2ade87b48b1162910270395c32c294c7e84c20c3f961b8e720b40eddd124` |
| Giant Stag Beetle / `sprint14-insects` | `2ecf222413eb857051b70df8b5fd61592a939108c7661ccdfb96f20fbcea794d` | `967d506b96019914fd40363cc80b21b0ce08f80662ef96ef259f5e0a8caecde4` |
| Stegosaurus / `sprint23-armored-dinosaurs` | `4ea887c98387d90d546e5512cf862cd47cb5694b0bbb1f99431c83c1a12c95ef` | `27ef9768948c6f28a2836a28cfa45e9be03220b8f590d15af656212e540511bf` |
| Elephant / `sprint25-giant-bodies` | `5afe5464331a083c0de63d7b4bee4a1266d71376fb8360f7d96b0bf7a4f13028` | `5a973962a26dbf38208b66feeb5ffd154d469ac69b25137ae487b9d6767f2919` |
| Brachiosaurus / `sprint25-giant-bodies` | `9bd237aad9b1a99f25162e4a109814b5f72fb9da6167d2f000e88f333e24a974` | `198b61966762624adc3ffdfb5975fe08bfc7ad4589fc6e0f318e7bbb06d30776` |

Existing Xill, Scorpion, Bebelith, Ankylosaurus, Ape and Dire Ape pairs retain
all twelve previous exact pins. The separate Salamander human/tail pin remains
unchanged. The full package allowlist, binary rules, icons and SoundBank checks
are unchanged. The production entry point directly invokes its curated internal
`Assert-KmgProtectedBodyExports`; no pin parameter, purpose override or bypass
was added.

## Focused evidence

Follow-up after the Gorgon native audit and authored repair: the same
production pin validator now checks the eighth revised pair, including the
actual retained prechange Gorgon mesh refusal. **126/126 PASS, 2.8844252 s**,
Windows PowerShell 5.1; evidence
`artifacts/cleanup2-package-body-pins/6ccaf4a5ae404542abee2e9f5df7d156/test-summary.json`.
This is technical asset admission, with native repaired appearance and owner
approval still pending. The older seven-pair results below retain their scope.

From the active `summon-cleanup-iteration2` worktree, Windows PowerShell
5.1.19041.7802:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Cleanup2PackageBodyPins.ps1
```

**116/116 PASS, 2.0212011 seconds.** The test executes the actual production
function and hash helper with private copies of all protected files. It checks
exact source admission, missing/wrong bytes for all 26 pinned files, all seven
revised cross-wired albedo refusals, twelve unchanged control literals, and
refusal of actual retained prechange Griffon, Elephant and Brachiosaurus mesh
inputs. Evidence is ignored local output at
`artifacts/cleanup2-package-body-pins/24e37078b34f482096dbadaa7a1dc158/test-summary.json`.

The untouched failed review1 ZIP was then checked offline by the current full
validator:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/validate-package.ps1 -PackagePath C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger/artifacts/worktrees/summon-cleanup2-review1/artifacts/development-candidates/summon-cleanup-iteration2/827983eb7485/KingmakerGunslinger-0.0.163-development-summon-cleanup-iteration2-827983eb7485.zip -Configuration Release
```

**PASS, exit 0, 9.1835486 seconds**, including the production firearm manifest
and SoundBank validator. ZIP SHA-256 before and after validation:
`a6e7188f5eb28878e6c2754656d2ae69812ece4d1af859335a1f58f952d22ed6`.
Extraction and cleanup occurred only in the validator's private temporary
directory. No receipt was issued; this older source candidate remains invalid
for deployment. A later candidate must rebuild from the new clean commit.

No build, game, installation mutation, Git mutation, BASE change or frozen
worktree edit was performed. Native behavior, presentation acceptance and owner
approval are not established by these checks.
