# HealthChecksTesting

A simple .net9 worker service that implements health checks for app contingencies around rabbitmq and a postgres database.

Local startup (from Visual Studio as example) expects docker containers to be running on standard ports to simulate Healthy status messages. You'll need to create your own docker containers for RabbitMQ and Postgres to see fully healthy status messages for the app. Alternatively, start the app using docker compose up -d and the app will spin up all dependencies on an internal network outlined below in the Docker section

For local setup without using docker compose, something like the following can be ran to create the docker containers for RabbitMQ and Postgres:

```sh
docker run --name postgres -e POSTGRES_PASSWORD=postgres -d postgres
docker run --name rabbitmq -d rabbitmq:4-management
```

## Docker

Includes a Dockerfile and docker-compose.yml file for local testing that starts up the health checks app, a rabbitmq container, and a postgres container for example output logs. RabbitMQ and Postgres containers are exposed on the host machine mapped to non-standard ports to avoid container collisions with any running containers on the host machine. The internal container uses a bridge network to allow communication between containers on standard ports

The compose file also exposes port 5000 to the host machine to enable use of the HealthChecksUI functionality. RabbitMQ management is mapped to port 15673 on the host as well for visiblity.
