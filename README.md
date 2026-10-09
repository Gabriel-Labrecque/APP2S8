# APP2S8

Site web Sanssoussi (ASP.NET Core, .NET 10), sécurisé selon le Top 10 de l'OWASP 2025.

## Lancer le projet

Seul Docker est requis.

```bash
docker compose up --build
```

Ouvrir ensuite https://localhost:5001. Le navigateur affiche un avertissement de certificat, ce qui est normal : le certificat est auto-signé.

Pour arrêter le site :

```bash
docker compose down
```

La base de données et les journaux sont conservés dans le dossier `data/`. Pour repartir d'une base vide, supprimer `data/Sanssoussi.db`.

## Connexion avec Google

Créer un fichier `.env` à côté de `docker-compose.yml` :

```
GOOGLE_CLIENT_ID=ton_id
GOOGLE_CLIENT_SECRET=ton_secret
```

Ces deux valeurs viennent d'un « ID client OAuth » de type « Application Web » créé dans la console Google Cloud, avec l'URI de redirection `https://localhost:5001/signin-google`.

Le fichier `.env` est ignoré par git : il ne faut jamais le pousser sur GitHub. Sans ce fichier, des valeurs factices sont utilisées et la connexion par Google échoue ; la connexion par courriel et mot de passe fonctionne quand même.
