using System.ComponentModel.DataAnnotations;
using NoRMatic;

namespace TestModel {

    public class Customer : NoRMaticModel<Customer> {

        [Required]
        public string Name { get; set; }

        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        public string Address { get; set; }
    }
}
