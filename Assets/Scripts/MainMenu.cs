using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public Text scoreText;
    public GameObject Panel;
    public Color32[] BGColors;

    private Image panelImg;
    private float currentColor;
    private float stepsBetweenColors;

    private int DwightCounter = 0;

    private void Start()
    {
        scoreText.text = "Top: " + PlayerPrefs.GetInt("TopScore").ToString();
        panelImg = Panel.GetComponent<Image>();

        // Initialize Color
        currentColor = UnityEngine.Random.Range(0f, 1f);
        stepsBetweenColors = 1f / BGColors.Length;
    }

    public void ToGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void Update()
    {
        panelImg.color = GetCurrentColor();
    }

    public void ShowAchievements()
    {
        PlayGames.ShowAchievementsUI();
    }

    public void ShowLeaderboard()
    {
        PlayGames.ShowLeaderboardsUI();
    }

    public void PokeDwight()
    {
        DwightCounter++;
        if(DwightCounter == 10)
        {
            PlayGames.UnlockAchievement(GPGSIds.achievement_stop_poking_me);
        }
    }

    private Color32 GetCurrentColor()
    {
        // Interpolate
        var currLowerColor = ((int)(currentColor * 100)) / ((int)(stepsBetweenColors * 100));
        var currStep = (currentColor % stepsBetweenColors) * BGColors.Length;


        // Progress Color
        currentColor += .001f;
        currentColor %= 1;
        if (currLowerColor != BGColors.Length - 1)
            return Color.Lerp(BGColors[currLowerColor], BGColors[currLowerColor + 1], currStep);
        else
            return Color.Lerp(BGColors[currLowerColor], BGColors[0], currStep);
    }
}
