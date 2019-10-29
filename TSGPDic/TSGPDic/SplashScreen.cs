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
using System.Threading.Tasks;
using PL.DroidsOnRoids.Gif;
using Android;
using System.Collections;
using Android.Content.PM;

namespace TSGPDic
{
    [Activity(Label = "TPSG Dic", Theme = "@style/TSGP.Splash", MainLauncher = true)]
    public class SplashScreen : AppCompatActivity
    {
        private GifImageView AniView;
        private TextView Status;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.SplashLayout);

            AniView = FindViewById<GifImageView>(Resource.Id.SplashLoadingAnimation);
            Status = FindViewById<TextView>(Resource.Id.SplashStatusText);

            if ((int.Parse(Build.VERSION.Release.Split('.')[0])) >= 6) CheckPermission();
            else InitLoad();
        }

        private async Task InitLoad()
        {
            try
            {
                Status.Text = "초기화 중";
                ETC.BasicInitializeApp(this);

                Status.Text = "서버 확인 중";
                await ETC.CheckServerNetwork();

                if (ETC.IsServerDown == false)
                {
                    Status.Text = "업데이트 확인 중";
                    if (await ETC.CheckDBVersion() == true)
                        await ETC.UpdateDB(this);
                }

                StartActivity(typeof(MainActivity));
                OverridePendingTransition(Android.Resource.Animation.SlideInLeft, Android.Resource.Animation.SlideOutRight);
                Finish();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void CheckPermission()
        {
            try
            {
                string[] check = { Manifest.Permission.WriteExternalStorage, Manifest.Permission.ReadExternalStorage, Manifest.Permission.Internet };
                ArrayList request = new ArrayList();

                foreach (string permission in check)
                    if (CheckSelfPermission(permission) == Permission.Denied) request.Add(permission);

                request.TrimToSize();

                if (request.Count == 0) InitLoad();
                else RequestPermissions((string[])request.ToArray(typeof(string)), 0);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
                Toast.MakeText(this, Resource.String.Permission_Error, ToastLength.Long).Show();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            //base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (grantResults[0] == Permission.Denied)
            {
                Toast.MakeText(this, Resource.String.PermissionDeny_Message, ToastLength.Short).Show();
                FinishAffinity();
            }
            else InitLoad();
        }

    }
}