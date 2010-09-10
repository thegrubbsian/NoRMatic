namespace NoRMatic.Tests.TestModel {

    public class NoRMaticSetup : INoRMaticInitializer {

        public void Setup() {
            
            Subscriber.EnableVersioning();
            Subscriber.AddQueryBehavior(x => x.City == "Charlotte");
        }
    }
}
