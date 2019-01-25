using System.Collections;
using System.Linq;
using GoogleARCore;
using UnityEngine;

public class PotCarrotController : MonoBehaviour
{
    public GameObject CloudPrefab;
    public GameObject CarrotPrefab;
    public float preFirstPhaseDelaySeconds = 4f;
    public float firstPhaseDelaySeconds = 10f;
    public float secondPhaseDelaySeconds = 10f;
    public float cloudHeightOffset = 0.5f;
    public float lastGrowthClipLength = 10f;
    public float updateYInterval = 5f;
    private Animator Animator;
    private bool needsRain = true; // pot is before first growth phase
    private float lastAnimationOffset = 3f;
    private DetectedPlane Plane;

    void Start()
    {
        Animator = GetComponent<Animator>();
        lastGrowthClipLength = Animator.runtimeAnimatorController.animationClips
            .First(anim => anim.name == PotCarrotAnim.GROW_SECOND_CLIP).length;
        StartCoroutine(UpdateYPositionCoroutine());
    }

    public bool DoesPotNeedsRain()
    {
        return needsRain;
    }

    public void PrepareFirstGrowth()
    {
        StartCoroutine(PrepareFirstGrowthCoroutine());
    }

    public void SetPlane(DetectedPlane plane)
    {
        Plane = plane;
    }

    private IEnumerator PrepareFirstGrowthCoroutine()
    {
        needsRain = false;
        yield return new WaitForSeconds(preFirstPhaseDelaySeconds);
        var cloud = Instantiate(CloudPrefab, transform.position + new Vector3(0, cloudHeightOffset, 0),
            transform.rotation);
        MusicManager.Instance.PlayRainSequenceMusic();
        yield return new WaitForSeconds(firstPhaseDelaySeconds);
        cloud.GetComponent<CloudController>().DestroyCloud();
        MusicManager.Instance.PlayGameMusic();
        Animator.SetTrigger(PotCarrotAnim.GROW_FIRST_TRIGGER);
        yield return new WaitForSeconds(secondPhaseDelaySeconds);
        Animator.SetTrigger(PotCarrotAnim.GROW_SECOND_TRIGGER);
        yield return new WaitForSeconds(lastGrowthClipLength + lastAnimationOffset);
        var carrot = Instantiate(CarrotPrefab, transform.position, transform.rotation);
        carrot.GetComponent<CarrotController>().SetPlane(Plane);
        ReleaseResources();
        Destroy(gameObject);
    }

    private void ReleaseResources()
    {
        StopAllCoroutines();
        Plane = null;
    }

    private IEnumerator UpdateYPositionCoroutine()
    {
        while (true)
        {
            if (Plane != null && Plane.TrackingState == TrackingState.Tracking)
            {
                var updatePosition = transform.position;
                updatePosition.y = Plane.CenterPose.position.y;
                transform.position = updatePosition;
            }

            yield return new WaitForSeconds(updateYInterval);
        }
    }
}