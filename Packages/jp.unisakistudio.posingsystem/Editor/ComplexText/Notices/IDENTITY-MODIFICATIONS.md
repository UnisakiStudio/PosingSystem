# Private identity modifications — 2026-11-11

These copies are modified dependencies distributed with Unisaki Posing System,
not upstream signed releases. Original copyright/license/notices are retained here.

Managed input versions: SkiaSharp/SkiaSharp.HarfBuzz 4.153.1,
HarfBuzzSharp 14.2.1.301, Topten.RichTextKit 0.6.170.
Mono.Cecil 0.11.1 from Unity 2022.3.22f1 was used only at build time; it is not shipped.

Assembly identities were changed to UnisakiStudio.Posing.Text.SkiaSharp,
UnisakiStudio.Posing.Text.SkiaSharp.HarfBuzz,
UnisakiStudio.Posing.Text.HarfBuzzSharp and
UnisakiStudio.Posing.Text.RichTextKit. Their mutual assembly references and
InternalsVisibleTo grants were updated. Original strong-name keys/signatures were
removed from the modified copies and rewritten references. No claim is made that
the modified assemblies retain upstream strong-name authentication. Original input
binaries were not modified. Type namespaces and upstream API implementations remain.

Native P/Invoke module names libSkiaSharp and libHarfBuzzSharp were rewritten to
libUnisakiStudio_Posing_SkiaSharp and libUnisakiStudio_Posing_HarfBuzzSharp.
The six native Windows/macOS/Linux files were copied under those names without
changing binary bytes, embedded native IDs, signatures or ABI. Fonts are unchanged
official Noto files (SIL OFL 1.1). The renderer supplies synthetic italic ink in memory;
it does not alter fonts.

SkiaSharp/HarfBuzzSharp MIT texts and native third-party notices are retained.
RichTextKit Apache-2.0 license and pinned upstream license are retained.
Only selected runtime packages are included; System.* compatibility assemblies,
Mono.Cecil, test fixtures, original Skia2.88, proof harnesses and caches are excluded.

Windows Unity2022 Mono coexistence was executed with both original Skia4.153 and
original Skia2.88.9/native88 while the private renderer used native153.
macOS Intel/ARM and Linux x64 binaries were statically inspected, not executed here.
Their runtime, native-loader coexistence and macOS signing/Gatekeeper behavior still
require testing on those hosts. Keep this distinction in release evidence.
