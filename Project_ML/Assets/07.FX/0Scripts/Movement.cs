using UnityEngine;

public class Movement : MonoBehaviour
{
    // ✨ 에디터에서 설정할 수 있는 변수들
    [Header("이동 속도 설정 (Speed per second)")]
    [Tooltip("X축 방향으로 초당 이동할 속도 (양수는 오른쪽, 음수는 왼쪽)")]
    public float speedX = 0f;

    [Tooltip("Y축 방향으로 초당 이동할 속도 (양수는 위쪽, 음수는 아래쪽)")]
    public float speedY = 0f;

    [Tooltip("Z축 방향으로 초당 이동할 속도 (양수는 앞쪽, 음수는 뒤쪽)")]
    public float speedZ = 5f; // 보통 메테오는 Z축(앞)으로 이동하므로 기본값을 5로 설정

    // Start 함수는 필요 없으므로 제거하거나 주석 처리합니다.
    // void Start()
    // {
    //     
    // }

    // Update 함수는 매 프레임마다 호출됩니다.
    void Update()
    {
        // 1. 이동 방향과 속도를 나타내는 벡터를 생성합니다.
        Vector3 movement = new Vector3(speedX, speedY, speedZ);

        // 2. transform.Translate 함수를 사용하여 오브젝트를 이동시킵니다.

        // Time.deltaTime을 곱하는 이유:
        // PC 성능에 따라 프레임 속도가 다릅니다. Time.deltaTime은 마지막 프레임이 완료되기까지 걸린 시간(초)을
        // 나타내므로, 이 값을 곱하면 모든 PC에서 초당 일정한 속도로 이동하게 됩니다. (프레임 독립적인 움직임)

        // Space.Self를 사용하여 오브젝트의 로컬 좌표계(오브젝트가 바라보는 방향)를 기준으로 이동합니다.
        transform.Translate(movement * Time.deltaTime, Space.Self);
    }
}