using ImageInfoTool.App.Resources;
using PhoneKit.Framework.Controls;
using System;
using System.Collections.Generic;

namespace ImageInfoTool.App.Controls
{
    /// <summary>
    /// The localized about control.
    /// </summary>
    public class LocalizedAboutControl : AboutControlBase
    {
        protected override void LocalizeContent()
        {
            // app
            ApplicationIconSource = new Uri("/Assets/Images/176.png", UriKind.Relative);
            ApplicationTitle = AppResources.ApplicationTitle;
            ApplicationVersion = AppResources.ApplicationVersion;
            ApplicationAuthor = AppResources.ApplicationAuthor;
            ApplicationDescription = AppResources.ApplicationDescription;

            // buttons
            SupportAndFeedbackText = AppResources.SupportAndFeedback;
            SupportAndFeedbackEmail = "apps@bsautermeister.de";
            PrivacyInfoText = AppResources.PrivacyInfo;
            PrivacyInfoLink = "http://bsautermeister.de/privacy.php";
            RateAndReviewText = AppResources.RateAndReview;
            MoreAppsText = AppResources.MoreApps;
            MoreAppsSearchTerms = "Benjamin Sautermeister";

            // contributors
            ContributorsListVisibility = System.Windows.Visibility.Visible;
            IList<ContributorModel> contributors = new List<ContributorModel>();
            contributors.Add(new ContributorModel("/Assets/Images/icon.png", "Johanna from The Noun Project"));
            contributors.Add(new ContributorModel("/Assets/Images/geophoto.png", "Timo Partl for GeoPhoto benefits"));
            contributors.Add(new ContributorModel("/Assets/Images/icon.png", "E. MacDonald from The Noun Project"));
            contributors.Add(new ContributorModel("/Assets/Images/indonesian.png","Agus Setiawan"));
            contributors.Add(new ContributorModel("/Assets/Images/spanish.png", "Juan Febrero"));
            contributors.Add(new ContributorModel("/Assets/Images/french.png", "Vincent Vuillaume"));
            contributors.Add(new ContributorModel("/Assets/Images/czech.png", "Petr Hovorka"));
            contributors.Add(new ContributorModel("/Assets/Images/polish.png", "Rudolf_PL"));
            contributors.Add(new ContributorModel("/Assets/Images/russia.png", "Александр Чуркин"));
            contributors.Add(new ContributorModel("/Assets/Images/chinese.png", "陈玉龙Charles"));
            SetContributorsList(contributors);
        }
    }
}
