
precision mediump float;

uniform vec3  uMaterial;
uniform float uSigma;
uniform float uRefract;
uniform float uRatioMT;
uniform int   uDistrib;
uniform int   uMode;
uniform int   uRayAmount;

uniform samplerCube uSampler;

varying vec4 pos3D;
varying vec3 lightSource;
varying vec3 normal;


varying mat4 vRMatrix;

// FONCTIONS UTILITAIRES
// ==============================================
float square(float f){
	return f*f;
}

// ==============================================
float clampValue(float value, float min, float max){
	float ret = value;

	if(value < min) ret = min;
	if(value > max) ret = max;

	return ret;
}

// ==============================================
float clampedDot(vec3 a, vec3 b){
	return max(0.0, dot(a, b));
}

// ==============================================
float seed = 0.0;

float getRandom(){
	seed++;
	return fract(sin(dot(pos3D.xy + seed, vec2(12.9898,78.233))) * 43758.5453123);
}

// ==============================================
mat3 getNewRotationMatrix(vec3 N, vec3 o){
	vec3 newRMatrix = vec3(1.0, 0.0, 0.0);

	if (clampedDot(N,newRMatrix) > 0.9){
		newRMatrix = vec3(0.0, 1.0, 0.0);
	}

    vec3 j = normalize(cross(N,newRMatrix));
    vec3 i = normalize(cross(j,N));

    return mat3(i,j,N);
}

// ==============================================
vec3 getMicroNormal(vec3 N, vec3 o){
    mat3 localRMatrix = getNewRotationMatrix(N, o);

    float phi = getRandom() * 2.0 * 3.1415;
	float theta = atan( sqrt((-square(uSigma)) * log(1.0 - getRandom())) );

    float x = sin(theta)*cos(phi);
    float y = sin(theta)*sin(phi);
    float z = cos(theta);

    vec3 m = localRMatrix * normalize(vec3(x, y, z));

    return m;
}

// BRDF
// ==============================================
float fresnel(vec3 i, vec3 m){

	float c = dot(i, m);
	float g = sqrt(uRefract*uRefract + c*c - 1.0);

	float gPc = g+c;
	float gLc = g-c;

	return 0.5 * (square(gLc)/square(gPc)) * (1.0 + square(c * gPc - 1.0 ) / square(c * gLc + 1.0));
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
float walter_ggx(vec3 m, vec3 N){

	float cosTheta  = dot(N, normalize(m));
	float cosTheta2 = square(cosTheta);
	float tanTheta2 = (1.0 - cosTheta2)/cosTheta2;
	float sigma2    = square(uSigma);

	float denom = 3.14 * square(cosTheta2) * square((sigma2 + tanTheta2));

	return sigma2/denom;
}

// ==============================================
float masquage(vec3 m, vec3 N, vec3 i, vec3 o){

	float nm = dot(N, m);

	float Go = 2.0*nm*dot(N, o) / dot(o, m);
	float Gi = 2.0*nm*dot(N, i) / dot(i, m);

	return min(1.0, min(Go, Gi));
}

// ==============================================
vec4 reflection(vec3 o, vec3 N){
    vec3 r = reflect(-o, N);
    r = vec3(vRMatrix * vec4(r, 1.0));
    return textureCube(uSampler, r.xzy);
}

// ==============================================
vec4 refraction(vec3 o, vec3 N){
    vec3 r = refract(-o, N, 1.0/uRefract);
    r = vec3(vRMatrix * vec4(r, 1.0));
    return textureCube(uSampler, r.xzy);
}

// ==============================================
vec4 reflect_refract(vec3 o, vec3 N){

	vec4 T = refraction(o, N) * uRatioMT;
	vec4 M = reflection(o, N) * (1.0-uRatioMT);

	return M+T;
}

// ==============================================
vec4 CookTorrance(vec3 o, vec3 i, vec3 N){
	vec3 colorCT;
	vec3 m = normalize(o + i);

	float iN = clampedDot(i, N);
	float oN = clampedDot(o, N);

	if(iN != 0.0 || oN != 0.0){
		float F = fresnel(i,m);
		float D;
		if(uDistrib == 0){
			D = beckmann(m, N);
		}
		else{
			D = walter_ggx(m, N);
		}
		float G = masquage(m, N, i, o);

		float cookTorance = F*D*G / 4.0*iN*oN;

		colorCT = vec3(cookTorance, cookTorance, cookTorance);
	}
	else{
		colorCT = vec3(0.0, 0.0, 0.0);
	}

	vec3 col = uMaterial * dot(N, i) + colorCT; // Lambert rendering with Cook & Torrance
	
	return vec4(col, 1.0);
}

// FROSTED MIRRORS
// ==============================================
vec4 frosted_mirror_noFresnel(vec3 o, vec3 N, int rayAmount){
	vec4 Lo = vec4(0.0);

	for (int i = 0; i < 100; ++i) {
		if (i >= rayAmount) break;
		
        vec3 m = getMicroNormal(N, o);
        Lo += reflection(o, m);
	}
	return Lo / float(rayAmount);
}

vec4 frosted_mirror_withFresnel(vec3 o, vec3 N, int rayAmount){
	vec3 Lo = vec3(0.0);
	
    int rayCpt = 0;

	for (int i = 0; i < 100; ++i) {
		if (i >= rayAmount) break;

        vec3 m  = getMicroNormal(N, o);  
        vec3 li = normalize(reflection(o, m).xyz);

        vec3 iVec = normalize(reflect(-o, m));
		
        float iN = clampedDot(iVec, N);
        float oN = clampedDot(o, N);
        float mN = clampedDot(m, N);
        float im = clampedDot(iVec, m);
        float om = clampedDot(o, m);

		const float margin = 0.001;

        if(iN < margin || oN < margin || mN < margin || im < margin || om < margin) continue;

        float F = fresnel(iVec, m);
        float D = beckmann(m, N);
        float G = masquage(m, N, iVec, o);
        float Fr = (F * D * G) / (4.0 * iN * oN);
        float pdf = D * mN;

        Lo += li * Fr * iN / pdf;

        rayCpt++;
    }
	
	return vec4(Lo / float(rayCpt), 1.0);
}

// ==============================================
void main(void)
{
	vec3 o =  normalize(vec3(-pos3D));
	vec3 i = -lightSource;
	vec3 N =  normalize(normal);

	if(uMode == 0){
		gl_FragColor = CookTorrance(o, i, N);
	} else if(uMode == 1){
		gl_FragColor = reflection(o, N);
	} else if(uMode == 2){
		gl_FragColor = refraction(o, N);
	} else if(uMode == 3){
		gl_FragColor = reflect_refract(o, N);
	} else if(uMode == 4){
		gl_FragColor = frosted_mirror_noFresnel(o, N, uRayAmount);
	} else if(uMode == 5){
		gl_FragColor = frosted_mirror_withFresnel(o, N, uRayAmount);
	}
}
