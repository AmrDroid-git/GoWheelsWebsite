# 🚀 GoWheels – Docker Deployment Guide (No Repository Cloning)

This document explains how to run the **GoWheels** application using **Docker and Docker Compose**, without cloning the GitHub repository or building the project locally.

The application and its PostgreSQL database are deployed automatically using prebuilt Docker images.

---

## ✅ Prerequisites

* Docker installed on the machine
  👉 [https://docs.docker.com/get-docker/](https://docs.docker.com/get-docker/)

Verify installation:

```bash
docker --version
docker compose version
```

---

## 📦 Step 1 — Download `docker-compose.yml`

The deployment is managed using a single `docker-compose.yml` file.

👉 **Direct download link**:

```
https://github.com/AmrDroid-git/GoWheelsWebsite/blob/App_ops_docker/GoWheels/docker-compose.yml
```

### Download via terminal (Linux / macOS / WSL)

```bash
curl -O https://github.com/AmrDroid-git/GoWheelsWebsite/blob/App_ops_docker/GoWheels/docker-compose.yml
```

Or simply:

* Right click → **Save As…**

📌 The GitHub repository does **not** need to be cloned.

---

## 📦 Step 2 — Pull the Application Image from Docker Hub

```bash
docker pull DOCKERHUB_USERNAME/gowheels-app:latest
```

Example:

```bash
docker pull meddev/gowheels-app:latest
```

---

## ▶️ Step 3 — Start the Application (Single Command)

From the directory containing `docker-compose.yml`:

```bash
docker compose up -d
```

⏳ The first startup may take 30–60 seconds while PostgreSQL initializes.

---

## 🌍 Access the Application

Open a browser and navigate to:

```
http://localhost:8080
```

---

## ⏹️ Stop the Application

```bash
docker compose down
```

---

## 🗑️ Remove Application and Database (Full Cleanup)

```bash
docker compose down -v
```

---

## 🧱 Docker Architecture

* **gowheels-app**
  ASP.NET Core application (.NET 8)

* **gowheels_postgres**
  PostgreSQL 15 database

* PostgreSQL data is persisted using a Docker volume

* The database container starts automatically before the application

---

## ❓ Frequently Asked Questions

### ❓ Is cloning the GitHub repository required?

**No.**
The application is provided as a ready-to-run Docker image via Docker Hub.

---

### ❓ Is local compilation required?

**No.**
The image is already built and published.

---

### ❓ How can the application be restarted later?

```bash
docker compose up -d
```

---

### ❓ Which ports are used?

* Application: **8080**
* PostgreSQL: **5432** (optional, for inspection)

---

## 🧪 Optional Checks

List running containers:

```bash
docker ps
```

View application logs:

```bash
docker logs gowheels-app
```

---

## 🎓 Evaluation Summary

✔ Only 2 commands required
✔ No source code needed
✔ Fully reproducible deployment
✔ Database starts automatically
✔ Follows modern DevOps practices

---

## 📌 Academic Notes

* Database credentials are provided for demonstration purposes
* In a production environment, credentials would be externalized using environment variables or secrets

---

## ✅ Conclusion

This deployment approach allows the **GoWheels** application to be executed quickly and reliably, without manual configuration or repository cloning, ensuring a smooth and reproducible evaluation process.

