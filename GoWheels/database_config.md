# For Windows
## for First Time
  - open docker desktop
  - open wsl in terminal 
  - dans wsl : 
    - \GoWheelsWebsite> cd gowheels-db
    - /gowheels-db$ docker compose up -d
    - Pour vérifier que docker a bien lancé:
      - docker ps
    - /gowheels-db$ cd ..
    - cd GoWheels 
    - \GoWheelsWebsite\GoWheels> dotnet ef database update
    - cd ..
    - cd gowheels-db
    - cd init
    - docker exec -i gowheels_postgres psql   -U gowheels_user -d gowheels_db < seed_users_and_posts.sql
      - (il doit appraitre :
      INSERT 0 1 
      INSERT 0 1
      INSERT 0 1
      INSERT 0 1
      INSERT 0 1
      INSERT 0 1
      INSERT 0 1
      INSERT 0 1
      INSERT 0 1
      COMMIT)
    - Pour entrer la db :
      - docker exec -i gowheels_postgres psql   -U gowheels_user -d gowheels_db 
    - Pour visualiser les données à partir du cli:
      - \dt
      - SELECT COUNT(*) FROM "AspNetUsers";
      - SELECT COUNT(*) FROM "Posts";
    - To restart a container and delete its volume
      - docker compose down -v
## Then
  - all u need is : \GoWheelsWebsite\GoWheels> dotnet ef database update
  #à chaque ajout de migration , bien sûr, the container should be running
    -  !!! jamais faire : dotnet ef migrations add

# For LINUX
- \GoWheelsWebsite> cd gowheels-db
- /gowheels-db$ docker compose up -d
- Pour vérifier que docker a bien lancé:
  - docker ps
- /gowheels-db$ cd ..
- cd GoWheels
- \GoWheelsWebsite\GoWheels> dotnet ef database update
  - (il didn't work , open another terminal in the same folder , and reexecute the command (HORS WSL))
- cd ..
- cd gowheels-db
- cd init
- docker exec -i gowheels_postgres psql   -U gowheels_user -d gowheels_db < seed_users_and_posts.sql
  - (il doit appraitre :
    INSERT 0 1
    INSERT 0 1
    INSERT 0 1
    INSERT 0 1
    INSERT 0 1
    INSERT 0 1
    INSERT 0 1
    INSERT 0 1
    INSERT 0 1
    COMMIT)
- Pour entrer la db :
  - docker exec -i gowheels_postgres psql   -U gowheels_user -d gowheels_db
- Pour visualiser les données à partir du cli:
  - \dt
  - SELECT COUNT(*) FROM "AspNetUsers";
  - SELECT COUNT(*) FROM "Posts";
## To restart a container and delete its volume
  - docker compose down -v
## Pour visualiser les données dans rider
    - va à droite et cliquer sur l'icone de database
    - cliquer sur New (se trouve en haut , 2nd icon)
    - cliquer sur Connect to Database
    - choisir ,Database type: PostgreSQL
    - cliquer sur "Connect to Database"
    - attend un peu et c bon ,
    - cliquer sur gowheels_db@localhost
    - va vers gowheels_db/public/tables/Posts
    - cliquer sur Posts double fois et un nouveau onglet va s'ouvrir , contient un tableau avec les données 

