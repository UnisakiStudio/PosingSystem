using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace UnisakiStudio.PosingSystem.Tests
{
    public class LocalizationAssetTests
    {
        [Test]
        public void ErrorLocalization_DoesNotCreateUnityLocalizationAsset()
        {
            var existingInstanceIds = Resources.FindObjectsOfTypeAll<UnityEngine.LocalizationAsset>()
                .Select(asset => asset.GetInstanceID())
                .ToHashSet();

            var localizer = LocalizationAsset.ErrorLocalization();
            var newlyCreatedAssets = Resources.FindObjectsOfTypeAll<UnityEngine.LocalizationAsset>()
                .Where(asset => !existingInstanceIds.Contains(asset.GetInstanceID()))
                .ToArray();

            try
            {
                Assert.That(newlyCreatedAssets, Is.Empty,
                    "NDMFが保持するLocalizerは破棄可能なUnityEngine.LocalizationAssetを参照しないこと");
                Assert.That(
                    localizer.GetLocalizedString("オブジェクトの設定が更新されています。再度プレビルドを行ってください"),
                    Is.EqualTo("「{0}」オブジェクトの設定が更新されています。再度プレビルドを行ってください"));
            }
            finally
            {
                foreach (var asset in newlyCreatedAssets)
                {
                    Object.DestroyImmediate(asset);
                }
            }
        }
    }
}
