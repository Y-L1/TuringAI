using UnityEngine;

namespace Game
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Input Region")]
        public Rect screenPercentRect = new Rect(0.25f, 0f, 0.75f, 1f);
        
        [Header("Target")]
        public Transform target;
        public float distance = 4.0f; // 相机据检
        public Vector3 targetOffset = new Vector3(0, 5, -6);

        [Header("Follow Settings")]
        public float followSpeed = 5f;
        
        [Header("Rotation Settings")]
        public float rotationSpeed = 100f;
        private float currentYaw = 0f; // 水平旋转
        private float currentPitch = 0f; // 垂直旋转
        public float minPitch = -30f; // 最小俯仰角度
        public float maxPitch = 45f;  // 最大俯仰角度
        private Vector2 lastTouchPos;
        private bool isDragging = false;
        
        [Header("Collision Settings")]
        public float collisionDistance = 1f; // 最小距离，防止相机进入障碍物
        public LayerMask collisionLayer; // 碰撞层
        
        private float CurrentDistance { get; set; }

        private void LateUpdate()
        {
            if (target == null) return;

            if (UIJoystickLayer.GetLayer())
            {
                // 处理输入
                HandleInput();
            }

            // 相机
            // ...
            // 计算目标旋转
            Quaternion targetRotation = Quaternion.Euler(-currentPitch, currentYaw, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);

            // 碰撞检测
            Vector3 origin = target.position + Vector3.up * targetOffset.y + transform.right * targetOffset.x;
            Vector3 backDirection = transform.rotation * Vector3.back;
            
            float desiredDistance = distance;
            if (Physics.Raycast(origin, backDirection, out var hit, distance, collisionLayer) && hit.collider.transform != target)
            {
                // CurrentDistance = Mathf.Lerp(CurrentDistance, hit.distance - collisionDistance, followSpeed * Time.deltaTime);
                float safeDistance = hit.distance - collisionDistance;
                // 限制最小安全距离，避免负值或贴脸
                safeDistance = Mathf.Max(safeDistance, 0.3f); 
                desiredDistance = safeDistance;
                this.LogEditorOnly($"Distance: {hit.distance - collisionDistance}");
            }
            else
            {
                // CurrentDistance = Mathf.Lerp(CurrentDistance, distance, followSpeed * Time.deltaTime);
            }
            
            CurrentDistance = Mathf.Lerp(CurrentDistance, desiredDistance, followSpeed * Time.deltaTime);

            // 设置相机位置
            Vector3 finalPosition = origin + backDirection * CurrentDistance;
            transform.position = finalPosition;

            // 始终看向目标
            transform.LookAt(target.position + Vector3.up * targetOffset.y);
        }

        private void HandleInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButton(0) && IsInControlArea(Input.mousePosition))
            {
                float mouseX = Input.GetAxis("Mouse X");
                currentYaw += mouseX * rotationSpeed * Time.deltaTime;

                float mouseY = Input.GetAxis("Mouse Y");
                currentPitch += mouseY * rotationSpeed * Time.deltaTime;

                currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
            }
#elif UNITY_IOS || UNITY_ANDROID
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began && IsInControlArea(touch.position))
                {
                    lastTouchPos = touch.position;
                    isDragging = true;
                }
                else if (touch.phase == TouchPhase.Moved && isDragging)
                {
                    if (!IsInControlArea(touch.position)) continue; // 仍在控制区域内才处理
            
                    float deltaX = touch.position.x - lastTouchPos.x;
                    currentYaw += deltaX * rotationSpeed * 0.01f * Time.deltaTime;

                    float deltaY = touch.position.y - lastTouchPos.y;
                    currentPitch += deltaY * rotationSpeed * 0.01f * Time.deltaTime;

                    currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
                    lastTouchPos = touch.position;
                }
                else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isDragging)
                {
                    isDragging = false;
                }
            }
#endif
        }
        
        private bool IsInControlArea(Vector2 screenPos)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
    
            Rect pixelRect = new Rect(
                screenPercentRect.x * screenWidth,
                screenPercentRect.y * screenHeight,
                screenPercentRect.width * screenWidth,
                screenPercentRect.height * screenHeight
            );

            return pixelRect.Contains(screenPos);
        }
    }
}