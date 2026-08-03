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

Four candidate model sets are approved and preserved byte-for-byte under
`assets-source/third-party/models`: *Flintlock pistol* by Cyril43,
*Blunderbuss Low Poly* by ccotwist, *1851 Colt Navy Revolver* by Steven
Jurriaans, and *Flintlock Rifle* by Mesh Masters. Each is CC-BY-4.0; distributed
source or derivatives must retain the creator, title, source URL, license link,
and change notices in its adjacent records.

The advanced-rifle folder now
claims **Martini Henry rifle** by **ASHISH (Ashish0096)** under CC-BY-4.0, but
its FBX embeds the contradictory Maya filename `fusil winchester v3.mb` and no
ASHISH, Ashish0096, or Martini-Henry identifier. The license evidence therefore
does not yet establish that it covers the exact local binary hashes. This is a
single file-to-source identity gap, not a second candidate or attribution to a
different Winchester creator. See
`C:\Dev\KingmakerGunslingerLab\ASSET-INTAKE-AUDIT.md` for hashes and the exact
evidence required to clear quarantine.

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

The six 0.0.62 semantic icons are original AI-assisted project artwork. Their editable source sheet and processing record are under `assets-source/original-icons/second-playtest/`.
