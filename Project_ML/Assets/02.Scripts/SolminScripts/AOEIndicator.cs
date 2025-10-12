using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AOEIndicator : MonoBehaviour
{
    public int segments = 60;       // 원 세분화 정도 
    public float radius = 5f;       // 반지름

    private void Start()
    {
        LineRenderer line = GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        line.useWorldSpace = false;
        line.loop = true;

        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Unlit/Color"));    
        line.material.color = new Color(0f, 1f, 1f, 0.5f);

        UpdateCircle(line);
    }

    void UpdateCircle(LineRenderer line)
    {
        for(int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            line.SetPosition(i,new Vector3(x, 0.01f, z));
        }
    }
}
