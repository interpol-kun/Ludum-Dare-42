using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [SerializeField]
    private Image hpBar, ammoBar;
    [SerializeField]
    private Text ammoText, waveText, endWaveText;

    [SerializeField]
    private GameObject upgradePanel, restartButton;

    [SerializeField]
    private GameObject[] objectsToHide;

    [SerializeField]
    private bool menu = false;

    [SerializeField]
    private Texture2D crosshair;

    private int waveCount;

    private void Awake()
    {
        if (!menu)
        {
            Cursor.SetCursor(crosshair, Vector2.zero, CursorMode.Auto);

            if(upgradePanel == null)
            {
                GameObject.FindGameObjectWithTag("UpgradePanel");
            }
            if(waveText == null)
            {
                waveText = GameObject.Find("WaveText").GetComponent<Text>();
            }
            
            waveText.text = "WAVE: " + 1;
        }   
    }

    private void Start()
    {
        Player.OnAmmoUpdate += UpdateAmmo;
        Player.OnHealthUpdate += UpdateHealth;
        Player.OnLevelFinished += SwitchUpgradePane;
        Player.OnDeath += DeathUI;
        Spawner.endOfWaveEvent += SwitchUpgradePane;
        Spawner.OnWaveEnd += UpdateWaveText;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void _ExitGame() {
        Application.Quit();
    }

    private void UpdateHealth(float value, float maxValue)
    {
        hpBar.fillAmount = value / maxValue;
    }

    private void UpdateAmmo(float value, float maxValue)
    {
        if (ammoBar != null)
        {
            ammoBar.fillAmount = value / maxValue;
            ammoText.text = value + "/" + maxValue;
        }
    }

    public void SwitchUpgradePane()
    {
        if(upgradePanel != null)
        {
            upgradePanel.SetActive(!upgradePanel.activeSelf);
        }
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateWaveText(int value)
    {
        waveText.text = "WAVE: " + (value + 1);
        waveCount = value;
    }

    private void DeathUI()
    {
        foreach(GameObject i in objectsToHide)
        {
            i.SetActive(false);
        }
        restartButton.SetActive(true);
        endWaveText.text = waveCount + " WAVES IN A ROW";
        Time.timeScale = 0.1f;
    }
}
