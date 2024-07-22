using UnityEngine;

public class NodeController : MonoBehaviour
{
    public bool canMoveLeft;
    public bool canMoveRight;
    public bool canMoveUp;
    public bool canMoveDown;

    public GameObject nodeLeft;
    public GameObject nodeRight;
    public GameObject nodeUp;
    public GameObject nodeDown;

    public bool isWarpRightNode;
    public bool isWarpLeftNode;
    public bool isSideNode;
    
    public GameManager gameManager;

    // If the node contains a pellet when the game starts
    public bool isPelletNode;
    // If the node still has a pellet
    public bool hasPellet;

    public bool isGhostStartingNode;
    
    public SpriteRenderer pelletSprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        if (transform.childCount > 0)
        {
            gameManager.GotPelletFromNodeController(this);
            hasPellet = true;
            isPelletNode = true;
            pelletSprite = GetComponentInChildren<SpriteRenderer>();
        }
        
        var hitsDown =
            //Shoot raycast line going down
            Physics2D.RaycastAll(transform.position, Vector2.down);
        //Loop through all the game objects that the raycast hit
        for (var i = 0; i < hitsDown.Length; i++)
        {
            var distance = Mathf.Abs(hitsDown[i].point.y - transform.position.y);
            if (!(distance < 0.4f && hitsDown[i].collider.CompareTag("Node"))) continue;
            canMoveDown = true;
            nodeDown = hitsDown[i].collider.gameObject;
        }

        var hitsUp =
            //Shoot raycast line going up
            Physics2D.RaycastAll(transform.position, Vector2.up);
        //Loop through all the game objects that the raycast hit
        for (var i = 0; i < hitsUp.Length; i++)
        {
            var distance = Mathf.Abs(hitsUp[i].point.y - transform.position.y);
            if (!(distance < 0.4f && hitsUp[i].collider.CompareTag("Node"))) continue;
            canMoveUp = true;
            nodeUp = hitsUp[i].collider.gameObject;
        }

        var hitsRight =
            // Shoot raycast line going right
            Physics2D.RaycastAll(transform.position, Vector2.right);
        // Loop through all the game objects that the raycast hit
        for (var i = 0; i < hitsRight.Length; i++)
        {
            var distance = Mathf.Abs(hitsRight[i].point.x - transform.position.x);
            if (!(distance < 0.4f && hitsRight[i].collider.CompareTag("Node"))) continue;
            canMoveRight = true;
            nodeRight = hitsRight[i].collider.gameObject;
        }

        var hitsLeft =
            // Shoot raycast line going left
            Physics2D.RaycastAll(transform.position, Vector2.left);
        // Loop through all the game objects that the raycast hit
        for (var i = 0; i < hitsLeft.Length; i++)
        {
            var distance = Mathf.Abs(hitsLeft[i].point.x - transform.position.x);
            if (!(distance < 0.4f && hitsLeft[i].collider.CompareTag("Node"))) continue;
            canMoveLeft = true;
            nodeLeft = hitsLeft[i].collider.gameObject;
        }

        if (!isGhostStartingNode) return;
        canMoveDown = true;
        nodeDown = gameManager.ghostNodeCenter;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GetNodeFromDirection(string direction)
    {
        switch (direction)
        {
            case "left" when canMoveLeft:
                return nodeLeft;
            case "right" when canMoveRight:
                return nodeRight;
            case "up" when canMoveUp:
                return nodeUp;
            case "down" when canMoveDown:
                return nodeDown;
            default:
                return null;
        }
    }

    public void RespawnPellet()
    {
        if (!isPelletNode) return;
        hasPellet = true;
        pelletSprite.enabled = true;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !hasPellet) return;
        hasPellet = false;
        pelletSprite.enabled = false;
        gameManager.CollectedPellet(this);
    }
}
