# v3.2.0 localization authoring

## Shared language policy (2026-11-12)

All products, avatars, inspectors, setup programs and generated menus use the same
EditorPrefs language for the current OS user on this computer. Legacy serialized
`menuLanguage` values are retained only for compatibility and must never override
or initialize that preference. Language changes must not dirty scenes or prefabs.
Read the preference live, including nested inspector/build calls. Unselected means
OS language with English fallback. Generated menus change on the next upload.

Show the setup language button only on the first confirmation when no valid
preference is saved. Accepting the detected PC language also saves that choice.
Completion/error dialogs use ordinary buttons, even before a choice exists;
subsequent language changes use the Inspector. Keep first-use policy checks across
all languages and restore/isolate preferences in tests.

This is development work, not a published release. The user confirmed all 30 language selections on 2026-11-09.

## Product-owned translations (2026-11-12)

Text that belongs to one product lives in that product's repository and package, not
here. This repository keeps only text shared by every posing product.

- **Product repository**: `Tools/Localization/product.translations.json` (all 29 target
  languages, Japanese source text as key) and `display-names.json` (`poses` and `menus`
  that may be translated in lists and generated in-game menus).
- **Product package**: generated `Editor/Localization/translations.json` (one file, all
  languages) and a small `[InitializeOnLoad]` class that calls
  `PosingSystemLocalization.RegisterProductCatalog(path)`. Do not edit the generated file.
- **Lookup order**: shared catalog, then registered product catalogs, then the embedded
  setup tables. The setup helper reaches product catalogs through `Language.External`,
  so native menu captions defined in product code are translated too. That lookup is an
  exact source match in one direction; product text is never reverse-translated.
- **Ownership**: one owner per text. A name or caption used by both products, or by
  shared code, stays here (`products.problems` rejects a key defined in both places).
  Currently the three pose names used by both main prefabs and the common menu/category
  names are shared; each product owns its name, its menu root, its own pose names and
  the captions that only its code uses.
- **Setup text stays in `Tools/InstallerLocalization`**: license and installation
  dialogs must work before any product package exists, so they cannot move.
- **Tools**: `build_catalogs.py` also writes every available sibling product checkout;
  `validate_catalogs.py`, `audit_coverage.py` and both terminology audits include the
  product tables when the checkouts are present and skip them in a core-only checkout.
  A released product without its own catalog (3.1.x) keeps Japanese product names.

When adding a product: add it to `PRODUCTS` in `products.py`, create its tables and the
registration class, then run the usual build and gates from this repository.

VirtualLove (三点だいしゅきツール, added 2026-11-13) follows the same pattern. Its license
dialogs are composed at run time: the product-specific first line is an exact key in its
own table and the remaining lines are shared setup sentences, which the setup helper
translates line by line. Its two installer menu captions are product text as well.

## Performance and free-text boundaries (2026-11-12)

Use `Text`/`Format` for exact UI keys; use `TextMessage` only for already-formatted
diagnostics. Never parse pose, avatar or user-preset names as diagnostics. Standard
pose/menu display names use the generated `PosingSystemDisplayNameKeys` allowlist
from the union of every language's pose/menu source keys. User presets remain literal.
`Link` and `Section` translate source keys once internally. Use `LinkLocalized`
for an already localized caption (for example a semantic-key header or a measured
prebuild label). Do not add another translation layer around these helpers.
Feature-card descriptions are already localized by `DescriptionFor`; draw them
without passing them through a reverse alias lookup again. A translated word can
coincide with a different source key in another language.

Keep cached notices as source keys and arguments, resolving the current language
when drawing them. The shared Inspector warning callback uses `TextMessage` for
raw warnings and exception details. Cover the actual producers as well as catalogs:
bulk-import candidates/duplicates/completion, preset failures, and motion warnings.
Initial preset confirmation must use `PresetName(name, official)`, just like the
preset browser; never translate a user preset merely because its name matches an
official one. Verify existing state across language changes, including return to JA.
The standalone diagnostic fallback has a bounded cache (1024 entries, at most 4096
characters per source/result); keep preference reads independent of catalog/regex
initialization. Test both translated and untranslated repeated strings for memory
allocation, not only their resulting text. Preserve placeholders and JA round trips.

Select checks by impact: exact UI/diagnostic changes need focused language/input
and performance tests plus the offline audit. Menu/build changes additionally need
a representative full avatar build. Full product/prebuild matrices and storefront
repackaging belong to affected generation/installer changes and release validation;
do not repeat them for every unrelated display fix. Record the checked source and
live-project synchronization separately; a disposable test project's pass does not
establish that a different live package copy is verified.

## Required for every feature and fix (2026-11-09)

The user has confirmed **all 30 selectable languages**, including Japanese. A
feature is not translated just because its English fallback works. Include titles
and ON/OFF explanations as pairs, tooltips, dialogs, diagnostics, native menus,
generated menus, standalone license/install UI and affected guide text. Check the
existing target-language terminology before writing new translations. Preserve
placeholders, markup, identifiers and custom names. Translate all standard pose
names together per language, keeping numbering and left/right conventions.

Use AI with the complete feature/pose context and existing terms. Record provenance
and outstanding linguistic/visual review. Do not silently switch to an external
translation service. `prepare_translation_batches.py --out <directory>` prepares
context-rich missing-key batches locally without network access.

After editing authoritative tables run:

```
python Tools/Localization/build_feature_cards.py
python Tools/Localization/build_catalogs.py
python Tools/Localization/validate_catalogs.py
python Tools/InstallerLocalization/generate_language.py
python Tools/Localization/audit_coverage.py --report localization-coverage.json
```

`audit_coverage.py` is the strict release gate. It compares every selectable target
language with the full English inventory, independently checks feature descriptions,
standard pose names and setup messages, and detects unregistered literal lookup
keys, stale generated catalogs and declared terminology violations. Missing or
fallback text blocks release. `--allow-incomplete` is only for documenting work in
progress, never a passing release check. The CI workflow runs the strict gate.

The display-path scan also checks all dialog arguments, direct GUI/GUIContent
strings, local popup arrays, native MenuItem/CreateAssetMenu path segments and
prebuild/build exception and diagnostic summaries/actions. It also checks NDMF
plugin `DisplayName` getters, including registered Japanese literals that bypass
the translation call. It scans the three
product checkouts when they are available and lists the covered roots in the
report. Exact developer-only diagnostic exclusions require a reason in
`source-audit-exceptions.json`; custom names, asset filenames and language
autonyms remain display invariants. Run `python Tools/Localization/test_audit_coverage.py`
to verify the scanner regressions.

Review a diff for user-visible strings that bypass localization or are assembled
dynamically; the literal scanner cannot prove arbitrary C# data flow. Check switching
languages and returning to Japanese in Unity. Font coverage, wrapping, RTL and Indic
shaping still require separate rendered checks; JSON coverage is not visual proof.

Use `TextMessage` for already formatted/cached diagnostics that can survive a
language change. It can reverse a previous translation when returning to Japanese;
ordinary `Text` keeps original/custom-name fallback behavior unchanged.

The pre-2026-11-09 scope notes below describe earlier checkpoints, not completion
of the now-confirmed full language scope.

## Complex-script Editor rendering (2026-11-11)

The Editor's `PosingSystemTextGUI` adapter measures and paints Arabic, Hebrew,
Devanagari, Bengali, Tamil, Telugu and Thai captions through the isolated
`UnisakiStudio.Posing.Text.Editor` assembly. Keep the original logical strings,
native control calls/IDs and editable values. Do not reverse strings or translate
custom names, serialized values or paths. Use the adapter for new visible controls
and its measurement helpers for wrapping; the coverage scanner checks adapter
sinks as well as native Unity calls. End GUI scopes inside the language scope.

The `Editor/ComplexText` tree contains hash-bound fonts, renamed private managed
and native dependencies, Editor-only import settings and original third-party
notices. Do not enable them for Player builds, introduce public API types from
those dependencies or replace them with similarly named DLLs from other tools.
Retain the source/Fonts/manifest directory layout. Dependency/ABI failures retain
native drawing with one diagnostic; unsupported custom text and size limits fall
back per entry without disabling other captions. Cache, rejection and resource
limits are intentional. Preserve native tooltip content if shaping is unavailable.

`FeatureCardTextTests` exercises all 30 languages, 19 feature cards and three
widths, including glyph clusters and saved-value preservation. `TextGuiEventTests`
compares native/adapted input, IDs, values and GUI.changed. Text editing comparisons
must establish their selection through actual key events because Unity reuses a
global TextEditor. Do not assign its internal cursor state to make a test pass.

Catalog and automated rendering checks are separate from actual window, tooltip,
expanded-popup, platform and VRChat-client visual review. Generic/third-party
property drawers and native editable text retain their original Unity behavior.

## Authoritative data

- `feature-cards.ai.json`: reviewed feature-card batch with frozen `features`
  records (`path`, Japanese `title`, Japanese `description`) and explicit
  `title_paths`. The description rows follow these frozen records; title rows
  follow `title_paths`, never the order of C# declarations. These bindings were
  recovered from commit `906e634` (15 SettingsInspector cards, then 4 Inspector
  cards). `build_feature_cards.py` checks the current path set and both Japanese
  fields before emitting every reviewed row. New/missing paths or changed source
  text stop generation for batch review and translation updates. Reordering C#
  metadata needs no translation changes. Do not infer bindings from the current
  source order or filter rows against old generated output/merged catalogs.
  Run `python Tools/Localization/test_build_feature_cards.py` to check ordering,
  exact preservation of the original 8-language/31-entry batch and drift failures.
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

The legacy `menuLanguage` field remains serialized for compatibility and is ignored. Inspector and generated menus use the shared EditorPrefs preference, with OS language then English fallback. A Generating pass translates only the NDMF build clone before Modular Avatar processes menus, after synchronizing optional features. GameObject names, pose definitions, parameters and animations stay unchanged. Referenced expression menus are copied with shared-reference and cycle handling; owned menu-installer targets follow their copies. External target menus are not translated. Language changes do not invalidate animation prebuild snapshots.

The original name remains editable in pose details; translated names appear in the list and work in search. Custom names without a catalog entry are displayed as entered.

## Scope and verification

As of 2026-11-07: 30 selectable languages including Japanese; navigation and common menus in all 29 target languages; complete pose-name batches in English, French, Italian, Simplified Chinese, Traditional Chinese, Korean, Thai and Hindi. English Inspector coverage is broader than other languages but is not complete. The Indian-language choices remain provisional, not a final interpretation of the user's request.

Seven user guides are in `Documentation~/Localization`. Full existing online manuals have not been translated or published. Source-document translation jobs are prepared separately in the development workspace.

Unity tests cover preferences, catalog availability, fallback, cycles, original-data preservation, nested ownership, inactive objects and installer targets. Development-only integration tests additionally build actual KawaiiPosing and CoolPosing prefabs in English and Traditional Chinese, with and without prebuild. The main development workspace stores test logs and the handoff record under `Recordings/Localization_20261107` and `docs/v3.2.0-feature-candidates.md`.

No version bump, package publication or public deployment is part of this checkpoint. More Inspector strings, full manuals, remaining pose languages, translation review and actual UI/VRChat visual checks remain before release.


## Historical language UX (2026-11-07; superseded by the shared policy above)

The initial unified selector stored an avatar-specific value. That behavior was replaced on 2026-11-12: the header changes only the shared EditorPrefs preference, never a component, scene or prefab. No avatar language scope remains. The removed Features-tab selector stays removed, and popup changes still compare indices rather than relying on the surrounding GUI.changed state.


## Setup programs (2026-11-08)

`../InstallerLocalization/README.md` documents the shared EditorPrefs behavior, standalone license/setup translation, native editor menus, upstream MIT source build, and review-only installer packages. The same code works before the main product package is present. License storage compatibility is preserved.
