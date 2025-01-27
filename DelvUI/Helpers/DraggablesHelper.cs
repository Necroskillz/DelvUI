﻿using DelvUI.Config;
using DelvUI.Interface;
using DelvUI.Interface.GeneralElements;
using DelvUI.Interface.Jobs;
using DelvUI.Interface.StatusEffects;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DelvUI.Helpers
{
    public static class DraggablesHelper
    {
        public static void DrawGrid(GridConfig config, HUDOptionsConfig? hudConfig, DraggableHudElement? selectedElement)
        {
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);

            ImGui.SetNextWindowBgAlpha(config.BackgroundAlpha);

            ImGui.Begin("DelvUI_grid",
                ImGuiWindowFlags.NoTitleBar
              | ImGuiWindowFlags.NoScrollbar
              | ImGuiWindowFlags.AlwaysAutoResize
              | ImGuiWindowFlags.NoInputs
              | ImGuiWindowFlags.NoDecoration
              | ImGuiWindowFlags.NoBringToFrontOnFocus
              | ImGuiWindowFlags.NoFocusOnAppearing
            );

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            Vector2 screenSize = ImGui.GetMainViewport().Size;
            Vector2 offset = hudConfig != null && hudConfig.UseGlobalHudShift ? hudConfig.HudOffset : Vector2.Zero;
            Vector2 center = screenSize / 2f + offset;

            // grid
            if (config.ShowGrid)
            {
                int count = (int)(Math.Max(screenSize.X, screenSize.Y) / config.GridDivisionsDistance) / 2 + 1;

                for (int i = 0; i < count; i++)
                {
                    var step = i * config.GridDivisionsDistance;

                    drawList.AddLine(new Vector2(center.X + step, 0), new Vector2(center.X + step, screenSize.Y), 0x88888888);
                    drawList.AddLine(new Vector2(center.X - step, 0), new Vector2(center.X - step, screenSize.Y), 0x88888888);

                    drawList.AddLine(new Vector2(0, center.Y + step), new Vector2(screenSize.X, center.Y + step), 0x88888888);
                    drawList.AddLine(new Vector2(0, center.Y - step), new Vector2(screenSize.X, center.Y - step), 0x88888888);

                    if (config.GridSubdivisionCount > 1)
                    {
                        for (int j = 1; j < config.GridSubdivisionCount; j++)
                        {
                            var subStep = j * (config.GridDivisionsDistance / config.GridSubdivisionCount);

                            drawList.AddLine(new Vector2(center.X + step + subStep, 0), new Vector2(center.X + step + subStep, screenSize.Y), 0x44888888);
                            drawList.AddLine(new Vector2(center.X - step - subStep, 0), new Vector2(center.X - step - subStep, screenSize.Y), 0x44888888);

                            drawList.AddLine(new Vector2(0, center.Y + step + subStep), new Vector2(screenSize.X, center.Y + step + subStep), 0x44888888);
                            drawList.AddLine(new Vector2(0, center.Y - step - subStep), new Vector2(screenSize.X, center.Y - step - subStep), 0x44888888);
                        }
                    }
                }
            }

            // center lines
            if (config.ShowCenterLines)
            {
                drawList.AddLine(new Vector2(center.X, 0), new Vector2(center.X, screenSize.Y), 0xAAFFFFFF);
                drawList.AddLine(new Vector2(0, center.Y), new Vector2(screenSize.X, center.Y), 0xAAFFFFFF);
            }

            if (config.ShowAnchorPoints && selectedElement != null)
            {
                Vector2 parentAnchorPos = center + selectedElement.ParentPos();
                Vector2 anchorPos = parentAnchorPos + selectedElement.GetConfig().Position;

                drawList.AddLine(parentAnchorPos, anchorPos, 0xAA0000FF, 2);

                var anchorSize = new Vector2(10, 10);
                drawList.AddRectFilled(anchorPos - anchorSize / 2f, anchorPos + anchorSize / 2f, 0xAA0000FF);
            }

            ImGui.End();
        }

        public static void DrawElements(
            Vector2 origin,
            HudHelper hudHelper,
            List<DraggableHudElement> elements,
            JobHud? jobHud,
            DraggableHudElement? selectedElement)
        {
            bool canTakeInput = true;

            // selected
            if (selectedElement != null && !hudHelper.IsElementHidden(selectedElement))
            {
                selectedElement.CanTakeInputForDrag = true;
                selectedElement.Draw(origin);
                canTakeInput = !selectedElement.NeedsInputForDrag;
            }

            // all
            foreach (DraggableHudElement element in elements)
            {
                if (element == selectedElement || hudHelper.IsElementHidden(element)) { continue; }

                element.CanTakeInputForDrag = canTakeInput;
                element.Draw(origin);
                canTakeInput = !canTakeInput ? false : !element.NeedsInputForDrag;
            }

            // job hud
            if (jobHud != null && jobHud != selectedElement && !hudHelper.IsElementHidden(jobHud))
            {
                jobHud.CanTakeInputForDrag = canTakeInput;
                jobHud.Draw(origin);
            }
        }

        public static bool DrawArrows(Vector2 position, Vector2 size, string tooltipText, out Vector2 offset)
        {
            offset = Vector2.Zero;

            var windowFlags = ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoDecoration;

            var margin = new Vector2(4, 0);
            var windowSize = ArrowSize + margin * 2;

            // left, right, up, down
            var positions = GetArrowPositions(position, size);
            var offsets = new Vector2[]
            {
                new Vector2(-1, 0),
                new Vector2(1, 0),
                new Vector2(0, -1),
                new Vector2(0, 1)
            };

            for (int i = 0; i < 4; i++)
            {
                var pos = positions[i] - margin;

                ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);
                ImGui.SetNextWindowPos(pos);

                ImGui.Begin("DelvUI_draggablesArrow " + i.ToString(), windowFlags);

                // fake button
                ImGui.ArrowButton("arrow button " + i.ToString(), (ImGuiDir)i);

                if (ImGui.IsMouseHoveringRect(pos, pos + windowSize))
                {
                    // track click manually to not deal with window focus stuff
                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    {
                        offset = offsets[i];
                    }

                    // tooltip
                    TooltipsHelper.Instance.ShowTooltipOnCursor(tooltipText);
                }

                ImGui.End();
            }

            return offset != Vector2.Zero;
        }

        public static Vector2 ArrowSize = new Vector2(40, 40);

        public static Vector2[] GetArrowPositions(Vector2 position, Vector2 size)
        {
            return GetArrowPositions(position, size, ArrowSize);
        }

        public static Vector2[] GetArrowPositions(Vector2 position, Vector2 size, Vector2 arrowSize)
        {
            return new Vector2[]
            {
                new Vector2(position.X - arrowSize.X + 10, position.Y + size.Y / 2f - arrowSize.Y / 2f - 2),
                new Vector2(position.X + size.X - 8, position.Y + size.Y / 2f - arrowSize.Y / 2f - 2),
                new Vector2(position.X + size.X / 2f - arrowSize.X / 2f + 2, position.Y - arrowSize.Y + 1),
                new Vector2(position.X + size.X / 2f - arrowSize.X / 2f + 2, position.Y + size.Y - 7)
            };
        }

        public static void DrawGridWindow()
        {
            var configManager = ConfigurationManager.Instance;
            var node = configManager.GetConfigPageNode<GridConfig>();
            if (node == null)
            {
                return;
            }

            GridConfig config = (GridConfig)node.ConfigObject;

            ImGui.SetNextWindowSize(new Vector2(340, 300), ImGuiCond.Appearing);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(10f / 255f, 10f / 255f, 10f / 255f, 0.95f));

            if (!ImGui.Begin("Grid", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.End();
                return;
            }

            ImGui.PushItemWidth(150);
            var changed = false;
            node.Draw(ref changed);

            ImGui.SetCursorPos(new Vector2(8, 260));

            if (ImGui.Button("Lock HUD", new Vector2(ImGui.GetWindowContentRegionWidth(), 30)))
            {
                changed = true;
                config.Enabled = false;
                configManager.LockHUD = true;
            }

            if (changed)
            {
                configManager.SaveConfigurations();
            }

            ImGui.End();
            ImGui.PopStyleColor();
        }
    }
}
