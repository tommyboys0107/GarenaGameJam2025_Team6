using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingUIManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Title");
    }

}
