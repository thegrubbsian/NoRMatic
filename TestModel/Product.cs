using Norm.BSON.DbTypes;
using NoRMatic;

namespace TestModel {

    public class Product : NoRMaticModel<Product> {

        public string Name { get; set; }
        public string Price { get; set; }
        public string Weight { get; set; }
        public DbReference<Supplier> Supplier { get; set; }
    }
}
