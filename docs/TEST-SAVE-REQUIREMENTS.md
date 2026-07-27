# Sprint 30 test-save requirements

Use a disposable save containing one established firearm-test character in a
safe, controllable location where movement can interrupt a full-round action.
The character must be selectable, able to receive Firearm Proficiency, and able
to equip the Test Musket and a genuine native Heavy Crossbow.

Required inventory and state:

- two distinct Test Muskets;
- one genuine native Heavy Crossbow;
- at least two Firearm Repair Kits;
- at least one Black Powder Charge and one Lead Ball;
- diagnostics capable of preparing the exact equipped target as empty/Wrecked
  and the independent second Test Musket as empty/Normal;
- Reload, Overhaul, Repair, qualification-matrix, item identity/revision,
  token-reconciliation, fault, and duplicate diagnostics.

The exact starting sequence is: native Heavy Crossbow isolated with no firearm
state; then exactly one Test Musket equipped, second Test Musket in shared
inventory, qualification baseline cleared, fixture prepared, target
empty/Wrecked, second item empty/Normal, resources present, and
`overall=PASS; stage=FixtureReady`.

Name the immutable baseline clearly with version and purpose, for example
`KMG_S30_BASELINE_DO_NOT_OVERWRITE`. Create a separately named working save for
every run. Never quicksave over or otherwise overwrite the baseline.

A save is unsuitable if its identity is uncertain; it is an active campaign
save; prerequisites, resources, diagnostics, or second item are missing; the
target state cannot be proved; unrelated mods/errors affect the scenario; the
location cannot safely interrupt actions; cloud conflict is present; or the
baseline has been loaded after an unexpected mutation.
