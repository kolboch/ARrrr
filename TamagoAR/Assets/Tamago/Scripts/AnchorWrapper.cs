using GoogleARCore;
using UnityEngine;

public class AnchorWrapper
{
    public Anchor anchor;
    public int trackableHashCode;
    public int numberOfUsers = 0;

    public AnchorWrapper(Trackable trackable, Pose pose, int trackableHashCode)
    {
        anchor = trackable.CreateAnchor(pose);
        this.trackableHashCode = trackableHashCode;
        numberOfUsers = 0;
    }

    public void ReleaseAnchor()
    {
        anchor.DetachAnchor();
    }
}