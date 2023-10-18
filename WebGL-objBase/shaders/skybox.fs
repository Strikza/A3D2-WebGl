precision mediump float;

varying vec3 TexCoords;

uniform samplerCube uSampler;

void main()
{  
	gl_FragColor = textureCube(uSampler, TexCoords.xzy);
}
