using UnityEngine;

public class ClickAndHover : MonoBehaviour
{
    private Vector3 originalScale;
    private Vector3 hoverScale;

    void Start()
    {
        originalScale = transform.localScale;
        hoverScale = originalScale * 1.1f; // Increase size by 10%
    }

    void OnMouseEnter()
    {
        transform.localScale = hoverScale;
    }

    void OnMouseExit()
    {
        transform.localScale = originalScale;
    }

    void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
