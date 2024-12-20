﻿using FP_CLOCKLib;
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
using System.IO;
using Newtonsoft.Json;


using AxFP_CLOCKLib;
using System.Threading;

namespace ZamanPardazSDK
{
    public partial class MainForm : Form
    {
        // Constants
        private const int UserMessageBase = 0x500;
        private const int CustomMessage = UserMessageBase + 1;
        LogRecord LastLoggedUser = new LogRecord
        {
            EnrollNumber = -1
        };


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


        public bool ConnectToDevice(string ipAddress, int port, int password = 0, bool prompt = false)
        {
            // Validate input parameters
            if (string.IsNullOrWhiteSpace(ipAddress) || port <= 0)
            {
                if (prompt)
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
            toolDeviceStat.Text = "در حال اتصال...";

            DeviceConnectionInfo connectionInfo;
            try
            {
                connectionInfo = DeviceConfigManager.LoadDeviceConnectionInfo();
            }
            catch (FileNotFoundException)
            {
                toolDeviceStat.Text = "فایل تنظیمات موجود نیست.";
                return;
            }

            var stat = ConnectToDevice(connectionInfo.DeviceConnection.IPAddress, connectionInfo.DeviceConnection.Port, prompt: false);
            if (stat)
            {
                toolDeviceStat.Text = "اتصال برقرار شد";
                var data = GetAllLogData(connectionInfo.DeviceConnection.MachineNumber);
                if (data.Any())
                {
                    LastLoggedUser = data.LastOrDefault();
                }
                timer1.Enabled = true;
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            var data = GetAllLogData(MachineNumber);
            if (data.Any())
            {


                var current = data.LastOrDefault();
                if (LastLoggedUser!= null && LastLoggedUser.Timestamp != current.Timestamp)
                {
                    LastLoggedUser = current;
                    var username = GetUserName(current.EnrollNumber);
                    lblMessage.Text = $"{username}";
                    lblDateTime.Text = $" at {current.Timestamp}";
                    this.BackColor = Color.Gold;

                    timerFlash.Enabled = true;
                }
            }
        }

        private void timerFlash_Tick(object sender, EventArgs e)
        {
            this.BackColor = Color.LightGray;
            timerFlash.Enabled = false;

        }

        private void lblMessage_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "خوش آمدید";
        }
    }
}
