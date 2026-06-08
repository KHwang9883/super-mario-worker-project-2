using Godot;
using System;

public partial class Grid : Node2D {
    [Export] public Color GridColor = new(1, 1, 1, 0.2f);
    
    private const int CellSize = 32;
    private const int GridWidth = 50;
    private const int GridHeight = 50;
    
    public override void _Draw() {
        for (int x = 0; x <= GridWidth; x++) {
            DrawLine(new Vector2(x * CellSize, 0), new Vector2(x * CellSize, GridHeight * CellSize), GridColor);
        }

        for (int y = 0; y <= GridHeight; y++) {
            DrawLine(new Vector2(0, y * CellSize), new Vector2(GridWidth * CellSize, y * CellSize), GridColor);
        }
    }
}
