# Desafio Dev

This repository was created using the template from [jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture)

## Development

This project is an ASP.NET Core application (C# 14 / .NET 10) that uses PostgreSQL as its database. Below are the steps to get a local development environment up and running.

### Prerequisites
- .NET SDK 10.x installed (verify with: `dotnet --version`)
- Docker Desktop (or Docker Engine) with Docker Compose
- Git

### Clone and restore
```bash
git clone https://github.com/<your-org-or-user>/desafio-dev.git
cd desafio-dev
dotnet restore
```

### Running the database (PostgreSQL) with Docker
The repository includes a `docker-compose.yml` that provisions a PostgreSQL 17 instance with the following defaults:
- Host port: `5432`
- Database: `DesafioDevDb`
- Username: `admin`
- Password: `password`

Start only the database service:
```bash
docker compose up db -d
```

Stop the database:
```bash
docker compose stop db
```

### Configure the connection string
The app reads the connection string `ConnectionStrings:DesafioDevDb`.

Options:
- When running the Web app inside Docker Compose, the `web` service receives the connection string via environment variables (already set in `docker-compose.yml`). No extra setup is needed.
- When running the Web app on your host (dotnet CLI), point it to the Docker database using this connection string in `src/Web/appsettings.Development.json` or via environment variable:
  - JSON (appsettings.Development.json):
    ```json
    {
      "ConnectionStrings": {
        "DesafioDevDb": "Server=localhost;Port=5432;Database=DesafioDevDb;Username=admin;Password=password;"
      }
    }
    ```
  - Environment variable (macOS/Linux):
    ```bash
    export ConnectionStrings__DesafioDevDb="Server=localhost;Port=5432;Database=DesafioDevDb;Username=admin;Password=password;"
    ```

### Run the Web application (development)

#### Using Docker Compose (runs both Web and DB):
  ```bash
  docker compose up
  ```
  The Web app will listen on http://localhost:8080 (health check: `http://localhost:8080/health`).

#### Using the .NET CLI on your host (DB via Docker):
  ```bash
  dotnet build
  dotnet run --project src/Web
  ```
  By default, ASP.NET Core uses the `Development` environment. If needed:
  ```bash
  export ASPNETCORE_ENVIRONMENT=Development
  ```

### Database migrations

Migrations are run automatically when the application starts. You can check this [documnet](https://jasontaylor.dev/ef-core-database-initialisation-strategies/) for more details.

#z## Useful commands
- List SDKs: `dotnet --list-sdks`
- Clean build artifacts: `dotnet clean`
- Restore packages: `dotnet restore`

## Testing application

Run all tests:
```bash
dotnet test
```

Spin up the full stack with Docker (Web + DB):
```bash
docker compose up
```