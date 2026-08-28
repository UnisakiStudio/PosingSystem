using System;
using System.Reflection;
using NUnit.Framework;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemProductMenuCatalogTests
    {
        private Type catalogType;

        [SetUp]
        public void SetUp()
        {
            catalogType = typeof(PosingSystemEditor).Assembly.GetType(
                "jp.unisakistudio.posingsystemeditor.PosingSystemProductMenuCatalog");
            Assert.IsNotNull(catalogType);
            Invoke("ResetForTests");
        }

        [TearDown]
        public void TearDown()
        {
            Invoke("ResetForTests");
        }

        [Test]
        public void RepeatedLookup_ReusesSingleAssetCatalog()
        {
            Invoke("GetMatchingGuids", "KawaiiSitting_ExpressionsMenu");
            Invoke("GetMatchingGuids", "VirtualLove_ExpressionsMenu");
            Invoke("GetMatchingGuids", "KawaiiSitting_ExpressionsMenu");

            Assert.AreEqual(1, GetProperty<int>("AssetSearchRequestCount"));
        }

        private object Invoke(string methodName, params object[] arguments)
        {
            var method = catalogType.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(method, methodName);
            return method.Invoke(null, arguments);
        }

        private T GetProperty<T>(string propertyName)
        {
            var property = catalogType.GetProperty(propertyName, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(property, propertyName);
            return (T)property.GetValue(null);
        }
    }
}
