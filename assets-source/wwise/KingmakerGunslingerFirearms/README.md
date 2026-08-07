# Kingmaker Gunslinger firearm SoundBank authoring project

This is the curated Kingmaker Wwise project seed from the
`Owlcat.Templates` 1.14.4 `kmsoundvoicemod` template. Its project and Work Unit
identities target Wwise 2016.2.6 build 6153. The copied Master Mixer is the
template's exact Kingmaker hierarchy; in particular, the native `WEAPONS` bus
is `{90EB9CC7-BB9C-42E0-9B57-62AC34459906}`. Do not replace or regenerate that
hierarchy and never ship this project's generated `Init.bnk`.

The project file is `KingmakerGunslingerFirearms.wproj`. Its name was curated
from `KMTemplate`; its supplied project, platform, Work Unit, and bus IDs were
preserved. The five firearm objects, Play events, and `KMG_Firearms` bank are
not hand-authored in XML because Wwise must create their object IDs.

Preparation:

```powershell
.\scripts\audio\Prepare-FirearmWwiseSources.ps1
```

Preparation preserves four approved processed WAVs byte-for-byte. For the
Blunderbuss only, it removes exactly 2.180 seconds (104,640 frames at 48 kHz)
from the approved processed source so the measured blast transient begins
near time zero while retaining its reverb tail. `source-map.json` records both
the approved parent and deterministic derived SHA-256 values.

## Minimal Wwise 2016.2 GUI completion

1. Open `KingmakerGunslingerFirearms.wproj` in Wwise 2016.2.6.6153. Decline any
   project conversion if a different Wwise version opens it.
2. In **Audio**, import the five prepared files from
   `artifacts\wwise-source-staging` as five **Sound SFX** objects in the
   Actor-Mixer Hierarchy. Use the event/source pairs in `source-map.json` as
   the authoritative mapping. Allow Wwise to copy the media into `Originals`.
3. For each sound, set **Output Bus** to the supplied `WEAPONS` bus. Leave
   **Stream** disabled. Use uncompressed PCM conversion for the first release.
4. For each sound, use Wwise's **New Event > Play** command and rename the
   event to its exact `KMG_Firearm_*_Shot` name from `source-map.json`. This
   lets Wwise create every new GUID.
5. In **SoundBanks**, create one bank named exactly `KMG_Firearms`. Add all
   five Play events to it and confirm their media is included in the bank.
6. Save all Work Units, select only **Windows** and `KMG_Firearms`, then
   generate SoundBanks. Do not curate or package `Init.bnk`.

After saving, run the authoring validator. It fails until all five events and
the bank exist:

```powershell
.\scripts\audio\Validate-FirearmWwiseAuthoringProject.ps1 -RequireAuthoredObjects
```

Then generate the bank from the installed 2016.2.6 CLI:

```powershell
& 'C:\Audiokinetic\Wwise_2016.2.6.6153\Authoring\x64\Release\bin\WwiseCLI.exe' `
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

The CLI's `-help` invocation did not return in a bounded noninteractive run, so
the generation flags remain an execution-time check: use the GUI's generated
command/log if this documented 2016-era command shape is rejected. Never use a
newer Wwise generation as a substitute.
