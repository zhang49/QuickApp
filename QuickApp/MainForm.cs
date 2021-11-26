using Microsoft.Win32;
using NHotkey.WindowsForms;
using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.WinForms;
using CefSharp;
using System.IO;
using System.Management;

namespace QuickApp
{
    public partial class MainForm : Form
    {
        private NotifyIcon notifyIcon = null;
        private bool mIsStop = false;
        public ChromiumWebBrowser browser;
        public MainForm()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.app;
            //InitialTray();
            InitBrowser();
        }


        private void InitBrowser()
        {
            Cef.Initialize(new CefSettings());
            browser = new ChromiumWebBrowser(Directory.GetCurrentDirectory() + Properties.Resources.CefIndexHtmlRelFilePath);
            browser.Dock = DockStyle.Fill;
            browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            CefSharpSettings.WcfEnabled = true;

            browser.KeyboardHandler = new CEFKeyBoardHander();
            this.Controls.Add(browser);
        }

        public void RegistHotKey()
        {
            try
            {
                HotkeyManager.Current.AddOrReplace("Hotkey1", Keys.LControlKey | Keys.Shift | Keys.Space, (sender, o) =>
                {
                    this.Invoke(new ThreadStart(() =>
                    {
                        MessageBox.Show("d");
                    }));
                });
            }
            catch (Exception e)
            {

                this.Invoke(new ThreadStart(() =>
                {
                    MessageBox.Show(e.Message, "错误");
                }));
            }
        }


        /// <summary>
        /// 托盘初始化函数
        /// </summary>
        private void InitialTray()
        {
            //隐藏主窗体
            this.Hide();
            //实例化一个NotifyIcon对象  
            notifyIcon = new NotifyIcon();
            //托盘图标显示的内容
            notifyIcon.Text = this.Text;
            //注意：下面的路径可以是绝对路径、相对路径。但是需要注意的是：文件必须是一个.ico格式  

            notifyIcon.Icon = Properties.Resources.app;
            //true表示在托盘区可见，false表示在托盘区不可见  
            notifyIcon.Visible = true;

            //托盘图标气泡显示的内容  
            //notifyIcon.BalloonTipText = "正在后台运行";
            //气泡显示的时间（单位是毫秒）  
            //notifyIcon.ShowBalloonTip(2000);

            notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(notifyIcon_MouseClick);
            ////设置二级菜单  
            //MenuItem setting1 = new MenuItem("二级菜单1");
            //MenuItem setting2 = new MenuItem("二级菜单2");  
            //MenuItem setting = new MenuItem("一级菜单", new MenuItem[]{setting1,setting2});
            MenuItem exit = new MenuItem("Exit");
            exit.Click += new EventHandler(exit_Click);
            ////关联托盘控件
            MenuItem[] childen = new MenuItem[] { exit };
            notifyIcon.ContextMenu = new ContextMenu(childen);
        }

        private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //鼠标左键单击  
            if (e.Button == MouseButtons.Left)
            {
                //如果窗体是可见的，那么鼠标左击托盘区图标后，窗体为不可见  
                if (this.Visible == true)
                {
                    this.Visible = false;
                }
                else
                {
                    this.Visible = true;
                    this.Activate();
                }
            }
        }

        public static string GetRegeditData()
        {
            //Win10 读写LocalMachine权限，没有访问权限
            RegistryKey hkml = Registry.CurrentUser;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
            RegistryKey aimdir = software.OpenSubKey("EmailTool", true);
            if (aimdir == null)
            {
                return null;
            }
            object value = aimdir.GetValue("LastDate");
            return value == null ? null : value.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RegistHotKey();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            //退出程序  
            mIsStop = true;
            System.Environment.Exit(0);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
