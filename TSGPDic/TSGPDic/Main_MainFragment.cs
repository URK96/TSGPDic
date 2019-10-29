using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using PL.DroidsOnRoids.Gif;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace TSGPDic
{
    public class Main_MainFragment : Android.Support.V4.App.Fragment
    {
        private View v;

        private FrameLayout LoadingLayout;
        private TextView NotificationView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            v = inflater.Inflate(Resource.Layout.Main_MainLayout, container, false);

            LoadingLayout = v.FindViewById<FrameLayout>(Resource.Id.LoadingMainLayout);
            NotificationView = v.FindViewById<TextView>(Resource.Id.Main_MainNotification);

            if (string.IsNullOrWhiteSpace(ETC.Notification_String) == true)
                LoadNotification();
            else NotificationView.Text = ETC.Notification_String;

            return v;
        }

        private async Task LoadNotification()
        {
            try
            {
                LoadingLayout.Visibility = ViewStates.Visible;

                await Task.Delay(500);

                using (WebClient wc = new WebClient())
                    ETC.Notification_String = await wc.DownloadStringTaskAsync(Path.Combine(ETC.Server, "Notification.txt"));
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
                ETC.Notification_String = "Loading Fail...";
            }
            finally
            {
                NotificationView.Text = ETC.Notification_String;
                LoadingLayout.Visibility = ViewStates.Gone;
            }
        }
    }
}