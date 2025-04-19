using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeToRoleSelector : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("RoleSelector");
        }
    }
}
