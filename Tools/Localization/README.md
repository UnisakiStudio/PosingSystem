# v3.2.0 localization authoring

This is development work, not a published release. The language list is provisional.

## Authoritative data

- `ui.translations.json`: Inspector display text. Japanese source text is the lookup key, with a small set of semantic keys for header navigation.
- `menus.translations.json`: common expression-menu controls and the five pose categories.
- `navigation.translations.json`: help and documentation navigation.
- `poses.translations.json`: the complete 128-name set from both products, translated together for consistent naming. Original pose names remain serialized unchanged.
- Generated Unity data lives in `Packages/jp.unisakistudio.posingsystem/Editor/Localization`. Do not edit generated catalogs directly.

Run `python Tools/Localization/build_catalogs.py` from this repository, then `python Tools/Localization/validate_catalogs.py`. Neither command uses a network service. The builder consumes machine-authored batch data; it does not call a translation model. To generate more translations, provide the entire pose table, existing target-language terms and these rules as one context rather than translating each pose independently.

Use numbered variants consistently. Keep left/right markers consistent within each language. Prefer short natural names over literal accuracy. Never translate file paths, asset GUIDs, Animator parameters, pose keys or error codes. Preserve composite-format placeholders. Changes to a Japanese lookup key require updating its translations.

`validate_catalogs.py --max-name-length 20` optionally screens Unicode code-point counts. This is an editorial check, not a verified VRChat SDK limit or a rendered-width test. The final display budget remains undecided. Indic shaping, Thai glyph placement, right-to-left text and truncation need visual checks in Unity and VRChat before release.

## Runtime behavior in the Editor

The Inspector language dropdown uses a user-specific EditorPrefs key independent of Unity's UI language. Without an explicit choice, the OS language is used; unsupported languages use English. Missing translations use the standalone setup catalog's English fallback where available, then the original display text. JSON catalogs are cached and invalidated on import changes. Display-only language changes preserve GUI.changed.

The per-tool `menuLanguage` setting is serialized on PosingSystem. The Inspector and generated game menu share this value. Empty values use the remembered preference or OS language; existing explicit avatar choices are respected. A Generating pass translates only the NDMF build clone before Modular Avatar processes menus, after synchronizing optional features. GameObject names, pose definitions, parameters and animations stay unchanged. Referenced expression menus are copied with shared-reference and cycle handling; owned menu-installer targets follow their copies. External target menus are not translated. Language changes do not invalidate animation prebuild snapshots.

The original name remains editable in pose details; translated names appear in the list and work in search. Custom names without a catalog entry are displayed as entered.

## Scope and verification

As of 2026-11-07: 30 selectable languages including Japanese; navigation and common menus in all 29 target languages; complete pose-name batches in English, French, Italian, Simplified Chinese, Traditional Chinese, Korean, Thai and Hindi. English Inspector coverage is broader than other languages but is not complete. The Indian-language choices remain provisional, not a final interpretation of the user's request.

Seven user guides are in `Documentation~/Localization`. Full existing online manuals have not been translated or published. Source-document translation jobs are prepared separately in the development workspace.

Unity tests cover preferences, catalog availability, fallback, cycles, original-data preservation, nested ownership, inactive objects and installer targets. Development-only integration tests additionally build actual KawaiiPosing and CoolPosing prefabs in English and Traditional Chinese, with and without prebuild. The main development workspace stores test logs and the handoff record under `Recordings/Localization_20261107` and `docs/v3.2.0-feature-candidates.md`.

No version bump, package publication or public deployment is part of this checkpoint. More Inspector strings, full manuals, remaining pose languages, translation review and actual UI/VRChat visual checks remain before release.


## Updated language UX (2026-11-07)

The two language selectors have been unified at the user's request. The header is the only selector. It stores `menuLanguage` on the selected PosingSystem with Undo/Prefab support, and remembers the last choice for tools without an explicit selection. An Inspector language scope uses that same saved value for all display text; the NDMF menu pass resolves the same value. Empty values fall back to the last preference, then the OS language, then English. The old Features-tab selector is removed. Popup changes compare selected indices directly instead of relying on the surrounding GUI.changed state. Earlier descriptions of independent Inspector and menu languages are superseded by this section.


## Setup programs (2026-11-08)

`../InstallerLocalization/README.md` documents the shared EditorPrefs behavior, standalone license/setup translation, native editor menus, upstream MIT source build, and review-only installer packages. The same code works before the main product package is present. License storage compatibility is preserved.
