# Asset provenance

## First-playtest icon set

- Source/author: original artwork generated specifically for Kingmaker
  Gunslinger by OpenAI image generation at the project's direction on
  2026-08-02.
- License: project-owned generated output; no third-party source artwork was
  supplied or copied.
- Source files: `assets/source/icons/playtest-icon-sheet-original.png` and
  `assets/source/icons/playtest-equipment-strip-original.png`.
- Modifications: mechanically divided into cells, resized to 128 by 128 pixels,
  and flat green backgrounds converted to alpha.
- Game exports: `assets/game/icons/*.png`.
- Attribution required in distribution: none.

No icon in this set was extracted from Kingmaker, Wrath of the Righteous, or
another mod.

## Second-playtest intake

Five model sets are approved and preserved byte-for-byte under
`assets-source/third-party/models`: *Flintlock pistol* by Cyril43,
*Blunderbuss Low Poly* by ccotwist, *1851 Colt Navy Revolver* by Steven
Jurriaans, and *Flintlock Rifle* by Mesh Masters. Each is CC-BY-4.0; distributed
source or derivatives must retain the creator, title, source URL, license link,
and change notices in its adjacent records. The fifth is *Winchester lever
action rifle* by Killian Delias (`Killian_Delias`), Sketchfab model UID
`678f6e091d7149da8fce413b6fd31288`, CC-BY-4.0. The user attested that the exact
six preserved local files came from that source; their hashes match the
corrected provenance record, and embedded `fusil winchester v3.mb`,
`FusilALevier`, and `fusilALevier` names agree with the Winchester/lever-action
identity. Former Martini-Henry/ASHISH records remain under `provenance-history`.
The Winchester derivative is permitted only for Advanced Rifle.

## Third-playtest SSE audio intake

The user supplied all five original recordings beneath the exact
`incoming-assets/audio/sse-library-guns/original` provenance folder and
explicitly mapped that folder to *SSE Library: GUNS* (USC Cinema / Sunset
Editorial Collection, Internet Archive upload by Jason Scott), CC0-1.0. Each
file is valid mono 48 kHz/24-bit PCM and has no clipped samples. Originals are
preserved byte-for-byte under
`assets-source/third-party/audio/sse-library-guns/original`; deterministic
16-bit PCM runtime derivatives and their complete hashes are recorded in the
adjacent `audio-manifest.json`. Credit is retained as project policy.

No mlsulli/Freesound binary was supplied, processed, mapped, or required.

The Windows bundle `kingmakergunslinger.firearms` contains derivatives of the
five approved CC-BY-4.0 models and the five approved SSE CC0 recordings.
It was built with Unity 2018.4.10f1; input and output identities are recorded in
the adjacent model/audio records and `assets/bundles/asset-bundle-manifest.json`.
Meshes were normalized for scale, grip, forward axis, muzzle marker, and Standard
shader materials. Audio was trimmed/normalized conservatively and converted to
mono 48 kHz 16-bit PCM. The Winchester derivative maps only to Advanced Rifle.

The 0.0.64 Rapid Reload icon is original AI-assisted project artwork generated
specifically for Kingmaker Gunslinger with the built-in OpenAI image tool. The
preserved 1254-by-1254 source, prompt intent, and processing record are under
`assets-source/original-icons/fourth-playtest/`. The deterministic exporter
`tools/New-RapidReloadIcon.ps1` removes the green chroma, despills, crops, and
resamples the 64-by-64 game asset. No third-party source art was supplied or
copied, and distribution attribution is not required.

The six 0.0.62 semantic icons are original AI-assisted project artwork. Their editable source sheet and processing record are under `assets-source/original-icons/second-playtest/`.
