using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiController:MonoBehaviour {
    [Header("Setup")]
    public EventSystem EventSystem;
    [Header("Main Menu")]
    public GameObject MainMenu;
    public Button MainMenuFirstButton;
    [Header("Level Select Menu")]
    public GameObject LevelSelectMenu;
    public Button LevelSelectMenuFirstButton;
    [Header("Pause Menu")]
    public GameObject PauseMenu;
    public Button PauseMenuFirstButton;
    [Header("Win Menu")]
    public GameObject WinMenu;
    public Button WinMenuFirstButton;
    [Header("Lose Menu")]
    public GameObject LoseMenu;
    public Button LoseMenuFirstButton;
    [Header("GameOver Menu")]
    public GameObject GameOverMenu;
    public Button GameOverMenuFirstButton;
    [Header("Hud")]
    public GameObject Hud;
    public GameObject Outline;
    public GameObject FlipsWrapper;
    public TextMeshProUGUI FlipsLeft;
    public TextMeshProUGUI Timer;
    [Header("Audio")]
    public AudioController audioController;

    private Quaternion targetRotation = Quaternion.identity;

    public void Start() {
        ShowMainMenu();
    }

    public void Update() {
        if (Hud != null && Hud.activeSelf) {
            Outline.transform.rotation = Quaternion.Lerp(Outline.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    public void ShowPauseMenu() {
        PauseMenu.SetActive(true);
        PauseMenuFirstButton.Select();
    }

    public void ShowMainMenu() {
        Hud.SetActive(false);
        MainMenu.SetActive(true);
        MainMenuFirstButton.Select();
    }

    public void ShowWinMenu() {
        audioController.playWin();
        WinMenu.SetActive(true);
        StartCoroutine(EnableALlButtonsAfterDelay(WinMenu, WinMenuFirstButton));
    }

    public void ShowLevelSelectMenu() {
        LevelSelectMenu.SetActive(true);
        LevelSelectMenu.GetComponent<LevelSelectComponennt>().ShowFirstLevel();
        StartCoroutine(EnableALlButtonsAfterDelay(LevelSelectMenu, LevelSelectMenuFirstButton));
    }

    public void ShowLoseMenu() {
        audioController.playLose();
        LoseMenu.SetActive(true);
        StartCoroutine(EnableALlButtonsAfterDelay(LoseMenu, LoseMenuFirstButton));
    }
    public void ShowGameOverMenu() {
        audioController.playgameOver();
        GameOverMenu.SetActive(true);
        StartCoroutine(EnableALlButtonsAfterDelay(GameOverMenu, GameOverMenuFirstButton));
    }

    public void HideAllMenus() {
        MainMenu?.SetActive(false);
        PauseMenu?.SetActive(false);
        DisableAlllButtonsInMenu(LoseMenu);
        LoseMenu?.SetActive(false);
        LevelSelectMenu?.SetActive(false);
        DisableAlllButtonsInMenu(LevelSelectMenu);
        GameOverMenu?.SetActive(false);
        DisableAlllButtonsInMenu(GameOverMenu);
        WinMenu?.SetActive(false);
        DisableAlllButtonsInMenu(WinMenu);
        ResetHud();
        // set hud active
        Hud.SetActive(true);
    }

    public void FlipHud(Quaternion roation) {
        targetRotation = roation;
    }

    public void ResetHud() {
        FlipsWrapper.SetActive(false);
        FlipsLeft.text = "1";
    }

    public void SetCurrentRotations(int rotations) {
        if(!FlipsWrapper.activeSelf) {
            FlipsWrapper.SetActive(true);
        }
        FlipsLeft.text = rotations.ToString();
    }

    private IEnumerator EnableALlButtonsAfterDelay(GameObject Menu, Button firstButton) {
        yield return new WaitForSeconds(0.5f);
        Button[] buttons = Menu.GetComponentsInChildren<Button>();
        foreach (Button button in buttons) {
            button.interactable = true;
        }
        firstButton.Select();
    }

    private void DisableAlllButtonsInMenu(GameObject Menu) {
        if (!Menu.activeSelf)
            Menu.SetActive(true); // Aktivieren, falls inaktiv

        Button[] buttons = Menu.GetComponentsInChildren<Button>();
        foreach (Button button in buttons) {
            button.interactable = false;
        }

        Menu.SetActive(false); // Wieder deaktivieren
    }
}
