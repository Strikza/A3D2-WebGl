# A3D2-WebGl - Maxime Lebreton, Samuel Goubeau

---
# <u>Jalon 1</u>
### Consignes :
- la visualisation interactive d'objets 3D (avec un choix parmi plusieurs objets)
- le développement du modèle de Cook et Torrance pour la réflection lumineuse
- une source de lumière positionnée en 0,0,0 (à la place de l'observateur, donc)
- une interface javascript permettant de jouer sur les paramètres :
    - rugosité (sigma)
    - ni (pour la réflexion Fresnel)
    - Kd (pour la couleur de l'objet)
    - et éventuellement la position de la source (avec la souris)

### Travail réalisé :

- Possibilité de modifier la couleur de l'objet de façon dynamique
- Possibilité de modifier l'objet de façon dynamique, grâce à une liste d'objets présélectionnés
- Possibilité de déplacer la lumière sur les trois axes x, y et z, en cochant/décochant une checkbox :
    - **Clique gauche** déplacera la lumière sur les axes x et y
    - **Shift + clique gauche** ou **la molette de la souris** déplacera la lumière sur son axe z
- Application de la formule de Cook et Torrance :
    - Modification de la rugosité avec un slider (sigma)
    - Modification de l'indice de réfraction avec un slider
    - Choix de la fonction de la distribution D avec un radioGroup (Beckmann ou GGX)


---
# <u>Jalon 2</u>
### Consignes :
- Définissez le cube associé à ses textures pour représenter l'environnement autour de l'objet
- Dans le fragment shader, réalisez les calculs pour une surface miroir / transparente (à partir des équations de Fresnel)
- avec une case à cocher pour choisir entre miroir et transparence
- et un slider pour choisir interactivement la valeur de ni

### Travail réalisé :

- Ajout d'une skybox environnementale
- Visualisation des effets de réflection et de réfraction entre l'objet et la skybox
- Utilisation d'un toggle-switch pour changer entre la réflection et la réfraction dynamiquement