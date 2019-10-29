using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using System;
using System.IO;

namespace TSGPDic
{
    public class Main_DicFragment : Android.Support.V4.App.Fragment
    {
        private View v;

        private CardView MusicDBView;
        private CardView CharacterDBView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            v = inflater.Inflate(Resource.Layout.Main_DicLayout, container, false);

            MusicDBView = v.FindViewById<CardView>(Resource.Id.Main_Dic_MusicDBCardView);
            MusicDBView.Click += CardView_Click;
            CharacterDBView = v.FindViewById<CardView>(Resource.Id.Main_Dic_CharacterDBCardView);
            CharacterDBView.Click += CardView_Click;

            return v;
        }

        private void CardView_Click(object sender, EventArgs e)
        {
            CardView cv = sender as CardView;

            try
            {
                switch (cv.Id)
                {
                    case Resource.Id.Main_Dic_MusicDBCardView:
                        ETC.LoadDBSync(ETC.MusicList, Path.Combine(ETC.DBPath, "Song.tsgp"), true);
                        Activity.StartActivity(typeof(Dic_MusicMain));
                        Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                    case Resource.Id.Main_Dic_CharacterDBCardView:
                        ETC.LoadDBSync(ETC.CharacterList, Path.Combine(ETC.DBPath, "Character.tsgp"), true);
                        Activity.StartActivity(typeof(Dic_StarMain));
                        Activity.OverridePendingTransition(Android.Resource.Animation.FadeIn, Android.Resource.Animation.FadeOut);
                        break;
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(Activity, ex.ToString());
            }
        }
    }
}