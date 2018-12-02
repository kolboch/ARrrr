using System.Collections;
using System.Linq;
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
    private Animator Animator;
    private bool needsRain = true; // pot is before first growth phase
    private float lastAnimationOffset = 3f;

    void Start()
    {
        Animator = GetComponent<Animator>();
        lastGrowthClipLength = Animator.runtimeAnimatorController.animationClips
            .First(anim => anim.name == PotCarrotAnim.GROW_SECOND_CLIP).length;
    }

    public bool DoesPotNeedsRain()
    {
        return needsRain;
    }

    public void PrepareFirstGrowth()
    {
        StartCoroutine(PrepareFirstGrowthCoroutine());
    }

    private IEnumerator PrepareFirstGrowthCoroutine()
    {
        needsRain = false;
        yield return new WaitForSeconds(preFirstPhaseDelaySeconds);
        var cloud = Instantiate(CloudPrefab, transform.position + new Vector3(0, cloudHeightOffset, 0),
            transform.rotation);
        yield return new WaitForSeconds(firstPhaseDelaySeconds);
        cloud.GetComponent<CloudController>().DestroyCloud();
        Animator.SetTrigger(PotCarrotAnim.GROW_FIRST_TRIGGER);
        yield return new WaitForSeconds(secondPhaseDelaySeconds);
        Animator.SetTrigger(PotCarrotAnim.GROW_SECOND_TRIGGER);
        yield return new WaitForSeconds(lastGrowthClipLength + lastAnimationOffset);
        Destroy(gameObject);
        Instantiate(CarrotPrefab, transform.position, transform.rotation);
    }
}