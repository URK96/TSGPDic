using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace TSGPDic
{
    [Activity(Label = "Dic_Music", Theme = "@style/TSGP.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class Dic_MusicMain : AppCompatActivity
    {
        private enum LineUp { Name, Update }

        delegate void DownloadProgress();

        private List<Music> RootList = new List<Music>();
        private List<Music> SubList = new List<Music>();
        private List<string> Download_List = new List<string>();

        private bool[] MusicTypeFilter = { true, true, true };

        int p_now = 0;
        int p_total = 0;

        private LineUp LineUpStyle = LineUp.Name;

        private RecyclerView MusicRecyclerView;
        private RecyclerView.LayoutManager MainLayoutManager;
        private ImageButton SearchResetButton;
        private EditText SearchText;

        private Dialog dialog;
        private ProgressBar totalProgressBar;
        private ProgressBar nowProgressBar;
        private TextView totalProgress;
        private TextView nowProgress;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Dic_MusicMainLayout);

            Display display = WindowManager.DefaultDisplay;
            Android.Graphics.Point point = new Android.Graphics.Point();
            display.GetSize(point);

            MusicRecyclerView = FindViewById<RecyclerView>(Resource.Id.MusicDicRecyclerView);
            MainLayoutManager = new GridLayoutManager(this, point.X / 200);
            MusicRecyclerView.SetLayoutManager(MainLayoutManager);
            SearchResetButton = FindViewById<ImageButton>(Resource.Id.MusicSearchResetButton);
            SearchText = FindViewById<EditText>(Resource.Id.MusicSearchText);
            SearchText.TextChanged += SearchText_TextChanged;

            FindViewById<LinearLayout>(Resource.Id.MusicDicLineUpSelector_Name).Click += LineUpSelector_Click;
            FindViewById<LinearLayout>(Resource.Id.MusicDicLineUpSelector_Update).Click += LineUpSelector_Click;
            FindViewById<LinearLayout>(Resource.Id.MusicDicMusicTypeSelector_Vocal).Click += MusicTypeSelector_Click;
            FindViewById<LinearLayout>(Resource.Id.MusicDicMusicTypeSelector_Dance).Click += MusicTypeSelector_Click;
            FindViewById<LinearLayout>(Resource.Id.MusicDicMusicTypeSelector_Session).Click += MusicTypeSelector_Click;

            InitLoad();

            ListMusic("");
        }

        private void MusicTypeSelector_Click(object sender, EventArgs e)
        {
            LinearLayout layout = sender as LinearLayout;

            try
            {
                switch (layout.Id)
                {
                    case Resource.Id.MusicDicMusicTypeSelector_Vocal:
                        MusicTypeFilter[0] = !MusicTypeFilter[0];
                        ImageView iv_vocal = FindViewById<ImageView>(Resource.Id.MusicDicMusicTypeIndicator_Vocal);
                        if (MusicTypeFilter[0] == true) iv_vocal.Visibility = ViewStates.Visible;
                        else iv_vocal.Visibility = ViewStates.Invisible;
                        break;
                    case Resource.Id.MusicDicMusicTypeSelector_Dance:
                        MusicTypeFilter[1] = !MusicTypeFilter[1];
                        ImageView iv_dance = FindViewById<ImageView>(Resource.Id.MusicDicMusicTypeIndicator_Dance);
                        if (MusicTypeFilter[1] == true) iv_dance.Visibility = ViewStates.Visible;
                        else iv_dance.Visibility = ViewStates.Invisible;
                        break;
                    case Resource.Id.MusicDicMusicTypeSelector_Session:
                        MusicTypeFilter[2] = !MusicTypeFilter[2];
                        ImageView iv_session = FindViewById<ImageView>(Resource.Id.MusicDicMusicTypeIndicator_Session);
                        if (MusicTypeFilter[2] == true) iv_session.Visibility = ViewStates.Visible;
                        else iv_session.Visibility = ViewStates.Invisible;
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }

            ListMusic(SearchText.Text);
        }

        private void LineUpSelector_Click(object sender, EventArgs e)
        {
            LinearLayout layout = sender as LinearLayout;

            try
            {
                ImageView iv_name = FindViewById<ImageView>(Resource.Id.MusicDicLineUpIndicator_Name);
                ImageView iv_update = FindViewById<ImageView>(Resource.Id.MusicDicLineUpIndicator_Update);

                switch (layout.Id)
                {
                    case Resource.Id.MusicDicLineUpSelector_Name:
                        LineUpStyle = LineUp.Name;
                        iv_name.Visibility = ViewStates.Visible;
                        iv_update.Visibility = ViewStates.Invisible;
                        break;
                    case Resource.Id.MusicDicLineUpSelector_Update:
                        LineUpStyle = LineUp.Update;
                        iv_name.Visibility = ViewStates.Invisible;
                        iv_update.Visibility = ViewStates.Visible;
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }

            ListMusic(SearchText.Text);
        }

        private void InitLoad()
        {
            CreateListObject();

            if (CheckCropImage() == true)
            {
                Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG);
                ad.SetTitle(Resource.String.MusicDic_CropImageDownloadTitle);
                ad.SetMessage(Resource.String.MusicDic_CropImageDownloadQuestion);
                ad.SetCancelable(true);
                ad.SetPositiveButton(Resource.String.AlertDialog_Download, async delegate { await MusicCropImageDownloadProcess(); });
                ad.SetNegativeButton(Resource.String.AlertDialog_Cancel, delegate { });

                ad.Show();
            }
        }

        private bool CheckCropImage()
        {
            Download_List.Clear();

            for (int i = 0; i < RootList.Count; ++i)
            {
                Music music = RootList[i];
                string FilePath = Path.Combine(ETC.CachePath, "Music", "Album", "Crop", $"{music.CodeName}.tsgp");
                if (File.Exists(FilePath) == false) Download_List.Add(music.CodeName);
            }

            Download_List.TrimExcess();

            if (Download_List.Count == 0) return false;
            else return true;
        }

        private async Task MusicCropImageDownloadProcess()
        {
            View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            Android.Support.V7.App.AlertDialog.Builder pd = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG_Download);
            pd.SetTitle(Resource.String.MusicDic_CropImageDownloadTitle);
            pd.SetCancelable(false);
            pd.SetView(v);

            dialog = pd.Create();
            dialog.Show();

            try
            {
                totalProgressBar = v.FindViewById<ProgressBar>(Resource.Id.TotalProgressBar);
                totalProgress = v.FindViewById<TextView>(Resource.Id.TotalProgressPercentage);
                nowProgressBar = v.FindViewById<ProgressBar>(Resource.Id.NowProgressBar);
                nowProgress = v.FindViewById<TextView>(Resource.Id.NowProgressPercentage);

                p_now = 0;
                p_total = 0;
                p_total = Download_List.Count;
                totalProgressBar.Max = 100;
                totalProgressBar.Progress = 0;

                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += Wc_DownloadFileCompleted;

                    for (int i = 0; i < p_total; ++i)
                    {
                        string url = Path.Combine(ETC.Server, "Resource", "Images", "Music", "Album", $"{Download_List[i]}.png");
                        string target = Path.Combine(ETC.CachePath, "Music", "Album", "Crop", $"{Download_List[i]}.tsgp");
                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                await Task.Delay(500);

                ListMusic(SearchText.Text);
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
            finally
            {
                dialog.Dismiss();
                dialog = null;
                totalProgressBar = null;
                totalProgress = null;
                nowProgressBar = null;
                nowProgress = null;
            }
        }

        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            p_now += 1;

            totalProgressBar.Progress = Convert.ToInt32(p_now / Convert.ToDouble(p_total) * 100);
            totalProgress.Text = $"{totalProgressBar.Progress}%";
        }

        private void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            nowProgressBar.Progress = e.ProgressPercentage;
            nowProgress.Text = $"{e.ProgressPercentage}%";
        }


        private void CreateListObject()
        {
            try
            {
                foreach (DataRow dr in ETC.MusicList.Rows)
                    RootList.Add(new Music(dr));

                RootList.TrimExcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void SearchText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            ListMusic(SearchText.Text);
        }

        private async void ListMusic(string searchText)
        {
            SubList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < RootList.Count; ++i)
                {
                    Music music = RootList[i];

                    if (CheckFilter(music) == false) continue;

                    if (searchText != "")
                    {
                        string name = music.Name.ToUpper();

                        if (name.Contains(searchText) == false) continue;
                    }

                    SubList.Add(music);
                }

                SubList.Sort(SortMusic);

                var adapter = new MusicListAdapter(SubList, this);

                if (adapter.HasOnItemClick() == false) adapter.ItemClick += Adapter_ItemClick;

                await Task.Delay(100);

                RunOnUiThread(() => { MusicRecyclerView.SetAdapter(adapter); });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private bool CheckFilter(Music music)
        {
            try
            {
                switch (music.Type)
                {
                    case string type when type == Resources.GetString(Resource.String.MusicType_Vocal):
                        if (MusicTypeFilter[0] == false) return false;
                        break;
                    case string type when type == Resources.GetString(Resource.String.MusicType_Dance):
                        if (MusicTypeFilter[1] == false) return false;
                        break;
                    case string type when type == Resources.GetString(Resource.String.MusicType_Session):
                        if (MusicTypeFilter[2] == false) return false;
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }

            return true;
        }

        private async void Adapter_ItemClick(object sender, int position)
        {
            await Task.Delay(100);
            var MusicInfo = new Intent(this, typeof(MusicDetail));
            MusicInfo.PutExtra("CodeName", SubList[position].CodeName);
            StartActivity(MusicInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private int SortMusic(Music x, Music y)
        {
            switch (LineUpStyle)
            {
                case LineUp.Update:
                    if ((x.UpdateDate == "Default") && (y.UpdateDate == "Default"))
                        return x.Name.CompareTo(y.Name);
                    else if (x.UpdateDate == "Default")
                        return -1;
                    else if (y.UpdateDate == "Default")
                        return 1;
                    else
                    {
                        string[] x_date = x.UpdateDate.Split('-');
                        string[] y_date = y.UpdateDate.Split('-');

                        for (int i = 0; i < x_date.Length; ++i)
                        {
                            int x1 = int.Parse(x_date[i]);
                            int y1 = int.Parse(y_date[i]);

                            if (x1 < y1) return -1;
                            else if (x1 > y1) return 1;
                        }
                        return x.Name.CompareTo(y.Name);
                    }
                case LineUp.Name:
                default:
                    return x.Name.CompareTo(y.Name);
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Finish();
            OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
            GC.Collect();
        }
    }

    class MusicListViewHolder : RecyclerView.ViewHolder
    {
        public ImageView AlbumImage { get; private set; }
        public TextView Name { get; private set; }
        public TextView Artist { get; private set; }
        
        public MusicListViewHolder(View view, Action<int> listener) : base(view)
        {
            Name = view.FindViewById<TextView>(Resource.Id.MusicListMusicName);
            Artist = view.FindViewById<TextView>(Resource.Id.MusicListMusicArtist);
            AlbumImage = view.FindViewById<ImageView>(Resource.Id.MusicListAlbumImage);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    class MusicListAdapter : RecyclerView.Adapter
    {
        List<Music> items;
        Activity context;

        public event EventHandler<int> ItemClick;

        public MusicListAdapter(List<Music> items, Activity context)
        {
            this.items = items;
            this.context = context;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.MusicListLayout, parent, false);

            MusicListViewHolder vh = new MusicListViewHolder(view, OnClick);
            return vh;
        }

        public override int ItemCount
        {
            get { return items.Count; }
        }

        void OnClick(int position)
        {
            if (ItemClick != null)
            {
                ItemClick(this, position);
            }
        }

        public bool HasOnItemClick()
        {
            if (ItemClick == null) return false;
            else return true;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            MusicListViewHolder vh = holder as MusicListViewHolder;

            var item = items[position];

            try
            {
                string AlbumImagePath = Path.Combine(ETC.CachePath, "Music", "Album", "Crop", $"{item.CodeName}.tsgp");
                if (File.Exists(AlbumImagePath) == true)
                    vh.AlbumImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(AlbumImagePath));

                vh.Name.Text = item.Name;
                vh.Artist.Text = item.Artist;
            }
            catch (Exception ex)
            {
                ETC.LogError(context, ex.ToString());
                Toast.MakeText(context, "Error Create View", ToastLength.Short).Show();
            }
        }
    }

}