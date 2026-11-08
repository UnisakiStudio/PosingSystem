# Posing System development

Use the primary checkout. The user has requested ongoing multilingual work on
`codex/v3.2.0-localization`, merging the released `main` through 3.1.2.
Commit author and committer must both be `UnisakiStudio <unisaki.studio@gmail.com>`;
retain all existing Git guards. Public release still needs explicit user approval.

For **every feature or fix with display text**, read `Tools/Localization/README.md`.
Update all advertised target languages, including Inspector feature descriptions,
tooltips, dialogs, diagnostics, generated menus and standalone setup UI. Check the
existing glossary/context and avoid inconsistent alternate names for the same
feature. Translate pose names as a complete set per language, preserving numbering
and direction conventions. AI-authored translations require contextual review;
external translation services require explicit user permission.

Edit authoritative `Tools/Localization/*.translations.json`, never generated JSON.
Rebuild catalogs and the standalone setup language helper, run the offline coverage
audit and affected tests, and report remaining omissions explicitly. English
fallback is not completed translation. Key changes require corresponding translation
updates. Preserve placeholders, tags, error codes, package/API names, paths, GUIDs,
serialized values, parameters, original/custom names and license storage.

Before release require zero unexplained missing translation keys, fresh generated
files, language switching and Japanese round-trip checks. Font/wrapping/RTL/Indic
visual verification is a separate release check and must not be inferred from JSON
coverage. Do not run unrelated expensive animator compiler suites for text-only work.
