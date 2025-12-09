using UnityEngine;

public class GameOverCanvas : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [SerializeField] private GameObject gameOverPart;
    [SerializeField] private GameObject gameplayCanvas;

    public void EnableAnimator()
    {
        animator.enabled = true;
    }

    public void DisableAnimator()
    {
        animator.enabled = false;
    }

    public void DisplayGameOver()
    {
        gameOverPart.SetActive(true);
        gameplayCanvas.SetActive(false);
    }

    public void HideGameOver()
    {
        gameOverPart.SetActive(false);
        gameplayCanvas.SetActive(true);
    }
}
