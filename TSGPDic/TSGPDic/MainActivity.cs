using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Support.V4.View;
using System;
using Android.Support.Design.Widget;

namespace TSGPDic
{
    [Activity(Label = "Akasic Record Main", Theme = "@style/TSGP.NoActionBar")]
    public class MainActivity : AppCompatActivity
    {
        private DrawerLayout MainDrawerLayout;
        private NavigationView MainNavigationView;

        private Android.Support.V4.App.Fragment Main_F;
        private Android.Support.V4.App.Fragment Dic_F;

        private Android.Support.V4.App.FragmentTransaction ft;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.MainLayout);

            SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.MainToolbar));
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.MenuDrawer);

            MainDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.MainDrawerLayout);
            MainNavigationView = FindViewById<NavigationView>(Resource.Id.MainNavigationView);
            MainNavigationView.NavigationItemSelected += MainNavigationView_NavigationItemSelected;

            ft = SupportFragmentManager.BeginTransaction();

            Main_F = new Main_MainFragment();
            Dic_F = new Main_DicFragment();

            ft.Add(Resource.Id.MainFragmentContainer, Main_F, "Main");

            ft.Commit();
        }

        private void MainNavigationView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            try
            {
                ft = SupportFragmentManager.BeginTransaction();

                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.Main_Main:
                        ft.Replace(Resource.Id.MainFragmentContainer, Main_F, "Main");
                        break;
                    case Resource.Id.Main_Dic:
                        ft.Replace(Resource.Id.MainFragmentContainer, Dic_F, "Dic");
                        break;
                    case Resource.Id.Main_AppInfo:
                        StartActivity(typeof(AppInfo));
                        OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.Main_Setting:
                        break;
                }

                ft.Commit();

                MainDrawerLayout.CloseDrawer(GravityCompat.Start);
            }
            catch (Exception ex)
            {
                ETC.LogError(ex.ToString());
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (MainDrawerLayout.IsDrawerOpen(GravityCompat.Start) == false)
                        MainDrawerLayout.OpenDrawer(GravityCompat.Start);
                    else MainDrawerLayout.CloseDrawer(GravityCompat.Start);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnBackPressed()
        {
            if (MainDrawerLayout.IsDrawerOpen(GravityCompat.Start) == true)
                MainDrawerLayout.CloseDrawer(GravityCompat.Start);
            else
            {
                base.OnBackPressed();
                OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
                GC.Collect();
            }
        }
    }
}