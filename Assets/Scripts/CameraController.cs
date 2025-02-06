using UnityEngine;
using System.Collections;

public class CameraController:MonoBehaviour {
    [Header("Camera Settings")]
    public Transform cameraTarget; // Das Ziel der Kamera (Eltern der Main Camera)
    public Transform backgroundWrapper; // Der Hintergrund-Würfel

    [Header("Movement Settings")]
    public float cameraDistance = 10f; // Abstand zur Kamera
    public float followSpeed = 5f; // Lerping-Geschwindigkeit

    [Header("Camera Shake")]
    public float duration = 0.25f;
    public float intensity = 0.25f;

    [Header("Optional Settings")]
    public Transform initialCameraPoint; // Startpunkt der Kamera
    public Transform focusObject; // Fokusobjekt für einmalige Animation
    public Transform followObject; // Folgeobjekt für kontinuierliche Position

    private bool isFlipping = false; // Flag für Flip-Animation
    private bool isFocusing = false; // Flag für Fokus-Animation

    public int currentFlipState;

    [Header("Sound")]
    public AudioController audioController;

    [Header("Debug")]
    public Transform debugTargetPostion;
    public bool jumpToTarget = false;

    private void Start() {
        // Setze die Kamera auf den Initialpunkt, falls vorhanden
        if (initialCameraPoint != null) {
            AdjustTargetPosition(initialCameraPoint);
        }
    }

    private void Update() {
        // Folgebewegung
        if (followObject != null && !isFocusing) {
            cameraTarget.position = Vector3.Lerp(
                cameraTarget.position,
                followObject.position,
                Time.deltaTime * followSpeed
            );
        }

        // Fokusbewegung (Animation zum Startpunkt)
        if (focusObject != null && isFocusing) {
            cameraTarget.position = Vector3.Lerp(
                cameraTarget.position,
                focusObject.position,
                Time.deltaTime * followSpeed
            );

            cameraTarget.rotation = Quaternion.Slerp(
               cameraTarget.rotation,
               focusObject.rotation,
               Time.deltaTime * followSpeed
            );

            // Animation beenden, wenn fast erreicht
            if (Vector3.Distance(cameraTarget.position, focusObject.position) < 0.01f) {
                isFocusing = false;
                focusObject = null;
            }
        }

        // Hintergrund mit der Kamera bewegen
        if (backgroundWrapper != null) {
            backgroundWrapper.position = cameraTarget.position;
        }
    }

    public void SetFocusObject(Transform newFocusObject) {
        focusObject = newFocusObject;

        if (focusObject != null) {
            isFocusing = true;
        }
    }


    public void SetFollowObject(Transform newFollowObject) {
        followObject = newFollowObject;
    }

    public void Flip() {
        if (isFlipping) return; // Verhindere mehrfaches Flippen
        audioController.playRotateCamera();
        currentFlipState = (currentFlipState + 1) % 4; // Zyklisch zwischen den 4 Zuständen wechseln
        StartCoroutine(FlipCoroutine());
    }


    private IEnumerator FlipCoroutine() {
        isFlipping = true;

        Quaternion[] flipRotations = new Quaternion[] {
        Quaternion.Euler(0f, 0f, 0f),
        Quaternion.Euler(90f, 0f, 0f),
        Quaternion.Euler(180f, 0f, 0f),
        Quaternion.Euler(-90f, 0f, 0f),
    };

        Quaternion startRotation = cameraTarget.rotation;
        Quaternion endRotation = flipRotations[currentFlipState];

        bool flipSoundPlayed = false;
        float elapsedTime = 0f;
        while (elapsedTime < 1f) {
            elapsedTime += Time.deltaTime * followSpeed;

            // Dynamisch basierend auf dem aktuellen FlipState
            endRotation = flipRotations[currentFlipState];

            if (elapsedTime > 0.9f && !flipSoundPlayed) {
                // audioController.playRotateCamera();
                flipSoundPlayed = true;
            }

            cameraTarget.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime);
            yield return null;
        }

        cameraTarget.rotation = flipRotations[currentFlipState];
        isFlipping = false;
        ShakeCamera(duration, intensity);
    }

    public EPlayerOrientation GetOrientation() {
        float xRotation = Mathf.Repeat(transform.rotation.eulerAngles.x, 360f);

        if (Mathf.Approximately(xRotation, 0f)) {
            return EPlayerOrientation.Top;
        } else if (Mathf.Approximately(xRotation, 180f)) {
            return EPlayerOrientation.Bottom;
        } else if (Mathf.Approximately(xRotation, 90f)) {
            return EPlayerOrientation.Right;
        } else if (Mathf.Approximately(xRotation, 270f)) {
            return EPlayerOrientation.Left;
        }

        return EPlayerOrientation.None;
    }

    public void ResetCameraRotation() {
        // Stoppe alle laufenden Coroutines
        StopAllCoroutines();

        currentFlipState = 0; // Zurück auf die Ausgangsrotation
        isFlipping = false;
        isFocusing = false;

        cameraTarget.rotation = Quaternion.Euler(0f, 0f, 0f);

        // Debug-Ausgabe zur überprüfung
        
    }

    public void ShakeCamera(float duration, float magnitude) {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude) {
        Vector3 originalPosition = cameraTarget.localPosition; // Ursprüngliche Position der Kamera
        float elapsedTime = 0f;

        while (elapsedTime < duration) {
            // Generiere eine zufällige Versatzposition
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;
            float offsetZ = Random.Range(-1f, 1f) * magnitude;

            // Verschiebe die Kamera
            cameraTarget.localPosition = new Vector3(
                originalPosition.x + offsetX,
                originalPosition.y + offsetY,
                originalPosition.z + offsetZ
            );

            elapsedTime += Time.deltaTime;
            yield return null; // Warte auf den nächsten Frame
        }

        // Setze die Kamera zurück auf ihre ursprüngliche Position
        cameraTarget.localPosition = originalPosition;
    }

    public void AdjustTargetPosition(Transform targetPos) {
        cameraTarget.position = targetPos.position;
        cameraTarget.rotation = targetPos.rotation;
    }

    private void OnDrawGizmos() {
        if (debugTargetPostion != null && jumpToTarget) {
            AdjustTargetPosition(debugTargetPostion);
            jumpToTarget = false;
        }
    }
}
