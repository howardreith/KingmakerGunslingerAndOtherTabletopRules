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

**What that control established, and what it could not.** It was run on the
SAME repaired candidate, so it established that **staging is not required to
reproduce the failures** - which is what the fixture question needed. It could
not establish that they predate the repair, because a candidate-wide regression
would have appeared in it too. Attribution stayed OPEN on that evidence alone.

**The matched baseline comparison has since been run, and it settles
attribution.** A development candidate built from released master
`a3912f569ac5` (v0.0.185) - carrying NONE of this campaign's commits - was
deployed to the same machine, in the same session, beside the same eleven
installed mods, and the same scenario was run with no staging and every module
ON. Both gates failed there too, and the gate's own measured values are
IDENTICAL, not merely similar:

| | master `a3912f569ac5` | candidate `468391fc06a3` |
| --- | --- | --- |
| summoning gate | `unclassified=38` vs expected `0` | `unclassified=38` vs expected `0` |
| Brown-Fur gate | `contract:True/identities:25/published:True/selectorRef:1/selectorGuid:1/foreign:14/unique:True/index:6/firstCombined:7/knownCombined:5` | byte-identical |

So the two gates fail on released master without this campaign. They are
**pre-existing**, not caused by the settings repair or by any other repair in
this campaign, and that is now a measured comparison rather than an inference.

They remain **open obligations** - the gates do genuinely fail, and neither is
claimed to be defective. What the comparison decides is only their
ATTRIBUTION. Their mechanism is still not traced: the ten other mods present
(CallOfTheWild, RacesUnleashed, TweakOrTreat, CraftMagicItems and others) and
the hard-coded `unclassified=0` in `ExpandedSummoningBoundaryExpectation`
remain a plausible explanation, not a demonstrated one, and "calibrated for a
single-mod installation" - which this README once stated as fact - is still
only a candidate explanation.

So the wider mix would have been fine too. `shield-other` is kept because it is
the version actually verified, and one OFF value is sufficient to prove
non-substitution.

The two failing gates are **not** evidence about the settings migration in
either fixture.

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
