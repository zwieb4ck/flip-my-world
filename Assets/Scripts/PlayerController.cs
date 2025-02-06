using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class PlayerController:MonoBehaviour {
    [Header("Game Refrences")]
    public GameObject Mesh;

    [Header("Basic Settings")]
    public bool enableGravity = false;
    [Header("Character Settings")]
    public bool GracityEnabled = false;
    public float MovementSpeed = 10f;
    public float GravityMultiplier = 2f;
    private float baseGravity = -9.81f;
    private Vector3 moveDirection;
    private float rotationTolerance = 5f;
    private Vector3 velocity;     // Speichert die aktuelle Bewegungsgeschwindigkeit
    public Transform cameraTarget;
    float Gravity { get { return baseGravity * GravityMultiplier; } }
    Rigidbody rb;
    [Header("Raycast Settings")]
    public LayerMask groundLayer; // Der Layer für den Boden
    public float rayDistance = 1.5f; // Die Distanz des Raycasts
    public bool acceptInput = true;
    public bool active = false;
    public GameObject currentCapsule = null;
    public AudioController AudioController;

    public float lookDirection = 90f;

    public event System.Action OnLevelCompleted;
    public event System.Action OnLevelFailed;
    private bool wasFalling = false;

    public LevelController levelController;

    public void FlipGravity() {
        baseGravity = -baseGravity;
        Physics.gravity = new Vector3(0, baseGravity, 0);
    }

    public void Start() {
        rb = GetComponent<Rigidbody>();
    }

    private EPlayerOrientation CurrentView() {
        if (!active) return EPlayerOrientation.None;
        Quaternion topViewRotation = Quaternion.Euler(90f, 0f, 0f);
        Quaternion bottomViewRotation = Quaternion.Euler(-90f, 0f, 0f);
        Quaternion leftViewRotation = Quaternion.Euler(0f, 0f, 0f);
        Quaternion rightViewRotation = Quaternion.Euler(180f, 0f, 0f);

        if (Quaternion.Angle(cameraTarget.rotation, topViewRotation) < rotationTolerance) {
            return EPlayerOrientation.Top;
        } else if (Quaternion.Angle(cameraTarget.rotation, bottomViewRotation) < rotationTolerance) {
            return EPlayerOrientation.Bottom; // Korrektur: Gib Bottom zurück
        } else if (Quaternion.Angle(cameraTarget.rotation, leftViewRotation) < rotationTolerance) {
            return EPlayerOrientation.Left;
        } else if (Quaternion.Angle(cameraTarget.rotation, rightViewRotation) < rotationTolerance) {
            return EPlayerOrientation.Right;
        }

        return EPlayerOrientation.None;
    }

    // Überprüft, ob sich der Boden unter dem Spieler befindet
    public bool IsGrounded() {
        bool flipped = Gravity < 0;
        Vector3 origin = Mesh.transform.position + new Vector3(0, flipped ? 0.5f : 0f, 0);
        Ray ray = new Ray(origin, flipped ? Vector3.down : Vector3.up); // Ray nach unten
        return Physics.Raycast(ray, rayDistance, groundLayer);
    }

    public void Update() {
        Vector3 horizontalMovement = Vector3.zero;
        switch (CurrentView()) {
            case EPlayerOrientation.Left:
                horizontalMovement = new Vector3(moveDirection.x, 0f, 0f) * MovementSpeed;
                checkFalling();
                break;
            case EPlayerOrientation.Right:
                horizontalMovement = new Vector3(moveDirection.x, 0f, 0f) * MovementSpeed;
                checkFalling();
                break;
            case EPlayerOrientation.Top:
                horizontalMovement = new Vector3(moveDirection.x, 0f, moveDirection.y) * MovementSpeed;
                break;
            case EPlayerOrientation.Bottom:
                horizontalMovement = new Vector3(moveDirection.x, 0f, -moveDirection.y) * MovementSpeed;
                break;
            case EPlayerOrientation.None:
                horizontalMovement = Vector3.zero;
                break;
        }
        if (horizontalMovement.sqrMagnitude > 0.01f) {
            Mesh.transform.rotation = Quaternion.Slerp(
                Mesh.transform.rotation,
                Quaternion.LookRotation(horizontalMovement, Vector3.up),
                Time.deltaTime * 10f // Geschwindigkeit der Drehung
            );
        }

        if (CurrentView() == EPlayerOrientation.Left && Gravity > 0) {
            FlipGravity();
        } else if (CurrentView() == EPlayerOrientation.Right && Gravity < 0) {
            FlipGravity();
        }

        if (enableGravity) {
            velocity.y = Gravity * Time.deltaTime;
        } else {
            velocity.y = 0f;
        }
        Vector3 finalMovement = horizontalMovement;
        finalMovement.y = rb.linearVelocity.y + velocity.y;
        rb.linearVelocity = finalMovement;

        if (!IsGrounded()) {
            rb.constraints = RigidbodyConstraints.None;
            acceptInput = false;
            Vector3 antiGravityForce = (Gravity > 0 ? Vector3.up : Vector3.down) * 2;
            rb.AddForce(rb.linearVelocity.normalized + antiGravityForce, ForceMode.Impulse);
            FailLevel();
        }


        if (currentCapsule != null) {
            float currentWalkSpeed = Mathf.Abs(horizontalMovement.sqrMagnitude);
            currentCapsule.GetComponent<Animator>().SetFloat("Speed", currentWalkSpeed * 3);

            if (currentWalkSpeed > 0.1f) {
                AudioController.PlayFootSteps();
            } else {
                AudioController.StopFootSteps();
            }
            if(currentWalkSpeed > 1) {
                levelController.CurrentLevel.HideFlipZones();
            }

            bool isFlipped = Gravity < 0;
            Vector3 newPosition = isFlipped ? Mesh.transform.position : Mesh.transform.position + new Vector3(0, 0.5f, 0f);
            currentCapsule.transform.position = Vector3.Lerp(currentCapsule.transform.position, newPosition, Time.deltaTime * 20f);
            currentCapsule.transform.rotation = Mesh.transform.rotation;
            currentCapsule.transform.localScale = isFlipped ? new Vector3(0.25f, 0.25f, 0.25f) : new Vector3(0.25f, -0.25f, 0.25f);

            bool isFalling = checkFalling();

            if (isFalling && !wasFalling) {
                currentCapsule.GetComponent<Animator>().SetBool("isFalling", true);
            } else if (wasFalling && !isFalling) {
                AudioController.playPlayerDrop();
                levelController.CurrentLevel.ShowFlipZones();
                currentCapsule.GetComponent<Animator>().SetBool("isFalling", false);
            }
            wasFalling = isFalling;
        }
    }

    public bool checkFalling() {
        bool flipped = Gravity < 0;
        Vector3 origin = Mesh.transform.position + new Vector3(0, flipped ? 0.5f : 0f, 0);

        if (Physics.Raycast(origin, flipped ? Vector3.down : Vector3.up, out RaycastHit hit, rayDistance, groundLayer)) {
            Debug.DrawLine(origin, (flipped ? Vector3.down : Vector3.up) * rayDistance + origin, Color.magenta);
            if (hit.distance > 0.8f) {
                return true;
            }
        }
        return false;
    }

    public void AddInput(Vector2 actionInput) {
        if (acceptInput == false) {
            moveDirection = Vector2.zero;
            return;
        }
        moveDirection = actionInput;
    }

    public void AddCapsule(GameObject Capsule) {
        currentCapsule = Capsule;
    }

    public void RemoveCapsule() {
        currentCapsule = null;
    }


    public void CompleteLevel() {
        if (!active) return;
        active = false;
        OnLevelCompleted?.Invoke(); // Event auslösen
    }

    public void FailLevel() {
        if (!active) return;
        currentCapsule.GetComponent<Animator>().SetBool("lost", true);
        active = false;
        OnLevelFailed?.Invoke(); // Event auslösen
    }
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Finish")) {
            CompleteLevel(); // Level abgeschlossen
        }
    }

    public void ResetPositionTo(Vector3 point) {
        transform.position = point;
        transform.rotation = Quaternion.identity * Quaternion.Euler(0, 90f, 0);
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearVelocity = Vector3.zero;
        Physics.gravity = new Vector3(0, -Mathf.Abs(baseGravity), 0);
        if (currentCapsule != null) {
            currentCapsule.GetComponent<Animator>().SetBool("lost", false);
            currentCapsule.transform.rotation = Quaternion.identity * Quaternion.Euler(0, 90f, 0); ;
        }
        acceptInput = true;
        active = true;
        levelController.CurrentLevel.ShowFlipZones();
    }

}