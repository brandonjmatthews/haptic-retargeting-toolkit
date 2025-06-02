/*
 * HRTK: InpaintTest.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using System.IO;
using UnityEngine;

public class InpaintTest : MonoBehaviour
{
    [Range(1, 20)][SerializeField] int iterations = 5;
    public Texture2D source;
    [SerializeField] RenderTexture[] mipmaps;
    [SerializeField] RenderTexture[] upscaled;
    [SerializeField] Shader downsampleShader;
    Material downsampleMaterial;
    [SerializeField] Shader upscaleShader;

    Material upscaleMaterial;

    void SaveRenderTexture(RenderTexture texture, string name)
    {
        SaveTexture.SaveRenderTextureToFile(texture, Path.Combine(Application.persistentDataPath, name));
    }

    private void Start()
    {
        downsampleMaterial = new Material(downsampleShader);
        upscaleMaterial = new Material(upscaleShader);

        mipmaps = new RenderTexture[iterations];
        upscaled = new RenderTexture[iterations];
        // Downscale output
        int rtW = source.width / 2;
        int rtH = source.height / 2;

        mipmaps[0] = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
        mipmaps[0].filterMode = FilterMode.Point;
        Graphics.Blit(source, mipmaps[0]);

        for (int i = 1; i < iterations; i++)
        {

            if (mipmaps[i] == null)
            {
                mipmaps[i] = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
                // mipmaps[i] = new RenderTexture(rtW, rtH, 0, RenderTextureFormat.ARGB32);
                mipmaps[i].filterMode = FilterMode.Point;
            }

            Graphics.Blit(mipmaps[i - 1], mipmaps[i], downsampleMaterial, 0);
            SaveRenderTexture(mipmaps[i], "mipmap" + i + ".png");
            rtW = rtW / 2;
            rtH = rtH / 2;
        }

        for (int i = iterations - 2; i >= 0; i--)
        {
            // Fill map is the map that mask pixels are being filled
            RenderTexture fillMap = mipmaps[i];
            int fillW = fillMap.width;
            int fillH = fillMap.height;

            // Source map is the smaller mipmap that is being used to fill 'fillMap' 
            RenderTexture srcMap = mipmaps[i + 1];

            if (upscaled[i + 1] == null)
            {
                upscaled[i + 1] = RenderTexture.GetTemporary(fillW, fillH, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
                // upscaled[i + 1] = new RenderTexture(fillW, fillH, 0, RenderTextureFormat.ARGB32);
            }
            RenderTexture dstMap = upscaled[i + 1];

            upscaleMaterial.SetTexture("_FillTex", fillMap);
            // Write to dstMap so fillMap can be passed as an argument
            Graphics.Blit(srcMap, dstMap, upscaleMaterial);
            if (upscaled[i+1] != null) SaveRenderTexture(upscaled[i+1], "upscale" + (i+1) + ".png");
            Graphics.Blit(dstMap, fillMap);
        }
    }
}