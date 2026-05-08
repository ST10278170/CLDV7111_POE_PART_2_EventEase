CLDV7111 – POE Part 2: EventEase Web Application
A cloud‑aligned ASP.NET Core MVC system for managing Events, Venues, and Bookings with full CRUD, validation, and local image storage.

Project Overview
EventEase is a structured, database‑driven web application developed for the Cloud Development (CLDV7111) POE Part 2 assessment.
The system demonstrates core cloud‑aligned development principles, including:

MVC architectural design
Entity Framework Core data management
Local file storage for images
CRUD operations across all entities
Search, sorting, and pagination
Consolidated data presentation
Validation and error‑handling workflows

The project reflects a complete, functional implementation aligned with academic requirements and industry expectations.

Project Structure
Controllers/ — Logic for Events, Venues, Bookings, and consolidated views
Models/ — Entity classes, ViewModels, and EF Core configurations
Data/ — Database context and migrations
Views/ — Razor views for all UI pages
wwwroot/ — Static assets (CSS, JS, images)
Images/ — Local storage for uploaded event/venue images

Key Features
Event Management  
Create, edit, delete, search, sort, and upload event images.

Venue Management  
CRUD operations with capacity validation and image uploads.

Booking Management  
Prevents double‑bookings, includes search, sorting, and pagination.

Consolidated Bookings View  
Displays Event, Venue, and Booking details in a unified table.

Local Image Storage  
Images saved to wwwroot/images using GUID‑based filenames.

Validation & Error Handling  
Required fields, duplicate prevention, and user‑friendly feedback.

Technologies Used
ASP.NET Core MVC

Entity Framework Core
SQL Server LocalDB
Bootstrap 5
C#
Razor Views
LINQ

How to Run the Project
Clone the repository
Open the solution in Visual Studio
Restore NuGet packages
Apply EF Core migrations
Run the project using IIS Express

Image Storage
Uploaded images are stored locally in:

Code
wwwroot/images/
The folder may initially be empty; the application generates files automatically during runtime.

Student Information
Name: Darshan
Student Number: ST10278170
Module: Cloud Development (CLDV7111)
Assessment: POE Part 2 – EventEase Web Application
