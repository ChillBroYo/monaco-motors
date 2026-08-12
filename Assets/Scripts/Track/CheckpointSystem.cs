using System;
using UnityEngine;

namespace MonacoMotors.Track
{
    public class CheckpointSystem : MonoBehaviour
    {
        [SerializeField] private Checkpoint[] checkpoints;

        public int TotalCheckpoints => checkpoints?.Length ?? 0;

        public event Action<int, bool, GameObject> OnCheckpointTriggered;

        private void Start()
        {
            InitializeCheckpoints();
        }

        private void InitializeCheckpoints()
        {
            if (checkpoints == null) return;

            for (int i = 0; i < checkpoints.Length; i++)
            {
                int index = i;
                bool isFinishLine = (i == 0);
                checkpoints[i].OnTriggered += (go) => OnCheckpointTriggered?.Invoke(index, isFinishLine, go);
            }
        }

        public Vector3 GetCheckpointPosition(int index)
        {
            if (checkpoints == null || index < 0 || index >= checkpoints.Length)
                return Vector3.zero;

            return checkpoints[index].transform.position;
        }

        public int GetNextCheckpoint(int current)
        {
            return (current + 1) % TotalCheckpoints;
        }
    }
}
