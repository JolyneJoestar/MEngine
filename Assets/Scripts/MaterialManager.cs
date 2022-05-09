using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MaterialManager
{
    private static MaterialManager m_instance = null;

    public Material particleMaterial { get; set; }
    List<ParticleEmitter> particleEmitters = new List<ParticleEmitter>();
    public void add(ParticleEmitter emitter)
    {
        particleEmitters.Add(emitter);
    }
    public void remove(ParticleEmitter emiiter)
    {
        particleEmitters.Remove(emiiter);
    }
    public void render(CommandBuffer commadnBuffer, ScriptableRenderContext contex, Camera camera)
    {
        foreach(var emitter in particleEmitters)
        {
            emitter.RenderParticles(commadnBuffer, contex, camera);
        }
    }

    private MaterialManager() { }
    public static MaterialManager Instance
    {
        get
        {
            if(m_instance == null)
            {
                m_instance = new MaterialManager();
            }
            return m_instance;
        }
    }
}
