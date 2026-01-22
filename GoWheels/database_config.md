# For Windows

Make sure to have `docker-desktop` and `WSL` installed

- To run a docker container 
    - Open WSL
    - `cd gowheels-db`
    - `docker compose up -d`


- Test :
  - `docker ps`


- To drop a container and delete its volume :
  - `docker compose down -v`

# For LINUX

Make sure to have `docker.io` and `docker-compose-v2` installed (use `apt`)

- To run a docker container :
    - `cd gowheels-db`
    - `sudo docker compose up -d`


- Test :
    - `sudo docker ps`


- To drop a container and delete its volume :
    - `sudo docker compose down -v`
