# Kingmaker Gunslinger firearm SoundBank authoring handoff

Target authoring generation: Wwise 2016.2.x for Windows. This directory is a
source scaffold, not a generated SoundBank and not evidence that authoring has
occurred.

Project name: `KingmakerGunslingerFirearms`. Create one non-streamed, in-memory
sound object and Play event for each row in `source-map.json`, then assign all
five events to one bank named `KMG_Firearms`. Media must be embedded in that
bank. Do not distribute generated `Init.bnk` or separate `.wem` files.

Use the exact Kingmaker/Owlcat 2016.2 template bus identities. The release goal
is the native SFX/effects bus. Do not invent a bus that requires a replacement
Init bank. If the exact SFX bus identity remains unproven, leave routing as an
explicit authoring/manual-acceptance item rather than generating against a
made-up master hierarchy.

Preparation:

```powershell
.\scripts\audio\Prepare-FirearmWwiseSources.ps1
```

After a genuine 2016.2.x authoring project has been completed, query that
installation's own CLI help and generate the Windows bank. The expected 2016
command shape is:

```powershell
& '<Wwise-2016.2-path>\WwiseCLI.exe' `
  '<project-path>\KingmakerGunslingerFirearms.wproj' `
  -GenerateSoundBanks -Platform Windows -Bank KMG_Firearms
```

Copy only the resulting `KMG_Firearms.bnk` and relevant generated
`SoundbanksInfo.xml` into a temporary review directory. Verify exact event
membership and embedded media, compute SHA-256, then create the production
manifest and curate only the bank plus manifest beneath `assets/soundbanks`.

```powershell
.\scripts\Validate-FirearmSoundBank.ps1
```

The exact CLI flags must be confirmed from the located 2016.2 executable; this
document intentionally does not assert that a newer Wwise CLI is compatible.
