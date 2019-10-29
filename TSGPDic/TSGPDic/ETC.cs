using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using Android.Preferences;
using System.Threading.Tasks;
using System.Net;

namespace TSGPDic
{
    internal static class ETC
    {
        internal static string Server = "http://chlwlsgur96.ipdisk.co.kr/publist/HDD1/Data/Project/TSGPDic/";
        internal static string SDCardPath = (string)Android.OS.Environment.ExternalStorageDirectory;
        internal static string tempPath = Path.Combine(SDCardPath, "TSGPDic_Temp");
        internal static string AppDataPath = Path.Combine(SDCardPath, "Android", "data", "com.tsgp.dic");
        internal static string DBPath = Path.Combine(AppDataPath, "DB");
        internal static string SystemPath = Path.Combine(AppDataPath, "System");
        internal static string CachePath = Path.Combine(AppDataPath, "Cache");
        internal static string LogPath = Path.Combine(SystemPath, "Log");

        internal static bool HasEvent = false;
        internal static bool IsServerDown = false;

        internal static int DialogBG = 0;
        internal static int DialogBG_Vertical = 0;
        internal static int DialogBG_Download = 0;

        internal static string Notification_String = "";

        internal static DataTable MusicList = new DataTable();
        internal static DataTable CharacterList = new DataTable();

        internal static Android.Content.Res.Resources Resources = null;

        internal static ISharedPreferences sharedPreferences;

        internal static void BasicInitializeApp(Activity context)
        {
            sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
            Resources = context.Resources;

            CheckInitFolder();
            //SetDialogTheme();
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NTQzNDRAMzEzNjJlMzQyZTMwZHNFSDUyRjdlWXZ6WXNtelNkRWV3QVh1WmR0Q3hSbTFqZ0dKTTVsQlBOQT0=");
        }

        internal static async Task CheckServerNetwork()
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            try
            {
                request = WebRequest.Create(ETC.Server) as HttpWebRequest;
                request.Method = "HEAD";
                response = request.GetResponse() as HttpWebResponse;

                if (response.StatusCode == HttpStatusCode.OK)
                    IsServerDown = false;
                else IsServerDown = true;
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
                IsServerDown = true;
            }
            finally
            {
                if (response != null) response.Close();
                response.Dispose();
            }
        }

        internal static DataRow FindDataRow<T>(DataTable table, string index, T value)
        {
            for (int i = 0; i < table.Rows.Count; ++i)
            {
                DataRow dr = table.Rows[i];
                if (((T)dr[index]).Equals(value)) return dr;
            }

            return null;
        }

        internal static bool FindDataRow<T>(DataTable table, string index, T value, out DataRow row)
        {
            for (int i = 0; i < table.Rows.Count; ++i)
            {
                DataRow dr = table.Rows[i];
                if (((T)dr[index]).Equals(value))
                {
                    row = dr;
                    return true;
                }
            }

            row = null;
            return false;
        }

        internal static void CheckInitFolder()
        {
            if (Directory.Exists(tempPath) == false) Directory.CreateDirectory(tempPath);
            else
            {
                Directory.Delete(tempPath, true);
                Directory.CreateDirectory(tempPath);
            }

            DirectoryInfo AppDataDI = new DirectoryInfo(AppDataPath);

            if (AppDataDI.Exists == false) AppDataDI.Create();

            string[] MainPaths =
            {
                DBPath,
                SystemPath,
                LogPath,
                CachePath
            };

            string[] SubPaths =
            {
                Path.Combine(CachePath, "Character"),
                Path.Combine(CachePath, "Character", "SD"),
                Path.Combine(CachePath, "Character", "SD", "Animation"),
                Path.Combine(CachePath, "Character", "Crop"),
                Path.Combine(CachePath, "Character", "Crop", "Normal"),
                Path.Combine(CachePath, "Character", "Crop", "Awakening"),
                Path.Combine(CachePath, "Character", "Crop", "Costume"),
                Path.Combine(CachePath, "Character", "Normal"),
                Path.Combine(CachePath, "Character", "Skill"),
                Path.Combine(CachePath, "Music"),
                Path.Combine(CachePath, "Music", "Album"),
                Path.Combine(CachePath, "Music", "Album", "Crop"),
                Path.Combine(CachePath, "Music", "Preview")
            };

            foreach (string path in MainPaths) if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
            foreach (string path in SubPaths) if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
        }

        internal static async Task<bool> CheckDBVersion()
        {
            if (IsServerDown == true) return false;

            string LocalDBVerPath = Path.Combine(SystemPath, "DBVer.txt");
            string ServerDBVerPath = Path.Combine(Server, "DBVer.txt");
            string TempDBVerPath = Path.Combine(tempPath, "DBVer.txt");

            bool HasDBUpdate = false;

            if (File.Exists(LocalDBVerPath) == false) HasDBUpdate = true;
            else
            {
                using (WebClient wc = new WebClient())
                    await wc.DownloadFileTaskAsync(ServerDBVerPath, TempDBVerPath);

                using (StreamReader sr1 = new StreamReader(new FileStream(LocalDBVerPath, FileMode.Open, FileAccess.Read)))
                {
                    using (StreamReader sr2 = new StreamReader(new FileStream(TempDBVerPath, FileMode.Open, FileAccess.Read)))
                    {
                        int localVer = int.Parse(sr1.ReadToEnd());
                        int serverVer = int.Parse(sr2.ReadToEnd());

                        if (localVer < serverVer) HasDBUpdate = true;
                    }
                }
            }

            return HasDBUpdate;
        }

        internal static async Task UpdateDB(Activity activity, int TitleMsg = Resource.String.CheckDBUpdateDialog_Title, int MessageMgs = Resource.String.CheckDBUpdateDialog_Message)
        {
            string[] DBFiles =
            {
                "Song.tsgp",
                "Character.tsgp",
            };

            ProgressDialog pd = new ProgressDialog(activity, DialogBG_Download);
            pd.SetProgressStyle(ProgressDialogStyle.Horizontal);
            pd.SetTitle(TitleMsg);
            pd.SetMessage(Resources.GetString(MessageMgs));
            pd.SetCancelable(false);
            pd.Max = 100;
            pd.Show();

            using (WebClient wc = new WebClient())
            {
                for (int i = 0; i < DBFiles.Length; ++i)
                {
                    string url = Path.Combine(Server, "DB", DBFiles[i]);
                    string target = Path.Combine(tempPath, DBFiles[i]);
                    pd.SecondaryProgress = Convert.ToInt32((double)pd.Max / DBFiles.Length * (i + 1));
                    await wc.DownloadFileTaskAsync(url, target);
                }

                string url2 = Path.Combine(Server, "DBVer.txt");
                string target2 = Path.Combine(tempPath, "DBVer.txt");
                await wc.DownloadFileTaskAsync(url2, target2);
                await Task.Delay(100);
            }

            for (int i = 0; i < DBFiles.Length; ++i)
            {
                string originalFile = Path.Combine(tempPath, DBFiles[i]);
                string targetFile = Path.Combine(DBPath, DBFiles[i]);
                File.Copy(originalFile, targetFile, true);
                pd.Progress = Convert.ToInt32(((double)pd.Max / DBFiles.Length) * (i + 1));
                await Task.Delay(100);
            }

            await Task.Delay(500);

            activity.RunOnUiThread(() => { pd.SetMessage(Resources.GetString(Resource.String.UpdateDBDialog_RefreshVersionMessage)); });

            string oldVersion = Path.Combine(SystemPath, "DBVer.txt");
            string newVersion = Path.Combine(tempPath, "DBVer.txt");
            File.Copy(newVersion, oldVersion, true);

            await Task.Delay(500);

            pd.Dismiss();
        }

        internal static bool LoadDBSync(DataTable table, string DBFile, bool BeforeClear)
        {
            try
            {
                if (BeforeClear == true) table.Clear();
                table.ReadXml(Path.Combine(DBPath, DBFile));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        internal static void LogError(Activity activity, string error)
        {
            try
            {
                DateTime now = DateTime.Now;

                string nowDateTime = $"{now.Year}{now.Month}{now.Day} {now.Hour}{now.Minute}{now.Second}";
                string ErrorFileName = $"{nowDateTime}-ErrorLog.txt";

                DirectoryInfo di = new DirectoryInfo(LogPath);
                if (di.Exists == false) di.Create();

                using (StreamWriter sw = new StreamWriter(new FileStream(Path.Combine(LogPath, ErrorFileName), FileMode.Create, FileAccess.ReadWrite)))
                    sw.Write(error);
            }
            catch (Exception)
            {
                activity.RunOnUiThread(() => { Toast.MakeText(activity, "Error Write Log", ToastLength.Long).Show(); });
            }
        }

        internal static void LogError(string error)
        {
            try
            {
                DateTime now = DateTime.Now;

                string nowDateTime = $"{now.Year}{now.Month}{now.Day} {now.Hour}{now.Minute}{now.Second}";
                string ErrorFileName = $"{nowDateTime}-ErrorLog.txt";

                DirectoryInfo di = new DirectoryInfo(LogPath);
                if (di.Exists == false) di.Create();

                using (StreamWriter sw = new StreamWriter(new FileStream(Path.Combine(LogPath, ErrorFileName), FileMode.Create, FileAccess.ReadWrite)))
                    sw.Write(error);
            }
            catch (Exception)
            {

            }
        }

    }
}