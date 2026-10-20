#if PLATFORM_ZERO_YARN
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlatformZero
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class StationWalker : MonoBehaviour
    {
        [SerializeField] private Camera view;
        [SerializeField] private StationGame game;
        private CharacterController controller;
        private float pitch;
        private float verticalSpeed;
        public Camera View => view;
        public void Configure(Camera camera, StationGame owner) { view = camera; game = owner; }
        private void Awake() { controller = GetComponent<CharacterController>(); }
        private void Update()
        {
            if (!game.CanExplore || !Application.isFocused) return;
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 look = mouse.delta.ReadValue() * 0.10f;
                transform.Rotate(0, look.x, 0);
                pitch = Mathf.Clamp(pitch - look.y, -75, 75);
                view.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            }
            Vector2 input = Vector2.zero;
            if (keyboard != null)
            {
                input.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0)
                    - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
                input.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0)
                    - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
            }
            input = Vector2.ClampMagnitude(input, 1);
            verticalSpeed = controller.isGrounded ? -2 : verticalSpeed - 18 * Time.deltaTime;
            Vector3 motion = (transform.right * input.x + transform.forward * input.y) * 3.4f;
            motion.y = verticalSpeed;
            controller.Move(motion * Time.deltaTime);
            if (transform.position.y < -8) { controller.enabled = false; transform.position = new Vector3(0, 0.1f, -10); controller.enabled = true; }
        }
    }
}

#endif
