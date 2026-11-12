# Staged FeatureModules.json fixtures

`schema10-mixed-off.json` is a VALID older-schema settings file carrying one
deliberate OFF value (`shield-other`) among ten ON ones. Defaults are all-ON, so
if a migration failure ever caused defaults to be substituted it would come back
ON and the startup observation would fail on that module's gate rather than
passing quietly. One OFF value is enough to prove non-substitution.

**Why only `shield-other`, and why not a wider mix.** The first version turned
off `acadamae-graduate` and `urban-barbarian`. Both runs then failed two
unrelated gates in this scenario -
`feature-module-expanded-summoning-publication-gate` with `unclassified=38`
against a hard-coded `unclassified=0`, and
`feature-module-brown-fur-publication-gate` - while the per-module gates
themselves agreed exactly with the staged values. Those two gates carry
structural expectations calibrated for the all-ON configuration
(`ExpandedSummoningBoundaryExpectation` hard-codes `unclassified=0` regardless
of module state), so an arbitrary OFF combination fails them for reasons that
have nothing to do with the settings migration. `shield-other` is a single spell
with no archetype and no summon rows, so it cannot move either. The wider mix is
covered by the domain tests, which have no such coupling.

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
