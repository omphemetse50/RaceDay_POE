# RaceDay — POE

## Roles

Roles \& Capabilities
Organiser: Creates, edits and deletes events; manages event categories; captures participant results; views all enrolments for their events.
Participant: Registers an account; browses events; enrols in an event by selecting a category; views their own enrolments and personal race results.

\---

# Part 1: System Planning and Database

## Contents of this repository (Part 1)

All planning documents are in the [`/docs`](./docs) folder:
File \& Description
`RaceDay ERD.pdf`: Entity Relationship Diagram — 6 entities (Roles, Users, Events, Categories, Enrolments, Results), with primary keys, foreign keys, and relationship cardinality.
`RaceDay API Endpoint Plan.pdf`: Full REST API endpoint specification covering authentication, user profiles, events, categories, event enrolments, and results — including HTTP method, route, description, required role,
request body, and expected response for every endpoint.
`RaceDaySQLQuery.sql`: SQL Server (SSMS) script that creates the full database schema matching the ERD, with primary/foreign keys, constraints, and seed data (2 Organisers, 3 Participants, 3 Events,
categories per event, and sample enrolments/results).

## Design notes

* **Users** table holds both Organisers and Participants, differentiated by a `RoleID` foreign key into a `Roles` lookup table.
* **Enrolments** is the junction table implementing the many-to-many relationship between Participants (Users) and Categories.
* **Results** has a one-to-one relationship with Enrolments — one result per race entry, captured by the Organiser after the event.

## Running the SQL script

1. Open SQL Server Management Studio (SSMS) and connect to a clean SQL Server instance.
2. Open `docs/RaceDaySQLQuery.sql`.
3. Execute the script. It will create a `RaceDayDB` database, all six tables, and seed sample data.
4. Verify with `SELECT \* FROM Users;` etc.

## Part 1 CI/CD

A GitHub Actions workflow (`.github/workflows/Part 1 CI.yml`) runs on every push and validates that:

* the `/docs` folder exists,
* it contains the ERD file, the endpoint plan and the SQL script,
* `README.md` exists at the repository root.

<img width="1322" height="282" alt="image" src="https://github.com/user-attachments/assets/f3ad6d75-9c46-42ac-980b-25e9e7557a15" />

<img width="630" height="119" alt="Screenshot 2026-09-04 230437" src="https://github.com/user-attachments/assets/883c0397-ca0c-4cff-8e7c-97b7e051300c" />



## Part 1 video walkthrough

**YouTube (unlisted):** \[`(https://youtu.be/sLPNnJ\_g7q4?si=\_8\_nj2le3DNoH5yX)`]
The video walks through the planning documents, explains the ERD design decisions, the endpoint plan and runs the SQL script live in SSMS.

\---



# Part 2: RESTful API Development

## Overview

`RaceDay` is an ASP.NET Core Web API (net8.0) that implements every endpoint from the Part 1 plan. It uses Entity Framework Core (Code-First) against SQL Server, **server-side session authentication** for login state and roles, and Swagger for live endpoint testing.

## Repository structure

```
README.md
docker-compose.yml
docs/                      <- Part 1 planning documents (ERD, endpoint plan, SQL script)
.github/workflows/         <- Part 1 CI.yml, Part 2 CI.yml, Part 3 CI.yml
RaceDay/                   <- the Web API (also holds RaceDay.sln and Dockerfile)
  Authentication/          <- SessionAuthenticationHandler
  Common/                  <- ApiError
  Controllers/             <- Authentication, Users, Events, Categories, Enrolments
  Database/                <- RaceDayDbContext
  DTOs/                    <- request/response shapes per feature area
  Extensions/              <- ClaimsPrincipalExtensions
  Migrations/              <- EF Core Code-First migrations
  Models/                  <- Role, User, Event, Category, Enrolment, Result
  Program.cs
RaceDay.Tests/             <- xUnit test project
  AuthenticationTests/  CategoriesTests/  EnrolmentsTests/
  EventsTests/  ResultsTests/  TestHelpers/
```

## The two roles

* **Organiser** - creates, updates and deletes their own events; defines categories (e.g. Under 20, Senior, 10km, 21km) for their events; views all enrolments for their events; captures finish times and positions for participants.
* **Participant** - registers an account; browses events and categories; enrols in an event by selecting a category; cancels their own enrolment; views their own results.

Both roles can register, log in, log out and view/update their own profile.

## Setup and run

**Prerequisites:** .NET 8 SDK, Visual Studio 2022 (ASP.NET workload), SQL Server or SQL Server LocalDB, and the EF Core CLI (`dotnet tool install --global dotnet-ef`).

1. **Clone** the repository and open `RaceDay/RaceDay.sln` in Visual Studio.
2. **Check the connection string** in `RaceDay/appsettings.json` (`DefaultConnection`). Point it at your own SQL Server / LocalDB instance if needed.
3. **Create the database** from the committed migrations. In Package Manager Console (default project: `RaceDay`):

```powershell
   Update-Database
   ```

   or from the repository root in a terminal:

```bash
   dotnet ef database update --project RaceDay
   ```

   This creates `RaceDayDB` matching the Part 1 ERD and seeds the two roles (Organiser, Participant).

4. **Run** `RaceDay` (F5 or `dotnet run --project RaceDay`). It opens Swagger at `/swagger`.

## Authentication and role-based access

Authentication uses **ASP.NET Core server-side session state**:

* `POST /api/auth/register` creates a user as `Organiser` or `Participant`. Passwords are hashed with ASP.NET Core's `PasswordHasher<User>` and never stored in plain text.
* `POST /api/auth/login` verifies the credentials and stores the user's ID and role server-side in `HttpContext.Session`. The client only receives an opaque, `HttpOnly` session cookie (`RaceDay.Session`).
* A custom `SessionAuthenticationHandler` rebuilds the `ClaimsPrincipal` from the session on every request, so `\[Authorize]`, `\[Authorize(Roles = "Organiser")]` and `\[Authorize(Roles = "Participant")]` work as normal.
* `POST /api/auth/logout` clears the session; a unit test confirms protected calls then return 401.
* Ownership is also checked in code, e.g. an Organiser can only change their own events and categories.

### Testing auth in Swagger

1. Open `/swagger`, use **Try it out** on `POST /api/auth/register`, then `POST /api/auth/login`.
2. The browser stores the session cookie and sends it automatically with later requests from the Swagger page - no Authorize button needed.
3. Call `POST /api/auth/logout` and confirm protected endpoints return 401.

## Endpoints

|Area|Endpoints|
|-|-|
|Authentication|`POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout`|
|Users|`GET /api/users/me`, `PUT /api/users/me`, `POST /api/users/me/profile-picture`|
|Events|`GET /api/events`, `GET /api/events/{id}` (public); `POST`, `PUT /{id}`, `DELETE /{id}`, `POST /{id}/banner`, `GET /{id}/enrolments` (Organiser)|
|Categories|`GET /api/events/{eventId}/categories` (public); `POST /api/events/{eventId}/categories`, `PUT`/`DELETE /api/categories/{id}` (Organiser)|
|Enrolments|`POST /api/categories/{categoryId}/enrolments`, `GET /api/users/me/enrolments`, `DELETE /api/enrolments/{id}` (Participant)|
|Results|`POST /api/enrolments/{enrolmentId}/result` (Organiser); `GET /api/enrolments/{enrolmentId}/result` (authorised user); `GET /api/users/me/results` (Participant)|

Every endpoint has an XML `<summary>` doc comment shown in Swagger UI (purpose, expected input, response).



## Unit tests

`RaceDay.Tests` uses `WebApplicationFactory<Program>` with the EF Core InMemory provider, so tests go through the real HTTP pipeline, including session cookies and role enforcement. Each test identity uses its own `HttpClient` (own cookie jar). 29 tests cover:

* **Registration and login** - valid registration, duplicate email, invalid role, valid login, wrong password, unknown email, session persists across requests.
* **Event management** - Organiser can create and delete events; Participant gets 403; unauthenticated gets 401; another Organiser cannot update the event; logout ends the session.
* **Categories** - owning Organiser can create/delete; non-owning Organiser and Participant get 403; public listing works.
* **Enrolments** - Participant can enrol and the enrolment is recorded; Organiser gets 403; duplicate enrolment gets 409; unauthenticated gets 401; another Participant cannot cancel it.
* **Results** - owning Organiser can capture a result; Participant gets 403; duplicate capture gets 409; Participant sees their own result; unrelated Participant gets 403.

Run them:

```bash
dotnet test RaceDay/RaceDay.sln
```

## Part 2 CI/CD

The GitHub Actions workflow `.github/workflows/Part 2 CI.yml` runs on every push and pull request. It validates the repository structure, restores,
builds in Release configuration and runs all unit tests, then uploads the results as an artifact.

**Screenshot of a successful green build:** (<img width="629" height="164" alt="image" src="https://github.com/user-attachments/assets/c7dbb78f-5ea1-42e4-8892-adfac7cb3881" />)

## Part 2 video walkthrough

**YouTube (unlisted):** \[`https://youtu.be/AcpJlanLyYE?si=BG9-Rns0u8ZWUDgA`]

The video demonstrates the running API, and explains the code structure, endpoints, authentication, Swagger and the unit tests.

\---



# Part 3: Automation of Web Application (MVC + Azure Blob Storage + Docker)

## Overview

`RaceDay.Web` is an ASP.NET Core MVC application that brings the RaceDay platform together into a role-aware front end. It consumes the Part 2 API for **every** data operation -- it holds no direct database access and no replicated business logic.
Azure Blob Storage (via Azurite locally) is integrated **into the API**, not the MVC app, per the brief: image uploads are posted from the browser to the MVC app, forwarded from the MVC app to the API, and the API is the only thing that talks to Azure.

## Project structure

```
RaceDay/                    <- RaceDay.sln lives here and references all three projects
  (API)                     <- Part 2 API (+ Azure Blob Storage, Part 3)
  Services/IBlobStorageService.cs, AzureBlobStorageService.cs
  Dockerfile
RaceDay.Tests/               <- unit tests (unchanged from Part 2)
RaceDay.Web/                 <- Part 3 MVC application
  Controllers/                Account, Home, Dashboard, Events, Categories, Enrolments, Results, Profile
  Services/                   ApiClient, AuthApiService, ApiSessionCookieHandler, StatusBadgeHelper
  Models/                     ViewModels + API DTO mirrors
  Views/                      Razor views, Bootstrap 5, RaceDay branding
  Dockerfile
docker-compose.yml            <- SQL Server + Azurite + API + MVC, one command
.github/workflows/Part 3 CI.yml
```

## How authentication actually works (read this before anything else)

The brief asks for "session management to maintain the user's authenticated state and role across all pages," communicating with the Part 2 API for login. Here's the exact mechanism:

1. `POST /api/auth/login` on the API establishes a **server-side session on the API** and returns a `Set-Cookie: RaceDay.Session=...` header (this is the Part 2 session, unchanged).
2. The MVC app's `AuthApiService` captures that raw cookie value from the response (it's never sent to the browser).
3. The MVC app signs the user in with **its own** ASP.NET Core cookie authentication (`HttpContext.SignInAsync`), storing `UserId`, `Role`, `FullName`, **and the captured API session cookie value** as claims inside its own `RaceDay.Web.Auth` cookie.
4. On every later request, `ApiSessionCookieHandler` (a `DelegatingHandler` on the typed `ApiClient`) reads that claim and resends it as the `Cookie` header to the API -- so the API's own `\[Authorize(Roles = ...)]` checks work exactly as they do in Swagger.
5. `\[Authorize]` / `\[Authorize(Roles = "Organiser")]` on MVC controllers gate pages the same idiomatic way ASP.NET Core always does; an unauthenticated visitor hitting a protected page is automatically redirected to `/Account/Login`.
6. Logout calls the API's `/api/auth/logout` (ending the API's session) and then clears the MVC app's own cookie (`SignOutAsync`) -- both sides of the session end together.

This means there are, correctly, **two** cookies at play (the MVC app's own auth cookie in the browser, and the API's session cookie held server-side inside that cookie's claims) -- but only one of them ever reaches the browser, and the user experiences a single, continuous session.

## Setup and run (Visual Studio, without Docker)

1. Make sure Part 2's database and connection string are already working (see the Part 2 section above).
2. **Run Azurite** (the local Azure Storage emulator used by `ConnectionStrings:AzureBlobStorage` in `RaceDay/appsettings.json`, which defaults to `UseDevelopmentStorage=true`):

```bash
   npm install -g azurite
   azurite --silent --location ./azurite-data
   ```

   Or run it as a Docker container on its own: `docker run -p 10000:10000 mcr.microsoft.com/azure-storage/azurite`.
(To use a real Azure Storage account instead, replace that connection string with your account's real connection string -- nothing else changes.)

3. **Apply EF Core migrations** if you haven't already (see Part 2 section) -- the API auto-applies any pending migrations on startup too, but they still need to exist and be committed first.
4. In Visual Studio, right-click the solution → **Set Startup Projects** → **Multiple startup projects** → set both `RaceDay` and `RaceDay.Web` to **Start**, then run (F5). The API starts on `https://localhost:7071`, and the MVC app on `https://localhost:7100`.
5. `RaceDay.Web/appsettings.json`'s `RaceDayApi:BaseUrl` must match the API's actual URL -- update it if your ports differ.

## Running everything with Docker (one command)

```bash
docker compose up --build
```

This builds and starts **SQL Server, Azurite, the API, and the MVC app** together. Once it's up:

* MVC app: http://localhost:5100
* API + Swagger: http://localhost:5236/swagger

> The EF Core migrations are committed in `RaceDay/Migrations/`; the API applies them automatically when the container starts, so no manual database step is needed. Uploaded images are stored in Azurite and served from `http://localhost:10000`.

**Stop everything:** `docker compose down` (add `-v` to also wipe the database and stored images).

To run just the MVC app's own container against an API running elsewhere:

```bash
docker build -f RaceDay.Web/Dockerfile -t raceday-web .
docker run -p 5100:8080 -e RaceDayApi\_\_BaseUrl=http://host.docker.internal:5236/ raceday-web
```

## Organiser interface

* **Dashboard** (`/Dashboard`) -- summary of events created, total enrolments across them, and the next upcoming event date.
* **Events** -- create, edit, and delete events (name, description, date, location, distance, event type), with banner image upload.
* **Categories** (`/Categories?eventId=`) -- add age- or distance-based categories per event.
* **Enrolments** (`/Events/Enrolments/{id}`) -- every participant enrolled in an event, with status, and a link to capture their result.
* **Results** (`/Results/Capture`) -- finish time, position, and status entry per enrolment.

## Participant interface

* **Home / Browse** (`/Events`) -- all upcoming events with location/date filtering, open to anyone (even logged out).
* **Event details** (`/Events/Details/{id}`) -- full event info, categories, and an **Enter Event** button per category for logged-in Participants.
* **My Enrolments** (`/Enrolments`) -- every event entered, with colour-coded status and a cancel option.
* **My Results** (`/Results/MyResults`) -- personal race history: event, category, finish time, and finishing position, once an Organiser has captured it.
* **Profile** (`/Profile`) -- view/update name and email, and upload a profile picture.

## Azure Blob Storage integration

* `RaceDay/Services/AzureBlobStorageService.cs` is the **only** code in the whole solution that references `Azure.Storage.Blobs`.
* The MVC app never imports the Azure SDK and never sees an Azure connection string -- it only ever posts `multipart/form-data` to the API's existing `POST /api/events/{id}/banner` and `POST /api/users/me/profile-picture` endpoints (same routes as Part 2), exactly matching "All file handling must go through the Part 2 API."
* The API uploads to a `profile-pictures` or `event-banners` container (auto-created, public read access for the blob itself only) and returns the blob's URL, which is stored on the `User`/`Event` row and then rendered directly via `<img src="...">` in the MVC views.

## UI design

Bootstrap 5 (via CDN) with a custom RaceDay stylesheet (`RaceDay.Web/wwwroot/css/site.css`): a navy-to-blue gradient navbar, an orange accent colour for primary actions, and consistent typography across every page. Enrolment and result statuses use colour-coded badges (green = Confirmed/Finished, amber = Pending, red = Cancelled) defined once in `StatusBadgeHelper` and reused everywhere so the colours never drift between pages. Navigation adapts to role: Organisers see Dashboard/Create Event, Participants see My Enrolments/My Results, and anonymous visitors see only the public event browse and login/signup.

## Part 3 CI/CD

`.github/workflows/Part 3 CI.yml` validates the repository structure (including `RaceDay.Web` and both Dockerfiles), then restores, builds, and tests the **entire** solution -- `RaceDay.sln` now includes `RaceDay.Web`, so this single build step covers the MVC project too. It finishes by building both Docker images to confirm they aren't broken.

**Screenshot of a successful green build:**

`[<img width="632" height="201" alt="Screenshot_10-11-2026_05424_github com" src="https://github.com/user-attachments/assets/2f7f4c74-e382-415f-988b-ddf078e84c6d" />]`

## Part 3 video walkthrough

**YouTube (unlisted):** `\[https://youtu.be/gpmJaFbk3Ic?si=ojo5jOymQnzHZOBQ]`

The video demonstrates the full running application (API + MVC together), walks through the Organiser and Participant interfaces, shows an image upload actually landing in Azure/Azurite Blob Storage, and shows `docker compose up` starting the whole stack from nothing.

## Commit history

Part 3 is built with commits reflecting real, incremental work (MVC scaffolding, authentication wiring, each controller/view, Azure Blob integration, Dockerfiles, docker-compose) rather than a single upload.

