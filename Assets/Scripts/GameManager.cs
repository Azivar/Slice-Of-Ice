using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(System.IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);

    public Transform guy;
    public Transform island;

    private int MAX_SCREEN_WIDTH = 96;
    private int MAX_SCREEN_HEIGHT = 96;

    public int updatesPerSecond = 40;
    public float playerSpeed = 1f;
    private int posX, posY, whenToReset, counter, direction;
    private bool moveIsland;

    private System.IntPtr hwnd;

    void Start() {
        whenToReset = Random.Range(50, 70);
        posX = Screen.currentResolution.width / 2;
        posY = Screen.currentResolution.height / 2;
        hwnd = GetActiveWindow();
        counter = 0;
        direction = 1;
        moveIsland = true;

        Time.fixedDeltaTime = 1f / updatesPerSecond;

        SetWindowPos(hwnd, 0, posX, posY, MAX_SCREEN_WIDTH, MAX_SCREEN_HEIGHT, 32 | 64);
        StartCoroutine(FreezeIsland(5f));
    }

    void Update() {
        Vector3 verticalMovementSpeed = new Vector3(0f, playerSpeed, 0f);
        Vector3 horizontalMovementSpeed = new Vector3(playerSpeed, 0f, 0f);

        // Move the guy
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
            guy.localPosition -= horizontalMovementSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
            guy.localPosition += horizontalMovementSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
            guy.localPosition += verticalMovementSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
            guy.localPosition -= verticalMovementSpeed * Time.deltaTime;
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
            whenToReset = Random.Range(updatesPerSecond*3, updatesPerSecond*4);

            StartCoroutine(FreezeIsland(0.2f)); // Wait a god damn second
        }

        if (moveIsland) {
            Vector3 verticalPushSpeed = new Vector3(0f, Time.fixedDeltaTime * 8 / MAX_SCREEN_HEIGHT, 0f);
            Vector3 horizontalPushSpeed = new Vector3(Time.fixedDeltaTime * 8 / MAX_SCREEN_WIDTH, 0f, 0f);

            // Game window moves based off of direction
            if (direction <= 3) {
                if (!(posY - 1 <= 0)) { // Don't pass bottom of screen
                    posY -= 1;
                    guy.localPosition -= verticalPushSpeed;
                } else {
                    posY = 0;
                }
            }
            if (direction >= 6) {
                if (!(posY + 1 >= Screen.currentResolution.height - MAX_SCREEN_HEIGHT)) { // Don't pass top of screen
                    posY += 1;
                    guy.localPosition += verticalPushSpeed;
                } else {
                    posY = Screen.currentResolution.height - MAX_SCREEN_HEIGHT;
                }
            }
            if (direction == 2 || direction == 4 || direction == 6) {
                if (!(posX - 1 <= 0)) { // Don't pass left of screen
                    posX -= 1;
                    guy.localPosition += horizontalPushSpeed;
                } else {
                    posX = 0;
                }
            }
            if (direction == 3 || direction == 5 || direction == 7) {
                if (!(posX + 1 >= Screen.currentResolution.width - MAX_SCREEN_WIDTH)) { // Don't pass right of screen
                    posX += 1;
                    guy.localPosition -= horizontalPushSpeed;
                } else {
                    posX = Screen.currentResolution.width - MAX_SCREEN_WIDTH;
                }
            }

            SetWindowPos(hwnd, 0, posX, posY, MAX_SCREEN_WIDTH, MAX_SCREEN_HEIGHT, 32 | 64);

            if (counter % 1000 == 0) {
                Time.fixedDeltaTime = 1f / ++updatesPerSecond;
            }
        }
    }

}