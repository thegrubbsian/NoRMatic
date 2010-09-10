namespace NoRMatic.Tests.TestModel {

    public class MedicalRecord : NoRMaticModel<MedicalRecord> {

        public string PatientIdentifier { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Temperature { get; set; }
        public int Weight { get; set; }
    }
}
