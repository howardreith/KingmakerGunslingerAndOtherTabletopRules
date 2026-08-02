# Sprint 75 entry criteria: evaluated hit die and skill points

Installed `ApplyClassMechanics.ApplyHitPoints` IL gives a player-faction base
class its full hit die plus one at level one and half hit die plus two at later
levels. For d10 Gunslinger this is base HP `0 -> 11 -> 18`. Installed
`LevelUpState` calculation uses class skill points plus Intelligence contribution;
with Intelligence fixed at 10 the Gunslinger must expose 4 points at levels one
and two.

Use only a detached native unit and exact `CharGen`/`LevelUp` apply paths. Do not
commit, load a save, or touch shared inventory. Require full source/build/package
qualification, exact mod load, and two fresh save-free PASS runs.
