using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class StageManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject puzzlePanel;    // PuzzlePanel object
    [FormerlySerializedAs("outlineImage")] 
    public Image outlineArea;        // OutlineArea's Image
    public Transform workspaceArea;   // WorkspaceArea container
    public Button validateButton;     // ValidateButton

    [Header("Render Config")]
    public Camera captureCamera;      // MaskCamera
    public RenderTexture captureRT;   // Mask_RT

    [Header("Stage Data")]
    public StageData currentStage;    // Your StageData asset

    [Header("Clone Capture Config")]
    public Canvas captureCanvas;          // Off-screen canvas for clone capture
    public Transform captureWorkspaceArea; // Parent in captureCanvas to clone pieces under

    [Header("Export Settings")]
    [SerializeField] private string exportFolderName = "Temp";    // Folder under Assets
    [SerializeField] private string exportFileName = "playerMask";// PNG file name without extension

    private Texture2D playerMask;

    void Start()
    {
        // hide puzzle at start
        puzzlePanel.SetActive(false);
        // bind validate
        validateButton.onClick.AddListener(ValidatePuzzle);
        // Permanently direct captureCamera to render into captureRT
        captureCamera.targetTexture = captureRT;
    }

    /// <summary>
    /// Show puzzle UI and display target outline
    /// </summary>
    public void LoadStage(StageData stage)
    {
        currentStage = stage;
        // set outline sprite
        outlineArea.sprite = stage.outlineSprite;
        outlineArea.color = Color.white;
        // clear previous pieces
        foreach (Transform t in workspaceArea) Destroy(t.gameObject);
        // show panel
        puzzlePanel.SetActive(true);
    }

    /// <summary>
    /// Capture player's workspace, compute IoU and log result
    /// </summary>
    public void ValidatePuzzle()
    {
        // Clone workspace pieces into off-screen capture canvas
        List<GameObject> clones = new List<GameObject>();
        foreach (Transform orig in workspaceArea)
        {
            // Instantiate clone under captureWorkspaceArea, preserving world position
            GameObject clone = Instantiate(orig.gameObject, captureWorkspaceArea, worldPositionStays: true);
            clones.Add(clone);
        }

        Debug.Log("[StageManager] ValidatePuzzle called");
        // Enable capture canvas so captureCamera sees clones
        captureCanvas.gameObject.SetActive(true);
        
        // Render clones into the RenderTexture
        captureCamera.Render();
        RenderTexture.active = captureRT;

        // Read pixels into Texture2D
        if (playerMask == null ||
            playerMask.width != captureRT.width ||
            playerMask.height != captureRT.height)
        {
            playerMask = new Texture2D(captureRT.width, captureRT.height, TextureFormat.RGBA32, false);
        }
        playerMask.ReadPixels(new Rect(0, 0, captureRT.width, captureRT.height), 0, 0);
        playerMask.Apply();

        // Convert captured mask to pure black and white
        playerMask = BinarizeMask(playerMask);

        // Export captured mask to Assets folder
        string folderPath = Path.Combine(Application.dataPath, exportFolderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string filePath = Path.Combine(folderPath, exportFileName + ".png");
        File.WriteAllBytes(filePath, playerMask.EncodeToPNG());
        Debug.Log($"Player mask exported to: {filePath}");

        // Cleanup RenderTexture binding
        RenderTexture.active = null;

        // Disable capture canvas so it doesn’t display in main UI
        captureCanvas.gameObject.SetActive(false);

        // Compute centroids for alignment
        Vector2 targetCentroid = ComputeCentroid(currentStage.maskTexture);
        Vector2 playerCentroid = ComputeCentroid(playerMask);
        Vector2 offset = targetCentroid - playerCentroid;
        int dx = Mathf.RoundToInt(offset.x);
        int dy = Mathf.RoundToInt(offset.y);

        // Compute IoU and check threshold
        float iou = ComputeTranslatedIoU(currentStage.maskTexture, playerMask, dx, dy);
        bool success = iou >= currentStage.iouThreshold;
        Debug.Log($"[Validate] IoU = {iou:F2} => {(success ? "Success" : "Fail")}");

        // Destroy clones
        foreach (var c in clones)
        {
            Destroy(c);
        }
    }

    /// <summary>
    /// Calculates the centroid of white pixels in a binary mask.
    /// If no white pixels are found, returns the center of the texture.
    /// </summary>
    private Vector2 ComputeCentroid(Texture2D mask)
    {
        Color32[] pixels = mask.GetPixels32();
        int width = mask.width;
        int height = mask.height;
        long sumX = 0, sumY = 0, count = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pixels[y * width + x].r > 128)
                {
                    sumX += x;
                    sumY += y;
                    count++;
                }
            }
        }

        if (count == 0)
        {
            // No shape pixels: default to center
            return new Vector2(width / 2f, height / 2f);
        }
        // Return average position
        return new Vector2(sumX / (float)count, sumY / (float)count);
    }

    /// <summary>
    /// Computes the Intersection-over-Union (IoU) of two binary masks
    /// after translating the player's mask by (dx, dy).
    /// </summary>
    private float ComputeTranslatedIoU(Texture2D target, Texture2D player, int dx, int dy)
    {
        int width = target.width;
        int height = target.height;
        Color32[] targetPixels = target.GetPixels32();
        Color32[] playerPixels = player.GetPixels32();
        int overlap = 0, union = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool t = targetPixels[y * width + x].r > 128;
                int px = x - dx;
                int py = y - dy;
                bool p = false;
                if (px >= 0 && px < width && py >= 0 && py < height)
                {
                    p = playerPixels[py * width + px].r > 128;
                }

                if (t || p) union++;
                if (t && p) overlap++;
            }
        }

        return union > 0 ? overlap / (float)union : 0f;
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