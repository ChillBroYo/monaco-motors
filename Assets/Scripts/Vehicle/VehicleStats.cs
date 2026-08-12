using UnityEngine;

namespace MonacoMotors.Vehicle
{
    [CreateAssetMenu(fileName = "NewVehicleStats", menuName = "Monaco Motors/Vehicle Stats")]
    public class VehicleStats : ScriptableObject
    {
        [Header("Performance")]
        [Tooltip("Maximum speed in km/h")]
        [Range(150f, 400f)]
        public float topSpeed = 220f;

        [Tooltip("Acceleration feel (lower = faster 0-100)")]
        [Range(2f, 8f)]
        public float accelerationTime = 4.5f;

        [Tooltip("Turn responsiveness")]
        [Range(0.5f, 1.5f)]
        public float handling = 1f;

        [Tooltip("How easily the car slides")]
        [Range(0.1f, 1f)]
        public float driftFactor = 0.5f;

        [Tooltip("Braking power")]
        [Range(0.5f, 1.5f)]
        public float braking = 1f;

        [Header("Physics")]
        [Tooltip("Vehicle mass in kg")]
        [Range(800f, 2500f)]
        public float mass = 1400f;

        [Tooltip("Center of mass offset (lower = more stable)")]
        public Vector3 centerOfMassOffset = new Vector3(0f, -0.3f, 0.2f);

        [Header("Visuals")]
        public string displayName = "Vehicle";
        public string className = "Sports";
        [TextArea(2, 4)]
        public string description = "A balanced sports car.";
    }
}
