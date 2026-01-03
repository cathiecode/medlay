using UnityEngine;

namespace com.superneko.medlay.Tests
{
    static class TestObjectLoader
    {
        public static T LoadTestObject<T>(string guid) where T : UnityEngine.Object
        {
            var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            return Object.Instantiate(obj);
        }
    }
}
