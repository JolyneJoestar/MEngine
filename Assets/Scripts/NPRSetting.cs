using UnityEngine;

[System.Serializable]
public class NPRSetting
{
    public bool EnableNPR = false;
    public Vector4 ColorStreet = new Vector4(0.1f, 0.3f, 0.6f, 1.0f);
    public Color OutlineColor = Color.black;
    [Range(0.0f, 3.0f)]
    public float OutlineWidth = 1.0f;
    [Range(0.8f, 1.0f)]
    public float SpecularSegment = 0.9f;
}
