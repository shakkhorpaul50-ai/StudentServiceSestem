# IUBAT Student Service System — Shakkhor Paul

Simple ASP.NET Core MVC Student Service Request System for IUBAT (International University of Business Agriculture and Technology) — built for Render free hosting + Neon free Postgres.

## Features
- **Student:** Register/Login, submit service request (ID Card Replacement / Transcript / Certificate), description 10-500 chars, view My Requests, track status (Pending ? Processing ? Completed / Rejected), view details (own only).
- **Staff:** Login as \s.paul@iubat.edu\ / \\@2kh0R\, view All Requests (latest first), view details (student name/email), update status.
- **Auth:** ASP.NET Identity, roles \Student\/\Staff\, \[Authorize(Roles=...)]\, cookie login path \/Account/Login\, seeded staff + roles.
- **Branding:** Real IUBAT logos/images from \https://iubat.edu\ (\Iubat-Logo-with-name.png\, \home-slider-iubat.jpg\, etc.) in \wwwroot/images\, IUBAT primary \#b1040e\ palette.

## Stack
- .NET 10 (10.0.400), ASP.NET Core MVC, Entity Framework Core 10.0.11, \Npgsql.EntityFrameworkCore.PostgreSQL\ 10.0.0
- Identity, Razor Views, Bootstrap 5, jQuery validation, polling file watcher for Render inotify limit.

## Project Structure
\\\
shakkhor/
 +- Dockerfile                 # multi-stage sdk:10.0 ? aspnet:10.0, handles PORT env
 +- render.yaml                # Render service iubat-student-service-shakkhor (docker, healthCheck /)
 +- .gitignore
 +- IUBAT_Student_Service/
     +- Program.cs             # PORT binding, ReloadOnChange=false, ForwardedHeaders, DATABASE_URL parsing, MigrateAsync+Seed
     +- Data/ApplicationDbContext.cs + SeedData.cs (seeds s.paul@iubat.edu)
     +- Models/ApplicationUser.cs, ServiceRequest.cs, Enums.cs + ViewModels/*
     +- Controllers/Account, Student, Staff, Home
     +- Views/Account, Student, Staff, Home, Shared/_Layout.cshtml
     +- wwwroot/images/* (real IUBAT assets), css/site.css, lib/bootstrap
     +- Migrations/20260904071020_InitialCreate
     +- appsettings.json (placeholder — set via Render env var)
\\\

## Local Run
\\\powershell
# set Neon connection (pooled) for local dev — do NOT commit secrets
\="Host=ep-silent-shadow-ae9qrh4s-pooler.c-2.us-east-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_ViLMzXJAa82Q;SSL Mode=Require;Channel Binding=Require"
# or set DATABASE_URL="postgresql://neondb_owner:npg_ViLMzXJAa82Q@ep-silent-shadow-ae9qrh4s-pooler.c-2.us-east-2.aws.neon.tech/neondb?sslmode=require&channel_binding=require"

dotnet ef database update
dotnet run --project IUBAT_Student_Service/IUBAT_Student_Service.csproj
# http://localhost:5197 — Register student, login staff with s.paul@iubat.edu / \@2kh0R
\\\

## Render Deployment
1. Push to GitHub \shakkhorpaul50-ai/StudentServiceSystem\.
2. Render ? New ? Web Service ? Connect repo ? Runtime \Docker\ ? Dockerfile at root.
3. Add env vars (Environment ? Add):
   - \ASPNETCORE_ENVIRONMENT\ = \Production\
   - \RENDER\ = \	rue\
   - \DOTNET_USE_POLLING_FILE_WATCHER\ = \	rue\
   - \DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE\ = \alse\
   - \ConnectionStrings__DefaultConnection\ = \Host=ep-silent-shadow-ae9qrh4s-pooler.c-2.us-east-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_ViLMzXJAa82Q;SSL Mode=Require;Channel Binding=Require\
   - **or** \DATABASE_URL\ = \postgresql://neondb_owner:npg_ViLMzXJAa82Q@ep-silent-shadow-ae9qrh4s-pooler.c-2.us-east-2.aws.neon.tech/neondb?sslmode=require&channel_binding=require\
4. Deploy ? logs should show \[DB] Using host: ep-silent-shadow...\ + \[DB] Database ready\.
5. Test: Register student ? Create request ? Staff login ? All Requests ? Update status.

## Neon
- Free tier pooled host: \ep-silent-shadow-ae9qrh4s-pooler.c-2.us-east-2.aws.neon.tech\
- Uses \SslMode=Require\ + \ChannelBinding=Require\. Program.cs parses both key=value and \postgresql://\ URL forms.

## Branding Assets
Downloaded from \iubat.edu\: \Iubat-Logo-with-name.png\, \Iubat-logo.png\, \home-slider-iubat.jpg\, \cropped-IUBAT_30_years_glory...\, \qs-world-ranking...\ — see \wwwroot/images\.

## Database
- Tables: \AspNetUsers\, \AspNetRoles\, \ServiceRequests\ (FK \StudentId\ ? \AspNetUsers\ Restrict, indexes on \StudentId\ & \CreatedDate\).
- Migrations auto-run on startup via \context.Database.MigrateAsync()\.

## Author
Shakkhor Paul — s.paul@iubat.edu