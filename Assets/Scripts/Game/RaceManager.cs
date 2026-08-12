using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonacoMotors.Game
{
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }

        public enum RaceState
        {
            Countdown,
            Racing,
            Finished
        }

        [Header("Race Settings")]
        [SerializeField] private int totalLaps = 3;
        [SerializeField] private int countdownSeconds = 3;
        [SerializeField] private int aiOpponentCount = 4;

        [Header("References")]
        [SerializeField] private Transform[] startPositions;

        private RaceState currentState;
        private float raceTime;
        private float countdownTimer;
        private List<RacerData> racers = new List<RacerData>();

        public RaceState CurrentState => currentState;
        public float RaceTime => raceTime;
        public int TotalLaps => totalLaps;
        public List<RacerData> Racers => racers;

        public event Action<RaceState> OnRaceStateChanged;
        public event Action<int> OnCountdownTick;
        public event Action<RacerData> OnRacerFinished;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeRace();
        }

        private void Update()
        {
            switch (currentState)
            {
                case RaceState.Countdown:
                    UpdateCountdown();
                    break;
                case RaceState.Racing:
                    UpdateRacing();
                    break;
            }
        }

        private void InitializeRace()
        {
            currentState = RaceState.Countdown;
            countdownTimer = countdownSeconds;
            raceTime = 0f;
            racers.Clear();

            SpawnRacers();
            OnRaceStateChanged?.Invoke(currentState);
        }

        private void SpawnRacers()
        {
            // TODO: Spawn player and AI cars at start positions
        }

        private void UpdateCountdown()
        {
            countdownTimer -= Time.deltaTime;
            int secondsLeft = Mathf.CeilToInt(countdownTimer);

            if (secondsLeft != Mathf.CeilToInt(countdownTimer + Time.deltaTime))
            {
                OnCountdownTick?.Invoke(secondsLeft);
            }

            if (countdownTimer <= 0f)
            {
                StartRace();
            }
        }

        private void StartRace()
        {
            currentState = RaceState.Racing;
            OnRaceStateChanged?.Invoke(currentState);
        }

        private void UpdateRacing()
        {
            raceTime += Time.deltaTime;
            UpdatePositions();
        }

        private void UpdatePositions()
        {
            // Sort racers by progress (lap * checkpoints + current checkpoint)
            racers.Sort((a, b) =>
            {
                float progressA = a.currentLap * 1000 + a.lastCheckpoint;
                float progressB = b.currentLap * 1000 + b.lastCheckpoint;
                return progressB.CompareTo(progressA);
            });

            for (int i = 0; i < racers.Count; i++)
            {
                racers[i].position = i + 1;
            }
        }

        public void OnCheckpointReached(RacerData racer, int checkpointIndex, bool isFinishLine)
        {
            racer.lastCheckpoint = checkpointIndex;

            if (isFinishLine && checkpointIndex == 0 && racer.lastCheckpoint > 0)
            {
                racer.currentLap++;

                if (racer.currentLap > totalLaps)
                {
                    racer.finishTime = raceTime;
                    racer.hasFinished = true;
                    OnRacerFinished?.Invoke(racer);

                    CheckRaceComplete();
                }
            }
        }

        private void CheckRaceComplete()
        {
            bool allFinished = racers.TrueForAll(r => r.hasFinished);
            if (allFinished)
            {
                currentState = RaceState.Finished;
                OnRaceStateChanged?.Invoke(currentState);
            }
        }

        public int GetPlayerPosition()
        {
            var player = racers.Find(r => r.isPlayer);
            return player?.position ?? 0;
        }

        public int GetPlayerLap()
        {
            var player = racers.Find(r => r.isPlayer);
            return player?.currentLap ?? 1;
        }
    }

    [Serializable]
    public class RacerData
    {
        public string racerId;
        public string displayName;
        public bool isPlayer;
        public int position;
        public int currentLap = 1;
        public int lastCheckpoint;
        public float finishTime;
        public bool hasFinished;
        public Transform transform;
    }
}
