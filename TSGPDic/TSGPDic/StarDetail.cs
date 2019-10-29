using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using System.IO;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System.Net;

namespace TSGPDic
{
    [Activity(Label = "StarDetail", Theme = "@style/TSGP.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class StarDetail : AppCompatActivity
    {
        private Star star;

        private ImageView StarCropImage;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.StarDetailLayout);

            star = new Star(ETC.FindDataRow(ETC.CharacterList, "DicNumber", Intent.GetIntExtra("DicNumber", 0)));

            StarCropImage = FindViewById<ImageView>(Resource.Id.StarDetailStarCropImage);

            await DownloadProcess();
            InitLoadProcess();
        }

        private async Task DownloadProcess()
        {
            if (ETC.IsServerDown == true)
                return;

            string[] url =
            {
                Path.Combine(ETC.Server, "Resource", "Images", "Character", "Crop", "Normal", $"{star.DicNumber}.png"),
                Path.Combine(ETC.Server, "Resource", "Images", "Character", "Crop", "Awakening", $"{star.DicNumber}.png"),
                //Path.Combine(ETC.Server, "Resource", "Images", "Character", "Skill", $"{star.SkillCode}.tsgp")
            };
            string[] target =
            {
                Path.Combine(ETC.CachePath, "Character", "Crop", "Normal", $"{star.DicNumber}.tsgp"),
                Path.Combine(ETC.CachePath, "Character", "Crop", "Awakening", $"{star.DicNumber}.tsgp"),
                //Path.Combine(ETC.CachePath, "Character", "Skill", $"{star.SkillCode}.tsgp")
            };

            using (WebClient wc = new WebClient())
            {
                try
                {
                    for (int i = 0; i < url.Length; ++i)
                        await wc.DownloadFileTaskAsync(url[i], target[i]);
                }
                catch (Exception ex)
                {
                    ETC.LogError(this, ex.ToString());
                }
            }
        }

        private async Task InitLoadProcess()
        {
            try
            {
                try
                {
                    string crop_normal_path = Path.Combine(ETC.CachePath, "Character", "Crop", "Normal", $"{star.DicNumber}.tsgp");
                    if (File.Exists(crop_normal_path) == false)
                    {
                        StarCropImage.SetImageResource(Resource.Drawable.No_StarCrop);
                    }
                    else StarCropImage.SetImageDrawable(Android.Graphics.Drawables.Drawable.CreateFromPath(crop_normal_path));

                    int GradeId = 0;
                    switch (star.BornGrade)
                    {
                        default:
                        case 1:
                            GradeId = Resource.Drawable.GradeStar_1;
                            break;
                        case 2:
                            GradeId = Resource.Drawable.GradeStar_2;
                            break;
                        case 3:
                            GradeId = Resource.Drawable.GradeStar_3;
                            break;
                        case 4:
                            GradeId = Resource.Drawable.GradeStar_4;
                            break;
                        case 5:
                            GradeId = Resource.Drawable.GradeStar_5;
                            break;
                    }
                    FindViewById<ImageView>(Resource.Id.StarDetailStarGrade).SetImageResource(GradeId);

                    FindViewById<TextView>(Resource.Id.StarDetailStarName).Text = star.Name;

                    TextView attribute = FindViewById<TextView>(Resource.Id.StarDetailStarAttribute);
                    int attr_color_id = 0;
                    switch (star.Attribute)
                    {
                        default:
                        case "Support":
                            attr_color_id = Resource.Color.Attr_SupportColor;
                            break;
                        case "Vocal":
                            attr_color_id = Resource.Color.Attr_VocalColor;
                            break;
                        case "Session":
                            attr_color_id = Resource.Color.Attr_SessionColor;
                            break;
                        case "Dance":
                            attr_color_id = Resource.Color.Attr_DanceColor;
                            break;
                    }
                    attribute.Text = star.Attribute.ToUpper();
                    attribute.SetBackgroundResource(attr_color_id);

                    int TypeIconId = 0;
                    switch (star.Type)
                    {
                        case "Tap":
                            TypeIconId = Resource.Drawable.Type_Tap_Icon;
                            break;
                        case "Long":
                            TypeIconId = Resource.Drawable.Type_Long_Icon;
                            break;
                        case "Slide":
                            TypeIconId = Resource.Drawable.Type_Slide_Icon;
                            break;
                        case "Flick":
                            TypeIconId = Resource.Drawable.Type_Flick_Icon;
                            break;
                        case "Support":
                            TypeIconId = Resource.Drawable.Type_Support_Icon;
                            break;
                        default:
                        case "Special":
                            TypeIconId = Resource.Drawable.Type_Special_Icon;
                            break;
                    }
                    FindViewById<ImageView>(Resource.Id.StarDetailStarTypeIcon).SetImageResource(TypeIconId);
                    FindViewById<TextView>(Resource.Id.StarDetailStarType).Text = star.Type.ToUpper();
                }
                catch (Exception ex)
                {
                    ETC.LogError(this, ex.ToString());
                }
            }
            catch (Exception ex)
            {
                ETC.LogError(this, ex.ToString());
            }
        }
    }
}