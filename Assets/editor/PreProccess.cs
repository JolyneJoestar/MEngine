using UnityEngine;
using UnityEditor;

public class PreProccess
{

    [MenuItem("Menu/test")]
    public static void test()
    {
        PreProccessRender preproccesRendering = new PreProccessRender();
        preproccesRendering.GendIrradiance();
        Debug.Log("Test");
    }
    [MenuItem("Menu/GenBRDFLUT")]
    public static void GenBRDFLUT()
    {
        PreProccessRender preProccessRendering = new PreProccessRender();
        preProccessRendering.GenLut();
        Debug.Log("GenBRDFLUT Successed!!");
    }
}
