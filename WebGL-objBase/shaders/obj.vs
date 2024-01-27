
attribute vec3 aVertexPosition;
attribute vec3 aVertexNormal;

uniform mat4 uRMatrix;
uniform mat4 uMVMatrix;
uniform mat4 uPMatrix;

varying vec4 pos3D;
varying vec3 normal;

varying mat4 vRMatrix;


mat4 transpose(mat4 m) {
	return mat4(m[0][0], m[1][0], m[2][0], m[3][0],
				m[0][1], m[1][1], m[2][1], m[3][1],
				m[0][2], m[1][2], m[2][2], m[3][2],
				m[0][3], m[1][3], m[2][3], m[3][3] );
}


void main(void) {
	vRMatrix = transpose(uRMatrix);

	pos3D = uMVMatrix * vec4(aVertexPosition,1.0);
	normal = vec3(uRMatrix * vec4(aVertexNormal, 1.0));
	
	gl_Position = uPMatrix * pos3D;
}
