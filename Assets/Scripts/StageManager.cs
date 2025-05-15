using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Serialization;
using System.Collections;

public class StageManager : MonoBehaviour
{
    public Action<bool, StageData> OnPuzzleComplete;

    [Header("UI References")]
    public GameObject puzzlePanel; // PuzzlePanel object

    [SerializeField] private GameObject blackScreenGameObject;

    public Image workspaceOutlineImage; // Image in WorkspaceArea to display selected outline
    public Transform workspaceArea; // WorkspaceArea container
    public Button validateButton; // ValidateButton
    public Button startPuzzleButton; // Start button to open puzzle panel

    [FormerlySerializedAs("generatedTangramParent")] [SerializeField]
    private Transform generatedTangramHolder;

    [Header("Render Config")]
    public Camera captureCamera; // MaskCamera

    public RenderTexture captureRT; // Mask_RT


    [Header("Stage Data (Don't modify)")]
    public StageData currentStage; // Your StageData asset

    [Header("Clone Capture Config")]
    public Canvas captureCanvas; // Off-screen canvas for clone capture

    public Transform captureWorkspaceArea; // Parent in captureCanvas to clone pieces under

    [Header("Pattern Selector Config")]
    public Transform patternSelectorContainer; // Container for pattern buttons

    public GameObject patternButtonPrefab; // Prefab for pattern button
    // public List<StageData> allStages; // List of all stages to generate buttons

    // Holds all stage data references
    private List<StageData> allStages;
    private Dictionary<string, StageData> stageDict = new Dictionary<string, StageData>();

    [Header("Export Settings")]
    [SerializeField] private string exportFolderName = "Temp"; // Folder under Assets

    [SerializeField] private string exportFileName = "playerMask"; // PNG file name without extension

    [Header("Normalization Settings")]
    [SerializeField] private int normalizationResolution = 256;

    [Header("Gameplay Timing")]
    [SerializeField] private float countdownDuration = 30f;

    [SerializeField] public float generatedTangramFlickerDuration = 2.0f;
    [SerializeField] public int generatedTangramFlickerCount = 2;

    private float countdownTimer;
    private bool puzzleCompleted = false;
    [SerializeField] private TMPro.TextMeshProUGUI countdownText;

    [SerializeField] private Image screenFlashImage;
    [SerializeField] private float flashDuration = 0.2f;

    private Texture2D playerMask;

    [Header("Testing")]
    [SerializeField] private Button testSuccessButton;

    [SerializeField] private Button testFailButton;

    void Start()
    {
        // hide puzzle at start
        puzzlePanel.SetActive(false);
        // hide screen flash image
        screenFlashImage.gameObject.SetActive(false);
        // hide black screen image
        // bind Start Demo to show puzzle panel
        startPuzzleButton.onClick.AddListener(ShowPuzzlePanel);
        // bind validate
        validateButton.onClick.AddListener(ValidatePuzzle);
        // Permanently direct captureCamera to render into captureRT
        captureCamera.targetTexture = captureRT;
        countdownTimer = countdownDuration;

        // Test buttons for success/fail
        testSuccessButton.onClick.AddListener(() => { HandlePuzzleResult(true); });
        testFailButton.onClick.AddListener(() => { HandlePuzzleResult(false); });
    }

    /// <summary>
    /// Injects the list of stages to be used by this manager.
    /// </summary>
    public void SetupStages(List<StageData> stages)
    {
        allStages = stages;
        stageDict.Clear();

        foreach (var s in allStages)
        {
            if (!string.IsNullOrEmpty(s.id))
            {
                stageDict[s.id] = s;
            }
        }

        GeneratePatternButtons();
    }

    /// <summary>
    /// Generates pattern selector buttons for each stage.
    /// </summary>
    public void GeneratePatternButtons()
    {
        if (allStages == null) return;

        foreach (Transform child in patternSelectorContainer)
        {
            Destroy(child.gameObject); // Clear previous
        }

        for (int i = 0; i < allStages.Count; i++)
        {
            StageData sd = allStages[i];
            GameObject btnGO = Instantiate(patternButtonPrefab, patternSelectorContainer);
            btnGO.name = "PatternBtn_" + sd.name;
            Image img = btnGO.GetComponent<Image>();
            img.sprite = sd.outlineSprite;
            img.preserveAspect = true;
            // If completed and answerSprite is available, show answerSprite
            if (GameManager.Instance.HasCompletedStage(sd.id) && sd.solutionSprite != null)
            {
                img.sprite = sd.solutionSprite;
            }

            Button btn = btnGO.GetComponent<Button>();
            string stageId = sd.id;
            btn.onClick.AddListener(() => SelectStageById(stageId));
        }
    }

    /// <summary>
    /// Show puzzle UI and display target outline
    /// </summary>
    public void LoadStage(StageData stage)
    {
        currentStage = stage;
        // set outline sprite
        workspaceOutlineImage.sprite = stage.outlineSprite;
        workspaceOutlineImage.color = Color.white;
        // If completed and answerSprite is available, show answerSprite
        if (GameManager.Instance.HasCompletedStage(stage.id) && stage.solutionSprite != null)
        {
            workspaceOutlineImage.sprite = stage.solutionSprite;
        }

        // clear previous pieces
        // Return all pieces to their original positions and parents
        for (int i = workspaceArea.childCount - 1; i >= 0; i--)
        {
            Transform piece = workspaceArea.GetChild(i);
            TangramDraggable draggable = piece.GetComponent<TangramDraggable>();
            if (draggable != null)
            {
                draggable.Reset();
            }
        }

        // show panel
        puzzlePanel.SetActive(true);
    }


    public void SelectStageById(string id)
    {
        if (stageDict != null && stageDict.TryGetValue(id, out StageData stage))
        {
            currentStage = stage;
            LoadStage(stage);
        }
        else
        {
            Debug.LogWarning($"Stage with ID '{id}' not found.");
        }
    }

    /// <summary>
    /// Displays the puzzle panel without loading a stage.
    /// </summary>
    public void ShowPuzzlePanel()
    {
        // Activate the black screen
        if (blackScreenGameObject != null)
        {
            blackScreenGameObject.SetActive(true);
            // Get and set the alpha to the desired value for the puzzle from GameManager
            blackScreenGameObject.GetComponent<BlackScreenController>()
                .SetAlphaInstantly(GameManager.Instance.blackScreenAlphaForPuzzle / 255f);
        }

        // Show the puzzle panel
        puzzlePanel.SetActive(true);

        // Align the capture camera to the workspace area
        AlignCaptureCameraToWorkspace();

        // Hide the start button
        if (startPuzzleButton != null)
        {
            // Disable the button to prevent multiple clicks
            startPuzzleButton.interactable = false;
            startPuzzleButton.gameObject.SetActive(false);
        }
        BeginCountdown();
    }

    /// <summary>
    /// Capture player's workspace, compute IoU and log result
    /// </summary>
    public void ValidatePuzzle()
    {
        if (currentStage == null)
        {
            Debug.LogWarning("[StageManager] No stage loaded.");
            // Flash red to indicate failure
            StartCoroutine(FlashRedEffect());
            return;
        }

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

        // // Export captured mask to Assets folder
        // string folderPath = Path.Combine(Application.dataPath, exportFolderName);
        // if (!Directory.Exists(folderPath))
        // {
        //     Directory.CreateDirectory(folderPath);
        // }
        //
        // string filePath = Path.Combine(folderPath, exportFileName + ".png");
        // File.WriteAllBytes(filePath, playerMask.EncodeToPNG());
        // Debug.Log($"Player mask exported to: {filePath}");

        // Cleanup RenderTexture binding
        RenderTexture.active = null;

        // Disable capture canvas so it doesn’t display in main UI
        captureCanvas.gameObject.SetActive(false);

        // Normalize and align both masks
        Texture2D normTarget = NormalizeAndAlignMask(currentStage.maskTexture);
        Texture2D normPlayer = NormalizeAndAlignMask(playerMask);

        // Compute IoU on normalized masks (no translation needed)
        float iou = ComputeTranslatedIoU(normTarget, normPlayer, 0, 0);
        bool success = iou >= currentStage.iouThreshold;
        Debug.Log($"[Validate] IoU = {iou:F2} => {(success ? "Success" : "Fail")}");

        if (success && !puzzleCompleted)
        {
            // Disable the button to prevent multiple clicks
            validateButton.interactable = false;

            HandlePuzzleResult(true);
        }
        else if (!success)
        {
            // If not successful, flash red, but player can still retry
            StartCoroutine(FlashRedEffect());
        }

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
                : new Color32(0, 0, 0, 255);
        }

        Texture2D mask = new Texture2D(width, height, TextureFormat.RGBA32, false);
        mask.SetPixels32(dst);
        mask.Apply();
        return mask;
    }


    /// <summary>
    /// Normalizes a binary mask by cropping to its bounding box, scaling to a fixed resolution,
    /// centering its centroid, and rotating to align principal axis.
    /// </summary>
    private Texture2D NormalizeAndAlignMask(Texture2D mask)
    {
        int w = mask.width, h = mask.height;
        Color32[] pixels = mask.GetPixels32();

        // 1) Find bounding box of white pixels
        int minX = w, minY = h, maxX = 0, maxY = 0;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (pixels[y * w + x].r > 128)
                {
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }
            }
        }

        if (maxX < minX || maxY < minY)
        {
            // empty mask -> return blank
            return new Texture2D(normalizationResolution, normalizationResolution, TextureFormat.RGBA32, false);
        }

        // 2) Crop and scale to normalizationResolution
        int cropW = maxX - minX + 1, cropH = maxY - minY + 1;
        Texture2D scaled = new Texture2D(normalizationResolution, normalizationResolution, TextureFormat.RGBA32, false);
        for (int ny = 0; ny < normalizationResolution; ny++)
        {
            for (int nx = 0; nx < normalizationResolution; nx++)
            {
                float u = nx / (float)(normalizationResolution - 1);
                float v = ny / (float)(normalizationResolution - 1);
                int sx = minX + Mathf.RoundToInt(u * (cropW - 1));
                int sy = minY + Mathf.RoundToInt(v * (cropH - 1));
                Color32 c = pixels[sy * w + sx];
                scaled.SetPixel(nx, ny, c.r > 128 ? Color.white : Color.black);
            }
        }

        scaled.Apply();

        // 3) Compute centroid of scaled mask
        Vector2 centroid = ComputeCentroid(scaled);

        // 4) PCA orientation and 5) rotation removed for consistency.
        // Return the scaled mask directly.
        return scaled;
    }

    private IEnumerator CountdownTimer()
    {
        int secondsLeft = Mathf.CeilToInt(countdownDuration);

        while (secondsLeft > 0)
        {
            countdownTimer = secondsLeft;

            if (countdownText != null)
            {
                countdownText.text = secondsLeft.ToString();
            }

            yield return new WaitForSeconds(1f);
            if (puzzleCompleted) yield break;

            secondsLeft--;
        }

        countdownTimer = 0;
        if (countdownText != null)
        {
            countdownText.text = "";
        }

        if (!puzzleCompleted)
        {
            Debug.Log("[StageManager] Countdown expired. Puzzle failed.");
            HandlePuzzleResult(false);
        }
    }

    private void HandlePuzzleResult(bool success)
    {
        if (!success)
        {
            StartCoroutine(FlashRedEffect());
        }

        puzzleCompleted = success;
        StartCoroutine(PlayPuzzleSuccessVisualEffect(() => { OnPuzzleComplete?.Invoke(success, currentStage); }));
    }

    private IEnumerator FlashRedEffect()
    {
        if (screenFlashImage == null) yield break;

        Color originalColor = screenFlashImage.color;
        screenFlashImage.color = new Color(1f, 1, 1f, 0.5f);
        screenFlashImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(flashDuration);

        screenFlashImage.gameObject.SetActive(false);
        screenFlashImage.color = originalColor;
    }


    /// <summary>
    /// Plays a synchronized visual effect: fade in black screen, fade out outline, flicker workspaceArea children, then fade out them.
    /// </summary>
    public IEnumerator PlayPuzzleSuccessVisualEffect(Action onComplete = null)
    {
        // Step 1: Fade out all puzzlePanel children except workspaceArea's children
        List<Graphic> graphicsToFade = new List<Graphic>();
        foreach (Graphic g in puzzlePanel.GetComponentsInChildren<Graphic>(true))
        {
            if (g.transform == puzzlePanel.transform) continue;
            if (g.transform.IsChildOf(workspaceArea)) continue;
            Debug.Log(g.name);
            graphicsToFade.Add(g);
        }

        // Also add workspaceArea's own Graphic if present
        Graphic workspaceGraphic = workspaceArea.GetComponent<Graphic>();
        if (workspaceGraphic != null)
        {
            graphicsToFade.Add(workspaceGraphic);
        }

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(1f, 0f, t);
            foreach (var g in graphicsToFade)
            {
                Color c = g.color;
                c.a = alpha;
                g.color = c;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (var g in graphicsToFade)
        {
            Color c = g.color;
            c.a = 0f;
            g.color = c;
        }

        // Step 2: Flicker all workspaceArea children (2 full flickers, then one fade out)
        int flickerCount = generatedTangramFlickerCount;
        float flickerDuration = generatedTangramFlickerDuration;
        float minAlpha = 0.0f;

        // Prepare canvas groups
        for (int i = 0; i < workspaceArea.childCount; i++)
        {
            CanvasGroup group = workspaceArea.GetChild(i).GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = workspaceArea.GetChild(i).gameObject.AddComponent<CanvasGroup>();
            }

            group.alpha = 1f;
        }

        // Perform 2 flickers (fade out then in, twice) using Mathf.Sin for smoother flicker
        for (int flick = 0; flick < flickerCount; flick++)
        {
            // Fade out
            float fadeOutElapsed = 0f;
            float halfFlicker = flickerDuration / 2f;
            while (fadeOutElapsed < halfFlicker)
            {
                float t = fadeOutElapsed / halfFlicker;
                float alpha = Mathf.Sin((1f - t) * Mathf.PI * 0.5f); // 1→0, sin(π/2→0): 1→0
                alpha = Mathf.Lerp(minAlpha, 1f, alpha); // ensure minAlpha support
                foreach (Transform child in workspaceArea)
                {
                    child.GetComponent<CanvasGroup>().alpha = alpha;
                }

                fadeOutElapsed += Time.deltaTime;
                yield return null;
            }

            // Fade in
            float fadeInElapsed = 0f;
            while (fadeInElapsed < halfFlicker)
            {
                float t = fadeInElapsed / halfFlicker;
                float alpha = Mathf.Sin(t * Mathf.PI * 0.5f); // 0→1, sin(0→π/2): 0→1
                alpha = Mathf.Lerp(minAlpha, 1f, alpha);
                foreach (Transform child in workspaceArea)
                {
                    child.GetComponent<CanvasGroup>().alpha = alpha;
                }

                fadeInElapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Third flicker: fade out only, do not fade back in, use Mathf.Sin for smooth fade
        float finalFadeDuration = flickerDuration / 2f;
        float finalFadeElapsed = 0f;
        while (finalFadeElapsed < finalFadeDuration)
        {
            float t = finalFadeElapsed / finalFadeDuration;
            float alpha = Mathf.Sin((1f - t) * Mathf.PI * 0.5f); // 1→0, sin(π/2→0): 1→0
            foreach (Transform child in workspaceArea)
            {
                child.GetComponent<CanvasGroup>().alpha = alpha;
            }

            finalFadeElapsed += Time.deltaTime;
            yield return null;
        }

        foreach (Transform child in workspaceArea)
        {
            child.GetComponent<CanvasGroup>().alpha = 0f;
        }

        // Disable workspaceArea
        workspaceArea.gameObject.SetActive(false);

        // Step 3: Create new solution sprite as a child of generatedTangramHolder and center it
        if (currentStage != null && currentStage.solutionSprite != null && generatedTangramHolder != null && puzzleCompleted)
        {
            SpawnSpriteAtWorkspace(currentStage.solutionSprite);

            // Fade in the sprite using Mathf.Sin for a smooth ease
            // float fadeInDuration = flickerDuration / 2f;
            // float fadeElapsed = 0f;
            // while (fadeElapsed < fadeInDuration)
            // {
            //     float t = fadeElapsed / fadeInDuration;
            //     float alpha = Mathf.Sin(t * Mathf.PI * 0.5f); // 0→1, sin(0→π/2): 0→1
            //     Color newColor = sr.color;
            //     newColor.a = alpha;
            //     sr.color = newColor;
            //     fadeElapsed += Time.deltaTime;
            //     yield return null;
            // }
            // sr.color = new Color(1f, 1f, 1f, 1f); // Ensure fully opaque
        }

        // Step 4: Disable puzzle panel
        puzzlePanel.SetActive(false);

        // Step 5: Invoke completion callback
        onComplete?.Invoke();
    }

    /// <summary>
    /// Manually begins the countdown timer, resetting it and updating the display.
    /// </summary>
    public void BeginCountdown()
    {
        countdownTimer = countdownDuration;

        if (countdownText != null)
        {
            countdownText.text = Mathf.CeilToInt(countdownTimer).ToString();
        }

        StartCoroutine(CountdownTimer());
    }

    /// <summary>
    /// Aligns the capture camera to frame the workspace area exactly.
    /// </summary>
    private void AlignCaptureCameraToWorkspace()
    {
        RectTransform workspaceRect = workspaceArea.GetComponent<RectTransform>();
        Camera uiCamera = workspaceRect.GetComponentInParent<Canvas>().worldCamera;

        // 1. 获取世界中心点
        Vector3[] corners = new Vector3[4];
        workspaceRect.GetWorldCorners(corners);
        Vector3 centerWorld = (corners[0] + corners[2]) * 0.5f;

        // 2. 将摄像机移动到这个中心上方
        captureCamera.transform.position =
            new Vector3(centerWorld.x, centerWorld.y, captureCamera.transform.position.z);

        // 3. 设置合适的 orthographic size，使它刚好框住 workspace
        float height = Vector3.Distance(corners[1], corners[2]);
        float orthoSize = height * 0.5f;
        captureCamera.orthographicSize = orthoSize;
    }

    /// <summary>
    /// Returns the world-space center of a RectTransform.
    /// </summary>
    private Vector3 GetWorldCenterOfRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return (corners[0] + corners[2]) * 0.5f;
    }

    /// <summary>
    /// Spawns a sprite at the center of the workspace area in world space.
    /// </summary>
    private void SpawnSpriteAtWorkspace(Sprite spriteToUse)
    {
        if (spriteToUse == null) return;

        Vector3 worldCenter = GetWorldCenterOfRect(workspaceArea.GetComponent<RectTransform>());
        generatedTangramHolder.position = worldCenter;

        GameObject go = new GameObject("GeneratedTangram");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spriteToUse;
        sr.sortingLayerName = "AboveCurtain";
        sr.sortingOrder = 10;
        // Make sure the sprite is transparent
        sr.color = new Color(1f, 1f, 1f, 0f);

        go.transform.SetParent(generatedTangramHolder, false);
        go.transform.localScale = CalculateSpriteScaleToFit(workspaceArea.GetComponent<RectTransform>(), spriteToUse);
    }

    /// <summary>
    /// Calculates a scale vector so that the sprite fits within the target RectTransform.
    /// </summary>
    private Vector3 CalculateSpriteScaleToFit(RectTransform targetRect, Sprite sprite)
    {
        Vector3[] corners = new Vector3[4];
        targetRect.GetWorldCorners(corners);
        float worldWidth = Vector3.Distance(corners[0], corners[3]);
        float worldHeight = Vector3.Distance(corners[0], corners[1]);

        Vector2 spriteSize = sprite.bounds.size;

        float scaleX = worldWidth / spriteSize.x;
        float scaleY = worldHeight / spriteSize.y;

        float finalScale = Mathf.Min(scaleX, scaleY);
        return Vector3.one * finalScale;
    }
}