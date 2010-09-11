namespace NoRMatic.Tests.TestModel {

    public interface IBoundByAccount {
        string AccountName { get; set; }
    }

    public class Order : NoRMaticModel<Order>, IBoundByAccount {
        public string AccountName { get; set; }
        public string Sky { get; set; }
        public int Quantity { get; set; }
    }
}
