using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    [DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern System.IntPtr SetActiveWindow(System.IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(System.IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);

    private GameObject guy, island, gameOverScreen;
    private Rigidbody2D guyPhysics;
    private Vector3 guySize, islandSize;

    private int MAX_SCREEN_WIDTH = 96;
    private int MAX_SCREEN_HEIGHT = 96;

    public GameObject wall, bomb;
    public float playerAcceleration = 0.3f;
    public float maxSpeed = 0.8f;
    public float iceFriction = 0.03f;
    public float screenPushForce = 60f;

    public Sprite[] sprites;
    public Sprite[] frozenSprites;
    public int currentSpriteIndex;
    public AudioClip fallSound;
    public AudioClip earthquake;

    private int posX, posY, whenToReset, points, direction, updatesPerSecond;
    private bool moveIsland, playerDied;

    private System.IntPtr hwnd;

    void Start() {
        posX = (Screen.currentResolution.width) / 2 - (MAX_SCREEN_WIDTH / 2);
        posY = (Screen.currentResolution.height) / 2 - (MAX_SCREEN_HEIGHT / 2);
        hwnd = GetActiveWindow();
        
        guy = GameObject.FindGameObjectWithTag("Player");
        guyPhysics = guy.GetComponent<Rigidbody2D>();
        guySize = guy.GetComponent<SpriteRenderer>().bounds.size;

        island = GameObject.FindGameObjectWithTag("Island");
        islandSize = island.GetComponent<SpriteRenderer>().bounds.size;

        gameOverScreen = GameObject.FindGameObjectWithTag("GameOver");

        Restart();
    }

    void Update() {
        if (!playerDied) {
            Vector2 verticalMovementSpeed = new Vector2(0f, playerAcceleration);
            Vector2 horizontalMovementSpeed = new Vector2(playerAcceleration, 0f);

            // Update direction penguin is facing
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
                currentSpriteIndex = 1;
            } else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
                currentSpriteIndex = 7;
            } else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
                currentSpriteIndex = 3;
            } else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
                currentSpriteIndex = 5;
            }
            guy.GetComponent<SpriteRenderer>().sprite = sprites[currentSpriteIndex];

            // Move the guy
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
                guyPhysics.velocity += (guyPhysics.velocity.x > -maxSpeed) ? -horizontalMovementSpeed : Vector2.zero;
            } if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
                guyPhysics.velocity += (guyPhysics.velocity.x < maxSpeed) ? horizontalMovementSpeed : Vector2.zero;
            } if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
                guyPhysics.velocity += (guyPhysics.velocity.y < maxSpeed) ? verticalMovementSpeed : Vector2.zero;
            } if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
                guyPhysics.velocity += (guyPhysics.velocity.y > -maxSpeed) ? -verticalMovementSpeed : Vector2.zero;
            }

            // Check to see if you are falling off the ledge
            if (
                (guy.transform.position.x >= islandSize.x / 2) ||
                (guy.transform.position.x <= -islandSize.x / 2) ||
                (guy.transform.position.y >= islandSize.y / 2) ||
                (guy.transform.position.y <= -islandSize.y / 2)
            ) {
                StartCoroutine(GameOver());
            }
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

    private IEnumerator GameOver() {
        Vector2 pushDirection;
        int x = 0;

        // Do not get pushed by walls or bombs
        foreach (GameObject obstacle in GameObject.FindGameObjectsWithTag("Obstacle")) {
            if (obstacle.TryGetComponent(out Collider2D c2d)) {
                Physics2D.IgnoreCollision(guy.GetComponent<Collider2D>(), obstacle.GetComponent<Collider2D>());
            }
        }

        // Stop game and stop dude
        guyPhysics.velocity = Vector2.zero;
        playerDied = true;
        moveIsland = false;
        

        // What side of screen did he fall off of? Rotate and push him that direction
        if (guy.transform.position.x >= islandSize.x / 2) {
            guy.transform.Rotate(0f, 0f, -10f);
            pushDirection = new Vector2(0.8f, 0f);
        } else if (guy.transform.position.x <= -islandSize.x / 2) {
            guy.transform.Rotate(0f, 0f, 10f);
            pushDirection = new Vector2(-0.8f, 0f);
        } else if (guy.transform.position.y >= islandSize.y / 2) {
            guy.transform.Rotate(0f, 0f, -10f);
            pushDirection = new Vector2(0f, 0.8f);
        } else {
            guy.transform.Rotate(0f, 0f, 10f);
            pushDirection = new Vector2(0f, -0.8f);
        }

        this.GetComponent<AudioSource>().clip = fallSound;
        this.GetComponent<AudioSource>().Play();

        // Wiggle wings 6 times - 2.4 seconds of wing flapping
        while (x++ < 6) {
            yield return new WaitForSeconds(0.2f);
            guy.GetComponent<SpriteRenderer>().sprite = sprites[currentSpriteIndex - 1];
            yield return new WaitForSeconds(0.2f);
            guy.GetComponent<SpriteRenderer>().sprite = sprites[currentSpriteIndex];
        }

        guy.GetComponent<SpriteRenderer>().sprite = sprites[5];
        guyPhysics.velocity = pushDirection; // Push

        // 1.2 seconds of falling and shrinking to nothing
        while (x++ < 12) {
            guy.transform.localScale = guy.transform.localScale / 1.5f;
            yield return new WaitForSeconds(0.2f);
        }
        guy.transform.localScale = new Vector2(0f, 0f);

        // GameOver splash screen
        gameOverScreen.SetActive(true);
        GameObject.FindGameObjectWithTag("Score").GetComponent<Text>().text = System.Convert.ToString(points / 10);
    }

    private IEnumerator FreezeIsland(float waitTime) {
        moveIsland = false;
        yield return new WaitForSeconds(waitTime);
        if (!playerDied) {
            moveIsland = true;
        }
    }

    private IEnumerator PlayIntro(float playTime) {
        int x = 0;
        yield return new WaitForSeconds(0.5f);
        this.GetComponent<AudioSource>().clip = earthquake;
        this.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(1.5f);
        int iterations = 8;

        while (x++ < iterations * (playTime - 2)) {
            if (x % 2 == 0) 
                SetWindowPos(hwnd, 0, posX + Random.Range(1, 5), posY + Random.Range(-1, 2), MAX_SCREEN_WIDTH, MAX_SCREEN_HEIGHT, 32 | 64);
            else
                SetWindowPos(hwnd, 0, posX - Random.Range(1, 5), posY + Random.Range(-1, 2), MAX_SCREEN_WIDTH, MAX_SCREEN_HEIGHT, 32 | 64);
            yield return new WaitForSeconds(1f / iterations);
        }
    }

    // Fixed is a lie
    void FixedUpdate() {
        // New random direction set at a random time
        if (moveIsland && ++points % whenToReset == 0) {
            direction = Random.Range(0, 8);
            whenToReset = Random.Range(updatesPerSecond*3, updatesPerSecond*10);

            Debug.Log("direction: " + direction + "    resetvalue: " + whenToReset);

            StartCoroutine(FreezeIsland(0.2f)); // Wait a god damn second
        }

        else if (moveIsland) {
            Vector3 verticalPushSpeed = new Vector3(0f, Time.fixedDeltaTime * screenPushForce, 0f);
            Vector3 horizontalPushSpeed = new Vector3(Time.fixedDeltaTime * screenPushForce, 0f, 0f);

            // Game window moves based off of direction
            if (direction <= 2) {
                if (!(posY - 1 <= 0)) { // Don't pass bottom of screen
                    posY -= 1;
                    if (guyPhysics.velocity.y > -maxSpeed)
                        guyPhysics.AddForce(-verticalPushSpeed);
                } else {
                    posY = 0;
                }
            }
            if (direction >= 5) {
                if (!(posY + 1 >= Screen.currentResolution.height - MAX_SCREEN_HEIGHT)) { // Don't pass top of screen
                    posY += 1;
                    if (guyPhysics.velocity.y < maxSpeed)
                        guyPhysics.AddForce(verticalPushSpeed);
                } else {
                    posY = Screen.currentResolution.height - MAX_SCREEN_HEIGHT;
                }
            }
            if (direction == 1 || direction == 3 || direction == 5) {
                if (!(posX - 1 <= 0)) { // Don't pass left of screen
                    posX -= 1;
                    if (guyPhysics.velocity.x < maxSpeed)
                        guyPhysics.AddForce(horizontalPushSpeed);
                } else {
                    posX = 0;
                }
            }
            if (direction == 2 || direction == 4 || direction == 6) {
                if (!(posX + 1 >= Screen.currentResolution.width - MAX_SCREEN_WIDTH)) { // Don't pass right of screen
                    posX += 1;
                    if (guyPhysics.velocity.x > -maxSpeed)
                        guyPhysics.AddForce(-horizontalPushSpeed);
                } else {
                    posX = Screen.currentResolution.width - MAX_SCREEN_WIDTH;
                }
            }

            // Shoot an obstacle
            if (points % Random.Range(200 - updatesPerSecond, 250 - updatesPerSecond) == 0) {
                if (Obstacle.NumberOfWalls < 10) {
                    Obstacle.SendWall(updatesPerSecond, wall);
                }
            } /* else if (points % Random.Range(200 - updatesPerSecond, 250 - updatesPerSecond) == 0) {
                if (Obstacle.NumberOfBombs < 10) {
                    Obstacle.SendBomb(bomb);
                }
            } */

            // Increase difficulty
            if (points % 200 == 0) {
                Time.fixedDeltaTime = 1f / ++updatesPerSecond;
            }

            // Move screen 1 pixel in some sort of direction
            SetWindowPos(hwnd, 0, posX, posY, MAX_SCREEN_WIDTH, MAX_SCREEN_HEIGHT, 32 | 64);
        }
    }

    public void Restart() {
        SetWindowPos(hwnd, 0, posX, posY, MAX_SCREEN_WIDTH, MAX_SCREEN_HEIGHT, 32 | 64);

        gameOverScreen.SetActive(false);
        SetActiveWindow(hwnd);
        currentSpriteIndex = 5;

        guyPhysics.transform.SetPositionAndRotation(Vector2.zero, new Quaternion(0f, 0f, 0f, 0f));
        guy.transform.localScale = new Vector2(1f, 1f);
        guy.GetComponent<SpriteRenderer>().sprite = sprites[currentSpriteIndex];
        updatesPerSecond = 40;
        points = 0;
        direction = 1;
        moveIsland = true;
        playerDied = false;
        whenToReset = Random.Range(50, 70);
        Time.fixedDeltaTime = 1f / updatesPerSecond;

        StartCoroutine(PlayIntro(5f));
        StartCoroutine(FreezeIsland(5f));
    }
}