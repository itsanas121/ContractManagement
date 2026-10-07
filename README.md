

## Database setup (local)
1. Install SQL Server LocalDB and start it: `sqllocaldb start MSSQLLocalDB`
2. Install the EF tool once: `dotnet tool install --global dotnet-ef --version 10.*`
3. From the repo root, create/update the database:
   `dotnet ef database update --project src/ContractManagement.Infrastructure --startup-project src/ContractManagement.Api`
4. Secrets (DB password, JWT key) go in `dotnet user-secrets`, never in appsettings.