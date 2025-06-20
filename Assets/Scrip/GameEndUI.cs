using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameEndUI : MonoBehaviour {
    public TextMeshProUGUI m_ResultText;
    public TextMeshProUGUI m_ScoreText;
    public TextMeshProUGUI m_TimeText;

    public void ShowGameEnd(string winner, int[] wins, float gameTime) {
        gameObject.SetActive(true);
        m_ResultText.text = winner == "Time Over" ? "TIEMPO AGOTADO!" : $"{winner} GANA!";
        m_ScoreText.text = $"Puntaje: {wins[0]} - {wins[1]}";
        m_TimeText.text = $"Tiempo: {gameTime:F1}s";
    }

    public void OnRestartButton() {
        SceneManager.LoadScene(0);
    }
}