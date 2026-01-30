using IronVault.BLL;
using IronVault.DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronVault.Presentation
{
    // ==================== NOTIFICATION HANDLER ====================
    public class NotificationHandler
    {
        public void SendExpiryNotification(object sender, MembershipExpiringEventArgs e)
        {
            Console.WriteLine($"\n[EMAIL SENT] Member {e.MemberName} (ID: {e.MemberID}) - Membership expires tomorrow!");
            Console.WriteLine($"Email sent to: {e.Email}");
        }
    }

    // ==================== NEW: SALARY NOTIFICATION HANDLER ====================
    public class SalaryNotificationHandler
    {
        public void OnSalaryPaid(object sender, SalaryPaidEventArgs e)
        {
            Console.Clear();
            Console.WriteLine("========================================");
            if (e.AlreadyPaid)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️  ALREADY PAID NOTICE");
                Console.ResetColor();
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"Staff: {e.StaffName} (ID: {e.StaffID})");
                Console.WriteLine($"Amount: ${e.Amount:N2}");
                Console.WriteLine($"Period: {e.Month} / {e.Year}");
                Console.WriteLine($"\nThis staff member was already paid for this month.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ SALARY PAYMENT SUCCESSFUL");
                Console.ResetColor();
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"Staff: {e.StaffName} (ID: {e.StaffID})");
                Console.WriteLine($"Amount: ${e.Amount:N2}");
                Console.WriteLine($"Period: {e.Month} / {e.Year}");
                Console.WriteLine($"Payment Date: {DateTime.Now:dd-MMM-yyyy HH:mm}");
                Console.WriteLine($"\n💰 Payment recorded successfully!");
            }
            Console.WriteLine("========================================");
            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }
    }

    // ==================== MAIN CONSOLE CONTROLLER ====================
    public class MainController
    {
        private GymService _gymService;
        private NotificationHandler _notificationHandler;
        private SalaryNotificationHandler _salaryNotificationHandler;

        public MainController()
        {
            _gymService = new GymService(new GymRepository());
            _notificationHandler = new NotificationHandler();
            _salaryNotificationHandler = new SalaryNotificationHandler();
        }

        public void Initialize()
        {
            _gymService.OnMembershipExpiring += _notificationHandler.SendExpiryNotification;
            _gymService.OnSalaryPaid += _salaryNotificationHandler.OnSalaryPaid;
            Console.WriteLine("✓ System Initialized. Events Connected.\n");
        }

        private void ShowPaymentHistory()
        {
            Console.Clear();
            Console.WriteLine("=== RECENT PAYMENT HISTORY ===\n");

            var payments = _gymService.GetDetailedPaymentHistory();

            if (payments.Count == 0)
            {
                Console.WriteLine("No recent payments found.");
            }
            else
            {
                Console.WriteLine(string.Format("{0,-10} | {1,-10} | {2,-15} | {3,-15}", "Pay ID", "Member ID", "Amount", "Date"));
                Console.WriteLine(new string('-', 60));

                foreach (var p in payments)
                {
                    Console.WriteLine(string.Format("{0,-10} | {1,-10} | ${2,-14:N2} | {3,-15:dd-MMM-yyyy}",
                        p.PaymentID, p.MemberID, p.Amount, p.PaymentDate));
                }
            }

            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        // ==================== MAIN MENU ====================
        public void ShowMainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("       IRONVAULT GYM MANAGEMENT         ");
                Console.WriteLine("\n1. Receptionist Login");
                Console.WriteLine("2. Owner Login");
                Console.WriteLine("3. Trainer Login");
                Console.WriteLine("4. Exit");
                Console.Write("\nSelect Role: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": ReceptionistLoginFlow(); break;
                    case "2": OwnerLoginFlow(); break;
                    case "3": TrainerLoginFlow(); break;
                    case "4": return;
                    default:
                        ShowPopup("Invalid choice!", ConsoleColor.Red);
                        break;
                }
            }
        }

        // ==================== LOGIN FLOWS ====================
        private void ReceptionistLoginFlow()
        {
            Console.Clear();
            Console.WriteLine("=== RECEPTIONIST LOGIN ===\n");
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            if (_gymService.LoginReceptionist(username, password))
            {
                ShowReceptionistDashboard();
            }
            else
            {
                ShowPopup("Invalid credentials!", ConsoleColor.Red);
            }
        }

        private void OwnerLoginFlow()
        {
            Console.Clear();
            Console.WriteLine("=== OWNER LOGIN ===\n");
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            if (_gymService.LoginOwner(username, password))
            {
                ShowOwnerDashboard();
            }
            else
            {
                ShowPopup("Invalid credentials!", ConsoleColor.Red);
            }
        }

        private void TrainerLoginFlow()
        {
            Console.Clear();
            Console.WriteLine("=== TRAINER LOGIN ===\n");
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            if (_gymService.LoginTrainer(username, password))
            {
                ShowTrainerDashboard();
            }
            else
            {
                ShowPopup("Invalid credentials!", ConsoleColor.Red);
            }
        }

        // ==================== RECEPTIONIST DASHBOARD ====================
        private void ShowReceptionistDashboard()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("       RECEPTIONIST DASHBOARD           ");

                Console.WriteLine("1. New Member Registration");
                Console.WriteLine("2. Check-in Member");
                Console.WriteLine("3. Renew Membership");
                Console.WriteLine("4. View Plans");
                Console.WriteLine("5. View Payment History");
                Console.WriteLine("6. View Member Details");
                Console.WriteLine("7. Terminate Member Membership");
                Console.WriteLine("8. Logout");
                Console.Write("\nChoice: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1": RegisterNewMember(); break;
                    case "2": CheckInMember(); break;
                    case "3": RenewMembershipFlow(); break;
                    case "4":
                        ShowPlanSelection();
                        Console.WriteLine("\n[Press any key to continue...]");
                        Console.ReadKey();
                        break;
                    case "5": ShowPaymentHistory(); break;
                    case "6": ViewMemberDetails(); break;
                    case "7": TerminateMemberMembership(); break;
                    case "8": return;
                }
            }
        }

        // ==================== RECEPTIONIST FUNCTIONS ====================
        private void RegisterNewMember()
        {
            Console.Clear();
            Console.WriteLine("=== NEW MEMBER REGISTRATION ===\n");

            Console.Write("Name: ");
            string name = Console.ReadLine();
            Console.Write("Email: ");
            string email = Console.ReadLine();

            ShowPlanSelection();
            Console.Write("\nSelect Plan ID (1-3): ");

            if (!int.TryParse(Console.ReadLine(), out int planId)) return;

            var credentials = _gymService.RegisterNewMember(name, email, planId);
            var plan = _gymService.GetPlanById(planId);

            Console.Clear();
            Console.WriteLine("      REGISTRATION SUCCESSFUL!          ");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Member ID: {credentials.AssignedMemberID}");
            Console.WriteLine($"Password:  {credentials.GeneratedPassword}");
            Console.WriteLine($"Plan Type: {plan.PlanName}");
            Console.WriteLine($"Monthly:   ${plan.Price}");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        private void CheckInMember()
        {
            Console.Clear();
            Console.WriteLine("=== MEMBER CHECK-IN ===\n");
            Console.Write("Enter Member ID: ");

            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowPopup("Invalid ID!", ConsoleColor.Red);
                return;
            }

            bool allowed = _gymService.ValidateCheckIn(id);

            if (allowed)
            {
                var member = _gymService.GetMemberById(id);
                Console.WriteLine("\n✓ ACCESS GRANTED");
                Console.WriteLine($"Welcome, {member.Name}!");
                Console.WriteLine($"Plan: {_gymService.GetPlanById(member.PlanID).PlanName}");
                Console.WriteLine($"Expires: {member.ExpiryDate:dd-MMM-yyyy}");
            }
            else
            {
                Console.WriteLine("\n✗ ACCESS DENIED - Membership Expired!");
            }

            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        private void RenewMembershipFlow()
        {
            Console.Clear();
            Console.WriteLine("=== RENEW MEMBERSHIP ===\n");
            Console.Write("Member ID: ");

            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowPopup("Invalid ID!", ConsoleColor.Red);
                return;
            }

            var member = _gymService.GetMemberById(id);
            if (member == null)
            {
                ShowPopup("Member not found!", ConsoleColor.Red);
                return;
            }

            Console.WriteLine($"\nCurrent Member: {member.Name}");
            Console.WriteLine($"Current Plan: {_gymService.GetPlanById(member.PlanID).PlanName}");
            Console.WriteLine($"Current Expiry: {member.ExpiryDate:dd-MMM-yyyy}");

            ShowPlanSelection();
            Console.Write("\nSelect New Plan ID (1-3): ");

            if (!int.TryParse(Console.ReadLine(), out int planId))
            {
                ShowPopup("Invalid Plan ID!", ConsoleColor.Red);
                return;
            }

            DateTime newExpiry = _gymService.RenewMembership(id, planId);
            var plan = _gymService.GetPlanById(planId);

            Console.Clear();
            Console.WriteLine("      RENEWAL SUCCESSFUL!             ");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Member: {member.Name}");
            Console.WriteLine($"New Plan: {plan.PlanName}");
            Console.WriteLine($"New Expiry: {newExpiry:dd-MMM-yyyy}");
            Console.WriteLine($"Amount Paid: ${plan.Price}");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        private void ShowPlanSelection()
        {
            var plans = _gymService.GetAvailablePlans();
            Console.WriteLine("\n=== AVAILABLE PLANS ===");
            foreach (var plan in plans)
            {
                Console.WriteLine($"[{plan.PlanID}] {plan.PlanName} - ${plan.Price}/month");
                Console.WriteLine($"    Trainer: {(plan.IncludesTrainer ? "Yes" : "No")} | Supplements: {(plan.IncludesSupplements ? "Yes" : "No")}");
            }
        }

        private void ViewMemberDetails()
        {
            Console.Clear();
            Console.WriteLine("=== VIEW MEMBER DETAILS ===\n");
            Console.Write("Enter Member ID: ");

            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowPopup("Invalid ID!", ConsoleColor.Red);
                return;
            }

            Console.Write("Enter Member Password: ");
            string password = Console.ReadLine();

            var member = _gymService.GetMemberByIdAndPassword(id, password);

            if (member == null)
            {
                ShowPopup("Invalid credentials or member not found!", ConsoleColor.Red);
                return;
            }

            Console.Clear();
            Console.WriteLine("=== MEMBER DETAILS ===\n");
            Console.WriteLine($"ID:           {member.MemberID}");
            Console.WriteLine($"Name:         {member.Name}");
            Console.WriteLine($"Email:        {member.Email}");
            Console.WriteLine($"Plan:         {_gymService.GetPlanById(member.PlanID).PlanName}");
            Console.WriteLine($"Status:       {member.Status}");
            Console.WriteLine($"Expiry Date:  {member.ExpiryDate:dd-MMM-yyyy}");

            int daysLeft = (member.ExpiryDate - DateTime.Now).Days;
            if (daysLeft > 0)
                Console.WriteLine($"Days Left:    {daysLeft} days");
            else
                Console.WriteLine($"Days Left:    EXPIRED");

            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        private void TerminateMemberMembership()
        {
            Console.Clear();
            Console.WriteLine("=== TERMINATE MEMBER MEMBERSHIP ===\n");
            Console.Write("Enter Member ID: ");

            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowPopup("Invalid ID!", ConsoleColor.Red);
                return;
            }

            var member = _gymService.GetMemberById(id);
            if (member == null)
            {
                ShowPopup("Member not found!", ConsoleColor.Red);
                return;
            }

            Console.WriteLine($"\nMember: {member.Name}");
            Console.WriteLine($"Current Status: {member.Status}");
            Console.Write("\nAre you sure you want to terminate? (Y/N): ");

            string confirm = Console.ReadLine();
            if (confirm?.ToUpper() == "Y")
            {
                _gymService.TerminateMembership(id);
                ShowPopup("Member membership terminated successfully!", ConsoleColor.Green);
            }
            else
            {
                ShowPopup("Operation cancelled.", ConsoleColor.Yellow);
            }
        }

        // ==================== OWNER DASHBOARD (UPDATED) ====================
        private void ShowOwnerDashboard()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("       OWNER DASHBOARD                  ");
                Console.WriteLine("------------------------------------------");
                Console.WriteLine("1. View Statistics");
                Console.WriteLine("2. View Loyal Members");
                Console.WriteLine("3. Machine & Equipment Management");
                Console.WriteLine("4. Staff Management");
                Console.WriteLine("5. 💰 Financial Management");
                Console.WriteLine("6. 💵 Salary Management");
                Console.WriteLine("7. 📋 Expense History");
                Console.WriteLine("8. Logout");
                Console.Write("\nChoice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": ShowStatistics(); break;
                    case "2": ShowLoyalMembers(); break;
                    case "3": ShowMachineInventoryMenu(); break;
                    case "4": ShowStaffManagement(); break;
                    case "5": ShowFinancialManagementMenu(); break;
                    case "6": ShowSalaryManagementMenu(); break;
                    case "7": ShowExpenseHistory(); break;
                    case "8": return;
                    default:
                        ShowPopup("Invalid choice!", ConsoleColor.Red);
                        break;
                }
            }
        }

        // ==================== NEW: FINANCIAL MANAGEMENT MENU ====================
        private void ShowFinancialManagementMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("       FINANCIAL MANAGEMENT              ");
                Console.WriteLine("------------------------------------------");
                Console.WriteLine("1. View Financial Summary");
                Console.WriteLine("2. View Revenue Details");
                Console.WriteLine("3. View Expense Breakdown");
                Console.WriteLine("4. Back to Main Menu");
                Console.Write("\nChoice: ");

                string choice = Console.ReadLine();
                if (choice == "4") break;

                switch (choice)
                {
                    case "1": ShowFinancialSummary(); break;
                    case "2": ShowRevenueDetails(); break;
                    case "3": ShowExpenseBreakdown(); break;
                    default:
                        ShowPopup("Invalid choice!", ConsoleColor.Red);
                        break;
                }
            }
        }

        // ==================== NEW: FINANCIAL SUMMARY ====================
        private void ShowFinancialSummary()
        {
            Console.Clear();
            Console.WriteLine("=== 💰 FINANCIAL SUMMARY ===\n");

            var summary = _gymService.GetFinancialSummary();

            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Total Revenue:        ${summary.TotalRevenue:N2}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Total Expenses:       ${summary.TotalExpenses:N2}");
            Console.WriteLine($"  - Salaries Paid:    ${summary.TotalSalariesPaid:N2}");
            Console.ResetColor();

            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            if (summary.NetProfit >= 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Net Profit:           ${summary.NetProfit:N2}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Net Loss:             ${Math.Abs(summary.NetProfit):N2}");
            }
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Available Balance:    ${summary.AvailableBalance:N2}");
            Console.ResetColor();

            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        // ==================== NEW: REVENUE DETAILS ====================
        private void ShowRevenueDetails()
        {
            Console.Clear();
            Console.WriteLine("=== 💵 REVENUE DETAILS ===\n");

            var payments = _gymService.GetDetailedPaymentHistory();
            var summary = _gymService.GetFinancialSummary();

            Console.WriteLine($"Total Revenue: ${summary.TotalRevenue:N2}");
            Console.WriteLine($"Total Payments: {payments.Count}\n");

            Console.WriteLine(string.Format("{0,-10} | {1,-10} | {2,-15} | {3,-15}",
                "Pay ID", "Member ID", "Amount", "Date"));
            Console.WriteLine(new string('-', 60));

            foreach (var p in payments.Take(20)) // Show last 20
            {
                Console.WriteLine(string.Format("{0,-10} | {1,-10} | ${2,-14:N2} | {3,-15:dd-MMM-yyyy}",
                    p.PaymentID, p.MemberID, p.Amount, p.PaymentDate));
            }

            if (payments.Count > 20)
            {
                Console.WriteLine($"\n... and {payments.Count - 20} more payments");
            }

            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        // ==================== NEW: EXPENSE BREAKDOWN ====================
        private void ShowExpenseBreakdown()
        {
            Console.Clear();
            Console.WriteLine("=== 📊 EXPENSE BREAKDOWN ===\n");

            var expenses = _gymService.GetAllExpenses();
            var summary = _gymService.GetFinancialSummary();

            var groupedExpenses = expenses.GroupBy(e => e.ExpenseType)
                .Select(g => new { Type = g.Key, Total = g.Sum(e => e.Amount), Count = g.Count() })
                .OrderByDescending(g => g.Total);

            Console.WriteLine($"Total Expenses: ${summary.TotalExpenses:N2}\n");

            Console.WriteLine("By Category:");
            Console.WriteLine(string.Format("{0,-20} | {1,-15} | {2,-10}", "Type", "Total", "Count"));
            Console.WriteLine(new string('-', 50));

            foreach (var group in groupedExpenses)
            {
                Console.WriteLine(string.Format("{0,-20} | ${1,-14:N2} | {2,-10}",
                    group.Type, group.Total, group.Count));
            }

            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        // ==================== NEW: SALARY MANAGEMENT MENU ====================
        private void ShowSalaryManagementMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("       SALARY MANAGEMENT                 ");
                Console.WriteLine("------------------------------------------");
                Console.WriteLine("1. View All Staff & Salary Status");
                Console.WriteLine("2. Pay Salary for Staff Member");
                Console.WriteLine("3. View All Salary Payments");
                Console.WriteLine("4. View Salary History (Single Staff)");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("\nChoice: ");

                string choice = Console.ReadLine();
                if (choice == "5") break;

                switch (choice)
                {
                    case "1": ShowStaffSalaryStatus(); break;
                    case "2": PayStaffSalary(); break;
                    case "3": ShowAllSalaryPayments(); break;
                    case "4": ShowSingleStaffSalaryHistory(); break;
                    default:
                        ShowPopup("Invalid choice!", ConsoleColor.Red);
                        break;
                }
            }
        }

        // ==================== NEW: STAFF SALARY STATUS ====================
        private void ShowStaffSalaryStatus()
        {
            Console.Clear();
            Console.WriteLine("=== STAFF SALARY STATUS ===\n");

            var staff = _gymService.GetActiveStaff();
            string currentMonth = DateTime.Now.ToString("yyyy-MM");

            Console.WriteLine($"Current Month: {DateTime.Now:MMMM yyyy}\n");

            Console.WriteLine(string.Format("{0,-5} | {1,-20} | {2,-15} | {3,-12} | {4,-10}",
                "ID", "Name", "Role", "Salary", "Paid?"));
            Console.WriteLine(new string('-', 75));

            foreach (var s in staff)
            {
                var salaryHistory = _gymService.GetSalaryHistory(s.StaffID);
                bool paidThisMonth = salaryHistory.Any(sp => sp.Month == currentMonth);

                Console.Write(string.Format("{0,-5} | {1,-20} | {2,-15} | ${3,-11:N2} | ",
                    s.StaffID, s.Name, s.Role, s.Salary));

                if (paidThisMonth)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✓ PAID");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("✗ UNPAID");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        // ==================== NEW: PAY STAFF SALARY ====================
        private void PayStaffSalary()
        {
            Console.Clear();
            Console.WriteLine("=== PAY STAFF SALARY ===\n");

            // Show available balance first
            var financials = _gymService.GetFinancialSummary();
            Console.WriteLine($"Available Balance: ${financials.AvailableBalance:N2}\n");

            // Show active staff
            var staff = _gymService.GetActiveStaff();
            Console.WriteLine("Active Staff:");
            Console.WriteLine(string.Format("{0,-5} | {1,-20} | {2,-15} | {3,-12}",
                "ID", "Name", "Role", "Salary"));
            Console.WriteLine(new string('-', 60));

            foreach (var s in staff)
            {
                Console.WriteLine(string.Format("{0,-5} | {1,-20} | {2,-15} | ${3,-11:N2}",
                    s.StaffID, s.Name, s.Role, s.Salary));
            }

            Console.Write("\nEnter Staff ID to pay: ");
            if (!int.TryParse(Console.ReadLine(), out int staffId))
            {
                ShowPopup("Invalid ID!", ConsoleColor.Red);
                return;
            }

            try
            {
                bool alreadyPaid = _gymService.PayStaffSalary(staffId);
                // The event handler will display the result
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ PAYMENT FAILED");
                Console.ResetColor();
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("\n[Press any key to continue...]");
                Console.ReadKey();
            }
        }

        // ==================== NEW: ALL SALARY PAYMENTS ====================
        private void ShowAllSalaryPayments()
        {
            Console.Clear();
            Console.WriteLine("=== ALL SALARY PAYMENTS ===\n");

            var payments = _gymService.GetAllSalaryPayments();

            if (payments.Count == 0)
            {
                Console.WriteLine("No salary payments found.");
            }
            else
            {
                Console.WriteLine($"Total Payments: {payments.Count}\n");

                Console.WriteLine(string.Format("{0,-8} | {1,-10} | {2,-12} | {3,-12} | {4,-15}",
                    "Pay ID", "Staff ID", "Amount", "Month", "Date"));
                Console.WriteLine(new string('-', 70));

                foreach (var p in payments.OrderByDescending(p => p.PaymentDate).Take(30))
                {
                    Console.WriteLine(string.Format("{0,-8} | {1,-10} | ${2,-11:N2} | {3,-12} | {4,-15:dd-MMM-yyyy}",
                        p.SalaryPaymentID, p.StaffID, p.Amount, p.Month, p.PaymentDate));
                }

                if (payments.Count > 30)
                {
                    Console.WriteLine($"\n... and {payments.Count - 30} more payments");
                }
            }

            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        // ==================== NEW: SINGLE STAFF SALARY HISTORY ====================
        private void ShowSingleStaffSalaryHistory()
        {
            Console.Clear();
            Console.WriteLine("=== STAFF SALARY HISTORY ===\n");

            Console.Write("Enter Staff ID: ");
            if (!int.TryParse(Console.ReadLine(), out int staffId))
            {
                ShowPopup("Invalid ID!", ConsoleColor.Red);
                return;
            }

            var staff = _gymService.GetStaffById(staffId);
            if (staff == null)
            {
                ShowPopup("Staff not found!", ConsoleColor.Red);
                return;
            }

            Console.Clear();
            Console.WriteLine($"=== SALARY HISTORY: {staff.Name} ===\n");
            Console.WriteLine($"Staff ID: {staff.StaffID}");
            Console.WriteLine($"Role: {staff.Role}");
            Console.WriteLine($"Current Salary: ${staff.Salary:N2}\n");

            var history = _gymService.GetSalaryHistory(staffId);

            if (history.Count == 0)
            {
                Console.WriteLine("No salary payments found for this staff member.");
            }
            else
            {
                Console.WriteLine($"Total Payments: {history.Count}");
                Console.WriteLine($"Total Paid: ${history.Sum(h => h.Amount):N2}\n");

                Console.WriteLine(string.Format("{0,-8} | {1,-12} | {2,-12} | {3,-15}",
                    "Pay ID", "Amount", "Month", "Date"));
                Console.WriteLine(new string('-', 60));

                foreach (var h in history.OrderByDescending(h => h.PaymentDate))
                {
                    Console.WriteLine(string.Format("{0,-8} | ${1,-11:N2} | {2,-12} | {3,-15:dd-MMM-yyyy}",
                        h.SalaryPaymentID, h.Amount, h.Month, h.PaymentDate));
                }
            }

            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        // ==================== NEW: EXPENSE HISTORY ====================
        private void ShowExpenseHistory()
        {
            Console.Clear();
            Console.WriteLine("=== 📋 EXPENSE HISTORY ===\n");

            var expenses = _gymService.GetAllExpenses();

            if (expenses.Count == 0)
            {
                Console.WriteLine("No expenses found.");
            }
            else
            {
                Console.WriteLine($"Total Expenses: ${expenses.Sum(e => e.Amount):N2}");
                Console.WriteLine($"Total Records: {expenses.Count}\n");

                Console.WriteLine(string.Format("{0,-8} | {1,-15} | {2,-30} | {3,-12} | {4,-12}",
                    "ID", "Type", "Description", "Amount", "Date"));
                Console.WriteLine(new string('-', 95));

                foreach (var e in expenses.OrderByDescending(e => e.ExpenseDate).Take(30))
                {
                    string desc = e.Description.Length > 30 ? e.Description.Substring(0, 27) + "..." : e.Description;
                    Console.WriteLine(string.Format("{0,-8} | {1,-15} | {2,-30} | ${3,-11:N2} | {4,-12:dd-MMM-yyyy}",
                        e.ExpenseID, e.ExpenseType, desc, e.Amount, e.ExpenseDate));
                }

                if (expenses.Count > 30)
                {
                    Console.WriteLine($"\n... and {expenses.Count - 30} more expenses");
                }
            }

            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        // ==================== OWNER STATISTICS ====================
        private void ShowStatistics()
        {
            Console.Clear();
            Console.WriteLine("=== GYM STATISTICS ===\n");

            int activeMembers = _gymService.GetCurrentActiveMembers();
            decimal monthlyRevenue = _gymService.CalculateMonthlyRevenue();
            var summary = _gymService.GetFinancialSummary();

            Console.WriteLine($"Active Members:     {activeMembers}");
            Console.WriteLine($"Total Revenue:      ${summary.TotalRevenue:N2}");
            Console.WriteLine($"Monthly Revenue:    ${monthlyRevenue:N2}");

            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        private void ShowLoyalMembers()
        {
            Console.Clear();
            Console.WriteLine("=== TOP LOYAL MEMBERS ===\n");

            var (avgRenewals, topLoyal) = _gymService.GetRetentionAnalysis();

            Console.WriteLine($"Average Renewals: {avgRenewals:F2}\n");
            Console.WriteLine("Top members by renewals:");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("{0,-10} | {1,-25} | {2,-15}", "ID", "Name", "Renewals");

            foreach (var member in topLoyal)
            {
                Console.WriteLine("{0,-10} | {1,-25} | {2,-15}", member.MemberID, member.MemberName, member.RenewalCount);
            }

            Console.WriteLine("\n[Press any key to return...]");
            Console.ReadKey();
        }

        // ==================== SUB-MENU: MACHINES (UPDATED) ====================
        private void ShowMachineInventoryMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("       MACHINE & EQUIPMENT MGMT           ");
                Console.WriteLine("------------------------------------------");
                Console.WriteLine("1. View All Machines");
                Console.WriteLine("2. Add New Machine (with Payment)");
                Console.WriteLine("3. Update Machine Status");
                Console.WriteLine("4. Place New Equipment Order");
                Console.WriteLine("5. View Unpaid Orders");
                Console.WriteLine("6. Pay for Equipment Order");
                Console.WriteLine("7. View Order History");
                Console.WriteLine("8. Return to Main Hub");
                Console.Write("\nChoice: ");

                string choice = Console.ReadLine();
                if (choice == "8") break;

                switch (choice)
                {
                    case "1": ShowMachinesView(); break;
                    case "2": AddNewMachineWithPayment(); break;
                    case "3": UpdateMachineStatusFlow(); break;
                    case "4": ShowEquipmentOrderForm(); break;
                    case "5": ShowUnpaidOrders(); break;
                    case "6": PayForEquipmentOrder(); break;
                    case "7": ShowEquipmentOrderHistory(); break;
                    default:
                        ShowPopup("Invalid choice!", ConsoleColor.Red);
                        break;
                }
            }
        }

        // ==================== NEW: ADD MACHINE WITH PAYMENT ====================
        private void AddNewMachineWithPayment()
        {
            Console.Clear();
            Console.WriteLine("=== ADD NEW MACHINE (WITH PAYMENT) ===\n");

            // Show available balance
            var financials = _gymService.GetFinancialSummary();
            Console.WriteLine($"Available Balance: ${financials.AvailableBalance:N2}\n");

            Console.Write("Machine Name: ");
            string name = Console.ReadLine();

            Console.Write("Purchase Price: $");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price))
            {
                ShowPopup("Invalid price!", ConsoleColor.Red);
                return;
            }

            Console.Write("Status (Working/Maintenance): ");
            string status = Console.ReadLine();

            try
            {
                _gymService.AddMachineWithPayment(name, status, price);
                ShowPopup($"Machine added and ${price:N2} paid from revenue!", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                ShowPopup($"Error: {ex.Message}", ConsoleColor.Red);
            }
        }

        // ==================== NEW: UNPAID ORDERS ====================
        private void ShowUnpaidOrders()
        {
            Console.Clear();
            Console.WriteLine("=== UNPAID EQUIPMENT ORDERS ===\n");

            var unpaidOrders = _gymService.GetUnpaidEquipmentOrders();

            if (unpaidOrders.Count == 0)
            {
                Console.WriteLine("No unpaid orders found. All orders have been paid!");
            }
            else
            {
                decimal totalUnpaid = unpaidOrders.Sum(o => o.TotalPrice);
                Console.WriteLine($"Total Unpaid: ${totalUnpaid:N2}");
                Console.WriteLine($"Orders: {unpaidOrders.Count}\n");

                Console.WriteLine(string.Format("{0,-8} | {1,-25} | {2,-10} | {3,-12} | {4,-12}",
                    "Order ID", "Equipment", "Quantity", "Price", "Date"));
                Console.WriteLine(new string('-', 80));

                foreach (var o in unpaidOrders)
                {
                    Console.WriteLine(string.Format("{0,-8} | {1,-25} | {2,-10} | ${3,-11:N2} | {4,-12:dd-MMM-yyyy}",
                        o.OrderID, o.EquipmentName, o.Quantity, o.TotalPrice, o.OrderDate));
                }
            }

            Console.WriteLine("\n[Press any key to continue...]");
            Console.ReadKey();
        }

        // ==================== NEW: PAY FOR EQUIPMENT ORDER ====================
        private void PayForEquipmentOrder()
        {
            Console.Clear();
            Console.WriteLine("=== PAY FOR EQUIPMENT ORDER ===\n");

            // Show available balance
            var financials = _gymService.GetFinancialSummary();
            Console.WriteLine($"Available Balance: ${financials.AvailableBalance:N2}\n");

            // Show unpaid orders
            var unpaidOrders = _gymService.GetUnpaidEquipmentOrders();

            if (unpaidOrders.Count == 0)
            {
                Console.WriteLine("No unpaid orders found!");
                Console.WriteLine("\n[Press any key to continue...]");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Unpaid Orders:");
            Console.WriteLine(string.Format("{0,-8} | {1,-25} | {2,-10} | {3,-12}",
                "Order ID", "Equipment", "Quantity", "Price"));
            Console.WriteLine(new string('-', 65));

            foreach (var o in unpaidOrders)
            {
                Console.WriteLine(string.Format("{0,-8} | {1,-25} | {2,-10} | ${3,-11:N2}",
                    o.OrderID, o.EquipmentName, o.Quantity, o.TotalPrice));
            }

            Console.Write("\nEnter Order ID to pay: ");
            if (!int.TryParse(Console.ReadLine(), out int orderId))
            {
                ShowPopup("Invalid Order ID!", ConsoleColor.Red);
                return;
            }

            try
            {
                _gymService.PayForEquipmentOrder(orderId);
                ShowPopup("Payment successful! Order marked as paid.", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                ShowPopup($"Error: {ex.Message}", ConsoleColor.Red);
            }
        }

        // ==================== TRAINER DASHBOARD ====================
        private void ShowTrainerDashboard()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("        TRAINER DASHBOARD              ");

                var allMembers = _gymService.GetAllMembers();
                var members = allMembers
                    .Where(m => (m.PlanID == 2 || m.PlanID == 3) && m.Status == "Active")
                    .ToList();

                Console.WriteLine("Assigned Members (Gold/Platinum Plans):\n");
                foreach (var m in members)
                {
                    Console.WriteLine($"ID: {m.MemberID} | {m.Name} | Plan: {_gymService.GetPlanById(m.PlanID).PlanName}");
                }

                Console.WriteLine("\n1. Back to Menu");
                Console.ReadKey();
                return;
            }
        }

        // ==================== OWNER FUNCTIONS ====================
        public void ShowMachinesView()
        {
            Console.Clear();
            Console.WriteLine("=== GYM MACHINES ===\n");
            var machines = _gymService.GetAllMachines();

            Console.WriteLine(string.Format("{0,-5} | {1,-25} | {2,-15} | {3,-12} | {4,-12}",
                "ID", "Name", "Status", "Price", "Purchase Date"));
            Console.WriteLine(new string('-', 80));

            foreach (var m in machines)
            {
                Console.WriteLine(string.Format("{0,-5} | {1,-25} | {2,-15} | {3,-12} | {4,-12}",
                    m.MachineID,
                    m.MachineName,
                    m.Status,
                    m.PurchasePrice.HasValue ? $"${m.PurchasePrice:N2}" : "N/A",
                    m.PurchaseDate.HasValue ? m.PurchaseDate.Value.ToString("dd-MMM-yyyy") : "N/A"));
            }

            Console.WriteLine("\n[Press any key...]");
            Console.ReadKey();
        }

        private void UpdateMachineStatusFlow()
        {
            ShowMachinesView();
            Console.Write("\nEnter Machine ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                ShowPopup("Invalid ID!", ConsoleColor.Red);
                return;
            }

            Console.Write("New Status: ");
            string status = Console.ReadLine();
            _gymService.UpdateMachineStatus(id, status);
            ShowPopup("Status updated!", ConsoleColor.Green);
        }

        public void ShowEquipmentOrderForm()
        {
            Console.Clear();
            Console.WriteLine("=== PLACE EQUIPMENT ORDER ===\n");
            Console.Write("Equipment Name: ");
            string name = Console.ReadLine();
            Console.Write("Quantity: ");
            if (!int.TryParse(Console.ReadLine(), out int qty))
            {
                ShowPopup("Invalid quantity!", ConsoleColor.Red);
                return;
            }
            Console.Write("Total Price: $");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price))
            {
                ShowPopup("Invalid price!", ConsoleColor.Red);
                return;
            }

            _gymService.PlaceEquipmentOrder(name, qty, price);
            ShowPopup("Order placed successfully! (Unpaid)", ConsoleColor.Yellow);
        }

        public void ShowEquipmentOrderHistory()
        {
            Console.Clear();
            Console.WriteLine("=== EQUIPMENT ORDER HISTORY ===\n");
            var orders = _gymService.GetEquipmentOrderHistory();

            Console.WriteLine(string.Format("{0,-8} | {1,-25} | {2,-8} | {3,-12} | {4,-12} | {5,-8}",
                "Order ID", "Equipment", "Quantity", "Price", "Date", "Status"));
            Console.WriteLine(new string('-', 90));

            foreach (var o in orders)
            {
                string status = o.IsPaid ? "PAID" : "UNPAID";
                ConsoleColor statusColor = o.IsPaid ? ConsoleColor.Green : ConsoleColor.Yellow;

                Console.Write(string.Format("{0,-8} | {1,-25} | {2,-8} | ${3,-11:N2} | {4,-12:dd-MMM-yyyy} | ",
                    o.OrderID, o.EquipmentName, o.Quantity, o.TotalPrice, o.OrderDate));

                Console.ForegroundColor = statusColor;
                Console.WriteLine(status);
                Console.ResetColor();
            }

            Console.WriteLine("\n[Press any key...]");
            Console.ReadKey();
        }

        public void ShowStaffManagement()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== STAFF MANAGEMENT ===\n");
                Console.WriteLine("1. View All Staff");
                Console.WriteLine("2. View Active Staff Only");
                Console.WriteLine("3. Add New Staff");
                Console.WriteLine("4. Terminate Staff");
                Console.WriteLine("5. Back");
                Console.Write("\nChoice: ");

                string choice = Console.ReadLine();
                if (choice == "5") break;

                switch (choice)
                {
                    case "1": ViewAllStaff(); break;
                    case "2": ViewActiveStaff(); break;
                    case "3": AddNewStaff(); break;
                    case "4": TerminateStaff(); break;
                    default:
                        ShowPopup("Invalid choice!", ConsoleColor.Red);
                        break;
                }
            }
        }

        // ==================== NEW: VIEW ALL STAFF ====================
        private void ViewAllStaff()
        {
            Console.Clear();
            Console.WriteLine("=== ALL STAFF ===\n");
            var staff = _gymService.GetAllStaff();

            Console.WriteLine(string.Format("{0,-5} | {1,-20} | {2,-15} | {3,-12} | {4,-10}",
                "ID", "Name", "Role", "Salary", "Status"));
            Console.WriteLine(new string('-', 75));

            foreach (var s in staff)
            {
                string status = s.IsActive ? "Active" : "Terminated";
                Console.Write(string.Format("{0,-5} | {1,-20} | {2,-15} | ${3,-11:N2} | ",
                    s.StaffID, s.Name, s.Role, s.Salary));

                Console.ForegroundColor = s.IsActive ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(status);
                Console.ResetColor();
            }

            Console.WriteLine("\n[Press any key...]");
            Console.ReadKey();
        }

        // ==================== NEW: VIEW ACTIVE STAFF ====================
        private void ViewActiveStaff()
        {
            Console.Clear();
            Console.WriteLine("=== ACTIVE STAFF ===\n");
            var staff = _gymService.GetActiveStaff();

            Console.WriteLine(string.Format("{0,-5} | {1,-20} | {2,-15} | {3,-12}",
                "ID", "Name", "Role", "Salary"));
            Console.WriteLine(new string('-', 60));

            foreach (var s in staff)
            {
                Console.WriteLine(string.Format("{0,-5} | {1,-20} | {2,-15} | ${3,-11:N2}",
                    s.StaffID, s.Name, s.Role, s.Salary));
            }

            Console.WriteLine("\n[Press any key...]");
            Console.ReadKey();
        }

        // ==================== ADD NEW STAFF ====================
        private void AddNewStaff()
        {
            Console.Clear();
            Console.WriteLine("=== ADD NEW STAFF ===\n");

            Console.Write("Name: ");
            string name = Console.ReadLine();
            Console.Write("Role (Receptionist/Trainer): ");
            string role = Console.ReadLine();
            Console.Write("Salary: $");
            if (!decimal.TryParse(Console.ReadLine(), out decimal salary))
            {
                ShowPopup("Invalid salary!", ConsoleColor.Red);
                return;
            }
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            _gymService.AddStaff(name, role, salary, username, password);
            ShowPopup("Staff added successfully!", ConsoleColor.Green);
        }

        // ==================== NEW: TERMINATE STAFF ====================
        private void TerminateStaff()
        {
            Console.Clear();
            Console.WriteLine("=== TERMINATE STAFF ===\n");

            var activeStaff = _gymService.GetActiveStaff();

            Console.WriteLine("Active Staff:");
            Console.WriteLine(string.Format("{0,-5} | {1,-20} | {2,-15}",
                "ID", "Name", "Role"));
            Console.WriteLine(new string('-', 50));

            foreach (var s in activeStaff)
            {
                Console.WriteLine(string.Format("{0,-5} | {1,-20} | {2,-15}",
                    s.StaffID, s.Name, s.Role));
            }

            Console.Write("\nEnter Staff ID to terminate: ");
            if (!int.TryParse(Console.ReadLine(), out int staffId))
            {
                ShowPopup("Invalid ID!", ConsoleColor.Red);
                return;
            }

            var staff = _gymService.GetStaffById(staffId);
            if (staff == null)
            {
                ShowPopup("Staff not found!", ConsoleColor.Red);
                return;
            }

            Console.Write($"\nAre you sure you want to terminate {staff.Name}? (Y/N): ");
            string confirm = Console.ReadLine();

            if (confirm?.ToUpper() == "Y")
            {
                _gymService.TerminateStaff(staffId);
                ShowPopup("Staff terminated successfully!", ConsoleColor.Green);
            }
            else
            {
                ShowPopup("Operation cancelled.", ConsoleColor.Yellow);
            }
        }

        // ==================== UTILITY ====================
        private void ShowPopup(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"\n{message}");
            Console.ResetColor();
            System.Threading.Thread.Sleep(2000);
        }
    }

    // ==================== PROGRAM ENTRY POINT ====================
    class Program
    {
        static void Main(string[] args)
        {
            var controller = new MainController();
            controller.Initialize();
            controller.ShowMainMenu();
        }
    }
}