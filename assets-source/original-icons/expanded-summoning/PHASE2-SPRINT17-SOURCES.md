# Sprint 17 snake icon sources

Created 2026-10-06 with the built-in image-generation tool, following the
current summon-creature painted family in `docs/ICON-ART-GUIDE.md`. Actual
project Dog, Eagle, Crocodile and Salamander sources were inspected as family
references. No game pixels, copied third-party artwork, frame or text were
included. The exact generation prompts are retained as `subjectPrompt` in
`prompts/icon-prompts.json`; the two generated originals are retained intact.

| Concept | Source SHA-256 | Composition |
|---|---|---|
| Viper | FE2A97B57A70262298BA19B8130A2F5A4191E2EB9E64F22C96E9415B6AC1C55B | Lean olive-bronze coil, S-neck, triangular head and two upper fangs |
| Constrictor Snake | DC4247E626D83143B4DD896AC795F8170ADC998691C705C95401E50F7BFF10FD | Thick tan/brown saddle-marked coils, compact mass and modest closed head |

The original paintings are 1254-square opaque PNGs. The established exporter
produces 128-square RGBA game icons; source/export hashes and every exact
unit, inspectable type, logical ability and template child are delegated to
`icon-manifest.json`. Registration is hidden pending full Sprint 17 runtime
qualification. Art inspection and source checks do not qualify actual UI use.

Consumer dispositions: both creature identities use their new paintings;
their hidden CombatProfile facts and Viper poison trigger have no independent
player-visible image. Viper Venom retains the exact native poison-condition
icon from its cloned lifecycle, not a Purple Worm portrait. Constrictor's
grab/constrict carrier uses the existing internal hold presentation and adds
no selectable action. The existing Salamander painting, assignments and all
protected artwork remain unchanged. No source-art replacement is authorized
by this addition.

Offline review inspected the unscaled 128-pixel exports, 64/48/32-pixel
reductions and a 48-pixel grayscale panel. The slender upright viper and
heavy constrictor coils remain distinct at every size; the smallest cells
retain the head/neck versus broad coil silhouettes without relying on color.
The reproducible review helper is `tools/New-Sprint17SnakeIconReview.ps1`;
its nonshipping artifact SHA-256 is
`223195FD43900CEC79C3A64A79F11B47A96E111D587FBD5A03448A3BDAAB7F7D`.
This is agent asset inspection, not human approval or native UI evidence.

HumanReview: NOT_PERFORMED_NONBLOCKING. Runtime UI review: NOT QUALIFIED.
