// 代码由 DeepSeek 完成

shader_type canvas_item;

uniform float corner_radius : hint_range(0, 0.5) = 0.1;
uniform bool antialiasing = true;

void fragment() {
    // 获取纹理坐标
    vec2 uv = UV;
    
    // 计算四个角的圆形区域
    vec2 corner_center1 = vec2(corner_radius, corner_radius); // 左下角
    vec2 corner_center2 = vec2(1.0 - corner_radius, corner_radius); // 右下角
    vec2 corner_center3 = vec2(corner_radius, 1.0 - corner_radius); // 左上角
    vec2 corner_center4 = vec2(1.0 - corner_radius, 1.0 - corner_radius); // 右上角
    
    // 计算到各个圆心的距离
    float dist1 = distance(uv, corner_center1);
    float dist2 = distance(uv, corner_center2);
    float dist3 = distance(uv, corner_center3);
    float dist4 = distance(uv, corner_center4);
    
    // 检查当前像素是否在圆角区域内
    bool in_corner1 = uv.x < corner_radius && uv.y < corner_radius && dist1 > corner_radius;
    bool in_corner2 = uv.x > 1.0 - corner_radius && uv.y < corner_radius && dist2 > corner_radius;
    bool in_corner3 = uv.x < corner_radius && uv.y > 1.0 - corner_radius && dist3 > corner_radius;
    bool in_corner4 = uv.x > 1.0 - corner_radius && uv.y > 1.0 - corner_radius && dist4 > corner_radius;
    
    if (in_corner1 || in_corner2 || in_corner3 || in_corner4) {
        if (antialiasing) {
            // 抗锯齿处理
            float alpha = 1.0;
            if (in_corner1) alpha = 1.0 - smoothstep(corner_radius - 0.005, corner_radius + 0.005, dist1);
            else if (in_corner2) alpha = 1.0 - smoothstep(corner_radius - 0.005, corner_radius + 0.005, dist2);
            else if (in_corner3) alpha = 1.0 - smoothstep(corner_radius - 0.005, corner_radius + 0.005, dist3);
            else if (in_corner4) alpha = 1.0 - smoothstep(corner_radius - 0.005, corner_radius + 0.005, dist4);
            
            COLOR.a *= alpha;
        } else {
            // 无抗锯齿，直接裁切
            discard;
        }
    }
}