# Magic Circle implementation and acceptance

The frozen installable candidate passed the feature-specific qualification
sequence below. It is ready for owner review within the recorded profiles and
scopes. Reduced-profile saved-world coverage remains environment-limited.

## Baseline and authority

- Repository: howardreith/KingmakerGunslingerAndOtherTabletopRules.
- Base: 21a1c4b25a61f481f76b8fb0d57bd7fc4a5698dc.
- Branch: codex/magic-circle-alignment-spells; dedicated worktree.
- Candidate: 0.0.132 / 0.0.132-icon-art-overhaul.
- Sprint 30 prerequisite: BUILD-QUALIFICATION-S30.json records native passes.
- Owner decisions: all four exact 128px exports approved; bearer death ends its
  circle. [Decision record](../reports/magic-circle/OWNER-DECISIONS.json).
- Steam App 640820 and guarded requests only; named Working save only. Baseline
  and campaign saves are never written. No merge, release, tag or force-push.

## Implemented contract

[Rules, settings, classes and acquisition](../docs/MAGIC-CIRCLE-SPELLS.md)
describe four separate level-three standard-action touch abjurations. Each
creates a moving native ten-foot emanation for ten minutes per original caster
level, covering bearer, allies, enemies, pets and summons. Native +2 deflection
AC and +2 resistance saves remain alignment-conditioned and use typed stacking.

The existing Protection component, catalog, source resolver, alignment policy,
unknown-source behavior and `protection-from-alignment-control-immunity` setting
remain the single control authority. Only new qualifying matching-source control
is blocked. Existing domination stays active. Ordinary defenses remain when the
enhancement is disabled, and descriptions match the startup configuration.

The original caster supplies level, metamagic, expiration and dispel attribution;
the touched bearer supplies position; the incoming controller supplies alignment.
Native carrier/area ownership keeps overlaps independent. Recipient buffs have
proximity lifetimes and cannot be separately dispelled. Bearer death ends its
circle; unconsciousness and original caster death do not end a living bearer's
circle. Permanent removal ends the bearer's area, while temporary scene unloading
can reconstruct it from the original carrier without restarting its timer.

Stable identities and finite vendor definitions remain registered OFF for native
hydration and purchase memory. New casts, list access and saved-vendor grants are
gated. Inert scrolls can remain visible OFF, including at unvisited suppliers,
following the existing Teleportation convention. This is not uninstall safety.

Deferred: new saves/morale and removal/suppression/resumption of existing control;
summoned-creature exclusion/contact/SR barriers; inward circles, binding and
diagrams; blanket descriptor immunity; silver-dust bookkeeping; unrelated systems.

## Final evidence

[Final candidate qualification](../reports/magic-circle/FINAL-CANDIDATE-QUALIFICATION.json)
maps the acceptance requirements to actual assertions, native profiles, exact
artifact identities, result hashes and guarded save identities.

| Check | Result |
| --- | --- |
| Repository validation | PASS |
| Complete clean domain/reflection suite | 1,657 PASS; zero failures; 8,192 settings combinations |
| Compatibility parameter checks / focused request guards | 8,227 PASS / PASS |
| Clean Release compilation | PASS |
| Strict standalone package validation | PASS |
| Full saved-world mechanics, enhancement ON / OFF | 268 PASS / 268 PASS |
| Native desktop UI, enhancement ON / OFF | 62 PASS / 62 PASS |
| Optional absent / CotW present native profile | 67 PASS / 67 PASS; save-free scope |
| Eight-carrier persistence preparation | 13 PASS; one authorized Working write |
| Fresh loads under all four content/control combinations | 10 PASS each; exact cross-launch comparison |
| Native area reload and world-map roundtrip | 22 PASS; no writes |
| Exact cleanup / fresh absence | 12 PASS / 3 PASS; one authorized cleanup write |

The frozen ZIP SHA-256 is
`fc440dbfb38d5496f6afeaa8cc97fd61fc85d27b928fc85a5c5b181707781d75`.
DLL SHA-256 is
`a5f8f6a759c672d367ae00a794dd013bb8075b1595e4dd9c3c2519209cac6057`;
MVID `ea9ac240-4984-421b-b2e1-4336fa5770c6`.
Local package: `artifacts/local-runtime/0.0.132/KingmakerGunslinger-0.0.132-local-runtime.zip`.

The DLL embeds 75faf15ace983a225af1e0bc7d0faa605e77237f and was compiled from
its validated working tree, source-state SHA-256
`f3b6db20350d823283082130eae5a415698fb1ec8f3637bb5cc69fb0bfb2329b`.
That embedded commit alone is not the final source identity. The final record
provides raw and canonical fingerprints of all 1,387 frozen product inputs;
later evidence/documentation edits do not alter those tested inputs.

Native game is 2.1.7b. Installed Assembly-CSharp SHA-256 is
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
RuleApplyBuff.Initiator is the recipient; the existing handler is unchanged.

## Visual acceptance and limits

[Native visual review](../reports/magic-circle/FINAL-NATIVE-UI-REVIEW.json)
records direct inspection of all 42 control-ON screenshots and eight control-OFF
carrier/recipient tooltips. Structured native checks cover all twenty visible
consumers and descriptions in both configurations. Owner approval remains tied
to the exact export hashes. Protected unrelated artwork is unchanged.

Native recipient rows say Permanent and target summaries say burst. Explicit
Within Circle names and authored proximity/moving-emanation descriptions explain
the actual behavior. No global UI patch is added.

Saved-world qualification uses the original eleven-mod installed stack. Reduced
stacks fail in native Player.PostLoad before Circle execution on the existing
Working save. [Environment record](../reports/magic-circle/REDUCED-PROFILE-SAVE-PREREQUISITE.json)
keeps those failures; [optional profile evidence](../reports/magic-circle/OPTIONAL-NATIVE-PROFILES.json)
qualifies bounded real native casting/control/lists without a saved-world claim.
The named `gunslinger-qualified-combined` profile has only three mods and is not
the original installed stack.

The broad settings observer remains FAIL for three unchanged Teleportation scroll
icon comparisons. [Separate attribution](../reports/magic-circle/UNRELATED-SETTINGS-OBSERVER.json)
records the pre-existing protected-art mismatch; no validator is weakened and no
unrelated code is repaired. No all-mod, gamepad UI or uninstall-safety claim is made.

The earlier checkpoints, red/green regressions and fixture failures remain under
`reports/magic-circle/`; [implementation history](../reports/magic-circle/IMPLEMENTATION-HISTORY.md)
retains the earlier detailed record. Raw packages, screenshots, saves and machine
artifacts are local only. Publication and normal-play restoration are recorded
in the [feature handoff](../docs/MAGIC-CIRCLE-HANDOFF.md).
