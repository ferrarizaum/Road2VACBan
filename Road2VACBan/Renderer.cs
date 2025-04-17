using System.Collections.Concurrent;
using System.Numerics;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace sauronsring
{
    public class Renderer : Overlay
    {
        // checkbox value
        public Vector2 screenSize = new Vector2(1920, 1080);
        private ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        private Entity localPlayer = new Entity();
        private readonly object entityLock = new object();

        private bool enableESP = true;
        public bool enablename = true;
        private Vector4 enemyColor = new Vector4(1,0,0,1); // default red
        private Vector4 teamColor = new Vector4(0, 1, 0, 1); // default green
        private Vector4 boneColor = new Vector4(1, 1, 1, 1);
        private Vector4 nameColor = new Vector4(1, 1, 1, 1);

        float boneThickness = 4;

        ImDrawListPtr drawList;

        protected override void Render()
        {
            ImGui.Begin("Menu");

            ImGui.Checkbox("Enable ESP", ref enableESP);
            ImGui.Checkbox("Enable name", ref enablename);

            // team color
            if (ImGui.CollapsingHeader("Team color"))
                ImGui.ColorPicker4("##teamcolor", ref teamColor);
            // enemy color
            if (ImGui.CollapsingHeader("Enemy color"))
                ImGui.ColorPicker4("##enemycolor", ref enemyColor);
            // bone color
            if (ImGui.CollapsingHeader("Bone color"))
                ImGui.ColorPicker4("##bonecolor", ref boneColor);
            DrawOverlay(screenSize);

            drawList = ImGui.GetWindowDrawList();

            if (enableESP)
            {
                foreach(var entity in entities)
                {
                    if (EntityOnScreen(entity))
                    {
                        DrawBones(entity);
                        DrawBox(entity);
                        DrawLine(entity);
                        DrawHealthBar(entity);  
                        DrawName(entity, 20);
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

        private void DrawName(Entity entity, int yOffset)
        {
            if (enablename)
            {
                Vector2 textLocation = new Vector2(entity.viewPosition2D.X, entity.viewPosition2D.Y - yOffset);
                drawList.AddText(textLocation, ImGui.ColorConvertFloat4ToU32(nameColor), $"{entity.name}");
            }
        }

        private void DrawBones(Entity entity)
        {
            uint uintColor = ImGui.ColorConvertFloat4ToU32(boneColor);
            float currentBoneThickness = boneThickness / entity.distance;

            drawList.AddLine(entity.bones2d[1], entity.bones2d[2], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[1], entity.bones2d[3], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[1], entity.bones2d[6], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[3], entity.bones2d[4], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[6], entity.bones2d[7], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[4], entity.bones2d[5], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[7], entity.bones2d[8], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[1], entity.bones2d[0], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[0], entity.bones2d[9], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[0], entity.bones2d[11], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[9], entity.bones2d[10], uintColor, currentBoneThickness);
            drawList.AddLine(entity.bones2d[11], entity.bones2d[12], uintColor, currentBoneThickness);
            drawList.AddCircle(entity.bones2d[2], 3 + currentBoneThickness, uintColor);
        }

        private void DrawHealthBar(Entity entity)
        {
            // calculate bar height
            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;
            // get box location
            float boxLeft = entity.viewPosition2D.X - entityHeight / 3;
            float boxRight = entity.position2D.X + entityHeight / 3;
            // calculate bar width
            float barPercentWidth = 0.05f; // 5% percent width
            float barPixelWidth = barPercentWidth * (boxRight - boxLeft);
            float barHeight = entityHeight * (entity.health / 100f);

            //calculate bar rectangle, two vectors
            Vector2 barTop = new Vector2(boxLeft - barPixelWidth, entity.position2D.Y - barHeight);
            Vector2 barBottom = new Vector2(boxLeft, entity.position2D.Y);

            Vector4 barColor = new Vector4(0, 1, 0, 1);

            // draw health bar
            drawList.AddRectFilled(barTop, barBottom, ImGui.ColorConvertFloat4ToU32(barColor));
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
