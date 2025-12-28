using Godot;
using System;

public partial class PerpetualMotionMachineMarker : StaticBody2D {
    public enum PerpetualMotionMachineMarkerTypeEnum {
        Left,
        Right,
        SemiSolid,
    }
    public PerpetualMotionMachineMarkerTypeEnum PerpetualMotionMachineMarkerType;
}
