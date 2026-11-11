using System;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnisakiStudio.Setup;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class LocalizationPerformanceTests
    {
        [Test]
        public void RepeatedEnglishAndUnknownMessagesDoNotAllocateCandidateArrays()
        {
            foreach (var source in new[] { "Manual", "Support", "Details", "My Custom Pose 123", "Target: MyAvatar" })
            {
                Language.Translate("en", source); // first-use preparation is measured separately
                var timer = new Stopwatch();
                long before = GC.GetAllocatedBytesForCurrentThread();
                timer.Start();
                for (int i = 0; i < 1000; i++) Language.Translate("en", source);
                timer.Stop();
                long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
                Assert.That(bytes, Is.LessThan(128000), source + " should allocate less than 128 bytes per warm lookup");
                TestContext.WriteLine(source + ": " + timer.Elapsed.TotalMilliseconds + " ms/1000, " + bytes + " bytes");
            }
        }

        [Test]
        public void ExactLookupDoesNotInterpretFormattedUserInput()
        {
            foreach (var code in Language.Codes)
                Assert.That(Language.TranslateExact(code, "Target: MyAvatar"), Is.EqualTo("Target: MyAvatar"), code);
            Assert.That(Language.Translate("fr", "Target: MyAvatar"), Is.EqualTo("Cible : MyAvatar"));
            Assert.That(Language.TranslateExact("en", "  キャンセル\n"), Is.EqualTo("  Cancel\n"));
        }

        [Test]
        public void CustomNamesAndMenuCopiesStayUnchangedInEveryLanguage()
        {
            var source = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            source.controls.Add(new VRCExpressionsMenu.Control { name = "Target: MyAvatar" });
            source.controls.Add(new VRCExpressionsMenu.Control { name = "Support" });
            try
            {
                foreach (var code in Language.Codes)
                {
                    foreach (var name in new[] { "Target: MyAvatar", "Support", "Cancel", "Storage: Private", "[PS001] My Pose" })
                        Assert.That(PosingSystemLocalization.TranslateDisplayName(code, name), Is.EqualTo(name), code);
                    var copies = new System.Collections.Generic.Dictionary<VRCExpressionsMenu, VRCExpressionsMenu>();
                    var copy = PosingSystemMenuLocalization.CloneMenu(source, code, copies);
                    try { Assert.That(copy.controls[0].name, Is.EqualTo(source.controls[0].name)); Assert.That(copy.controls[1].name, Is.EqualTo("Support")); }
                    finally { UnityEngine.Object.DestroyImmediate(copy); }
                }
                Assert.That(PosingSystemLocalization.TranslateDisplayName("en", "あぐら"), Is.EqualTo("Cross-legged"));
                Assert.That(PosingSystemLocalization.PresetName("キャンセル", false), Is.EqualTo("キャンセル"));
            }
            finally { UnityEngine.Object.DestroyImmediate(source); }
        }

        [Test]
        public void DiagnosticCacheIsBoundedAndLanguageSpecific()
        {
            for (int i = 0; i < 1100; i++) Language.Translate("en", "Unknown probe " + i);
            var cache = (IDictionary)typeof(Language).GetField("MessageCache", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            Assert.That(cache.Count, Is.LessThanOrEqualTo(1024));
            int count = cache.Count;
            Language.Translate("en", new string('x', 4200));
            Assert.That(cache.Count, Is.EqualTo(count));
            Assert.That(Language.Translate("fr", "Target: MyAvatar"), Is.EqualTo("Cible : MyAvatar"));
            Assert.That(Language.Translate("ja", "Cible : MyAvatar"), Is.EqualTo("対象: MyAvatar"));
            Assert.That(Language.Translate("en", "Target: MyAvatar"), Is.EqualTo("Target: MyAvatar"));
        }
    }
}
