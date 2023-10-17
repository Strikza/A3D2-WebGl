
attribute vec3 aVertexPosition;
attribute vec3 aVertexNormal;

uniform mat4 uRMatrix;
uniform mat4 uMVMatrix;
uniform mat4 uPMatrix;
uniform vec3 uLightSource;

varying vec4 pos3D;
varying vec3 lightSource;
varying vec3 normal;

void main(void) {
	pos3D = uMVMatrix * vec4(aVertexPosition,1.0);
	lightSource = normalize(uLightSource);
	normal = vec3(uRMatrix * vec4(aVertexNormal, 1.0));
	
	gl_Position = uPMatrix * pos3D;
}
