[Variables]

[Vertex]
void main()
{	
	gl_TexCoord[0].xy = gl_MultiTexCoord0.xy;	
	
	gl_FrontColor = gl_Color;

	gl_Position =  ftransform();
}

[Fragment]
uniform sampler2D ImageMap;
uniform float BlendFactor;

void main()
{	    
	vec4 baseColor = texture2D(ImageMap, gl_TexCoord[0].xy );

	gl_FragColor = (1.0 - BlendFactor) * baseColor + BlendFactor * gl_Color;	
}

