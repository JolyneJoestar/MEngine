
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/PreProccessSetting")]
public class PreProccessSetting : ScriptableObject
{
    Material material;

    public Material Material
    {
        get {
            if (material == null && shader != null)
            {
                material = new Material(shader);
                material.hideFlags = HideFlags.HideAndDontSave;
            }
            return material; 
        }
    }
    [SerializeField]
    Shader shader = default;

}
