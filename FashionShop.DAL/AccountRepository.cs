using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace FashionShop.DAL
{
    public class AccountRepository
    {
        public DataTable GetStaffAccounts()
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"
SELECT a.account_id, a.username, e.employee_id, e.employee_name, e.role, a.is_active
FROM accounts a
JOIN employees e ON a.employee_id = e.employee_id
WHERE LOWER(e.role) = 'staff'
ORDER BY a.account_id DESC;";

                var da = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public bool ExistsUsername(string username)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM accounts WHERE username=@u", conn);
                cmd.Parameters.AddWithValue("@u", username.Trim());
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public bool InsertStaffAccount(string username, string passHash, int employeeId)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"
INSERT INTO accounts(username, password_hash, employee_id, is_active)
VALUES(@u, @p, @eid, 1);";

                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@u", username.Trim());
                cmd.Parameters.AddWithValue("@p", passHash);
                cmd.Parameters.AddWithValue("@eid", employeeId);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteAccount(int accountId)
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM accounts WHERE account_id=@id";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", accountId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public DataTable GetStaffEmployeesForCombo()
        {
            using (var conn = DbContext.GetConnection())
            {
                conn.Open();
                string sql = @"
SELECT employee_id, employee_name
FROM employees
WHERE LOWER(role)='staff'
ORDER BY employee_name;";

                var da = new MySqlDataAdapter(sql, conn);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}
