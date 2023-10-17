attribute vec3 aVertexPosition;

uniform mat4 uMVMatrix;
uniform mat4 uPMatrix;

varying vec3 TexCoords;

void main()
{
    TexCoords = aVertexPosition;

    gl_Position = uPMatrix * uMVMatrix *vec4(aVertexPosition, 1.0);
}  
