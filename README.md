# aspnet-core-logging

## Description

This repo shows ASP.NET Core v3.x logging in action; it also serves as a learning, experimenting and teaching path for [Azure Pipelines](https://azure.microsoft.com/en-us/services/devops/pipelines/).  
This project has several posts associated with it:

- [Use Docker when running integration tests with Azure Pipelines](https://crossprogramming.com/2019/12/27/use-docker-when-running-integration-tests-with-azure-pipelines.html)
- [Build an ASP.NET Core application using Azure Pipelines](https://crossprogramming.com/2019/03/17/build-asp-net-core-app-using-azure-pipelines.html)
- [Logging HTTP context in ASP.NET Core](https://crossprogramming.com/2018/12/27/logging-http-context-in-asp-net-core.html)

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
