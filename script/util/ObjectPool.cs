using System;
using System.Collections.Generic;
using Godot;
using SMWP.Level.Interface;

namespace SMWP.Util;

public class ObjectPool {
    /*public Queue<Node> FreePool = new();
    public Node Create(PackedScene scene) {
        if (FreePool.Count == 0) {
            var node = scene.Instantiate();
            return node;
        } else {
            return FreePool.Dequeue();
        }
    }
    public void AddFree(Node node) {
        FreePool.Enqueue(node);
    }*/
}
