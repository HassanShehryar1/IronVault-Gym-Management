# 🏋️ IronVault Gym Management System

A comprehensive C# console-based gym management system with role-based access control for managing members, staff, trainers, equipment, and payments.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [System Architecture](#system-architecture)
- [User Roles](#user-roles)
- [Technologies Used](#technologies-used)
- [Installation](#installation)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [Database Schema](#database-schema)
- [Contributing](#contributing)
- [License](#license)

## 🎯 Overview

IronVault is a full-featured gym management system designed to streamline operations for gym owners, receptionists, and trainers. The system handles member management, staff operations, equipment tracking, payment processing, and comprehensive reporting.

## ✨ Features

### 👥 Member Management
- New member registration with membership plans
- Member check-in tracking
- Membership renewal processing
- View detailed member information
- Terminate memberships
- Automated expiry notifications

### 👔 Staff Management
- Add new staff members (Receptionists/Trainers)
- View all staff or active staff only
- Terminate staff employment
- Role-based salary tracking
- Automated salary payment system with duplicate prevention

### 💪 Trainer Management
- Assign members to trainers
- View trainer's assigned members
- Track trainer workload

### 🛠️ Equipment Management
- Place equipment orders
- Track order history
- Mark equipment payments as paid/unpaid
- Monitor equipment expenses

### 💰 Payment Processing
- Member membership payments
- Equipment purchase payments
- Staff salary payments
- Payment history tracking
- Detailed payment reports

### 📊 Reporting & Analytics
- Revenue reports
- Payment history
- Membership statistics
- Staff performance tracking
- Equipment expense analysis

### 🔐 Authentication System
- Role-based login (Owner, Receptionist, Trainer)
- Secure username/password authentication
- Role-specific dashboards and permissions

## 🏗️ System Architecture

The application follows a **three-tier architecture**:

```
┌─────────────────────────────────┐
│   Presentation Layer (UI)       │
│   - Console Interface           │
│   - User Input Handling         │
│   - Display Logic               │
└─────────────┬───────────────────┘
              │
┌─────────────▼───────────────────┐
│   Business Logic Layer (BLL)    │
│   - Business Rules              │
│   - Validation                  │
│   - Event Handling              │
└─────────────┬───────────────────┘
              │
┌─────────────▼───────────────────┐
│   Data Access Layer (DAL)       │
│   - Data Storage                │
│   - CRUD Operations             │
│   - In-Memory Collections       │
└─────────────────────────────────┘
```

### Files Structure:
- **Presentation.cs** - User interface and console interaction
- **Business_Logic.cs** - Business rules, validation, and services
- **Data_Access.cs** - Data models and repository pattern

## 👤 User Roles

### 🏆 Owner
- Full system access
- View all reports and analytics
- Manage staff (add/terminate)
- View equipment orders and payments
- Process staff salary payments
- Access revenue and payment reports

### 📞 Receptionist
- Register new members
- Member check-in
- Renew memberships
- View membership plans
- View payment history
- View and manage member details
- Terminate memberships

### 🏋️ Trainer
- View assigned members
- View member details
- Limited access to member information

## 🛠️ Technologies Used

- **Language**: C# (.NET Framework)
- **Architecture**: Three-tier (Presentation, Business Logic, Data Access)
- **Design Patterns**: 
  - Repository Pattern
  - Event-Driven Architecture
  - Service Layer Pattern
- **Data Storage**: In-memory collections (Lists and Dictionaries)

## 📥 Installation

### Prerequisites
- .NET Framework 4.7.2 or higher
- Visual Studio 2019 or later (or any C# IDE)

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/IronVault-Gym-Management.git
   ```

2. **Open the project**
   - Open Visual Studio
   - File → Open → Project/Solution
   - Navigate to the cloned folder and open the `.sln` file

3. **Build the solution**
   - Press `Ctrl + Shift + B` or
   - Build → Build Solution

4. **Run the application**
   - Press `F5` or
   - Debug → Start Debugging

## 🚀 Usage

### Default Login Credentials

#### Owner
```
Username: owner
Password: owner123
```

#### Receptionist
```
Username: receptionist
Password: receptionist123
```

#### Trainer
```
Username: trainer
Password: trainer123
```

### Main Workflow

1. **Launch Application**
   - Select your role (Owner/Receptionist/Trainer)
   - Enter credentials

2. **Receptionist Workflow**
   ```
   Login → Register Member → Select Plan → Process Payment → Member Active
   ```

3. **Owner Workflow**
   ```
   Login → View Reports → Manage Staff → Process Salaries → View Analytics
   ```

4. **Trainer Workflow**
   ```
   Login → View Assigned Members → Check Member Details
   ```

## 📁 Project Structure

```
IronVault-Gym-Management/
│
├── Presentation.cs          # UI Layer
│   ├── MainController       # Main application controller
│   ├── NotificationHandler  # Event notifications
│   ├── SalaryNotificationHandler
│   └── Program             # Entry point
│
├── Business_Logic.cs        # Business Layer
│   ├── GymService          # Core business logic
│   ├── Event Args Classes  # Custom event arguments
│   └── Business Rules      # Validation and rules
│
├── Data_Access.cs           # Data Layer
│   ├── Models              # Data models (Member, Staff, etc.)
│   ├── GymRepository       # Data repository
│   └── CRUD Operations     # Data access methods
│
└── README.md               # Documentation
```

## 🗄️ Database Schema

### Main Entities

#### Member
- MemberID (int)
- Name (string)
- Email (string)
- Phone (string)
- PlanID (int)
- JoinDate (DateTime)
- ExpiryDate (DateTime)
- IsActive (bool)

#### Staff
- StaffID (int)
- Name (string)
- Role (string)
- Salary (decimal)
- Username (string)
- Password (string)
- IsActive (bool)

#### MembershipPlan
- PlanID (int)
- PlanName (string)
- DurationMonths (int)
- Price (decimal)

#### Payment
- PaymentID (int)
- MemberID (int)
- Amount (decimal)
- PaymentDate (DateTime)

#### EquipmentOrder
- OrderID (int)
- EquipmentName (string)
- Quantity (int)
- TotalPrice (decimal)
- OrderDate (DateTime)
- IsPaid (bool)

#### SalaryPayment
- PaymentID (int)
- StaffID (int)
- Amount (decimal)
- Month (int)
- Year (int)
- PaymentDate (DateTime)

## 🎯 Key Features Implementation

### Event-Driven Architecture
The system uses C# events for notifications:
- **MembershipExpiring** - Triggers when membership is about to expire
- **SalaryPaid** - Triggers when staff salary is processed

### Validation & Business Rules
- Duplicate salary payment prevention
- Membership expiry tracking
- Active/Inactive status management
- Role-based access control

### User Experience
- Color-coded console output
- Clear navigation menus
- Confirmation prompts for critical actions
- Detailed error messages

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards
- Follow C# naming conventions
- Add XML documentation comments for public methods
- Include unit tests for new features
- Maintain the three-tier architecture

## 📝 Future Enhancements

- [ ] Database integration (SQL Server/MySQL)
- [ ] Web-based interface (ASP.NET)
- [ ] Mobile app integration
- [ ] Attendance tracking with biometric support
- [ ] Email/SMS notifications
- [ ] Payment gateway integration
- [ ] Advanced reporting and analytics
- [ ] Member mobile app
- [ ] Inventory management for gym supplies
- [ ] Workout plan management

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Your Name**
- GitHub: [@yourusername](https://github.com/yourusername)
- Email: your.email@example.com

## 🙏 Acknowledgments

- Inspired by real-world gym management needs
- Built with clean architecture principles
- Designed for extensibility and maintainability

## 📞 Support

For support, please open an issue in the GitHub repository or contact the maintainer.

---

**Made with ❤️ for efficient gym management**

⭐ Star this repository if you find it helpful!
