# Favored Class trait-setting contract — 2026-10-06

Supported inspected assembly SHA-256:
dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c
MVID: 3efd38e7-8682-4b4d-8d53-e368a3664919

ZFavoredClass.Main.Settings exposes a read-only Boolean enable_traits getter.
Its exact compiler-generated backing field is private and initonly:
<enable_traits>k__BackingField. The constructor reads enable_traits and
deity_for_everyone from the host's settings file. Main.settings is a static
Settings field assigned by the host's Load path. There is no enable_traits
setter or native live GUI setting transition in this supported contract.
Ordinary changes to that configuration take effect on a new process.

The DATA publication coordinator reads this exact supported getter through the
existing FavoredClassTraitResolver. Its first post-blueprint callback admits or
withdraws acquisition according to the loaded setting. Stable identities remain
resolvable when the host/traits are unavailable. Elemental Races itself has a
real mutable module notification and immediate exact owned-provider reconciliation.

The closed no-write runtime fixture tests loaded-setting OFF/ON conditions with
a request-local MemberwiseClone of the exact native Settings object. It changes
only that copy's exact Boolean backing field, verifies both getter results,
temporarily lends the copy to Main.settings while paused, and reconciles the
same publication callback. It restores the exact original object reference in
finally. It never invokes the settings constructor, GUI/save callbacks or file
writer, and never changes the original Settings object's fields or configuration
file. Foreign replacement of Main.settings fails closed.

This fixture is evidence for the native getter, conditional publication and
canonical graph reuse. It is not a claim that the supported host provides a live
GUI toggle. The production wrappers preserve optional existing host callbacks;
they do not manufacture a setter or change the native host's startup semantics.

The first added runtime attempt assumed a writable property and failed before
creating actors. PID 26560 exited automatically; its exact live snapshot was
restored and its lease completed. That evidence is retained. The repaired theory
uses the inspected immutable contract, with a private copy and exact reference
restoration. Proprietary binary/disassembly remains ignored and uncommitted.
