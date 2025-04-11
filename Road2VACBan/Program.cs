using Swed64;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Runtime.InteropServices;
using Road2VACBan;

Swed swed = new Swed("cs2");

IntPtr client = swed.GetModuleBase("client.dll");

// init imgui and overlay
Renderer renderer = new Renderer();

renderer.Start().Wait();


List<Entity> entities = new List<Entity>(); // all entities
Entity localPlayer = new Entity(); // our entity

const int HOTKEY = 0x06;

// aimbot loop

while (true)
{
    entities.Clear();
    Console.Clear();    

    IntPtr entityList = swed.ReadPointer(client, Offsets.dwEntityList);

    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    // update localplayer info
    localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
    localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iTeamNum);
    localPlayer.origin = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vOldOrigin);
    localPlayer.view = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vecViewOffset);

    // loop through entity list
    
    for(int i = 0; i < 64; i++)
    {
        if (listEntry == IntPtr.Zero) // skip if entry invalid
            continue;
        
        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);
        
        if(currentController == IntPtr.Zero) // skip if entry invalid 
            continue;

        // get pawn

        int pawnHandle = swed.ReadInt(currentController, Offsets.m_hPlayerPawn);

        if(pawnHandle == 0)
            continue;

        // apply bitmask 0x7FFF and shift bits by 9.
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);

        // triggerbot
        int entIndex = swed.ReadInt(localPlayer.pawnAddress, 0x1458);
        IntPtr forceattack = client + 0x186C850; // dwForceAttack

        // get pawn, with 1FF mask
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));

        if(currentPawn == localPlayer.pawnAddress) // if the entity is us
            continue;

        int health = swed.ReadInt(currentPawn, Offsets.m_iHealth);
        int team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
        uint lifeState = swed.ReadUInt(currentPawn, Offsets.m_lifeState);

        if (lifeState != 256)
            continue;
        if (team == localPlayer.team && !renderer.aimOnTeam)
            continue;

        Entity entity = new Entity();

        entity.pawnAddress = currentPawn;
        entity.controllerAddress = currentController;
        entity.health = health;
        entity.lifeState = lifeState;
        entity.origin = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin);
        entity.view = swed.ReadVec(currentPawn, Offsets.m_vecViewOffset);
        entity.distance = Vector3.Distance(entity.origin, localPlayer.origin);

        entities.Add(entity);

        Console.ForegroundColor = ConsoleColor.Green;

        if(team != localPlayer.team)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }

        Console.WriteLine($"{entity.health}hp, distance: {(int)(entity.distance) / 100}m");

        Console.ResetColor();

        // sort entities and aim

        entities = entities.OrderBy(o => o.distance).ToList(); // closest

        if(entities.Count > 0 && GetAsyncKeyState(HOTKEY) < 0 && renderer.aimbot) // count, hotkey and checkbox
        {
            Vector3 playerView = Vector3.Add(localPlayer.origin, localPlayer.view);
            Vector3 entityView = Vector3.Add(entities[0].origin, entities[0].view);

            Vector2 newAngles = Calculate.CalculateAngles(playerView, entityView);
            Vector3 newAnglesVec3 = new Vector3(newAngles.Y, newAngles.X, 0.0f);

            swed.WriteVec(client, Offsets.dwViewAngles, newAnglesVec3);

            // triggerbot
            if (entIndex > 0)// check if any entity is in crosshair
            {
                // shoot
                swed.WriteInt(forceattack, 65537); // +attack
                Thread.Sleep(1);
                swed.WriteInt(forceattack, 256); // -attack
            }

        }
        Thread.Sleep(20);
    }
}

[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);




// triggerbot bellow
/*
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
    
    Thread.Sleep(1);
}

//Imports
[DllImport("user32.dll")]

static extern short GetAsyncKeyState(int vKey); // handle hotkey
*/