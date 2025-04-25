using UnityEngine;
using UnityEngine.EventSystems;

public class TangramDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    // Removed dragOffset; now using screen delta for movement.
    private Vector3 originalPosition;
    private Transform originalParent;
    private bool isDragging = false;
    private Camera worldCamera;

    private float dragZ;

    private bool isPointerDown = false;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        worldCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera 
            ? canvas.worldCamera 
            : Camera.main;

        originalPosition = rectTransform.position;
        originalParent = transform.parent;
    }

    void Update()
    {
        if (isPointerDown)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                rectTransform.Rotate(0, 0, scroll * 15f);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform, true);
        // store Z distance for world-to-screen conversion
        dragZ = worldCamera.WorldToScreenPoint(rectTransform.position).z;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // convert screen drag delta to world movement
        Vector3 deltaScreen = new Vector3(eventData.delta.x, eventData.delta.y, dragZ);
        Vector3 worldDelta = worldCamera.ScreenToWorldPoint(deltaScreen)
            - worldCamera.ScreenToWorldPoint(new Vector3(0f, 0f, dragZ));
        rectTransform.position += worldDelta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        RectTransform workspaceRect = GameObject.Find("WorkspaceArea").GetComponent<RectTransform>();

        if (IsRectTransformFullyInside(rectTransform, workspaceRect))
        {
            transform.SetParent(workspaceRect, true);
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;

        // If no drag occurred (only rotation), return to original position and rotation
        if (!isDragging && transform.parent == originalParent)
        {
            ReturnToOriginalPosition();
        }
    }

    private void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent, true);
        rectTransform.position = originalPosition;
        rectTransform.rotation = Quaternion.identity;
    }

    // Returns true if all corners of 'child' are inside 'parent' using the worldCamera.
    private bool IsRectTransformFullyInside(RectTransform child, RectTransform parent)
    {
        Vector3[] childCorners = new Vector3[4];
        child.GetWorldCorners(childCorners);
        foreach (Vector3 corner in childCorners)
        {
            Vector2 screenPoint = worldCamera.WorldToScreenPoint(corner);
            if (!RectTransformUtility.RectangleContainsScreenPoint(parent, screenPoint, worldCamera))
                return false;
        }
        return true;
    }
}