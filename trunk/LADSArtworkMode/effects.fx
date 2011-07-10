sampler2D input : register(s0);
float brightness : register(c0);
float contrast : register(c1);
float saturation : register(c2);

float4 main(float2 uv : TEXCOORD) : COLOR
{
	float bright = brightness / (100.0 * 3.0);
	float cont = pow( (100.0 + contrast)/100.0,2);
	float sat = (saturation +100.0) / 100.0;
	
    float4 color = tex2D(input, uv); 
    float4 result = color;
	
	// CHANGE BRIGHTNESS:
    result.rgb = color.rgb + bright;
    result.rgb = result.rgb - 0.5;
	
	// CHANGE CONTRAST:
	result.rgb = result.rgb * (cont);
	result.rgb = result.rgb + 0.5;
	
	
	// CHANGE SATURATION:
    float3  LuminanceWeights = float3(0.299,0.587,0.114);
	float4    srcPixel = result;
	float    luminance = dot(srcPixel,LuminanceWeights);
	result = lerp(luminance,srcPixel,sat);
	//retain the incoming alpha
	result.a = srcPixel.a;             
    return result;
}
