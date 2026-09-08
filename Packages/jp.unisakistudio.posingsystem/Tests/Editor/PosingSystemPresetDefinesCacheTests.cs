using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemPresetDefinesCacheTests
    {
        private Type cacheType;

        [SetUp]
        public void SetUp()
        {
            cacheType = typeof(PosingSystemEditor).Assembly.GetType(
                "jp.unisakistudio.posingsystemeditor.PosingSystemPresetDefinesCache");
            Assert.IsNotNull(cacheType);
            Invoke("ResetForTests");
        }

        [TearDown]
        public void TearDown()
        {
            Invoke("ResetForTests");
        }

        [Test]
        public void RefreshCatalog_DoesNotLoadPresetAssets()
        {
            Invoke("RefreshCatalog");

            Assert.Greater(GetProperty<int>("CatalogCount"), 0);
            Assert.AreEqual(0, GetProperty<int>("AssetLoadRequestCount"));
            Assert.IsFalse(GetProperty<bool>("HasLoadedData"));
        }

        [Test]
        public void UnchangedGuidCatalog_ReusesLoadedPresetData()
        {
            var first = Invoke("GetPresetDefines", false);
            var firstLoadCount = GetProperty<int>("AssetLoadRequestCount");

            var catalogChanged = (bool)Invoke("RefreshCatalog");
            var second = Invoke("GetPresetDefines", false);

            Assert.IsFalse(catalogChanged);
            Assert.AreSame(first, second);
            Assert.Greater(firstLoadCount, 0);
            Assert.AreEqual(firstLoadCount, GetProperty<int>("AssetLoadRequestCount"));
        }

        [Test]
        public void SortPresetDefinesByAvatarName_SortsNamesAndKeepsUnnamedLast()
        {
            var alpha = new PosingSystemPresetDefines.PresetDefine { avatarName = "alpha" };
            var bravo = new PosingSystemPresetDefines.PresetDefine { avatarName = "Bravo" };
            var charlie = new PosingSystemPresetDefines.PresetDefine { avatarName = "charlie" };
            var unnamed = new PosingSystemPresetDefines.PresetDefine { avatarName = null };
            var whitespace = new PosingSystemPresetDefines.PresetDefine { avatarName = " " };
            var presetDefines = new[] { charlie, unnamed, bravo, whitespace, alpha };
            var method = typeof(PosingSystemEditor).GetMethod(
                "SortPresetDefinesByAvatarName",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(method);
            var sorted = (List<PosingSystemPresetDefines.PresetDefine>)method.Invoke(
                null,
                new object[] { presetDefines });

            CollectionAssert.AreEqual(
                new[] { alpha, bravo, charlie, unnamed, whitespace },
                sorted);
        }

        private object Invoke(string methodName, params object[] arguments)
        {
            var method = cacheType.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(method, methodName);
            return method.Invoke(null, arguments);
        }

        private T GetProperty<T>(string propertyName)
        {
            var property = cacheType.GetProperty(propertyName, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(property, propertyName);
            return (T)property.GetValue(null);
        }
    }
}
