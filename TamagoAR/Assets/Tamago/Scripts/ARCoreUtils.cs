
using GoogleARCore;
using UnityEngine;

public class ARCoreUtils {
    
    
}

public static class ARCoreExtensions {
    public static bool IsVerticalPlane(this DetectedPlane plane) {
        return plane.PlaneType == DetectedPlaneType.Vertical;
    }

    public static Pose GetPlaneCenter(this DetectedPlane plane) {
        return plane.m_NativeSession.PlaneApi.GetCenterPose(plane.m_TrackableNativeHandle);
    }
}
