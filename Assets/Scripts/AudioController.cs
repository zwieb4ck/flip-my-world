using System.Collections;
using UnityEngine;

public class AudioController:MonoBehaviour {
    [Header("Camera Rotate")]
    public AudioClip[] rotateCamera;
    public float rotateMinPitch = 1.25f;
    public float rotateMaxPitch = 1.75f;
    [Header("Player Drop")]
    public AudioClip[] playerDrop;
    public float dropMinPitch = 0.75f;
    public float dropMaxPitch = 1.25f;
    public AudioClip[] win;
    public AudioClip[] lose;
    public AudioClip[] gameOver;
    public AudioClip[] backgroundMusic;

    public AudioSource MusicSource;
    public AudioSource SFXSource;
    public AudioSource FootSteps;

    private void Start() {
        StartCoroutine(StartBackgroundMusic());
    }


    public void playRotateCamera() {
        SFXSource.pitch = 1;
        AudioClip randomClipFromList = rotateCamera[Random.Range(0, rotateCamera.Length - 1)];
        SFXSource.clip = randomClipFromList;
        SFXSource.Play(); // removed - does not sound right
    }

    public void playPlayerDrop() {
        SFXSource.pitch = 1;
        AudioClip randomClipFromList = playerDrop[Random.Range(0, playerDrop.Length - 1)];
        SFXSource.clip = randomClipFromList;
        SFXSource.Play();
    }
    public void playWin() {
        SFXSource.pitch = 1;
        AudioClip randomClipFromList = win[Random.Range(0, win.Length - 1)];
        SFXSource.clip = randomClipFromList;
        SFXSource.Play();
    }
    public void playLose() {
        SFXSource.pitch = 1;
        AudioClip randomClipFromList = lose[Random.Range(0, lose.Length - 1)];
        SFXSource.clip = randomClipFromList;
        SFXSource.Play();
    }
    public void playgameOver() {
        SFXSource.pitch = 1;
        AudioClip randomClipFromList = gameOver[Random.Range(0, gameOver.Length - 1)];
        SFXSource.clip = randomClipFromList;
        SFXSource.Play();
    }

    public IEnumerator StartBackgroundMusic() {
        float selectedVolume = MusicSource.volume;
        MusicSource.loop = true;
        MusicSource.clip = backgroundMusic[Random.Range(0, backgroundMusic.Length - 1)];
        MusicSource.volume = 0f; // Startlautstärke auf 0 setzen
        MusicSource.Play();

        float duration = 1.5f; // Dauer des Lautstärke-übergangs
        float elapsedTime = 0f;

        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;
            MusicSource.volume = Mathf.Lerp(0f, selectedVolume, elapsedTime / duration); // Lautstärke von 0 auf 1 interpolieren
            yield return null; // Warte auf den nächsten Frame
        }

        MusicSource.volume = selectedVolume; // Lautstärke sicherstellen
    }

    public void PlayFootSteps() {
        if (!FootSteps.isPlaying) { 
        FootSteps.Play();
        }
    }
    public void StopFootSteps() {
        FootSteps.Pause();
    }
}
