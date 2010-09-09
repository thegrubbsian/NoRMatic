using System.Linq;
using NoRMatic.Tests.TestModel;
using NUnit.Framework;

namespace NoRMatic.Tests {

    [TestFixture]
    public class BasicPersistanceTests {

        [Test]
        public void NoRMaticModelSave_ShouldPersistTheObject() {

            Patient.DropBehaviors();

            var patient = new Patient { FirstName = "John", LastName = "Doe", Age = 39, Gender = "male" };
            patient.Save();

            var fetched = Patient.GetById(patient.Id);

            Assert.NotNull(fetched);
        }

        [Test]
        public void GivenAModelWithABeforeSaveBehavior_Save_ShouldOnlyPersistTheObjectIfTheBehaviorAllows() {

            Patient.DropBehaviors();

            Patient.AddBeforeSaveBehavior(x => false);

            var patient = new Patient { FirstName = "Bob", LastName = "Smith", Age = 72, Gender = "male" };
            patient.Save();

            var fetched = Patient.GetById(patient.Id);

            Assert.IsNull(fetched);
        }

        [Test]
        public void GivenAModelWithAQueryBehavior_All_ShouldOnlyReturnObjectsMatchingTheBehaviorExpression() {
            
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
        public void GivenAModelWithMultipleQueryBehaviors_All_ShouldOnlyReturnObjectsMatchingTheBehaviorExpressions() {
            
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
        public void GivenAModelWithAQueryBehavior_Find_ShouldReturnObjectsThatMatchBothWhereAndBehaviorExpressions() {

            Patient.DropBehaviors();

            Patient.AddQueryBehavior(x => x.Age > 25);

            var patientA = new Patient { FirstName = "Bill", LastName = "Crane", Age = 50, Gender = "male" };
            var patientB = new Patient { FirstName = "Katie", LastName = "Edwards", Age = 12, Gender = "female" };
            var patientC = new Patient { FirstName = "Agetha", LastName = "Barnes", Age = 78, Gender = "female" };

            patientA.Save();
            patientB.Save();
            patientC.Save();

            var fetched = Patient.Find(x => x.Gender == "female");

            Assert.IsTrue(fetched.All(x => x.Age > 25 && x.Gender == "female"));
        }

        [Test]
        public void GivenAModelWithSoftDeleteEnabled_Delete_DoesNotDeleteTheEntityByMarksItsDeletedProperty() {
            
            Patient.DropBehaviors();
            Patient.EnableSoftDelete();

            var patient = new Patient { FirstName = "Bob", LastName = "Smith", Age = 72, Gender = "male" };
            patient.Save();

            patient.Delete();

            var fetched = Patient.GetById(patient.Id);

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

            var fetched = Patient.GetById(patient.Id);

            Assert.IsNotNull(fetched);
            Assert.IsTrue(fetched.IsDeleted);
        }
    }
}
