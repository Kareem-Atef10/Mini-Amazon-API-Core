\# Mini Amazon API



A mini e-commerce REST API built using \*\*ASP.NET Core Web API, C#, Entity Framework Core, and Microsoft SQL Server\*\*. This project demonstrates backend development concepts including RESTful API design, relational database management, shopping cart operations, order processing, and validation.



\## Features



\- \*\*Product and Category Management\*\* — Organize products and their associated categories.

\- \*\*Shopping Cart\*\* — Add, update, retrieve, and remove cart items.

\- \*\*Order Management\*\* — Place orders, retrieve order details, and list orders.

\- \*\*Inventory Management\*\* — Validate product availability and stock quantities.

\- \*\*Order Status Management\*\* — Update order statuses with administrator role verification.

\- \*\*Data Validation\*\* — Use DTOs and Data Annotations to validate incoming requests.

\- \*\*Password Hashing\*\* — Use ASP.NET Core Identity's password hashing functionality.

\- \*\*Global Exception Handling\*\* — Return consistent error responses for unexpected exceptions.

\- \*\*API Documentation\*\* — Explore and test endpoints through Swagger/OpenAPI.



\## Technologies Used



\- C#

\- ASP.NET Core Web API

\- Entity Framework Core — Code First

\- Microsoft SQL Server

\- LINQ

\- Data Transfer Objects (DTOs)

\- Data Annotations

\- ASP.NET Core Identity Password Hasher

\- Swagger/OpenAPI

\- Git and GitHub



\## Project Structure



Mini\_Amazon\_API\_Core/

├── Controllers/

├── DTOs/

├── Migrations/

├── Models/

├── Properties/

├── Program.cs

├── appsettings.json

└── Mini\_Amazon\_API\_Core.csproj



\## Getting Started



\### Prerequisites



\- .NET SDK compatible with the project

\- Microsoft SQL Server

\- Visual Studio or another compatible IDE



\### Installation



1\. Clone the repository:
     ```bash
   git clone https://github.com/Kareem-Atef10/Mini-Amazon-API-Core.git
   ```



2\. Navigate to the project directory:

&#x20;  cd Mini\_Amazon\_API\_Core



3\. Configure the `AMZ` connection string using .NET User Secrets or an environment variable.



4\. Apply the existing Entity Framework Core migrations:

&#x20;  dotnet ef database update



5\. Run the application:

&#x20;  dotnet run



6\. Open the Swagger URL displayed in the terminal to explore and test the API.



\## Administrator Order Management



The API checks the corresponding user's role before allowing administrator-only order status updates.



This is a simplified approach for the Project and is not a substitute for secure authentication in a production application.



\## What I Learned



\- Designing RESTful endpoints using ASP.NET Core Web API.

\- Modeling relationships with Entity Framework Core.

\- Managing database schemas using Code First migrations.

\- Implementing business logic for shopping carts, orders, and inventory.

\- Validating requests and handling API errors.

\- Documenting and testing endpoints with Swagger/OpenAPI.



\## Author



Developed as a backend learning project to practice ASP.NET Core Web API and e-commerce application development.



