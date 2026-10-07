# Sprint 11 ungulate summon-choice source paintings

These four square 1254 px PNG paintings were created with the built-in
`imagegen` tool on 2026-09-28 for the project's `painted-creature` icon family.
They are original source candidates; four 128 px exports are now packaged for
technical review, while the summon choices remain hidden. No Kingmaker or
third-party art pixels were used. The existing Dog,
Horse, Giant Wasp, and Stirge source paintings established the family frame,
dark backdrop, scale, and warm lighting; Paizo anatomy supplied the species
identities. Source inspection is complete; downsample, live menu, and owner
visual review remain pending.

| Species | Source SHA-256 | Editable composition brief |
| --- | --- | --- |
| Aurochs | `474e36b58c79b7b6a9a84c1e4be2faee2a70fc8e8cc1d46b98c43c70d55f7138` | Large muscular dark ox with pale outward-swept horns, broad muzzle and heavy neck. Low three-quarter approach, bronze circular rim, warm side light and dark earth behind. Keep both horn tips and the shoulder inside the square and readable at 128 px. |
| Bison | `cfbb414662a491465edff64717ee4a9c18274a9ed8fb03e97c20414772ef0ac1` | Humped, shaggy brown bison with a dark beard and shorter upturned horns. Distinguish its massive furred forequarters from the smooth Aurochs. Same restrained frame and warm rim light. |
| Rhinoceros | `c9ca0a0e1cce68a011382744193140b8a9cc8c33c0b9885fecd9c335d2896e93` | Bare gray rhinoceros, weight on four feet, a long nasal horn and smaller brow horn, thick folded skin, dust and stone atmosphere. No wool or bovine horns. Strong forward charge direction. |
| Woolly Rhinoceros | `20a9b45ac806ba26f9880cb8baa79a33ff12f343e6ca023387c9cc796bbadb34` | Distinct shaggy winter rhinoceros with two horns and reddish-brown long hair in a cold muted setting. Larger visual mass than the bare Rhinoceros; keep the two horns separate at 128 px. |

The composition briefs record the subjects and reference roles. The original
tool-call wording was not retained in this source file, so these briefs must
not be represented as verbatim generation prompts. The byte-identical source
PNGs are the editable raster masters. `New-ExpandedSummoningIcons.ps1`
downsampled the four paintings through the existing 128 px RGBA production
pipeline. The exact export SHA-256 values are:

| Species | Export SHA-256 | Manifest consumers |
| --- | --- | ---: |
| Aurochs | `578e235e53f5b3573e53d1cb953ae4c4ff140bb236cc5d26038c08ce802df62a` | 29 |
| Bison | `60064766af512af9af69d5c90723a886d6c3938c47167b828d4759cf6b3f44a5` | 25 |
| Rhinoceros | `4e0b2774a8b947ca2a4f4a9cecd8e7d2346eeb9bc5700728358e3a970c20e995` | 25 |
| Woolly Rhinoceros | `6c4fca4860efe04b48c4fd38639d8b6093bf71f6e61b88dcb2f0131948606c41` | 21 |

Guarded Steam inventory `20260929T0218259420008Z` passed 50/50 assertions,
including unchanged published menus and no missing icons; it is not a direct
ungulate UI-view or owner visual approval. At publication, validate actual UI
use and technical versus owner-visual status. Creature choices
require `original-required`; quantity/template variants use
`intentional-family-share`. Parent spells retain their existing art, and any
mechanics-only trample/charge helper may use `hidden-internal` only when it has
no player-facing command or condition.
