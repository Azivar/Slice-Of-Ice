using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class IslandController : MonoBehaviour
{

    // Define function signatures to import from Windows APIs

    [DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(System.IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);


    public int islandspeed = 1;
    private int direction;
    private int posX = 200;
    private int posY = 200;
    private int frames = 0;


    
    





    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    {
        // This is used to move the screen 
        var hwnd = GetActiveWindow();
        SetWindowPos(hwnd, 0, posX, posY, 96, 96, 32 | 64);

       

        // Get a random Direction every 10 frames
        frames++;
        if (frames % 30 == 0)
        {
            direction = Random.Range(0, 8);
        }

        //to move the window based off the random direction

        if (direction == 1)
        {
            posY -= islandspeed;
        }
        else if (direction == 2)
        {
            posY -= islandspeed;
            posX += islandspeed;
        }
        else if (direction == 3)
        {
            posX += islandspeed;
        }
        else if (direction == 4)
        {
            posY += islandspeed;
            posX += islandspeed;
        }
        else if (direction == 5)
        {
            posY += islandspeed;
        }
        else if (direction == 6)
        {
            posY += islandspeed;
            posX -= islandspeed;
        }
        else if (direction == 7)
        {
            posX -= islandspeed;
        }
        else if (direction ==8)
        {
            posY -= islandspeed;
            posX -= islandspeed;
        }
        else
        {
            
        }
    }
}
