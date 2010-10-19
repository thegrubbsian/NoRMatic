using System.Linq;
using Norm;
using Norm.BSON.DbTypes;
using NUnit.Framework;
using TestModel;
using System.Collections.Generic;

namespace NoRMatic.Tests {

    [TestFixture]
    public class BasicTests {

        [SetUp]
        public void DropCollections() {
            Customer.DeleteAll();
            MedicalRecord.DeleteAll();
            Order.DeleteAll();
            Patient.DeleteAll();
            Product.DeleteAll();
            Subscriber.DeleteAll();
            Supplier.DeleteAll();
        }

        [Test]
        public void GivenANonExistantCollection_All_ShouldReturnAnEmptyList() {

            Patient.DeleteAll();

            var patients = Patient.All().ToList();

            Assert.AreEqual(0, patients.Count);
        }

        [Test]
        public void NoRMaticModelSave_ShouldPersistTheObject() {

            Patient.DropBehaviors();

            var patient = new Patient { FirstName = "John", LastName = "Doe", Age = 39, Gender = "male" };
            patient.Save();

            var fetched = Patient.GetById(patient.Id);

            Assert.NotNull(fetched);
        }

        [Test]
        public void GivenAModelWithABeforeSaveBehavior_Save_ShouldOnlyPersistTheEntityIfTheBehaviorAllows() {

            Patient.DropBehaviors();

            Patient.AddBeforeSaveBehavior(x => false);

            var patient = new Patient { FirstName = "Bob", LastName = "Smith", Age = 72, Gender = "male" };
            patient.Save();

            var fetched = Patient.GetById(patient.Id);

            Assert.IsNull(fetched);
        }

        [Test]
        public void GivenAModelWithAQueryBehavior_All_ShouldOnlyReturnEntitiesMatchingTheBehaviorExpression() {

            Patient.DropBehaviors();

            Patient.AddQueryBehavior(x => x.Age < 20);

            var patientA = new Patient { FirstName = "Bill", LastName = "Crane", Age = 50, Gender = "male" };
            var patientB = new Patient { FirstName = "Katie", LastName = "Edwards", Age = 12, Gender = "female" };

            patientA.Save();
            patientB.Save();

            var fetched = Patient.All();

            Assert.IsTrue(fetched.All(x => x.Age < 20));
        }

        [Test]
        public void GivenAModelWithMultipleQueryBehaviors_All_ShouldOnlyReturnEntitiesMatchingTheBehaviorExpressions() {

            Patient.DropBehaviors();

            Patient.AddQueryBehavior(x => x.Gender == "female");
            Patient.AddQueryBehavior(x => x.Age > 25);

            var patientA = new Patient { FirstName = "Bill", LastName = "Crane", Age = 50, Gender = "male" };
            var patientB = new Patient { FirstName = "Katie", LastName = "Edwards", Age = 12, Gender = "female" };
            var patientC = new Patient { FirstName = "Agetha", LastName = "Barnes", Age = 78, Gender = "female" };

            patientA.Save();
            patientB.Save();
            patientC.Save();

            var fetched = Patient.All();

            Assert.IsTrue(fetched.All(x => x.Age > 25 && x.Gender == "female"));
        }

        [Test]
        public void GivenAModelWithAQueryBehavior_Find_ShouldReturnEntitiesThatMatchBothWhereAndBehaviorExpressions() {

            Patient.DropBehaviors();

            Patient.AddQueryBehavior(x => x.Age > 25);

            var patientA = new Patient { FirstName = "Bill", LastName = "Crane", Age = 50, Gender = "male" };
            var patientB = new Patient { FirstName = "Katie", LastName = "Edwards", Age = 12, Gender = "female" };
            var patientC = new Patient { FirstName = "Agetha", LastName = "Barnes", Age = 78, Gender = "female" };

            patientA.Save();
            patientB.Save();
            patientC.Save();

            var fetched = Patient.Find(x => x.Gender == "female").ToList();

            Assert.IsTrue(fetched.All(x => x.Age > 25 && x.Gender == "female"));
        }

        [Test]
        public void GivenAModelWithSoftDeleteEnabled_Delete_DoesNotDeleteTheEntityByMarksItsDeletedProperty() {

            Patient.DropBehaviors();
            Patient.EnableSoftDelete();

            var patient = new Patient { FirstName = "Bob", LastName = "Smith", Age = 72, Gender = "male" };
            patient.Save();

            patient.Delete();

            var fetched = Patient.GetById(patient.Id, includeDeleted: true);

            Assert.IsNotNull(fetched);
            Assert.IsTrue(fetched.IsDeleted);
        }

        [Test]
        public void GivenAModelWithSoftDeleteEnabled_Delete_DoesNotPersistSubsequentChangesToTheEntity() {

            Patient.DropBehaviors();
            Patient.EnableSoftDelete();

            var patient = new Patient { FirstName = "Bob", LastName = "Smith", Age = 72, Gender = "male" };
            patient.Save();

            patient.Delete();

            var fetched = Patient.GetById(patient.Id, includeDeleted: true);

            fetched.FirstName = "NEWFIRSTNAME";
            fetched.Save();

            var reFetched = Patient.GetById(patient.Id, includeDeleted: true);

            Assert.AreEqual(reFetched.FirstName, patient.FirstName);
        }

        [Test]
        public void GivenAModelWithValidation_Save_ShouldNotPersistTheEntity() {

            Customer.DropBehaviors();

            var customer = new Customer();
            customer.Save();

            Assert.IsNull(customer.Id);
            Assert.AreEqual(2, customer.Errors.Count);
        }

        [Test]
        public void GivenAModelWithVersioningEnabledButNotSoftDelete_Delete_ShouldDeleteAllVersions() {

            Patient.DropBehaviors();

            Patient.EnableVersioning();

            var patient = new Patient { FirstName = "Jill", LastName = "Stevens", Age = 29, Gender = "female" };
            patient.Save();

            patient.Age = 30;
            patient.Save();

            patient.LastName = "Martin";
            patient.Save();

            var versions = patient.GetVersions();

            Assert.AreEqual(3, versions.Count());
        }

        [Test]
        public void GivenAModelSetupByNoRMaticInitializer_Save_ShouldRespectBehaviors() {

            Subscriber.DropBehaviors();

            NoRMaticConfig.Initialize();

            var subscriber = new Subscriber { FirstName = "Joe", LastName = "Peters" };
            subscriber.Save();

            subscriber.LastName = "Jones";
            subscriber.Save();

            var versions = subscriber.GetVersions();

            Assert.AreEqual(2, versions.Count());
        }

        [Test]
        public void GivenAModelSetupByNoRMaticInitializer_Find_ShouldRespectBehaviors() {

            Subscriber.DropBehaviors();

            NoRMaticConfig.Initialize();

            var subscriberA = new Subscriber { FirstName = "Samantha", LastName = "Jones", City = "New York" };
            var subscriberB = new Subscriber { FirstName = "Niki", LastName = "Jones", City = "Charlotte" };
            var subscriberC = new Subscriber { FirstName = "Maria", LastName = "Gilcher", City = "Charlotte" };

            subscriberA.Save();
            subscriberB.Save();
            subscriberC.Save();

            var fetched = Subscriber.Find(x => x.LastName == "Jones");

            Assert.IsFalse(fetched.Any(x => x.City != "Charlotte"));
        }

        [Test]
        public void GivenAModelWithSoftDeleteAndVersioningEnabled_All_ShouldNotReturnAnyDeletedItemsOrVersions() {

            Patient.DropBehaviors();

            Patient.EnableVersioning();
            Patient.EnableSoftDelete();

            var patientA = new Patient { FirstName = "Greg", LastName = "Sanderson", Age = 32, Gender = "male" };
            var patientB = new Patient { FirstName = "Lou", LastName = "Marcus", Age = 22, Gender = "male" };
            var patientC = new Patient { FirstName = "Sandra", LastName = "O'Connor", Age = 78, Gender = "female" };

            patientA.Save();
            patientB.Save();
            patientC.Save();

            patientA.Delete();
            patientC.Delete();

            patientB.LastName = "Michaels";
            patientB.Save();

            var fetched = Patient.All();

            Assert.IsTrue(!fetched.Any(x => x.IsDeleted));
            Assert.IsTrue(!fetched.Any(x => x.IsVersion));
        }

        [Test]
        public void GivenAModelWithSoftDeleteAndVersioningEnabled_Find_ByDefaultShouldNotReturnAnyDeletedItemsOrVersions() {

            Patient.DropBehaviors();

            Patient.EnableVersioning();
            Patient.EnableSoftDelete();

            var patientA = new Patient { FirstName = "Greg", LastName = "Sanderson", Age = 32, Gender = "male" };
            var patientB = new Patient { FirstName = "Lou", LastName = "Marcus", Age = 22, Gender = "male" };
            var patientC = new Patient { FirstName = "Sandra", LastName = "O'Connor", Age = 78, Gender = "female" };

            patientA.Save();
            patientB.Save();
            patientC.Save();

            patientA.Delete();
            patientC.Delete();

            patientB.LastName = "Michaels";
            patientB.Save();

            var fetched = Patient.Find(x => x.Age > 12);

            Assert.IsTrue(!fetched.Any(x => x.IsDeleted));
            Assert.IsTrue(!fetched.Any(x => x.IsVersion));
        }

        [Test]
        public void GivenAModelWithSoftDeleteAndVersioningEnabled_Find_ShouldReturnDeletedItemsAndVersionsWhenOverridden() {

            Patient.DeleteAll();
            Patient.DropBehaviors();

            Patient.EnableVersioning();
            Patient.EnableSoftDelete();

            var patientA = new Patient { FirstName = "Greg", LastName = "Sanderson", Age = 32, Gender = "male" };
            var patientB = new Patient { FirstName = "Lou", LastName = "Marcus", Age = 22, Gender = "male" };
            var patientC = new Patient { FirstName = "Sandra", LastName = "O'Connor", Age = 78, Gender = "female" };

            patientA.Save();
            patientB.Save();
            patientC.Save();

            patientA.Delete();
            patientC.Delete();

            patientB.LastName = "Michaels";
            patientB.Save();

            var fetched = Patient.Find(x => x.Age > 12, includeDeleted: true, includeVersions: true);

            Assert.IsTrue(fetched.Any(x => x.IsDeleted));
            Assert.IsTrue(fetched.Any(x => x.IsVersion));
        }

        [Test]
        public void GivenAModelWithUserAuditingEnabled_Save_ShouldApplyTheCurrentUserToUpdatedByProperty() {

            MedicalRecord.DropBehaviors();

            NoRMaticConfig.SetCurrentUserProvider(() => "user1");
            MedicalRecord.EnableUserAuditing();

            var medicalRecord = new MedicalRecord {
                PatientIdentifier = "12345",
                FirstName = "Tom",
                LastName = "Burris",
                Temperature = 98,
                Weight = 173
            };

            medicalRecord.Save();

            Assert.AreEqual(medicalRecord.UpdatedBy, "user1");
        }

        [Test]
        public void GivenAModelWithAnAbstractBehavior_Save_ShouldApplyTheAbstractBehavior() {

            NoRMaticConfig.DropAbstractBehaviors();

            NoRMaticConfig.AddBeforeSaveAbstractBehavior<IBoundByAccount>(x => { x.AccountName = "accountA"; return true; });

            var order = new Order { Quantity = 20, Sku = "A3EF29" };
            order.Save();

            Assert.AreEqual("accountA", order.AccountName);
        }

        [Test]
        public void GivenAModelWithNoBehaviors_GetById_ShouldReturnTheCorrectDocument() {

            Patient.DropBehaviors();
            Patient.DeleteAll();

            ObjectId lastId = null;

            for (var i = 0; i < 20; i++) {
                var patient = new Patient { FirstName = "Jim", LastName = "Bob", Age = 10 + i };
                patient.Save();
                if (i == 19)
                    lastId = patient.Id;
            }

            var fetched = Patient.GetById(lastId);

            Assert.AreEqual(lastId, fetched.Id);
            Assert.AreEqual(29, fetched.Age);
        }

        [Test]
        public void GivenAModelWithSoftDeleteAndVersioning_GetById_ShouldRespectTheBehaviors() {

            Patient.DropBehaviors();
            Patient.DeleteAll();

            Patient.EnableSoftDelete();
            Patient.EnableVersioning();

            var patientA = new Patient { FirstName = "Jim", LastName = "Bob", Age = 10 };
            var patientB = new Patient { FirstName = "Jim", LastName = "Bob", Age = 10 };
            var patientC = new Patient { FirstName = "Jim", LastName = "Bob", Age = 10 };
            var patientD = new Patient { FirstName = "Jim", LastName = "Bob", Age = 10 };
            var patientE = new Patient { FirstName = "Jim", LastName = "Bob", Age = 10 };
            var patientF = new Patient { FirstName = "Jim", LastName = "Bob", Age = 10 };
            patientA.Save();
            patientB.Save();
            patientC.Save();
            patientD.Save();
            patientE.Save();
            patientF.Save();

            patientA.LastName = "Edwards";
            patientA.Save();

            patientB.FirstName = "Steve";
            patientB.Save();

            patientE.Delete();
            patientF.Delete();

            var fetchedA = Patient.GetById(patientA.Id);
            var fetchedC = Patient.GetById(patientC.Id);
            var fetchedF = Patient.GetById(patientF.Id);

            Assert.AreEqual(fetchedA.Id, patientA.Id);
            Assert.IsTrue(!fetchedA.IsVersion);

            Assert.AreEqual(fetchedC.Id, patientC.Id);

            Assert.IsNull(fetchedF);
        }

        [Test]
        public void GivenAConfigurationWithALogListener_Find_ShouldWriteAppropriateLogMessages() {

            Subscriber.DeleteAll();

            var messages = new List<string>();

            NoRMaticConfig.SetLogListener(messages.Add);

            var subscriberA = new Subscriber {
                FirstName = "James",
                LastName = "Stimpleton",
                Address = "100 Street Avenue",
                City = "Citytown",
                State = "ST",
                Zip = "12345"
            };
            subscriberA.Save();

            var fetched = Subscriber.Find(x => x.FirstName == "James");

            Assert.AreEqual(2, messages.Count);
        }

        [Test]
        public void GivenAModelWithADbReferenceProperty_FetchRef_ShouldReturnTheCorrectEntity() {

            var supplier = new Supplier { Name = "Acme" };
            supplier.Save();

            var product = new Product {
                Name = "Hammer",
                Price = "10",
                Supplier = new DbReference<Supplier>(supplier.Id)
            };
            product.Save();

            var fetchedRef = product.GetRef(x => x.Supplier);

            Assert.IsNotNull(fetchedRef);
            Assert.AreEqual(fetchedRef.Name, supplier.Name);
        }
    }
}
