using UnityEngine;

public class TutorialText : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = Camera.main.transform.position;
        transform.LookAt(2 * transform.position - targetPosition, Camera.main.transform.up);
    }
}
