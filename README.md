# 📦 Bin Width Calculator API

A **production-ready .NET 7 Web API** for calculating the minimum bin width required for customer orders — featuring **JWT authentication**, **clean architecture**, and **comprehensive testing**.

---

## 🚀 Features

- **🧾 Order Management** — Create and manage orders, automatically calculating required bin width based on product types  
- **🔐 JWT Authentication** — Secure API endpoints with token-based authentication  
- **🧑‍💼 Role-based Authorization** — Built-in `User` and `Admin` roles with appropriate permissions  
- **🏗️ Clean Architecture** — Organized into `Domain`, `Application`, and `Infrastructure` layers for maintainability  
- **🗃️ SQLite Database** — Lightweight, file-based database powered by Entity Framework Core  
- **🧪 Comprehensive Testing** — Includes unit tests, integration tests, and authentication tests  
- **🐳 Docker Support** — Fully containerized for local or cloud deployment  
- **⚙️ CI/CD Pipeline** — Automated build, test, and deployment to **Azure Container Instances**  
- **📘 Swagger Documentation** — Interactive OpenAPI documentation for easy API exploration


## 📁 Project Structure

```mermaid
%%{init: {'theme': 'neutral', 'themeVariables': { 'primaryColor': '#1f77b4', 'edgeLabelBackground':'#ffffff', 'tertiaryColor': '#f9f9f9'}}}%%
graph TD
    A[BinWidthCalculator] --> B[BinWidthCalculator.API]
    A --> C[BinWidthCalculator.Tests]

    B --> B1[Controllers]
    B --> B2[Application]
    B --> B3[Domain]
    B --> B4[Infrastructure]
    B --> B5[Program.cs]

    B2 --> B2a[DTOs / Services / Validators]
    B3 --> B3a[Entities / Interfaces / Enums]
    B4 --> B4a[DbContext / Repositories]
    C --> C1[Unit & Integration Tests]