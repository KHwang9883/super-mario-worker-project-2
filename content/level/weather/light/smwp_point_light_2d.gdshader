shader_type canvas_item;

const int MAX_LIGHTS = 100;
uniform vec2 lights[MAX_LIGHTS];

uniform float lightRadius : hint_range(0.0, 100.0, 0.1);
uniform vec2 res = vec2(1024,576);

void fragment() {
    float result = 0.0;
    vec2 uv = UV * res;
    for(int i = 0; i < MAX_LIGHTS; i++){
        vec2 l = lights[i];
        if(l == vec2(0.0,0.0)) continue;

        result += 1.0 - min(distance(l, uv) / lightRadius, 1.0);
    }

    COLOR.a *= 1.0 - min(result,1.0);
}