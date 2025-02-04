# HealthChecksTesting

A simple .net9 worker service that implements health checks for app contingencies around rabbitmq and a postgres database.

Local startup (from Visual Studio as example) expects docker containers to be running on standard ports to simulate Healthy status messages. You'll need to create your own docker containers for RabbitMQ and Postgres to see fully healthy status messages for the app. Alternatively, start the app using docker compose up -d and the app will spin up all dependencies on an internal network outlined below in the Docker section

## Docker

Includes a Dockerfile and docker-compose.yml file for local testing that starts up the health checks app, a rabbitmq container, and a postgres container for example output logs. RabbitMQ and Postgres containers are exposed on the host machine mapped to non-standard ports to avoid container collisions with any running containers on the host machine. The internal container uses a bridge network to allow communication between containers on standard ports
