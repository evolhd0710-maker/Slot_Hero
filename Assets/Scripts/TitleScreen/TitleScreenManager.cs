using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    [Header("이동할 선택 씬")]
    [SerializeField] private string selectionSceneName = "StartingSelectionScene";

    [Header("입력 대기 시간")]
    [SerializeField] private float inputDelay = 0.3f;

    private float elapsedTime;
    private bool isLoading;

    public void NewGameButtonClicked()
    {
        if (string.IsNullOrWhiteSpace(selectionSceneName))
        {
            Debug.LogError("이동할 선택 씬 이름이 설정되지 않았습니다.");
            return;
        }

        isLoading = true;
        SceneManager.LoadScene(selectionSceneName);
    }

    public void ExitButtonClicked()
    {
        Application.Quit();
    }
}