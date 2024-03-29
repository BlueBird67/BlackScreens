﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.ApplicationServices;
using System.Windows.Forms;
using WindowsDisplayAPI.DisplayConfig;
using NLog;

using System.Xml.Linq;
using NLog.Config;
using System.IO;
using System.Xml.Serialization;

namespace BlackScreensWPF
{
    public class CommonData
    {
        public static CommonData dataInstance = new CommonData();

        private int opacity = 90;
        private string screen1Name = "";
        private string screen2Name = "";
        private string screen3Name = "";
        private string screen4Name = "";
        private string screen5Name = "";
        private string screen6Name = "";
        private string imageFileNameScreen1 = "";
        private string imageFileNameScreen2 = "";
        private string imageFileNameScreen3 = "";
        private string imageFileNameScreen4 = "";
        private string imageFileNameScreen5 = "";
        private string imageFileNameScreen6 = "";
        private string paramsScreenDeviceName = "";
        private MainWindow fParams = null;
        private BlackWindow[] blackWindows = new BlackWindow[Screen.AllScreens.Length];
        private PathDisplayTarget[] displays = null;
        private Rectangle screen1TooltipData = new Rectangle();
        private Rectangle screen2TooltipData = new Rectangle();
        private Rectangle screen3TooltipData = new Rectangle();
        private Rectangle screen4TooltipData = new Rectangle();
        private Rectangle screen5TooltipData = new Rectangle();
        private Rectangle screen6TooltipData = new Rectangle();
        private bool hideTexts = true;
        private bool clickThrough = false;
        private bool reduceAppOnLaunch = false;
        private bool firstAppLaunch = true;
        private int msDelayMouseCursorHide = 3000;
        private Logger logToFile = null;
        private string optionFileLocation = "";
        private String appDataBaseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private String appDataCompleteFolderUri = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BlackScreens";

        // Users options
        public int Opacity { get => opacity; set => opacity = value; }
        public bool HideTexts { get => hideTexts; set => hideTexts = value; }
        public bool ClickThrough { get => clickThrough; set => clickThrough = value; }
        public bool ReduceAppOnLaunch { get => reduceAppOnLaunch; set => reduceAppOnLaunch = value; }
        public bool FirstAppLaunch { get => firstAppLaunch; set => firstAppLaunch = value; }

        public string Screen1Name { get => screen1Name; set => screen1Name = value; }
        public string Screen2Name { get => screen2Name; set => screen2Name = value; }
        public string Screen3Name { get => screen3Name; set => screen3Name = value; }
        public string Screen4Name { get => screen4Name; set => screen4Name = value; }
        public string Screen5Name { get => screen5Name; set => screen5Name = value; }
        public string Screen6Name { get => screen6Name; set => screen6Name = value; }
        public String ImageFileNameScreen1 { get => imageFileNameScreen1; set => imageFileNameScreen1 = value; }
        public String ImageFileNameScreen2 { get => imageFileNameScreen2; set => imageFileNameScreen2 = value; }
        public String ImageFileNameScreen3 { get => imageFileNameScreen3; set => imageFileNameScreen3 = value; }
        public String ImageFileNameScreen4 { get => imageFileNameScreen4; set => imageFileNameScreen4 = value; }
        public String ImageFileNameScreen5 { get => imageFileNameScreen5; set => imageFileNameScreen5 = value; }
        public String ImageFileNameScreen6 { get => imageFileNameScreen6; set => imageFileNameScreen6 = value; }
        // Keep MainWindow handle, use in BlackWindows and App.cs
        public MainWindow FParams { get => fParams; set => fParams = value; }
        // BlackWindow used for each screen
        public BlackWindow[] BlackWindows { get => blackWindows; set => blackWindows = value; }
        public string ParamsScreenDeviceName { get => paramsScreenDeviceName; set => paramsScreenDeviceName = value; }
        public PathDisplayTarget[] Displays { get => displays; set => displays = value; }
        public Rectangle Screen1TooltipData { get => screen1TooltipData; set => screen1TooltipData = value; }
        public Rectangle Screen2TooltipData { get => screen2TooltipData; set => screen2TooltipData = value; }
        public Rectangle Screen3TooltipData { get => screen3TooltipData; set => screen3TooltipData = value; }
        public Rectangle Screen4TooltipData { get => screen4TooltipData; set => screen4TooltipData = value; }
        public Rectangle Screen5TooltipData { get => screen5TooltipData; set => screen5TooltipData = value; }
        public Rectangle Screen6TooltipData { get => screen6TooltipData; set => screen6TooltipData = value; }
        public int MsDelayMouseCursorHide { get => msDelayMouseCursorHide; set => msDelayMouseCursorHide = value; }
        public string OptionFileLocation { get => optionFileLocation; set => optionFileLocation = value; }
        public string AppDataBaseFolder { get => appDataBaseFolder; set => appDataBaseFolder = value; }
        public string AppDataCompleteFolderUri { get => appDataCompleteFolderUri; set => appDataCompleteFolderUri = value; }
        public Logger LogToFile
        {
            get => logToFile; set => logToFile = value;
        }

        static CommonData()
        {
            if (!Directory.Exists(dataInstance.AppDataCompleteFolderUri))
            {
                Directory.CreateDirectory(dataInstance.AppDataCompleteFolderUri);
            }
            var logFactory = new LogFactory();
            String fileConfigLogUri = dataInstance.AppDataCompleteFolderUri + "\\NLog.config";
            if (File.Exists(fileConfigLogUri)) {
                logFactory.Setup().LoadConfigurationFromFile(fileConfigLogUri);
                dataInstance.LogToFile = logFactory.GetCurrentClassLogger();
            }
            else
                dataInstance.LogToFile = logFactory.CreateNullLogger();

            

            dataInstance.LogToFile.Debug("Creating CommonData static instance");
        }

        /// <summary>
        /// Update all BlackWindows Opacity parameter
        /// </summary>
        public void updateAllBlackWindowParams()
        {
            foreach (BlackWindow bw in blackWindows)
            {
                if (bw != null)
                    bw.updateBlackWindowParams();
            }
        }

        /// <summary>
        /// Searching for PathDisplayTarget of a System.WindowsForm.Screen.DeviceName
        /// DeviceName give by System.WindowsForm.Screen.DeviceName only give result like "\\.\DISPLAYX"
        /// Needed to search Windows API struct PathDisplayTarget.FriendlyName, which give us the REAL name of the screen known by Windows
        /// PathDisplayTotarget.ScreenName == System.WindowsForm.Screen.DeviceName == "\\.\DISPLAYX" 
        /// ex. PathDisplayToTarget.FriendlyName = "SAMSUNG XXX"
        /// </summary>
        /// <param name="screenNameToSearch">Screen name to search, coming from System.WindowsForm.Screen.AllDevices[x].DeviceName (ex "\\.\DISPLAY1")</param>
        public PathDisplayTarget findDisplayByScreenName(String screenNameToSearch)
        {
            if ((displays != null) && (displays.Length > 0))
            {
                foreach (PathDisplayTarget currentDisplayTarget in displays)
                {
                    if (String.Equals(currentDisplayTarget.ToDisplayDevice().ScreenName, screenNameToSearch))
                        return currentDisplayTarget;
                }
            }
            return null;
        }

        /// <summary>
        /// Save user config file by XmlSerializer
        /// </summary>
        public void saveUserConfigFiles()
        {
            UserPreferences up = new UserPreferences
            {
                Opacity = CommonData.dataInstance.Opacity,
                ShowTextsOnBlackScreens = !CommonData.dataInstance.HideTexts,
                ClickThrough = CommonData.dataInstance.ClickThrough,
                MsDelayMouseCursorHide = CommonData.dataInstance.MsDelayMouseCursorHide,
                FirstAppLaunch = CommonData.dataInstance.FirstAppLaunch,
                ReduceAppOnLaunch = CommonData.dataInstance.ReduceAppOnLaunch,
                ImageFileNameScreen1 = CommonData.dataInstance.ImageFileNameScreen1,
                ImageFileNameScreen2 = CommonData.dataInstance.ImageFileNameScreen2,
                ImageFileNameScreen3 = CommonData.dataInstance.ImageFileNameScreen3,
                ImageFileNameScreen4 = CommonData.dataInstance.ImageFileNameScreen4,
                ImageFileNameScreen5 = CommonData.dataInstance.ImageFileNameScreen5,
                ImageFileNameScreen6 = CommonData.dataInstance.ImageFileNameScreen6
            };

            //String exeLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            XmlSerializer mySerializer = new XmlSerializer(typeof(UserPreferences));
            try
            {
                StreamWriter myWriter = new StreamWriter(AppDataCompleteFolderUri + "\\BlackScreensPrefs.xml");
                mySerializer.Serialize(myWriter, up);
                myWriter.Close();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Load user config file by XmlSerializer
        /// </summary>
        public void loadUserConfigFile(MainWindow mainWindow)
        {
            UserPreferences up;

            //String exeLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            CommonData.dataInstance.OptionFileLocation = AppDataCompleteFolderUri + "\\BlackScreensPrefs.xml";
            XmlSerializer mySerializer = new XmlSerializer(typeof(UserPreferences));

            CommonData.dataInstance.LogToFile.Debug("loadUserConfigFile()");

            // Default values
            CommonData.dataInstance.ClickThrough = false;
            CommonData.dataInstance.HideTexts = false;
            CommonData.dataInstance.Opacity = 90;
            CommonData.dataInstance.MsDelayMouseCursorHide = 3000;
            CommonData.dataInstance.ReduceAppOnLaunch = false;
            CommonData.dataInstance.FirstAppLaunch = true;
            CommonData.dataInstance.ImageFileNameScreen1 = "";
            CommonData.dataInstance.ImageFileNameScreen2 = "";
            CommonData.dataInstance.ImageFileNameScreen3 = "";
            CommonData.dataInstance.ImageFileNameScreen4 = "";
            CommonData.dataInstance.ImageFileNameScreen5 = "";
            CommonData.dataInstance.ImageFileNameScreen6 = "";

            try
            {
                FileStream myFileStream = new FileStream(CommonData.dataInstance.AppDataCompleteFolderUri + "\\BlackScreensPrefs.xml", FileMode.Open);
                up = (UserPreferences)mySerializer.Deserialize(myFileStream);
                CommonData.dataInstance.Opacity = up.Opacity;
                CommonData.dataInstance.HideTexts = !up.ShowTextsOnBlackScreens;
                CommonData.dataInstance.ClickThrough = up.ClickThrough;
                CommonData.dataInstance.ReduceAppOnLaunch = up.ReduceAppOnLaunch;
                CommonData.dataInstance.FirstAppLaunch = up.FirstAppLaunch;
                CommonData.dataInstance.MsDelayMouseCursorHide = up.MsDelayMouseCursorHide;
                CommonData.dataInstance.ImageFileNameScreen1 = up.ImageFileNameScreen1;
                CommonData.dataInstance.ImageFileNameScreen2 = up.ImageFileNameScreen2;
                CommonData.dataInstance.ImageFileNameScreen3 = up.ImageFileNameScreen3;
                CommonData.dataInstance.ImageFileNameScreen4 = up.ImageFileNameScreen4;
                CommonData.dataInstance.ImageFileNameScreen5 = up.ImageFileNameScreen5;
                CommonData.dataInstance.ImageFileNameScreen6 = up.ImageFileNameScreen6;
            }
            catch (Exception) { }
        }
    }
}
