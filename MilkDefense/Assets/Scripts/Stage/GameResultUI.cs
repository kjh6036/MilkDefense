using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameResultUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private Button _lobbyButton;
    [SerializeField] private Button _retryButton;

    private const string LobbySceneName = "Lobby";

    private void Awake()
    {
        DependencyInjector.Constructor(this);
        _panel.SetActive(false);

        _lobbyButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(LobbySceneName);
        });
        _retryButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    private void OnDestroy()
    {
        DependencyInjector.Demolisher(this);
    }

    public void ShowVictory()
    {
        _resultText.text = "승리!";
        _panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ShowDefeat()
    {
        _resultText.text = "패배...";
        _panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        _panel.SetActive(false);
        Time.timeScale = 1f;
    }
}