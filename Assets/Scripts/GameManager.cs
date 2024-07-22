using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject pacman;
    public GameObject leftWarpNode;
    public GameObject rightWarpNode;
    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;
    public GameObject redGhost;
    public GameObject pinkGhost;
    public GameObject blueGhost;
    public GameObject orangeGhost;

    public AudioSource siren;
    public AudioSource munch1;
    public AudioSource munch2;
    public AudioSource startGameAudio;

    public EnemyController redGhostController;
    public EnemyController pinkGhostController;
    public EnemyController blueGhostController;
    public EnemyController orangeGhostController;

    public int currentMunch;
    public int score;
    public int totalPellets;
    public int pelletsLeft;
    public int pelletsCollectedOnThisLife;
    public int lives;
    public int currentLevel;
    
    public TextMeshProUGUI scoreText;

    public bool hadDeathOnThisLevel;
    public bool gameIsRunning;
    public bool newGame;
    public bool clearedLevel;

    public List<NodeController> nodeControllers = new();
    
    public enum GhostMode
    {
        Chase,
        Scatter
    }

    public GhostMode currentGhostMode;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Debug.Log("1");
        newGame = true;
        clearedLevel = false;
        redGhostController = redGhost.GetComponent<EnemyController>();
        pinkGhostController = pinkGhost.GetComponent<EnemyController>();
        blueGhostController = blueGhost.GetComponent<EnemyController>();
        orangeGhostController = orangeGhost.GetComponent<EnemyController>();
        ghostNodeStart.GetComponent<NodeController>().isGhostStartingNode = true;
        scoreText = GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>();

        pacman = GameObject.Find("Player");

        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        // If pacman clears a level, a background will appear covering the level and the game will pause for 0.1 seconds
        if (clearedLevel)
        {
            yield return new WaitForSeconds(0.1f);
        }

        pelletsCollectedOnThisLife = 0;
        currentGhostMode = GhostMode.Scatter;
        gameIsRunning = false;
        currentMunch = 0;
        var waitTimer = 1f;
        if (clearedLevel || newGame)
        {
            waitTimer = 4f;
            // Pellets will respawn when pacman clears the level or starts a new game
            foreach (var t in nodeControllers)
            {
                t.RespawnPellet();
            }
        }

        if (newGame)
        {
            startGameAudio.Play();
            score = 0;
            scoreText.SetText("Score: " + score);
            lives = 3;
            currentLevel = 1;
        }
        
        pacman.GetComponent<PlayerController>().Setup();
        redGhostController.Setup();
        pinkGhostController.Setup();
        blueGhostController.Setup();
        orangeGhostController.Setup();

        newGame = false;
        clearedLevel = false;
        yield return new WaitForSeconds(waitTimer);
        
        StartGame();
    }

    private void StartGame()
    {
        gameIsRunning = true;
        siren.Play();
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public void GotPelletFromNodeController(NodeController nodeController)
    {
        nodeControllers.Add(nodeController);
        totalPellets++;
        pelletsLeft++;
    }
    private void AddToScore(int amount)
    {
        score += amount;
        scoreText.SetText("Score: " + score);
    }
    public void CollectedPellet(NodeController nodeController)
    {
        switch (currentMunch)
        {
            case 0:
                munch1.Play();
                currentMunch = 1;
                break;
            case 1:
                munch2.Play();
                currentMunch = 0;
                break;
        }

        pelletsLeft--;
        pelletsCollectedOnThisLife++;
        int requiredBluePellets;
        int requiredOrangePellets;

        if (hadDeathOnThisLevel)
        {
            requiredBluePellets = 12;
            requiredOrangePellets = 32;
        }
        else
        {
            requiredBluePellets = 30;
            requiredOrangePellets = 60;
        }

        if (pelletsCollectedOnThisLife >= requiredBluePellets && !blueGhostController.leftHomeBefore)
        {
            blueGhostController.readyToLeaveHome = true;
        }
        if (pelletsCollectedOnThisLife >= requiredOrangePellets && !orangeGhostController.leftHomeBefore)
        {
           orangeGhostController.readyToLeaveHome = true;
        }
        // Add to score
        AddToScore(10);
        // Check if there are any pellets left
        
        // Check how many pellets are eaten
        
        // Check if it is a power pellet
    }
}
