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
        this.url_box    = './textures/skybox_3/';
        this.texture    = null;
        
		this.initAll();
        loadShaders(this);
	}
		
	// --------------------------------------------
	initAll() {
		var size=50.0;
        var vertices = [
            //Right
             size, -size, -size,  //0
             size,  size, -size,  //1  
             size,  size,  size,  //2
             size, -size,  size,  //3
            //Left
            -size, -size,  size,  //4
            -size,  size,  size,  //5 
            -size,  size, -size,  //6
            -size, -size, -size,  //7
            //Top
            -size,  size, -size,  //8 
            -size,  size,  size,  //9 
             size,  size,  size,  //10
             size,  size, -size,  //11
            //Bottom
            -size, -size,  size,  //12
            -size, -size, -size,  //13 
             size, -size, -size,  //14
             size, -size,  size,  //15  
            //Back
            -size, -size, -size,  //16
            -size,  size, -size,  //17
             size,  size, -size,  //18
             size, -size, -size,  //19  
            //Front
             size, -size,  size,  //20
             size,  size,  size,  //21
            -size,  size,  size,  //22
            -size, -size,  size   //23
        ];

        var indices = [
            //Right
            3,1,0,      1,3,2,
            //Left
            7,5,4,      5,7,6,
            //Top
            11,9,8,     9,11,10,
            //Bottom
            15,13,12,   13,15,14,
            //Back
            19,17,16,   17,19,18,
            //Front
            23,21,20,   21,23,22,
        ];

		this.vBuffer = gl.createBuffer();
		gl.bindBuffer(gl.ARRAY_BUFFER, this.vBuffer);
		gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vertices), gl.STATIC_DRAW);
		this.vBuffer.itemSize = 3;
		this.vBuffer.numItems = vertices.length/3;

		this.iBuffer = gl.createBuffer();
		gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.iBuffer);
		gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, new  Uint16Array(indices), gl.STATIC_DRAW);
		this.iBuffer.itemSize = 1;
		this.iBuffer.numItems = indices.length;

        this.initCubeMap();
	}


    // --------------------------------------------
    initCubeMap()
    {
        this.texture = gl.createTexture();
        gl.bindTexture(gl.TEXTURE_CUBE_MAP, this.texture);

        const faceInfos = [
			{target: gl.TEXTURE_CUBE_MAP_POSITIVE_X, url:this.url_box+"/0.png"},
			{target: gl.TEXTURE_CUBE_MAP_NEGATIVE_X, url:this.url_box+"/1.png"},
			{target: gl.TEXTURE_CUBE_MAP_POSITIVE_Y, url:this.url_box+"/2.png"},
			{target: gl.TEXTURE_CUBE_MAP_NEGATIVE_Y, url:this.url_box+"/3.png"},
			{target: gl.TEXTURE_CUBE_MAP_POSITIVE_Z, url:this.url_box+"/5.png"},
			{target: gl.TEXTURE_CUBE_MAP_NEGATIVE_Z, url:this.url_box+"/4.png"},
		];

		faceInfos.forEach((faceInfo) => {
			const {target, url} = faceInfo;
		
			const image = new Image();
			image.src = url;
			image.addEventListener('load', ()=> {
				gl.bindTexture(gl.TEXTURE_CUBE_MAP, this.texture);
				gl.texImage2D(target, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
			});
		});

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
		mat4.multiply(mvMatrix, rotMatrix);

		gl.uniformMatrix4fv(this.shader.mvMatrixUniform, false, mvMatrix);
		gl.uniformMatrix4fv(this.shader.pMatrixUniform,  false, pMatrix);
        gl.uniform1i(this.shader.samplerUniform, 0);
		gl.activeTexture(gl.TEXTURE0);
	}


    // --------------------------------------------
	draw() {
		if(this.shader && this.loaded==4 ) {
			this.setShadersParams();
			this.setUniforms();

			gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.iBuffer);
			gl.drawElements(gl.TRIANGLES, this.iBuffer.numItems, gl.UNSIGNED_SHORT, 0);
		}
	}
}
