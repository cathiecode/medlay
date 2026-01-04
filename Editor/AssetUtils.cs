namespace com.superneko.medlay.Core
{
    internal class AssetUtils
    {
        public static T LoadAssetByGUID<T>(string guid) where T : UnityEngine.Object
        {
            var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            return obj;
        }
    }
}
