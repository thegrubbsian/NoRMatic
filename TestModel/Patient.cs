using NoRMatic;

namespace TestModel {

    public class Patient : NoRMaticModel<Patient> {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
    }
}
