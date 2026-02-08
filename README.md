
# GoWheels

Vehicle listings and reviews platform built with .NET 8.0 and Docker.

## Setup

### 1. Clone
```bash
git clone https://github.com/AmrDroid-git/GoWheelsWebsite.git
cd GoWheelsWebsite
```

### 2. Database
Run the PostgreSQL container:

```bash
cd GoWheels-Database
# Windows/WSL
docker compose up -d
# Linux
sudo docker compose up -d
```

### 3. Run
From the root folder:

```bash
dotnet restore
dotnet run --project GoWheels
```

App URL: `http://localhost:5237`

## Accounts

| Role | Email | Password |
| :--- | :--- | :--- |
| Admin | `admin@gowheels.local` | `Password123!` |
| Expert | `expert@gowheels.local` | `Password123!` |
| User | `user@gowheels.local` | `Password123!` |

## Notes

### Persistence
By default, the database is wiped and re-seeded on every start. To keep your data, edit `GoWheels/appsettings.Development.json` and set `RemakeDatabase` to `false`.

```json
"DatabaseSettings": {
  "RemakeDatabase": false
}
```

### Troubleshooting
If it won't connect:
1. Check if the container is up: `docker ps`
2. Reset everything (wipes data):
   ```bash
   cd GoWheels-Database
   docker compose down -v
   ```
