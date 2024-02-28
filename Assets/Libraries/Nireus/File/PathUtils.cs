
namespace Nireus
{
    public static class PathUtils
    {
        public static string GetUIPrefabPath(string remainingName)
        {
            return PathConst.BUNDLE_RES_UI_PREFABS + remainingName + ".prefab";
        }

        public static string GetPrefabPath(string remainingName)
        {
            return PathConst.BUNDLE_RES_PREFABS + remainingName + ".prefab";
        }

        public static string GetTexturePath(string remaining)
        {
            return PathConst.BUNDLE_RES_TEXTURES + remaining + ".png";
        }
        public static string GetAtlasPath(string remaining)
        {
            return $"{PathConst.BUNDLE_RES}Textures/UI/{remaining}.spriteatlas";
        }
    }

}

