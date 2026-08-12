using UnityEngine;

namespace MonacoMotors.Vehicle
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private VehicleStats stats;

        [Header("Wheel Raycasts")]
        [SerializeField] private Transform[] wheelTransforms;
        [SerializeField] private float suspensionRestLength = 0.5f;
        [SerializeField] private float suspensionStiffness = 30000f;
        [SerializeField] private float suspensionDamping = 4000f;
        [SerializeField] private float wheelRadius = 0.35f;

        [Header("Engine")]
        [SerializeField] private AnimationCurve powerCurve;
        [SerializeField] private float maxMotorTorque = 3000f;

        [Header("Steering")]
        [SerializeField] private float maxSteerAngle = 35f;
        [SerializeField] private float steerSpeed = 5f;

        [Header("Drift")]
        [SerializeField] private float driftGripMultiplier = 0.6f;
        [SerializeField] private float normalGripMultiplier = 1f;

        private Rigidbody rb;
        private float currentSpeed;
        private float steerInput;
        private float throttleInput;
        private float brakeInput;
        private bool isDrifting;
        private float currentSteerAngle;

        public float CurrentSpeedKmh => currentSpeed * 3.6f;
        public float NormalizedSpeed => Mathf.Clamp01(CurrentSpeedKmh / stats.topSpeed);
        public bool IsDrifting => isDrifting;
        public VehicleStats Stats => stats;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = stats.mass;
            rb.centerOfMass = stats.centerOfMassOffset;

            if (powerCurve == null || powerCurve.length == 0)
            {
                powerCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.2f);
            }
        }

        private void FixedUpdate()
        {
            currentSpeed = rb.linearVelocity.magnitude;

            ApplySuspension();
            AppleSteering();
            ApplyDrive();
            ApplyBraking();
            ApplyDrift();
        }

        public void SetInput(float steer, float throttle, float brake, bool drift)
        {
            steerInput = Mathf.Clamp(steer, -1f, 1f);
            throttleInput = Mathf.Clamp01(throttle);
            brakeInput = Mathf.Clamp01(brake);
            isDrifting = drift;
        }

        private void ApplySuspension()
        {
            foreach (var wheel in wheelTransforms)
            {
                if (Physics.Raycast(wheel.position, -wheel.up, out RaycastHit hit, suspensionRestLength + wheelRadius))
                {
                    float compression = (suspensionRestLength + wheelRadius - hit.distance) / suspensionRestLength;
                    float springForce = compression * suspensionStiffness;

                    float velocity = Vector3.Dot(rb.GetPointVelocity(wheel.position), wheel.up);
                    float damperForce = velocity * suspensionDamping;

                    float totalForce = springForce - damperForce;
                    rb.AddForceAtPosition(wheel.up * totalForce, wheel.position);
                }
            }
        }

        private void AppleSteering()
        {
            float targetAngle = steerInput * maxSteerAngle;
            float speedFactor = 1f - (NormalizedSpeed * 0.5f);
            targetAngle *= speedFactor * stats.handling;

            currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle, Time.fixedDeltaTime * steerSpeed);

            if (wheelTransforms.Length >= 2)
            {
                wheelTransforms[0].localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
                wheelTransforms[1].localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
            }
        }

        private void ApplyDrive()
        {
            if (CurrentSpeedKmh >= stats.topSpeed) return;

            float normalizedSpeed = NormalizedSpeed;
            float powerMultiplier = powerCurve.Evaluate(normalizedSpeed);
            float accelerationMultiplier = 1f / stats.accelerationTime;

            float torque = throttleInput * maxMotorTorque * powerMultiplier * accelerationMultiplier;
            rb.AddForce(transform.forward * torque);
        }

        private void ApplyBraking()
        {
            if (brakeInput <= 0f) return;

            float brakeTorque = brakeInput * maxMotorTorque * stats.braking * 0.5f;
            Vector3 brakeForce = -rb.linearVelocity.normalized * brakeTorque;
            rb.AddForce(brakeForce);
        }

        private void ApplyDrift()
        {
            Vector3 forwardVelocity = Vector3.Project(rb.linearVelocity, transform.forward);
            Vector3 sidewaysVelocity = rb.linearVelocity - forwardVelocity;

            float gripMultiplier = isDrifting ? driftGripMultiplier : normalGripMultiplier;
            gripMultiplier *= (1f - stats.driftFactor * 0.5f);

            Vector3 correctedVelocity = forwardVelocity + sidewaysVelocity * gripMultiplier;
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, correctedVelocity, Time.fixedDeltaTime * 10f);
        }
    }
}
