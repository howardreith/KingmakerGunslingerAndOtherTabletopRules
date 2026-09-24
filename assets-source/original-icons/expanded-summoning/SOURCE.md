# Expanded Summoning original icon sources

The first 77 high-resolution PNG sources were generated specifically for this
repository with OpenAI's image-generation capability. No source image was
supplied. They contain no Owlcat, third-party-mod, downloaded, public-domain,
or otherwise externally sourced pixels.

Phase 1 (charter Sprints 3-8) adds concepts without that capability. Each of
those sources is rendered by `tools/render_creature_icon.py` in Blender 4.5:
the creature is built from metaballs and mesh primitives with procedural
materials, lit with a warm key, a cool rim and a low fill, in front of a
radial atmospheric backdrop inside a dark bronze ring, in the roster icons'
framing. No game pixels, downloaded model, texture or generative-model output
is an input, so the renders are reproducible from the script alone. Rows in
`prompts/icon-prompts.json` carrying `"generator": "blender-procedural"` are
these; their `subjectPrompt` is the composition the script realises. Sprint 3
added Pony, Horse, Owlbear and Cyclops this way; Sprint 4 added Shambling
Mound, Giant Flytrap and Purple Worm.

`prompts/icon-prompts.json` records the shared art direction and the distinct
subject prompt for every concept. `tools/New-ExpandedSummoningIcons.ps1`
performs the deterministic 128 by 128 RGBA export without modifying these
sources and writes both provenance and runtime manifests with SHA-256 hashes.

The project owns and distributes these generated assets as part of the mod.
Each distinct creature concept has its own source and finished icon; the same
creature deliberately reuses that icon across spell families and quantities.
