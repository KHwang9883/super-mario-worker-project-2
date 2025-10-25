using Godot;
using Godot.Collections;
using System;

public static class ShapeQueryResult {
    // Overlap 检测请使用这个类的方法
    public static Array<Node2D> ShapeQuery(CharacterBody2D body, ShapeCast2D cast) {
        var spaceState = body.GetWorld2D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters2D();
        query.Shape = cast.Shape;
        query.CollisionMask = cast.CollisionMask;
        query.CollideWithAreas = cast.CollideWithAreas;
        query.CollideWithBodies = cast.CollideWithBodies;
        query.Transform = cast.GlobalTransform;

        var results = spaceState.IntersectShape(query, cast.MaxResults);
        
        var nodes = new Array<Node2D>();
        foreach (var result in results) {
            if (result.TryGetValue("collider", out var collider)) {
                if (collider.As<Node2D>() is Node2D node) {
                    nodes.Add(node);
                }
            }
        }
        
        return nodes;
    }
}