using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AOEIndicator : MonoBehaviour
{
    public int segments = 60;
    public float radius = 5f;

    void Start()
    {
        var line = GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        line.loop = true;
        line.useWorldSpace = false;
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Unlit/Color"));
        line.material.color = new Color(0f, 1f, 1f, 0.5f);

        for (int i = 0; i <= segments; i++)
        {
            float angle = i / (float)segments * Mathf.PI * 2f;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0.05f, Mathf.Sin(angle) * radius));
        }
    }
}
