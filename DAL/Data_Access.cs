using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace IronVault.DAL
{
    // ==================== ENTITIES ====================
    public class Member
    {
        public int MemberID { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
        public string Role { get; set; } = "Member";
        public int PlanID { get; set; }
    }

    public class Payment
    {
        public int PaymentID { get; set; }
        public int? MemberID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    // NEW: Salary Payment Entity
    public class SalaryPayment
    {
        public int SalaryPaymentID { get; set; }
        public int StaffID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Month { get; set; }  // Format: "YYYY-MM" (e.g., "2026-01")
        public string Year { get; set; }   // Format: "YYYY" (e.g., "2026")
    }

    // NEW: Expense Entity (for machines and other expenses)
    public class Expense
    {
        public int ExpenseID { get; set; }
        public string ExpenseType { get; set; }  // "Machine", "Equipment", "Maintenance", etc.
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public int? RelatedOrderID { get; set; }  // Links to EquipmentOrder if applicable
    }

    public class Owner
    {
        public int OwnerID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
    }

    public class Staff
    {
        public int StaffID { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public decimal Salary { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; } = true;  // NEW: Track if staff is terminated
    }

    public class Plan
    {
        public int PlanID { get; set; }
        public string PlanName { get; set; }
        public decimal Price { get; set; }
        public bool IncludesTrainer { get; set; }
        public bool IncludesSupplements { get; set; }
    }

    public class Machine
    {
        public int MachineID { get; set; }
        public string MachineName { get; set; }
        public string Status { get; set; }
        public decimal? PurchasePrice { get; set; }  // NEW: Track machine cost
        public DateTime? PurchaseDate { get; set; }   // NEW: Track purchase date
    }

    public class EquipmentOrder
    {
        public int OrderID { get; set; }
        public string EquipmentName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public bool IsPaid { get; set; } = false;  // NEW: Track payment status
    }

    // ==================== DATABASE CONNECTION (SINGLETON PATTERN) ====================
    public sealed class DatabaseConnection
    {
        private static DatabaseConnection _instance;
        private static readonly object _lock = new object();
        private readonly string _connectionString;

        private DatabaseConnection()
        {
            _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=IronVaultDB2.0;Integrated Security=True;Connect Timeout=30;";
        }

        public static DatabaseConnection Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseConnection();
                        }
                    }
                }
                return _instance;
            }
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }

    // ==================== REPOSITORY INTERFACE ====================
    public interface IGymRepository
    {
        // Member operations
        Member GetMemberByCredentials(int id, string password);
        bool ReceptionistLogin(string username, string password);
        Owner GetOwnerByCredentials(string username, string password);
        Staff GetStaffByCredentials(string username, string password);
        int AddMember(Member member);
        Member GetMemberById(int id);
        void UpdateMember(Member member);
        void DeleteMember(int memberId);
        List<Member> GetMembersByExpiryDate(DateTime targetDate);
        List<Member> GetAllMembers();

        // Payment operations
        void AddPayment(Payment payment);
        List<Payment> GetRecentPayments();
        Dictionary<int, int> GetMemberPaymentCounts();
        decimal GetTotalRevenue();
        int GetActiveMemberCount();

        // Owner operations
        void AddOwner(Owner owner);
        Owner GetOwnerById(int id);

        // Staff operations
        void AddStaff(Staff staff);
        Staff GetStaffById(int id);
        void UpdateStaff(Staff staff);
        List<Staff> GetAllStaff();
        List<Staff> GetActiveStaff();  // NEW: Get only active staff
        void TerminateStaff(int staffId);  // NEW: Soft delete staff

        // Salary Payment operations - NEW
        void AddSalaryPayment(SalaryPayment salaryPayment);
        List<SalaryPayment> GetSalaryPaymentsByStaff(int staffId);
        SalaryPayment GetSalaryPaymentForMonth(int staffId, string month, string year);
        List<SalaryPayment> GetAllSalaryPayments();
        decimal GetTotalSalariesPaid();

        // Expense operations - NEW
        void AddExpense(Expense expense);
        List<Expense> GetAllExpenses();
        decimal GetTotalExpenses();

        // Plan operations
        void AddPlan(Plan plan);
        Plan GetPlanById(int id);
        List<Plan> GetAllPlans();

        // Machine operations
        void AddMachine(Machine machine);
        Machine GetMachineById(int id);
        void UpdateMachine(Machine machine);
        List<Machine> GetAllMachines();

        // Equipment Order operations
        void AddEquipmentOrder(EquipmentOrder order);
        EquipmentOrder GetEquipmentOrderById(int id);
        void UpdateEquipmentOrder(EquipmentOrder order);  // NEW: Update order payment status
        List<EquipmentOrder> GetAllEquipmentOrders();
    }

    // ==================== REPOSITORY IMPLEMENTATION ====================
    public class GymRepository : IGymRepository
    {
        private readonly DatabaseConnection _db;

        public GymRepository()
        {
            _db = DatabaseConnection.Instance;
        }

        public Member GetMemberByCredentials(int id, string password)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Member WHERE MemberID = @id AND Password = @pwd", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pwd", password);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Member
                    {
                        MemberID = (int)reader["MemberID"],
                        Password = reader["Password"].ToString(),
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        ExpiryDate = (DateTime)reader["ExpiryDate"],
                        Status = reader["Status"].ToString(),
                        Role = reader["Role"].ToString(),
                        PlanID = (int)reader["PlanID"]
                    };
                }
            }
            return null;
        }

        public bool ReceptionistLogin(string username, string password)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT COUNT(*) FROM Staff WHERE Username = @user AND Password = @pwd AND Role = 'Receptionist' AND IsActive = 1", conn);
                cmd.Parameters.AddWithValue("@user", username);
                cmd.Parameters.AddWithValue("@pwd", password);

                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public Owner GetOwnerByCredentials(string username, string password)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM GymOwner WHERE Username = @user AND Password = @pwd", conn);
                cmd.Parameters.AddWithValue("@user", username);
                cmd.Parameters.AddWithValue("@pwd", password);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Owner
                    {
                        OwnerID = (int)reader["OwnerID"],
                        Username = reader["Username"].ToString(),
                        Password = reader["Password"].ToString(),
                        Name = reader["Name"].ToString()
                    };
                }
            }
            return null;
        }

        public Staff GetStaffByCredentials(string username, string password)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Staff WHERE Username = @user AND Password = @pwd AND IsActive = 1", conn);
                cmd.Parameters.AddWithValue("@user", username);
                cmd.Parameters.AddWithValue("@pwd", password);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Staff
                    {
                        StaffID = (int)reader["StaffID"],
                        Name = reader["Name"].ToString(),
                        Role = reader["Role"].ToString(),
                        Salary = (decimal)reader["Salary"],
                        Username = reader["Username"].ToString(),
                        Password = reader["Password"].ToString(),
                        IsActive = (bool)reader["IsActive"]
                    };
                }
            }
            return null;
        }

        public int AddMember(Member member)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"INSERT INTO Member (Password, Name, Email, ExpiryDate, Status, Role, PlanID) 
                                          OUTPUT INSERTED.MemberID
                                          VALUES (@pwd, @name, @email, @exp, @stat, @role, @plan)", conn);
                cmd.Parameters.AddWithValue("@pwd", member.Password);
                cmd.Parameters.AddWithValue("@name", member.Name);
                cmd.Parameters.AddWithValue("@email", member.Email);
                cmd.Parameters.AddWithValue("@exp", member.ExpiryDate);
                cmd.Parameters.AddWithValue("@stat", member.Status);
                cmd.Parameters.AddWithValue("@role", member.Role);
                cmd.Parameters.AddWithValue("@plan", member.PlanID);

                return (int)cmd.ExecuteScalar();
            }
        }

        public Member GetMemberById(int id)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Member WHERE MemberID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Member
                    {
                        MemberID = (int)reader["MemberID"],
                        Password = reader["Password"].ToString(),
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        ExpiryDate = (DateTime)reader["ExpiryDate"],
                        Status = reader["Status"].ToString(),
                        Role = reader["Role"].ToString(),
                        PlanID = (int)reader["PlanID"]
                    };
                }
            }
            return null;
        }

        public void UpdateMember(Member member)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"UPDATE Member 
                                          SET Password = @pwd, Name = @name, Email = @email, 
                                              ExpiryDate = @exp, Status = @stat, Role = @role, PlanID = @plan 
                                          WHERE MemberID = @id", conn);
                cmd.Parameters.AddWithValue("@id", member.MemberID);
                cmd.Parameters.AddWithValue("@pwd", member.Password);
                cmd.Parameters.AddWithValue("@name", member.Name);
                cmd.Parameters.AddWithValue("@email", member.Email);
                cmd.Parameters.AddWithValue("@exp", member.ExpiryDate);
                cmd.Parameters.AddWithValue("@stat", member.Status);
                cmd.Parameters.AddWithValue("@role", member.Role);
                cmd.Parameters.AddWithValue("@plan", member.PlanID);

                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteMember(int memberId)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Member WHERE MemberID = @id", conn);
                cmd.Parameters.AddWithValue("@id", memberId);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Member> GetMembersByExpiryDate(DateTime targetDate)
        {
            var members = new List<Member>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"SELECT * FROM Member 
                                          WHERE CAST(ExpiryDate AS DATE) = @date AND Status = 'Active'", conn);
                cmd.Parameters.AddWithValue("@date", targetDate.Date);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    members.Add(new Member
                    {
                        MemberID = (int)reader["MemberID"],
                        Password = reader["Password"].ToString(),
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        ExpiryDate = (DateTime)reader["ExpiryDate"],
                        Status = reader["Status"].ToString(),
                        Role = reader["Role"].ToString(),
                        PlanID = (int)reader["PlanID"]
                    });
                }
            }
            return members;
        }

        public List<Member> GetAllMembers()
        {
            var members = new List<Member>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Member", conn);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    members.Add(new Member
                    {
                        MemberID = (int)reader["MemberID"],
                        Password = reader["Password"].ToString(),
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        ExpiryDate = (DateTime)reader["ExpiryDate"],
                        Status = reader["Status"].ToString(),
                        Role = reader["Role"].ToString(),
                        PlanID = (int)reader["PlanID"]
                    });
                }
            }
            return members;
        }

        public void AddPayment(Payment payment)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"INSERT INTO Payment (MemberID, Amount, PaymentDate) 
                                          VALUES (@mid, @amt, @date)", conn);
                cmd.Parameters.AddWithValue("@mid", (object)payment.MemberID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@amt", payment.Amount);
                cmd.Parameters.AddWithValue("@date", payment.PaymentDate);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Payment> GetRecentPayments()
        {
            var payments = new List<Payment>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"SELECT * FROM Payment 
                                          WHERE PaymentDate >= DATEADD(MONTH, -1, GETDATE()) 
                                          ORDER BY PaymentDate DESC", conn);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    payments.Add(new Payment
                    {
                        PaymentID = (int)reader["PaymentID"],
                        MemberID = reader["MemberID"] == DBNull.Value ? null : (int?)reader["MemberID"],
                        Amount = (decimal)reader["Amount"],
                        PaymentDate = (DateTime)reader["PaymentDate"]
                    });
                }
            }
            return payments;
        }

        public Dictionary<int, int> GetMemberPaymentCounts()
        {
            var counts = new Dictionary<int, int>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"SELECT MemberID, COUNT(*) as PaymentCount 
                                          FROM Payment 
                                          WHERE MemberID IS NOT NULL
                                          GROUP BY MemberID", conn);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int memberId = (int)reader["MemberID"];
                    int count = (int)reader["PaymentCount"];
                    counts[memberId] = count;
                }
            }
            return counts;
        }

        public decimal GetTotalRevenue()
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ISNULL(SUM(Amount), 0) FROM Payment", conn);
                return (decimal)cmd.ExecuteScalar();
            }
        }

        public int GetActiveMemberCount()
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT COUNT(*) FROM Member WHERE Status = 'Active'", conn);
                return (int)cmd.ExecuteScalar();
            }
        }

        public void AddOwner(Owner owner)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"INSERT INTO GymOwner (Username, Password, Name) 
                                          VALUES (@user, @pwd, @name)", conn);
                cmd.Parameters.AddWithValue("@user", owner.Username);
                cmd.Parameters.AddWithValue("@pwd", owner.Password);
                cmd.Parameters.AddWithValue("@name", owner.Name);
                cmd.ExecuteNonQuery();
            }
        }

        public Owner GetOwnerById(int id)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM GymOwner WHERE OwnerID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Owner
                    {
                        OwnerID = (int)reader["OwnerID"],
                        Username = reader["Username"].ToString(),
                        Password = reader["Password"].ToString(),
                        Name = reader["Name"].ToString()
                    };
                }
            }
            return null;
        }

        public void AddStaff(Staff staff)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"INSERT INTO Staff (Name, Role, Salary, Username, Password, IsActive) 
                                          VALUES (@name, @role, @salary, @user, @pwd, @active)", conn);
                cmd.Parameters.AddWithValue("@name", staff.Name);
                cmd.Parameters.AddWithValue("@role", staff.Role);
                cmd.Parameters.AddWithValue("@salary", staff.Salary);
                cmd.Parameters.AddWithValue("@user", staff.Username);
                cmd.Parameters.AddWithValue("@pwd", staff.Password);
                cmd.Parameters.AddWithValue("@active", staff.IsActive);
                cmd.ExecuteNonQuery();
            }
        }

        public Staff GetStaffById(int id)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Staff WHERE StaffID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Staff
                    {
                        StaffID = (int)reader["StaffID"],
                        Name = reader["Name"].ToString(),
                        Role = reader["Role"].ToString(),
                        Salary = (decimal)reader["Salary"],
                        Username = reader["Username"].ToString(),
                        Password = reader["Password"].ToString(),
                        IsActive = (bool)reader["IsActive"]
                    };
                }
            }
            return null;
        }

        public void UpdateStaff(Staff staff)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"UPDATE Staff SET Name = @name, Salary = @salary, Role = @role, IsActive = @active 
                                          WHERE StaffID = @id", conn);
                cmd.Parameters.AddWithValue("@id", staff.StaffID);
                cmd.Parameters.AddWithValue("@name", staff.Name);
                cmd.Parameters.AddWithValue("@salary", staff.Salary);
                cmd.Parameters.AddWithValue("@role", staff.Role);
                cmd.Parameters.AddWithValue("@active", staff.IsActive);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Staff> GetAllStaff()
        {
            var staff = new List<Staff>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Staff", conn);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    staff.Add(new Staff
                    {
                        StaffID = (int)reader["StaffID"],
                        Name = reader["Name"].ToString(),
                        Role = reader["Role"].ToString(),
                        Salary = (decimal)reader["Salary"],
                        Username = reader["Username"].ToString(),
                        Password = reader["Password"].ToString(),
                        IsActive = (bool)reader["IsActive"]
                    });
                }
            }
            return staff;
        }

        public List<Staff> GetActiveStaff()
        {
            var staff = new List<Staff>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Staff WHERE IsActive = 1", conn);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    staff.Add(new Staff
                    {
                        StaffID = (int)reader["StaffID"],
                        Name = reader["Name"].ToString(),
                        Role = reader["Role"].ToString(),
                        Salary = (decimal)reader["Salary"],
                        Username = reader["Username"].ToString(),
                        Password = reader["Password"].ToString(),
                        IsActive = (bool)reader["IsActive"]
                    });
                }
            }
            return staff;
        }

        public void TerminateStaff(int staffId)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE Staff SET IsActive = 0 WHERE StaffID = @id", conn);
                cmd.Parameters.AddWithValue("@id", staffId);
                cmd.ExecuteNonQuery();
            }
        }

        // ==================== SALARY PAYMENT OPERATIONS ====================
        public void AddSalaryPayment(SalaryPayment salaryPayment)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"INSERT INTO SalaryPayment (StaffID, Amount, PaymentDate, Month, Year) 
                                          VALUES (@staffId, @amount, @date, @month, @year)", conn);
                cmd.Parameters.AddWithValue("@staffId", salaryPayment.StaffID);
                cmd.Parameters.AddWithValue("@amount", salaryPayment.Amount);
                cmd.Parameters.AddWithValue("@date", salaryPayment.PaymentDate);
                cmd.Parameters.AddWithValue("@month", salaryPayment.Month);
                cmd.Parameters.AddWithValue("@year", salaryPayment.Year);
                cmd.ExecuteNonQuery();
            }
        }

        public List<SalaryPayment> GetSalaryPaymentsByStaff(int staffId)
        {
            var payments = new List<SalaryPayment>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"SELECT * FROM SalaryPayment 
                                          WHERE StaffID = @staffId 
                                          ORDER BY PaymentDate DESC", conn);
                cmd.Parameters.AddWithValue("@staffId", staffId);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    payments.Add(new SalaryPayment
                    {
                        SalaryPaymentID = (int)reader["SalaryPaymentID"],
                        StaffID = (int)reader["StaffID"],
                        Amount = (decimal)reader["Amount"],
                        PaymentDate = (DateTime)reader["PaymentDate"],
                        Month = reader["Month"].ToString(),
                        Year = reader["Year"].ToString()
                    });
                }
            }
            return payments;
        }

        public SalaryPayment GetSalaryPaymentForMonth(int staffId, string month, string year)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"SELECT * FROM SalaryPayment 
                                          WHERE StaffID = @staffId AND Month = @month AND Year = @year", conn);
                cmd.Parameters.AddWithValue("@staffId", staffId);
                cmd.Parameters.AddWithValue("@month", month);
                cmd.Parameters.AddWithValue("@year", year);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new SalaryPayment
                    {
                        SalaryPaymentID = (int)reader["SalaryPaymentID"],
                        StaffID = (int)reader["StaffID"],
                        Amount = (decimal)reader["Amount"],
                        PaymentDate = (DateTime)reader["PaymentDate"],
                        Month = reader["Month"].ToString(),
                        Year = reader["Year"].ToString()
                    };
                }
            }
            return null;
        }

        public List<SalaryPayment> GetAllSalaryPayments()
        {
            var payments = new List<SalaryPayment>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM SalaryPayment ORDER BY PaymentDate DESC", conn);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    payments.Add(new SalaryPayment
                    {
                        SalaryPaymentID = (int)reader["SalaryPaymentID"],
                        StaffID = (int)reader["StaffID"],
                        Amount = (decimal)reader["Amount"],
                        PaymentDate = (DateTime)reader["PaymentDate"],
                        Month = reader["Month"].ToString(),
                        Year = reader["Year"].ToString()
                    });
                }
            }
            return payments;
        }

        public decimal GetTotalSalariesPaid()
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ISNULL(SUM(Amount), 0) FROM SalaryPayment", conn);
                return (decimal)cmd.ExecuteScalar();
            }
        }

        // ==================== EXPENSE OPERATIONS ====================
        public void AddExpense(Expense expense)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"INSERT INTO Expense (ExpenseType, Description, Amount, ExpenseDate, RelatedOrderID) 
                                          VALUES (@type, @desc, @amount, @date, @orderId)", conn);
                cmd.Parameters.AddWithValue("@type", expense.ExpenseType);
                cmd.Parameters.AddWithValue("@desc", expense.Description);
                cmd.Parameters.AddWithValue("@amount", expense.Amount);
                cmd.Parameters.AddWithValue("@date", expense.ExpenseDate);
                cmd.Parameters.AddWithValue("@orderId", (object)expense.RelatedOrderID ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Expense> GetAllExpenses()
        {
            var expenses = new List<Expense>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Expense ORDER BY ExpenseDate DESC", conn);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    expenses.Add(new Expense
                    {
                        ExpenseID = (int)reader["ExpenseID"],
                        ExpenseType = reader["ExpenseType"].ToString(),
                        Description = reader["Description"].ToString(),
                        Amount = (decimal)reader["Amount"],
                        ExpenseDate = (DateTime)reader["ExpenseDate"],
                        RelatedOrderID = reader["RelatedOrderID"] == DBNull.Value ? null : (int?)reader["RelatedOrderID"]
                    });
                }
            }
            return expenses;
        }

        public decimal GetTotalExpenses()
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ISNULL(SUM(Amount), 0) FROM Expense", conn);
                return (decimal)cmd.ExecuteScalar();
            }
        }

        public void AddPlan(Plan plan)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"INSERT INTO [Plan] (PlanName, Price, IncludesTrainer, IncludesSupplements) 
                                          VALUES (@name, @price, @trainer, @supp)", conn);
                cmd.Parameters.AddWithValue("@name", plan.PlanName);
                cmd.Parameters.AddWithValue("@price", plan.Price);
                cmd.Parameters.AddWithValue("@trainer", plan.IncludesTrainer);
                cmd.Parameters.AddWithValue("@supp", plan.IncludesSupplements);
                cmd.ExecuteNonQuery();
            }
        }

        public Plan GetPlanById(int id)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM [Plan] WHERE PlanID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Plan
                    {
                        PlanID = (int)reader["PlanID"],
                        PlanName = reader["PlanName"].ToString(),
                        Price = (decimal)reader["Price"],
                        IncludesTrainer = (bool)reader["IncludesTrainer"],
                        IncludesSupplements = (bool)reader["IncludesSupplements"]
                    };
                }
            }
            return null;
        }

        public List<Plan> GetAllPlans()
        {
            var plans = new List<Plan>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM [Plan] ORDER BY Price", conn);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    plans.Add(new Plan
                    {
                        PlanID = (int)reader["PlanID"],
                        PlanName = reader["PlanName"].ToString(),
                        Price = (decimal)reader["Price"],
                        IncludesTrainer = (bool)reader["IncludesTrainer"],
                        IncludesSupplements = (bool)reader["IncludesSupplements"]
                    });
                }
            }
            return plans;
        }

        public void AddMachine(Machine machine)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"INSERT INTO Machine (MachineName, Status, PurchasePrice, PurchaseDate) 
                                          VALUES (@name, @status, @price, @date)", conn);
                cmd.Parameters.AddWithValue("@name", machine.MachineName);
                cmd.Parameters.AddWithValue("@status", machine.Status);
                cmd.Parameters.AddWithValue("@price", (object)machine.PurchasePrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@date", (object)machine.PurchaseDate ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public Machine GetMachineById(int id)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Machine WHERE MachineID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Machine
                    {
                        MachineID = (int)reader["MachineID"],
                        MachineName = reader["MachineName"].ToString(),
                        Status = reader["Status"].ToString(),
                        PurchasePrice = reader["PurchasePrice"] == DBNull.Value ? null : (decimal?)reader["PurchasePrice"],
                        PurchaseDate = reader["PurchaseDate"] == DBNull.Value ? null : (DateTime?)reader["PurchaseDate"]
                    };
                }
            }
            return null;
        }

        public void UpdateMachine(Machine machine)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"UPDATE Machine SET MachineName = @name, Status = @status, 
                                          PurchasePrice = @price, PurchaseDate = @date
                                          WHERE MachineID = @id", conn);
                cmd.Parameters.AddWithValue("@id", machine.MachineID);
                cmd.Parameters.AddWithValue("@name", machine.MachineName);
                cmd.Parameters.AddWithValue("@status", machine.Status);
                cmd.Parameters.AddWithValue("@price", (object)machine.PurchasePrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@date", (object)machine.PurchaseDate ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Machine> GetAllMachines()
        {
            var machines = new List<Machine>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Machine", conn);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    machines.Add(new Machine
                    {
                        MachineID = (int)reader["MachineID"],
                        MachineName = reader["MachineName"].ToString(),
                        Status = reader["Status"].ToString(),
                        PurchasePrice = reader["PurchasePrice"] == DBNull.Value ? null : (decimal?)reader["PurchasePrice"],
                        PurchaseDate = reader["PurchaseDate"] == DBNull.Value ? null : (DateTime?)reader["PurchaseDate"]
                    });
                }
            }
            return machines;
        }

        public void AddEquipmentOrder(EquipmentOrder order)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"INSERT INTO EquipmentOrder (EquipmentName, Quantity, TotalPrice, OrderDate, IsPaid) 
                                          VALUES (@name, @qty, @price, @date, @paid)", conn);
                cmd.Parameters.AddWithValue("@name", order.EquipmentName);
                cmd.Parameters.AddWithValue("@qty", order.Quantity);
                cmd.Parameters.AddWithValue("@price", order.TotalPrice);
                cmd.Parameters.AddWithValue("@date", order.OrderDate);
                cmd.Parameters.AddWithValue("@paid", order.IsPaid);
                cmd.ExecuteNonQuery();
            }
        }

        public EquipmentOrder GetEquipmentOrderById(int id)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM EquipmentOrder WHERE OrderID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new EquipmentOrder
                    {
                        OrderID = (int)reader["OrderID"],
                        EquipmentName = reader["EquipmentName"].ToString(),
                        Quantity = (int)reader["Quantity"],
                        TotalPrice = (decimal)reader["TotalPrice"],
                        OrderDate = (DateTime)reader["OrderDate"],
                        IsPaid = (bool)reader["IsPaid"]
                    };
                }
            }
            return null;
        }

        public void UpdateEquipmentOrder(EquipmentOrder order)
        {
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(@"UPDATE EquipmentOrder 
                                          SET EquipmentName = @name, Quantity = @qty, TotalPrice = @price, IsPaid = @paid
                                          WHERE OrderID = @id", conn);
                cmd.Parameters.AddWithValue("@id", order.OrderID);
                cmd.Parameters.AddWithValue("@name", order.EquipmentName);
                cmd.Parameters.AddWithValue("@qty", order.Quantity);
                cmd.Parameters.AddWithValue("@price", order.TotalPrice);
                cmd.Parameters.AddWithValue("@paid", order.IsPaid);
                cmd.ExecuteNonQuery();
            }
        }

        public List<EquipmentOrder> GetAllEquipmentOrders()
        {
            var orders = new List<EquipmentOrder>();
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM EquipmentOrder ORDER BY OrderDate DESC", conn);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    orders.Add(new EquipmentOrder
                    {
                        OrderID = (int)reader["OrderID"],
                        EquipmentName = reader["EquipmentName"].ToString(),
                        Quantity = (int)reader["Quantity"],
                        TotalPrice = (decimal)reader["TotalPrice"],
                        OrderDate = (DateTime)reader["OrderDate"],
                        IsPaid = (bool)reader["IsPaid"]
                    });
                }
            }
            return orders;
        }
    }
}