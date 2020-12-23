shader_type canvas_item;
render_mode blend_add;

uniform vec4 color: hint_color = vec4(0.1, 0.75, 0.28, 1.0f);
uniform bool modulate = true;

void fragment() {
	COLOR = texture(TEXTURE, UV);
	if (modulate) {
		COLOR *= color;
	}
}
