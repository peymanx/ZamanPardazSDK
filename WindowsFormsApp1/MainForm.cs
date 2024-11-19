using FP_CLOCKLib;
using FPCLOCK_SVRLib;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using AxFP_CLOCKLib;
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class MainForm : Form
    {
        // Constants
        private const int UserMessageBase = 0x500;
        private const int CustomMessage = UserMessageBase + 1;

        // Fields
        private int MachineNumber = 1;
        private bool _isDeviceConnected = false;
        private AxFP_CLOCK FaceDevice;

        public MainForm()
        {
            InitializeComponent();

            // Initialize the ActiveX device instance
            FaceDevice = new AxFP_CLOCK();
            FaceDevice.CreateControl(); // Ensure proper initialization
            Controls.Add(FaceDevice);
        }

        /// <summary>
        /// Connects to the device over the network.
        /// </summary>
        /// <param name="ipAddress">IP address of the device.</param>
        /// <param name="port">Port to connect to.</param>
        /// <param name="password">Optional password (default is 0).</param>
        /// <returns>True if connected successfully, false otherwise.</returns>
        public bool ConnectToDevice(string ipAddress, int port, int password = 0, bool prompt = false)
        {
            // Validate input parameters
            if (string.IsNullOrWhiteSpace(ipAddress) || port <= 0)
            {
                if(prompt)
                MessageBox.Show("Invalid IP Address or Port.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Check if the device instance is initialized
            if (FaceDevice == null)
            {
                if (prompt)

                    MessageBox.Show("Device instance is not initialized.", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                // Configure network settings
                bool isConfigured = FaceDevice.SetIPAddress(ref ipAddress, port, password);
                if (!isConfigured)
                {
                    if (prompt)

                        MessageBox.Show("Failed to configure network parameters.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Attempt to open communication
                bool isConnected = FaceDevice.OpenCommPort(MachineNumber);
                if (isConnected)
                {
                    _isDeviceConnected = true;
                    if (prompt)

                        MessageBox.Show("Device connected successfully.", "Connection Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    if (prompt)

                        MessageBox.Show("Failed to connect to the device.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                if (prompt)

                    MessageBox.Show($"An error occurred: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }


        public struct GeneralLogInfo
        {
            public int dwTMachineNumber;
            public int dwEnrollNumber;
            public int dwEMachineNumber;
            public int dwVerifyMode;
            public int dwInout;
            public int dwEvent;
            public int dwYear;
            public int dwMonth;
            public int dwDay;
            public int dwHour;
            public int dwMinute;
            public int dwSecond;
        }

   


        public string GetUserName(int UserID)
        {
            // اطمینان از غیرفعال بودن دستگاه قبل از عملیات
            FaceDevice.EnableDevice(MachineNumber, 0);

            // مقدار اولیه نام کاربر
            string userName = "";
            object obj = new System.Runtime.InteropServices.VariantWrapper(userName);

            // تلاش برای دریافت نام کاربر
            bool isSuccess = FaceDevice.GetUserName(0, MachineNumber, UserID, 1, ref obj);

            // فعال‌سازی مجدد دستگاه پس از عملیات
            FaceDevice.EnableDevice(MachineNumber, 1);

            // اگر عملیات موفق بود، نام کاربر را برمی‌گردانیم
            if (isSuccess)
            {
                return (string)obj;
            }
            else
            {
                return null; // بازگشت مقدار null در صورت شکست عملیات
            }
        }


      

        // متد برای خواندن رکوردها و بازگرداندن یک لیست
        public List<LogRecord> GetAllLogData(int deviceNumber)
        {
            // لیستی برای ذخیره رکوردها
            List<LogRecord> logRecords = new List<LogRecord>();

            // اطمینان از غیرفعال بودن دستگاه قبل از عملیات
            FaceDevice.EnableDevice(MachineNumber, 0);

            // خواندن تمام داده‌های لاگ از دستگاه
            bool isReadSuccessful = FaceDevice.ReadAllGLogData(deviceNumber);
            if (!isReadSuccessful)
            {
               // ShowErrorInfo();
                FaceDevice.EnableDevice(deviceNumber, 1);
                return logRecords; // لیست خالی در صورت خطا
            }

            GeneralLogInfo gLogInfo = new GeneralLogInfo();
            bool hasMoreData;

            // خواندن داده‌ها به صورت حلقه تا زمانی که داده‌ها موجود باشد
            do
            {
                hasMoreData = FaceDevice.GetAllGLogDataWithSecond(
                    deviceNumber,
                    ref gLogInfo.dwTMachineNumber,
                    ref gLogInfo.dwEnrollNumber,
                    ref gLogInfo.dwEMachineNumber,
                    ref gLogInfo.dwVerifyMode,
                    ref gLogInfo.dwInout,
                    ref gLogInfo.dwEvent,
                    ref gLogInfo.dwYear,
                    ref gLogInfo.dwMonth,
                    ref gLogInfo.dwDay,
                    ref gLogInfo.dwHour,
                    ref gLogInfo.dwMinute,
                    ref gLogInfo.dwSecond
                );

                if (hasMoreData)
                {
                    // ساخت رکورد و افزودن آن به لیست
                    var logRecord = new LogRecord
                    {
                        DeviceNumber = gLogInfo.dwTMachineNumber,
                        EnrollNumber = gLogInfo.dwEnrollNumber,
                        MachineNumber = gLogInfo.dwEMachineNumber,
                        Event = gLogInfo.dwEvent,
                        VerifyMode = gLogInfo.dwVerifyMode,
                        Timestamp = new DateTime(
                            gLogInfo.dwYear,
                            gLogInfo.dwMonth,
                            gLogInfo.dwDay,
                            gLogInfo.dwHour,
                            gLogInfo.dwMinute,
                            gLogInfo.dwSecond
                        )
                    };
                    logRecords.Add(logRecord);
                }

            } while (hasMoreData);

            // فعال‌سازی مجدد دستگاه پس از عملیات
            FaceDevice.EnableDevice(deviceNumber, 1);

            // بازگرداندن لیست رکوردها
            return logRecords;
        }



        private void btnConnect(object sender, EventArgs e)
        {
            lblMessage.Text = "در حال اتصال...";

            string ipAddress = "172.21.60.58"; // Replace with your IP address
            int port = 5005; // Replace with your port
            var stat = ConnectToDevice(ipAddress, port, prompt: false);
            if (stat)
            {
                lblMessage.Text = "اتصال برقرار  شد";

            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var data = GetAllLogData(MachineNumber);
            if (data.Any())
            {
                var last = data.LastOrDefault();
                var username = GetUserName(last.EnrollNumber);
                MessageBox.Show($"{username} at {last.Timestamp}");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var me = GetUserName(2002);
            MessageBox.Show(me);
        }
    }
}
