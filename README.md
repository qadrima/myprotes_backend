# .NET CRUD API Tes

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/qadrima/myprotes_backend.git
   cd myprotes_backend
   ```

2. Update the database connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=mypro_backend;User=your_username;Password=your_password;"
     }
   }
   ```

3. Install dependencies:
   ```bash
   dotnet restore
   ```

4. Apply database migrations:
   ```bash
   dotnet ef database update
   ```

5. Run the application:
   ```bash
   dotnet run
   ```

The API will be available at `http://localhost:5114` or `http://localhost:5000`.

## Development

### Adding New Migrations

When you make changes to your models, create a new migration:

```bash
dotnet ef migrations add MigrationName
```

### Testing the API

You can use the included `EndpoinTesting.http` file to test the API endpoints using tools like Visual Studio Code's REST Client extension or Postman.