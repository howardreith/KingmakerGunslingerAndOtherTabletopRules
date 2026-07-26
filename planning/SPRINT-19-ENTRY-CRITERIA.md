# Sprint 19 entry criteria

Sprint 19 begins from the observed Kingmaker smoke-test result for `0.0.18`.

## If bootstrap or compile-contract behavior fails

Use the returned `[KMG]` and `output_log.txt` evidence to make the smallest exact-runtime correction. Do not add ammunition or class content.

## If bootstrap passes but persistence remains incomplete

Continue the lifecycle matrix beginning with the A-D fixture, save/load, and process-restart rows. Correct only the persistence carrier or migration path justified by observed evidence.

## If all Critical persistence rows pass

Record a persistence `Go` report. The following sprint may then resume the original feature plan with stackable Black Powder Charges and Lead Balls.

## Required returned artifacts

- The exact installed smoke-test ZIP hash.
- Unity Mod Manager log containing `[KMG]` entries.
- `output_log.txt` for any exception or crash.
- Generated persistence evidence JSON and Markdown.
- A concise note identifying the first unexpected behavior.
