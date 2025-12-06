using FashionShop.DAL;
using FashionShop.DTO;
using System.Data;

namespace FashionShop.BLL
{
    public class CustomerService
    {
        private CustomerRepository repo = new CustomerRepository();

        public DataTable GetAll() => repo.GetAll();
        public DataTable Search(string kw) => repo.Search(kw);
        public DataTable GetForCombo() => repo.GetForCombo();

        public bool Add(Customer c, out string err)
        {
            err = "";
            if (string.IsNullOrWhiteSpace(c.Name))
            { err = "Tên khách không được trống"; return false; }
            if (!string.IsNullOrWhiteSpace(c.Phone) && repo.ExistsPhone(c.Phone))
            { err = "SĐT đã tồn tại"; return false; }

            return repo.Insert(c) > 0;
        }

        public bool Update(Customer c, out string err)
        {
            err = "";
            if (string.IsNullOrWhiteSpace(c.Name))
            { err = "Tên khách không được trống"; return false; }
            return repo.Update(c) > 0;
        }

        public bool Delete(int id) => repo.Delete(id) > 0;

        // ===== NEW: cộng điểm khi checkout =====
        public bool AddPoints(int customerId, int addPts, out string err)
        {
            err = "";
            if (addPts <= 0) return true;

            int rows = repo.AddPoints(customerId, addPts);
            if (rows <= 0)
            {
                err = "Không cộng được điểm (không tìm thấy khách?)";
                return false;
            }
            return true;
        }

        public int GetPoints(int customerId)
        {
            return repo.GetPoints(customerId);
        }

        public bool SetPoints(int customerId, int newPoints, out string err)
        {
            err = "";
            if (newPoints < 0) newPoints = 0;

            int rows = repo.SetPoints(customerId, newPoints);
            if (rows <= 0)
            {
                err = "Không cập nhật được điểm (không tìm thấy khách?)";
                return false;
            }
            return true;
        }

    }
}
