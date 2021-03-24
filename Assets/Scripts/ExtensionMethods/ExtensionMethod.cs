using UnityEngine;

namespace AMPR.ExtensionMethod
{
    public static class ExtensionMethod
    {
        public static Texture2D ToTexture2D(this RenderTexture rTex, bool mipChain = false, bool recalculateMips = false, bool updateMips = false)
        {
            RenderTexture oldActiveRTex = RenderTexture.active;

            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, mipChain);
            RenderTexture.active = rTex;

            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0, recalculateMips);
            tex.Apply(updateMips);

            RenderTexture.active = oldActiveRTex;
            oldActiveRTex.Release();
            rTex.Release();

            return tex;
        }

        public static bool IsObjectVisible(this UnityEngine.Camera @this, Renderer renderer) => GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(@this), renderer.bounds);
    }

    public interface IBase {}

    public static class IBaseExtensions
    {
        public static bool IsValid(this IBase iBase)
        {
            if (iBase == null)
            {
                return false;
            }

            return iBase is Object ? iBase as Object : true;
        }
    }
}