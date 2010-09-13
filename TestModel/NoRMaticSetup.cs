using System.Configuration;
using NoRMatic;

namespace TestModel {

    public class NoRMaticSetup : INoRMaticInitializer {

        public void Setup() {

            NoRMaticConfig.SetConnectionStringProvider(() => 
                ConfigurationManager.ConnectionStrings["NoRMaticConnectionString2"].ConnectionString);
            
            Subscriber.EnableVersioning();
            Subscriber.AddQueryBehavior(x => x.City == "Charlotte");
        }
    }
}
