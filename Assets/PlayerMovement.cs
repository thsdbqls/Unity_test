using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 20f;
    public float runSpeed = 30f;
    private float currentSpeed;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 3f;
    // 위/아래 시야를 쓰고 싶으면 카메라(또는 카메라 부모)를 여기에 연결하세요. 비워두면 좌우 회전만 동작합니다.
    public Transform cameraPivot;
    public float minPitch = -60f;
    public float maxPitch = 60f;
    private float yaw;   // 좌우 회전 (몸통)
    private float pitch; // 위아래 회전 (카메라)

    [Header("Jump & Gravity Settings")]
    public float jumpHeight = 2.0f;
    public float gravity = -20.0f;
    private Vector3 velocity;

    [Header("Components")]
    public Animator animator;
    private CharacterController controller;

    [Header("Weapon Settings")]
    public GameObject weaponPrefab; // wep_00110622 모델(또는 프리팹)을 여기에 연결하세요.
    public string handBoneName = "CC_Base_R_Hand"; // 무기를 들 손 뼈 이름
    public Vector3 weaponPositionOffset;
    public Vector3 weaponRotationOffset;
    private GameObject equippedWeapon;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>();

        // 현재 바라보는 방향을 시작 각도로 사용
        yaw = transform.eulerAngles.y;

        // 마우스 커서 잠금 (FPS/TPS 조작감). 필요 없으면 이 두 줄을 지우세요.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        EquipWeapon();
    }

    void EquipWeapon()
    {
        if (weaponPrefab == null) return;

        Transform handBone = FindDeepChild(transform, handBoneName);
        if (handBone == null)
        {
            Debug.LogWarning($"'{handBoneName}' 이름의 손 뼈를 찾지 못했습니다. handBoneName 값을 확인하세요.");
            return;
        }

        equippedWeapon = Instantiate(weaponPrefab, handBone);
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }

    void Update()
    {
        // 1. 바닥 체크
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            if (animator != null) animator.SetBool("IsJumping", false);
        }

        // 2. 공격 입력 처리 (마우스 왼쪽 클릭: 0)
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        // 3. 마우스로 시야 회전
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f); // 몸통은 좌우로만 회전

        if (cameraPivot != null)
        {
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f); // 카메라는 위아래로 회전
        }

        // 4. 이동 입력 (W: 전진 / S: 후진 / A,D: 좌우 이동) - 바라보는 방향 기준
        float horizontal = Input.GetAxis("Horizontal"); // A / D
        float vertical = Input.GetAxis("Vertical");      // W / S
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        if (moveDirection.magnitude > 1f) moveDirection.Normalize();

        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // 5. 이동 애니메이션 (회전은 마우스가 담당하므로 여기서는 속도만 갱신)
        if (moveDirection.magnitude >= 0.1f)
        {
            float animSpeed = isRunning ? 1f : 0.5f;
            if (animator != null) animator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);
        }
        else
        {
            if (animator != null) animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
        }

        // 6. 점프 처리
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -5f * gravity);
            if (animator != null) animator.SetBool("IsJumping", true);
        }

        // 7. 최종 이동 및 중력 적용
        Vector3 finalMove = (moveDirection * currentSpeed) + velocity;
        controller.Move(finalMove * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;

        // 8. 무기 위치/각도 보정 (Play 모드에서 Inspector 값을 바로 조절해 맞출 수 있도록 매 프레임 적용)
        if (equippedWeapon != null)
        {
            equippedWeapon.transform.localPosition = weaponPositionOffset;
            equippedWeapon.transform.localRotation = Quaternion.Euler(weaponRotationOffset);
        }
    }

    [Header("Attack Settings")]
    public int numberOfAttacks = 3; // 전체 공격 모션 개수

    void Attack()
    {
        if (animator != null)
        {
            // 1. 0부터 (공격개수-1)까지 랜덤 인덱스 선택
            int randomIndex = Random.Range(0, numberOfAttacks);

            // 2. 애니메이터의 정수 파라미터 "AttackIndex"에 값을 설정
            animator.SetInteger("AttackIndex", randomIndex);

            // 3. 공격을 재생하는 트리거 실행
            animator.SetTrigger("Attack");
        }
    }
}
