using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace TSGPDic
{
    [Activity(Label = "MusicDetail", Theme = "@style/TSGP.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MusicDetail : AppCompatActivity
    {
        private LinearLayout MainLayout;
        private ScrollView MainScrollLayout;

        private Music music;
        private DataRow MusicInfoDR = null;

        private MediaPlayer mp = null;

        private ImageView BigAlbumView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.MusicDetailLayout);

            MusicInfoDR = ETC.FindDataRow(ETC.MusicList, "CodeName", Intent.GetStringExtra("CodeName"));
            music = new Music(MusicInfoDR);

            MainLayout = FindViewById<LinearLayout>(Resource.Id.MusicDetailMainLayout);
            MainScrollLayout = FindViewById<ScrollView>(Resource.Id.MusicDetailMainScrollLayout);

            switch (music.Type)
            {
                case string type when type == Resources.GetString(Resource.String.MusicType_Vocal):
                    MainScrollLayout.SetBackgroundResource(Resource.Drawable.VocalMusic_BG);
                    break;
                case string type when type == Resources.GetString(Resource.String.MusicType_Dance):
                    MainScrollLayout.SetBackgroundResource(Resource.Drawable.DanceMusic_BG);
                    break;
                case string type when type == Resources.GetString(Resource.String.MusicType_Session):
                    MainScrollLayout.SetBackgroundResource(Resource.Drawable.SessionMusic_BG);
                    break;
            }

            BigAlbumView = FindViewById<ImageView>(Resource.Id.MusicDetailBigAlbumView);
            FindViewById<Button>(Resource.Id.MusicDetailMissionListButton).Click += MusicMissonListButton_Click;
            FindViewById<Button>(Resource.Id.MusicDetailMusicPreviewButton).Click += MusicPreviewButton_Click;

            InitLoadProcess();
        }

        private void MusicPreviewButton_Click(object sender, EventArgs e)
        {
            Button bt = sender as Button;
            bt.Enabled = false;

            try
            {
                if (mp == null) mp = new MediaPlayer();
                string preview_path = Path.Combine(ETC.CachePath, "Music", "Preview", $"{music.CodeName}.tsgp");

                try
                {
                    mp.SetDataSource(preview_path);
                }
                catch (Exception)
                {
                    if (ETC.IsServerDown == false)
                    {
                        try
                        {
                            using (WebClient wc = new WebClient())
                                wc.DownloadFile(Path.Combine(ETC.Server, "Resource", "Music", "Preview", $"{music.CodeName}.opus"), preview_path);

                            mp.SetDataSource(preview_path);
                        }
                        catch (Exception ex)
                        {
                            ETC.LogError(this, ex.ToString());
                            Toast.MakeText(this, $"Preview 음원 {Resources.GetString(Resource.String.Download_Fail)}", ToastLength.Short).Show();
                            return;
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, Resource.String.Server_Down_Error, ToastLength.Short).Show();
                        return;
                    }
                }
                finally
                {
                    
                }

                mp.Completion += delegate 
                {
                    bt.Enabled = true;
                    mp.Release();
                    mp.Dispose();
                };
                
                mp.Prepare();
                mp.Start();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void MusicMissonListButton_Click(object sender, EventArgs e)
        {
            Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this);
            ad.SetTitle(Resource.String.Music_MissionButton);
            ad.SetView(SetMissionsList());
            ad.SetPositiveButton("확인", delegate { });
            ad.Show();
        }

        private async Task InitLoadProcess()
        {
            try
            {
                try
                {
                    string album_path = Path.Combine(ETC.CachePath, "Music", "Album", $"{music.CodeName}.tsgp");
                    if (File.Exists(album_path) == false)
                    {
                        if (ETC.IsServerDown == false)
                        {
                            using (WebClient wc = new WebClient())
                                await wc.DownloadFileTaskAsync(Path.Combine(ETC.Server, "Resource", "Images", "Music", "Album_Big", $"{music.CodeName}_big.png"), album_path);

                            BigAlbumView.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(album_path));
                        }
                        else BigAlbumView.SetImageResource(Resource.Drawable.No_Album_Big);
                    }
                    else BigAlbumView.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(album_path));
                }
                catch (Exception ex)
                {
                    ETC.LogError(this, ex.ToString());
                }

                FindViewById<TextView>(Resource.Id.MusicDetailMusicName).Text = music.Name;
                FindViewById<TextView>(Resource.Id.MusicDetailMusicArtist).Text = music.Artist;
                FindViewById<TextView>(Resource.Id.MusicDetailMusicBPM).Text = $"BPM\n{music.BPM}";
                FindViewById<TextView>(Resource.Id.MusicDetailMusicAddVersion).Text = $"최초수록\n{music.AddVersion_Full}";

                int[] LevelIcon_Id =
                {
                    Resource.Id.MusicDetailLevel_Easy,
                    Resource.Id.MusicDetailLevel_Normal,
                    Resource.Id.MusicDetailLevel_Hard,
                    Resource.Id.MusicDetailLevel_Expert
                };

                for (int i = 0; i < LevelIcon_Id.Length; ++i)
                {
                    int id = 0;
                    switch (music.Level[i])
                    {
                        case 1:
                            id = Resource.Drawable.Number_1;
                            break;
                        case 2:
                            id = Resource.Drawable.Number_2;
                            break;
                        case 3:
                            id = Resource.Drawable.Number_3;
                            break;
                        case 4:
                            id = Resource.Drawable.Number_4;
                            break;
                        case 5:
                            id = Resource.Drawable.Number_5;
                            break;
                        case 6:
                            id = Resource.Drawable.Number_6;
                            break;
                        case 7:
                            id = Resource.Drawable.Number_7;
                            break;
                        case 8:
                            id = Resource.Drawable.Number_8;
                            break;
                        case 9:
                            id = Resource.Drawable.Number_9;
                            break;
                        case 10:
                            id = Resource.Drawable.Number_10;
                            break;
                    }
                    FindViewById<ImageView>(LevelIcon_Id[i]).SetImageResource(id);
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private View SetMissionsList()
        {
            ScrollView MissionMainScrollLayout = new ScrollView(this);
            MissionMainScrollLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            LinearLayout MissionMainLayout = new LinearLayout(this);
            MissionMainLayout.Orientation = Android.Widget.Orientation.Vertical;
            MissionMainLayout.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            MissionMainLayout.SetGravity(GravityFlags.Center);

            for (int i = 0; i < 15; ++i)
            {
                string[] mission_reward = music.Missions[i].Split(',');
                string mission = $"M{i + 1}. ";
                string reward = mission_reward[mission_reward.Length - 1];

                switch (i)
                {
                    case 0:
                        mission += "최초 클리어";
                        break;
                    case 1:
                        mission += $"랭크 {mission_reward[0]}";
                        break;
                    case 2:
                        mission += $"올 콤보 {mission_reward[0]}회";
                        break;
                    case 3:
                        mission += $"PERFECT+ 판정 {mission_reward[0]}% 이상";
                        break;
                    case 4:
                        mission += $"올 퍼펙트 {mission_reward[0]}회";
                        break;
                    case 5:
                        mission += $"클리어 {mission_reward[0]}회";
                        break;
                    case 6:
                        mission += $"하이 스코어 {mission_reward[0]}회";
                        break;
                    case 7:
                        mission += $"{mission_reward[0]}점 이상";
                        break;
                    case 8:
                        mission += $"{mission_reward[0]}미스 이상";
                        break;
                    case 9:
                        mission += $"{mission_reward[0]}미스 이하";
                        break;
                    case 10:
                        mission += $"VOCAL 스타 {mission_reward[0]}명 이상";
                        break;
                    case 11:
                        mission += $"DANCER 스타 {mission_reward[0]}명 이상";
                        break;
                    case 12:
                        mission += $"SESSION 스타 {mission_reward[0]}명 이상";
                        break;
                    case 13:
                        mission += $"{mission_reward[0]}개 이상";
                        break;
                    case 14:
                        mission += $"{mission_reward[0]}명 포함";
                        break;
                }

                LinearLayout MissionLayout = new LinearLayout(this);
                MissionLayout.Orientation = Android.Widget.Orientation.Vertical;
                MissionLayout.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    BottomMargin = 10
                };
                MissionLayout.SetGravity(GravityFlags.Center);

                TextView Mission = new TextView(this);
                Mission.Text = mission;
                Mission.Gravity = GravityFlags.Center;
                Mission.Typeface = Android.Graphics.Typeface.DefaultBold;

                MissionLayout.AddView(Mission);

                TextView Reward = new TextView(this);
                Reward.Text = reward;
                Reward.SetTextColor(Android.Graphics.Color.ParseColor("#6B6B6B"));
                Reward.Gravity = GravityFlags.Center;

                MissionLayout.AddView(Reward);

                MissionMainLayout.AddView(MissionLayout);
            }

            MissionMainScrollLayout.AddView(MissionMainLayout);

            return MissionMainScrollLayout;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            GC.Collect();
            OverridePendingTransition(Resource.Animation.Activity_SlideInLeft, Resource.Animation.Activity_SlideOutRight);
        }
    }
}