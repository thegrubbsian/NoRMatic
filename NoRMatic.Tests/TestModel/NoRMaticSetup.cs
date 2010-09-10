namespace NoRMatic.Tests.TestModel {

    public class NoRMaticSetup : INoRMaticInitializer {

        public void Initialize() {
            
            Subscriber.EnableVersioning();
            Subscriber.AddQueryBehavior(x => x.City == "Charlotte");
        }
    }
}
