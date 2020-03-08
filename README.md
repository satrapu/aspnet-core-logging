# aspnet-core-logging

## Description

This repo shows ASP.NET Core v3.x logging in action; it also serves as a learning, experimenting and teaching path for [Azure Pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/).  
This project has several posts associated with it:

- [Use Docker when running integration tests with Azure Pipelines](https://crossprogramming.com/2019/12/27/use-docker-when-running-integration-tests-with-azure-pipelines.html)
- [Build an ASP.NET Core application using Azure Pipelines](https://crossprogramming.com/2019/03/17/build-asp-net-core-app-using-azure-pipelines.html)
- [Logging HTTP context in ASP.NET Core](https://crossprogramming.com/2018/12/27/logging-http-context-in-asp-net-core.html)

## Setup local persistence services
This ASP.NET Core web API uses [PostgreSQL](https://www.postgresql.org/) as persistent storage and [pgadmin](https://www.pgadmin.org/) as database manager, all running locally via [Docker Compose](https://github.com/docker/compose).

### Create Docker volumes
These volumes are needed to store data outside the Docker containers running the PostgreSQL databases and their manager.

- Volume used by the local development database
```bash
docker volume create --name=aspnet-core-logging-dev_data
```

- Volume to be targeted by the integration tests when run locally

```bash
docker volume create --name=aspnet-core-logging-it_data
```

- Volume to be used by pgadmin tool
```bash
docker volume create --name=pgadmin_data
```

### Prepare .env file
The [.env](https://docs.docker.com/compose/env-file/) file is used by Docker Compose to avoid storing sensitive data inside `docker-compose.yml` file.  
Create a new file named `.env` inside the folder where you have checked-out this git repository and add the following lines:
```properties
# Environment variables used by 'aspnet-core-logging-dev' service
DB_DEV_POSTGRES_USER=<DB_DEV_USER>
DB_DEV_POSTGRES_PASSWORD=<DB_DEV_PASSWORD>

# Environment variables used by 'aspnet-core-logging-it' service
DB_IT_POSTGRES_USER=<DB_IT_USER>
DB_IT_POSTGRES_PASSWORD=<DB_IT_PASSWORD>

# Environment variables used by 'pgadmin' service
PGADMIN_DEFAULT_EMAIL=<PGADMIN_EMAIL_ADDRESS>
PGADMIN_DEFAULT_PASSWORD=<PGADMIN_PASSWORD>
```

Make sure you replace all of the above `<DB_DEV_USER>`, `<DB_DEV_PASSWORD>`, ..., `<PGADMIN_PASSWORD>` tokens with the appropriate values.

### Compose commands
All of the commands below must be run from the folder where you have checked-out this git repository.  
This folder contains a `docker-compose.yml` file describing the compose services mention below.

#### Run compose services
```bash
# The -d flag instructs Docker Compose to run services in the background
docker-compose up -d
```

#### Stop compose services
```bash
docker-compose stop
```

#### Start compose services
```bash
docker-compose start
```

#### Display compose service log
```bash
# The -f flag instructs Docker Compose to display and follow the log entries of the 'pgadmin' service
docker-compose logs -f pgadmin
```

#### Destroy compose services
The command below will **not** delete the Docker volumes!
```bash 
docker-compose down
```

## Build

| Build Server                                                                    | Operating System | Status                                                                                                                                                                                                                                                          |
| ------------------------------------------------------------------------------- | ---------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Azure Pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/) | Linux            | [![Build Status](https://dev.azure.com/satrapu/aspnet-core-logging/_apis/build/status/ci-pipeline?branchName=master&jobName=Run%20on%20Linux)](https://dev.azure.com/satrapu/aspnet-core-logging/_build/latest?definitionId=2&branchName=master)                |
| [Azure Pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/) | macOs            | [![Build Status](https://dev.azure.com/satrapu/aspnet-core-logging/_apis/build/status/ci-pipeline?branchName=master&jobName=Run%20on%20macOS)](https://dev.azure.com/satrapu/aspnet-core-logging/_build/latest?definitionId=2&branchName=master)                |
| [Azure Pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/) | Windows          | [![Build Status](https://dev.azure.com/satrapu/aspnet-core-logging/_apis/build/status/ci-pipeline?branchName=master&jobName=Run%20on%20Windows)](https://dev.azure.com/satrapu/aspnet-core-logging/_build/latest?definitionId=2&branchName=master)              |

## Code Quality

| Provider                                  | Badge                                                                                                                                                                                                                  |
| ----------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Codacy](https://www.codacy.com/)         | [![Codacy Badge](https://api.codacy.com/project/badge/Grade/001d9d7bbf43459aae186c7d8cd49858)](https://www.codacy.com/app/satrapu/aspnet-core-logging)                                                                 |
| [FOSSA](https://fossa.com/)               | [![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fsatrapu%2Faspnet-core-logging.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fsatrapu%2Faspnet-core-logging?ref=badge_shield) |
| [SonarCloud](https://sonarcloud.io/about) | [![SonarCloud Status](https://sonarcloud.io/api/project_badges/measure?project=aspnet-core-logging&metric=alert_status)](https://sonarcloud.io/dashboard?id=aspnet-core-logging&branch=master)                         |
