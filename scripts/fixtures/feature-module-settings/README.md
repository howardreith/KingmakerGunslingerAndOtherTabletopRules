# Staged FeatureModules.json fixtures

`schema10-mixed-off.json` is a VALID older-schema settings file carrying one
deliberate OFF value (`shield-other`) among ten ON ones. Defaults are all-ON, so
if a migration failure ever caused defaults to be substituted it would come back
ON and the startup observation would fail on that module's gate rather than
passing quietly. One OFF value is enough to prove non-substitution.

**Why only `shield-other` - and a correction.** The first version turned off
`acadamae-graduate` and `urban-barbarian`, and both runs failed two unrelated
gates: `feature-module-expanded-summoning-publication-gate` with
`unclassified=38` against a hard-coded `unclassified=0`, and
`feature-module-brown-fur-publication-gate`. I narrowed the fixture believing
those OFF values caused it.

**That hypothesis was wrong, and a control refuted it.** Running this scenario
with NO staging at all and every module ON reproduces both failures identically
- `unclassified=38`, and the Brown-Fur gate failing with `active=` all true.

**What that control does and does not establish.** It was run on the SAME
repaired candidate, so it establishes that **staging is not required to
reproduce the failures** - which is what the fixture question needed. It does
NOT establish that they predate the repair or that they originate solely in the
installed mods: a candidate-wide regression would also appear in that control.
Attribution remains OPEN unless separately established by a matched baseline
comparison or a precise source trace.

The ten other mods present (CallOfTheWild, RacesUnleashed, TweakOrTreat,
CraftMagicItems and others) and the hard-coded `unclassified=0` in
`ExpandedSummoningBoundaryExpectation` are a plausible explanation, not a
demonstrated one.

So the wider mix would have been fine too. `shield-other` is kept because it is
the version actually verified, and one OFF value is sufficient to prove
non-substitution.

The two failing gates are **not** evidence about the settings migration in
either fixture, and neither is claimed to be defective. Their CAUSE is also not
established: "calibrated for a single-mod installation" is one candidate
explanation, and this README previously stated it as fact. The control cannot
separate it from a candidate-wide regression, so the gates remain open
obligations with attribution unsettled until a matched baseline comparison or a
precise source trace decides it.

It omits `teleportation-spells` and `magic-circle-spells`, which schema 10
predates. `FeatureModuleSettingsStore.ReadDefaultOn` gives an absent key ON, so
both are expected ON after migration - that is the documented behaviour for a
key the older schema did not have, not an accident of this fixture.

Used by `Invoke-ExpandedSummoningRuntimeScenario.ps1
-StageFeatureModulesPath`, which replaces the deployed tree's settings file
after deployment and before launch, and restores the original BYTES AND
ATTRIBUTES afterwards. With `-StageFeatureModulesReadOnly` the staged fixture
is marked read-only so the migration write fails; the attribute is applied to
the staged fixture only and is always cleared before the tree is restored, so
the owner's real settings file is never left read-only.

Expectations for both cases:

| Parameter | Writable | Read-only |
| --- | --- | --- |
| `expectedSettingsSource` | `migrated-schema-10` | `migrated-schema-10` |
| `expectedMigrationWriteFailed` | `false` | `true` |
| `expectedRecovered` | `false` | `false` |

`expectedRecovered=false` in BOTH cases is the point of the repair: a failed
migration write must not be reported as a malformed file, and must not cause
valid settings to be replaced by defaults.
