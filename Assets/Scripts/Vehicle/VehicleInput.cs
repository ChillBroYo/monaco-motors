using UnityEngine;
using UnityEngine.InputSystem;

namespace MonacoMotors.Vehicle
{
    [RequireComponent(typeof(VehicleController))]
    public class VehicleInput : MonoBehaviour
    {
        public enum SteeringMode
        {
            Tilt,
            Buttons,
            Stick
        }

        [Header("Settings")]
        [SerializeField] private SteeringMode steeringMode = SteeringMode.Tilt;
        [SerializeField] private float tiltSensitivity = 2f;
        [SerializeField] private bool autoAccelerate = true;

        private VehicleController vehicle;
        private float steerInput;
        private float throttleInput;
        private float brakeInput;
        private bool driftInput;

        public SteeringMode CurrentSteeringMode
        {
            get => steeringMode;
            set => steeringMode = value;
        }

        public bool AutoAccelerate
        {
            get => autoAccelerate;
            set => autoAccelerate = value;
        }

        private void Awake()
        {
            vehicle = GetComponent<VehicleController>();
        }

        private void Update()
        {
            ReadInput();
            vehicle.SetInput(steerInput, throttleInput, brakeInput, driftInput);
        }

        private void ReadInput()
        {
            switch (steeringMode)
            {
                case SteeringMode.Tilt:
                    ReadTiltInput();
                    break;
                case SteeringMode.Buttons:
                    ReadButtonInput();
                    break;
                case SteeringMode.Stick:
                    ReadStickInput();
                    break;
            }

            ReadThrottleAndBrake();
        }

        private void ReadTiltInput()
        {
            if (SystemInfo.supportsAccelerometer)
            {
                Vector3 accel = Input.acceleration;
                steerInput = Mathf.Clamp(accel.x * tiltSensitivity, -1f, 1f);
            }
            else
            {
                ReadStickInput();
            }
        }

        private void ReadButtonInput()
        {
            float left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f;
            float right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f;
            steerInput = left + right;
        }

        private void ReadStickInput()
        {
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                steerInput = gamepad.leftStick.x.ReadValue();
            }
            else
            {
                steerInput = Input.GetAxis("Horizontal");
            }
        }

        private void ReadThrottleAndBrake()
        {
            var gamepad = Gamepad.current;

            if (gamepad != null)
            {
                throttleInput = gamepad.rightTrigger.ReadValue();
                brakeInput = gamepad.leftTrigger.ReadValue();
                driftInput = gamepad.buttonEast.isPressed;
            }
            else
            {
                if (autoAccelerate)
                {
                    throttleInput = 1f;
                    brakeInput = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.S) ? 1f : 0f;
                }
                else
                {
                    throttleInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f;
                    brakeInput = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f;
                }

                driftInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }
        }

        public void OnSteerLeft()
        {
            if (steeringMode == SteeringMode.Buttons)
                steerInput = -1f;
        }

        public void OnSteerRight()
        {
            if (steeringMode == SteeringMode.Buttons)
                steerInput = 1f;
        }

        public void OnSteerRelease()
        {
            if (steeringMode == SteeringMode.Buttons)
                steerInput = 0f;
        }

        public void OnBrakeDown() => brakeInput = 1f;
        public void OnBrakeUp() => brakeInput = 0f;
        public void OnDriftDown() => driftInput = true;
        public void OnDriftUp() => driftInput = false;
    }
}
