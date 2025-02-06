using UnityEngine;
using UnityEngine.UI;

public class CircleDrawer:Graphic {
    public int segments = 100;
    public float outerRadius = 50f;
    public float thickness = 5f;
    public Color outlineColor = Color.white;

    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();

        float angleIncrement = 360f / segments;
        float innerRadius = outerRadius - thickness;

        for (int i = 0; i < segments; i++) {
            float angle1 = Mathf.Deg2Rad * (i * angleIncrement);
            float angle2 = Mathf.Deg2Rad * ((i + 1) * angleIncrement);

            Vector2 outer1 = new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * outerRadius;
            Vector2 outer2 = new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * outerRadius;

            Vector2 inner1 = new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * innerRadius;
            Vector2 inner2 = new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * innerRadius;

            vh.AddVert(outer1, outlineColor, Vector2.zero);
            vh.AddVert(outer2, outlineColor, Vector2.zero);
            vh.AddVert(inner1, outlineColor, Vector2.zero);
            vh.AddVert(inner2, outlineColor, Vector2.zero);

            vh.AddTriangle(i * 4, i * 4 + 1, i * 4 + 2);
            vh.AddTriangle(i * 4 + 1, i * 4 + 3, i * 4 + 2);
        }
    }
}