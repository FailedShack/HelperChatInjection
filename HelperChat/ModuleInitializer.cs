using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HelperChat
{
    internal class ModuleInitializer
    {
        public static void Initialize()
        {
            var assembly = Assembly.Load("WiiU_USB_Helper");
            var settingsType = assembly.GetType("WIIU_Downloader.Properties.Settings");
            var donationKey = settingsType.GetProperty("DonationKey");

            // Override donation key accessors
            SwapImplementations
            (
                donationKey.GetGetMethod(),
                typeof(Overrides).GetMethod("GetDonationKey", BindingFlags.NonPublic | BindingFlags.Instance)
            );
            SwapImplementations
            (
                donationKey.GetSetMethod(),
                typeof(Overrides).GetMethod("SetDonationKey", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string) }, null)
            );

            var proxy = (from type in assembly.GetTypes()
                         where typeof(Form).IsAssignableFrom(type)
                         from prop in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                         where prop.Name == "Proxy"
                         select prop).FirstOrDefault();

            // Override proxy accessors
            SwapImplementations
            (
                proxy.GetGetMethod(true),
                typeof(Overrides).GetMethod("GetProxy", BindingFlags.NonPublic | BindingFlags.Instance)
            );
            SwapImplementations
            (
                proxy.GetSetMethod(true),
                typeof(Overrides).GetMethod("SetProxy", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(WebProxy) }, null)
            );

            SetupTests();
        }

        private static void SwapImplementations(MethodInfo method1, MethodInfo method2)
        {
            RuntimeHelpers.PrepareMethod(method1.MethodHandle);
            RuntimeHelpers.PrepareMethod(method2.MethodHandle);
            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    int* replacePtr = (int*)method1.MethodHandle.Value.ToPointer() + 2;
                    int* injectPtr = (int*)method2.MethodHandle.Value.ToPointer() + 2;
                    int oldPtr = *replacePtr;
                    *replacePtr = *injectPtr;
                    *injectPtr = oldPtr;
                }
                else
                {
                    long* replacePtr = (long*)method1.MethodHandle.Value.ToPointer() + 1;
                    long* injectPtr = (long*)method2.MethodHandle.Value.ToPointer() + 1;
                    long oldPtr = *replacePtr;
                    *replacePtr = *injectPtr;
                    *injectPtr = oldPtr;
                }
            }
        }

        public static void SetupTests()
        {
            Overrides.OnSetDonationKey += delegate (string donationKey)
            {
                MessageBox.Show("New donation key: " + donationKey);
            };
            Overrides.OnSetProxy += delegate (WebProxy proxy)
            {
                MessageBox.Show("New proxy: " + proxy.Address.ToString());
            };
        }
    }
}
