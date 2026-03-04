# 📚 BookBox

BookBox is a full-featured online bookstore web application built with ASP.NET Core MVC and PostgreSQL. It provides a complete e-commerce solution for selling books with features like user authentication, shopping cart, order management, reviews, and an admin dashboard.

## ✨ Features

### Customer Features
- **Book Browsing & Search**: Browse books with advanced filtering by genre, format, price range, and availability
- **Book Categories**: View bestsellers, new releases, and categorized books
- **Shopping Cart**: Add books to cart with quantity management and item selection
- **Bookmarks**: Save favorite books for later
- **Order Management**: Place orders with shipping address management
- **Review System**: Rate and review purchased books (1-5 stars with comments)
- **User Profile**: Manage personal information, addresses, and view order history
- **Authentication**: Secure user registration and login with cookie-based authentication

### Admin/Staff Features
- **Dashboard**: Comprehensive analytics with charts and statistics
- **Book Management**: Add, edit, and delete books with image uploads
- **Order Management**: View and manage customer orders
- **User Management**: Manage customer accounts and roles
- **Announcements**: Create and manage site-wide announcements
- **Discount Management**: Create and manage promotional discounts
- **Email Notifications**: Automated order confirmation emails

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core MVC (.NET 6+)
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: Cookie-based authentication with role-based authorization
- **Email Service**: SMTP email integration

### Frontend
- **UI Framework**: Bootstrap 5
- **JavaScript**: jQuery
- **Icons**: Font Awesome
- **Styling**: Custom CSS

### Architecture
- **Design Pattern**: MVC (Model-View-Controller)
- **Service Layer**: Interface-based services for business logic
- **DTOs**: Data Transfer Objects for clean data handling
- **Repository Pattern**: EF Core DbContext

## 📋 Prerequisites

- .NET 6 SDK or higher
- PostgreSQL 12 or higher
- Visual Studio 2022, VS Code, or JetBrains Rider

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/dedbot365/bookbox.git
cd bookbox
```

### 2. Database Setup
1. Install PostgreSQL and create a new database
2. Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=bookbox;Username=your_username;Password=your_password"
  }
}
```

### 3. Apply Migrations
```bash
dotnet ef database update
```

### 4. Configure Email Settings (Optional)
Update email settings in `appsettings.json` for order confirmation emails:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.your-provider.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@example.com",
    "SenderPassword": "your-password"
  }
}
```

### 5. Run the Application
```bash
dotnet run
```

Navigate to `https://localhost:5001` or `http://localhost:5000`

## 👥 User Roles

The application supports three user roles:

- **Admin**: Full system access including user management and system settings
- **Staff**: Access to book and order management
- **Member**: Standard customer account with shopping capabilities

## 📁 Project Structure

```
Bookbox/
├── Controllers/          # MVC Controllers
├── Models/              # Domain models
├── DTOs/                # Data Transfer Objects
├── Services/            # Business logic services
│   └── Interfaces/      # Service interfaces
├── Data/                # Database context
├── Views/               # Razor views
├── wwwroot/             # Static files
│   ├── css/            # Stylesheets
│   ├── js/             # JavaScript files
│   └── images/         # Image assets
└── Constants/           # Application constants
```

## � Screenshots

Screenshots of key application pages and features:

- **[Home Page](screenshots/home/)** - Landing page with featured books and announcements
- **[Shop & Browse](screenshots/shop/)** - Book catalog with filtering and search functionality
- **[Book Details](screenshots/book-details/)** - Individual book information with reviews and ratings
- **[Shopping Cart](screenshots/shopping-cart/)** - Cart management with quantity adjustment
- **[Checkout](screenshots/checkout/)** - Order completion and payment process
- **[User Profile](screenshots/profile/)** - User account management and order history
- **[Authentication](screenshots/authentication/)** - Login and registration pages
- **[Bookmarks](screenshots/bookmarks/)** - Saved books collection
- **[Order Management](screenshots/order-management/)** - Order tracking and history
- **[Admin Dashboard](screenshots/admin-dashboard/)** - Analytics and system management interface

## �🔑 Key Services

- **IAuthService**: User authentication and authorization
- **IBookService**: Book CRUD operations
- **ICartService**: Shopping cart management
- **IOrderService**: Order processing and management
- **IReviewService**: Book review system
- **IBookmarkService**: User bookmark management
- **IDiscountService**: Promotional discount management
- **IEmailService**: Email notifications
- **IChartService**: Analytics and reporting

## 🛡️ Security Features

- Password hashing for secure storage
- Role-based authorization policies
- Cookie authentication with 7-day expiration
- HTTPS redirection in production
- CSRF protection on forms

## 📝 License

This project is available for educational and personal use.

## 👤 Author

**dedbot365**
- GitHub: [@dedbot365](https://github.com/dedbot365)

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!

## 📧 Support

For support or questions, please open an issue in the GitHub repository.

---

Made with ❤️ using ASP.NET Core
```
