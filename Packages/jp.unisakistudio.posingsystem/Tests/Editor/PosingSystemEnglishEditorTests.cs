using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemEnglishEditorTests : global::jp.unisakistudio.posingsystemeditor.tests.SharedLanguagePreferenceTestBase
    {
        private static readonly Regex Japanese = new Regex("[\\u3040-\\u30ff\\u3400-\\u9fff]");

        [Test]
        public void BulkImportNoticesFollowLanguageChangesAndDuplicateStaging()
        {
            var browser = new PosingSystemPoseBrowser();
            var clip = new AnimationClip { name = "原名保持" };
            try
            {
                PosingSystemLocalization.Select("en");
                Assert.That(browser.Notice, Is.Null);
                browser.Stage(new[] { clip });
                Assert.That(browser.Notice, Is.EqualTo("Review the candidates before adding them."));
                foreach (var code in PosingSystemLocalization.Codes.Concat(new[] { "ja", "en", "ja" }))
                {
                    PosingSystemLocalization.Select(code);
                    var expected = PosingSystemLocalization.Text("候補を確認してから追加できます。");
                    Assert.That(browser.Notice, Is.EqualTo(expected), code);
                    if (code != "ja") Assert.That(browser.Notice, Is.Not.EqualTo("候補を確認してから追加できます。"), code);
                }
                // Re-staging the same clip must replace the previous notice, without adding duplicates.
                browser.Stage(new[] { clip });
                PosingSystemLocalization.Select("en");
                Assert.That(browser.Notice, Is.EqualTo("No new animation clips can be added."));
                PosingSystemLocalization.Select("ja");
                Assert.That(browser.Notice, Is.EqualTo("追加できる新しいアニメーションクリップがありません。"));
                browser.SetAddedNotice(7);
                foreach (var code in PosingSystemLocalization.Codes.Concat(new[] { "en", "ja" }))
                {
                    PosingSystemLocalization.Select(code);
                    Assert.That(browser.Notice, Is.EqualTo(PosingSystemLocalization.Format("{0}件追加しました。", 7)), code);
                }
                Assert.That(browser.Notice, Is.EqualTo("7件追加しました。"));
                browser.Stage(Array.Empty<AnimationClip>());
                Assert.That(browser.Notice, Is.EqualTo("追加できる新しいアニメーションクリップがありません。"));
                Assert.That(clip.name, Is.EqualTo("原名保持"));
            }
            finally { Object.DestroyImmediate(clip); }
        }

        [Test]
        public void InitialPresetConfirmationTranslatesOfficialNamesAndPreservesCustomNames()
        {
            var official = new PosingSystemPresetCatalog.Entry { avatarName = "しなの（着せ替え）", official = true };
            var custom = new PosingSystemPresetCatalog.Entry { avatarName = official.avatarName, official = false };
            foreach (var code in PosingSystemLocalization.Codes.Concat(new[] { "en", "ja" }))
            {
                PosingSystemLocalization.Select(code);
                var translated = PosingSystemMenuItems.PresetConfirmation(official);
                Assert.That(translated, Does.Contain(PosingSystemLocalization.PresetName(official.avatarName, true)), code);
                Assert.That(PosingSystemMenuItems.PresetConfirmation(custom), Does.Contain(custom.avatarName), code);
                if (code == "en")
                    Assert.That(translated, Is.EqualTo("A preset for Shinano (Outfit) was found. Apply it to adjust the poses to this avatar?"));
            }
            Assert.That(PosingSystemMenuItems.PresetConfirmation(official), Does.Contain("しなの（着せ替え）"));
            Assert.That(official.avatarName, Is.EqualTo(custom.avatarName));
        }

        [Test]
        public void InspectorWarningTranslationPreservesExceptionDetailsAcrossLanguages()
        {
            const string prefix = "プリセット一覧を読み込めませんでした。「再読込」で再試行できます:";
            const string details = " C:/My Avatar/原名保持.asset: Target: MyAvatar";
            const string motion = "「立ち・歩き・走り」「しゃがみ（Crouching）」「伏せ（Prone）」には移動を含むBlendTreeを指定してください。単独の立ち姿勢は「立ちのみ」に指定できます。";
            foreach (var code in PosingSystemLocalization.Codes.Concat(new[] { "en", "ja" }))
            {
                PosingSystemLocalization.Select(code);
                Assert.That(PosingSystemLocalization.TextMessage(prefix + details),
                    Is.EqualTo(PosingSystemLocalization.Text(prefix) + details), code);
                var warning = PosingSystemLocalization.TextMessage(motion);
                Assert.That(warning, Is.EqualTo(PosingSystemLocalization.Text(motion)), code);
                if (code != "ja") Assert.That(warning, Is.Not.EqualTo(motion), code);
            }
        }

        [Test]
        public void AdjustmentWindowLabelsAndTooltipsFollowTheSavedAvatarLanguage()
        {
            var root = new GameObject("Localization editor test");
            try
            {
                var system = root.AddComponent<PosingSystem>();
                PosingSystemLocalization.Select("en");
                using (PosingSystemLocalization.ForInspector(system))
                    foreach (var property in typeof(PosingAnimationAdjustmentWindowText).GetProperties(BindingFlags.Public | BindingFlags.Static))
                        Assert.That(Japanese.IsMatch((string)property.GetValue(null)), Is.False, property.Name);

                PosingSystemLocalization.Select("ja");
                using (PosingSystemLocalization.ForInspector(system))
                    Assert.That(PosingAnimationAdjustmentWindowText.Title, Is.EqualTo("アニメーション調整"));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void MuscleAndBoneLabelsChangeWithoutChangingTheirHumanoidIndices()
        {
            var root = new GameObject("Localization editor test");
            try
            {
                var system = root.AddComponent<PosingSystem>();
                var boneLabels = typeof(PosingSystemAutoPoseEditorGui).GetProperty("TargetBoneLabels", BindingFlags.NonPublic | BindingFlags.Static);
                PosingSystemLocalization.Select("en");
                using (PosingSystemLocalization.ForInspector(system))
                {
                    for (var i = 0; i < HumanTrait.MuscleCount; i++)
                        Assert.That(PosingAnimationAdjustmentMuscle.GetMuscleNameJapanese(i), Is.EqualTo(HumanTrait.MuscleName[i]), "Humanoid muscle " + i);
                    foreach (var group in PosingAnimationAdjustmentMuscle.MuscleGroups.Where(g => g != null))
                        Assert.That(Japanese.IsMatch(group.DisplayName), Is.False, group.Name.ToString());
                    Assert.That(((GUIContent[])boneLabels.GetValue(null)).All(label => !Japanese.IsMatch(label.text)), Is.True);
                }
                PosingSystemLocalization.Select("ja");
                using (PosingSystemLocalization.ForInspector(system))
                {
                    Assert.That(PosingAnimationAdjustmentMuscle.GetMuscleNameJapanese(0), Is.EqualTo("腰 前後"));
                    Assert.That(((GUIContent[])boneLabels.GetValue(null)).Any(label => Japanese.IsMatch(label.text)), Is.True);
                }
            }
            finally { Object.DestroyImmediate(root); }
        }

        public sealed class EnumHolder : ScriptableObject
        {
            public PosingSystemAutoPoseLimb limb = PosingSystemAutoPoseLimb.RightLeg;
            public PosingSystemAutoPoseTargetSpace space;
            public PosingSystemAutoPoseLengthReference length;
            public PosingSystemAutoPoseBendDirectionMode bend;
            public PosingSystemAutoPosePositionSpace position;
        }

        [Test]
        public void LocalizedEnumLabelsLeaveSerializedValuesUntouched()
        {
            var holder = ScriptableObject.CreateInstance<EnumHolder>();
            var root = new GameObject("Localization editor test");
            try
            {
                var system = root.AddComponent<PosingSystem>(); PosingSystemLocalization.Select("en");
                using var serialized = new SerializedObject(holder);
                var before = EditorJsonUtility.ToJson(holder);
                using (PosingSystemLocalization.ForInspector(system))
                    foreach (var field in new[] { "limb", "space", "length", "bend", "position" })
                        foreach (var label in PosingSystemLocalizedEnumDrawer.Labels(serialized.FindProperty(field)))
                            Assert.That(Japanese.IsMatch(label.text), Is.False, field + ": " + label.text);
                Assert.That(EditorJsonUtility.ToJson(holder), Is.EqualTo(before));
                Assert.That(holder.limb, Is.EqualTo(PosingSystemAutoPoseLimb.RightLeg));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(holder); }
        }

        [Test]
        public void DiagnosticsTranslateTheExplanationAndPreserveCodesAndCustomNames()
        {
            var root = new GameObject("Localization editor test");
            try
            {
                var system = root.AddComponent<PosingSystem>(); PosingSystemLocalization.Select("en");
                using (PosingSystemLocalization.ForInspector(system))
                {
                    var missing = PosingSystemDiagnostics.Collect(null).Single();
                    Assert.That(missing.Message, Does.StartWith("[PS001]"));
                    Assert.That(Japanese.IsMatch(missing.Message), Is.False, missing.Message);
                    const string original = "「Pose category」の1番目（原名保持）にアニメーションがありません。";
                    var issue = new PosingSystemDiagnostics.Issue("PS025", original,
                        "ポーズ一覧でアニメーションを指定するか、その項目を無効にしてください。", MessageType.Warning, system);
                    Assert.That(issue.Message, Does.Contain("has no animation"));
                    Assert.That(issue.Message, Does.Contain("原名保持"));
                    Assert.That(Japanese.IsMatch(issue.Message.Replace("原名保持", "")), Is.False, issue.Message);
                    Assert.That(issue.Summary, Is.EqualTo(original));
                }
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void NdmfErrorReportHasAnEnglishManagedTemplate()
        {
            var localizer = LocalizationAsset.ErrorLocalization("en");
            var text = localizer.GetLocalizedString("オブジェクトの設定が更新されています。再度プレビルドを行ってください");
            Assert.That(text, Does.Contain("{0}"));
            Assert.That(text, Does.Contain("prebuild"));
            Assert.That(Japanese.IsMatch(text), Is.False);
        }

        [Test]
        public void NestedIkWarningsTranslateEvenWhenTheRuleLabelIsEmpty()
        {
            const string original = "2件のIKルールを適用しました。\n: LeftArm のHumanoidボーンがありません。";
            var english = UnisakiStudio.Setup.Language.Translate("en", original);
            Assert.That(english, Does.StartWith("Applied 2 IK rules."));
            Assert.That(english, Does.Contain("Humanoid bones for LeftArm are missing"));
            Assert.That(Japanese.IsMatch(english), Is.False, english);
            Assert.That(UnisakiStudio.Setup.Language.Translate("ja", english), Is.EqualTo(original));
        }

        [Test]
        public void AssetCreationMenuTranslatesWithoutCreatingAnAsset()
        {
            const string source = "Assets/Create/ゆにさきスタジオ/ポーズシステム/自動姿勢IK設定";
            var path = UnisakiStudio.Setup.LocalizedMenus.PathFor(source, "en");
            Assert.That(path, Is.EqualTo("Assets/Create/Unisaki Studio/Posing System/Pose IK Catalog"));
            var before = AssetDatabase.FindAssets("t:PosingSystemAutoPoseCatalog").OrderBy(guid => guid).ToArray();
            try
            {
                UnisakiStudio.Setup.LocalizedMenus.Rebuild("en");
                var exists = typeof(Menu).GetMethod("MenuItemExists", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                Assert.That((bool)exists.Invoke(null, new object[] { path }), Is.True);
                Assert.That(AssetDatabase.FindAssets("t:PosingSystemAutoPoseCatalog").OrderBy(guid => guid), Is.EqualTo(before));
            }
            finally { UnisakiStudio.Setup.LocalizedMenus.Rebuild(); }
        }

        [Test]
        public void ManualLinkUsesTheBundledEnglishGuideAndKeepsTheJapaneseOnlineManual()
        {
            var folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Unisaki Manual 日本語 " + Guid.NewGuid().ToString("N"));
            var root = new GameObject("Localization manual test");
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(folder, "Documentation~"));
                var document = System.IO.Path.Combine(folder, "Documentation~", "UserGuide.en.html");
                System.IO.File.WriteAllText(document, "<!doctype html><title>English guide</title>");
                var info = new PosingSystemPackageInfo.Info { name = "jp.unisakistudio.kawaiiposing", RootPath = folder };
                var system = root.AddComponent<PosingSystem>(); PosingSystemLocalization.Select("en");
                using (PosingSystemLocalization.ForInspector(system))
                {
                    var uri = new Uri(info.ManualUrl);
                    Assert.That(uri.IsFile, Is.True);
                    Assert.That(System.IO.File.ReadAllText(uri.LocalPath), Does.Contain("English guide"));
                }
                PosingSystemLocalization.Select("ja");
                using (PosingSystemLocalization.ForInspector(system))
                    Assert.That(info.ManualUrl, Is.EqualTo("https://docs.unisakistudio.jp/kawaiiposing/manual/"));
            }
            finally
            {
                Object.DestroyImmediate(root);
                if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true);
            }
        }
    }
}
