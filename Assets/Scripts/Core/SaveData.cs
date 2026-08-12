using System;
using System.Collections.Generic;

namespace MonacoMotors.Core
{
    [Serializable]
    public class SaveData
    {
        public int credits;
        public int xp;
        public int level;
        public string selectedCarId;
        public List<string> ownedCarIds;
        public List<string> unlockedTracks;
        public CareerProgress career;
        public PlayerSettings settings;

        public void Initialize()
        {
            credits = 5000;
            xp = 0;
            level = 1;
            selectedCarId = "vento_gt";
            ownedCarIds = new List<string> { "vento_gt" };
            unlockedTracks = new List<string> { "monaco_boulevard" };
            career = new CareerProgress();
            settings = new PlayerSettings();
        }

        public bool OwnsСar(string carId)
        {
            return ownedCarIds != null && ownedCarIds.Contains(carId);
        }

        public void UnlockCar(string carId)
        {
            if (ownedCarIds == null)
                ownedCarIds = new List<string>();

            if (!ownedCarIds.Contains(carId))
                ownedCarIds.Add(carId);
        }

        public void UnlockTrack(string trackId)
        {
            if (unlockedTracks == null)
                unlockedTracks = new List<string>();

            if (!unlockedTracks.Contains(trackId))
                unlockedTracks.Add(trackId);
        }
    }

    [Serializable]
    public class CareerProgress
    {
        public string currentLeague;
        public int currentSeries;
        public int currentRace;
        public List<string> completedRaces;
        public List<string> completedSeries;

        public CareerProgress()
        {
            currentLeague = "bronze";
            currentSeries = 0;
            currentRace = 0;
            completedRaces = new List<string>();
            completedSeries = new List<string>();
        }
    }

    [Serializable]
    public class PlayerSettings
    {
        public int steeringMode;
        public bool autoAccelerate;
        public float musicVolume;
        public float sfxVolume;
        public int graphicsQuality;

        public PlayerSettings()
        {
            steeringMode = 0;
            autoAccelerate = true;
            musicVolume = 0.8f;
            sfxVolume = 1f;
            graphicsQuality = 1;
        }
    }
}
