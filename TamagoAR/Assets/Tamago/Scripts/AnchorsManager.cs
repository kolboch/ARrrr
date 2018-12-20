using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

public class AnchorsManager : MonoBehaviour
{
    // to reuse one anchor for plane, identified by plane hashcode, if no users will be released
    public static Dictionary<int, AnchorWrapper> AnchorsDictionary = new Dictionary<int, AnchorWrapper>();

    public static AnchorWrapper GetAnchorForTrackable(Trackable trackable, Pose pose)
    {
        var trackableHashCode = trackable.GetHashCode();
        AnchorWrapper anchorToReturn;
        if (AnchorsDictionary.TryGetValue(trackableHashCode, out anchorToReturn))
        {
            AnchorsDictionary[trackableHashCode].numberOfUsers++;
            anchorToReturn = AnchorsDictionary[trackableHashCode];
        }
        else
        {
            AnchorsDictionary.Add(trackableHashCode,
                anchorToReturn = new AnchorWrapper(trackable, pose, trackableHashCode));
            anchorToReturn.numberOfUsers++;
        }

        return anchorToReturn;
    }

    public static void ReleaseUserFromAnchorWrapper(AnchorWrapper anchorWrapper)
    {
        AnchorWrapper current;
        if (AnchorsDictionary.TryGetValue(anchorWrapper.trackableHashCode, out current))
        {
            current.numberOfUsers--;
            if (current.numberOfUsers <= 0)
            {
                current.ReleaseAnchor();
                AnchorsDictionary.Remove(anchorWrapper.trackableHashCode);
            }
        }
    }
}