using UnityEngine;
using UnityEngine.UIElements;

public class Level:MonoBehaviour {
    public int LevelIndex = 0;
    public bool isTutorial = false;
    public bool disableRotation = false;

    public GameObject SpawnPoint;
    public GameObject LevelCapsule;
    public bool HasLimitedRotations = false;
    public int limitRotations = int.MaxValue;
    public bool HasMaxRotations = true;
    public int maxRotations = 3;
    [Header("MiniMap")]
    public GameObject MiniMapWrapper;
    public GameObject MiniMapTarget;

    [Header("Flipzones")]

    public GameObject FlipZone;

    public void Start() {
        if (SpawnPoint == null) {
            Debug.Log($"Level with index {LevelIndex} missing a spawn point");
        }
    }

    public void ResetLevel() {
        LevelCapsule.transform.position = SpawnPoint.transform.position;
        LevelCapsule.transform.rotation = SpawnPoint.transform.rotation;
        if (FlipZone != null) {
            FlipZone.SetActive(true);
        }
    }

    public void HideFlipZones() {
        if (FlipZone != null) {
            FlipZone.SetActive(false);
        }
    }

    public void ShowFlipZones() {
        if (FlipZone != null) {
            FlipZone.SetActive(true);
        }
    }
}
