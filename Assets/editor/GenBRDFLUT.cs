using UnityEngine;
using UnityEditor;

public class GenBRDFLUT
{
    
    [MenuItem("Menu/test")]
    public static void test()
    {
        PreProccessRender preproccesRendering = new PreProccessRender();
        preproccesRendering.draw();
        Debug.Log("Test");
    }

}
