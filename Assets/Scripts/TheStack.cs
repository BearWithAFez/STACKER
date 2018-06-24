using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TheStack : MonoBehaviour
{
    // Public's
    public GameObject Stack;            // Reference to the stack object
    public GameObject DeathPile;        // All Cut-offs
    public Camera Cam;                  // Reference to the camera
    public Color32[] TileColors;        // Colors the tiles will interpolate between
    public Text ScoreText;
    public GameObject EndPanel;

    private List<GameObject> theStack;
    private List<GameObject> theDeathPile;
    private GameObject underlyingTile;
    private int score;
    private bool playerAlive;

    private float currentColor;
    private float stepsBetweenColors;

    private Vector3 camera_velocity;
    private Vector3 camera_goal;
    private float cameraDelay;

    private GameObject newTile;
    private float tileSpeed;
    private float emptyMargin;
    private float errorMargin;
    private float tileTransition;
    private int tileTransitionDirX;
    private int tileTransitionDirZ;

    private Vector3 defaultCameraPosition;
    private bool cameraReachedBottom;

    private void Start()
    {
        // Initialize the stack
        theStack = new List<GameObject> { transform.GetChild(0).gameObject };

        // Initalize the DeathPile
        theDeathPile = new List<GameObject>();

        // Initialize Score
        score = 0;
        ScoreText.text = score.ToString();

        // Initialize Player
        playerAlive = true;
        emptyMargin = 0.5f;
        errorMargin = 0.05f;
        tileTransition = 0f;
        tileTransitionDirX = 1;
        tileTransitionDirZ = 0;

        // Initialize Color
        currentColor = UnityEngine.Random.Range(0f, 1f);
        stepsBetweenColors = 1f / TileColors.Length;

        // Initialize the underlying tile
        underlyingTile = Stack.transform.GetChild(0).gameObject;

        //Initialize Camera
        camera_goal = Cam.transform.position;
        cameraDelay = 0.5F;
        camera_velocity = Vector3.zero;
        defaultCameraPosition = camera_goal;
        cameraReachedBottom = false;

        // Get the newTile Going
        tileSpeed = 2.5f;
        CreateNewTile();
    }

    private void Update()
    {
        if (playerAlive)
        {
            // Upon Left-click
            if (Input.GetMouseButtonDown(0))
            {
                if (playerAlive)
                {
                    if (TryPlaceTile())
                    {
                        SpawnTile();
                        UpdateScore();
                    }
                    else
                    {
                        GameOver();
                    }
                }
            }
            // Move the camera
            Cam.transform.position = Vector3.SmoothDamp(Cam.transform.position, camera_goal, ref camera_velocity, cameraDelay);

            // Move temp Tile
            MoveTile();
        }
        else
        {
            // Camera movement
            SpinCamera();
        }
    }

    private void UpdateScore()
    {
        score++;
        ScoreText.text = score.ToString();

        //Achievements
        if (score == 1)
        {
            PlayGames.UnlockAchievement(GPGSIds.achievement_hello_world);
        }else if (score == 10)
        {
            PlayGames.UnlockAchievement(GPGSIds.achievement_baby_steps);
        }
        else if (score == 30)
        {
            PlayGames.UnlockAchievement(GPGSIds.achievement_getting_there);
        }
        else if (score == 100)
        {
            PlayGames.UnlockAchievement(GPGSIds.achievement_stack_champ);
        }
    }

    private void MoveTile()
    {
        if (!playerAlive) return;   // Not when gameOver

        tileTransition = Time.deltaTime * tileSpeed;
        newTile.transform.localPosition += new Vector3(tileTransition * tileTransitionDirX, 0, tileTransition * tileTransitionDirZ);

        // X-Bounds
        if (newTile.transform.position.x + newTile.transform.localScale.x / 2 + emptyMargin < underlyingTile.transform.position.x - underlyingTile.transform.localScale.x / 2) tileTransitionDirX = 1;
        if (newTile.transform.position.x - newTile.transform.localScale.x / 2 - emptyMargin > underlyingTile.transform.position.x + underlyingTile.transform.localScale.x / 2) tileTransitionDirX = -1;

        // Z-Bounds
        if (newTile.transform.position.z + newTile.transform.localScale.z / 2 + emptyMargin < underlyingTile.transform.position.z - underlyingTile.transform.localScale.z / 2) tileTransitionDirZ = 1;
        if (newTile.transform.position.z - newTile.transform.localScale.z / 2 - emptyMargin > underlyingTile.transform.position.z + underlyingTile.transform.localScale.z / 2) tileTransitionDirZ = -1;
    }

    /// <summary>
    /// End the game
    /// </summary>
    private void GameOver()
    {
        // Kill player
        playerAlive = false;
        if (PlayerPrefs.GetInt("TopScore") < score)
        {
            PlayerPrefs.SetInt("TopScore", score);
            PlayGames.AddScoreToLeaderboard(GPGSIds.leaderboard_highscores, score);
        }

        // 1000 Steps progress
        PlayGames.IncrementAchievement(GPGSIds.achievement_stack_addict, score);

        // Worst Stacker Achievement
        if (score == 0)
        {
            PlayGames.UnlockAchievement(GPGSIds.achievement_worst_stacker_ever);
        }

        // Make cut-off-s
        var deadPlayer = new GameObject[8];
        for (int i = 0; i < deadPlayer.Length; i++)
        {
            deadPlayer[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deadPlayer[i].transform.localScale = newTile.transform.localScale / 2;
            deadPlayer[i].transform.position = newTile.transform.position;
            deadPlayer[i].AddComponent<Rigidbody>();
            deadPlayer[i].GetComponent<Renderer>().material.color = newTile.GetComponent<Renderer>().material.color;
            theDeathPile.Add(deadPlayer[i]);
        }

        // Set them where the old one stood
        deadPlayer[0].transform.position += new Vector3(-newTile.transform.localScale.x, -newTile.transform.localScale.y, -newTile.transform.localScale.z) / 6;
        deadPlayer[1].transform.position += new Vector3(-newTile.transform.localScale.x, -newTile.transform.localScale.y, newTile.transform.localScale.z) / 6;
        deadPlayer[2].transform.position += new Vector3(-newTile.transform.localScale.x, newTile.transform.localScale.y, -newTile.transform.localScale.z) / 6;
        deadPlayer[3].transform.position += new Vector3(-newTile.transform.localScale.x, newTile.transform.localScale.y, newTile.transform.localScale.z) / 6;
        deadPlayer[4].transform.position += new Vector3(newTile.transform.localScale.x, -newTile.transform.localScale.y, -newTile.transform.localScale.z) / 6;
        deadPlayer[5].transform.position += new Vector3(newTile.transform.localScale.x, -newTile.transform.localScale.y, newTile.transform.localScale.z) / 6;
        deadPlayer[6].transform.position += new Vector3(newTile.transform.localScale.x, newTile.transform.localScale.y, -newTile.transform.localScale.z) / 6;
        deadPlayer[7].transform.position += new Vector3(newTile.transform.localScale.x, newTile.transform.localScale.y, newTile.transform.localScale.z) / 6;

        // Destroy the original
        Destroy(newTile);

        // Set camera to floor
        camera_goal = defaultCameraPosition;

        // Show end panel
        EndPanel.SetActive(true);
    }

    private Color32 GetCurrentColor()
    {
        // Interpolate
        var currLowerColor = ((int)(currentColor * 100)) / ((int)(stepsBetweenColors * 100));
        var currStep = (currentColor % stepsBetweenColors) * TileColors.Length;


        // Progress Color
        currentColor += .01f;
        currentColor %= 1;
        if (currLowerColor != TileColors.Length - 1)
            return Color.Lerp(TileColors[currLowerColor], TileColors[currLowerColor + 1], currStep);
        else
            return Color.Lerp(TileColors[currLowerColor], TileColors[0], currStep);
    }

    /// <summary>
    /// Looks if the newly placed tile is on the stack
    /// </summary>
    /// <returns>Wheter it's on it</returns>
    private bool TryPlaceTile()
    {
        // Cut the new tile X-Wise
        float delta = underlyingTile.transform.position.x - newTile.transform.position.x;
        float absDelta = Math.Abs(delta);

        if (absDelta > errorMargin)
        {
            int toInvert = (delta < 0) ? 1 : -1;

            if (absDelta > underlyingTile.transform.localScale.x) return false;

            // Adjust newtile
            newTile.transform.localScale = underlyingTile.transform.localScale - new Vector3(absDelta, 0, 0);
            newTile.transform.position = underlyingTile.transform.position + new Vector3(((underlyingTile.transform.localScale.x - newTile.transform.localScale.x) / 2) * toInvert, 1, 0);

            // Make cut-off
            var cut_off = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cut_off.name = "Cut-off #" + score;
            cut_off.transform.localScale = new Vector3(absDelta, 1, underlyingTile.transform.localScale.z);
            cut_off.transform.position = underlyingTile.transform.position + new Vector3(((underlyingTile.transform.localScale.x + absDelta - (0.2f * absDelta)) / 2) * toInvert, 1, 0);
            cut_off.transform.parent = DeathPile.transform;
            cut_off.GetComponent<Renderer>().material.color = newTile.GetComponent<Renderer>().material.color;
            cut_off.AddComponent<Rigidbody>();
            theDeathPile.Add(cut_off);
        }
        else if (absDelta > 0)
        {
            // Close enough
            newTile.transform.position = underlyingTile.transform.position + new Vector3(0, 1, 0);
        }

        // Cut the new tile Z-Wise
        delta = underlyingTile.transform.position.z - newTile.transform.position.z;
        absDelta = Math.Abs(delta);
        if (absDelta > errorMargin)
        {
            int toInvert = (delta < 0) ? 1 : -1;

            if (absDelta > underlyingTile.transform.localScale.z) return false; // Next To It

            newTile.transform.localScale = underlyingTile.transform.localScale - new Vector3(0, 0, absDelta);
            newTile.transform.position = underlyingTile.transform.position + new Vector3(0, 1, ((underlyingTile.transform.localScale.z - newTile.transform.localScale.z) / 2) * toInvert);

            // Make cut-off
            var cut_off = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cut_off.name = "Cut-off #" + score;
            cut_off.transform.localScale = new Vector3(underlyingTile.transform.localScale.x, 1, absDelta);
            cut_off.transform.position = underlyingTile.transform.position + new Vector3(0, 1, ((underlyingTile.transform.localScale.z + absDelta - (0.2f * absDelta)) / 2) * toInvert);
            cut_off.transform.parent = DeathPile.transform;
            cut_off.GetComponent<Renderer>().material.color = newTile.GetComponent<Renderer>().material.color;
            cut_off.AddComponent<Rigidbody>();
            theDeathPile.Add(cut_off);
        }
        else if (absDelta > 0)
        {
            // Close enough
            newTile.transform.position = underlyingTile.transform.position + new Vector3(0, 1, 0);
        }

        return true;
    }

    /// <summary>
    /// Actually adds a new tile to the stack
    /// </summary>
    private void SpawnTile()
    {
        // Add the new Tile to the Stack
        newTile.transform.parent = Stack.transform; // Add to stack object
        newTile.name = "Tile #" + score;            // Set name
        theStack.Add(newTile);                      // Add to list
        underlyingTile = newTile;                   // Set underlying to this
        CreateNewTile();

        // Set new camera Goal
        camera_goal += new Vector3(0, 1, 0);
    }

    private void CreateNewTile()
    {
        // Standard Tile Creation
        newTile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        newTile.name = "New tile";
        newTile.transform.localScale = underlyingTile.transform.localScale;
        newTile.transform.position = underlyingTile.transform.position;
        newTile.GetComponent<Renderer>().material.color = GetCurrentColor();

        // Movement swapping
        tileTransitionDirX = (tileTransitionDirX == 0) ? 1 : 0;
        tileTransitionDirZ = (tileTransitionDirZ == 0) ? 1 : 0;

        // Movement pre-placement
        newTile.transform.position += (tileTransitionDirX == 0) ?
            new Vector3(0, 1, -underlyingTile.transform.localScale.z - emptyMargin) :
            new Vector3(-underlyingTile.transform.localScale.x - emptyMargin, 1, 0);
    }

    private void SpinCamera()
    {
        if (cameraReachedBottom) Cam.transform.RotateAround(Vector3.zero, Vector3.up, .2f);
        else
        {
            Cam.transform.position = Vector3.SmoothDamp(Cam.transform.position, camera_goal, ref camera_velocity, cameraDelay/2);
            if (Cam.transform.position == camera_goal) cameraReachedBottom = true;
        }
    }

    public void OnButtonClick(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
