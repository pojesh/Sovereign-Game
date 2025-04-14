using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DIPCardController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private DIPGameController gameController;
    
    [Header("Card Settings")]
    public float swipeThreshold = 100f;
    public float returnSpeed = 10f;
    
    private bool isDragging = false;
    private bool isAnimating = false;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        gameController = FindObjectOfType<DIPGameController>();
    }
    
    void Start()
    {
        originalPosition = rectTransform.anchoredPosition;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isAnimating) return;
        isDragging = true;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isAnimating) return;
        
        rectTransform.anchoredPosition += eventData.delta;
        
        // Notify game controller about drag position for showing action text
        float dragDistance = rectTransform.anchoredPosition.x - originalPosition.x;
        gameController.UpdateDragPosition(dragDistance);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || isAnimating) return;
        isDragging = false;
        
        float dragDistance = rectTransform.anchoredPosition.x - originalPosition.x;
        
        if (Mathf.Abs(dragDistance) > swipeThreshold)
        {
            // Card was swiped beyond threshold
            bool isLeft = dragDistance < 0;
            Vector2 targetPos = originalPosition;
            targetPos.x += isLeft ? -1000 : 1000;
            
            isAnimating = true;
            StartCoroutine(AnimateCard(targetPos, () => {
                gameController.ProcessDecision(isLeft);
                rectTransform.anchoredPosition = originalPosition;
                isAnimating = false;
            }));
        }
        else
        {
            // Return card to center
            isAnimating = true;
            StartCoroutine(AnimateCard(originalPosition, () => {
                gameController.ResetActionTexts();
                isAnimating = false;
            }));
        }
    }
    
    IEnumerator AnimateCard(Vector2 targetPosition, System.Action onComplete)
    {
        while (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) > 0.5f)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition, 
                targetPosition, 
                Time.deltaTime * returnSpeed
            );
            yield return null;
        }
        
        rectTransform.anchoredPosition = targetPosition;
        onComplete?.Invoke();
    }
}