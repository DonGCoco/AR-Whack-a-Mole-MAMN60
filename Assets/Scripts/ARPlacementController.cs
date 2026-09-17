using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementController : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private Camera arCamera;

    private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();

    private GameObject reticle;
    private WhackAMoleGame game;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;

    public void Configure(ARRaycastManager raycast, ARPlaneManager planes, Camera camera)
    {
        raycastManager = raycast;
        planeManager = planes;
        arCamera = camera;
    }

    private void Awake()
    {
        if (raycastManager == null)
            raycastManager = GetComponent<ARRaycastManager>();

        if (planeManager == null)
            planeManager = GetComponent<ARPlaneManager>();

        if (arCamera == null)
            arCamera = Camera.main;

        CreateReticle();
    }

    private void Update()
    {
        if (arCamera == null)
            arCamera = Camera.main;

        if (game == null)
        {
            UpdateReticle();

            if (TryGetPressPosition(out Vector2 pressPosition))
                TryPlaceGame(pressPosition);
        }
        else if (TryGetPressPosition(out Vector2 pressPosition))
        {
            game.TryHit(pressPosition, arCamera);
        }
    }

    private bool TryGetPressPosition(out Vector2 position)
    {
        position = default;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            position = Input.mousePosition;
            return true;
        }
#endif

        if (Input.touchCount == 0)
            return false;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return false;

        position = touch.position;
        return true;
    }

    private void UpdateReticle()
    {
        if (raycastManager == null || reticle == null)
            return;

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        bool foundPlane = raycastManager.Raycast(screenCenter, Hits, TrackableType.PlaneWithinPolygon);

        reticle.SetActive(foundPlane);
        if (!foundPlane)
            return;

        Pose pose = Hits[0].pose;
        reticle.transform.SetPositionAndRotation(pose.position + pose.up * 0.003f, pose.rotation);
    }

    private void TryPlaceGame(Vector2 screenPosition)
    {
        if (raycastManager == null || arCamera == null)
            return;

        if (!raycastManager.Raycast(screenPosition, Hits, TrackableType.PlaneWithinPolygon))
            return;

        Pose hitPose = Hits[0].pose;

        Vector3 forward = Vector3.ProjectOnPlane(arCamera.transform.forward, hitPose.up);
        Quaternion rotation = hitPose.rotation;
        if (forward.sqrMagnitude > 0.001f)
        {
            // The mole faces are built on local +Z. Camera.forward points from the
            // camera toward the board, so use the opposite direction to make +Z
            // point back toward the user/camera.
            rotation = Quaternion.LookRotation(-forward.normalized, hitPose.up);
        }

        GameObject gameObject = new GameObject("AR Whack-a-Mole Game");
        gameObject.transform.SetPositionAndRotation(hitPose.position, rotation);

        game = gameObject.AddComponent<WhackAMoleGame>();
        game.Initialize();

        if (reticle != null)
            reticle.SetActive(false);

        if (planeManager != null)
        {
            foreach (ARPlane plane in planeManager.trackables)
                plane.gameObject.SetActive(false);

            planeManager.enabled = false;
        }
    }

    private void CreateReticle()
    {
        reticle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        reticle.name = "Placement Reticle";
        reticle.transform.localScale = new Vector3(0.075f, 0.0015f, 0.075f);

        Collider collider = reticle.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        Renderer renderer = reticle.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = MAMN60Materials.Create(new Color(0.20f, 1.00f, 0.35f, 1.00f));
            if (material != null)
                renderer.sharedMaterial = material;
        }

        reticle.SetActive(false);
    }

    public void ResetGame()
    {
        if (game != null)
            Destroy(game.gameObject);

        game = null;

        if (planeManager != null)
            planeManager.enabled = true;
    }

    private void EnsureStyles()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Max(22, Screen.width / 22),
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }

        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Max(18, Screen.width / 28)
            };
        }
    }

    private void OnGUI()
    {
        EnsureStyles();

        float margin = Screen.width * 0.04f;
        float top = Screen.safeArea.y + Screen.height * 0.03f;
        float topHeight = Screen.height * 0.10f;

        string message = game == null
            ? "Scan a table, then tap the marker to place the game"
            : $"Score: {game.Score}";

        GUI.Label(new Rect(margin, top, Screen.width - margin * 2f, topHeight), message, labelStyle);

        if (game != null)
        {
            float buttonWidth = Mathf.Min(260f, Screen.width * 0.32f);
            float buttonHeight = Mathf.Min(90f, Screen.height * 0.075f);
            Rect buttonRect = new Rect(Screen.width - margin - buttonWidth, Screen.height - margin - buttonHeight, buttonWidth, buttonHeight);

            if (GUI.Button(buttonRect, "Reset", buttonStyle))
                ResetGame();
        }
    }
}
