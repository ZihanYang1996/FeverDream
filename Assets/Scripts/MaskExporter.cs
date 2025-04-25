using UnityEngine;
using System.IO;

/// <summary>
/// Exports the original mask and its outline from a RenderTexture.
/// </summary>
public class MaskExporter : MonoBehaviour
{
    [Header("Mask Rendering Configuration")]
    public Camera maskCamera;      // The camera used to render the mask
    public RenderTexture maskRT;   // The RenderTexture target for mask rendering

    [Header("Export Settings (relative to Assets folder)")]
    public string outputFolder = "StageData";  // Folder under Assets to save PNGs
    public string baseFileName = "Stage1Mask";      // Base name for output files

    /// <summary>
    /// Renders the mask and exports both the original mask and generated outline as PNG files.
    /// </summary>
    public void ExportMaskAndOutline()
    {
        // Render the mask into the RenderTexture
        maskCamera.targetTexture = maskRT;
        maskCamera.Render();
        RenderTexture.active = maskRT;

        // Read pixels from the RenderTexture into a Texture2D
        Texture2D maskTexture = new Texture2D(maskRT.width, maskRT.height, TextureFormat.RGBA32, false);
        maskTexture.ReadPixels(new Rect(0, 0, maskRT.width, maskRT.height), 0, 0);
        maskTexture.Apply();

        // Binarize the captured mask to ensure white shape on black background
        Texture2D binMask = BinarizeMask(maskTexture);

        // Use the binarized mask for export and outline generation
        maskTexture = binMask;

        // Cleanup RenderTexture binding
        RenderTexture.active = null;
        maskCamera.targetTexture = null;

        // Ensure output folder exists
        string folderPath = Application.dataPath + "/" + outputFolder;
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Export original mask PNG
        string maskPath = folderPath + "/" + baseFileName + ".png";
        File.WriteAllBytes(maskPath, maskTexture.EncodeToPNG());
        Debug.Log($"Original binary mask exported to: {maskPath}");

        // Generate and export outline PNG from the binary mask
        Texture2D outlineTexture = GenerateOutline(maskTexture, 1);
        string outlinePath = folderPath + "/" + baseFileName + "_Outline.png";
        File.WriteAllBytes(outlinePath, outlineTexture.EncodeToPNG());
        Debug.Log($"Outline mask exported to: {outlinePath}");
    }

    /// <summary>
    /// Generates an outline texture by detecting edges in the source mask.
    /// </summary>
    /// <param name="mask">Source mask texture (white shape on black background)</param>
    /// <param name="thickness">Thickness in pixels for the outline detection</param>
    /// <returns>Texture2D containing the outline (white on transparent)</returns>
    private Texture2D GenerateOutline(Texture2D mask, int thickness = 1)
    {
        int width = mask.width;
        int height = mask.height;
        Color32[] sourcePixels = mask.GetPixels32();
        Color32[] outlinePixels = new Color32[sourcePixels.Length];

        bool IsWhite(int x, int y) => sourcePixels[y * width + x].r > 128;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!IsWhite(x, y))
                {
                    bool edge = false;
                    for (int oy = -thickness; oy <= thickness && !edge; oy++)
                    {
                        for (int ox = -thickness; ox <= thickness && !edge; ox++)
                        {
                            int nx = x + ox;
                            int ny = y + oy;
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height && IsWhite(nx, ny))
                            {
                                edge = true;
                            }
                        }
                    }
                    outlinePixels[y * width + x] = edge 
                        ? new Color32(255, 255, 255, 255) 
                        : new Color32(0, 0, 0, 0);
                }
                else
                {
                    outlinePixels[y * width + x] = new Color32(0, 0, 0, 0);
                }
            }
        }

        Texture2D outline = new Texture2D(width, height, TextureFormat.RGBA32, false);
        outline.SetPixels32(outlinePixels);
        outline.Apply();
        return outline;
    }
    
    /// <summary>
    /// Convert any source texture into a pure black/white mask.
    /// White = shape (any non-black pixel), Black = background.
    /// </summary>
    private Texture2D BinarizeMask(Texture2D source)
    {
        int width = source.width;
        int height = source.height;
        Color32[] src = source.GetPixels32();
        Color32[] dst = new Color32[src.Length];

        for (int i = 0; i < src.Length; i++)
        {
            // Any non-black pixel is considered part of the shape
            bool isShape = (src[i].r + src[i].g + src[i].b) > 0;
            dst[i] = isShape
                ? new Color32(255, 255, 255, 255)
                : new Color32(0,   0,   0,   255);
        }

        Texture2D mask = new Texture2D(width, height, TextureFormat.RGBA32, false);
        mask.SetPixels32(dst);
        mask.Apply();
        return mask;
    }
}
