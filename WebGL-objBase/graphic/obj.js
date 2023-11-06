// =====================================================
// OBJET 3D, lecture fichier obj
// =====================================================

class objmesh {

	// --------------------------------------------
	constructor(objFname) {
		this.objName    = objFname;
		this.shaderName = 'obj';
		this.loaded     = -1;
		this.shader     = null;
		this.mesh       = null;
		this.col        = null;
		
		loadObjFile(this);
		loadShaders(this);
	}

	// --------------------------------------------
	hexaToRGB(col) {
		return [
			parseInt(col.substring(1,3), 16)/255, 
			parseInt(col.substring(3,5), 16)/255, 
			parseInt(col.substring(5),   16)/255
		];
	}

	// --------------------------------------------
	setColor(value) {
		this.col = this.hexaToRGB(value);
	}

	// --------------------------------------------
	setShadersParams() {
		gl.useProgram(this.shader);

		this.shader.vAttrib = gl.getAttribLocation(this.shader, "aVertexPosition");
		gl.enableVertexAttribArray(this.shader.vAttrib);
		gl.bindBuffer(gl.ARRAY_BUFFER, this.mesh.vertexBuffer);
		gl.vertexAttribPointer(this.shader.vAttrib, this.mesh.vertexBuffer.itemSize, gl.FLOAT, false, 0, 0);

		this.shader.nAttrib = gl.getAttribLocation(this.shader, "aVertexNormal");
		gl.enableVertexAttribArray(this.shader.nAttrib);
		gl.bindBuffer(gl.ARRAY_BUFFER, this.mesh.normalBuffer);
		gl.vertexAttribPointer(this.shader.nAttrib, this.mesh.vertexBuffer.itemSize, gl.FLOAT, false, 0, 0);

		this.shader.rMatrixUniform  = gl.getUniformLocation(this.shader, "uRMatrix");
		this.shader.mvMatrixUniform = gl.getUniformLocation(this.shader, "uMVMatrix");
		this.shader.pMatrixUniform  = gl.getUniformLocation(this.shader, "uPMatrix");
		this.shader.lightSource     = gl.getUniformLocation(this.shader, "uLightSource");
		this.shader.material        = gl.getUniformLocation(this.shader, "uMaterial");
		this.shader.sigmaUniform    = gl.getUniformLocation(this.shader, "uSigma");
		this.shader.refractUniform  = gl.getUniformLocation(this.shader, "uRefract");
		this.shader.uDistrib        = gl.getUniformLocation(this.shader, "uDistrib");
        this.shader.uMode           = gl.getUniformLocation(this.shader, "uMode");
        this.shader.samplerUniform  = gl.getUniformLocation(this.shader, "uSampler");
	}
	
	// --------------------------------------------
	setUniforms() {
		mat4.identity(mvMatrix);
		mat4.translate(mvMatrix, distCENTER);
		//mat4.rotate(rotMatrix, 90.0, [1, 0, 0]);
		mat4.multiply(mvMatrix, rotMatrix);
		let distrib = window.localStorage.getItem("dShader");

		gl.uniformMatrix4fv(this.shader.rMatrixUniform,  false, rotMatrix);
		gl.uniformMatrix4fv(this.shader.mvMatrixUniform, false, mvMatrix);
		gl.uniformMatrix4fv(this.shader.pMatrixUniform,  false, pMatrix);
		gl.uniform3fv(this.shader.lightSource, lightSource);
		gl.uniform3f(this.shader.material, this.col[0], this.col[1], this.col[2]);
		gl.uniform1f(this.shader.sigmaUniform, gui.sigma.value);
		gl.uniform1f(this.shader.refractUniform, gui.refract.value);
		gl.uniform1i(this.shader.uDistrib, distrib);
		gl.uniform1i(this.shader.uMode, gui.display_mode.value);
		
        gl.uniform1i(this.shader.samplerUniform, 0);
	}
	
	// --------------------------------------------
	draw() {
		if(this.shader && this.loaded==4 && this.mesh != null) {
			this.setShadersParams();
			this.setUniforms();
			gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this.mesh.indexBuffer);
			gl.drawElements(gl.TRIANGLES, this.mesh.indexBuffer.numItems, gl.UNSIGNED_SHORT, 0);
		}
	}
}

// END obj.js
