using System.Linq;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;
using UnityEngine.XR.ARSubsystems;

public static class MAMN60Setup
{
    private const string ScenePath = "Assets/Scenes/ARWhackAMole.unity";
    private const string PreviewObjectName = "Whack-a-Mole Preview";

    [MenuItem("MAMN60/Setup AR Whack-a-Mole Scene")]
    public static void SetupScene()
    {
        EnsureFolder("Assets/Scenes");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        if (!EditorApplication.ExecuteMenuItem("GameObject/XR/AR Session"))
        {
            Debug.LogError("Could not create AR Session. Wait for AR Foundation packages to finish importing, then run setup again.");
            return;
        }

        Selection.activeGameObject = null;

        if (!EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (Mobile AR)"))
        {
            Debug.LogError("Could not create XR Origin (Mobile AR). Wait for AR Foundation packages to finish importing, then run setup again.");
            return;
        }

        XROrigin xrOrigin = Object.FindObjectOfType<XROrigin>();
        if (xrOrigin == null)
        {
            Debug.LogError("XR Origin was not found after creation.");
            return;
        }

        ARRaycastManager raycastManager = xrOrigin.GetComponent<ARRaycastManager>();
        if (raycastManager == null)
            raycastManager = xrOrigin.gameObject.AddComponent<ARRaycastManager>();

        ARPlaneManager planeManager = xrOrigin.GetComponent<ARPlaneManager>();
        if (planeManager == null)
            planeManager = xrOrigin.gameObject.AddComponent<ARPlaneManager>();

        planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;

        Camera camera = xrOrigin.Camera;
        if (camera == null)
            camera = xrOrigin.GetComponentInChildren<Camera>(true);

        if (camera == null)
        {
            Debug.LogError("AR camera was not found under XR Origin.");
            return;
        }

        camera.tag = "MainCamera";

        ARCameraManager cameraManager = camera.GetComponent<ARCameraManager>();
        if (cameraManager == null)
            cameraManager = camera.gameObject.AddComponent<ARCameraManager>();

        ARCameraBackground cameraBackground = camera.GetComponent<ARCameraBackground>();
        if (cameraBackground == null)
            cameraBackground = camera.gameObject.AddComponent<ARCameraBackground>();

        ARPlacementController placement = xrOrigin.GetComponent<ARPlacementController>();
        if (placement == null)
            placement = xrOrigin.gameObject.AddComponent<ARPlacementController>();

        placement.Configure(raycastManager, planeManager, camera);

        if (Object.FindObjectOfType<Light>() == null)
        {
            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeGameObject = camera.gameObject;
        Debug.Log("MAMN60 AR Whack-a-Mole scene created successfully at " + ScenePath + ". ARCameraManager and ARCameraBackground are present on Main Camera.");
    }

    [MenuItem("MAMN60/Preview Whack-a-Mole Board")]
    public static void PreviewBoard()
    {
        ClearPreview();

        GameObject previewObject = new GameObject(PreviewObjectName);
        Undo.RegisterCreatedObjectUndo(previewObject, "Create Whack-a-Mole Preview");
        previewObject.transform.position = Vector3.zero;
        previewObject.transform.rotation = Quaternion.identity;

        WhackAMoleGame previewGame = previewObject.AddComponent<WhackAMoleGame>();
        previewGame.BuildPreview();

        Selection.activeGameObject = previewObject;
        SceneView.lastActiveSceneView?.FrameSelected();
        EditorSceneManager.MarkSceneDirty(previewObject.scene);

        Debug.Log("Whack-a-Mole preview created at the world origin. Use MAMN60 > Clear Whack-a-Mole Preview when finished.");
    }

    [MenuItem("MAMN60/Clear Whack-a-Mole Preview")]
    public static void ClearPreview()
    {
        GameObject preview = GameObject.Find(PreviewObjectName);
        if (preview != null)
            Object.DestroyImmediate(preview);
    }

    [MenuItem("MAMN60/Configure iOS + ARKit")]
    public static void ConfigureIOS()
    {
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.dongjieru.mamn60.arwhackamole");
        PlayerSettings.iOS.cameraUsageDescription = "The camera is used to detect a table and place the AR Whack-a-Mole game.";
        PlayerSettings.iOS.targetOSVersionString = "15.0";

        var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.iOS);
        if (buildTargetSettings == null || buildTargetSettings.AssignedSettings == null)
        {
            Debug.LogWarning("XR Plug-in Management settings are not ready yet. Open Project Settings > XR Plug-in Management once, then run this menu item again.");
            return;
        }

        bool assigned = XRPackageMetadataStore.AssignLoader(
            buildTargetSettings.AssignedSettings,
            typeof(ARKitLoader).FullName,
            BuildTargetGroup.iOS);

        AssetDatabase.SaveAssets();

        if (assigned)
            Debug.Log("ARKit loader enabled for iOS. Switch the active platform to iOS in Build Profiles before building.");
        else
            Debug.LogWarning("Unity did not report a new ARKit assignment. Check Project Settings > XR Plug-in Management > iOS and make sure ARKit is enabled.");
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;
        if (currentScenes.Any(scene => scene.path == scenePath))
            return;

        EditorBuildSettings.scenes = currentScenes
            .Concat(new[] { new EditorBuildSettingsScene(scenePath, true) })
            .ToArray();
    }

    private static void EnsureFolder(string folderPath)
    {
        string[] parts = folderPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
