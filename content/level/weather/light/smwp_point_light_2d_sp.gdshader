// NOTE: Shader automatically converted from Godot Engine 4.5.stable.mono's CanvasItemMaterial.

shader_type canvas_item;
#define MAX_LIGHTS 200
render_mode blend_sub;

//uniform int light_count = MAX_LIGHTS;
uniform float scale = 1.0;

/*void fragment() {
    COLOR = vec4(1.0, 1.0, 1.0, 0.9); // 强制渲染为纯色
}*/

void fragment() {
    // 当scale=1时，这就是原封不动的显示
    vec2 scaled_uv = (UV - 0.5) / scale + 0.5;
    COLOR = texture(TEXTURE, scaled_uv);
}