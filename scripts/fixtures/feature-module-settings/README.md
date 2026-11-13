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

**That hypothesis was wrong, and a control disproved it.** Running this scenario
with NO staging at all and every module ON reproduces both failures identically
- `unclassified=38`, and the Brown-Fur gate failing with `active=` all true.
They are a property of THIS INSTALLATION, which carries ten other mods
(CallOfTheWild, RacesUnleashed, TweakOrTreat, CraftMagicItems and others)
contributing 38 unclassified host summon rows and `foreign:14` archetypes. The
gate hard-codes `unclassified=0`, so it cannot pass on a multi-mod installation
regardless of settings.

So the wider mix would have been fine too. `shield-other` is kept because it is
the version actually verified, and one OFF value is sufficient to prove
non-substitution. The two failing gates are **not** evidence about the settings
migration in either fixture, and neither gate is claimed to be defective - they
are calibrated for a single-mod installation.

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
