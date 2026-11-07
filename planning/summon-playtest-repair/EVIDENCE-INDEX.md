# Evidence Index — Owner Manual Playtest

> Adopted from the private mission package. The thirteen images are **not** in
> this repository; they stay private and local under
> `C:/Dev/KingmakerGunslingerLab/Summoned-Creature-Repair-Mission-Package/summon-repair-mission-package/evidence/`
> (hashes in that package's `MANIFEST.json`).

The thirteen PNG files in evidence/ are unchanged copies of the images supplied with the November 7, 2026 manual report. MANIFEST.json records original attachment names, portable names, byte counts, and SHA-256 values. These are owner observations on DATA, not a new automated acceptance run. The images do not identify the loaded package/DLL.

| ID | File | What is directly visible | Limit / relevant issue |
| --- | --- | --- | --- |
| E01 | `01-dire-rat-initiative.png` | Dire Rat in scene/log and a wolf image in initiative. | UI-01. Does not establish which portrait binding or binary caused it. |
| E02 | `02-stirge-bird-portrait.png` | Stirge/Giant Centipede scene, bird initiative image, successful Stirge touch attack in log. | CUT-01; historical UI-01 lead. A still cannot prove attachment jitter/drain timing. |
| E03 | `03-cheetah-sprint-null-fields.png` | Sprint tooltip has literal <null> under Duration and Saving Throw. | UI-06. Owner reports working mechanics. |
| E04 | `04-lantern-blue-protrusion.png` | Golden Lantern orb with a dark teal/blue protrusion and generic helmet initiative image. | PRES-04, UI-01. Cause and motion not established. |
| E05 | `05-lantern-blank-ray-icons.png` | Named Lantern Archon, protrusion, two blank white action icons. | UI-02, PRES-04. Confirm actual action IDs against Light Ray/Twin Light Ray report. |
| E06 | `06-obscuring-mist-installed-spell.png` | Obscuring Mist in Leinna's spellbook: target point, medium range, 20-foot burst, one minute/level, standard action, no SR. | SPELL-01. Proves available spell, not provider identity. |
| E07 | `07-unnamed-blast-action.png` | Blank action icon and tooltip missing name/effect description. Hover says Summoned Ghaele; log includes Trumpet Archon. | UI-03. Trace actual selected owner/GUID; do not misattribute from hover alone. |
| E08 | `08-glabrezu-middle-arms.png` | Glabrezu's small middle-arm roots appear detached; particles partially obscure the body. | ART-01. Compare clear idle and movement poses. |
| E09 | `09-nalfeshnee-and-ice-devil.png` | Nalfeshnee and Ice Devil at gameplay zoom; rounded orange Nalfeshnee, pale Ice Devil segments/dark torso, donor-looking initiative portraits. | ART-02, ART-03, UI-01. Current DATA visual baseline exists. |
| E10 | `10-unholy-nimbus-ui-and-save-spam.png` | Nimbus blank icon, null Duration/Save fields, repeated Nalfeshnee save messages. | UI-04, UI-06, LIFE-02. Nimbus's Daze description does not establish it caused Frightened. |
| E11 | `11-frightened-save-detail.png` | Target Nalfeshnee; Effect Frightened; DC 22; success. Nearby Ice Devil and rectangular Wall. | LIFE-02. Source is not identified by the target's name. Wall polish is superseded by CUT-02. |
| E12 | `12-nalfeshnee-clear-model-and-fear.png` | Clear Nalfeshnee front/side model, Ice Devil, stacked Frightened save floaters. | ART-02, ART-03, LIFE-02. A still does not measure repeat cadence. |
| E13 | `13-irresistible-dance-installed-spell.png` | Irresistible Dance in installed spell list: touch, 1d4+1 rounds, Will partial, SR, mind-affecting/compulsion, standard action. | SPELL-03. Resolve provider and preserve intended Pixie frequency separately. |

## Evidence handling

Keep owner images and raw runtime evidence private/local. Do not commit them to the public repository merely to link a PR. Repository planning notes may refer to private evidence locations and summarize outcomes without publishing the images.

For future checks, record candidate source and loaded binary identity, command/actor IDs where relevant, and actual frame/clip paths. A UI screenshot supports visible output; command or damage data supports mechanics; sound and motion require appropriate clips or direct observed playback.

Do not replace a missing native observation with an offline preview or a test count. Do not use old release images as evidence for changed appearance. Earlier results retain their own candidate identity and remain useful only for unchanged inputs.
