using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleType {
    WALL, BOMB
}

public class Obstacle : MonoBehaviour {
    public static int NumberOfBombs = -1; // Because the first one doesn't count
    public static int NumberOfWalls = -1;
    public ObstacleType type = ObstacleType.WALL;

    private static bool WallSet = false;
    private static bool BombSet = false;
    public static GameObject w, b;
    private readonly float SPLASH_RADIUS = 0.25f;

    void Start() {
        if (!WallSet && this.type == ObstacleType.WALL) {
            w = this.gameObject;
            WallSet = true;
        } else if (!BombSet && this.type == ObstacleType.BOMB) {
            b = this.gameObject;
            BombSet = true;
        }
    }

    void Update() {
        if (this.type == ObstacleType.WALL && this.transform.position.magnitude >= 2) {
            Destroy(this.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (this.type == ObstacleType.WALL) {
            this.GetComponent<AudioSource>().Play();
        }
    }

    void OnEnable() {
        if (this.type == ObstacleType.WALL) {
            NumberOfWalls++;
        } else if (this.type == ObstacleType.BOMB) {
            NumberOfBombs++;
        }
    }

    public IEnumerator OnExplode() {
        this.gameObject.GetComponent<SpriteRenderer>().enabled = false;

        GameObject guy = GameObject.FindGameObjectWithTag("Player");
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        this.GetComponent<AudioSource>().Play();

        if (
            (guy.transform.position.x > this.transform.position.x - SPLASH_RADIUS && guy.transform.position.x < this.transform.position.x + SPLASH_RADIUS) &&
            (guy.transform.position.y > this.transform.position.y - SPLASH_RADIUS && guy.transform.position.y < this.transform.position.y + SPLASH_RADIUS)
        ) {
            camera.GetComponent<GameManager>().Freeze(true);
            yield return new WaitForSeconds(0.5f);
            camera.GetComponent<GameManager>().Freeze(false);
            Destroy(this.gameObject);
        } else {
            yield return new WaitForSeconds(0.35f);
            Destroy(this.gameObject);
        }
    }

    void OnDestroy() {
        if (this.type == ObstacleType.WALL) {
            NumberOfWalls--;
        } else if (this.type == ObstacleType.BOMB) {
            NumberOfBombs--;
        }
    }

    public static void SendWall(float speed) {
            GameObject wall = (GameObject)Instantiate(w);
            int randomDirection = Random.Range(0, 4);
            float randomOffset = Random.Range(-90, 90)/100f;
            wall.transform.rotation = Quaternion.identity;
            float maxHeight = Random.Range(150, Mathf.CeilToInt(speed * (speed / 4))) / 200f;
            wall.transform.localScale = new Vector2(1f, (maxHeight > 3.5f) ? 3.5f : maxHeight);

            switch (randomDirection) {
                case 0: // Left
                    wall.GetComponent<Rigidbody2D>().velocity = new Vector2(-0.01f * (speed * 1.5f), 0f);
                    wall.transform.position = new Vector2(1.5f, randomOffset);
                    break;
                case 1: // Right
                    wall.GetComponent<Rigidbody2D>().velocity = new Vector2(0.01f * (speed * 1.5f), 0f);
                    wall.transform.position = new Vector2(-1.5f, randomOffset);
                    break;
                case 2: // Down
                    wall.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, -0.01f * (speed * 1.5f));
                    wall.transform.Rotate(0f, 0f, 90f);
                    wall.transform.position = new Vector2(randomOffset, 1.5f);
                    break;
                default: // Up
                    wall.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0.01f * (speed * 1.5f));
                    wall.transform.Rotate(0f, 0f, 90f);
                    wall.transform.position = new Vector2(randomOffset, -1.5f);
                    break;
            }
    
            wall.name = "Wall" + NumberOfWalls;
    }

    public static void SendBomb() {
        GameObject bomb = (GameObject)Instantiate(b);
        bomb.transform.position = new Vector2(Random.Range(-50, 50) / 100f, Random.Range(-50, 50) / 100f);
        bomb.name = "Bomb" + NumberOfBombs;
        bomb.gameObject.GetComponent<Animator>().enabled = true;
        bomb.gameObject.GetComponent<Animator>().Play("IceBombAnimation");
    }
}
