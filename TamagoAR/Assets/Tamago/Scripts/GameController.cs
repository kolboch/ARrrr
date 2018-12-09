using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoogleARCore;
using UnityEngine;
using Random = System.Random;

public class GameController : MonoBehaviour
{
    public Camera FirstPersonCamera;
    public GameObject CharacterPrefab;
    public GameObject StarPrefab;
    public MenusController MenusController;
    public GameObject PotMockPrefab;
    public GameObject PotPrefab;

    public float starSpawnIntervalSeconds = 10f;
    private bool isQuitting = false;
    private bool characterNeedsInit = true;
    private List<DetectedPlane> DetectedPlanes = new List<DetectedPlane>();
    private IEnumerator StarSpawnCoroutine;
    private GameState GameState;
    private bool showSearchingUI = false;
    private bool isPlacingCarrot = false;
    private bool placingFinished = false;
    private int carrotPrice = 1;
    private List<PotCarrotController> PotCarrots = new List<PotCarrotController>();

    void Start()
    {
        GameState = DataStorageUtils.GetSavedGameState();
        StartCoroutine(StarSpawningCoroutine());
        UpdateGameUI();
    }

    void Update()
    {
        UpdateApplicationLifecycle();
        UpdatePlaneUI();
    }

    public void RepositionCharacterAfterPlaneLost(GameObject character)
    {
        AndroidUtils.ShowAndroidToastMessage("Repositioning character after plane lost call!");
        DetectedPlane planeToPlaceOn = DetectedPlanes.Find(list => list.TrackingState == TrackingState.Tracking);
        if (planeToPlaceOn != null)
        {
            character.transform.position = planeToPlaceOn.CenterPose.position;
            character.GetComponent<AguController>().SetCurrentPlane(planeToPlaceOn);
        }
        else
        {
            Destroy(character);
            characterNeedsInit = true;
        }
    }

    public void CollectStar()
    {
        GameState.starsBalance++;
        DataStorageUtils.SaveStarCounter(GameState.starsBalance);
        UpdateGameStars();
    }

    public bool HasCarrotsThatNeedRain()
    {
        return PotCarrots.Any(controller => controller.DoesPotNeedsRain());
    }

    public void CollectCarrot()
    {
        GameState.carrotBalance++;
        DataStorageUtils.SaveCarrotCounter(GameState.carrotBalance);
        UpdateGameCarrots();
    }

    public void OnRainDancePerformed()
    {
        PotCarrots.FindAll(controller => controller.DoesPotNeedsRain())
            .ForEach(controller =>
            {
                if (controller.DoesPotNeedsRain())
                {
                    controller.PrepareFirstGrowth();
                }
            });
    }

    public void BuyCarrotPot()
    {
        MenusController.HideFullMenu();
        MenusController.ShowPlacingMenu();
        StartCoroutine(PlaceCarrotCoroutine());
    }

    public void FinishCarrotPlacing()
    {
        placingFinished = true;
    }

    public void CancelCarrotPlacing()
    {
        isPlacingCarrot = false;
    }

    public void FindSubpathTo(
        Vector3 currentPosition, Vector3 destinationPoint,
        DetectedPlane currentPlane, DetectedPlane destinationPlane,
        ref List<Vector3> subPoints
    )
    {
        if (currentPlane == destinationPlane)
        {
            return;
        }

        bool isJumpUp = destinationPoint.y - currentPosition.y > 0;
        var currentPointsConsidered = new List<Vector3>();
        var destinationPointsConsidered = new List<Vector3>();
        currentPlane.GetBoundaryPolygon(currentPointsConsidered);
        destinationPlane.GetBoundaryPolygon(destinationPointsConsidered);
        float deltaX = destinationPoint.x - currentPosition.x;
        float deltaZ = destinationPoint.z - currentPosition.z;
        currentPointsConsidered = currentPointsConsidered.FindAll(point =>
        {
            bool valid_x = deltaX > 0 ? point.x >= currentPosition.x : point.x <= currentPosition.x;
            return valid_x && deltaZ > 0 ? point.z >= currentPosition.z : point.z <= currentPosition.z; // valid_z
        });
        destinationPointsConsidered = destinationPointsConsidered.FindAll(point =>
        {
            bool valid_x = -deltaX > 0 ? point.x >= destinationPoint.x : point.x <= destinationPoint.x;
            return valid_x && -deltaZ > 0 ? point.z >= destinationPoint.z : point.z <= destinationPoint.z;
            ;
        });
        // we have only valid points at this moment ( considering direction to move)
        // now optimize if there is any overlapping of planes
        if (isJumpUp)
        {
            List<Vector3> pointsInCurrentPlaneExtents = new List<Vector3>();
            destinationPointsConsidered.ForEach(point =>
            {
                if (currentPlane.IsPoseInExtents(new Pose(point, Quaternion.identity)))
                {
                    pointsInCurrentPlaneExtents.Add(point);
                }
            });
            if (pointsInCurrentPlaneExtents.Count > 0)
            {
                destinationPointsConsidered = pointsInCurrentPlaneExtents;
                currentPointsConsidered = generateMidPoints(ref destinationPointsConsidered, currentPosition,
                    currentPosition.y, 1 / 3);
            }
        }
        else
        {
            List<Vector3> pointsInDestinationPlaneExtents = new List<Vector3>();
            currentPointsConsidered.ForEach(point =>
            {
                if (destinationPlane.IsPoseInExtents(new Pose(point, Quaternion.identity)))
                {
                    pointsInDestinationPlaneExtents.Add(point);
                }
            });
            if (pointsInDestinationPlaneExtents.Count > 0)
            {
                currentPointsConsidered = pointsInDestinationPlaneExtents;
                destinationPointsConsidered = generateMidPoints(ref currentPointsConsidered, destinationPoint,
                    destinationPoint.y, 1 / 3);
            }
        }

        // now find closest pair
        float minDistance = float.MaxValue;
        var minPoint1 = Vector3.zero;
        var minPoint2 = Vector3.zero;
        float distPoints, distOrigin, distDestination, distanceTotal;
        foreach (Vector3 p1 in currentPointsConsidered)
        {
            foreach (Vector3 p2 in destinationPointsConsidered)
            {
                distPoints = Mathf.Pow(p1.x - p2.x, 2) + Mathf.Pow(p1.z - p2.z, 2);
                distOrigin = Mathf.Pow(currentPosition.x - p1.x, 2) + Mathf.Pow(currentPosition.z - p1.z, 2);
                distDestination = Mathf.Pow(p2.x - destinationPoint.x, 2) + Mathf.Pow(p2.z - destinationPoint.z, 2);
                distanceTotal = distPoints + distOrigin + distDestination;
                if (distanceTotal < minDistance)
                {
                    minDistance = distanceTotal;
                    minPoint1 = p1;
                    minPoint2 = p2;
                }
            }
        }

        subPoints.Clear();
        subPoints.Add(minPoint1);
        subPoints.Add(minPoint2);
    }

    private void UpdatePlaneUI()
    {
        Session.GetTrackables<DetectedPlane>(DetectedPlanes);
        bool currentIsSearching = true;
        DetectedPlane firstTracked = DetectedPlanes.Find(plane => plane.TrackingState == TrackingState.Tracking);
        if (firstTracked != null)
        {
            if (characterNeedsInit)
            {
                InitCharacter(firstTracked);
            }

            currentIsSearching = false;
        }

        if (currentIsSearching != showSearchingUI)
        {
            showSearchingUI = currentIsSearching;
            MenusController.ShowSearchingPlanesUI(showSearchingUI);
            if (currentIsSearching)
            {
                CancelTaskThatNeedsPlanes();
            }
        }
    }

    private void CancelTaskThatNeedsPlanes()
    {
        isPlacingCarrot = false;
    }

    private void UpdateGameUI()
    {
        UpdateGameStars();
        UpdateGameCarrots();
        MenusController.FlashBalanceMenu();
    }

    private void UpdateGameCarrots()
    {
        MenusController.UpdateBalanceCarrotsUI(GameState.carrotBalance.ToString());
        MenusController.FlashBalanceMenu();
    }

    private void UpdateGameStars()
    {
        MenusController.UpdateBalanceStarsUI(GameState.starsBalance.ToString());
        MenusController.FlashBalanceMenu();
    }

    private void InitCharacter(DetectedPlane plane)
    {
        characterNeedsInit = false;
        Pose centerPose = plane.CenterPose;
        Vector3 center = centerPose.position;
        Debug.Log("Center from AR API: " + center);
        Vector3 direction = FirstPersonCamera.transform.position - center;
        GameObject character = Instantiate(CharacterPrefab, center, Quaternion.LookRotation(direction));
        character.GetComponent<AguController>().FirstPersonCamera = FirstPersonCamera;
        character.GetComponent<AguController>().SetCurrentPlane(plane);
        character.GetComponent<AguController>().SetGameController(this);
        characterNeedsInit = false;
    }

    private void UpdateApplicationLifecycle()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (isQuitting)
        {
            return;
        }

        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            AndroidUtils.ShowAndroidToastMessage("Camera permission is needed to run this application.");
            isQuitting = true;
            StartCoroutine(DoQuitWithDelay());
        }
        else if (Session.Status.IsError())
        {
            AndroidUtils.ShowAndroidToastMessage(
                "ARCore encountered a problem connecting.  Please start the app again.");
            isQuitting = true;
            StartCoroutine(DoQuitWithDelay());
        }
    }

    private IEnumerator DoQuitWithDelay(float delay = 0.5f)
    {
        yield return new WaitForSeconds(delay);
        Application.Quit();
    }

    /// <summary>
    /// Generate mid-points between points and singlePoint with yDefined
    /// </summary>
    private List<Vector3> generateMidPoints(ref List<Vector3> points, Vector3 singlePoint, float yDefined, float ratio)
    {
        var midPoints = new List<Vector3>();
        points.ForEach(point =>
        {
            var midPoint = point + (singlePoint - point) * ratio;
            midPoint.y = yDefined;
            midPoints.Add(midPoint);
        });
        return midPoints;
    }

    private IEnumerator StarSpawningCoroutine()
    {
        while (true)
        {
            if (StarSpawnCoroutine == null)
            {
                StarSpawnCoroutine = PlaceStarAtRandom();
                StartCoroutine(StarSpawnCoroutine);
            }

            yield return new WaitForSeconds(5);
        }
    }

    private IEnumerator PlaceStarAtRandom()
    {
        Debug.Log("PlaceStarAtRandom coroutine");
        Random random;
        DetectedPlane planeSelected;
        Pose planeCenterPose;
        Vector3 centerVector;
        int extentsDivisor = 4;
        yield return new WaitForSeconds(starSpawnIntervalSeconds);
        while (DetectedPlanes.Count > 0)
        {
            Debug.Log("Going to spawn a star.");
            random = new Random();
            int planeIndex = random.Next(0, DetectedPlanes.Count);
            planeSelected = DetectedPlanes[planeIndex];
            planeCenterPose = planeSelected.CenterPose;
            centerVector = planeCenterPose.position;
            float extentX = planeSelected.ExtentX;
            float extentZ = planeSelected.ExtentZ;
            float randomX = (float) (random.NextDouble() * extentX / extentsDivisor) *
                            (random.NextDouble() > 0.5 ? 1 : -1);
            float randomZ = (float) (random.NextDouble() * extentZ / extentsDivisor) *
                            (random.NextDouble() > 0.5 ? 1 : -1);
            Instantiate(StarPrefab, new Vector3(centerVector.x + randomX, centerVector.y, centerVector.z + randomZ),
                planeCenterPose.rotation);
            yield return new WaitForSeconds(starSpawnIntervalSeconds);
        }

        StarSpawnCoroutine = null;
    }

    private IEnumerator PlaceCarrotCoroutine()
    {
        isPlacingCarrot = true;
        Ray cameraRay;
        TrackableHit hit;
        TrackableHitFlags filter = TrackableHitFlags.PlaneWithinPolygon;
        DetectedPlane planeToPlace = null;
        Vector3 positionToPlace = Vector3.zero;
        GameObject potMock = null;
        while (isPlacingCarrot)
        {
            cameraRay = FirstPersonCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Frame.Raycast(cameraRay.origin, cameraRay.direction, out hit, float.PositiveInfinity, filter))
            {
                DetectedPlane plane;
                if (hit.Trackable is DetectedPlane)
                {
                    plane = (DetectedPlane) hit.Trackable;
                    if (!plane.IsVerticalPlane() &&
                        Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                            hit.Pose.rotation * Vector3.up) > 0)
                    {
                        planeToPlace = plane;
                        positionToPlace = hit.Pose.position;
                        if (potMock == null)
                        {
                            potMock = Instantiate(PotMockPrefab, positionToPlace, planeToPlace.CenterPose.rotation);
                        }
                        else
                        {
                            potMock.transform.position = positionToPlace;
                            potMock.transform.rotation = planeToPlace.CenterPose.rotation;
                        }

                        MenusController.EnableAcceptButton();
                    }
                }
                else
                {
                    MenusController.DisableAcceptButton();
                }
            }
            else
            {
                if (potMock != null)
                {
                    Destroy(potMock);
                }

                MenusController.DisableAcceptButton();
            }

            if (placingFinished)
            {
                OnPlacingCarrotExit();
                if (potMock != null)
                {
                    Destroy(potMock);
                }

                if (planeToPlace != null) // cant check Vector3 for null
                {
                    var potCarrot = Instantiate(PotPrefab, positionToPlace, planeToPlace.CenterPose.rotation);
                    PotCarrots.Add(potCarrot.GetComponent<PotCarrotController>());
                }

                placingFinished = false;
                isPlacingCarrot = false;
                yield break;
            }

            yield return null;
        }

        if (potMock != null)
        {
            Destroy(potMock);
        }

        OnPlacingCarrotExit();
    }

    private void OnPlacingCarrotExit()
    {
        MenusController.HidePlacingMenu();
        if (!showSearchingUI)
        {
            MenusController.ShowBottomMenu();
        }
    }
}