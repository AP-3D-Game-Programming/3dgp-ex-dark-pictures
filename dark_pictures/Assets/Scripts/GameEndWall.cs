using UnityEngine;

public class GameEndWall : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlayerCollider")
        {
            gameManager.RestartGame();
        }
    }
}
