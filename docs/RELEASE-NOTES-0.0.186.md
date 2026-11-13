# Tabletop Added Rules v0.0.186

This stable release is a repair tranche from an independent audit of v0.0.182. The largest item is a
sign error that made scatter shots and Dead Shot **more** accurate instead of less. Nothing was added
and nothing was removed: every identity from v0.0.185 is unchanged and none is appended, so existing
saves keep their references, and the summon roster and its 1,208 public choices are untouched.

## Firearms hit less often than they have been

Two attack penalties were applied with the wrong sign and were being added as **bonuses**.

- **Scatter.** A scatter volley is meant to take a **-2** attack penalty. It has been giving **+2**.
- **Dead Shot.** The follow-up attacks are meant to step down the attack bonus by 5 each
  (full, -5, -10, and -15 at high level). They have been stepping **up** by the same amounts.

Both now apply as penalties. If you use either, expect noticeably fewer hits than before — this makes
them behave as documented, and as the tabletop rules intend.

## Your module settings can no longer be wiped by a failed save

If this mod's `FeatureModules.json` was from an older version and the game could not write the updated
file — a read-only file, an unwritable `Mods` folder, or an antivirus or backup holding it open — the
mod treated the file as corrupt. It renamed your settings aside, turned **every module back on**, and
told you the file was malformed. The file was perfectly valid.

Now a failed write is reported as a failed write. Your settings file is left exactly as it was, nothing
is renamed, and the values you chose stay in effect for the session.

## Gunsmithing no longer loses money or items when a craft fails

Crafting ammunition is a single transaction: pay gold, receive the batch, use up the day's crafting.
If anything failed partway, the rollback could be wrong in two ways, both fixed:

- Items that had already been placed in your inventory could be left behind while the gold was
  refunded, or
- the rollback could remove **more** than the craft created, deleting matching items you already owned.

A failed craft now puts back exactly what it took and nothing else. If something it does not own is in
the way, it leaves it alone and says so rather than removing it.

## Teleport scrolls you cannot actually use now look unusable

A teleport scroll group whose reader cannot be chosen was being offered as a normal, clickable choice
that then did nothing. The row is still **visible** — it is a scroll you own — but it is greyed out and
tells you why when you click it. Nothing is spent, and the row becomes usable again by itself as soon as
a reader can be chosen, for example after a party change.

## Cord of Stubborn Resolve

The Cord now runs on a single substitution rule rather than two that had drifted apart, and its outcome
is clamped when you are at or below 0 hit points, so it can no longer convert damage into a negative
amount.

## Other mods' level-up choices are preserved

If another mod adds its own kind of entry to a level-up selection this mod also extends, that entry is
now kept in place and in order. Previously this mod's handlers could silently drop any entry they did
not recognise. No installed mod is known to be affected — this is a compatibility guarantee, not a fix
for a reported break.

## Shield Other description

The in-game description said the bond depends on staying within close range. It does not, and never did:
close range applies when you **cast** it. The text now matches the behaviour.

## Existing saves

Nothing in this release changes, adds or removes any identity, so saves load exactly as before. The
firearm changes apply to new attack rolls only; nothing is recalculated retroactively.

## Disclosed open items

- **Inherited summon templates.** Twelve summoned creatures are listed as needing an inherited
  celestial or fiendish template stripped. That list is hand-maintained and disagrees with the game's
  own data in both directions — some creatures that apply a template are missing from it, and some on it
  apply none. This is **unchanged from v0.0.185**: the membership in this release is identical, so
  nothing here makes it better or worse. It is tracked separately and is not fixed by this release.
- **Two feature-module self-checks fail on a heavily modded installation.** They also fail on released
  v0.0.185 with identical values, so they are not caused by this release. Their cause has not been
  traced. They are internal checks and do not affect play.
- **Not observed in game.** The Cord clamp at non-positive hit points; the summoning composition change
  being behaviour-neutral for every possible caller, which is reasoned from the source for the one route
  that was measured; and another mod genuinely supplying an unrecognised level-up entry, since none does.
- **Earlier items.** The disclosures in the v0.0.185 notes still stand.
