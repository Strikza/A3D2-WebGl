
precision mediump float;

uniform vec3  uLightSource;
uniform vec3  uMaterial;
uniform float uSigma;
uniform float uRefract;
uniform float uRatioMT;
uniform int   uDistrib;
uniform int   uMode;
uniform int   uRayAmount;
uniform float uBrightness;

uniform samplerCube uSampler;

varying vec4 pos3D;
varying vec3 normal;
varying mat4 vRMatrix;

float PI = 3.1415;

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
	return fract(sin(dot(pos3D.xy + seed, vec2(12.9898, 78.233))) * 43758.5453123);
}

// ==============================================
mat3 getLocalRotationMatrix(vec3 N){
	vec3 newRMatrix = vec3(1.0, 0.0, 0.0);

	if (dot(N, newRMatrix) > 0.9){
		newRMatrix = vec3(0.0, 1.0, 0.0);
	}

    vec3 j = cross(N,newRMatrix);
    vec3 i = cross(j,N);

    return mat3(i,j,N);
}

// ==============================================
vec3 getMicroNormal(vec3 N){
	float phi;
	float theta;
	float xi1 = getRandom();
	float xi2 = getRandom();

	if(uDistrib == 0){
		phi   = xi1 * 2.0 * PI;                               // => Microfacette pour Backmann
		theta = atan( sqrt((-square(uSigma)) * log(1.0 - xi2)) ); //
	}
	else{
		phi   = xi1 * 2.0 * PI;                          // => Microfacette pour Walter GGX
		theta = atan( (uSigma*sqrt(xi2)/(sqrt(1.0 - xi2)))); //
	}

    float x = sin(theta)*cos(phi);
    float y = sin(theta)*sin(phi);
    float z = cos(theta);

    vec3 m = normalize(getLocalRotationMatrix(N)*vec3(x, y, z));

    return m;
}

// BRDF - BTDF - BSDF
// ==============================================
float Fresnel(vec3 i, vec3 m){

	float c = abs(dot(i, m));
	float g = sqrt(uRefract*uRefract + c*c - 1.0);

	float gPc = g+c;
	float gLc = g-c;

	return 0.5 * (square(gLc)/square(gPc)) * (1.0 + square(c * gPc - 1.0 ) / square(c * gLc + 1.0));
}

// ==============================================
float D_beckmann(vec3 m, vec3 N){
	
	float cosTheta  = clampedDot(N, m);
	float cosTheta2 = square(cosTheta);
	float tanTheta2 = (1.0 - cosTheta2)/cosTheta2;
	float sigma2    = square(uSigma);

	float e = exp(-(tanTheta2) / (2.0*(sigma2)));
	float denom = 3.14*sigma2*square(cosTheta2);

	return 1.0/denom * e;
}

// ==============================================
float D_walter_ggx(vec3 m, vec3 N){

	float cosTheta  = dot(N, normalize(m));
	float cosTheta2 = square(cosTheta);
	float tanTheta2 = (1.0 - cosTheta2)/cosTheta2;
	float sigma2    = square(uSigma);

	float denom = 3.14 * square(cosTheta2) * square((sigma2 + tanTheta2));

	return sigma2/denom;
}

// ==============================================
float G_vcavite(vec3 m, vec3 N, vec3 i, vec3 o){

	float nm = dot(N, m);

	float Go = 2.0*nm*dot(N, o) / dot(o, m);
	float Gi = 2.0*nm*dot(N, i) / dot(i, m);

	return min(1.0, min(Go, Gi));
}

// ==============================================
float G_beckmann(vec3 m, vec3 N){

	float cosTheta  = dot(N, normalize(m));
	float cosTheta2 = square(cosTheta);
	float tanTheta  = sqrt((1.0 - cosTheta2)/cosTheta2);

	float a = 1.0/(uSigma*tanTheta);

	if(a < 1.6){
		return (3.535*a + 2.181*square(a))/(1.0 + 2.276*a + 2.577*square(a));
	}
	else{
		return 1.0;
	}
}

// ==============================================
float G_walter_ggx(vec3 m, vec3 N){

	float nm = dot(N, m);
	if(nm < 0.2) return 0.0;

	float cosTheta2 = square(nm);
	float tanTheta2 = (1.0 - cosTheta2)/cosTheta2;
	float sigma2    = square(uSigma);

	return 2.0/(1.0 + sqrt(1.0 + sigma2*tanTheta2));
}

// ==============================================
float BRDF(vec3 i, vec3 o, vec3 hr, vec3 N){
	float brdf = 0.0;

	float iN = clampedDot(i, N);
	float oN = clampedDot(o, N);

	const float margin = 0.001;

	if(iN > margin && oN > margin){
		float F = Fresnel(i, hr);
		float D;
		float G;
		if(uDistrib == 0){
			D = D_beckmann(hr, N);
			G = G_beckmann(hr, N);
			// G = G_vcavite(hr, N, i, o); // Masquage passe-partout, mais incorrect
		}
		else{
			D = D_walter_ggx(hr, N);
			G = G_walter_ggx(hr, N);
		}

		brdf = (F*D*G) / (4.0*iN*oN);
	}

	return brdf;
}

// ==============================================
float BTDF(vec3 i, vec3 o, vec3 ht, vec3 N){

	float btdf = 0.0;

	float iHt = clampedDot(i, ht);
	float iN  = clampedDot(i, N);
	float oHt = clampedDot(o, ht);
	float oN  = clampedDot(o, N);

	float d_iHt = dot(i, ht);
	float d_oHt = dot(o, ht);

	const float margin = 0.001;

	if(iN  > margin && 
	   oN  > margin && 
	   iHt > margin && 
	   oHt > margin && 
	   d_iHt > margin && 
	   d_oHt > margin
	   ){
		float F = Fresnel(i, ht);
		float D;
		float G;
		if(uDistrib == 0){
			D = D_beckmann(ht, N);
			G = G_beckmann(ht, N);
			// G = G_vcavite(ht, N, i, o); // Masquage passe-partout, mais incorrect
		}
		else{
			D = D_walter_ggx(ht, N);
			G = G_walter_ggx(ht, N);
		}

		btdf = (iHt*oHt / iN*oN) * ((square(uRefract) * (1.0 - F)*G*D) / square(d_iHt + uRefract*d_oHt));
	}

	return btdf;
}


// COOK & TORRANCE
// ==============================================
vec4 CookTorrance(vec3 o, vec3 i, vec3 N){

	vec3 m  = normalize(o + i);
	vec3 colorCT = vec3(BRDF(i, o, m, N));

	vec3 col = uMaterial * dot(N, i) + colorCT/PI; // Lambert rendering with Cook & Torrance
	
	return vec4(col, 1.0);
}



// REFLECT & REFRACT
// ==============================================
vec4 reflection(vec3 o, vec3 u){
    vec3 r = reflect(-o, u);
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



// FROSTED MIRRORS
// ==============================================
vec4 frosted_mirror_noFresnel(vec3 o, vec3 N, int rayAmount){
	vec4 Lo = vec4(0.0);

	for (int j=0; j<100; ++j) {
		if (j >= rayAmount) break;
		
        vec3 m = getMicroNormal(N);
        Lo += reflection(o, m);
	}
	return Lo / float(rayAmount);
}

// ==============================================
vec4 frosted_mirror_withFresnel(vec3 o, vec3 N, int rayAmount){
	vec3 Lo = vec3(0.0);
    int rayCpt = 0;

	for (int j=0; j<100; ++j) { 
		if (j >= rayAmount) break;

        vec3 m = getMicroNormal(N);
        vec3 i = reflect(-o, m);
		
		float mN = clampedDot(m, N);
        float iN  = clampedDot(i, N);

		const float margin = 0.001;

        if(mN < margin || iN < margin) continue;

		float D;  // Recalculé pour la pdf, même si déjà calculé pour la BRDF
		if(uDistrib == 0){
			D = D_beckmann(m, N);
		}
		else{
			D = D_walter_ggx(m, N);
		}
		
        vec3  Li  = reflection(o, m).xyz*uBrightness;
        float pdf = mN*D;

        Lo += Li*BRDF(i, o, m, N)*iN/pdf;

        rayCpt++;
    }
	
	return vec4(Lo / float(rayCpt), 1.0);
}

// ==============================================
vec4 frosted_refraction(vec3 o, vec3 N, int rayAmount){
	vec3 Lo = vec3(0.0);
    int rayCpt = 0;

	for (int j=0; j<100; ++j) { 
		if (j >= rayAmount) break;

        vec3 m = getMicroNormal(N);
        vec3 i = reflect(-o, m);
		
		float mN  = clampedDot(m, N);
        float iN  = clampedDot(i, N);

		const float margin = 0.001;

        if(iN < margin || mN < margin ) continue;

		float D;
		if(uDistrib == 0){
			D = D_beckmann(m, N);
		}
		else{
			D = D_walter_ggx(m, N);
		}
		
        vec3  Li  = refraction(o, m).xyz*uBrightness;
        float pdf = mN*D;
		float BSDF = BTDF(i, o, m, N) + BRDF(i, o, m, N);

        Lo += Li*BSDF/pdf;

        rayCpt++;
    }
	
	return vec4(Lo / float(rayCpt), 1.0);
}



// ==============================================
void main(void)
{
	vec3 o =  normalize(vec3(-pos3D));
	vec3 i = -normalize(uLightSource);;
	vec3 N =  normalize(normal);
	vec4 col;

	if(uMode == 0){
		col = CookTorrance(o, i, N);
	} else if(uMode == 1){
		col = reflection(o, N);
	} else if(uMode == 2){
		col = refraction(o, N);
	} else if(uMode == 3){
		col = reflect_refract(o, N);
	} else if(uMode == 4){
		col = frosted_mirror_noFresnel(o, N, uRayAmount);
	} else if(uMode == 5){
		col = frosted_mirror_withFresnel(o, N, uRayAmount);
	} else if(uMode == 6){
		col = frosted_refraction(o, N, uRayAmount);
	}

	gl_FragColor = col;
}
