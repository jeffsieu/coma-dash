shader_type spatial;
render_mode unshaded;

uniform int scale = 2;
uniform float threshold = 2.0;
uniform vec4 color : hint_color = vec4(0, 0, 0, 1);
uniform bool enabled = true;

uniform float pixels_x = 1024.0;
uniform float pixels_y = 600.0;

// varying mat4 CAMERA;

void vertex() 
{
	POSITION = vec4(VERTEX, 1.0);
	// CAMERA = CAMERA_MATRIX;
}

float depth_calc(vec2 uv, sampler2D depth_tex, mat4 inv_projection_mat) 
{
	// based on: https://docs.godotengine.org/en/stable/tutorials/shading/advanced_postprocessing.html
	float d = texture(depth_tex, uv).r;
	vec3 ndc = vec3(uv, d) * 2.0 - 1.0;
	vec4 view = inv_projection_mat * vec4(ndc, 1.0);
  	view.xyz /= view.w;
	// For future use: computing world coordinates
	// vec4 world = CAMERA * inv_projection_mat * vec4(ndc, 1.0);
  	// vec3 world_position = world.xyz / world.w;
  	float linear_depth = -view.z;
	return linear_depth;
}

float diff_sq(vec2 uv, vec2 dir, sampler2D depth_tex, mat4 inv_projection_mat)
{
	float d1 = depth_calc(uv + dir, depth_tex, inv_projection_mat);
	float d2 = depth_calc(uv - dir, depth_tex, inv_projection_mat);
	return (d1 - d2) * (d1 - d2);
}

float edge(vec2 uv, sampler2D depth_tex, mat4 inv_projection_mat)
{
	vec2 offset = vec2(1.0 / pixels_x, 1.0 / pixels_y) * float(scale);
	float dist =
		diff_sq(uv, offset, depth_tex, inv_projection_mat) +
		diff_sq(uv, offset * vec2(-1.0, 1.0),
			depth_tex, inv_projection_mat);
	dist = sqrt(dist);
	if (dist > threshold && enabled)
		return 1.0;
	else
		return 0.0;
}

float aliased_edge(vec2 uv, sampler2D depth_tex, mat4 inv_projection_mat)
{
	vec2 offset = vec2(1.0 / pixels_x, 1.0 / pixels_y) * float(scale) / 2.0;
	float total = 0.0;
	// total += edge(uv, depth_tex, inv_projection_mat);
	total += edge(uv + offset, depth_tex, inv_projection_mat);
	total += edge(uv - offset, depth_tex, inv_projection_mat);
	total += edge(uv + offset * vec2(-1.0, 1.0), depth_tex, inv_projection_mat);
	total += edge(uv - offset * vec2(-1.0, 1.0), depth_tex, inv_projection_mat);
	total += 2.0 * edge(uv + offset * vec2(1.0, 0.0), depth_tex, inv_projection_mat);
	total += 2.0 * edge(uv - offset * vec2(1.0, 0.0), depth_tex, inv_projection_mat);
	total += 2.0 * edge(uv + offset * vec2(0.0, 1.0), depth_tex, inv_projection_mat);
	total += 2.0 * edge(uv - offset * vec2(0.0, 1.0), depth_tex, inv_projection_mat);
	total += 4.0 * edge(uv, depth_tex, inv_projection_mat);
	total /= 16.0;
	// total += edge(uv + offset * vec2(1.0, 0.0), depth_tex, inv_projection_mat);
	// total += edge(uv - offset * vec2(1.0, 0.0), depth_tex, inv_projection_mat);
	// total += edge(uv + offset * vec2(0.0, 1.0), depth_tex, inv_projection_mat);
	// total += edge(uv - offset * vec2(0.0, 1.0), depth_tex, inv_projection_mat);
	// total += edge(uv, depth_tex, inv_projection_mat);
	// total /= 9.0;
	return total;
}

void fragment() 
{	
	vec2 uv = SCREEN_UV;
	// Once Godot supports accessing the normal maps, more enhancements can be made
	// See: https://roystan.net/articles/outline-shader.html
	ALBEDO = color.rgb;
	ALPHA = aliased_edge(uv, DEPTH_TEXTURE, INV_PROJECTION_MATRIX);
}
