using UnityEngine;

public class MovementController : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject currentNode;
    public float speed = 4f;
    
    public string direction = "";
    public string lastMovingDirection = "";

    public bool canWarp = true;

    public bool isGhost;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Debug.Log("3");
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    private void Update()
    {
        Debug.Log("4");
        if (!gameManager.gameIsRunning)
        {
            return;
        }
        var currentNodeController = currentNode.GetComponent<NodeController>();

        transform.position = Vector2.MoveTowards(transform.position, currentNode.transform.position, speed*Time.deltaTime);

        var reverseDirection = (direction == "left" && lastMovingDirection == "right")
                               || (direction == "right" && lastMovingDirection == "left")
                               || (direction == "up" && lastMovingDirection == "down")
                               || (direction == "down" && lastMovingDirection == "up");
        
        //Figure out if we're at the center of our current node
        if ((Mathf.Approximately(transform.position.x, currentNode.transform.position.x) &&
             Mathf.Approximately(transform.position.y, currentNode.transform.position.y)) || reverseDirection)
        {
            if (isGhost)
            {
                GetComponent<EnemyController>().ReachedCenterOfNode(currentNodeController);
            }
            //If we reached the center of the left warp, warp to the right warp
            if (currentNodeController.isWarpLeftNode && canWarp)
            {
                currentNode = gameManager.rightWarpNode;
                direction = "left";
                lastMovingDirection = "left";
                transform.position = currentNode.transform.position;
                canWarp = false;
            }
            //If we reached the center of the right warp, warp to the left warp
            else if (currentNodeController.isWarpRightNode && canWarp)
            {
                currentNode = gameManager.leftWarpNode;
                direction = "right";
                lastMovingDirection = "right";
                transform.position = currentNode.transform.position;
                canWarp = false;
            }
            //Otherwise, find the next node we are going to be moving towards
            else
            {
                // If we are not a ghost that is respawning, and we are on the start node, and we are trying to move down, stop
                if (currentNodeController.isGhostStartingNode && direction == "down" 
                                                              && (!isGhost ||
                                                                  GetComponent<EnemyController>().ghostNodeState !=
                                                                  EnemyController.GhostNodeStatesEnum.Respawning))
                {
                    direction = lastMovingDirection;
                }
                //Get the next node from out node controller using our current direction
                var newNode = currentNodeController.GetNodeFromDirection(direction);
                //If we can move in the desired direction
                if (newNode)
                {
                    currentNode = newNode;
                    lastMovingDirection = direction;
                }
                //We can't move in desired direction try to keep going in the last moving direction
                else
                {
                    direction = lastMovingDirection;
                    newNode = currentNodeController.GetNodeFromDirection(direction);
                    if (newNode)
                    {
                        currentNode = newNode;
                    }
                }
            }
        }
        //We aren't in the center of a node
        else
        {
            canWarp = true;
        }
        
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public void SetDirection(string newDirection)
    {
        direction = newDirection;
    }
}
