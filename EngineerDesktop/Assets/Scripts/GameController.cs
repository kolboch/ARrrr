using Panda;
using UnityEngine;

public class GameController : MonoBehaviour {

    public Transform Character;
    public Animator animator;
    public Camera MainCamera;
    public float WalkSpeed = 0.5f;
    public float RotateSpeed = 5f;
    private PandaBehaviour CharacterBT;
    private Vector3 DestinationPoint;
    private Vector3 DestinationYIgnored;
    private Quaternion LookRotation;
    
    // Use this for initialization
    void Start() {
        CharacterBT = GetComponent<PandaBehaviour>();
    }

    // Update is called once per frame
    void Update() {
        
    }

    [Task]
    bool HasInput() {
        if (Input.GetMouseButton(0)) {
            RaycastHit hit;
            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit);
            if (hit.collider.tag == "Environment") {
                DestinationPoint = hit.point;
                ComputeLookDirection();
                return true;
            }
        }
        return false;
    }
    
    private void ComputeLookDirection() {
        Vector3 direction = DestinationPoint - Character.position;
        LookRotation = Quaternion.LookRotation(direction);
    }

    [Task]
    void Rotate() {
        Character.rotation = Quaternion.RotateTowards(Character.rotation, LookRotation, RotateSpeed);
        if(Character.rotation == LookRotation) {
            Task.current.Succeed();
        }
    }
    
    [Task]
    void SetIsWalkingAnim(bool isWalking) {
        animator.SetBool("IsWalking", isWalking);
        Task.current.Succeed();
    }

    [Task]
    void IgnoreYPosition() {
        // ignore Y by now
        DestinationPoint.y = Character.position.y;
        Task.current.Succeed();
    }
    
    [Task]
    void Walk() {
        Vector3 delta = (DestinationPoint - Character.position);
        Character.position = Vector3.MoveTowards(Character.position, DestinationPoint, WalkSpeed * Time.deltaTime);
        Vector3 newDelta = (DestinationPoint - Character.position);
        float d = newDelta.magnitude;
        if (Vector3.Dot(delta, newDelta) <= 0.0f || d < 1e-3) {
            Character.position = DestinationPoint;
            animator.SetBool("IsWalking", false);
            Task.current.Succeed();
        }
    }
}
