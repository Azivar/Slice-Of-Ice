using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(System.IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);

    private GameObject guy;
    private Rigidbody2D guyPhysics;
    private GameObject island;
    public GameObject wall;
    public GameObject bomb;

    private Vector3 guySize;
    private Vector3 islandSize;


    private int MAX_SCREEN_WIDTH = 96;
    private int MAX_SCREEN_HEIGHT = 96;

    public int updatesPerSecond = 40;
    public float playerAcceleration = 0.3f;
    public float maxSpeed = 0.8f;
    public float iceFriction = 0.04f;

    private int posX, posY, whenToReset, counter, direction;
    private bool moveIsland, freezePlayer;

    private System.IntPtr hwnd;

    void Start() {
        whenToReset = Random.Range(50, 70);
        posX = Screen.currentResolution.width / 2;
        posY = Screen.currentResolution.height / 2;
        hwnd = GetActiveWindow();
        counter = 0;
        direction = 1;
        moveIsland = true;
        freezePlayer = false;

        Time.fixedDeltaTime = 1f / updatesPerSecond;

        guy = GameObject.FindGameObjectWithTag("Player");
        guyPhysics = guy.GetComponent<Rigidbody2D>();
        island = GameObject.FindGameObjectWithTag("Island");
        guySize = guy.GetComponent<SpriteRenderer>().bounds.size;
        islandSize = island.GetComponent<SpriteRenderer>().bounds.size;

        SetWindowPos(hwnd, 0, posX, posY, MAX_SCREEN_WIDTH, MAX_SCREEN_HEIGHT, 32 | 64);
        StartCoroutine(FreezeIsland(4f));
    }

    void Update() {
        if (!freezePlayer) {
            Debug.Log(guyPhysics.velocity);

            Vector2 verticalMovementSpeed = new Vector2(0f, playerAcceleration);
            Vector2 horizontalMovementSpeed = new Vector2(playerAcceleration, 0f);

            // Move the guy
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
                guyPhysics.velocity += (guyPhysics.velocity.x > -maxSpeed) ? -horizontalMovementSpeed : Vector2.zero;
            } 
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
                guyPhysics.velocity += (guyPhysics.velocity.x < maxSpeed) ? horizontalMovementSpeed : Vector2.zero;
            } 
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
                guyPhysics.velocity += (guyPhysics.velocity.y < maxSpeed) ? verticalMovementSpeed : Vector2.zero;
            } 
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
                guyPhysics.velocity += (guyPhysics.velocity.y > -maxSpeed) ? -verticalMovementSpeed : Vector2.zero;
            }

            // Check to see if you are falling off the ledge
            if (
                (guy.transform.position.x + guySize.x / 2 >= islandSize.x / 2) ||
                (guy.transform.position.x - guySize.x / 2 <= -islandSize.x / 2) ||
                (guy.transform.position.y + guySize.y / 2 >= islandSize.y / 2) ||
                (guy.transform.position.y - guySize.y / 2 <= -islandSize.y / 2)
            ) {
                freezePlayer = true;
                moveIsland = false;
            }
        } else {
            guyPhysics.velocity = Vector2.zero;
            guy.transform.Rotate(0, 0, Time.deltaTime * 60 * 5);
        }
    }

    void LateUpdate() {
        Vector2 verticalFriction = new Vector2(0f, iceFriction);
        Vector2 horizontalFriction = new Vector2(iceFriction, 0f);
        Vector2 currentSpeed = guyPhysics.velocity;

        // Friction on ICY surface
        if (guyPhysics.velocity.x > 0) {
            guyPhysics.velocity -= horizontalFriction;
            if (guyPhysics.velocity.x < 0) {
                guyPhysics.velocity = new Vector2(0, currentSpeed.y);
            }
        }
        if (guyPhysics.velocity.x < 0) {
            guyPhysics.velocity += horizontalFriction;
            if (guyPhysics.velocity.x > 0) {
                guyPhysics.velocity = new Vector2(0, currentSpeed.y);
            }
        }
        if (guyPhysics.velocity.y > 0) {
            guyPhysics.velocity -= verticalFriction;
            if (guyPhysics.velocity.y < 0) {
                guyPhysics.velocity = new Vector2(currentSpeed.x, 0);
            }
        }
        if (guyPhysics.velocity.y < 0) {
            guyPhysics.velocity += verticalFriction;
            if (guyPhysics.velocity.y > 0) {
                guyPhysics.velocity = new Vector2(currentSpeed.x, 0);
            }
        }

    }

    private IEnumerator FreezeIsland(float waitTime) {
        moveIsland = false;
        yield return new WaitForSeconds(waitTime);
        moveIsland = true;
    }

    // Fixed is a lie
    void FixedUpdate() {
        // New random direction set at a random time
        if (moveIsland && ++counter % whenToReset == 0) {
            direction = Random.Range(0, 8);
            whenToReset = Random.Range(updatesPerSecond*3, updatesPerSecond*10);

            StartCoroutine(FreezeIsland(0.2f)); // Wait a god damn second
        }

        else if (moveIsland) {
            Vector3 verticalPushSpeed = new Vector3(0f, Time.fixedDeltaTime * 5, 0f);
            Vector3 horizontalPushSpeed = new Vector3(Time.fixedDeltaTime * 5, 0f, 0f);

            // Game window moves based off of direction
            if (direction <= 3) {
                if (!(posY - 1 <= 0)) { // Don't pass bottom of screen
                    posY -= 1;
                    Debug.Log(-verticalPushSpeed);
                    guyPhysics.AddForce(-verticalPushSpeed);
                } else {
                    posY = 0;
                }
            }
            if (direction >= 6) {
                if (!(posY + 1 >= Screen.currentResolution.height - MAX_SCREEN_HEIGHT)) { // Don't pass top of screen
                    posY += 1;
                    Debug.Log(verticalPushSpeed);
                    guyPhysics.AddForce(verticalPushSpeed);
                } else {
                    posY = Screen.currentResolution.height - MAX_SCREEN_HEIGHT;
                }
            }
            if (direction == 2 || direction == 4 || direction == 6) {
                if (!(posX - 1 <= 0)) { // Don't pass left of screen
                    posX -= 1;
                    Debug.Log(horizontalPushSpeed);
                    guyPhysics.AddForce(horizontalPushSpeed);
                } else {
                    posX = 0;
                }
            }
            if (direction == 3 || direction == 5 || direction == 7) {
                if (!(posX + 1 >= Screen.currentResolution.width - MAX_SCREEN_WIDTH)) { // Don't pass right of screen
                    posX += 1;
                    Debug.Log(-horizontalPushSpeed);
                    guyPhysics.AddForce(-horizontalPushSpeed);
                } else {
                    posX = Screen.currentResolution.width - MAX_SCREEN_WIDTH;
                }
            }

            // Shoot an obstacle
            if (counter % (whenToReset * whenToReset / 100) == 0) {
                foreach (GameObject obstacle in GameObject.FindGameObjectsWithTag("Obstacle")) {
                    if (Obstacle.NumberOfWalls < 1) {
                        Obstacle.SendWall(updatesPerSecond, wall);
                    }
                }
            }

            // Increase difficulty
            if (counter % 1000 == 0) {
                Time.fixedDeltaTime = 1f / ++updatesPerSecond;
            }

            // Move screen 1 pixel in some sort of direction
            SetWindowPos(hwnd, 0, posX, posY, MAX_SCREEN_WIDTH, MAX_SCREEN_HEIGHT, 32 | 64);
        }
    }

}