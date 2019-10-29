using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using System.Data;
using System.Net;
using System.Threading.Tasks;

namespace TSGPDic
{
    [Activity(Label = "Dic_StarMain")]
    public class Dic_StarMain : AppCompatActivity
    {
        private enum LineUp { Name, DicNumber }

        delegate void DownloadProgress();

        private List<Star> RootList = new List<Star>();
        private List<Star> SubList = new List<Star>();
        private List<int> Download_List = new List<int>();

        private bool[] StarAttributeFilter = { true, true, true, true };
        private bool[] StarTypeFilter = { true, true, true, true, true };

        int p_now = 0;
        int p_total = 0;

        private LineUp LineUpStyle = LineUp.Name;

        private RecyclerView StarRecyclerView;
        private RecyclerView.LayoutManager MainLayoutManager;
        private ImageButton SearchResetButton;
        private EditText SearchText;

        private ImageView TypeSelector_Tap;
        private ImageView TypeSelector_Long;
        private ImageView TypeSelector_Slide;
        private ImageView TypeSelector_Flick;
        private ImageView TypeSelector_Support;
        private ImageView TypeSelector_Special;

        private Dialog dialog;
        private ProgressBar totalProgressBar;
        private ProgressBar nowProgressBar;
        private TextView totalProgress;
        private TextView nowProgress;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Dic_StarMainLayout);

            Display display = WindowManager.DefaultDisplay;
            Android.Graphics.Point point = new Android.Graphics.Point();
            display.GetSize(point);

            StarRecyclerView = FindViewById<RecyclerView>(Resource.Id.StarDicRecyclerView);
            MainLayoutManager = new GridLayoutManager(this, point.X / 200);
            StarRecyclerView.SetLayoutManager(MainLayoutManager);
            SearchResetButton = FindViewById<ImageButton>(Resource.Id.StarSearchResetButton);
            SearchText = FindViewById<EditText>(Resource.Id.StarSearchText);
            SearchText.TextChanged += SearchText_TextChanged;

            FindViewById<LinearLayout>(Resource.Id.StarDicLineUpSelector_Name).Click += LineUpSelector_Click;
            FindViewById<LinearLayout>(Resource.Id.StarDicLineUpSelector_DicNumber).Click += LineUpSelector_Click;
            FindViewById<LinearLayout>(Resource.Id.StarDicStarAttributeSelector_Vocal).Click += StarAttributeSelector_Click;
            FindViewById<LinearLayout>(Resource.Id.StarDicStarAttributeSelector_Dance).Click += StarAttributeSelector_Click;
            FindViewById<LinearLayout>(Resource.Id.StarDicStarAttributeSelector_Session).Click += StarAttributeSelector_Click;
            FindViewById<LinearLayout>(Resource.Id.StarDicStarAttributeSelector_Support).Click += StarAttributeSelector_Click;

            TypeSelector_Tap = FindViewById<ImageView>(Resource.Id.StarDicStarTypeIndicator_Tap);
            TypeSelector_Tap.Click += TypeSelector_Click;
            TypeSelector_Long = FindViewById<ImageView>(Resource.Id.StarDicStarTypeIndicator_Long);
            TypeSelector_Slide = FindViewById<ImageView>(Resource.Id.StarDicStarTypeIndicator_Slide);
            TypeSelector_Flick = FindViewById<ImageView>(Resource.Id.StarDicStarTypeIndicator_Flick);
            TypeSelector_Support = FindViewById<ImageView>(Resource.Id.StarDicStarTypeIndicator_Support);
            TypeSelector_Special = FindViewById<ImageView>(Resource.Id.StarDicStarTypeIndicator_Special);

            InitLoad();

            ListStar("");
        }

        private void TypeSelector_Click(object sender, EventArgs e)
        {
            const float enable = 1.0f;
            const float disable = 0.3f;
            ImageView iv = sender as ImageView;

            try
            {
                switch (iv.Id)
                {
                    case Resource.Id.StarDicStarTypeIndicator_Tap:
                        StarTypeFilter[0] = !StarTypeFilter[0];
                        if (StarTypeFilter[0] == true) iv.Alpha = enable;
                        else iv.Alpha = disable;
                        break;
                    case Resource.Id.StarDicStarTypeIndicator_Long:
                        StarTypeFilter[1] = !StarTypeFilter[1];
                        if (StarTypeFilter[1] == true) iv.Alpha = enable;
                        else iv.Alpha = disable;
                        break;
                    case Resource.Id.StarDicStarTypeIndicator_Slide:
                        StarTypeFilter[2] = !StarTypeFilter[2];
                        if (StarTypeFilter[2] == true) iv.Alpha = enable;
                        else iv.Alpha = disable;
                        break;
                    case Resource.Id.StarDicStarTypeIndicator_Flick:
                        StarTypeFilter[3] = !StarTypeFilter[3];
                        if (StarTypeFilter[3] == true) iv.Alpha = enable;
                        else iv.Alpha = disable;
                        break;
                    case Resource.Id.StarDicStarTypeIndicator_Support:
                        StarTypeFilter[4] = !StarTypeFilter[4];
                        if (StarTypeFilter[4] == true) iv.Alpha = enable;
                        else iv.Alpha = disable;
                        break;
                    case Resource.Id.StarDicStarTypeIndicator_Special:
                        StarTypeFilter[5] = !StarTypeFilter[5];
                        if (StarTypeFilter[5] == true) iv.Alpha = enable;
                        else iv.Alpha = disable;
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }

            ListStar(SearchText.Text);
        }

        private void StarAttributeSelector_Click(object sender, EventArgs e)
        {
            LinearLayout layout = sender as LinearLayout;

            try
            {
                switch (layout.Id)
                {
                    case Resource.Id.StarDicStarAttributeSelector_Vocal:
                        StarAttributeFilter[0] = !StarAttributeFilter[0];
                        ImageView iv_vocal = FindViewById<ImageView>(Resource.Id.StarDicStarAttributeIndicator_Vocal);
                        if (StarAttributeFilter[0] == true) iv_vocal.Visibility = ViewStates.Visible;
                        else iv_vocal.Visibility = ViewStates.Invisible;
                        break;
                    case Resource.Id.StarDicStarAttributeSelector_Dance:
                        StarAttributeFilter[1] = !StarAttributeFilter[1];
                        ImageView iv_dance = FindViewById<ImageView>(Resource.Id.StarDicStarAttributeIndicator_Dance);
                        if (StarAttributeFilter[1] == true) iv_dance.Visibility = ViewStates.Visible;
                        else iv_dance.Visibility = ViewStates.Invisible;
                        break;
                    case Resource.Id.StarDicStarAttributeSelector_Session:
                        StarAttributeFilter[2] = !StarAttributeFilter[2];
                        ImageView iv_session = FindViewById<ImageView>(Resource.Id.StarDicStarAttributeIndicator_Session);
                        if (StarAttributeFilter[2] == true) iv_session.Visibility = ViewStates.Visible;
                        else iv_session.Visibility = ViewStates.Invisible;
                        break;
                    case Resource.Id.StarDicStarAttributeSelector_Support:
                        StarAttributeFilter[3] = !StarAttributeFilter[3];
                        ImageView iv_support = FindViewById<ImageView>(Resource.Id.StarDicStarAttributeIndicator_Support);
                        if (StarAttributeFilter[3] == true) iv_support.Visibility = ViewStates.Visible;
                        else iv_support.Visibility = ViewStates.Invisible;
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }

            ListStar(SearchText.Text);
        }

        private void LineUpSelector_Click(object sender, EventArgs e)
        {
            LinearLayout layout = sender as LinearLayout;

            try
            {
                ImageView iv_name = FindViewById<ImageView>(Resource.Id.StarDicLineUpIndicator_Name);
                ImageView iv_dicnumber = FindViewById<ImageView>(Resource.Id.StarDicLineUpIndicator_DicNumber);

                switch (layout.Id)
                {
                    case Resource.Id.StarDicLineUpSelector_Name:
                        LineUpStyle = LineUp.Name;
                        iv_name.Visibility = ViewStates.Visible;
                        iv_dicnumber.Visibility = ViewStates.Invisible;
                        break;
                    case Resource.Id.StarDicLineUpSelector_DicNumber:
                        LineUpStyle = LineUp.DicNumber;
                        iv_name.Visibility = ViewStates.Invisible;
                        iv_dicnumber.Visibility = ViewStates.Visible;
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }

            ListStar(SearchText.Text);
        }

        private void InitLoad()
        {
            CreateListObject();

            if (CheckCropImage() == true)
            {
                Android.Support.V7.App.AlertDialog.Builder ad = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG);
                ad.SetTitle(Resource.String.StarDic_CropImageDownloadTitle);
                ad.SetMessage(Resource.String.StarDic_CropImageDownloadQuestion);
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
                Star star = RootList[i];
                string FilePath = Path.Combine(ETC.CachePath, "Character", "Crop", "Normal", $"{star.DicNumber}.tsgp");
                if (File.Exists(FilePath) == false) Download_List.Add(star.DicNumber);
            }

            Download_List.TrimExcess();

            if (Download_List.Count == 0) return false;
            else return true;
        }

        private async Task MusicCropImageDownloadProcess()
        {
            View v = LayoutInflater.Inflate(Resource.Layout.ProgressDialogLayout, null);

            Android.Support.V7.App.AlertDialog.Builder pd = new Android.Support.V7.App.AlertDialog.Builder(this, ETC.DialogBG_Download);
            pd.SetTitle(Resource.String.StarDic_CropImageDownloadTitle);
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
                        string url = Path.Combine(ETC.Server, "Resource", "Images", "Character", "Crop", "Normal", $"{Download_List[i]}.png");
                        string target = Path.Combine(ETC.CachePath, "Character", "Crop", "Normal", $"{Download_List[i]}.tsgp");
                        await wc.DownloadFileTaskAsync(url, target);
                    }
                }

                await Task.Delay(500);

                ListStar(SearchText.Text);
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
                foreach (DataRow dr in ETC.CharacterList.Rows)
                    RootList.Add(new Star(dr));

                RootList.TrimExcess();
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private void SearchText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            ListStar(SearchText.Text);
        }

        private async void ListStar(string searchText)
        {
            SubList.Clear();

            searchText = searchText.ToUpper();

            try
            {
                for (int i = 0; i < RootList.Count; ++i)
                {
                    Star star = RootList[i];

                    if (CheckFilter(star) == false) continue;

                    if (searchText != "")
                    {
                        string name = star.Name.ToUpper();

                        if (name.Contains(searchText) == false) continue;
                    }

                    SubList.Add(star);
                }

                SubList.Sort(SortMusic);

                var adapter = new StarListAdapter(SubList, this);

                if (adapter.HasOnItemClick() == false) adapter.ItemClick += Adapter_ItemClick;

                await Task.Delay(100);

                RunOnUiThread(() => { StarRecyclerView.SetAdapter(adapter); });
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }

        private bool CheckFilter(Star star)
        {
            try
            {
                switch (star.Attribute)
                {
                    case string attribute when attribute == Resources.GetString(Resource.String.StarAttribute_Vocal):
                        if (StarAttributeFilter[0] == false) return false;
                        break;
                    case string attribute when attribute == Resources.GetString(Resource.String.StarAttribute_Dance):
                        if (StarAttributeFilter[1] == false) return false;
                        break;
                    case string attribute when attribute == Resources.GetString(Resource.String.StarAttribute_Session):
                        if (StarAttributeFilter[2] == false) return false;
                        break;
                    case string attribute when attribute == Resources.GetString(Resource.String.StarAttribute_Support):
                        if (StarAttributeFilter[3] == false) return false;
                        break;
                }

                switch (star.Type)
                {
                    case string type when type == Resources.GetString(Resource.String.StarType_Tap):
                        if (StarTypeFilter[0] == false) return false;
                        break;
                    case string type when type == Resources.GetString(Resource.String.StarType_Long):
                        if (StarTypeFilter[1] == false) return false;
                        break;
                    case string type when type == Resources.GetString(Resource.String.StarType_Slide):
                        if (StarTypeFilter[2] == false) return false;
                        break;
                    case string type when type == Resources.GetString(Resource.String.StarType_Flick):
                        if (StarTypeFilter[3] == false) return false;
                        break;
                    case string type when type == Resources.GetString(Resource.String.StarType_Support):
                        if (StarTypeFilter[4] == false) return false;
                        break;
                    case string type when type == Resources.GetString(Resource.String.StarType_Special):
                        if (StarTypeFilter[5] == false) return false;
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
            var StarInfo = new Intent(this, typeof(StarDetail));
            StarInfo.PutExtra("DicNumber", SubList[position].DicNumber);
            StartActivity(StarInfo);
            OverridePendingTransition(Resource.Animation.Activity_SlideInRight, Resource.Animation.Activity_SlideOutLeft);
        }

        private int SortMusic(Star x, Star y)
        {
            switch (LineUpStyle)
            {
                case LineUp.DicNumber:
                    return x.DicNumber.CompareTo(y.DicNumber);
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

    class StarListViewHolder : RecyclerView.ViewHolder
    {
        public ImageView StarCropImage { get; private set; }
        public ImageView StarBornGrade { get; private set; }
        public TextView Name { get; private set; }

        public StarListViewHolder(View view, Action<int> listener) : base(view)
        {
            StarCropImage = view.FindViewById<ImageView>(Resource.Id.StarListCropImage);
            StarBornGrade = view.FindViewById<ImageView>(Resource.Id.StarListStarBornGrade);
            Name = view.FindViewById<TextView>(Resource.Id.StarListStarName);

            view.Click += (sender, e) => listener(LayoutPosition);
        }
    }

    class StarListAdapter : RecyclerView.Adapter
    {
        List<Star> items;
        Activity context;

        public event EventHandler<int> ItemClick;

        public StarListAdapter(List<Star> items, Activity context)
        {
            this.items = items;
            this.context = context;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.StarListLayout, parent, false);

            StarListViewHolder vh = new StarListViewHolder(view, OnClick);
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
            StarListViewHolder vh = holder as StarListViewHolder;

            var item = items[position];

            try
            {
                string CropImagePath = Path.Combine(ETC.CachePath, "Character", "Crop", "Normal", $"{item.DicNumber}.tsgp");
                if (File.Exists(CropImagePath) == true)
                    vh.StarCropImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(CropImagePath));

                if (item.BornGrade != -1)
                {
                    int grade_id = 0;
                    switch (item.BornGrade)
                    {
                        case 1:
                            grade_id = Resource.Drawable.GradeStar_1;
                            break;
                        case 2:
                            grade_id = Resource.Drawable.GradeStar_2;
                            break;
                        case 3:
                            grade_id = Resource.Drawable.GradeStar_3;
                            break;
                        case 4:
                            grade_id = Resource.Drawable.GradeStar_4;
                            break;
                        case 5:
                            grade_id = Resource.Drawable.GradeStar_5;
                            break;
                        case 6:
                            grade_id = Resource.Drawable.GradeStar_6;
                            break;
                    }
                    vh.StarBornGrade.SetImageResource(grade_id);
                }

                vh.Name.Text = item.Name;
            }
            catch (Exception ex)
            {
                ETC.LogError(context, ex.ToString());
                Toast.MakeText(context, "Error Create View", ToastLength.Short).Show();
            }
        }
    }
}