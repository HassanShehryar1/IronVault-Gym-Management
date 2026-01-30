using IronVault.DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronVault.BLL
{
    // ==================== DTOs ====================
    public class NewMemberCredentials
    {
        public int AssignedMemberID { get; set; }
        public string GeneratedPassword { get; set; }
    }

    public class MembershipExpiringEventArgs : EventArgs
    {
        public int MemberID { get; set; }
        public string MemberName { get; set; }
        public string Email { get; set; }
    }

    // NEW: Salary Payment Event Args
    public class SalaryPaidEventArgs : EventArgs
    {
        public int StaffID { get; set; }
        public string StaffName { get; set; }
        public decimal Amount { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public bool AlreadyPaid { get; set; }
    }

    public class MemberRetentionData
    {
        public int MemberID { get; set; }
        public string MemberName { get; set; }
        public int RenewalCount { get; set; }
    }

    // NEW: Financial Summary DTO
    public class FinancialSummary
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalSalariesPaid { get; set; }
        public decimal NetProfit { get; set; }
        public decimal AvailableBalance { get; set; }
    }

    // ==================== FACTORY PATTERN: USER FACTORY ====================
    public interface IUserFactory
    {
        Member CreateMember(string name, string email, int planId, int durationDays, string password);
        Owner CreateOwner(string username, string password, string name);
        Staff CreateStaff(string name, string role, decimal salary, string username, string password);
    }

    public class UserFactory : IUserFactory
    {
        public Member CreateMember(string name, string email, int planId, int durationDays, string password)
        {
            return new Member
            {
                Name = name,
                Email = email,
                PlanID = planId,
                Password = password,
                ExpiryDate = DateTime.Now.AddDays(durationDays),
                Status = "Active",
                Role = "Member"
            };
        }

        public Owner CreateOwner(string username, string password, string name)
        {
            return new Owner
            {
                Username = username,
                Password = password,
                Name = name
            };
        }

        public Staff CreateStaff(string name, string role, decimal salary, string username, string password)
        {
            return new Staff
            {
                Name = name,
                Role = role,
                Salary = salary,
                Username = username,
                Password = password,
                IsActive = true
            };
        }
    }

    // ==================== BUSINESS LOGIC SERVICE ====================
    public class GymService
    {
        private readonly GymRepository _repository;
        private readonly IUserFactory _userFactory;

        // ==================== OBSERVER PATTERN: EVENTS ====================
        public event EventHandler<MembershipExpiringEventArgs> OnMembershipExpiring;
        public event EventHandler<SalaryPaidEventArgs> OnSalaryPaid;  // NEW: Salary payment event

        public GymService(GymRepository repository)
        {
            _repository = repository;
            _userFactory = new UserFactory();
        }

        // ==================== AUTHENTICATION ====================
        public bool LoginMember(int memberId, string password)
        {
            var member = _repository.GetMemberByCredentials(memberId, password);
            return member != null;
        }

        public bool LoginReceptionist(string username, string password)
        {
            return _repository.ReceptionistLogin(username, password);
        }

        public bool LoginOwner(string username, string password)
        {
            return _repository.GetOwnerByCredentials(username, password) != null;
        }

        public bool LoginTrainer(string username, string password)
        {
            var staff = _repository.GetStaffByCredentials(username, password);
            return staff != null && staff.Role == "Trainer";
        }

        // ==================== MEMBER REGISTRATION ====================
        public NewMemberCredentials RegisterNewMember(string name, string email, int planId)
        {
            var plan = _repository.GetPlanById(planId);
            if (plan == null)
            {
                throw new Exception("Invalid plan selected");
            }

            const int standardDuration = 30;
            string password = GeneratePassword();

            var member = _userFactory.CreateMember(name, email, planId, standardDuration, password);
            int newMemberId = _repository.AddMember(member);

            var payment = new Payment
            {
                MemberID = newMemberId,
                Amount = plan.Price,
                PaymentDate = DateTime.Now
            };
            _repository.AddPayment(payment);

            return new NewMemberCredentials
            {
                AssignedMemberID = newMemberId,
                GeneratedPassword = password
            };
        }

        private string GeneratePassword()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // ==================== ACCESS CONTROL ====================
        public bool ValidateCheckIn(int memberId)
        {
            var member = _repository.GetMemberById(memberId);
            if (member == null) return false;

            if (member.ExpiryDate >= DateTime.Now)
            {
                member.Status = "Active";
                _repository.UpdateMember(member);
                return true;
            }
            else
            {
                member.Status = "Expired";
                _repository.UpdateMember(member);
                return false;
            }
        }

        // ==================== MEMBERSHIP RENEWAL ====================
        public DateTime RenewMembership(int memberId, int planId)
        {
            var member = _repository.GetMemberById(memberId);
            if (member == null) throw new Exception("Member not found");

            var plan = _repository.GetPlanById(planId);
            if (plan == null) throw new Exception("Plan not found");

            member.PlanID = planId;

            RecordPayment(memberId, plan.Price);

            DateTime newExpiry = ExtendExpiryDate(member, 30);

            return newExpiry;
        }

        private void RecordPayment(int memberId, decimal amount)
        {
            var payment = new Payment
            {
                MemberID = memberId,
                Amount = amount,
                PaymentDate = DateTime.Now
            };
            _repository.AddPayment(payment);
        }

        private DateTime ExtendExpiryDate(Member member, int daysToAdd)
        {
            DateTime startDate = member.ExpiryDate < DateTime.Now ? DateTime.Now : member.ExpiryDate;
            member.ExpiryDate = startDate.AddDays(daysToAdd);
            member.Status = "Active";
            _repository.UpdateMember(member);
            return member.ExpiryDate;
        }

        public List<string> GetUnpaidMembersWarningList()
        {
            var recentPayments = _repository.GetRecentPayments();
            var allMembers = _repository.GetAllMembers();
            var unpaidMembers = new List<string>();

            foreach (var member in allMembers.Where(m => m.Status == "Active"))
            {
                bool hasRecentPayment = recentPayments.Any(p => p.MemberID == member.MemberID);
                if (!hasRecentPayment)
                {
                    unpaidMembers.Add($"ID: {member.MemberID} | {member.Name} | Last Active: {member.ExpiryDate:dd-MMM-yyyy}");
                }
            }

            return unpaidMembers;
        }

        // ==================== DASHBOARD STATS ====================
        public decimal CalculateMonthlyRevenue()
        {
            var recentPayments = _repository.GetRecentPayments();
            return recentPayments.Sum(p => p.Amount);
        }

        public int GetCurrentActiveMembers()
        {
            return _repository.GetActiveMemberCount();
        }

        // NEW: Get Financial Summary
        public FinancialSummary GetFinancialSummary()
        {
            decimal totalRevenue = _repository.GetTotalRevenue();
            decimal totalExpenses = _repository.GetTotalExpenses();
            decimal totalSalaries = _repository.GetTotalSalariesPaid();

            return new FinancialSummary
            {
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                TotalSalariesPaid = totalSalaries,
                NetProfit = totalRevenue - totalExpenses - totalSalaries,
                AvailableBalance = totalRevenue - totalExpenses - totalSalaries
            };
        }

        // ==================== RETENTION ANALYSIS ====================
        public (double AverageRenewals, List<MemberRetentionData> TopLoyalMembers) GetRetentionAnalysis()
        {
            var paymentCounts = _repository.GetMemberPaymentCounts();
            var allMembers = _repository.GetAllMembers();

            var retentionList = new List<MemberRetentionData>();
            int totalRenewals = 0;
            int membersWhoRenewed = 0;

            foreach (var member in allMembers)
            {
                int totalPayments = paymentCounts.ContainsKey(member.MemberID) ? paymentCounts[member.MemberID] : 0;
                int renewals = (totalPayments - 1) > 0 ? (totalPayments - 1) : 0;

                retentionList.Add(new MemberRetentionData
                {
                    MemberID = member.MemberID,
                    MemberName = member.Name,
                    RenewalCount = renewals
                });

                if (renewals > 0)
                {
                    totalRenewals += renewals;
                    membersWhoRenewed++;
                }
            }

            double averageRenewals = membersWhoRenewed > 0
                ? (double)totalRenewals / membersWhoRenewed
                : 0.0;

            var topLoyal = retentionList
                .OrderByDescending(r => r.RenewalCount)
                .Take(5)
                .ToList();

            return (averageRenewals, topLoyal);
        }

        // ==================== TERMINATION ====================
        public void TerminateMembership(int memberId)
        {
            _repository.DeleteMember(memberId);
        }

        // ==================== OBSERVER PATTERN: EVENT-DRIVEN NOTIFICATIONS ====================
        public void ProcessDailyExpirations()
        {
            DateTime tomorrow = DateTime.Now.AddDays(1).Date;
            var expiringMembers = _repository.GetMembersByExpiryDate(tomorrow);

            foreach (var member in expiringMembers)
            {
                RaiseExpiryEvent(member);
            }
        }

        protected virtual void RaiseExpiryEvent(Member member)
        {
            OnMembershipExpiring?.Invoke(this, new MembershipExpiringEventArgs
            {
                MemberID = member.MemberID,
                MemberName = member.Name,
                Email = member.Email
            });
        }

        // ==================== PLAN MANAGEMENT ====================
        public List<Plan> GetAvailablePlans()
        {
            return _repository.GetAllPlans();
        }

        public Plan GetPlanById(int planId)
        {
            return _repository.GetPlanById(planId);
        }

        // ==================== STAFF MANAGEMENT ====================
        public void AddStaff(string name, string role, decimal salary, string username, string password)
        {
            var staff = _userFactory.CreateStaff(name, role, salary, username, password);
            _repository.AddStaff(staff);
        }

        public List<Staff> GetAllStaff()
        {
            return _repository.GetAllStaff();
        }

        public List<Staff> GetActiveStaff()
        {
            return _repository.GetActiveStaff();
        }

        public void UpdateStaffSalary(int staffId, decimal newSalary)
        {
            var staff = _repository.GetStaffById(staffId);
            if (staff != null)
            {
                staff.Salary = newSalary;
                _repository.UpdateStaff(staff);
            }
        }

        // NEW: Terminate Staff (soft delete - keeps salary records)
        public void TerminateStaff(int staffId)
        {
            var staff = _repository.GetStaffById(staffId);
            if (staff == null)
            {
                throw new Exception("Staff member not found");
            }

            _repository.TerminateStaff(staffId);
        }

        // ==================== SALARY PAYMENT MANAGEMENT ====================
        // NEW: Pay staff salary
        public bool PayStaffSalary(int staffId)
        {
            var staff = _repository.GetStaffById(staffId);
            if (staff == null)
            {
                throw new Exception("Staff member not found");
            }

            // Check available balance
            var financials = GetFinancialSummary();
            if (financials.AvailableBalance < staff.Salary)
            {
                throw new Exception($"Insufficient funds! Available: ${financials.AvailableBalance:N2}, Required: ${staff.Salary:N2}");
            }

            // Get current month and year
            string currentMonth = DateTime.Now.ToString("yyyy-MM");
            string currentYear = DateTime.Now.Year.ToString();

            // Check if salary already paid this month
            var existingPayment = _repository.GetSalaryPaymentForMonth(staffId, currentMonth, currentYear);

            bool alreadyPaid = existingPayment != null;

            if (!alreadyPaid)
            {
                // Create salary payment record
                var salaryPayment = new SalaryPayment
                {
                    StaffID = staffId,
                    Amount = staff.Salary,
                    PaymentDate = DateTime.Now,
                    Month = currentMonth,
                    Year = currentYear
                };
                _repository.AddSalaryPayment(salaryPayment);

                // Record as expense
                var expense = new Expense
                {
                    ExpenseType = "Salary",
                    Description = $"Salary payment for {staff.Name} ({staff.Role}) - {currentMonth}",
                    Amount = staff.Salary,
                    ExpenseDate = DateTime.Now
                };
                _repository.AddExpense(expense);
            }

            // Raise event to notify owner
            RaiseSalaryPaidEvent(staff, currentMonth, currentYear, alreadyPaid);

            return alreadyPaid;
        }

        protected virtual void RaiseSalaryPaidEvent(Staff staff, string month, string year, bool alreadyPaid)
        {
            OnSalaryPaid?.Invoke(this, new SalaryPaidEventArgs
            {
                StaffID = staff.StaffID,
                StaffName = staff.Name,
                Amount = staff.Salary,
                Month = month,
                Year = year,
                AlreadyPaid = alreadyPaid
            });
        }

        // NEW: Get salary payment history for a staff member
        public List<SalaryPayment> GetSalaryHistory(int staffId)
        {
            return _repository.GetSalaryPaymentsByStaff(staffId);
        }

        // NEW: Get all salary payments
        public List<SalaryPayment> GetAllSalaryPayments()
        {
            return _repository.GetAllSalaryPayments();
        }

        // ==================== MACHINE MANAGEMENT ====================
        public void AddMachine(string machineName, string status)
        {
            var machine = new Machine
            {
                MachineName = machineName,
                Status = status
            };
            _repository.AddMachine(machine);
        }

        // NEW: Add machine with price (paid from revenue)
        public void AddMachineWithPayment(string machineName, string status, decimal price)
        {
            // Check available balance
            var financials = GetFinancialSummary();
            if (financials.AvailableBalance < price)
            {
                throw new Exception($"Insufficient funds! Available: ${financials.AvailableBalance:N2}, Required: ${price:N2}");
            }

            // Create machine with purchase details
            var machine = new Machine
            {
                MachineName = machineName,
                Status = status,
                PurchasePrice = price,
                PurchaseDate = DateTime.Now
            };
            _repository.AddMachine(machine);

            // Record as expense
            var expense = new Expense
            {
                ExpenseType = "Machine",
                Description = $"Purchase of {machineName}",
                Amount = price,
                ExpenseDate = DateTime.Now
            };
            _repository.AddExpense(expense);
        }

        public List<Machine> GetAllMachines()
        {
            return _repository.GetAllMachines();
        }

        public void UpdateMachineStatus(int machineId, string newStatus)
        {
            var machine = _repository.GetMachineById(machineId);
            if (machine != null)
            {
                machine.Status = newStatus;
                _repository.UpdateMachine(machine);
            }
        }

        // ==================== EQUIPMENT ORDER MANAGEMENT ====================
        public void PlaceEquipmentOrder(string equipmentName, int quantity, decimal totalPrice)
        {
            var order = new EquipmentOrder
            {
                EquipmentName = equipmentName,
                Quantity = quantity,
                TotalPrice = totalPrice,
                OrderDate = DateTime.Now,
                IsPaid = false
            };
            _repository.AddEquipmentOrder(order);
        }

        // NEW: Pay for equipment order from revenue
        public void PayForEquipmentOrder(int orderId)
        {
            var order = _repository.GetEquipmentOrderById(orderId);
            if (order == null)
            {
                throw new Exception("Order not found");
            }

            if (order.IsPaid)
            {
                throw new Exception("This order has already been paid");
            }

            // Check available balance
            var financials = GetFinancialSummary();
            if (financials.AvailableBalance < order.TotalPrice)
            {
                throw new Exception($"Insufficient funds! Available: ${financials.AvailableBalance:N2}, Required: ${order.TotalPrice:N2}");
            }

            // Mark order as paid
            order.IsPaid = true;
            _repository.UpdateEquipmentOrder(order);

            // Record as expense
            var expense = new Expense
            {
                ExpenseType = "Equipment",
                Description = $"Payment for {order.EquipmentName} (Qty: {order.Quantity})",
                Amount = order.TotalPrice,
                ExpenseDate = DateTime.Now,
                RelatedOrderID = orderId
            };
            _repository.AddExpense(expense);
        }

        public List<EquipmentOrder> GetEquipmentOrderHistory()
        {
            return _repository.GetAllEquipmentOrders();
        }

        // NEW: Get unpaid equipment orders
        public List<EquipmentOrder> GetUnpaidEquipmentOrders()
        {
            return _repository.GetAllEquipmentOrders()
                .Where(o => !o.IsPaid)
                .ToList();
        }

        // ==================== EXPENSE MANAGEMENT ====================
        // NEW: Get all expenses
        public List<Expense> GetAllExpenses()
        {
            return _repository.GetAllExpenses();
        }

        // ==================== MEMBER RETRIEVAL WITH PASSWORD VERIFICATION ====================
        public Member GetMemberById(int memberId)
        {
            return _repository.GetMemberById(memberId);
        }

        public Member GetMemberByIdAndPassword(int memberId, string password)
        {
            return _repository.GetMemberByCredentials(memberId, password);
        }

        public List<Member> GetAllMembers()
        {
            return _repository.GetAllMembers();
        }

        public List<Payment> GetDetailedPaymentHistory()
        {
            return _repository.GetRecentPayments();
        }

        public Staff GetStaffById(int staffId)
        {
            return _repository.GetStaffById(staffId);
        }
    }
}