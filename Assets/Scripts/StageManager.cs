using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
        Debug.Log("[StageManager] ValidatePuzzle called");
        // Manually render to the (permanently attached) captureRT
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

        // Cleanup
        RenderTexture.active = null;

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
}