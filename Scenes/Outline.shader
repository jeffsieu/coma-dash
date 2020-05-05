shader_type spatial;
render_mode unshaded;

uniform float scale = 2.0;
uniform float threshold = 2.0;
uniform vec3 color = vec3(0, 0, 0);
uniform bool enabled = true;

varying mat4 CAMERA;

void vertex() 
{
	POSITION = vec4(VERTEX, 1.0);
	CAMERA = CAMERA_MATRIX;
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

float diff_sq(vec2 uv, vec2 dir, sampler2D depth_tex, mat4 inv_projection_mat) {
	float d1 = depth_calc(uv + dir, depth_tex, inv_projection_mat);
	float d2 = depth_calc(uv - dir, depth_tex, inv_projection_mat);
	return (d1 - d2) * (d1 - d2);
}

void fragment() 
{	
	vec2 uv = SCREEN_UV;
	float offset = 0.001 * scale;
	float dist = 
		diff_sq(uv, vec2(offset, offset), DEPTH_TEXTURE, INV_PROJECTION_MATRIX) + 
		diff_sq(uv, vec2(-offset, offset), DEPTH_TEXTURE, INV_PROJECTION_MATRIX);
	// Once Godot supports accessing the normal maps, more enhancements can be made
	// See: https://roystan.net/articles/outline-shader.html
	dist = sqrt(dist);
	if (dist > threshold && enabled)
	{
		ALBEDO = color;
		ALPHA = 1.0;
	}
	else
	{
		ALPHA = 0.0;
	}
}
