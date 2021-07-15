# aspnet-core-logging

## Description

This repo shows ASP.NET Core v5.x logging in action; it also serves as a learning, experimenting and teaching path for .NET, Azure Pipelines and other technologies & tools.

:exclamation: Currently this web API uses JSON web tokens (JWT) for authentication & authorization purposes, but momentarily the mechanism used for generating these tokens has been __greatly__ simplified to the point of being actually naive as my focus is set on other topics; on the other hand, I do intend on providing a more realistic implementation in a not so far away future.

This project has several posts associated with it:

- [Use Docker Compose when running integration tests with Azure Pipelines](https://crossprogramming.com/2020/09/03/use-docker-compose-when-running-integration-tests-with-azure-pipelines.html)
- [Use Docker when running integration tests with Azure Pipelines](https://crossprogramming.com/2019/12/27/use-docker-when-running-integration-tests-with-azure-pipelines.html)
- [Build an ASP.NET Core application using Azure Pipelines](https://crossprogramming.com/2019/03/17/build-asp-net-core-app-using-azure-pipelines.html)
- [Logging HTTP context in ASP.NET Core](https://crossprogramming.com/2018/12/27/logging-http-context-in-asp-net-core.html)

## Build

| Build Server                                                                    | Operating System | Status                                                                                                                                                                                                                                                          |
| ------------------------------------------------------------------------------- | ---------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Azure Pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/) | Linux            | [![Build Status](https://dev.azure.com/satrapu/aspnet-core-logging/_apis/build/status/ci-pipeline?branchName=master&jobName=Run%20on%20Linux)](https://dev.azure.com/satrapu/aspnet-core-logging/_build/latest?definitionId=2&branchName=master)                |
| [Azure Pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/) | macOs            | Temporarily disabled due to this GitHub issue: [Unattended install of Docker Desktop (Windows and macOS)](https://github.com/docker/roadmap/issues/80).                                                                                                            |
| [Azure Pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/) | Windows          | [![Build Status](https://dev.azure.com/satrapu/aspnet-core-logging/_apis/build/status/ci-pipeline?branchName=master&jobName=Run%20on%20Windows)](https://dev.azure.com/satrapu/aspnet-core-logging/_build/latest?definitionId=2&branchName=master)              |

## Code quality

| Provider                                  | Badge                                                                                                                                                                                                                  |
| ----------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Codacy](https://www.codacy.com/)         | [![Codacy Badge](https://api.codacy.com/project/badge/Grade/001d9d7bbf43459aae186c7d8cd49858)](https://www.codacy.com/app/satrapu/aspnet-core-logging)                                                                 |
| [FOSSA](https://fossa.com/)               | [![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fsatrapu%2Faspnet-core-logging.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fsatrapu%2Faspnet-core-logging?ref=badge_shield) |
| [SonarCloud](https://sonarcloud.io/about) | [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=aspnet-core-logging&metric=ncloc)](https://sonarcloud.io/dashboard?id=aspnet-core-logging) <br/> [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=aspnet-core-logging&metric=coverage)](https://sonarcloud.io/dashboard?id=aspnet-core-logging) <br/> [![SonarCloud Status](https://sonarcloud.io/api/project_badges/measure?project=aspnet-core-logging&metric=alert_status)](https://sonarcloud.io/dashboard?id=aspnet-core-logging) |

## Setup local development environment

In order to run this application locally, you need to setup some things first, like: run PostgreSQL and pgAdmin via Docker Compose, create a PostgreSQL database using EF Core database migrations, etc.

### Setup local persistence services

This ASP.NET Core web API uses [PostgreSQL](https://www.postgresql.org/) as persistent storage and [pgAdmin](https://www.pgadmin.org/) as database manager, all running locally via [Docker Compose](https://github.com/docker/compose).

#### Create Docker volumes

These volumes are needed to store data outside the Docker containers running the PostgreSQL databases and their manager.

- Volume used by the local development database

```bash
docker volume create --name=aspnet-core-logging-dev_data
```

- Volume used by the integration tests when run locally

```bash
docker volume create --name=aspnet-core-logging-it_data
```

- Volume used by pgAdmin tool

```bash
docker volume create --name=pgadmin_data
```

- Volume used by Seq tool

```bash
docker volume create --name=seq_data
```

#### Create .env file

The [.env](https://docs.docker.com/compose/env-file/) file is used by Docker Compose to avoid storing sensitive data inside `docker-compose.yml` file.
Create a new file named `.env` inside the folder where you have checked-out this git repository and add the following lines:

```properties
# Environment variables used by 'aspnet-core-logging-dev' service
# suppress inspection "UnusedProperty"
DB_DEV_POSTGRES_USER=<DB_DEV_USER>
# suppress inspection "UnusedProperty"
DB_DEV_POSTGRES_PASSWORD=<DB_DEV_PASSWORD>

# Environment variables used by 'aspnet-core-logging-it' service
# suppress inspection "UnusedProperty"
DB_IT_POSTGRES_USER=<DB_IT_USER>
# suppress inspection "UnusedProperty"
DB_IT_POSTGRES_PASSWORD=<DB_IT_PASSWORD>

# Environment variables used by 'pgadmin' service
# suppress inspection "UnusedProperty"
PGADMIN_DEFAULT_EMAIL=<PGADMIN_EMAIL_ADDRESS>
# suppress inspection "UnusedProperty"
PGADMIN_DEFAULT_PASSWORD=<PGADMIN_PASSWORD>
```

Make sure you replace all of the above `<DB_DEV_USER>`, `<DB_DEV_PASSWORD>`, ..., `<PGADMIN_PASSWORD>` tokens with the appropriate values.

#### Compose commands

All of the commands below must be run from the folder where you have checked-out this git repository.
This folder contains a `docker-compose.yml` file describing the aforementioned compose services.

##### Run compose services

```bash
# The -d flag instructs Docker Compose to run services in the background
docker-compose up -d
```

##### Stop compose services

```bash
docker-compose stop
```

##### Start compose services

```bash
docker-compose start
```

##### Display compose service log

```bash
# The -f flag instructs Docker Compose to display and follow the log entries of the 'pgadmin' service
docker-compose logs -f pgadmin
```

##### Destroy compose services

The command below will **not** delete the Docker volumes!

```bash
docker-compose down
```

### Setup pgAdmin

Once the services have been started using `docker-compose up` command, pgAdmin UI is ready to be used.

#### Open pgAdmin UI

Open your browser and navigate to [http://localhost:8080](http://localhost:8080).
In order to start using pgAdmin, you need to authenticate - use the `PGADMIN_DEFAULT_EMAIL` and `PGADMIN_DEFAULT_PASSWORD` properties found in your `.env` file to login.

#### Register your local database server

When asked about a PostgreSQL server to register, populate the fields found inside `Connection` tab as below:

- Host name/address = `aspnet-core-logging-dev` - the compose service name and *not* the container name
(the Docker Compose [networking page](https://docs.docker.com/compose/networking/) is a little bit misleading,
as it mentions *container name*, that's why the services found inside the `docker-compose.yml` file are named differently than their containers)
- Port = `5432` - the Docker internal port
- Username = the value of the `${DB_DEV_POSTGRES_USER}` property from the local `.env` file
- Password = the value of the `${DB_DEV_POSTGRES_PASSWORD}` property from the local `.env` file

### Setup environment variables

Since storing sensitive data inside configuration file put under source control is not a very good idea,
the following environment variables must be defined on your local development machine:

| Name                                       | Value                                                                                          | Description                                                                    |
| ------------------------------------------ | ---------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| CONNECTIONSTRINGS__TODO                    | Server=localhost; Port=5432; Database=aspnet-core-logging-dev; Username=satrapu; Password=***; | The connection string pointing to the local development database               |
| CONNECTIONSTRINGS__TODOFORINTEGRATIONTESTS | Server=localhost; Port=5433; Database=aspnet-core-logging-it; Username=satrapu; Password=***;  | The connection string pointing to the integration tests database               |
| GENERATEJWT__SECRET                        | <YOUR_JWT_SECRET>                                                                              | The secret used for generating JSON web tokens for experimenting purposes only |

The connection strings above use the same username and password pairs find in the local `.env` file.
The port from each connection string represent the host port declared inside the local `docker-compose.yml` file -
see more about ports [here](https://docs.docker.com/compose/compose-file/#ports).

### Setup local development database

In order to run the application locally, you need to have an online PostgreSQL database whose schema is up-to-date.
The database will be started using the aforementioned Docker Compose commands, while its schema will be updated via one of the options below.

#### Option 1: Manually run database migrations

In order to create and update the local development database, you need to install EF Core CLI tools; the reference documentation can be found [here](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet). I also recommend reading about database migrations [here](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli).
All of the commands below should be executed from the folder where you have checked-out this git repository.

- Install dotnet-ef

```bash
dotnet tool install dotnet-ef --global
```

:exclamation: Please restart the terminal after running the above command to ensure the following `dotnet ef` commands do not fail.

- Update dotnet-ef to latest version, if requested to do so

```bash
dotnet tool update dotnet-ef --global
```

- Add a new database migration

```bash
dotnet ef migrations add <MIGRATION_NAME> --startup-project ./Sources/Todo.WebApi --project ./Sources/Todo.Persistence
```

- List existing database migrations

```bash
dotnet ef migrations list --startup-project ./Sources/Todo.WebApi --project ./Sources/Todo.Persistence
```

- Update database to the last migration

```bash
dotnet ef database update --startup-project ./Sources/Todo.WebApi --project ./Sources/Todo.Persistence
```

- Drop existing database

```bash
dotnet ef database drop --startup-project ./Sources/Todo.WebApi --project ./Sources/Todo.Persistence
```

#### Option 2: Run database migrations at application startup

Ensure the `MigrateDatabase` configuration property is set to `true`.
See more about applying EF Core migrations at runtime [here](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli#apply-migrations-at-runtime).

### Inspect MiniProfiler results

If you enable [MiniProfiler](https://miniprofiler.com/) by setting the configuration property `MiniProfiler:Enable` to `true`, you can navigate to the following MiniProfiler URLs:

- List all requests: [https://localhost:5001/miniprofiler/results-index](https://localhost:5001/miniprofiler/results-index)
- Inspect current request: [https://localhost:5001/miniprofiler/results](https://localhost:5001/miniprofiler/results)
- List all requests as JSON: [https://localhost:5001/miniprofiler/results-list](https://localhost:5001/miniprofiler/results-list)

### Inspect log events using Seq

In order to inspect application log events generated via [Serilog](https://serilog.net/), navigate to [http://localhost:8888](http://localhost:8888), which will open [Seq](https://datalust.co/seq) UI.
