using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonacoMotors.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string garageScene = "Garage";
        [SerializeField] private string raceScene = "Race";
        [SerializeField] private string loadingScene = "Loading";

        private SaveData currentSave;

        public SaveData CurrentSave => currentSave;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadGame();
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene(mainMenuScene);
        }

        public void LoadGarage()
        {
            SceneManager.LoadScene(garageScene);
        }

        public void LoadRace(string trackId, string carId)
        {
            PlayerPrefs.SetString("SelectedTrack", trackId);
            PlayerPrefs.SetString("SelectedCar", carId);
            SceneManager.LoadScene(raceScene);
        }

        public void SaveGame()
        {
            if (currentSave == null) return;

            string json = JsonUtility.ToJson(currentSave, true);
            System.IO.File.WriteAllText(GetSavePath(), json);
            Debug.Log("Game saved.");
        }

        public void LoadGame()
        {
            string path = GetSavePath();

            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                currentSave = JsonUtility.FromJson<SaveData>(json);
                Debug.Log("Game loaded.");
            }
            else
            {
                currentSave = new SaveData();
                currentSave.Initialize();
                Debug.Log("New save created.");
            }
        }

        private string GetSavePath()
        {
            return System.IO.Path.Combine(Application.persistentDataPath, "save.json");
        }

        public void AddCredits(int amount)
        {
            currentSave.credits += amount;
            SaveGame();
        }

        public bool SpendCredits(int amount)
        {
            if (currentSave.credits >= amount)
            {
                currentSave.credits -= amount;
                SaveGame();
                return true;
            }
            return false;
        }
    }
}
