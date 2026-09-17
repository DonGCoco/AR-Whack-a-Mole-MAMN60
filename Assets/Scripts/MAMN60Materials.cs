using UnityEngine;

public static class MAMN60Materials
{
    private static Shader cachedShader;

    public static Material Create(Color color)
    {
        if (cachedShader == null)
            cachedShader = Resources.Load<Shader>("MAMN60UnlitColor");

        if (cachedShader == null)
            cachedShader = Shader.Find("Unlit/Color");

        if (cachedShader == null)
            cachedShader = Shader.Find("Sprites/Default");

        if (cachedShader == null)
        {
            Debug.LogError("MAMN60 material shader could not be loaded.");
            return null;
        }

        Material material = new Material(cachedShader);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);

        return material;
    }
}
