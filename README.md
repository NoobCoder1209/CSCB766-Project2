# RoadSuite Marketplace

RoadSuite is a dark-themed ASP.NET Core MVC marketplace for Asian car brands. It replicates the user experience of the legacy CarSystem application with a modernized implementation using ASP.NET Core MVC, Entity Framework Core, SQL Server, and Identity.

## Tech Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core with SQL Server (code-first)
- ASP.NET Core Identity with default UI
- Bootstrap 5 and custom dark automotive theme

## Getting Started

1. Ensure you have the latest .NET 8 SDK and SQL Server (or LocalDB) installed.
2. Update the connection string in `RoadSuite.Web/appsettings.json` if needed.
3. Apply migrations and seed data:

   ```bash
   dotnet tool restore
   dotnet ef database update --project RoadSuite.Web
   ```

4. Run the application:

   ```bash
   dotnet run --project RoadSuite.Web
   ```

5. Browse to `https://localhost:5001`.

## Seeded Roles & Accounts

| Role       | Email                         | Password     |
|------------|-------------------------------|--------------|
| Admin      | admin@roadsuite.local         | Admin!23     |
| Moderator  | moderator@roadsuite.local     | Moderator!23 |
| Dealer     | dealer@roadsuite.local        | Dealer!23    |

Dealer accounts include a default profile and a curated inventory of Asian-brand vehicles across multiple categories.

## Feature Overview

- **Role-based access**
  - Anonymous visitors can browse approved cars with filtering, sorting, and pagination.
  - Dealers manage their inventory (create, edit, mark for deletion, permanently delete) via the **My Cars** dashboard.
  - Moderators approve or reject pending cars, manage categories, and review moderation history.
  - Admins have full access, including moderator management capabilities and category administration.

- **Car lifecycle**
  - Dealer submissions start as _Pending_ until approved by a moderator or admin.
  - Rejections require a feedback reason that is stored and displayed.
  - Deletions support both soft-delete (mark for deletion) and permanent removal paths.

- **Moderation queue**
  - Dedicated dashboard for moderators/admins to approve or reject pending cars with contextual details and history.

- **Categories management**
  - Moderators/Admins can create, edit, and remove categories (deletion restricted when cars exist).

- **Notifications (stub)**
  - Approvals, rejections, deletions, and edits produce records surfaced in the user notifications center.

- **Theming**
  - Dark automotive palette using custom styles in `wwwroot/css/site.css` with Bootstrap components.

- **Data model**
  - Includes Cars, Categories, Dealer Profiles, Moderation Feedback, and Notifications.
  - EF Core migrations provided (`Data/Migrations`).

## Project Structure

```
RoadSuite.sln
└── RoadSuite.Web
    ├── Controllers
    ├── Data
    │   ├── ApplicationDbContext.cs
    │   ├── Migrations
    │   └── SeedData.cs
    ├── Models
    ├── Services
    ├── ViewModels
    ├── Views
    └── wwwroot
```

## Notes

- All seed data focuses exclusively on Asian manufacturers (Honda, Toyota, Nissan, Hyundai, Lexus, Mazda, Kia, Subaru, Mitsubishi, Suzuki).
- Default Identity UI is used for authentication; additional customization can be added as needed.
- Notifications are stored-only (no email or push delivery) per requirements.
