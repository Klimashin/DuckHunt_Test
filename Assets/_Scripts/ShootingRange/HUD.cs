using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI BulletsText;
    [SerializeField] private TextMeshProUGUI ScoreText;
    [SerializeField] private TextMeshProUGUI LivesText;
    [SerializeField] private UiMenu PauseMenu;
    [SerializeField] private UiMenu WinMenu;
    [SerializeField] private UiMenu LoseMenu;
    [SerializeField] private Button PauseButton;
    [SerializeField] private ShootingRange ShootingRange;

    private void Start()
    {
        ShootingRange.OnGameLost += OnGameLost;
        ShootingRange.OnGameWon += OnGameWon;
        
        PauseButton.onClick.AddListener(() =>
        {
            if (Time.timeScale > 0)
            {
                PauseMenu.Show();
            }
        });

        PauseMenu.onShowAction = () =>
        {
            Time.timeScale = 0f;
            GameManager.Instance.InputActions.Disable();
        };

        PauseMenu.onHideAction = () =>
        {
            Time.timeScale = 1f;
            GameManager.Instance.InputActions.Enable();
        };
    }

    private void OnGameWon(ShootingRange shootingRange)
    {
        Time.timeScale = 0f;
        GameManager.Instance.InputActions.Disable();
        WinMenu.Show();
    }

    private void OnGameLost(ShootingRange shootingRange)
    {
        Time.timeScale = 0f;
        GameManager.Instance.InputActions.Disable();
        LoseMenu.Show();
    }

    private void Update()
    {
        BulletsText.text = ShootingRange.CurrentBullets.ToString();
        ScoreText.text = ShootingRange.CurrentScore.ToString();
        LivesText.text = ShootingRange.CurrentLives.ToString();
    }
}
