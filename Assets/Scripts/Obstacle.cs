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
        if (this.transform.position.magnitude >= 2) {
            Destroy(this.gameObject);
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
            Vector3 direction, position;
            Quaternion rotation;

            switch (randomDirection) {
                case 0: // Left
                    direction = new Vector2(-0.01f * s, 0f);
                    rotation = new Quaternion(0f, 0f, 0f, 0f);
                    position = new Vector2(1.5f, 0f);
                    break;
                case 1: // Right
                    direction = new Vector2(0.01f * s, 0f);
                    rotation = new Quaternion(0f, 0f, 0f, 0f);
                    position = new Vector2(-1.5f, 0f);
                    break;
                case 2: // Down
                    direction = new Vector2(0f, -0.01f * s);
                    rotation = new Quaternion(0f, 0f, 90f, 0f);
                    position = new Vector2(0f, 1.5f);
                    break;
                default: // Up
                    direction = new Vector2(0f, 0.01f * s);
                    rotation = new Quaternion(0f, 0f, 90f, 0f);
                    position = new Vector2(0f, -1.5f);
                    break;
            }
            
            wall.GetComponent<Rigidbody2D>().velocity = direction;
            wall.transform.SetPositionAndRotation(position, rotation);
            wall.name = "Wall" + NumberOfWalls;
    }
}
