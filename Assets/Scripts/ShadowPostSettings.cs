using UnityEngine;

public enum Pass
{
    Copy, Fourier
}

[CreateAssetMenu(menuName = "Rendering/Custom Shadow Post Settings")]
public class ShadowPostSettings : ScriptableObject
{
	[SerializeField]
	Shader shader = default;

	[System.NonSerialized]
	Material material;
	public Material Material
	{
		get
		{
			if (material == null && shader != null)
			{
				material = new Material(shader);
				material.hideFlags = HideFlags.HideAndDontSave;
			}
			return material;
		}
	}
}

