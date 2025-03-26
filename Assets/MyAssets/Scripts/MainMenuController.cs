using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour {
    [SerializeField] private string roleSelectorSceneName = "RoleSelector"; // Serialized field for scene name
    private Vector3 originalScale; // To store the original scale of the GameObject

    private void Start() {
        // Store the original scale of the GameObject
        originalScale = transform.localScale;
    }

    private void OnMouseEnter() {
        // Enlarge the GameObject when the mouse hovers over it
        transform.localScale = originalScale * 1.1f;
    }

    private void OnMouseExit() {
        // Restore the GameObject's scale when the mouse stops hovering
        transform.localScale = originalScale;
    }

    private void OnMouseDown() {
        Debug.Log("Image clicked! StartNewGame method called."); // Debug log to confirm click
        StartNewGame();
    }

    public void StartNewGame() {
        if (!string.IsNullOrEmpty(roleSelectorSceneName)) {
            SceneManager.LoadScene(roleSelectorSceneName);  // Loads Role Selector scene
        } else {
            Debug.LogError("Scene name is not set or is empty!");
        }
    }
}