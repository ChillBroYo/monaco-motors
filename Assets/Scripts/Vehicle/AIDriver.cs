using UnityEngine;

namespace MonacoMotors.Vehicle
{
    [RequireComponent(typeof(VehicleController))]
    public class AIDriver : MonoBehaviour
    {
        [Header("Path Following")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float waypointThreshold = 5f;
        [SerializeField] private float lookAheadDistance = 20f;

        [Header("Behavior")]
        [SerializeField] private float baseSpeed = 0.8f;
        [SerializeField] private float cornerSlowdown = 0.5f;
        [SerializeField] private float rubberBandStrength = 0.3f;
        [SerializeField] private float mistakeChance = 0.05f;

        private VehicleController vehicle;
        private int currentWaypointIndex;
        private float throttle = 1f;
        private float mistakeTimer;
        private bool isMakingMistake;

        private void Awake()
        {
            vehicle = GetComponent<VehicleController>();
        }

        private void Update()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            UpdateMistakes();
            FollowPath();
        }

        private void FollowPath()
        {
            Vector3 targetPosition = GetTargetPosition();
            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0f;

            float steer = CalculateSteering(toTarget);
            throttle = CalculateThrottle(toTarget);

            if (isMakingMistake)
            {
                steer *= 0.5f;
                throttle *= 0.7f;
            }

            vehicle.SetInput(steer, throttle, 0f, false);

            if (toTarget.magnitude < waypointThreshold)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }

        private Vector3 GetTargetPosition()
        {
            if (currentWaypointIndex >= waypoints.Length) return transform.position;

            Vector3 currentWp = waypoints[currentWaypointIndex].position;
            int nextIndex = (currentWaypointIndex + 1) % waypoints.Length;
            Vector3 nextWp = waypoints[nextIndex].position;

            float distToCurrent = Vector3.Distance(transform.position, currentWp);
            float t = Mathf.Clamp01((lookAheadDistance - distToCurrent) / lookAheadDistance);

            return Vector3.Lerp(currentWp, nextWp, t);
        }

        private float CalculateSteering(Vector3 toTarget)
        {
            Vector3 localTarget = transform.InverseTransformDirection(toTarget.normalized);
            return Mathf.Clamp(localTarget.x * 2f, -1f, 1f);
        }

        private float CalculateThrottle(Vector3 toTarget)
        {
            float angle = Vector3.Angle(transform.forward, toTarget.normalized);
            float cornerFactor = 1f - (angle / 90f) * cornerSlowdown;
            return Mathf.Clamp(baseSpeed * cornerFactor, 0.3f, 1f);
        }

        private void UpdateMistakes()
        {
            mistakeTimer -= Time.deltaTime;

            if (mistakeTimer <= 0f)
            {
                if (Random.value < mistakeChance)
                {
                    isMakingMistake = true;
                    mistakeTimer = Random.Range(0.5f, 1.5f);
                }
                else
                {
                    isMakingMistake = false;
                    mistakeTimer = Random.Range(2f, 5f);
                }
            }
        }

        public void SetWaypoints(Transform[] points)
        {
            waypoints = points;
            currentWaypointIndex = 0;
        }

        public void ApplyRubberBand(float distanceToPlayer, bool playerAhead)
        {
            float adjustment = Mathf.Clamp01(distanceToPlayer / 100f) * rubberBandStrength;

            if (playerAhead)
            {
                baseSpeed = Mathf.Min(1f, 0.8f + adjustment);
            }
            else
            {
                baseSpeed = Mathf.Max(0.5f, 0.8f - adjustment);
            }
        }
    }
}
