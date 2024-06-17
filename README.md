# Event Management System

This project is an Event Management System built with ASP.NET Core, Entity Framework Core, and PostgreSQL. It leverages Microsoft Identity for authentication and role-based authorization, along with various Azure services such as Azure Blob Storage, Application Insights, and Cosmos DB for PostgreSQL.

## Table of Contents

1. [Project Description](#project-description)
2. [Features](#features)
3. [Set Up](#set-up)
   - [Azure Accounts and Services](#azure-accounts-and-services)
   - [Configure Application](#configure-application)
   - [Apply Migrations](#apply-migrations)
4. [Containerization and Deployment](#containerization-and-deployment)
   - [Create Dockerfile](#create-dockerfile)
   - [Build and Run Docker Container Locally](#build-and-run-docker-container-locally)
   - [Deploy to Azure](#deploy-to-azure)
5. [Guideline](#guideline)
6. [Contributing](#contributing)
7. [License](#license)

## Project Description

The Event Management System allows users to register, manage their profiles, and participate in events. It includes role-based access control with different roles such as Admin, Event Provider, and User. Admins can manage users and roles, while Event Providers can create and manage events.

## Features

- **User Management:**

  - Register, update profile, and delete account
  - Role-based access control (Admin, Event Provider, User)

- **Event Management:**

  - Event Providers can create and manage events

- **Role Management:**

  - Admins can create roles, assign roles to users, and remove roles from users

- **Integration with Azure Services:**
  - **Azure Blob Storage** for storing event media
  - **Application Insights** for monitoring and logging
  - **Cosmos DB for PostgreSQL** for scalable database management
  - **Entra ID** for secure authentication and authorization

## Set Up

### Azure Accounts and Services

1. **Azure Account:**

   - Sign up for an [Azure Account](https://azure.microsoft.com/en-us/free/).

2. **Create Azure Cosmos DB for PostgreSQL:**

   - Follow the [instructions](https://learn.microsoft.com/en-us/azure/cosmos-db/postgresql/) to create a Cosmos DB for PostgreSQL.

3. **Create Azure Storage Account:**

   - Follow the [instructions](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create) to create an Azure Storage Account.

4. **Set Up Application Insights:**
   - Follow the [instructions](https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core?tabs=netcorenew) to create an Application Insights resource.

### Configure Application

Update the appsettings.json file with your PostgreSQL connection string, Entra ID credentials, Blob Storage details, and Application Insights connection string.

### Apply Migrations
Apply migrations and update the PostgreSQL database:

````sh
dotnet ef migrations add InitialCreate
dotnet ef database update
````

