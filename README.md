# aspnet-core-logging

**Table of Contents**
- [Description](#description)  
- [Build](#build) 
- [Code Quality](#code-quality)   
- [Setup Local Development Environment](#local-setup)  
  - [Setup Auth0 account](#setup-auth0-account)
  - [Setup local persistence services](#setup-persistence)
    - [Create Docker volumes](#create-volumes)
    - [Create .env file](#env-file)
    - [Compose commands](#compose-commands)
        - [Run compose services](#run-services)
        - [Stop compose services](#stop-services)
        - [Start compose services](#start-services)
        - [Display compose service log](#display-log)
        - [Destroy compose services](#destroy-services)
  - [Setup environment variables](#setup-env-vars)

<h2 id="description">Description</h2>

This repo shows ASP.NET Core v3.x logging in action; it also serves as a learning, experimenting and teaching path for [Azure Pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/).  
This project has several posts associated with it:

- [Use Docker when running integration tests with Azure Pipelines](https://crossprogramming.com/2019/12/27/use-docker-when-running-integration-tests-with-azure-pipelines.html)
- [Build an ASP.NET Core application using Azure Pipelines](https://crossprogramming.com/2019/03/17/build-asp-net-core-app-using-azure-pipelines.html)
- [Logging HTTP context in ASP.NET Core](https://crossprogramming.com/2018/12/27/logging-http-context-in-asp-net-core.html)

<h2 id="build">Build</h2>

| Build Server                                                                    | Operating System | Status                                                                                                                                                                                                                                                          |
| ------------------------------------------------------------------------------- | ---------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Azure Pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/) | Linux            | [![Build Status](https://dev.azure.com/satrapu/aspnet-core-logging/_apis/build/status/ci-pipeline?branchName=master&jobName=Run%20on%20Linux)](https://dev.azure.com/satrapu/aspnet-core-logging/_build/latest?definitionId=2&branchName=master)                |
| [Azure Pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/) | macOs            | [![Build Status](https://dev.azure.com/satrapu/aspnet-core-logging/_apis/build/status/ci-pipeline?branchName=master&jobName=Run%20on%20macOS)](https://dev.azure.com/satrapu/aspnet-core-logging/_build/latest?definitionId=2&branchName=master)                |
| [Azure Pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/) | Windows          | [![Build Status](https://dev.azure.com/satrapu/aspnet-core-logging/_apis/build/status/ci-pipeline?branchName=master&jobName=Run%20on%20Windows)](https://dev.azure.com/satrapu/aspnet-core-logging/_build/latest?definitionId=2&branchName=master)              |

<h2 id="code-quality">Code Quality</h2>

| Provider                                  | Badge                                                                                                                                                                                                                  |
| ----------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Codacy](https://www.codacy.com/)         | [![Codacy Badge](https://api.codacy.com/project/badge/Grade/001d9d7bbf43459aae186c7d8cd49858)](https://www.codacy.com/app/satrapu/aspnet-core-logging)                                                                 |
| [FOSSA](https://fossa.com/)               | [![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fsatrapu%2Faspnet-core-logging.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fsatrapu%2Faspnet-core-logging?ref=badge_shield) |
| [SonarCloud](https://sonarcloud.io/about) | [![SonarCloud Status](https://sonarcloud.io/api/project_badges/measure?project=aspnet-core-logging&metric=alert_status)](https://sonarcloud.io/dashboard?id=aspnet-core-logging&branch=master)                         |

<h2 id="local-setup">Setup Local Development Environment</h2>
In order to run this application locally, you need to setup some things first.

<h3 id="setup-auth0-account">Setup Auth0 account</h3>
Ensure you have an [Auth0](https://auth0.com) account; if you don't, sign-up for one [here](https://auth0.com/signup).
Once you have an account, follow [these steps](https://auth0.com/docs/getting-started/set-up-api) to create an API.
Once the API has been created, create a machine-to-machine application to get a client ID and a client secret which you can later use to generate a token to be used by the integration tests.

<h3 id="setup-persistence">Setup local persistence services</h3>
This ASP.NET Core web API uses [PostgreSQL](https://www.postgresql.org/) as persistent storage and [pgadmin](https://www.pgadmin.org/) as database manager, all running locally via [Docker Compose](https://github.com/docker/compose).

<h3 id="create-volumes">Create Docker volumes</h3>
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

<h3 id="env-file">Create .env file</h3>
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

<h3 id="compose-commands">Compose commands</h3>
All of the commands below must be run from the folder where you have checked-out this git repository.  
This folder contains a `docker-compose.yml` file describing the aforementioned compose services.

<h4 id="run-services">Run compose services</h4>
```bash
# The -d flag instructs Docker Compose to run services in the background
docker-compose up -d
```

<h4 id="stop-services">Stop compose services</h4>
```bash
docker-compose stop
```

<h4 id="start-services">Start compose services</h4>
```bash
docker-compose start
```

<h4 id="display-log">Display compose service log</h4>
```bash
# The -f flag instructs Docker Compose to display and follow the log entries of the 'pgadmin' service
docker-compose logs -f pgadmin
```

<h4 id="destroy-services">Destroy compose services</h4>
The command below will **not** delete the Docker volumes!
```bash 
docker-compose down
```

<h3 id="setup-env-vars">Setup environment variables</h3>

Since storing sensitive data inside configuration file put under source control is not a very good idea, 
the following environment variables must be defined on your local development machine:

| Name                                       | Value                                                                                          | Description                                                      |
| ------------------------------------------ | ---------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| CONNECTIONSTRINGS__TODO                    | Server=localhost; Port=5432; Database=aspnet-core-logging-dev; Username=satrapu; Password=***; | The connection string pointing to the local development database |
| CONNECTIONSTRINGS__TODOFORINTEGRATIONTESTS | Server=localhost; Port=5433; Database=aspnet-core-logging-it; Username=satrapu; Password=***;  | The connection string pointing to the integration tests database |
| AUTH0__CLIENTID                            | <YOUR_AUTH0_TEST_CLIENT_ID>                                                                    | The Auth0 test client id                                         |
| AUTH0__CLIENTSECRET                        | <YOUR_AUTH0_TEST_CLIENT_SECRET>                                                                | The Auth0 test client secret                                     |