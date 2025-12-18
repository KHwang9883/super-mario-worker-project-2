shader_type canvas_item;

uniform float corner_radius : hint_range(0, 0.5) = 0.1;
// 新增：圆角淡出过渡宽度（越大，圆角边缘淡出越柔和）
uniform float corner_fade_width : hint_range(0.001, 0.05) = 0.01;
uniform float fade_strength : hint_range(0.0, 1.0) = 1.0;
uniform float fade_curve : hint_range(1.0, 5.0) = 2.0;

render_mode blend_add;

void fragment() {
    // 获取纹理坐标
    vec2 uv = UV;
    vec2 center = vec2(0.5, 0.5); // 素材中心

    // ========== 1. 圆角平滑淡出逻辑（核心修改） ==========
    float corner_alpha = 1.0; // 圆角初始透明度因子（1=完全不透明）

    // 分别计算四个圆角的距离和淡出因子
    vec2 corner1 = vec2(corner_radius, corner_radius); // 左下角
    vec2 corner2 = vec2(1.0 - corner_radius, corner_radius); // 右下角
    vec2 corner3 = vec2(corner_radius, 1.0 - corner_radius); // 左上角
    vec2 corner4 = vec2(1.0 - corner_radius, 1.0 - corner_radius); // 右上角

    float dist1 = distance(uv, corner1);
    float dist2 = distance(uv, corner2);
    float dist3 = distance(uv, corner3);
    float dist4 = distance(uv, corner4);

    // 对每个圆角区域计算平滑淡出（不再硬切，而是渐变）
    // 仅在圆角范围内生效（比如左下角：uv.x < corner_radius 且 uv.y < corner_radius）
    if (uv.x < corner_radius && uv.y < corner_radius) {
        // smoothstep(内边界, 外边界, 距离)：距离越大于corner_radius，值越接近1，alpha越小
        corner_alpha = 1.0 - smoothstep(corner_radius - corner_fade_width, corner_radius + corner_fade_width, dist1);
    } else if (uv.x > 1.0 - corner_radius && uv.y < corner_radius) {
        corner_alpha = 1.0 - smoothstep(corner_radius - corner_fade_width, corner_radius + corner_fade_width, dist2);
    } else if (uv.x < corner_radius && uv.y > 1.0 - corner_radius) {
        corner_alpha = 1.0 - smoothstep(corner_radius - corner_fade_width, corner_radius + corner_fade_width, dist3);
    } else if (uv.x > 1.0 - corner_radius && uv.y > 1.0 - corner_radius) {
        corner_alpha = 1.0 - smoothstep(corner_radius - corner_fade_width, corner_radius + corner_fade_width, dist4);
    }
    // 非圆角区域：corner_alpha保持1.0（无淡出）

    // ========== 2. 中心向外先缓后快淡出逻辑（保留并优化） ==========
    // 计算到中心的距离
    float dist_to_center = length(uv - center);
    // 归一化到0~1（中心到角落的最大距离）
    float max_dist = length(vec2(0.5));
    float normalized_dist = dist_to_center / max_dist;
    // 先缓后快的淡出曲线
    float center_fade_factor = 1.0 - pow(normalized_dist, fade_curve);
    // 叠加淡出强度
    center_fade_factor = mix(1.0, center_fade_factor, fade_strength);

    // ========== 3. 合并所有淡出因子（圆角+中心） ==========
    COLOR.a *= corner_alpha * center_fade_factor;
}