shader_type spatial;

uniform float radius = 5.0;
uniform float startTime;

uniform float wave_speed = 3.0;
uniform float wave_width = 0.1;
uniform float wave_height = 1.0;

uniform float t;

float sinc(float x) {
	if (x < 0.001) return 1.0;
	return sin(x) / x;
}

bool onwave(float time, float r) {
	if (r > wave_speed * time + wave_width) return false;
	if (r < wave_speed * time - wave_width) return false;
	return true;
}

float height(float time, float r) {
	if (r > wave_speed * time + wave_width) return 0.0;
	if (r < wave_speed * time - wave_width) return 0.0;
	float dist = (r - wave_speed * time) / wave_width;
	return wave_height * smoothstep(0, 1.0, 1.0 - abs(dist)) * (1.0 - r);
	// return wave_height * (1.0 - dist * dist) * (1.0 - r);
}

void vertex() {
	float time = fract(TIME);
	// time = TIME - startTime;
	time = t;
	vec2 uv = UV * 2.0 - 1.0;
	float r = length(uv);
	float theta = atan(uv.y, uv.x);

	
	vec2 direction = normalize(uv);
	vec3 forward = vec3(direction.x, 0, direction.y);
	vec3 up = vec3(0, 1.0, 0);
	vec3 right = cross(forward, up);
	float ht = height(time, r);
	vec3 this = vec3(uv.x * radius, ht, uv.y * radius);
	
	float offset = 0.001;
	float r2 = length(uv + direction * offset);
	float h2 = height(time, r2);
	vec3 other = vec3(
		(uv.x + direction.x * offset) * radius, 
		h2, 
		(uv.y + direction.y * offset) * radius);
	vec3 tangent = other - this;
	
	NORMAL = cross(right, tangent);
	VERTEX.y += ht;
}

void fragment() {
	float time = fract(TIME);
	// time = TIME - startTime;
	time = t;
	vec2 uv = UV * 2.0 - 1.0;
	float r = length(uv);
	ALBEDO = vec3(0.5, 0.5, 0.5);
	ALBEDO = vec3(1.0, 1.0, 0);
	if (r < 1.0 && onwave(time, r))
		ALPHA = 0.5;
	else
		ALPHA = 0.0;
}