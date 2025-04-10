using Swed64;
using System.Runtime.InteropServices;

// memory stuff

Swed swed = new Swed("cs2");

// get client.dll base
IntPtr client = swed.GetModuleBase("client.dll");

// force attacks

IntPtr forceattack = client + 0x186C850; // dwForceAttack

int active = 0;

// trigger bot loop

while (true)
{
    Console.Clear();

    // Get updated Values

    IntPtr localPlayerPawn = swed.ReadPointer(client, 0x1874050); // dwLocalPlayerPawn
    int entIndex = swed.ReadInt(localPlayerPawn, 0x1458); // m_iIDEntIndex
    Console.WriteLine(entIndex);
    Console.WriteLine($"Crosshair/Entity ID: {entIndex}");
    Console.WriteLine($"key: {GetAsyncKeyState(0x6)}");

    if(GetAsyncKeyState(0x6) < 0)
    {
        if(active == 1)
        {
            active = 0;
        }
        else 
        { 
            active = 1; 
        }
    }

    if (active == 1)
    {
        if (entIndex > 0)// check if any entity is in crosshair
        {
            // shoot
            swed.WriteInt(forceattack, 65537); // +attack
            Thread.Sleep(1);
            swed.WriteInt(forceattack, 256); // -attack
        }
    }
    /*

    if (GetAsyncKeyState(0x6) < 0)// hotkey to be used
    {
        if(entIndex > 0)// check if any entity is in crosshair
        {
            // shoot
            swed.WriteInt(forceattack, 65537); // +attack
            Thread.Sleep(1);
            swed.WriteInt(forceattack, 256); // -attack
        }
    }
    */
    Thread.Sleep(1);
}

//Imports
[DllImport("user32.dll")]

static extern short GetAsyncKeyState(int vKey); // handle hotkey