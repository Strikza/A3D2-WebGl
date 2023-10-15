
precision mediump float;

uniform vec3  uMaterial;
uniform float uSigma;
uniform float uRefract;

varying vec4 pos3D;
varying vec3 lightSource;
varying vec3 normal;

// ==============================================
float square(float f){
	return f*f;
}

float clampedDot(vec3 a, vec3 b){
	return max(0.0, dot(a, b));
}


// ==============================================
float fresnel(vec3 m){

	float c = dot(lightSource, m);
	float g = sqrt(uRefract*uRefract + c*c - 1.0);

	float gPc = g+c;
	float gLc = g-c;

	return 0.5 * (square(gLc)/square(gPc)) * (1.0 + square(c * gPc - 1.0 ) / (1.0 + square(c * gLc + 1.0)));
}


// ==============================================
float beckmann(vec3 m, vec3 N){
	
	float cosTheta  = dot(N, normalize(m));
	float cosTheta2 = square(cosTheta);
	float tanTheta2 = (1.0 - cosTheta2)/cosTheta2;
	float sigma2    = square(uSigma);

	float e = exp(-(tanTheta2) / (2.0*(sigma2)));
	float denom = 3.14*sigma2*square(cosTheta2);

	return 1.0/denom * e;
}


// ==============================================
float masquage(vec3 m, vec3 N, vec3 i, vec3 o){

	float nm = dot(N, m);

	float Go = 2.0*nm*dot(N, o) / dot(o, m);
	float Gi = 2.0*nm*dot(N, i) / dot(i, m);

	return min(1.0, min(Go, Gi));
}


// ==============================================
void main(void)
{
	vec3 colorCT;
	vec3 o = normalize(vec3(-pos3D));
	vec3 i = -lightSource;
	vec3 N = normalize(normal);
	vec3 m = normalize(o + i);

	float iN = clampedDot(i, N);
	float oN = clampedDot(o, N);

	if(iN != 0.0 || oN != 0.0){
		float F = fresnel(m);
		float D = beckmann(m, N);
		float G = masquage(m, N, i, o);

		float cookTorance = F*D*G / 4.0*iN*oN;

		colorCT = vec3(cookTorance, cookTorance, cookTorance);
	}
	else{
		colorCT = vec3(0.0, 0.0, 0.0);
	}

	vec3 col = uMaterial * dot(N, i) + colorCT; // Lambert rendering, eye light source
	gl_FragColor = vec4(col,1.0);
}
