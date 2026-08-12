using System.Collections.Generic;
using UnityEngine;

namespace MonacoMotors.Vehicle
{
    [CreateAssetMenu(fileName = "CarDatabase", menuName = "Monaco Motors/Car Database")]
    public class CarDatabase : ScriptableObject
    {
        [SerializeField] private List<CarEntry> cars = new List<CarEntry>();

        public List<CarEntry> Cars => cars;

        public CarEntry GetCar(string carId)
        {
            return cars.Find(c => c.carId == carId);
        }

        public List<CarEntry> GetCarsByClass(string className)
        {
            return cars.FindAll(c => c.stats.className == className);
        }
    }

    [System.Serializable]
    public class CarEntry
    {
        public string carId;
        public VehicleStats stats;
        public GameObject prefab;
        public Sprite thumbnail;
        public int purchasePrice;
        public bool starterCar;
    }
}
