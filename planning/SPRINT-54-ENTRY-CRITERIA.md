# Sprint 54 entry criteria - Menacing Shot

## Authority and exact contract

The mission-authorized local Gunslinger rules source defines Menacing Shot at
level 15. It spends exactly 1 grit, shoots a firearm into the air, and affects
all living creatures within a 30-foot-radius burst as though subject to the
fear spell. Its save DC is `10 + floor(Gunslinger level / 2) + Wisdom modifier`.

## Required installed-contract observation

Before implementation, a save-free guarded observer must resolve the exact
installed Kingmaker 2.1.7b Fear spell blueprint and record its delivery,
saving-throw, descriptors, conditions, duration, immunity handling, target
filtering, and DC components. It must also identify the narrowest native
self-centered 30-foot burst mechanism. Names or contracts from another Owlcat
game are not evidence.

The observer must establish whether the native spell includes allies and the
caster and how it excludes nonliving targets. If the installed spell's normal
shape or spell-level DC differs, Menacing Shot may reuse its condition/save and
immunity mechanics but must supply the authoritative radius and Gunslinger DC.

## Firearm and transaction boundary

"Shoot a firearm into the air" requires exactly one equipped, loaded,
non-Wrecked exact firearm and consumes one item-owned chamber without an attack
roll or weapon damage. The implementation must preserve the existing firearm
condition because no natural attack roll exists to produce a misfire. Grit and
the chamber must change atomically: a rejected delivery changes neither, and a
late delivery fault restores both.

## Qualification

Pure tests must cover level, grit, firearm, living-target, radius, DC, and
atomicity gates. A guarded save-free scenario must prove one chamber and one
grit spent, exact native save/condition behavior for pass and fail targets,
native immunity and nonliving exclusion, radius boundary, ally/self behavior,
cleanup, and external isolation. The exact assembly then requires mod-load and
two independent feature PASS runs.

## Non-goals

Sprint 54 does not alter the native Fear spell, invent anatomy or morale
categories, create a weapon attack or damage roll, or implement Slinger's Luck
or later deeds.
