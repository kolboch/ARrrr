using UnityEngine;

public class CarrotController : MonoBehaviour
{
    private AnchorWrapper AnchorWrapper;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(Tags.TAG_CHARACTER))
        {
            other.gameObject.GetComponent<AguController>().OnCarrotCollected();
            ReleaseAnchor();
            Destroy(gameObject);
        }
    }

    public void SetAnchor(AnchorWrapper anchorWrapper)
    {
        AnchorWrapper = anchorWrapper;
    }

    private void ReleaseAnchor()
    {
        if (AnchorWrapper != null)
        {
            AnchorsManager.ReleaseUserFromAnchorWrapper(AnchorWrapper);
        }
    }
}