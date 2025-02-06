using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class LevelSelectComponennt:MonoBehaviour {
    public Button buttonLeft;
    public Button buttonRight;

    public LevelController levelController;
    public CameraController cameraController;
    public TextMeshProUGUI buttonLeftText;
    public TextMeshProUGUI buttonRightText;
    public TextMeshProUGUI displayText;

    public InputActionReference levelChoose;

    public bool triggeredLeft;
    public bool triggeredRight;
    public bool actionPerformed = false;
    private void Update() {
        if (levelController.CurrentLevelIndex == 0) {
            buttonLeft.interactable = false;
            buttonLeftText.color = new Color(255, 255, 255, 0.2f);
        } else {
            buttonLeft.interactable = true;
            buttonLeftText.color = new Color(255, 255, 255, 1);
        }

        if (levelController.CurrentLevelIndex == levelController.levelsReached) {
            buttonRight.interactable = false;
            buttonRightText.color = new Color(255, 255, 255, .2f);
        } else {
            buttonRight.interactable = true;
            buttonRightText.color = new Color(255, 255, 255, 1);
        }

        if(triggeredLeft && !actionPerformed) {
            PrevLevel();
            actionPerformed = true;
        }
        if(triggeredRight && !actionPerformed) {
            NextLevel();
            actionPerformed = true;
        }
        int LevelCount = (levelController.CurrentLevelIndex + 1);
        displayText.text = levelController.CurrentLevel.isTutorial ? "Tutorial " + LevelCount.ToString() : "Level " + (LevelCount - 3).ToString();
    }

    public void OnEnable() {
        // Event abonnieren
        if (levelChoose != null && levelChoose.action != null) {
            levelChoose.action.performed += OnActionPerformed;
        }

    }
    public void OnDisable() {
        // Event abbestellen
        if (levelChoose != null && levelChoose.action != null) {
            levelChoose.action.performed -= OnActionPerformed;
        }
    }

    public  void OnActionPerformed(InputAction.CallbackContext context) {
        Vector2 inputValue = context.ReadValue<Vector2>();

        // Prüfen, ob links oder rechts gedrückt wurde
        if (inputValue.x < -0.5f) // Links
        {
            triggeredLeft = true;
            triggeredRight = false;
        } else if (inputValue.x > 0.5f) // Rechts
          {
            triggeredRight = true;
            triggeredLeft = false;
        } else {
            // Keine Richtung gedrückt
            triggeredLeft = false;
            triggeredRight = false;
            actionPerformed = false;
        }

    }

    public void NextLevel() {
        if (levelController.CurrentLevelIndex == levelController.levelsReached) return;
        levelController.CurrentLevelIndex++;
        showLevel();
    }
    public void PrevLevel() {
        if (levelController.CurrentLevelIndex == 0) return;
        levelController.CurrentLevelIndex--;
        showLevel();
    }

    public void ShowFirstLevel() {
        showLevel();
    }
    private void showLevel() {
        levelController.CurrentLevel.ResetLevel();
        cameraController.SetFocusObject(levelController.CurrentLevel.SpawnPoint.transform);
    }
}
