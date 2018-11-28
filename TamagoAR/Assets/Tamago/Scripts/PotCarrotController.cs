using System.Collections;
using UnityEngine;

public class PotCarrotController : MonoBehaviour
{
    public GameObject CloudPrefab;
    public GameObject CarrotPrefab;
    public float firstPhaseDelaySeconds = 10f;
    public float secondPhaseDelaySeconds = 10f;
    private Animator Animator;

    void Start()
    {
        Animator = GetComponent<Animator>();
    }

    public void PrepareFirstGrowth()
    {
        StartCoroutine(PrepareFirstGrowthCoroutine());
    }

    public IEnumerator PrepareFirstGrowthCoroutine()
    {
        var cloud = Instantiate(CloudPrefab, transform.position, transform.rotation);
        yield return new WaitForSeconds(firstPhaseDelaySeconds);
        cloud.GetComponent<CloudController>().DestroyCloud();
        Animator.SetBool(PotCarrotAnim.GROW_FIRST_TRIGGER, true);
        yield return new WaitForSeconds(secondPhaseDelaySeconds);
        Animator.SetBool(PotCarrotAnim.GROW_SECOND_TRIGGER, true);
        Destroy(gameObject);
        Instantiate(CarrotPrefab, transform.position, transform.rotation);
    }
}