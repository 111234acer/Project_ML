using UnityEngine;

public class PlayerInput_CP : MonoBehaviour
{
    [HideInInspector] public Vector2 move;
    [HideInInspector] public bool jump;
    [HideInInspector] public bool dash;
    [HideInInspector] public bool primaryFire; // 좌클릭
    [HideInInspector] public bool Skill1;  // 우클릭
    [HideInInspector] public bool ultimate;    // R

    void Update()
    {
        move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        jump = Input.GetKeyDown(KeyCode.Space);
        dash = Input.GetKeyDown(KeyCode.LeftShift);
        primaryFire = Input.GetMouseButtonDown(0);
        Skill1 = Input.GetMouseButtonDown(1);
        ultimate = Input.GetKeyDown(KeyCode.R);
    }
}
