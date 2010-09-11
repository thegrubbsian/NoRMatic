using NoRMatic;

namespace WebSample.Models {

    public class NoRMaticSetup : INoRMaticInitializer {

        public void Setup() {
            
            Patient.EnableVersioning();
        }
    }
}