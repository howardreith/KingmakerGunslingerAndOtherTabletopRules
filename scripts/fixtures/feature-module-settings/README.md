# Staged FeatureModules.json fixtures

`schema10-mixed-off.json` is a VALID older-schema settings file carrying two
deliberate OFF values (`acadamae-graduate`, `urban-barbarian`) among nine ON
ones. Defaults are all-ON, so if a migration failure ever caused defaults to be
substituted those two would come back ON and the startup observation would fail
on the module gates rather than passing quietly.

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
