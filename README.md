This project is used for learning Unity Scriptable Render Pipeline, and following functions have been realized until now。

- Precompute functions
  - [x] Gen brdf LUT
  - [x] Gen irradiance map

- Functions realized in both forward render pipeline & deferred render pipeline：
  - [ ] Shadows
    - [x] Cascaded shadow map
    - [x] Convolution shadow map (still have some problem)
    - [x] VSM
    - [x] PCSS 
    - [ ] ESM
    - [x] PCF
    
  - [x] PBR
  
  - [x] Volume Light
  
  - [x] Bloom
  
    ![screenshot](\images\Bloom.png)
  
  - [x] GPU Particles
  
    ![screenshot](\images\GPUParticle.png)
  
- Forward render pipline 

  - [x] LightMap

    ![screenshot](/images/lightMap.png)
    
    ![screenshot](https://github.com/JolyneJoestar/MapEngine/blob/SSR/images/lightMap.png)
    
  - [x] NPR (basic implement)

    ![screenshot](/images/simpleNPR.png)

    ![screenshot](https://github.com/JolyneJoestar/MapEngine/blob/SSR/images/simpleNPR.png)

- Deferred render pipline
  
  - [x] SSR (still have some problem)
  
  - [x] TAA
  
  - [x] SSAO
  
    ![ssao](/images/ssao.png)
  
    ![screenshot](https://github.com/JolyneJoestar/MapEngine/blob/SSR/images/ssao.png)
  
  - [x] HBAO
  
    ![screenshot](/images/hbao.png)
  
    ![screenshot](https://github.com/JolyneJoestar/MapEngine/blob/SSR/images/hbao.png)
  
  



