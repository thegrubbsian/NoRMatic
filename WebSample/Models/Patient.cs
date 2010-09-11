using System;
using NoRMatic;

namespace WebSample.Models {

    public class Patient : NoRMaticModel<Patient> {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}