# Sprint 67 entry criteria: install, update, removal, and compatibility

## Required contract

- The standalone package contains one dedicated installation and compatibility
  guide in addition to the existing README and smoke-test guide.
- Installation names Unity Mod Manager and forbids source/private/compiler
  archives and manual file overlays.
- Updates require a save backup, complete folder replacement through the mod
  manager, version verification, and no downgrade claim.
- Removal warns that saves can retain references to custom class, feature,
  item, and state-token blueprints. No uninstall cleanup or save safety is
  claimed.
- Compatibility claims are limited to the qualified Kingmaker 2.1.7b runtime,
  the declared Unity Mod Manager baseline, no bundled dependency, and known
  high-risk integration surfaces. Unqualified third-party-mod compatibility is
  not claimed.
- Strict package validation rejects a package missing the guide or containing
  any unapproved extra file.

## Qualification

Focused documentation checks, repository validation, the complete domain
suite, a clean Release build, and strict standalone package validation must
pass. This documentation/package-only checkpoint does not require a new game
launch because it changes no assembly, blueprint, or runtime input.
