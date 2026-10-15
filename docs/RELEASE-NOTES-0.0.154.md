# v0.0.154 — Gunslinger animal-companion eligibility

`0.0.154-gunslinger-animal-companion-eligibility`

This patch stops animal companions from taking Gunslinger levels. Kingmaker
decides whether a class can be chosen with the class's own prerequisites, and
every native player class carries one that refuses a unit with Animal class
levels. The Gunslinger class had no prerequisites, so a companion levelled
through the ordinary class selector could take it. Gunslinger now carries that
same native Animal-class exclusion.

| Character | Gunslinger selectable |
| --- | --- |
| Standard animal companion | No |
| Player character | Yes, unchanged |
| Humanoid companion | Yes, unchanged |
| Existing Gunslinger | Yes, keeps levelling |

The selector itself appears only when another mod, such as Call of the Wild,
levels companions through the ordinary class screen. Gunslinger keeps
Fighter's selector presentation: where that screen lists classes a character
cannot take, Gunslinger is listed unavailable beside Fighter rather than
removed.

The class identity, catalog entry and save data are unchanged; no blueprint
is added. A companion that already took a Gunslinger level keeps it and loads
normally, but cannot gain another. Call of the Wild's Eidolon, Phantom and
Dragon companion classes add their own exclusions before this mod registers
Gunslinger, so those companions are not covered by this patch.

Native prerequisite and class-application behavior was checked against the
installed Kingmaker 2.1.7b assemblies. The patch was not observed on a real
companion's level-up screen before release.
