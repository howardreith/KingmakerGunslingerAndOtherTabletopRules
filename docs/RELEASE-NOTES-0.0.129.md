# Kingmaker Gunslinger 0.0.129

This owner-authorized full release contains the completed
`0.0.129-recall-and-smart-scrolls` update.

- Word of Recall is available as an ordinary level-6 Oracle choice when the
  character gains one. Real native level-up at Oracle 11 to 12 verified candidate
  selection, cancellation, completion and casting from the newly learned spell.
  The existing Oracle list repair is retained; Oracle 13 to 14 grants no new
  ordinary level-6 choice in the tested Call of the Wild configuration.
- Word of Recall casts immediately after source selection and arrives quietly,
  like Greater Teleport. Its destination remains the established capital, or
  Oleg's Trading Post before establishment. Ordinary Teleport keeps its risk
  confirmation and arrival/mishap messages.
- Each equivalent scroll group has one compact action and a shared stock count
  on desktop and controller. The best eligible traveling-party reader is chosen
  automatically using the supported native activation checks. Distinct scroll
  variants remain separate, and spell-slot casts retain explicit caster selection.
- Failed scroll activation names the actual reader and spell and reports the
  verified scroll or charge outcome. One click authorizes one attempt; there is
  no automatic retry or fallback to spell slots. Native preservation and reusable
  item contracts are respected.
- Teleportation scroll publication also works when Call of the Wild is absent.
  Existing modal, stale-resource, repeated-input, destination and spending guards
  remain in place. Spell/scroll identities, Cleric 6 and Druid 8 are preserved.

The accepted implementation passed 1,632 domain tests, 520 guarded native
assertions in the installed mod profile, and 22 additional startup assertions
with Call of the Wild absent. The release workflow repeats the complete suite,
clean deterministic builds, strict 135-file package validation and focused
native checks on the final versioned binary. Candidate and release identities
are recorded separately in the
[mission evidence](../Z-RECALL-AND-SMART-SCROLL-STATE.md).

NOT RUN: Sayan's exact Oracle 13-to-14 favored-class/archetype selection and
save-backed gameplay with Call of the Wild absent. Native UI text, containment,
focus and event behavior were qualified; final pixel screenshots were black
and do not establish visual acceptance. Unsupported activation modifiers are
not assigned invented probabilities; an unresolved comparison fails closed.

Download `KingmakerGunslinger-0.0.129-recall-and-smart-scrolls.zip`.
The unchanged firearm SoundBank SHA-256 is
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
`CraftMagicItems.dll` remains optional external software and is never bundled.
Historical suite checkpoints of 1,288 and 1,325 cases remain attributed to their
original releases; the current suite contains 1,632 cases.
