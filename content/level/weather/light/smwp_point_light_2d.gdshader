shader_type canvas_item;

const int MAX_LIGHTS = 256;

uniform float darkness = 1.0;
uniform float radius_ratio = 1.0;
uniform vec2 positions[MAX_LIGHTS];
uniform float single_radius[MAX_LIGHTS];
uniform bool is_lava_light[MAX_LIGHTS];

void fragment() {
	float origin_alpha = 1.0;
	vec4 final_color = vec4(0.0, 0.0, 0.0, origin_alpha);

	vec2 screen_size = 1.0 / TEXTURE_PIXEL_SIZE;

	// 计算当前片段在屏幕上的位置（像素坐标）
	vec2 frag_pos = UV * screen_size;

	// 如果是 ColorRect 用下面这行
	//frag_pos = UV * vec2(640.0, 480.0);

	float max_alpha[MAX_LIGHTS];
	float alpha_reduction = 0.0;
	
	// 遍历所有灯光位置
	for (int i = 0; i < MAX_LIGHTS; i++) {
		// 跳过无效位置
		if (positions[i].x == 0.0 && positions[i].y == 0.0) {
			continue;
		}
		
		float d;
		if (!is_lava_light[i]) {
			d = length(frag_pos - positions[i]);
		}
		// 全局流体岩浆光源
		else {
			d = abs(frag_pos.y - positions[i].y);
		}

		// Easy version test
		/*if (d < 100.0) {
			float alpha_reduction = 1.0 - (d / 100.0);
			final_color.a = max(0.0, final_color.a - alpha_reduction);
		}*/

		d /= radius_ratio;
		d /= single_radius[i];

		if (d < 640.0) {
			alpha_reduction =
				min(1.0, max(0.0, (
					-0.00000252 * d * d * d
					+ 0.00011728 * d * d
					+ 0.00008833 * d
					+ 0.98487793)
					)
				);
			max_alpha[i] = origin_alpha - alpha_reduction;
			final_color.a = min(final_color.a, max_alpha[i]);
			
			// 原版直接叠加，光源之间有部分重合不美观
			//final_color.a = final_color.a - alpha_reduction;
		}

		// Offset delay debug
		/*if (d < 100.0) {
			final_color.a = 0.0;
		}
		if (d < 10.0) {
			final_color.a = 1.0;
		}*/
	}

	COLOR = clamp(final_color, vec4(0.0, 0.0, 0.0, 0.0), vec4(0.0, 0.0, 0.0, darkness));
}