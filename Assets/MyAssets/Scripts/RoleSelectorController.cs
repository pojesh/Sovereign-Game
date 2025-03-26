using UnityEngine;
using UnityEngine.SceneManagement;

public class RoleSelectorController : MonoBehaviour {
    public Transform primeMinisterCard;
    public Transform ceoCard;
    public Transform diplomatCard;
    private Vector3 originalScale;

    void Start() {
        // Store the original scale of the Prime Minister card (assuming all cards have the same scale)
        originalScale = primeMinisterCard.localScale;
    }

    private void OnMouseEnter() {
        // Enlarge the card when the mouse hovers over it
        transform.localScale = originalScale * 1.1f;
    }

    private void OnMouseExit() {
        // Restore the card's size when the mouse stops hovering
        transform.localScale = originalScale;
    }

    private void OnMouseDown() {
        // Detect which card is clicked and select the corresponding role
        if (gameObject == primeMinisterCard.gameObject) {
            SelectRole("PrimeMinister");
        } else if (gameObject == ceoCard.gameObject) {
            SelectRole("CEO");
        } else if (gameObject == diplomatCard.gameObject) {
            SelectRole("Diplomat");
        }
    }

    public void SelectRole(string role) {
        // Save the selected role and load the main game scene
        PlayerPrefs.SetString("SelectedRole", role);
        SceneManager.LoadScene("MainGameScene");
    }
}