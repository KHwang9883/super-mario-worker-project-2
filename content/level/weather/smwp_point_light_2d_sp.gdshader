// NOTE: Shader automatically converted from Godot Engine 4.5.stable.mono's CanvasItemMaterial.

shader_type canvas_item;
#define MAX_LIGHTS 200
render_mode blend_add;

uniform int light_count = MAX_LIGHTS;

/*void fragment() {
    COLOR = vec4(1.0, 1.0, 1.0, 0.9); // 强制渲染为纯色
}*/