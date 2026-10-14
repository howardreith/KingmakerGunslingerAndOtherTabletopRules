# Sprint 24 theropod summon-icon sources

Deinonychus and Tyrannosaurus are two separate original summon-choice
paintings created with the built-in OpenAI image-generation tool on October 14,
2026. No input image was supplied to or incorporated by the generation tool.
The approved-family Triceratops, Stegosaurus, and Ankylosaurus source paintings
were inspected locally only as style references before writing the prompts;
their pixels were not inputs. The generated originals remain preserved at
1254×1254 RGB and the established deterministic exporter creates the 128×128
RGBA production files.

Human aesthetic review is `NOT_PERFORMED_NONBLOCKING`. These are internally
reviewed technical candidates; native menu placement remains a separate gate.

## Deinonychus

- Source: `sources/deinonychus.png`
- Source SHA-256: `72cd8953be29a040cf0960dafa981146e8836257cfcd439cf2427874c5df98aa`
- Production export: `assets/game/icons/expanded-summoning/deinonychus.png`
- Production SHA-256: `d7c15ef813f4920efac88a5b1bcd194e1f872af98c5d619dd60758e96aa1dd36`
- Built-in generation record:
  `C:/Dev/KingmakerBuffPlannerLab/codex-home/generated_images/01a138c7-2f6f-7243-aa63-d502758a2fd9/exec-cb502186-3b0b-4b16-b740-a4bd5ae5accd.png`
- Required reading: agile biped; long balancing tail; visible jaw and two
  forelimbs; two powerful hind legs; one enlarged sickle claw on each foot.
- Forbidden reading: Tyrannosaurus proportions, quadruped, cat/ape anatomy,
  armor plates, horns, wings, weapons, extra limbs, text or counters.
- Exact generation prompt:

```text
Use case: stylized-concept
Asset type: Pathfinder-style game summon-choice source painting
Primary request: an original painted portrait of an adult Deinonychus, unmistakably a light agile two-legged predator
Scene/backdrop: rugged prehistoric badlands at warm amber dawn, contained inside an ornate aged-bronze circular fantasy medallion consistent with a classic isometric CRPG creature-icon family
Subject: one lean scaly Deinonychus in a dynamic low pouncing stride, long straight balancing countertail fully readable, narrow predatory head with open visible jaw and teeth, both small grasping forelimbs visible, two powerful hind legs, and the enlarged raised sickle claw on each foot clearly distinct; anatomy must read as bipedal at thumbnail size
Style/medium: richly painted realistic fantasy creature portrait, tactile scales, dramatic but controlled rim light, strong silhouette, high detail
Composition/framing: square; three-quarter side view; creature fills the inner medallion while head, tail, forelimbs, legs, and sickle claws remain legible; generous safe margin inside the circular frame
Lighting/mood: warm golden backlight with darker earthy foreground, alert and dangerous rather than monstrous fantasy exaggeration
Color palette: olive-brown and muted rust scales, amber light, aged bronze
Constraints: exactly one animal; original design; no text; no letters; no logos; no watermark; no gameplay counters; no weapons or armor; no feathers; no quadruped posture
Avoid: Tyrannosaurus proportions, oversized blocky skull, four-legged stance, cat anatomy, ape anatomy, armored plates, horns, wings, extra limbs, duplicated feet or claws, cropped tail, modern scientific labels
```

## Tyrannosaurus

- Source: `sources/tyrannosaurus.png`
- Source SHA-256: `5031201677ad4fc53db7f756ab74054d958427e6175d9f9f9c63915f1703f05e`
- Production export: `assets/game/icons/expanded-summoning/tyrannosaurus.png`
- Production SHA-256: `e80aeaa47e293e74fd6bd589670ff864f4c3375d01f1a8c6c7ca3eb25eb50698`
- Built-in generation record:
  `C:/Dev/KingmakerBuffPlannerLab/codex-home/generated_images/01a138c7-2f6f-7243-aa63-d502758a2fd9/exec-03a2fc33-0220-4f13-9234-4107b037963b.png`
- Required reading: colossal biped; dominant deep skull and jaws; tiny
  two-finger forearms; heavy planted hind legs; thick countertail.
- Forbidden reading: raptor proportions, raised sickle claws, quadruped,
  armor plates, horns, wings, weapons, extra limbs, text or counters.
- Exact generation prompt:

```text
Use case: stylized-concept
Asset type: Pathfinder-style game summon-choice source painting
Primary request: an original painted portrait of an adult Tyrannosaurus, unmistakably a colossal heavy two-legged apex predator
Scene/backdrop: rugged prehistoric badlands at stormy warm amber dusk, contained inside an ornate aged-bronze circular fantasy medallion consistent with a classic isometric CRPG creature-icon family
Subject: exactly one Tyrannosaurus in a grounded three-quarter side stance; enormous deep muscular skull and open crushing jaws dominate the image, visible heavy neck, massive hind legs planted with weight, very small two-finger forearms clearly visible, and a thick long counterbalancing tail; anatomy must remain bipedal and readable at thumbnail size
Style/medium: richly painted realistic fantasy creature portrait, tactile pebbled scales and scars, dramatic controlled rim light, strong silhouette, high detail
Composition/framing: square; low three-quarter camera; head and jaws are the focal point but both tiny forearms, both heavy hind limbs, torso, and countertail remain legible; generous safe margin inside the circular frame
Lighting/mood: dark earth and bronze shadows with a fierce amber horizon glow, monumental and dangerous
Color palette: deep umber, charcoal olive, muted rust, amber light, aged bronze
Constraints: exactly one animal; original design; no text; no letters; no logos; no watermark; no gameplay counters; no weapons or armor; no feathers; no quadruped posture
Avoid: Deinonychus/raptor proportions, raised sickle claws, long grasping forearms, four-legged stance, armored plates, horns, wings, extra limbs, duplicated jaws or feet, cropped head, cartoon styling
```
