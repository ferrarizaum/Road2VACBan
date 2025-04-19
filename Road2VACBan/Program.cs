using Swed64;
using System.Numerics;
using sauronsring;
using System;
using System.Runtime.InteropServices;
using ImGuiNET;

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

        IntPtr sceneNode = swed.ReadPointer(currentPawn, Offsets.m_pGameSceneNode);
        IntPtr boneMatrix = swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80);

        Entity entity = new Entity();

        // Write glow enabled (uint32_t)
        IntPtr glowTypeAddress = IntPtr.Add(currentPawn, Offsets.m_Glow + Offsets.m_iGlowType);
        IntPtr glowColorAddress = IntPtr.Add(currentPawn, Offsets.m_Glow + Offsets.m_glowColorOverride);
        IntPtr glowEnabledAddress = IntPtr.Add(currentPawn, Offsets.m_Glow + Offsets.m_bGlowing);

        //bool success3 = Memory.WriteMemory(glowEnabledAddress, (uint)1);

        // Check results
        //Console.WriteLine($"Glow Type Write: {(success1 ? "Success" : "Failed")}");
        //Console.WriteLine($"Glow Color Write: {(success2 ? "Success" : "Failed")}");
        Console.WriteLine($"Glow Type Address: ${glowTypeAddress}");
        Console.WriteLine($"Glow Color Address: ${glowColorAddress}");
        Console.WriteLine($"Glow Enabled Address: ${glowEnabledAddress}");

        // bool sucess1 = swed.WriteUInt(glowTypeAddress, 1);
        Vector4 nameColor = new Vector4(1, 1, 1, 1);
        bool sucess2 = swed.WriteFloat(glowColorAddress, 0xFFFF0000);
        bool sucess3 = swed.WriteInt(glowEnabledAddress, 1);

        //Console.WriteLine($"Glow Enabled Write: {(success3 ? "Success" : "Failed")}");
        // Console.WriteLine($"Glow Type Write: {(sucess1 ? "Success" : "Failed")}");
        Console.WriteLine($"Glow Color Write: {(sucess2 ? "Success" : "Failed")}");
        Console.WriteLine($"Glow Enabled Write: {(sucess3 ? "Success" : "Failed")}");


        entity.team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
        entity.health = swed.ReadInt(currentPawn, Offsets.m_iHealth);
        entity.position = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin);
        entity.viewOffset = swed.ReadVec(currentPawn, Offsets.m_vecViewOffset);
        entity.position2D = Calculate.WorldToScreen(viewMatrix, entity.position, screenSize);
        entity.viewPosition2D = Calculate.WorldToScreen(viewMatrix,
            Vector3.Add(entity.position, entity.viewOffset), screenSize);
        entity.distance = Vector3.Distance(entity.position, localPlayer.position);
        entity.bones = Calculate.ReadBones(boneMatrix, swed);
        entity.bones2d = Calculate.ReadBones2d(entity.bones, viewMatrix, screenSize);
        entity.name = swed.ReadString(currentController, Offsets.m_iszPlayerName, 16).Split("\0")[0];

        entities.Add(entity);       
    }
    renderer.UpdateLocalPlayer(localPlayer);
    renderer.UpdateEntities(entities);
    Thread.Sleep(10);
}
