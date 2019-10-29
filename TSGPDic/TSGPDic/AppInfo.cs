using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace TSGPDic
{
    [Activity(Label = "AppInfo", Theme = "@style/TSGP.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class AppInfo : AppCompatActivity
    {
        Button UpdateButton;
        TextView NowVersion;
        TextView ServerVersion;

        FloatingActionButton DiscordFAB;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.AppInfoLayout);

            UpdateButton = FindViewById<Button>(Resource.Id.AppInfoAppUpdateButton);
            UpdateButton.Click += UpdateButton_Click;
            NowVersion = FindViewById<TextView>(Resource.Id.AppInfoNowAppVersion);
            ServerVersion = FindViewById<TextView>(Resource.Id.AppInfoServerAppVersion);

            DiscordFAB = FindViewById<FloatingActionButton>(Resource.Id.DiscordFAB);
            DiscordFAB.Click += HelpFAB_Click;

            StringBuilder sb = new StringBuilder();

            sb.Append("Developer : URK96\n");
            sb.Append("E-mail : chlwlsgur96@hotmail.com");

            FindViewById<TextView>(Resource.Id.AppInfoDeveloperInfo).Text = sb.ToString();

            CheckAppVersion();
        }

        private void HelpFAB_Click(object sender, EventArgs e)
        {
            try
            {
                FloatingActionButton fab = sender as FloatingActionButton;

                Intent intent = null;
                string url = "";

                switch (fab.Id)
                {
                    case Resource.Id.DiscordFAB:
                        url = "https://discord.gg/5cJ5WQD";
                        break;
                }

                intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            string AppPackageName = PackageName;

            try
            {
                string url = string.Format("market://details?id={0}", PackageName);
                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
            }
            catch (Exception)
            {
                string url = string.Format("https://play.google.com/store/apps/details?id={0}", PackageName);
                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(url)));
            }
        }

        private async Task CheckAppVersion()
        {
            bool HasUpdate = false;
            await Task.Delay(100);

            try
            {
                var context = ApplicationContext;
                string[] now_ver = context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName.Split('.');
                string[] server_ver = new string[now_ver.Length];

                NowVersion.Text = $"{Resources.GetString(Resource.String.AppInfo_NowAppVersion)} : {context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName} Alpha - ";
#if DEBUG
                NowVersion.Text += "Debug";
#else
                NowVersion.Text += "Release";
#endif
                string url = Path.Combine(ETC.Server, "AppVer.txt");
                string target = Path.Combine(ETC.tempPath, "AppVer.txt");

                using (WebClient wc = new WebClient())
                    await wc.DownloadFileTaskAsync(url, target);

                using (StreamReader sr = new StreamReader(new FileStream(target, FileMode.Open, FileAccess.Read)))
                    server_ver = (sr.ReadToEnd()).Split('.');

                for (int i = 0; i < server_ver.Length; ++i)
                {
                    if (int.Parse(now_ver[i]) < int.Parse(server_ver[i])) HasUpdate = true;
                    else if (int.Parse(now_ver[i]) == int.Parse(server_ver[i])) continue;
                    else
                    {
                        HasUpdate = false;
                        break;
                    }
                }

                if (HasUpdate == true)
                {
                    UpdateButton.Visibility = ViewStates.Visible;
                    ServerVersion.Text = string.Format("{0} : {1}.{2}.{3}", Resources.GetString(Resource.String.AppInfo_NewVersion), server_ver[0], server_ver[1], server_ver[2]);
                }
                else
                {
                    ServerVersion.Text = Resources.GetString(Resource.String.AppInf_LatestUpdateVersion);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                ServerVersion.Text = Resources.GetString(Resource.String.Update_CheckFail);
            }
        }

    }
}