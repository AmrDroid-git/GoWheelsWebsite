# Guide de configuration de la base de données – GoWheels



## 1️⃣ Prérequis (selon votre OS)

---

### 🐧 Linux (Ubuntu, Debian, Arch…)

Vous devez avoir :

* Docker
* Docker Compose (plugin ou binaire)
* .NET SDK (version du `global.json`)
* Git

Vérification :

```bash
docker --version
docker compose version
dotnet --version
```

---

### 🪟 Windows (IMPORTANT)

Sur Windows, **Docker doit impérativement utiliser WSL 2**.

#### Obligatoire :

* Windows 10/11
* WSL 2 installé
* Docker Desktop
* Distribution Linux (Ubuntu recommandée)

Vérification dans PowerShell :

```powershell
wsl -l -v
```

Vous devez voir :

```text
Ubuntu    Running    2
```

⚠️ **Toutes les commandes Docker et dotnet doivent être exécutées dans le terminal Ubuntu (WSL)**
❌ Pas dans PowerShell
❌ Pas dans CMD

---

## 2️⃣ Règle importante sur les migrations (pour TOUS)

* ❌ Ne JAMAIS exécuter `dotnet ef migrations add`
* ✔️ Les migrations sont déjà dans le dépôt Git
* ✔️ Vous devez uniquement appliquer les migrations existantes

---

## 3️⃣ Première installation (Linux & Windows)

### 📍 Placez-vous à la racine du projet

```bash
cd GoWheelsWebsite
```

---

### Étape 1 – Démarrer PostgreSQL (Docker)

```bash
docker compose up -d
```

➡️ Cette commande :

* démarre PostgreSQL
* crée la base `gowheels_db`
* exécute automatiquement le script SQL d’initialisation
* **remplit la base avec des données**

Pour Windows :
    - lanceer docker desktop
    - lancer wsl dans le dossier du projet (/GoWheelsWebsite/GoWheels)
    - puis executer dans wsl les commandes de docker normalement

⏳ (premier lancement : 10–20 secondes)

---

### Étape 2 – Appliquer les migrations EF Core

```bash
dotnet ef database update
```

➡️ Cette commande :

* synchronise la base avec les modèles C#
* applique uniquement les migrations manquantes
* **ne supprime aucune donnée**

---

### Étape 3 – Lancer l’application

```bash
dotnet run
```

🎉 L’application est maintenant connectée à une base **remplie et fonctionnelle**.

---

## 4️⃣ Démarrage normal (après la première fois)

### Linux & Windows (WSL)

```bash
git pull
docker compose up -d
dotnet ef database update
dotnet run
```

⚠️ Docker doit être en cours d’exécution.

---

## 5️⃣ Cas spécifique Windows (erreurs fréquentes)

### ❌ Erreur : Docker tourne mais l’app ne se connecte pas

➡️ Vérifiez que :

* Docker Desktop est lancé
* Vous êtes **dans Ubuntu (WSL)**

```bash
uname -a
```

Doit afficher `Linux`.

---

### ❌ Erreur : `dotnet ef` ne trouve pas la DB

➡️ Docker n’est pas démarré ou mauvais terminal.

Solution :

```bash
docker compose ps
```

---

## 6️⃣ Reset complet de la base (Linux & Windows)

À utiliser **uniquement si demandé** :

```bash
docker compose down -v
docker compose up -d
dotnet ef database update
dotnet run
```

⚠️ `-v` supprime totalement la base et la recrée.

---

## 7️⃣ Ce qu’il ne faut PAS faire (très important)

❌ Lancer Docker dans PowerShell et dotnet dans WSL
❌ Créer des migrations
❌ Modifier la base manuellement
❌ Utiliser PostgreSQL local
❌ Changer la chaîne de connexion

---

## 8️⃣ Ordre obligatoire (résumé)

```text
1. docker compose up -d
2. dotnet ef database update
3. dotnet run
```

---

## 9️⃣ En cas de doute

➡️ **Contactez le responsable DB**
Ne tentez pas de corriger la base vous-même.

---

✔ Base identique pour toute l’équipe
✔ Compatible Linux / Windows
✔ Aucun conflit de migration
✔ Démarrage fiable

Merci de respecter ce guide.
