// =====================================================
// PLAN 3D, Support géométrique
// =====================================================

class skybox {
	
	// --------------------------------------------
	constructor() {
		this.shaderName = 'skybox';
		this.loaded     = -1;
        this.texLoaded  =  0;
		this.shader     = null;
        this.url_box    = './textures/skybox_1/';
        this.texture    = null;
        
        loadShaders(this);
		this.initAll();
	}
		
	// --------------------------------------------
	initAll() {
		var size=50.0;
        var vertices = [
             size, -size, -size,    size,  size, -size,    size,  size,  size,    size, -size,  size,     /*Sol*/     /*Plafond*/
            -size, -size,  size,   -size,  size,  size,    size,  size,  size,    size, -size,  size,     /*Plafond*/
            -size, -size,  size,    size, -size,  size,    size, -size, -size,   -size, -size, -size,     /*Devant*/
            -size,  size,  size,    size,  size,  size,    size,  size, -size,   -size,  size, -size,     /*Arrière*/
            -size,  size,  size,   -size, -size,  size,   -size, -size, -size,   -size,  size, -size,     /*Gauche*/
             size, -size,  size,    size,  size,  size,    size,  size, -size,    size, -size, -size,     /*Droite*/
        ];

        var indices = [
             0,  3,  2,     2,  1,  0,     /*Sol*/     /*Plafond*/
             4,  5,  6,     7,  4,  6,     /*Plafond*/
             9, 11,  8,    10, 11,  9,     /*Devant*/
            12, 14, 13,    14, 12, 15,     /*Arrière*/
            17, 18, 16,    18, 19, 16,     /*Gauche*/
            21, 23, 20,    23, 21, 22      /*Droite*/
        ];

		this.vBuffer = gl.createBuffer();
		gl.bindBuffer(gl.ARRAY_BUFFER, this.vBuffer);
		gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vertices), gl.STATIC_DRAW);
		this.vBuffer.itemSize = 3;
		this.vBuffer.numItems = vertices.length/3;

		this.iBuffer = gl.createBuffer();
		gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.iBuffer);
		gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, new Float32Array(indices), gl.STATIC_DRAW);
		this.iBuffer.itemSize = 1;
		this.iBuffer.numItems = indices.length;

        this.initCubeMap();
	}


    // --------------------------------------------
    initCubeMap()
    {
        this.texture = gl.createTexture();
        gl.bindTexture(gl.TEXTURE_CUBE_MAP, this.texture);

        for(let i=0; i<6; i++){
            var texImage = new Image();
            texImage.src = this.url_box + i + '.png';

            texImage.onload = function () {
                gl.texImage2D(gl.TEXTURE_CUBE_MAP_POSITIVE_X + i,
                    0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, texImage);
                    console.log(this.texLoaded)
            }
        }

        gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    }


    // --------------------------------------------
	setShadersParams() {
		gl.useProgram(this.shader);

		this.shader.vAttrib = gl.getAttribLocation(this.shader, "aVertexPosition");
		gl.enableVertexAttribArray(this.shader.vAttrib);
		gl.bindBuffer(gl.ARRAY_BUFFER, this.vBuffer);
		gl.vertexAttribPointer(this.shader.vAttrib, this.vBuffer.itemSize, gl.FLOAT, false, 0, 0);

        this.shader.samplerUniform  = gl.getUniformLocation(this.shader, "uSampler");
		this.shader.mvMatrixUniform = gl.getUniformLocation(this.shader, "uMVMatrix");
		this.shader.pMatrixUniform  = gl.getUniformLocation(this.shader, "uPMatrix");
    }


     // --------------------------------------------
	setUniforms() {
		mat4.identity(mvMatrix);
		// mat4.translate(mvMatrix, distCENTER);
		mat4.multiply(mvMatrix, rotMatrix);

		gl.uniformMatrix4fv(this.shader.mvMatrixUniform, false, mvMatrix);
		gl.uniformMatrix4fv(this.shader.pMatrixUniform,  false, pMatrix);
        gl.uniform1i(this.shader.samplerUniform, 0);
		gl.activeTexture(gl.TEXTURE0);
	}


    // --------------------------------------------
	draw() {
		if(this.shader && this.loaded==4 && this.texLoaded == 6) {
            console.log("oyu")
			this.setShadersParams();
			this.setUniforms();

			gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.iBuffer);
			gl.drawElements(gl.TRIANGLES, this.iBuffer.numItems, gl.UNSIGNED_SHORT, 0);
		}
	}
}
