using System.Collections.Concurrent;
using System.Numerics;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace sauronsring
{
    public class Renderer : Overlay
    {
        // checkbox value
        public bool aimbot = true;
        public bool aimOnTeam = false;
        public Vector2 screenSize = new Vector2(1920, 1080);
        private ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        private Entity localPlayer = new Entity();
        private readonly object entityLock = new object();

        private bool enableESP = true;
        private Vector4 enemyColor = new Vector4(1,0,0,1); // default red
        private Vector4 teamColor = new Vector4(0, 1, 0, 1); // default green

        ImDrawListPtr drawList;

        protected override void Render()
        {
            ImGui.Begin("Menu");

            ImGui.Checkbox("aimbot", ref aimbot);

            ImGui.Checkbox("aim on teammates, aswell", ref aimOnTeam);

            ImGui.Checkbox("Enable ESP", ref enableESP);

            // team color
            if (ImGui.CollapsingHeader("Team color"))
                ImGui.ColorPicker4("##teamcolor", ref teamColor);
            // enemy color
            if (ImGui.CollapsingHeader("Enemy color"))
                ImGui.ColorPicker4("##enemycolor", ref enemyColor);

            DrawOverlay(screenSize);

            drawList = ImGui.GetWindowDrawList();

            if (enableESP)
            {
                foreach(var entity in entities)
                {
                    if (EntityOnScreen(entity))
                    {
                        DrawBox(entity);
                        DrawLine(entity);
                    }
                }
            }
        }

        bool EntityOnScreen(Entity entity)
        {
           // causing entities to disappear, commented out while no better fix
            /*
            if(entity.position.X > 0 
                && entity.position2D.X < screenSize.X 
                && entity.position2D.Y > 0 
                && entity.position2D.Y < screenSize.Y) 
            {
                return true;
            }
            return false;
            */
            return true;
        }

        private void DrawBox(Entity entity)
        {
            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;

            Vector2 rectTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3,
                entity.viewPosition2D.Y);

            Vector2 rectBottom = new Vector2(entity.position2D.X + entityHeight / 3,
                entity.position2D.Y);

            Vector4 boxColor = localPlayer.team == entity.team ? teamColor : enemyColor;

            drawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));
        }

        private void DrawLine(Entity entity)
        {
            Vector4 lineColor = localPlayer.team == entity.team ? teamColor : enemyColor;
            drawList.AddLine(new Vector2(screenSize.X / 2, 
                screenSize.Y), 
                entity.position2D, 
                ImGui.ColorConvertFloat4ToU32(lineColor));
        }

        public void UpdateEntities(IEnumerable<Entity> newEntities)
        {
            entities = new ConcurrentQueue<Entity>(newEntities);
        }

        public void UpdateLocalPlayer(Entity newEntity)
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        }

        void DrawOverlay(Vector2 screenSize)
        {
            ImGui.SetNextWindowSize(screenSize);
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );
        }
    }
}
