shader_type canvas_item;

uniform vec4 color: hint_color = vec4(0.498f, 0.729f, 0.816, 1.0f);

void fragment(){
	vec4 tex = texture(SCREEN_TEXTURE, SCREEN_UV);
	float gray_scale = (tex.r + tex.g + tex.b) / 3.0f;
	COLOR.rgb = vec3(gray_scale) * color.rgb;
}
