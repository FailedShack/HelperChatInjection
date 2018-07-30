using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HelperChat
{
    public class Overrides
    {
        public static event OnDonationKeyChange OnSetDonationKey;

        public static event OnProxyChange OnSetProxy;

        public delegate void OnDonationKeyChange(string donationKey);

        public delegate void OnProxyChange(WebProxy proxy);

        public static string DonationKey { get; set; }

        public static WebProxy Proxy { get; set; }

        private string GetDonationKey()
        {
            if (DonationKey != null)
            {
                return DonationKey;
            }
            // After swapping the implementations
            // this will call the original getter.
            return GetDonationKey();
        }

        private WebProxy GetProxy()
        {
            if (Proxy != null)
            {
                return Proxy;
            }
            // After swapping the implementations
            // this will call the original getter.
            return GetProxy();
        }

        private void SetDonationKey(string donationKey)
        {
            OnSetDonationKey?.Invoke(donationKey);
            // After swapping the implementations
            // this will call the original setter.
            SetDonationKey(donationKey);
        }

        private void SetProxy(WebProxy proxy)
        {
            OnSetProxy?.Invoke(proxy);
            // After swapping the implementations
            // this will call the original setter.
            SetProxy(proxy);
        }
    }
}
