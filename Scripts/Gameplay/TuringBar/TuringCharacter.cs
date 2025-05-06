using DragonLi.Frame;
using UnityEngine;

namespace Game
{
    public class TuringCharacter : MonoBehaviour
    {
        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Run = Animator.StringToHash("Run");
        
        public enum ECharacterType
        {
            Idle,
            Walk,
            Run,
        }
        
        [Header("References")]
        private CameraFollow cameraFollow;

        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float rotationSpeed = 10f;
        public float gravity = -9.81f;

        [Header("Joystick Reference")]
        public JoyStickController joyStick;
        public Animator animator;

        private CharacterController controller;
        private Vector3 moveDirection;
        private float verticalVelocity = 0f;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
        }
        
        private void Update()
        {
            Vector2 input = joyStick.Direction * -1f;

            // 获取相机的 forward 和 right，并将它们的 y 分量设为 0（防止倾斜）
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = Camera.main.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            // 将摇杆方向投射到相机的 forward 和 right 上
            moveDirection = camForward * input.y + camRight * input.x;

            // 如果有移动输入，旋转角色朝向移动方向
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }

            // controller.Move(moveDirection * moveSpeed * Time.deltaTime);
            if (controller.isGrounded)
            {
                verticalVelocity = 0f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
            
            Vector3 move = new Vector3(moveDirection.x, verticalVelocity, moveDirection.z);
            controller.Move(move * moveSpeed * Time.deltaTime);

            // 动作控制
            float inputMagnitude = input.magnitude;
            if (inputMagnitude <= 0.1f)
            {
                SetCharacter(ECharacterType.Idle);
            }
            else if (inputMagnitude <= 0.5f)
            {
                SetCharacter(ECharacterType.Walk);
            }
            else
            {
                SetCharacter(ECharacterType.Run);
            }
        }


        // private void Update()
        // {
        //     Vector2 input = joyStick.Direction;
        //     moveDirection = new Vector3(input.x, 0, input.y);
        //
        //     if (moveDirection.sqrMagnitude > 0.01f)
        //     {
        //         Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        //         transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        //     }
        //
        //     controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        //
        //     if(Vector2.Distance(Vector2.zero, input) <= 0.1f)
        //     {
        //         SetCharacter(ECharacterType.Idle);
        //     }
        //     else if (Vector2.Distance(Vector2.zero, input) <= 0.2f)
        //     {
        //         SetCharacter(ECharacterType.Walk);
        //     }
        //     else
        //     {
        //         SetCharacter(ECharacterType.Run);
        //     }
        // }

        private void SetCharacter(ECharacterType characterType)
        {
            animator.SetBool(Idle, !animator.GetBool(Idle) && characterType == ECharacterType.Idle);
            animator.SetBool(Walk, !animator.GetBool(Walk) && characterType == ECharacterType.Walk);
            animator.SetBool(Run, !animator.GetBool(Run) && characterType == ECharacterType.Run);
        }
    }
}