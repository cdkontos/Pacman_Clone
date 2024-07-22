using UnityEngine;

public class PlayerController : MonoBehaviour
{
    MovementController movementController;
    
    public SpriteRenderer sprite;
    public Animator animator;

    public GameObject startNode;

    public Vector2 startPos;

    public GameManager gameManager;
        
    private static readonly int Moving = Animator.StringToHash("moving");
    private static readonly int Direction = Animator.StringToHash("direction");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Debug.Log("2b");
        
    }

    public void Setup()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        startPos = new Vector2(-0.06f, -0.65f);
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        movementController = GetComponent<MovementController>();
        startNode = movementController.currentNode;
        movementController.currentNode = startNode;
        movementController.lastMovingDirection = "left";
        transform.position = startPos;
        animator.SetBool(Moving, false);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            return;
        }
        animator.SetBool(Moving,true);
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            movementController.SetDirection("left");
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            movementController.SetDirection("right");
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            movementController.SetDirection("up");
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            movementController.SetDirection("down");
        }

        var flipX = false;
        var flipY = false;
        switch (movementController.lastMovingDirection)
        {
            case "left":
                animator.SetInteger(Direction, 0);
                break;
            case "right":
                animator.SetInteger(Direction, 0);
                flipX = true;
                break;
            case "up":
                animator.SetInteger(Direction,1);
                break;
            case "down":
                animator.SetInteger(Direction,1);
                flipY = true;
                break;
        }

        sprite.flipY = flipY;
        sprite.flipX = flipX;
    }
}
