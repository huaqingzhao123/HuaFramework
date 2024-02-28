using Nireus;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 图片置灰
/// </summary>
public static class UIGray
{
    private static Material grayMat;

    /// <summary>
    /// 创建置灰材质球
    /// </summary>
    /// <returns></returns>
    private static Material GetGrayMat()
    {
        if (grayMat == null)
        {
            Shader shader = AssetManager.Instance.loadSync<Shader>($"{PathConst.BUNDLE_RES}Shaders$/UI/UI-Gray.shader");
            //Shader.Find("Custom/UI-Gray");
            if (shader == null)
            {
                GameDebug.Log("null");
                return null;
            }
            Material mat = new Material(shader);
            grayMat = mat;
        }

        return grayMat;
    }

    /// <summary>
    /// 图片置灰
    /// </summary>
    /// <param name="img"></param>
    public static void SetUIGray(Image img)
    {
        img.material = GetGrayMat();
        img.SetMaterialDirty();
    }

    /// <summary>
    /// 图片回复
    /// </summary>
    /// <param name="img"></param>
    public static void Recovery(Image img)
    {
        img.material = null;
    }

    /// <summary>
    /// 图片加颜色包含所有子节点
    /// </summary>
    /// <param name="img"></param>
    public static void SetUIColorAddAll(Transform transform, Color color, Image except = null)
    {
        var tr = transform;
        var image = tr.gameObject.GetComponent<Image>();
        if (image && image != except)
        {
            SetUIColor(image, color);
        }
        for (int i = tr.childCount - 1; i >= 0; i--)
        {
            SetUIColorAddAll(tr.GetChild(i), color, except);
        }
    }

    /// <summary>
    /// 图片加颜色
    /// </summary>
    /// <param name="img"></param>
    public static void SetUIColor(Image img, Color color)
    {
        img.color = color;
    }

    /// <summary>
    /// 图片置灰包含所有子节点
    /// </summary>
    /// <param name="img"></param>
    public static void SetUIGrayAll(Transform transform)
    {
        var tr = transform;
        if (tr.childCount == 0)
        {
            var image = tr.gameObject.GetComponent<Image>();
            if (image)
            {
                SetUIGray(image);
            }
            return;
        }
        for (int i = tr.childCount - 1; i >= 0; i--)
        {
            var image = tr.gameObject.GetComponent<Image>();
            if (image)
            {
                SetUIGray(image);
            }
            SetUIGrayAll(tr.GetChild(i));
        }
    }

    /// <summary>
    /// 图片回复包含所有子节点
    /// </summary>
    /// <param name="img"></param>
    public static void RecoveryAll(Transform transform)
    {
        var tr = transform;
        if (tr.childCount == 0)
        {
            var image = tr.gameObject.GetComponent<Image>();
            if (image)
            {
                Recovery(image);
            }
            return;
        }
        for (int i = tr.childCount - 1; i >= 0; i--)
        {
            var image = tr.gameObject.GetComponent<Image>();
            if (image)
            {
                Recovery(image);
            }
            RecoveryAll(tr.GetChild(i));
        }
    }
}
