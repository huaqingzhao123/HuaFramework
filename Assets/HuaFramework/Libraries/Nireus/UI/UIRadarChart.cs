using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UIRadarChart : Graphic
{
    public RectTransform[] points;

    [Range(0, 1f)]
    public float[] progress;

    /// <summary>
    /// 下标为3点位置开始逆时针
    /// </summary>
    /// <param name="progress"></param>
    public void SetProgress(float[] progress)
    {
        this.progress = progress;
        Refresh();
    }

    public float[] GetProgress()
    {
        return this.progress;
    }

    void Refresh()
    {
        var r = GetPixelAdjustedRect();
        float oneAngle = Mathf.Deg2Rad * 360f / points.Length;
        for (int i = 0; i < points.Length; i++)        
            points[i].localPosition = new Vector2(Mathf.Cos(oneAngle * i), Mathf.Sin(oneAngle * i)) * r.width * progress[i];
        
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        Color32 color32 = color;
        vh.Clear();
        vh.AddVert(new Vector3(0, 0), color32, Vector2.zero);
        for (int i = 0; i < points.Length; i++)        
            vh.AddVert(points[i].localPosition, color32, Vector2.zero);

        for (int i = 0; i < points.Length - 1; i++)
            vh.AddTriangle(0, i + 1, i + 2);
        vh.AddTriangle(0, points.Length, 1);
    }



#if UNITY_EDITOR
    void Update()
    {
        Refresh();
    }
#endif

}