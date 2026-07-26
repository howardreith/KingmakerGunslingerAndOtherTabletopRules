# Kingmaker 2.1.7b item-enchantment reconciliation observation

The exact private `Assembly-CSharp.dll` supplied for this project was inspected locally to determine why quicksave made a loaded Test Musket appear empty in version 0.0.21.

The relevant private instance method is:

```text
Kingmaker.Items.ItemEntity.ApplyEnchantments()
```

Its observed behavior removes a runtime enchantment when both of the following are true:

- the runtime enchantment has no `ParentContext`; and
- the enchantment blueprint is not present in the item's built-in blueprint-enchantment collection.

The Sprint 21 loaded-state token was an item-owned dynamic enchantment with a null context, so native reconciliation treated it as removable. The analysis informed two Sprint 22 safeguards:

1. create new state tokens with a `MechanicsContext` whenever the item exposes a current owner or wielder; and
2. surround the native reconciliation call with an exact-token prefix/postfix that restores only one known token that was observed before and removed afterward.

This file intentionally records only the behavior needed for the project decision. It does not reproduce or redistribute Owlcat assembly code.
