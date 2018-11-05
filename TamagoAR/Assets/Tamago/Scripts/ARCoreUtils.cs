
using GoogleARCore;
using UnityEngine;

public class ARCoreUtils {
    
    
}

public static class ARCoreExtensions {
    public static bool IsVerticalPlane(this DetectedPlane plane) {
        return plane.PlaneType == DetectedPlaneType.Vertical;
    }

    public static bool IsPoseInPolygon(this DetectedPlane plane, Pose pose) {
        return plane.m_NativeSession.PlaneApi.IsPoseInPolygon(plane.m_TrackableNativeHandle, pose);
    }

    public static bool IsPoseInExtents(this DetectedPlane plane, Pose pose) {
        return plane.m_NativeSession.PlaneApi.IsPoseInExtents(plane.m_TrackableNativeHandle, pose);
    }
}
