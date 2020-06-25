shader_type spatial;

uniform vec2 size;
uniform vec2 start_pos;
uniform vec4 tile_color: hint_color;
uniform vec4 grout_color: hint_color;

float tile(vec2 uv)
{
	vec2 color = step(uv, vec2(0.9));
	return color.x * color.y;
}

void fragment()
{
	vec2 uv = UV * size + start_pos;
	uv = fract(uv + vec2(0., step(mod(uv.x, 2.), 1) * 0.5));
	vec3 color = mix(grout_color.rgb, tile_color.rgb, tile(uv));

	ALBEDO = color;
}