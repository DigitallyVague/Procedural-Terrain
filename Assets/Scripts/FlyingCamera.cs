using UnityEngine;

/// <summary>
/// Simple flying camera the behaves much like the editor camera.
/// </summary>
public class FlyingCamera : MonoBehaviour{
    [SerializeField]
    private float speed = 60f;
    private Vector3 moveVector = Vector3.zero;

    [SerializeField]
    private float lookSpeed = 8f;
    [SerializeField]
    private bool invertLook = false;
    private Vector3 lookRotation = Vector3.zero;


    void Start(){
        lookRotation = transform.eulerAngles;
    }


    void Update(){
        if (Input.GetMouseButton(1)) {
            lookRotation.y += Input.GetAxis("Mouse X") * lookSpeed;
            lookRotation.x += (invertLook ? 1 : -1) * Input.GetAxis("Mouse Y") * lookSpeed;
        }

        if (lookRotation.y > 360) {
            lookRotation.y = 0;
        }
        if (lookRotation.y < 0) {
            lookRotation.y = 360;
        }
        if (lookRotation.x > 360) {
            lookRotation.x = 0;
        }
        if (lookRotation.x < 0) {
            lookRotation.x = 360;
        }

        transform.rotation = Quaternion.Euler(lookRotation);

        moveVector.z = Input.GetAxis("Vertical");
        moveVector.x = Input.GetAxis("Horizontal");
        transform.Translate(moveVector * (speed * (Input.GetKey(KeyCode.LeftShift) ? 2 : 1)) * Time.deltaTime, Space.Self);
    }
}