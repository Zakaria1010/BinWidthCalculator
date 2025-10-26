## 🧰 Tech Stack

- **.NET 7**  
- **Entity Framework Core (SQLite)**  
- **JWT Authentication (Microsoft.IdentityModel.Tokens)**  
- **FluentValidation**  
- **xUnit** for testing  
- **GitHub Actions** for CI/CD  
- **Azure Container Registry (ACR)** + **Azure Container Instances (ACI)**  

---

## 📋 Business Requirements
 **Order Processing**
Customers can order 1 or multiple items (photo books, calendars, canvases, cards, mugs)

Calculate minimum bin width required for orders at creation time

Retrieve order information by ID

**Authentication & Authorization**
JWT Bearer token authentication

Role-based access control (User, Admin)

Secure password handling

User registration and login endpoints

## 📦 Development Steps
**Phase 1: Project Setup & Basic Structure**
Created solution structure with clean architecture

Set up projects: API(Domain, Application, Infrastructure), Tests

Configured dependencies and project references

**Phase 2: Domain Layer Implementation**
Defined entities: Order, OrderItem, ProductType enum

Created interfaces: IOrderRepository, IBinWidthCalculator

**Phase 3: Business Logic Implementation**
Implemented bin width calculation service with mug stacking logic

Created order service for business operations

Added validation using FluentValidation

Implemented DTOs for API contracts

**Phase 4: Data Access & Infrastructure**
Configured Entity Framework Core with SQLite

Implemented repository pattern

Set up database context and migrations

Configured dependency injection

Established business contracts and domain models

**Phase 5: API Layer Development**
Created RESTful controllers: OrdersController

Implemented endpoints:

POST /api/orders - Create order with bin width calculation

GET /api/orders/{id} - Retrieve order by ID

Added error handling and proper HTTP status codes

Configured Swagger for API documentation

**Phase 6: Authentication & Authorization**
Added JWT authentication with bearer tokens

Implemented user management with roles

Created AuthController with login/register endpoints

Added role-based authorization to protected endpoints

Configured secure password handling

**Phase 7: Testing Implementation**
Unit tests for business logic and services

Integration tests for API endpoints

Authentication tests for security features

Comprehensive test coverage for all major components

**Phase 8: Containerization & Deployment**
Created Dockerfile for .NET 7 application

Set up Docker Compose for local development

Configured GitHub Actions CI/CD pipeline

Prepared Azure deployment configuration

## 🚀 Deployment
**Azure Deployment with GitHub Actions**
Set up Azure resources using provided PowerShell script

### Configure GitHub Secrets:

ACR_USERNAME - Azure Container Registry username

ACR_PASSWORD - Azure Container Registry password

AZURE_CREDENTIALS - Azure service principal credentials

Push to main branch to trigger automated deployment
