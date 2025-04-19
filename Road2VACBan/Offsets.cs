namespace sauronsring
{
    public class Offsets
    {
        // offsets.cs
        public static int dwViewAngles = 0x1A933C0;
        public static int dwLocalPlayerPawn = 0x1874050;
        public static int dwEntityList = 0x1A1F730;
        public static int dwViewMatrix = 0x1A89130;

        // client.dll.cs
        public static int m_hPlayerPawn = 0x814;
        public static int m_iHealth = 0x344;
        public static int m_vOldOrigin = 0x1324;
        public static int m_iTeamNum = 0x3E3;
        public static int m_vecViewOffset = 0xCB0;
        public static int m_lifeState = 0x348;

        // triggerbot
        public static int m_iIDEntIndex = 0x1458;

        // skeleton
        public static int m_pGameSceneNode = 0x328;
        public static int m_modelState = 0x170;

        // name
        public static int m_iszPlayerName = 0x660;

        // glow
        public static int m_Glow = 0xC00;
        public static int m_bGlowing = 0x51;
        public static int m_glowColorOverride = 0x40;
        public static int m_iGlowType = 0x30;
    }
}
