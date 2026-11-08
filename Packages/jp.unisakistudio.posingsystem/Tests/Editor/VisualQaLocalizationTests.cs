using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class VisualQaLocalizationTests
    {
        [TestCase("手動調整を保存してから、現在のIKの結果を別の調整アニメーションに保存します。既存のファイルは残ります。")]
        [TestCase("保存して続ける")]
        [TestCase("すべてのポーズ画像を作り直します。ポーズの数によっては少し時間がかかります。\n更新中はUnityの操作をお待ちください。")]
        [TestCase("更新する")]
        [TestCase("生成済みの画像ファイルを削除します。削除後は「アイコンを撮り直す」で再生成できます。")]
        [TestCase("Project内のPrefabを直接変更できません。シーン内のアバター、またはPrefab編集モードでプレビルドしてください。")]
        [TestCase("共通ポージングAnimatorの参照からAnimator Controllerを読み込めません。")]
        [TestCase("共通ポージングAnimatorの有効状態・EditorOnlyタグ・MA Merge Animatorの参照を確認してください。調整済みの生成ファイルは保持してください。")]
        [TestCase("頭のボーンには既にVRC Head Chopが設定されています。")]
        [TestCase("既存のHead Chop設定との重複を避けるため、「撮影モード中は頭を非表示にしない」をオフにするか、既存設定のHead対象を見直してください。耳など別のボーンの設定はそのまま併用できます。")]
        public void ConfirmationBodyAndActionsHaveRealCatalogEntriesInEveryLanguage(string key)
        {
            foreach (var language in PosingSystemLocalization.Codes)
            {
                if (language == "ja")
                {
                    Assert.That(PosingSystemLocalization.Translate(language, key, key), Is.EqualTo(key));
                    continue;
                }
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(
                    "Packages/jp.unisakistudio.posingsystem/Editor/Localization/" + language + ".json");
                Assert.That(asset, Is.Not.Null, language);
                var catalog = PosingSystemLocalization.ParseCatalog(asset.text);
                Assert.That(catalog.ContainsKey(key), Is.True, language + ": " + key);
                Assert.That(PosingSystemLocalization.Translate(language, key, "missing"), Is.EqualTo(catalog[key]), language);
            }
        }

        [Test]
        public void PosePositionLabelsKeepNumberAndJapaneseRoundTrip()
        {
            const string key = "{0}番";
            foreach (var language in PosingSystemLocalization.Codes)
            {
                var format = PosingSystemLocalization.Translate(language, key, key);
                var label = string.Format(format, 123);
                Assert.That(label, Does.Contain("123"), language);
                Assert.That(label, Does.Not.Contain("{0}"), language);
                if (language == "ja") Assert.That(label, Is.EqualTo("123番"));
                else Assert.That(label, Does.Not.Contain("番"), language);
            }
            Assert.That(PosingSystemLocalization.Translate("ja", key, key), Is.EqualTo(key));
        }
    }
}
