using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] GameObject endScreen;

	[Header("Game Entities")]
	public GameObject realMonster;

	public bool isGameOver = false;

	void Start()
	{
		if (endScreen != null) endScreen.SetActive(false);

		if (realMonster != null) realMonster.SetActive(false);
	}

	public void StartGame()
	{
		Debug.Log("--- GAME STARTED: Monster Active ---");

		// Activeer het echte monster zodat hij begint te patrouilleren
		if (realMonster != null)
		{
			realMonster.SetActive(true);
		}

	}

	public void GameOver()
	{
		if (isGameOver) return;

		isGameOver = true;

		if (endScreen != null) endScreen.SetActive(true);

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public void RestartGame()
	{
		Debug.Log("Restarting...");
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}