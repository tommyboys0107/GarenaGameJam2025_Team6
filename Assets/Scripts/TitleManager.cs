using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField]
    GameObject credit = null;
    public void OpenCredits()
    {
        credit.SetActive(true);
    }

    public void CloseCredits()
    {
        credit.SetActive(true);
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Battle");
    }



    public void ExitGame()
    {
        Application.Quit();
    }

}
