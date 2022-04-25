#ifndef NPR_INCLUDE
#define NPR_INCLUDE

float4 _ColorStreet;
float _SpecularSegment;

float3 GetNPRLighting(Surface surface, BRDF brdf, Light light)
{
	_ColorStreet = float4(0.1, 0.3, 0.6, 1.0);
	_SpecularSegment = 0.9;
	float spec = SpecularStrength(surface, brdf, light);
	float diff = IncomingLightAttenua;
	float w = fwidth(diff) * 2.0;
	if (diff < ColorStreet.x + w)
	{
		diff = lerp(ColorStreet.x, ColorStreet.y, smoothstep(ColorStreet.x - w, ColorStreet.x + w, diff));
	}
	else if (diff < ColorStreet.y + w)
	{
		diff = lerp(ColorStreet.y, ColorStreet.z, smoothstep(ColorStreet.y - w, ColorStreet.y + w, diff));
	}
	else if (diff < ColorStreet.z + w)
	{
		diff = lerp(ColorStreet.z, ColorStreet.w, smoothstep(ColorStreet.z - w, ColorStreet.z + w, diff));
	}
	else
	{
		diff = ColorStreet.w;
	}
	w = fwidth(spec);
	if (spec < _SpecularSegment + w)
	{
		spec = lerp(0, 1, smoothstep(_SpecularSegment - w, _SpecularSegment + w, spec));
	}
	else
		spec = 1.0;
	return diff * light.color * surface.color * (spec * brdf.specular + brdf.diffuse);
}

#endif //NPR_INCLUDE