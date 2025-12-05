shader_type canvas_item;

const int MAX_LIGHTS = 128;

uniform float darkness = 1.0;
uniform vec2 positions[MAX_LIGHTS];

void fragment() {
	// 初始化全屏黑色
	vec4 final_color = vec4(0.0, 0.0, 0.0, darkness);
	
	// 获取屏幕尺寸
	vec2 screen_size = 1.0 / TEXTURE_PIXEL_SIZE;
	
	// 计算当前片段在屏幕上的位置（像素坐标）
	vec2 frag_pos = UV * screen_size;
	
	// 遍历所有灯光位置
	for (int i = 0; i < MAX_LIGHTS; i++) {
		// 跳过无效位置
		if (positions[i].x == 0.0 && positions[i].y == 0.0) {
			continue;
		}
		
		// 计算到灯光中心的距离（像素单位）
		float d = length(frag_pos - positions[i]);
		
		// 如果距离小于半径（例如100像素），则减少透明度
		if (d < 100.0) {
			// 距离越近，透明度越低（越透明）
			float alpha_reduction = 1.0 - (d / 100.0);
			final_color.a = max(0.0, final_color.a - alpha_reduction);
		}
	}
	
	COLOR = final_color;
}