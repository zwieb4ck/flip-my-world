using UnityEngine;
using System.Collections.Generic;

public class LevelController:MonoBehaviour {
    public List<Level> LevelList;
    public int CurrentLevelIndex = 0;
    public bool repositionLevelsInEditor = false;
    public float levelDistance = 25f;
    public int levelsReached = 0;
    public void Start() {
    }
    public Level CurrentLevel {
        get {
            if (LevelList[CurrentLevelIndex] != null && CurrentLevelIndex < LevelList.Count) {
                return LevelList[CurrentLevelIndex];
            }
            return null;
        }
    }
    public void repositionLevel() {
        for (int i = 0; i < LevelList.Count; i++) {
            LevelList[i].transform.position = new Vector3(i * levelDistance, 0f, 0f);
        }
    }

    private void OnDrawGizmos() {
        if (repositionLevelsInEditor) {
            repositionLevel();
            repositionLevelsInEditor = false;
        }
    }

    public void DisableAllMiniMapWrapper() {
        foreach (var level in LevelList) {
            level.MiniMapWrapper.SetActive(false);
        }
    }
}
