using UnityEngine;
using UnityEditor;

public class PreProccess
{

    [MenuItem("PreProcessMenu/GenIrradiance")]
    public static void GenIrradiance()
    {
        PreProccessRender preproccesRendering = new PreProccessRender();
        preproccesRendering.GenIrradiance();
        Debug.Log("Test");
    }
    [MenuItem("PreProcessMenu/GenBRDFLUT")]
    public static void GenBRDFLUT()
    {
        PreProccessRender preProccessRendering = new PreProccessRender();
        preProccessRendering.GenLut();
        Debug.Log("GenBRDFLUT Successed!!");
    }
}
