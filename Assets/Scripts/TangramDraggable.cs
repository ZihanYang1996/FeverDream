using UnityEngine;
using UnityEngine.EventSystems;

public class TangramDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 originalPosition;
    private Transform originalParent;
    private bool isDragging = false;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
    }

    void Update()
    {
        if (isDragging)
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
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        RectTransform workspaceRect = GameObject.Find("WorkspaceArea").GetComponent<RectTransform>();

        if (IsRectTransformFullyInside(rectTransform, workspaceRect, eventData.pressEventCamera))
        {
            transform.SetParent(workspaceRect, true);
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }

    private void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.rotation = Quaternion.identity;
    }

    // uiCamera: null for Screen Space - Overlay, use the camera for Screen Space - Camera
    private bool IsRectTransformFullyInside(RectTransform child, RectTransform parent, Camera uiCamera)
    {
        Vector3[] childCorners = new Vector3[4];
        child.GetWorldCorners(childCorners);

        foreach (Vector3 corner in childCorners)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(parent, corner, uiCamera))
            {
                return false;
            }
        }
        return true;
    }
}