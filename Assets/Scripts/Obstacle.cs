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

    void Update() {
        if (type == ObstacleType.WALL && this.transform.position.magnitude >= 2) {
            Destroy(this.gameObject);
        } /* else if (type == ObstacleType.BOMB && !this.GetComponent<Animation>().IsPlaying("TheActualIceBombAnimation")) {
            Destroy(this.gameObject);
        } */
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (this.type == ObstacleType.WALL) {
            this.GetComponent<AudioSource>().Play();
        }
    }

    void OnEnable() {
        if (type == ObstacleType.WALL) {
            NumberOfWalls++;
        } else if (type == ObstacleType.BOMB) {
            NumberOfBombs++;
        }
    }

    void OnDestroy() {
        if (type == ObstacleType.WALL) {
            NumberOfWalls--;
        } else if (type == ObstacleType.BOMB) {
            NumberOfBombs--;
        }
    }

    public static void SendWall(float s, GameObject w) {
            GameObject wall = (GameObject)Instantiate(w);
            int randomDirection = Random.Range(0, 4);
            float randomOffset = Random.Range(-90, 90)/100f;
            Vector2 sizeMultiplier = new Vector2(1f, Random.Range(100, Mathf.CeilToInt(s * 5)) / 100f);
            Vector2 direction, position;
            Quaternion rotation;

            switch (randomDirection) {
                case 0: // Left
                    direction = new Vector2(-0.01f * s, 0f);
                    rotation = new Quaternion(0f, 0f, 0f, 0f);
                    position = new Vector2(1.5f, randomOffset);
                    break;
                case 1: // Right
                    direction = new Vector2(0.01f * s, 0f);
                    rotation = new Quaternion(0f, 0f, 0f, 0f);
                    position = new Vector2(-1.5f, randomOffset);
                    break;
                case 2: // Down
                    direction = new Vector2(0f, -0.01f * s);
                    rotation = new Quaternion(0f, 0f, 90f, 0f);
                    position = new Vector2(randomOffset, 1.5f);
                    break;
                default: // Up
                    direction = new Vector2(0f, 0.01f * s);
                    rotation = new Quaternion(0f, 0f, 90f, 0f);
                    position = new Vector2(randomOffset, -1.5f);
                    break;
            }

            wall.transform.localScale = sizeMultiplier;
            wall.GetComponent<Rigidbody2D>().velocity = direction;
            wall.transform.SetPositionAndRotation(position, rotation);
            wall.name = "Wall" + NumberOfWalls;
    }

    public static void SendBomb(GameObject b) {
        GameObject bomb = (GameObject)Instantiate(b);
        bomb.transform.position = new Vector2(Random.Range(-80, 80) / 100f, Random.Range(-80, 80) / 100f);
        bomb.name = "Bomb" + NumberOfWalls;
        
    }
}
