
precision mediump float;

uniform vec3  uLightSource;
uniform vec3  uMaterial;
uniform float uSigma;
uniform float uRefract;
uniform int   uDistrib;
uniform int   uMode;
uniform int   uRayAmount;
uniform float uBrightness;

uniform samplerCube uSampler;

varying vec4 pos3D;
varying vec3 normal;
varying mat4 vRMatrix;

float PI = 3.1415;



///////////////////////////
// FONCTIONS UTILITAIRES //
///////////////////////////

// Met au carré le flottant en paramètre
// ==============================================
float square(float f){
	return f*f;
}

// Renvoie le max entre 0 et le produit scalaire des vecteurs en paramètres
// ==============================================
float clampedDot(vec3 a, vec3 b){
	return max(0.0, dot(a, b));
}

// Génère un nombre pseudo-aléatoire entre 0 et 1 (structuration dû pos3D)
// ==============================================
float seed = 0.0;

float getRandom(){
	seed++;
	return fract(sin(dot(pos3D.xy + seed, vec2(12.9898, 78.233))) * 43758.5453123);
}

// Récupère la matrice de rotaion locale selon le vecteur en paramètre
// ==============================================
mat3 getLocalRotationMatrix(vec3 N){
	vec3 i_temp = vec3(1.0, 0.0, 0.0);

	if (dot(N, i_temp) > 0.9){
		i_temp = vec3(0.0, 1.0, 0.0);
	}

    vec3 j = cross(N, i_temp);
    vec3 i = cross(j, N);

    return mat3(i,j,N);
}

// Renvoie la normal de la microfacette via un échantillonnage d'importance
// ==============================================
vec3 getMicroNormal(vec3 N){
	float phi;
	float theta;
	float xi1 = getRandom();
	float xi2 = getRandom();

	if(uDistrib == 0){
		phi   = xi1 * 2.0 * PI;                                   // => Microfacette pour Beckmann
		theta = atan( sqrt((-square(uSigma)) * log(1.0 - xi2)) ); //
	}
	else{
		phi   = xi1 * 2.0 * PI;                              // => Microfacette pour Walter GGX
		theta = atan( (uSigma*sqrt(xi2)/(sqrt(1.0 - xi2)))); //
	}

    float x = sin(theta)*cos(phi);
    float y = sin(theta)*sin(phi);
    float z = cos(theta);

	vec3 m = vec3(x, y, z);

    return normalize(getLocalRotationMatrix(N)*m);
}



////////////////////////
// BRDF - BTDF - BSDF //
////////////////////////

// Renvoie la valeur de fresnel selon les vecteurs en paramètres
// ==============================================
float Fresnel(vec3 i, vec3 m){

	float c = abs(dot(i, m));
	float g = sqrt(uRefract*uRefract + c*c - 1.0);

	float gPc = g+c;
	float gLc = g-c;

	return 0.5 * (square(gLc)/square(gPc)) * (1.0 + square(c * gPc - 1.0 ) / square(c * gLc + 1.0));
}

// Renvoie la valeur de la distribution de Beckmann selon les vecteurs en paramètres
// ==============================================
float D_beckmann(vec3 m, vec3 N){
	
	float cosTheta  = clampedDot(N, m);
	float cosTheta2 = square(cosTheta);
	float tanTheta2 = (1.0 - cosTheta2)/cosTheta2;
	float sigma2    = square(uSigma);

	float e = exp(-(tanTheta2) / sigma2);
	float denom = 3.14*sigma2*square(cosTheta2);

	return 1.0/denom * e;
}

// Renvoie la valeur de la distribution de WalterGGX selon les vecteurs en paramètres
// ==============================================
float D_walter_ggx(vec3 m, vec3 N){

	float cosTheta  = dot(N, normalize(m));
	float cosTheta2 = square(cosTheta);
	float tanTheta2 = (1.0 - cosTheta2)/cosTheta2;
	float sigma2    = square(uSigma);

	float denom = 3.14 * square(cosTheta2) * square((sigma2 + tanTheta2));

	return sigma2/denom;
}

// Renvoie la valeur du masquage VCavité selon les vecteurs en paramètres
// -> C'est un masquage passe-partout, mais incorrect
// ==============================================
float G_vcavite(vec3 m, vec3 N, vec3 i, vec3 o){

	float nm = dot(N, m);

	float Go = 2.0*nm*dot(N, o) / dot(o, m);
	float Gi = 2.0*nm*dot(N, i) / dot(i, m);

	return min(1.0, min(Go, Gi));
}

// Renvoie la valeur du masquage de Beckmann selon les vecteurs en paramètres
// -> Fonction réalisée avec l'article EGSR07
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

// Renvoie la valeur du masquage de WalterGGX selon les vecteurs en paramètres
// -> Fonction réalisée avec l'article EGSR07
// ==============================================
float G_walter_ggx(vec3 m, vec3 N){

	float nm = dot(N, m);
	if(nm < 0.2) return 0.0;

	float cosTheta2 = square(nm);
	float tanTheta2 = (1.0 - cosTheta2)/cosTheta2;
	float sigma2    = square(uSigma);

	return 2.0/(1.0 + sqrt(1.0 + sigma2*tanTheta2));
}



/////////////////////
// COOK & TORRANCE //
/////////////////////

// Calcul de l'éclairement selon Cook&Torrance
// ==============================================
vec4 CookTorrance(vec3 o, vec3 i, vec3 N){

	vec3 m     = normalize(o + i);
	float brdf = 0.0;

	float iN = clampedDot(i, N);
	float oN = clampedDot(o, N);
	const float margin = 0.001;

	if(iN > margin && oN > margin){
		float F = Fresnel(i, m);
		float D;
		float G;
		if(uDistrib == 0){
			D = D_beckmann(m, N);
			G = G_beckmann(m, N);
		}
		else{
			D = D_walter_ggx(m, N);
			G = G_walter_ggx(m, N);
		}

		brdf = (F*D*G) / (4.0*iN*oN);
	}
	vec3 colorCT = vec3(brdf);

	vec3 col = uMaterial * dot(N, i) + colorCT/PI; // Rendu de Lambert avec Cook & Torrance
	
	return vec4(col, 1.0);
}



///////////////////////
// REFLECT & REFRACT //
///////////////////////

// Réflection de la skybox selon les vecteurs en paramètres
// -> Utilisée pour le mirroir
// ==============================================
vec4 reflection(vec3 o, vec3 u){
    vec3 r = reflect(-o, u);
    r = vec3(vRMatrix * vec4(r, 1.0));

    return textureCube(uSampler, r.xzy);
}

// Réfraction de la skybox selon les vecteurs en paramètres
// -> Utilisée pour la transparence
// ==============================================
vec4 refraction(vec3 o, vec3 N){
    vec3 r = refract(-o, N, 1.0/uRefract);
    r = vec3(vRMatrix * vec4(r, 1.0));

    return textureCube(uSampler, r.xzy);
}

// Réflection et Réfraction selon un facteur fresnel
// ==============================================
vec4 reflect_refract_fresnel(vec3 o, vec3 N){

	vec3 i = reflect(-o, N);
	float fresnel = Fresnel(i, N);

	vec4 Li = reflection(o, N);
	vec4 Lt = refraction(o, N);

	return Li*fresnel + Lt*(1.0 - fresnel);
}



///////////////////////////////////
// FROSTED MIRRORS & REFRACTIONS //
///////////////////////////////////

// Simple mirroir dépoli avec échantillonnage d'importance, et sans autre facteur
// ==============================================
vec4 frosted_mirror(vec3 o, vec3 N, int rayAmount){
	vec4 Lo = vec4(0.0);

	for (int j=0; j<100; ++j) {
		if (j >= rayAmount) break;
		
        vec3 m = getMicroNormal(N);

        Lo += reflection(o, m);
	}
	return Lo / float(rayAmount);
}

// Mirroir dépoli avec échantillonnage d'importance, utilisant la BRDF
// ==============================================
vec4 frosted_mirror_withBRDF(vec3 o, vec3 N, int rayAmount){
	vec3 Lo = vec3(0.0);
    int rayCpt = 0;

	for (int j=0; j<100; ++j) { 
		if (j >= rayAmount) break;

        vec3 m = getMicroNormal(N);
        vec3 i = reflect(-o, m);
		
		float mN = clampedDot(m, N);
        float iN = clampedDot(i, N);
		float oN = clampedDot(o, N);
		const float margin = 0.01;

        if(
			mN < margin || 
			iN < margin || 
			oN < margin
		) continue;

		float F = Fresnel(i, m);
		float D;
		float G;
		if(uDistrib == 0){
			D = D_beckmann(m, N);
			G = G_beckmann(m, N);
		}
		else{
			D = D_walter_ggx(m, N);
			G = G_walter_ggx(m, N);
		}

		float brdf = (F*D*G) / (4.0*iN*oN);
        vec3  Li   = reflection(o, m).xyz*uBrightness;
        float pdf  = mN*D;

        Lo += Li*brdf*iN/pdf;

        rayCpt++;
    }
	
	return vec4(Lo / float(rayCpt), 1.0);
}

// Simple transparence dépolie avec échantillonnage d'importance, sans autre facteur
// ==============================================
vec4 frosted_refraction(vec3 o, vec3 N, int rayAmount){
	vec3 Lo = vec3(0.0);
    int rayCpt = 0;

	for (int j=0; j<100; ++j) { 
		if (j >= rayAmount) break;

        vec3 m = getMicroNormal(N);

        vec3 Li = refraction(o, m).xyz;

        Lo += Li;

        rayCpt++;
    }
	
	return vec4(Lo / float(rayCpt), 1.0);
}

// Transparence dépolie avec échantillonnage d'importance, utilisant la BSDF
// -> Fonction réalisée avec l'article EGSR07
// ==============================================
vec4 frosted_refraction_withBSDF(vec3 o, vec3 N, int rayAmount){
	vec3 Lo = vec3(0.0);
    int rayCpt = 0;

	for (int j=0; j<100; ++j) { 
		if (j >= rayAmount) break;

        vec3 m = getMicroNormal(N);
        vec3 i = reflect(-o, m);
		
		float mN = clampedDot(m, N);
		float iM = clampedDot(i, m);
        float iN = clampedDot(i, N);
		float oM = clampedDot(o, m);
		float oN = clampedDot(o, N);
		const float margin = 0.01;

        if(
			mN < margin || 
			iM < margin || 
			iN < margin || 
			oM < margin || 
			oN < margin
		) continue;

		float F = Fresnel(i, m);
		float D;
		float G;
		if(uDistrib == 0){
			D = D_beckmann(m, N);
			G = G_beckmann(m, N);
		}
		else{
			D = D_walter_ggx(m, N);
			G = G_walter_ggx(m, N);
		}

		float brdf = (F*D*G) / (4.0*iN*oN);
		vec3  Li   = reflection(o, m).xyz*uBrightness;

		float btdf = (iM*oM / iN*oN) * ((square(uRefract) * (1.0 - F)*G*D) / square(iM + uRefract*oM));
        vec3  Lt   = refraction(o, m).xyz*uBrightness;

		vec3  BSDF = Li*brdf*iN + Lt*btdf*iN;
        float pdf  = mN*D;

        Lo += BSDF/pdf;

        rayCpt++;
    }
	
	return vec4(Lo / float(rayCpt), 1.0);
}



//////////
// MAIN //
//////////

void main(void)
{
	vec3 o =  normalize(vec3(-pos3D));
	vec3 i = -normalize(uLightSource);
	vec3 N =  normalize(normal);
	vec4 col;

	if(uMode == 0){
		col = CookTorrance(o, i, N);
	} else if(uMode == 1){
		col = reflection(o, N);
	} else if(uMode == 2){
		col = refraction(o, N);
	} else if(uMode == 3){
		col = reflect_refract_fresnel(o, N);
	} else if(uMode == 4){
		col = frosted_mirror(o, N, uRayAmount);
	} else if(uMode == 5){
		col = frosted_mirror_withBRDF(o, N, uRayAmount);
	} else if(uMode == 6){
		col = frosted_refraction(o, N, uRayAmount);
	} else if(uMode == 7){
		col = frosted_refraction_withBSDF(o, N, uRayAmount);
	}

	gl_FragColor = col;
}
