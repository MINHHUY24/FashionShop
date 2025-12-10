using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FashionShop.BLL;
using FashionShop.DTO;

namespace FashionShop.GUI
{
    public partial class FrmAccounts : Form
    {
        private readonly AccountService service = new AccountService();
        private readonly Account current;

        private DataGridView dgv;
        private TextBox txtUser, txtPass, txtSearch;
        private ComboBox cboStaff;
        private Button btnAdd, btnDel, btnReload, btnClearSearch;
        private Label lblCount;

        private DataTable accountsTable;
        private DataView accountsView;
        private readonly string hintSearch = "Search staff username...";

        public FrmAccounts(Account acc)
        {
            current = acc;

            Text = "Manage Staff Accounts";
            Size = new Size(720, 520);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10f);
            MinimumSize = new Size(720, 520);

            // ✅ Designer có thể vẫn tồn tại nhưng không bắt buộc gọi,
            // gọi cũng không sao vì chúng ta không tự viết InitializeComponent ở đây.
            // Nếu designer file bạn vẫn có, dòng này OK.
            InitializeComponent();

            BuildUI();
            ApplyRolePermission();
        }

        // ✅ HÀM LOAD ĐÚNG TÊN để Designer không báo CS1061
        private void FrmAccounts_Load(object sender, EventArgs e)
        {
            LoadStaffCombo();
            LoadGrid();
        }

        private void BuildUI()
        {
            Controls.Clear();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 1
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 140)); // input
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));  // buttons
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));  // search
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));  // count
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // grid
            Controls.Add(root);

            // ===== INPUT =====
            var pnlInput = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            pnlInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            pnlInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            pnlInput.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // staff
            pnlInput.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // username
            pnlInput.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // password

            pnlInput.Controls.Add(new Label
            {
                Text = "Staff employee",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            cboStaff = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            pnlInput.Controls.Add(cboStaff, 1, 0);

            pnlInput.Controls.Add(new Label
            {
                Text = "Username",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);

            txtUser = new TextBox { Dock = DockStyle.Fill };
            pnlInput.Controls.Add(txtUser, 1, 1);

            pnlInput.Controls.Add(new Label
            {
                Text = "Password",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 2);

            txtPass = new TextBox
            {
                Dock = DockStyle.Fill,
                UseSystemPasswordChar = true
            };
            pnlInput.Controls.Add(txtPass, 1, 2);

            root.Controls.Add(pnlInput, 0, 0);

            // ===== BUTTONS =====
            var pnlBtn = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10, 0, 10, 0)
            };

            btnAdd = MakeButton("Create Staff", Color.FromArgb(33, 150, 243));
            btnDel = MakeButton("Delete Staff", Color.FromArgb(244, 67, 54));
            btnReload = MakeButton("Reload", Color.FromArgb(96, 125, 139));

            pnlBtn.Controls.AddRange(new Control[] { btnAdd, btnDel, btnReload });
            root.Controls.Add(pnlBtn, 0, 1);

            btnAdd.Click += BtnAdd_Click;
            btnDel.Click += BtnDel_Click;
            btnReload.Click += (s, e) =>
            {
                LoadStaffCombo();
                LoadGrid();
                ClearForm();
            };

            // ===== SEARCH =====
            var pnlSearch = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10, 0, 10, 0)
            };
            pnlSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));

            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10.5f),
                ForeColor = Color.Gray,
                Text = hintSearch
            };

            btnClearSearch = new Button
            {
                Dock = DockStyle.Fill,
                Text = "X",
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClearSearch.FlatAppearance.BorderSize = 0;

            pnlSearch.Controls.Add(txtSearch, 0, 0);
            pnlSearch.Controls.Add(btnClearSearch, 1, 0);
            root.Controls.Add(pnlSearch, 0, 2);

            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text == hintSearch)
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = Color.Black;
                }
            };
            txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = hintSearch;
                    txtSearch.ForeColor = Color.Gray;
                }
            };

            txtSearch.TextChanged += (s, e) =>
            {
                if (txtSearch.Focused) ApplySearch();
            };

            btnClearSearch.Click += (s, e) =>
            {
                txtSearch.Text = hintSearch;
                txtSearch.ForeColor = Color.Gray;
                if (accountsView != null) accountsView.RowFilter = "";
            };

            // ===== COUNT =====
            lblCount = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Font = new Font("Segoe UI Semibold", 10.5f),
                ForeColor = Color.FromArgb(33, 33, 33)
            };
            root.Controls.Add(lblCount, 0, 3);

            // ===== GRID =====
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };
            StyleGrid(dgv);
            root.Controls.Add(dgv, 0, 4);
        }

        private void LoadStaffCombo()
        {
            var dt = service.GetStaffEmployeesForCombo();
            cboStaff.DataSource = dt;
            cboStaff.DisplayMember = "employee_name";
            cboStaff.ValueMember = "employee_id";
            cboStaff.SelectedIndex = dt.Rows.Count > 0 ? 0 : -1;
        }

        private void LoadGrid()
        {
            accountsTable = service.GetStaffAccounts();
            accountsView = accountsTable.DefaultView;
            dgv.DataSource = accountsView;

            lblCount.Text = $"Total staff accounts: {accountsTable.Rows.Count}";
        }

        private void ApplySearch()
        {
            if (accountsView == null) return;

            string key = txtSearch.Text.Trim();
            if (key == hintSearch) key = "";
            accountsView.RowFilter = "";

            if (!string.IsNullOrWhiteSpace(key))
            {
                key = key.Replace("'", "''");
                accountsView.RowFilter =
                    $"CONVERT([username], 'System.String') LIKE '%{key}%'";
            }
        }

        private void ApplyRolePermission()
        {
            bool isAdmin = current != null &&
                           current.Role != null &&
                           current.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            btnAdd.Enabled = isAdmin;
            btnDel.Enabled = isAdmin;
            txtUser.ReadOnly = !isAdmin;
            txtPass.ReadOnly = !isAdmin;
            cboStaff.Enabled = isAdmin;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();
            int employeeId = cboStaff.SelectedIndex >= 0
                ? Convert.ToInt32(cboStaff.SelectedValue)
                : 0;

            if (service.CreateStaffAccount(username, password, employeeId, out string err))
            {
                MessageBox.Show("Created staff account!");
                LoadGrid();
                ClearForm();
            }
            else MessageBox.Show(err);
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("Please choose a staff account first.");
                return;
            }

            int accountId = Convert.ToInt32(dgv.CurrentRow.Cells["account_id"].Value);
            string username = dgv.CurrentRow.Cells["username"].Value?.ToString();

            var confirm = MessageBox.Show(
                $"Delete staff account '{username}'?",
                "Confirm delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm == DialogResult.Yes)
            {
                if (service.DeleteStaffAccount(accountId, out string err))
                {
                    MessageBox.Show("Deleted!");
                    LoadGrid();
                }
                else MessageBox.Show(err);
            }
        }

        private void ClearForm()
        {
            txtUser.Clear();
            txtPass.Clear();
        }

        private Button MakeButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                Width = 135,
                Height = 32,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(6)
            };
        }

        private void StyleGrid(DataGridView g)
        {
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10f);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;

            g.AllowUserToResizeColumns = false;
            g.AllowUserToResizeRows = false;
            g.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            g.RowTemplate.Height = 30;
            g.ColumnHeadersHeight = 36;

            g.DefaultCellStyle.Font = new Font("Segoe UI", 10f);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(33, 150, 243);
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
        }
    }
}
