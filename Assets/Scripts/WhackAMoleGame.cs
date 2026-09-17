using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhackAMoleGame : MonoBehaviour
{
    private readonly List<MoleTarget> moles = new List<MoleTarget>();

    private static readonly Color BoardColor = new Color(0.20f, 0.55f, 0.28f);
    private static readonly Color HoleColor = new Color(0.06f, 0.06f, 0.06f);
    private static readonly Color MoleColor = new Color(0.45f, 0.24f, 0.10f);
    private static readonly Color EyeColor = Color.white;
    private static readonly Color PupilColor = new Color(0.03f, 0.03f, 0.03f);

    public int Score { get; private set; }

    public void Initialize()
    {
        Score = 0;
        BuildBoard();
        StartCoroutine(GameLoop());
    }

    public void BuildPreview()
    {
        Score = 0;
        BuildBoard();

        for (int i = 0; i < moles.Count; i++)
        {
            if (i % 2 == 0)
                moles[i].ShowForPreview();
        }
    }

    public void TryHit(Vector2 screenPosition, Camera camera)
    {
        if (camera == null)
            return;

        Ray ray = camera.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 10f))
            return;

        MoleTarget mole = hit.collider.GetComponentInParent<MoleTarget>();
        if (mole != null)
            mole.Hit();
    }

    public void RegisterHit(MoleTarget mole)
    {
        Score++;
    }

    private void BuildBoard()
    {
        moles.Clear();

        Material boardMaterial = MAMN60Materials.Create(BoardColor);
        Material holeMaterial = MAMN60Materials.Create(HoleColor);
        Material moleMaterial = MAMN60Materials.Create(MoleColor);
        Material eyeMaterial = MAMN60Materials.Create(EyeColor);
        Material pupilMaterial = MAMN60Materials.Create(PupilColor);

        CreatePrimitive(
            PrimitiveType.Cube,
            "Game Board",
            transform,
            new Vector3(0f, 0.01f, 0f),
            new Vector3(0.56f, 0.02f, 0.38f),
            boardMaterial);

        Vector3[] holePositions =
        {
            new Vector3(-0.18f, 0.025f,  0.10f),
            new Vector3( 0.00f, 0.025f,  0.10f),
            new Vector3( 0.18f, 0.025f,  0.10f),
            new Vector3(-0.09f, 0.025f, -0.10f),
            new Vector3( 0.09f, 0.025f, -0.10f)
        };

        for (int i = 0; i < holePositions.Length; i++)
            CreateHoleAndMole(i + 1, holePositions[i], holeMaterial, moleMaterial, eyeMaterial, pupilMaterial);
    }

    private void CreateHoleAndMole(
        int index,
        Vector3 holePosition,
        Material holeMaterial,
        Material moleMaterial,
        Material eyeMaterial,
        Material pupilMaterial)
    {
        GameObject hole = CreatePrimitive(
            PrimitiveType.Cylinder,
            $"Hole {index}",
            transform,
            holePosition,
            new Vector3(0.065f, 0.004f, 0.065f),
            holeMaterial);

        Collider holeCollider = hole.GetComponent<Collider>();
        if (holeCollider != null)
            DestroyImmediateSafe(holeCollider);

        // Keep the mole root at scale 1 so facial features are not shrunk by the
        // capsule's visual scale. The body mesh is a scaled child of this root.
        GameObject moleRoot = new GameObject($"Mole {index}");
        moleRoot.transform.SetParent(transform, false);
        moleRoot.transform.localRotation = Quaternion.identity;
        moleRoot.transform.localScale = Vector3.one;

        GameObject body = CreatePrimitive(
            PrimitiveType.Capsule,
            "Body",
            moleRoot.transform,
            Vector3.zero,
            new Vector3(0.07f, 0.06f, 0.07f),
            moleMaterial);

        Vector3 hidden = holePosition + new Vector3(0f, -0.095f, 0f);
        Vector3 visible = holePosition + new Vector3(0f, 0.055f, 0f);

        MoleTarget target = moleRoot.AddComponent<MoleTarget>();
        target.Initialize(this, hidden, visible);
        moles.Add(target);

        AddFace(moleRoot.transform, eyeMaterial, pupilMaterial);
    }

    private void AddFace(Transform moleRoot, Material eyeMaterial, Material pupilMaterial)
    {
        CreateEye("Left Eye", moleRoot, new Vector3(-0.017f, 0.022f, 0.034f), eyeMaterial, pupilMaterial);
        CreateEye("Right Eye", moleRoot, new Vector3(0.017f, 0.022f, 0.034f), eyeMaterial, pupilMaterial);
    }

    private void CreateEye(
        string name,
        Transform parent,
        Vector3 localPosition,
        Material eyeMaterial,
        Material pupilMaterial)
    {
        GameObject eye = CreatePrimitive(
            PrimitiveType.Sphere,
            name,
            parent,
            localPosition,
            new Vector3(0.017f, 0.017f, 0.010f),
            eyeMaterial);

        Collider eyeCollider = eye.GetComponent<Collider>();
        if (eyeCollider != null)
            DestroyImmediateSafe(eyeCollider);

        GameObject pupil = CreatePrimitive(
            PrimitiveType.Sphere,
            "Pupil",
            parent,
            localPosition + new Vector3(0f, 0f, 0.007f),
            new Vector3(0.007f, 0.007f, 0.005f),
            pupilMaterial);

        Collider pupilCollider = pupil.GetComponent<Collider>();
        if (pupilCollider != null)
            DestroyImmediateSafe(pupilCollider);
    }

    private IEnumerator GameLoop()
    {
        yield return new WaitForSeconds(0.7f);

        while (true)
        {
            List<MoleTarget> available = moles.FindAll(mole => !mole.IsActive);
            if (available.Count > 0)
            {
                MoleTarget mole = available[Random.Range(0, available.Count)];
                mole.Pop(Random.Range(0.65f, 1.0f));
            }

            yield return new WaitForSeconds(Random.Range(0.55f, 0.9f));
        }
    }

    private static GameObject CreatePrimitive(
        PrimitiveType primitiveType,
        string objectName,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Material material)
    {
        GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
        gameObject.name = objectName;
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.localPosition = localPosition;
        gameObject.transform.localRotation = Quaternion.identity;
        gameObject.transform.localScale = localScale;

        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null && material != null)
            renderer.sharedMaterial = material;

        return gameObject;
    }

    private static void DestroyImmediateSafe(Object obj)
    {
        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }
}
