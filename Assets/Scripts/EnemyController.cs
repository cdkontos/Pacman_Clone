using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class EnemyController : MonoBehaviour
{

    public enum GhostNodeStatesEnum
    {
        Respawning,
        LeftNode,
        RightNode,
        CenterNode,
        StartNode,
        MovingInNodes
    }

    public GhostNodeStatesEnum ghostNodeState;
    public GhostNodeStatesEnum startGhostNodeState;
    public GhostNodeStatesEnum respawnState;

    public enum GhostType
    {
        Red,
        Blue,
        Pink,
        Orange
    }

    public GhostType ghostType;
    
    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

    public MovementController movementController;
    public GameManager gameManager;

    public GameObject startingNode;

    public bool readyToLeaveHome;
    public bool testRespawn;
    public bool isFrightened;
    public bool leftHomeBefore;
    
    public GameObject[] scatterNodes;

    public int scatterNodeIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Debug.Log("2a");
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        movementController = GetComponent<MovementController>();
        switch (ghostType)
        {
            case GhostType.Red:
                startGhostNodeState = GhostNodeStatesEnum.StartNode;
                respawnState = GhostNodeStatesEnum.CenterNode;
                startingNode = ghostNodeStart;
                Debug.Log(startingNode);
                break;
            case GhostType.Pink:
                startGhostNodeState = GhostNodeStatesEnum.CenterNode;
                respawnState = GhostNodeStatesEnum.CenterNode;
                startingNode = ghostNodeCenter;
                Debug.Log(startingNode);
                break;
            case GhostType.Blue:
                startGhostNodeState = GhostNodeStatesEnum.LeftNode;
                respawnState = GhostNodeStatesEnum.LeftNode;
                startingNode = ghostNodeLeft;
                Debug.Log(startingNode);
                break;
            case GhostType.Orange:
                startGhostNodeState = GhostNodeStatesEnum.RightNode;
                respawnState = GhostNodeStatesEnum.RightNode;
                startingNode = ghostNodeRight;
                Debug.Log(startingNode);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        // Reset our ghosts back to their starting position
        movementController.currentNode = startingNode;
        transform.position = startingNode.transform.position;
    }

    public void Setup()
    {
        ghostNodeState = startGhostNodeState;
        
        // Set their scatter node index back to 0
        scatterNodeIndex = 0;
        // Set isFrightened
        isFrightened = false;
        // Set readyToLeaveHome to false for some ghosts
        if (ghostType != GhostType.Red && ghostType != GhostType.Pink) return;
        readyToLeaveHome = true;
        leftHomeBefore = true;

    }

    // Update is called once per frame
    private void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            return;
        }
        if (testRespawn)
        {
            readyToLeaveHome = false;
            ghostNodeState = GhostNodeStatesEnum.Respawning;
            testRespawn = false;
        }

        movementController.SetSpeed(movementController.currentNode.GetComponent<NodeController>().isSideNode ? 1 : 3);
    }

    public void ReachedCenterOfNode(NodeController nodeController)
    {
        string direction;
        switch (ghostNodeState)
        {
            case GhostNodeStatesEnum.MovingInNodes:
                leftHomeBefore = true;
                // Scatter mode
                if (gameManager.currentGhostMode == GameManager.GhostMode.Scatter)
                {
                    DetermineGhostScatterModeDirection();
                }
                // Frightened mode
                else if (isFrightened)
                {
                    direction = GetRandomDirection();
                    movementController.SetDirection(direction);
                }
                // Chase mode
                else
                {
                    // Determine the next game node to go to
                    if (ghostType == GhostType.Red)
                    {
                        DetermineRedGhostDirection();
                    }
                    else if (ghostType == GhostType.Pink)
                    {
                        DeterminePinkGhostDirection();
                    }
                    else if (ghostType == GhostType.Blue)
                    {
                        DetermineBlueGhostDirection();
                    }
                    else if (ghostType == GhostType.Orange)
                    {
                        DetermineOrangeGhostDirection();
                    }
                }
                break;
            case GhostNodeStatesEnum.Respawning:
                direction = "";
                // We have reached our start node, move to the center node
                if (Mathf.Approximately(transform.position.x, ghostNodeStart.transform.position.x) &&
                    Mathf.Approximately(transform.position.y, ghostNodeStart.transform.position.y))
                {
                    direction = "down";
                }
                // We have reached our center node, either finish respawn or move to the left/right node
                else if (Mathf.Approximately(transform.position.x, ghostNodeCenter.transform.position.x) &&
                         Mathf.Approximately(transform.position.y, ghostNodeCenter.transform.position.y))
                {
                    switch (respawnState)
                    {
                        case GhostNodeStatesEnum.CenterNode:
                            ghostNodeState = respawnState;
                            break;
                        case GhostNodeStatesEnum.LeftNode:
                            direction = "left";
                            break;
                        case GhostNodeStatesEnum.RightNode:
                            direction = "right";
                            break;
                    }
                }
                // If our respawn state is either the left or the right node, and we got to that node, leave home again
                else if ((Mathf.Approximately(transform.position.x, ghostNodeLeft.transform.position.x) &&
                          Mathf.Approximately(transform.position.y, ghostNodeLeft.transform.position.y))
                         || (Mathf.Approximately(transform.position.x, ghostNodeRight.transform.position.x) &&
                             Mathf.Approximately(transform.position.y, ghostNodeRight.transform.position.y)))
                {
                    ghostNodeState = respawnState;
                }
                // We are in the game board still, locate our start node
                else
                {
                    // Determine quickest direction to home
                    direction = GetClosestDirection(ghostNodeStart.transform.position);
                }
                
                movementController.SetDirection(direction);
                break;
            case GhostNodeStatesEnum.LeftNode:
            case GhostNodeStatesEnum.RightNode:
            case GhostNodeStatesEnum.CenterNode:
            case GhostNodeStatesEnum.StartNode:
            default:
            {
                // If we are ready to leave home 
                if (!readyToLeaveHome) return;
                switch (ghostNodeState)
                {
                    // If we are in the left home node, move to the center
                    case GhostNodeStatesEnum.LeftNode:
                        ghostNodeState = GhostNodeStatesEnum.CenterNode;
                        movementController.SetDirection("right");
                        break;
                    // If we are in the right home node, move to the center
                    case GhostNodeStatesEnum.RightNode:
                        ghostNodeState = GhostNodeStatesEnum.CenterNode;
                        movementController.SetDirection("left");
                        break;
                    // If we are in the center node, move in the start node
                    case GhostNodeStatesEnum.CenterNode:
                        ghostNodeState = GhostNodeStatesEnum.StartNode;
                        movementController.SetDirection("up");
                        break;
                    // If we are in the start node, start moving around in the game
                    case GhostNodeStatesEnum.StartNode:
                        ghostNodeState = GhostNodeStatesEnum.MovingInNodes;
                        movementController.SetDirection("left");
                        break;
                    case GhostNodeStatesEnum.Respawning:
                        break;
                    case GhostNodeStatesEnum.MovingInNodes:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            }
        }
    }

    private string GetRandomDirection()
    {
        var possibleDirections = new List<string>();
        var nodeController = movementController.currentNode.GetComponent<NodeController>();

        if (nodeController.canMoveDown && movementController.lastMovingDirection != "up")
        {
            possibleDirections.Add("down");
        }

        if (nodeController.canMoveUp && movementController.lastMovingDirection != "down")
        {
            possibleDirections.Add("up");
        }

        if (nodeController.canMoveRight && movementController.lastMovingDirection != "left")
        {
            possibleDirections.Add("right");
        }

        if (nodeController.canMoveLeft && movementController.lastMovingDirection != "right")
        {
            possibleDirections.Add("left");    
        }

        var randomDirectionIndex = Random.Range(0, possibleDirections.Count - 1);
        var direction = possibleDirections[randomDirectionIndex];
        return direction;
    }
    private void DetermineGhostScatterModeDirection()
    {
        // If we reached the scatter node add 1 to our scatter node index
        if (Mathf.Approximately(transform.position.x, scatterNodes[scatterNodeIndex].transform.position.x) && Mathf.Approximately(transform.position.y, scatterNodes[scatterNodeIndex].transform.position.y))
        {
            scatterNodeIndex++;
            if (scatterNodeIndex == scatterNodes.Length - 1)
            {
                scatterNodeIndex = 0;
            }
        }
        var direction = GetClosestDirection(scatterNodes[scatterNodeIndex].transform.position);
        movementController.SetDirection(direction);
    }
    
    private void DetermineRedGhostDirection()
    {
        var direction = GetClosestDirection(gameManager.pacman.transform.position);
        movementController.SetDirection(direction);
    }

    private void DeterminePinkGhostDirection()
    {
        var pacmansDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirection;
        const float distanceBetweenNodes = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;
        switch (pacmansDirection)
        {
            case "left":
                target.x -= distanceBetweenNodes * 2;
                break;
            case "right":
                target.x += distanceBetweenNodes * 2;
                break;
            case "up":
                target.y += distanceBetweenNodes * 2;
                break;
            case "down":
                target.y -= distanceBetweenNodes * 2;
                break;
        }

        var direction = GetClosestDirection(target);
        movementController.SetDirection(direction);
    }

    private void DetermineBlueGhostDirection()
    {
        var pacmansDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirection;
        const float distanceBetweenNodes = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;
        switch (pacmansDirection)
        {
            case "left":
                target.x -= distanceBetweenNodes * 2;
                break;
            case "right":
                target.x += distanceBetweenNodes * 2;
                break;
            case "up":
                target.y += distanceBetweenNodes * 2;
                break;
            case "down":
                target.y -= distanceBetweenNodes * 2;
                break;
        }

        var redGhost = gameManager.redGhost;
        var xDistance = target.x - redGhost.transform.position.x;
        var yDistance = target.y - redGhost.transform.position.y;
        var blueTarget = new Vector2(target.x + xDistance, target.y + yDistance);
        var direction = GetClosestDirection(blueTarget);
        movementController.SetDirection(direction);
    }

    private void DetermineOrangeGhostDirection()
    {
        const float distanceBetweenNodes = 0.35f;
        var distance = Vector2.Distance(gameManager.pacman.transform.position, transform.position);
        if (distance < 0)
        {
            distance *= -1;
        }
        // If we are withing 8 nodes of pacman, chase him using red's logic
        if (distance <= distanceBetweenNodes * 8)
        {
            DetermineRedGhostDirection();
        }
        // Otherwise, use scatter mode logic
        else
        {
            // Scatter Mode
            DetermineGhostScatterModeDirection();
        }
    }

    private string GetClosestDirection(Vector2 target)
    {
        float shortestDistance = 0;
        var lastMovingDirection = movementController.lastMovingDirection;
        var newDirection = "";
        var nodeController = movementController.currentNode.GetComponent<NodeController>();
        
        // If we can move up and we aren't reversing 
        if (nodeController.canMoveUp && lastMovingDirection != "down")
        {
            // Get the node above us
            var nodeUp = nodeController.nodeUp;
            // Get the distance between our top node and pacman
            var distance = Vector2.Distance(nodeUp.transform.position, target);
            
            // If this is the shortest distance so far, set our direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "up";
            }
        }
        // If we can move up and we aren't reversing 
        if (nodeController.canMoveDown && lastMovingDirection != "up")
        {
            // Get the node above us
            var nodeDown = nodeController.nodeDown;
            // Get the distance between our top node and pacman
            var distance = Vector2.Distance(nodeDown.transform.position, target);
            
            // If this is the shortest distance so far, set our direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "down";
            }
        }
        // If we can move up and we aren't reversing 
        if (nodeController.canMoveLeft && lastMovingDirection != "right")
        {
            // Get the node above us
            var nodeLeft = nodeController.nodeLeft;
            // Get the distance between our top node and pacman
            var distance = Vector2.Distance(nodeLeft.transform.position, target);
            
            // If this is the shortest distance so far, set our direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "left";
            }
        }
        // If we can move up and we aren't reversing 
        if (!nodeController.canMoveRight || lastMovingDirection == "left") return newDirection;
        {
            // Get the node above us
            var nodeRight = nodeController.nodeRight;
            // Get the distance between our top node and pacman
            var distance = Vector2.Distance(nodeRight.transform.position, target);
            
            // If this is the shortest distance so far, set our direction
            if (!(distance < shortestDistance) && shortestDistance != 0) return newDirection;
            shortestDistance = distance;
            newDirection = "right";
        }

        return newDirection;
    }
}
