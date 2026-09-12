using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhackAMoleGame : MonoBehaviour
{
    private readonly List<MoleTarget> moles = new List<MoleTarget>();

    private Material boardMaterial;
    private Material holeMaterial;
    private Material moleMaterial;
    private Material eyeMaterial;

    public int Score { get; private set; }

    public void Initialize()
    {
        Score = 0;
        BuildBoard();
        StartCoroutine(GameLoop());
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
        boardMaterial = CreateMaterial(new Color(0.20f, 0.55f, 0.28f));
        holeMaterial = CreateMaterial(new Color(0.06f, 0.06f, 0.06f));
        moleMaterial = CreateMaterial(new Color(0.45f, 0.24f, 0.10f));
        eyeMaterial = CreateMaterial(Color.white);

        GameObject board = CreatePrimitive(
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
            CreateHoleAndMole(i + 1, holePositions[i]);
    }

    private void CreateHoleAndMole(int index, Vector3 holePosition)
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
            Destroy(holeCollider);

        GameObject mole = CreatePrimitive(
            PrimitiveType.Capsule,
            $"Mole {index}",
            transform,
            Vector3.zero,
            new Vector3(0.07f, 0.06f, 0.07f),
            moleMaterial);

        Vector3 hidden = holePosition + new Vector3(0f, -0.095f, 0f);
        Vector3 visible = holePosition + new Vector3(0f, 0.055f, 0f);

        MoleTarget target = mole.AddComponent<MoleTarget>();
        target.Initialize(this, hidden, visible);
        moles.Add(target);

        AddEyes(mole.transform);
    }

    private void AddEyes(Transform mole)
    {
        Vector3 leftEyePosition = new Vector3(-0.012f, 0.035f, 0.031f);
        Vector3 rightEyePosition = new Vector3(0.012f, 0.035f, 0.031f);

        CreateEye("Left Eye", mole, leftEyePosition);
        CreateEye("Right Eye", mole, rightEyePosition);
    }

    private void CreateEye(string name, Transform parent, Vector3 localPosition)
    {
        GameObject eye = CreatePrimitive(
            PrimitiveType.Sphere,
            name,
            parent,
            localPosition,
            new Vector3(0.012f, 0.012f, 0.012f),
            eyeMaterial);

        Collider eyeCollider = eye.GetComponent<Collider>();
        if (eyeCollider != null)
            Destroy(eyeCollider);
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
        if (renderer != null)
            renderer.material = material;

        return gameObject;
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        return material;
    }
}
