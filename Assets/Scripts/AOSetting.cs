using UnityEngine;
[System.Serializable]
public class AOSetting
{
    [Range(0.0f, 10.0f)]
    public float Radius = 0.5f;
    [Range(32, 256)]
    public float MaxRadiusPixel = 128;
    [Range(0.0f, 1.0f)]
    public float AngleBias = 30.0f;
    [Range(0.0f, 1.0f)]
    public float AOStrength = 1.0f;
    //[Range (1, 8)]
    //public int DirectionNum = 6;
    //[Range (1, 8)]
    //public int StepNum = 6;
}
