# KENN Archive

KENN Archive is a course-project foundation for a clothing e-commerce application.

## Current foundation

- ASP.NET Core Web API
- Clean Monolith project structure
- Entity Framework Core + SQL Server
- DTO and dependency injection examples
- URL-based product search/filter/sort/pagination
- Slug-based public product identity
- Cloudinary-ready product image model (URLs/PublicIds only)
- React/Vite frontend with a custom KENN Archive visual direction

## Important

This package is intentionally a clean foundation. Authentication, email OTP, Google sign-in, Cloudinary upload service, SignalR chat, cart, wishlist, orders, returns, admin panel and tests should be added and verified one module at a time rather than presenting unverified code as finished.

## Run backend

Open KENNArchive.sln in Visual Studio 2022, restore NuGet packages, set KENNArchive.API as startup project and build.

## Run frontend

Open a terminal in `frontend`:

npm install
npm run dev
