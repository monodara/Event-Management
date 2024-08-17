# Event Management System

![Azure](https://img.shields.io/badge/Azure-Enabled-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Enabled-blue)

## Project Description

The project consists of 3 applications, an event management system, a registration processing system, and a registration result notification system.
The Event Management System is a web application built with `ASP.NET Core`, `Entity Framework Core (EF Core)`, and `PostgreSQL`. It uses `ASP.NET Core Identity` for user authentication and authorization, and integrates with Azure services for storage and monitoring. It allows admin users to manage users and roles, event providers to manage events and related documents, plain users to browse and register for events.
The registration processing system and the registration result notification system, which are `serverless`, are implemented with `Azure Function App` and `Azure Service Bus`.

## Features

1. **User registration and authentication** with `ASP.NET Core Identit`y.
2. **Role-based authorization** (Admin, EventProvider, User).
3. **CRUD operations** for events.
4. **Event registration with FIFO processing** using `Azure Service Bus` and `Azure Functions`.
5. **Storage of images and documents** in `Azure Blob Storage`.
6. **Email notification** with `Azure Functions`.
7. **Monitoring and diagnostics** with `Azure Application Insights`.

