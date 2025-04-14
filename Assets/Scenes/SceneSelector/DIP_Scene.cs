using UnityEngine;
using UnityEngine.SceneManagement;

public class DIPLOMATScene : MonoBehaviour
{
    private Vector3 originalScale;
    private Vector3 hoverScale;

    void Start()
    {
        originalScale = transform.localScale;
        hoverScale = originalScale * 1.1f; // Increase by 10%
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
            SceneManager.LoadScene("DIP_Game");
        }
    }
}