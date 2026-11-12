# Complete multilingual documentation

The advertised 30 languages must each have a full README, complete historical
CHANGELOG and offline user guide for PosingSystem, KawaiiPosing and CoolPosing.
Quick references are additional concise references, not substitutes for full text.
Publication remains a separate user decision. Keep the upcoming versions marked
unreleased until their actual release; never manufacture a release date.

## Authoring and review

`sources.json` freezes twelve complete source documents and deduplicates identical
paragraphs by content digest. `translations/<locale>.json` is authoritative local
AI authoring, preserving full-document context, existing UI labels and pose names.
Generated Markdown/HTML must not be edited directly. Original agreements, legal
references, third-party notices, asset names and identifiers remain intact.

Before changing a document, read the matching UI glossary in
`../Localization/AIReview/<locale>.json` and the current display catalogs. Update
all languages, including new features, operating limits and troubleshooting.
Use normal grammar and record reviewed inflections; do not replace coherent prose
with word-by-word substitution or accept an English/Japanese fallback as translated.
Translate standard pose-name sets together and reuse the approved display names.
Historical avatar/product/author names and original file names remain identifiable.

If sources change, run `freeze_sources.py`, review affected content-ID changes,
translate every new/changed paragraph, and update source provenance only after
confirming unchanged IDs still have the same context. A new global digest alone
does not make old/omitted translations valid. External translation services are
not used and require explicit permission before any future use.

## Products that own their documentation data (2026-11-13)

`sources.json` and `translations/` in this folder cover the three main products. A
product added later keeps its own frozen sources, translations and reviewed inflections
in `<product repo>/Tools/Documentation`, so adding or changing it never alters the shared
digests or the manifests of the released products. VirtualLove is the first one:

```sh
python Tools/Documentation/freeze_sources.py --product VirtualLove
python Tools/Documentation/build_documents.py --data ../VirtualLove/Tools/Documentation --report <file>
python Tools/Documentation/build_documents.py --data ../VirtualLove/Tools/Documentation --check --report <file>
python Tools/Documentation/audit_terminology.py --data ../VirtualLove/Tools/Documentation --strict --report <file>
```

Such a product has no separate guide: its package README is frozen as the guide too, so
`UserGuide` pages exist for every language and the Inspector's guide link follows the
selected language. Paragraphs identical to the main products' documents should stay
identical; their reviewed translations are then copied instead of re-translated. The
terminology gate applies the shared UI glossary and the shared reviewed inflections.

## Build and verify

Run from the core repository (Python 3.10+, standard library only):

```sh
python Tools/Documentation/build_documents.py --report documentation-audit.json
python Tools/Documentation/build_documents.py --check --report documentation-check.json
python Tools/Documentation/check_documents.py
python Tools/Documentation/audit_terminology.py --strict --report documentation-terminology.json
python -m unittest discover -s Tools/Documentation -p test_documents.py
```

The generator writes all available primary product checkouts. Use `--products
PosingSystem` for the core CI checkout. `--audit-only` writes only the requested
report. Explicit `--locales` is for intermediate work and does not pass the final
30-language gate. Missing text, stale provenance, changed links/code/history,
lost table/list items, broken local links and stale generated output fail checks.
The terminology gate compares feature names with the existing UI glossary.
Reviewed grammatical forms and digest-bound exceptions belong in `Reviews/`;
never add an English fallback or an unexplained synonym to make a check pass.

When refreshing the existing quick references, first build the complete documents,
then run the development workspace's `Tools/GumroadEnglish/generate_multilingual_reference.py`.
It reuses reviewed localized scope text and links to each full localized guide.
Rebuild the document manifests after that change and run the final `--check`.

Bundled files live in `Documentation~/Localization/<locale>/`, with a native-name
language index and full conventional English README/CHANGELOG entry points.
HTML is offline, declares language/direction, and isolates code in RTL paragraphs.
Inspector links use the selected language. Changelog caches are language-specific;
test changing languages and returning to Japanese on the same package-info object.

Each product includes `Localization/manifest.json`, recording source and generated
digests, language coverage and AI provenance. `check_documents.py` also works in
product-only CI checkouts. The core copy of this checker is mirrored to the two
product repositories. Run all three checks before packaging the exact commits.

Structural coverage does not prove native-speaker accuracy or Unity complex-script
rendering. Record those separate review states honestly. Any formal release also
requires the outstanding Inspector typography and installer integration checks.
