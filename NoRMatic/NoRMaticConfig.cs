using System.Configuration;

namespace NoRMatic {

    public static class NoRMaticConfig {

        public static string ConnectionString {
            get { return ConfigurationManager.ConnectionStrings["NoRMaticConnectionString"].ConnectionString; }
        }
    }
}
