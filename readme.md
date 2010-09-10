#NoRMatic
NoRMatic is a wrapper/extender for the excellent NoRM library (thanks to Andrew Theken) for interacting with MongoDB in .NET.  NoRM exposes a complete LINQ provider over MongoDB collections along with helpers for index and collection management as well as strongly typed document mapping.  NoRMatic applies an easy-to-use API in an ActiveRecord style using NoRM as it's underlying data access layer.  NoRMatic also provides behavior hooks for before and after save, before and after delete, as well as soft deleting, versioning, and basic auditing.

##Basics
Most of what you'll need to use NoRMatic is provided in the NoRMaticModel<T> base class.  Any class inheriting from this will be able to take advantage of the following members:

### Instance Members

#### Properties
* Id [ObjectId] - The MongoDB _id property for the document
* DateUpdated [DateTime] - This is updated each time the document is saved
* Errors [List<ValidationResult>] - A list of validation results from System.ComponentModel.DataAnnotations
* IsDeleted [bool] - If EnableSoftDelete is set, this will indicate if the document has been deleted
* DateDeleted [DateTime] - If EnableSoftDelete is set, this indicates the date the document was deleted
* IsVersion [bool] - If EnableVersioning is set, this will indicate if the document is a version
* DateVersioned [DateTime] - If EnableVersioning is set, this indicates the date the version was created
* VersionOfId [ObjectId] - If EnableVersioning is set, this is the source document's Id

#### Methods
* Save() [void] - Persists the document and creates versions if EnableVersioning is set
* Delete() [void] - Deletes the document or "soft deletes" it if EnableSoftDelete is set
* GetVersions() [List<T>] - Returns a list of versions for the document if EnableVersioning is set

### Static Methods

* All() [IEnumerable<T>] - Returns all documents from the collection excluding versions or soft deleted documents
* Find(Expression<Func<T, bool>>) [IQueryable<T>] - Finds a document via the NoRM LINQ provider
* GetById(ObjectId) [T] - Finds a single document by its Id which includes soft deleted items
* DeleteAll() [void] - Drops the entire collection regardless of EnableSoftDelete
* GetMongoCollection() [IMongoCollection<T>] - Returns a raw hook to the NoRM collection
	
	public class Customer : NoRMaticModel<Customer> { ... }
	var customer = new Customer { Name = "Bill Smith", Address = "101 Address Way" };
	customer.Save();
	customer.Delete();

## Soft Deletes
To enable soft delete on a type, use the static method EnableSoftDelete() on any NoRMaticModel<T> type.  For example, the following will enable soft delete for all instances of Customer or Product.
	
	Customer.EnableSoftDelete();
	Product.EnableSoftDelete();

When soft delete is enabled, any calls to Delete() will not remove the document from the collection, but will set the documents IsDeleted property to true and the DateDeleted property to the current date.  The document will no longer be updateable, you will be able to access it, but not save any changes to the documents properties.

## Versioning
To enable versioning on a type, use the static method EnableVersioning() on any NoRMaticModel<T> type.  For example, the following will enable versioning for all instances of Note and MedicalRecord.
	
	Note.EnableVersioning();
	MedicalRecord.EnableVersioning();

Versions are stored in the same collection as the source documents except they have three additional properties set: IsVersion, DateVersioned, and VersionOfId.  The version documents recieve a new Id but their VersionOfId property points back to the Id of the source document.  Each instance of a versioned document has a GetVersions() method wich will return all past versions of the document.

## Validation
Validation in NoRMatic is done using the System.ComponentModel.DataAnnotations attributes.  If any of these validation attributes are applied, all calls to Save() will cause validation to occur.  Any documents with validation errors will not be saved.  The documents Errors property can be used to view any errors generated by the current state of the document.

The base set of validation attributes doesn't support complex nested types (such as a produc having a list of sub-products) so NoRMatic provides the [ValidateChild].  This attribute, when applied to a property, causes deep validation for the document.

## Behaviors



## Configuration/Initialization