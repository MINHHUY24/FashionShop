using System;
using System.Data;
using FashionShop.DAL;
using FashionShop.DTO;

namespace FashionShop.BLL
{
    public class AccountService
    {
        private readonly AccountRepository repo = new AccountRepository();

        public DataTable GetStaffAccounts() => repo.GetStaffAccounts();

        public DataTable GetStaffEmployeesForCombo() => repo.GetStaffEmployeesForCombo();

        public bool CreateStaffAccount(string username, string password, int employeeId, out string err)
        {
            err = "";

            if (string.IsNullOrWhiteSpace(username))
            {
                err = "Username không được để trống.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                err = "Password không được để trống.";
                return false;
            }
            if (employeeId <= 0)
            {
                err = "Bạn phải chọn nhân viên Staff.";
                return false;
            }
            if (repo.ExistsUsername(username))
            {
                err = "Username đã tồn tại.";
                return false;
            }

            string passHash = HashHelper.Sha256(password);
            return repo.InsertStaffAccount(username, passHash, employeeId);
        }

        public bool DeleteStaffAccount(int accountId, out string err)
        {
            err = "";
            if (accountId <= 0)
            {
                err = "Vui lòng chọn tài khoản cần xóa.";
                return false;
            }

            if (!repo.DeleteAccount(accountId))
            {
                err = "Xóa thất bại.";
                return false;
            }
            return true;
        }
    }
}
