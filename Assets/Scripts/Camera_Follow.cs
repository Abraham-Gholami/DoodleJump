
using UnityEngine;

public class Camera_Follow : MonoBehaviour 
{
    public Transform Target;

    [SerializeField] private float yPos = 3;
    
    [Header("Game Over Camera Settings")]
    [SerializeField] private float gameOverCameraSpeed = 0.1f; // Much slower speed when game over
    [SerializeField] private float gameOverDelay = 1f; // Wait time before camera starts moving down
    [SerializeField] private float cleanupDelay = 4f; // Total time before cleanup
    
    private GameObject GameController;
    private bool Game_Over = false;
    private float gameOverStartTime = 0;
    private bool hasStartedGameOverCamera = false;

    // Use this for initialization
    void Start()
    {
        GameController = GameObject.Find("GameController");
    }

    // Update is called once per frame
    void Update()
    {
        // Check game over
        if (GameController == null)
        {
            return;
        }
        
        bool wasGameOver = Game_Over;
        Game_Over = GameController.GetComponent<GameController>().Get_GameOver();
        
        // Record when game over starts
        if (Game_Over && !wasGameOver)
        {
            gameOverStartTime = Time.time;
            hasStartedGameOverCamera = false;
        }
    }

    void FixedUpdate()
    {
        // Move camera down slowly if game over
        if (Game_Over)
        {
            float timeSinceGameOver = Time.time - gameOverStartTime;
            
            // Wait for delay before starting camera movement
            if (timeSinceGameOver >= gameOverDelay && !hasStartedGameOverCamera)
            {
                hasStartedGameOverCamera = true;
            }
            
            // Move camera down slowly after delay
            if (hasStartedGameOverCamera && timeSinceGameOver < cleanupDelay)
            {
                transform.position -= new Vector3(0, gameOverCameraSpeed * Time.fixedDeltaTime, 0);
            }
            else if (timeSinceGameOver >= cleanupDelay)
            {
                // Delete player and all objects after full delay
                GameObject Player = GameObject.FindGameObjectWithTag("Player");
                GameObject[] Objects = GameObject.FindGameObjectsWithTag("Object");

                if (Player != null) Destroy(Player);
                foreach (GameObject Obj in Objects)
                {
                    if (Obj != null) Destroy(Obj);
                }
            }
        }
    }

    void LateUpdate() 
    {
        if (!Game_Over)
        {
            if (Target == null) return;
            // if target.y > camera.y + 2
            if (Target.position.y > transform.position.y - yPos)
            {
                Vector3 New_Pos = new Vector3(transform.position.x, Target.position.y + yPos, transform.position.z);
                transform.position = New_Pos;
            }
        }
    }
}