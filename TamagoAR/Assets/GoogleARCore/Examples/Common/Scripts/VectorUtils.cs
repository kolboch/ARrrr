using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VectorUtils {

    public static Vector3 FindCentroid(List<Vector3> pointsList) {
        Vector3 sumOfVectors = pointsList.Aggregate(Vector3.zero, (sum, next) => sum + next);
        Vector3 centroid = sumOfVectors / pointsList.Count;
        return centroid;
    }

}
