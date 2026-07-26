# Environment fingerprint

Runtime evidence must identify the exact local assemblies rather than merely saying “Kingmaker 2.1.7b.” Storefront builds and loader installations can differ.

`scripts/fingerprint-environment.ps1` records:

- Game installation path, storefront, displayed game version, and executable file version.
- Host operating system, process architecture, and PowerShell version.
- Assembly identity, file version, size, and SHA-256 for the Kingmaker, Unity, Unity Mod Manager, and Harmony files used by the build.
- The explicitly supplied enabled-mod list.

The output defaults to `environment.json`, which is ignored by Git. A redacted copy may be attached to bug reports when local paths are sensitive.
