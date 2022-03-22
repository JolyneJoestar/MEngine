using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PreProccessRender
{


    Shader shader = Shader.Find("MyPipeline/PreProccess");

    RenderTexture rt0 = null;
    RenderTexture rt1 = null;

    RenderTexture m_brdfLutTex = null;
    Cubemap cubemap = null;
    string m_irradianceTexturePath = "Texture/_irradiance.png";
    string m_lutTexturePath = "Texture/brdfLUT.png";

    private Material m_material = null;

    public void GendIrradiance()
    {
        if(shader != null)
        {
            Debug.Log("render start!");
            if(m_material == null)
            {
                m_material = new Material(shader);
                m_material.hideFlags = HideFlags.HideAndDontSave;
        
            }
            if(cubemap == null)
            {
                cubemap = Resources.Load<Cubemap>("IBLSource");
                Debug.Log("cube map loading!");
            }
            if(cubemap == null)
                Debug.Log("cube map load failed!");
            rt0 = new RenderTexture(cubemap.width * 2, cubemap.width, 0, RenderTextureFormat.ARGBFloat);
            rt1 = new RenderTexture(cubemap.width * 2, cubemap.width, 0, RenderTextureFormat.ARGBFloat);
            rt0.wrapMode = TextureWrapMode.Repeat;
            rt1.wrapMode = TextureWrapMode.Repeat;
            rt0.Create();
            rt1.Create();
            int Count = 1024;
            Graphics.Blit(cubemap, rt0, m_material, 0);
            m_material.SetTexture("_CubeTex", cubemap);
            for (int i = 0; i < Count; i++)
            {
                EditorUtility.DisplayProgressBar("", "", 1f / Count);
                Vector3 n = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(0.0000001f, 1f),
                        Random.Range(-1f, 1f)
                    );
                while (n.magnitude > 1)
                    n = new Vector3(
                            Random.Range(-1f, 1f),
                            Random.Range(0.0000001f, 1f),
                            Random.Range(-1f, 1f)
                        );
                n = n.normalized;
                m_material.SetVector("_RandomVector", new Vector4(
                    n.x, n.y, n.z,
                    1f / (i + 2)
                    ));
                Graphics.Blit(rt0, rt1, m_material, 1);
                // ·­×ª
                var t = rt0;
                rt0 = rt1;
                rt1 = t;
            }
            Graphics.Blit(cubemap, rt1, m_material, 0);
            EditorUtility.ClearProgressBar();
            // ±£´æ
            Texture2D texture = new Texture2D(cubemap.width * 2, cubemap.width, TextureFormat.ARGB32, true);
            var k = RenderTexture.active;
            RenderTexture.active = rt0;
            texture.ReadPixels(new Rect(0, 0, rt0.width, rt0.height), 0, 0);
            RenderTexture.active = k;
            byte[] bytes = texture.EncodeToPNG();
            System.IO.FileStream fs = new System.IO.FileStream(System.IO.Path.Combine(Application.dataPath) + "/" + m_irradianceTexturePath, System.IO.FileMode.Create);
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs);
            bw.Write(bytes);
            fs.Close();
            bw.Close();

            Debug.Log("render successed!");


        }
        else
        {
            Debug.Log("render failed!!!!!!");
        }
    }

    public void GenLut()
    {
        if(shader != null)
        {
            Debug.Log("start Gen brdf Lut!!");
            if(m_material == null)
            {
                m_material = new Material(shader);
                m_material.hideFlags = HideFlags.HideAndDontSave;
            }
            if (m_brdfLutTex == null)
                m_brdfLutTex = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
            m_brdfLutTex.Create();
            Graphics.Blit(null, m_brdfLutTex, m_material, 2);
            EditorUtility.ClearProgressBar();
            Texture2D texture = new Texture2D(1024,1024,TextureFormat.ARGB32, true);
            var k = RenderTexture.active;
            RenderTexture.active = m_brdfLutTex;
            texture.ReadPixels(new Rect(0, 0, m_brdfLutTex.width, m_brdfLutTex.height), 0, 0);
            RenderTexture.active = k;
            byte[] bytes = texture.EncodeToPNG();
            System.IO.FileStream fs = new System.IO.FileStream(System.IO.Path.Combine(Application.dataPath) + "/" + m_lutTexturePath, System.IO.FileMode.Create);
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs);
            bw.Write(bytes);
            fs.Close();
            bw.Close();

            Debug.Log("Gen brdf Lut Successed");
        }
        else
        {
            Debug.Log("Gen brdf lut starting failed!!");
        }
    }
}
