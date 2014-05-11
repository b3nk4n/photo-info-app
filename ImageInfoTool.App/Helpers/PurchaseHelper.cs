using PhoneKit.Framework.InAppPurchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageInfoTool.App.Helpers
{
    public static class PurchaseHelper
    {
        public static bool IsFreeTrialOrProVersion()
        {
            if (InAppPurchaseHelper.IsProductActive(AppConstants.PRO_VERSION_IN_APP_KEY))
                return true;

            var deadlineDate = AppSettings.AddFreeDateDeadline.Value;
            if (deadlineDate != DateTime.MinValue)
            {
                var remainingTrial = deadlineDate - DateTime.Now;

                if (remainingTrial.TotalDays > AppConstants.AD_FREE_TRIAL_TIME_IN_DAYS)
                {
                    // cheat detected -> reset!
                    AppSettings.AddFreeDateDeadline.Value = AppSettings.AddFreeDateDeadline.DefaultValue;
                    AppSettings.HasReviewed.Value = false;
                }
                else if (remainingTrial.TotalDays >= 0)
                {
                    // is in trial
                    return true;
                }
            }
            return false;
        }
    }
}
