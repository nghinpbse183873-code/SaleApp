# Sales & Store Management System (SaleApp)

A desktop retail and store management application developed using **C#**, **.NET (WPF)**, and **Microsoft SQL Server**, structured with **MVVM** and **Repository/Service** design patterns.

## 🚀 Key Features
* **Authentication & Authorization:** Secure user login and registration with **BCrypt** password hashing and role-based access control (Customer & Staff).
* **Customer Module:** Browse products by category, manage shopping cart, place orders, and track personal order history.
* **Staff / Admin Module:** Analytics dashboard, full CRUD management for products and categories, order processing, and customer purchase history tracking.

## 🛠 Tech Stack
* **Language & Framework:** C#, .NET (WPF)
* **Architecture:** MVVM Pattern, Repository Pattern, Service Layer
* **Database:** Microsoft SQL Server, ADO.NET (`Microsoft.Data.SqlClient`)
* **Security:** `BCrypt.Net-Next`

## ⚙️ Setup & Installation
1. Clone this repository: `git clone https://github.com/nghinpbse183873-code/SaleApp.git`
2. Open SQL Server Management Studio (SSMS) and execute the SQL script located at `SaleApp/Database/init_database.sql` to create the database schema and sample data.
3. Update the database connection string in `SaleApp/App.config` or `SaleApp/Infrastructure/AppSettings.cs` to match your local SQL Server instance.
4. Open `SaleApp.slnx` in Visual Studio, restore NuGet packages, and run the application.
