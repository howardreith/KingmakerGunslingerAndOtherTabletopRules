# Sprint 87 exact starting-firearm binding

Patch only native `LevelUpHelper.AddStartingItems(UnitDescriptor)`. Before the
call, snapshot shared-inventory object references only for a descriptor whose
exact maximum class is the registered Gunslinger. After the call, require the
same receiver and exactly one newly created weapon whose blueprint is the exact
production Early Pistol. Bind its engine-issued item GUID to the attached
descriptor unit's stable ID through the save-owned immutable carrier.

Never infer origin from current wielder, pre-existing pistols, value equality,
blueprint counts, or inventory order. Ambiguous/missing receiver, inventory,
blueprint, item, or identity evidence fails closed. Broad level-one creation
Commit remains excluded.

Focused source checks, repository validation, all domain tests, clean Release
build, strict package validation, exact mod load, and a guarded starting-item
scenario with binding assertions are required for runtime qualification.
