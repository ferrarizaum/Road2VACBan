using Swed64;
using System.Numerics;
using sauronsring;

Swed swed = new Swed("cs2");

IntPtr client = swed.GetModuleBase("client.dll");

// init imgui and overlay
Renderer renderer = new Renderer();
Thread renderThread = new Thread(new ThreadStart(renderer.Start().Wait));
renderThread.Start();

Vector2 screenSize = renderer.screenSize;

List<Entity> entities = new List<Entity>(); // all entities
Entity localPlayer = new Entity(); // our entity

// const int HOTKEY = 0x06;

while (true)
{
    entities.Clear();
    Console.Clear();

    IntPtr entityList = swed.ReadPointer(client, Offsets.dwEntityList);

    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    // update localplayer info
    localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
    localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iTeamNum);

    // loop through entity list

    for (int i = 0; i < 64; i++)
    {
        /*
        if (listEntry == IntPtr.Zero) // skip if entry invalid
            continue;
        */

        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);

        if (currentController == IntPtr.Zero) // skip if entry invalid 
            continue;

        int pawnHandle = swed.ReadInt(currentController, Offsets.m_hPlayerPawn);
        if (pawnHandle == 0)
            continue;

        // apply bitmask 0x7FFF and shift bits by 9.
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);

        if (listEntry2 == IntPtr.Zero)
            continue;

        // get pawn, with 1FF mask
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));
        
        if(currentPawn == localPlayer.pawnAddress) // if the entity is us
            continue;
        
        if (currentPawn == IntPtr.Zero)
            continue;

        int health = swed.ReadInt(currentPawn, Offsets.m_iHealth);
        int team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
        uint lifeState = swed.ReadUInt(currentPawn, Offsets.m_lifeState);

        if (lifeState != 256)
            continue;

        float[] viewMatrix = swed.ReadMatrix(client + Offsets.dwViewMatrix);

        Entity entity = new Entity();

        entity.team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
        entity.health = swed.ReadInt(currentPawn, Offsets.m_iHealth);
        entity.position = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin);
        entity.viewOffset = swed.ReadVec(currentPawn, Offsets.m_vecViewOffset);
        entity.position2D = Calculate.WorldToScreen(viewMatrix, entity.position, screenSize);
        entity.viewPosition2D = Calculate.WorldToScreen(viewMatrix,
            Vector3.Add(entity.position, entity.viewOffset), screenSize);

        entities.Add(entity);       
    }
    renderer.UpdateLocalPlayer(localPlayer);
    renderer.UpdateEntities(entities);
}
