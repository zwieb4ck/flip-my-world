using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController:MonoBehaviour {
    [Header("Controller References")]
    public CameraController CameraController;
    public UiController UiController;
    public PlayerController PlayerController;
    public LevelController LevelController;

    [Header("MiniMap")]
    public GameObject MiniMapCamera;

    // private
    private PlayerInput playerInput;
    public ECurrentView currentView = ECurrentView.MainMenu;
    private ECurrentView lastView = ECurrentView.MainMenu;
    private int rotations = 0;

    private ECurrentView[] pauseEnabledInView = new[] { ECurrentView.Game, ECurrentView.PauseMenu };

    private void Start() {
        playerInput = GetComponent<PlayerInput>();
        AddBindings();
        if (PlayerController != null) {
            PlayerController.OnLevelCompleted += HandleLevelCompleted;
            PlayerController.OnLevelFailed += HandleLevelFailed;
        }
        LevelController.levelsReached = PlayerPrefs.GetInt("LevelsReached", 0);
    }
    private EPlayerOrientation currentOrientation = EPlayerOrientation.None;

    private void Update() {
        // Hole die aktuelle Orientierung vom CameraController
        EPlayerOrientation newOrientation = CameraController.GetOrientation();

        // Aktualisiere nur, wenn sich die Orientierung geändert hat
        if (newOrientation != currentOrientation) {
            currentOrientation = newOrientation;

            // Informiere den PlayerController über die neue Orientierung
        }

        if (LevelController.CurrentLevel && LevelController.CurrentLevel.HasMaxRotations) {
            UiController.SetCurrentRotations(LevelController.CurrentLevel.maxRotations - rotations);
        }

    }

    private void FixedUpdate() {
        if (currentView == ECurrentView.Game) {
            PlayerController.AddInput(playerInput.actions["Move"].ReadValue<Vector2>());
        }
        switch (CameraController.currentFlipState) {
            case 0:
                UiController.FlipHud(Quaternion.Euler(0, 0, 0));
                break;
            case 1:
                UiController.FlipHud(Quaternion.Euler(0, 0, -90));
                break;
            case 2:
                UiController.FlipHud(Quaternion.Euler(0, 0, 180));
                break;
            case 3:
                UiController.FlipHud(Quaternion.Euler(0, 0, 90));
                break;
        }
    }

    public void SetCurrentView(ECurrentView newView) {
        lastView = currentView;
        currentView = newView;
    }
    public void StartGame() {
        StartLevel(0);
    }

    public void RestartLevel() {
        ResetLevel();
        StartLevel(LevelController.CurrentLevelIndex);
    }

    public void StartNextLevel() {
        LevelController.CurrentLevelIndex++;
        StartLevel(LevelController.CurrentLevelIndex);
    }

    public void StartLevel(int index) {
        PlayerController.RemoveCapsule();
        UiController.HideAllMenus();
        SetCurrentView(ECurrentView.Game);
        LevelController.CurrentLevelIndex = index;
        Level level = LevelController.CurrentLevel;
        rotations = 0;
        if (level != null) {
            PlayerController.AddCapsule(level.LevelCapsule);
            PlayerController.ResetPositionTo(level.SpawnPoint.transform.position);

            // Kamera-Animation zum Zielpunkt starten
            CameraController.SetFocusObject(level.SpawnPoint.transform);
            CameraController.SetFollowObject(level.LevelCapsule.transform);
            CameraController.ResetCameraRotation();

            // minimap
            LevelController.DisableAllMiniMapWrapper();
            level.MiniMapWrapper.SetActive(true);

            MiniMapCamera.transform.position = level.MiniMapTarget.transform.position;
            MiniMapCamera.transform.rotation = level.MiniMapTarget.transform.rotation;
        } else {
            Debug.Log("Level not found");
        }
    }

    public void AddBindings() {
        playerInput.actions["Pause"].performed += OnPause;
        playerInput.actions["RotateDown"].performed += OnRotateDown;
    }

    public void RemoveBindings() {
        playerInput.actions["Pause"].performed -= OnPause;
        playerInput.actions["RotateDown"].performed -= OnRotateDown;
    }

    public void Pause() {
        UiController.ShowPauseMenu();
        SetCurrentView(ECurrentView.PauseMenu);
    }

    public void Resume() {
        UiController.HideAllMenus();
        SetCurrentView(lastView);
    }

    public void BackToMenu() {
        CameraController.SetFollowObject(null);
        CameraController.ResetCameraRotation();
        currentView = ECurrentView.MainMenu;
        UiController.HideAllMenus();
        CameraController.SetFocusObject(CameraController.initialCameraPoint);
        UiController.ShowMainMenu();
    }

    public void ResetLevel() {
        LevelController.CurrentLevel.ResetLevel();
    }

    public void ShowLevelSelectMenu() {
        currentView = ECurrentView.MainMenu;
        CameraController.SetFollowObject(null);
        CameraController.ResetCameraRotation();
        UiController.HideAllMenus();
        UiController.ShowLevelSelectMenu();
    }
    public void Quit() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnPause(InputAction.CallbackContext context) {
        if (pauseEnabledInView.Contains(currentView)) {
            if (UiController.PauseMenu.activeSelf) {
                Resume();
            } else {
                Pause();
            }
        }
    }

    private void OnRotateDown(InputAction.CallbackContext context) {
        if (currentView != ECurrentView.Game) {
            return;
        }

        if (LevelController.CurrentLevel.disableRotation) {
            return;
        };

        if (LevelController.CurrentLevel.HasLimitedRotations) {
            // Limited Rotations: Rotation immer erlaubt, solange unter dem Limit
            if (rotations < LevelController.CurrentLevel.limitRotations) {
                CameraController.Flip();
                LevelController.CurrentLevel.ShowFlipZones();
                rotations++;
            }
        } else if (LevelController.CurrentLevel.HasMaxRotations) {
            // Max Rotations: Rotation nur erlaubt, wenn unter dem Maximum
            if (rotations < LevelController.CurrentLevel.maxRotations) {
                CameraController.Flip();
                LevelController.CurrentLevel.ShowFlipZones();
                rotations++;
            } else {
                // Spieler verliert, wenn Maximum überschritten wird
                UiController.LoseMenu.SetActive(true);
                PlayerController.FailLevel();
            }
        } else {
            CameraController.Flip();
            LevelController.CurrentLevel.ShowFlipZones();
            rotations++;
        }
    }

    private void HandleLevelCompleted() {
        if (currentView == ECurrentView.Game) {
            CameraController.SetFocusObject(null);
            CameraController.SetFollowObject(null);
            if (LevelController.CurrentLevelIndex == LevelController.LevelList.Count() - 1) {
                currentView = ECurrentView.GameOverScreen;
                UiController.ShowGameOverMenu();
            } else {
                currentView = ECurrentView.WinScreen;
                UiController.ShowWinMenu();
                LevelController.levelsReached = Mathf.Clamp(LevelController.levelsReached + 1, 0, LevelController.LevelList.Count - 1);
                PlayerPrefs.SetInt("LevelsReached", LevelController.levelsReached);
            }
        }
    }

    private void HandleLevelFailed() {
        if (currentView == ECurrentView.Game) {
            Failed();
        }
    }

    private void Failed() {
        CameraController.SetFocusObject(null);
        CameraController.SetFollowObject(null);
        currentView = ECurrentView.LoseScreen;
        UiController.ShowLoseMenu();
    }

    private void OnDestroy() {
        if (PlayerController != null) {
            PlayerController.OnLevelCompleted -= HandleLevelCompleted;
            PlayerController.OnLevelFailed -= HandleLevelFailed;
        }
    }
}
