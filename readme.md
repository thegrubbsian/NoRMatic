#NoRMatic
NoRMatic is a wrapper/extender for the excellent NoRM library for interacting with MongoDB.  NoRM exposes a complete LINQ provider over MongoDB collections.  NoRMatic applies an easy-to-use API in an ActiveRecord style using NoRM as it's underlying data access layer.  NoRMatic also provides behavior hooks for before and after save, before and after, delete, soft deleting, versioning, and basic auditing.

##Basics
Most of what you'll need to use NoRMatic is provided in the NoRMaticModel<T> base class.  Any class inheriting from this will be able to take advantage of the following instance members:

### Properties
* Id [ObjectId] - The MongoDB _id property for the document
* DateUpdated [DateTime] - This is updated each time the document is saved
* Errors [List<ValidationResult>] - A list of validation results from System.ComponentModel.DataAnnotations
* IsDeleted [bool] - If EnableSoftDelete is set, this will indicate if the document has been deleted
* DateDeleted [DateTime] - If EnableSoftDelete is set, this indicates the date the document was deleted
* IsVersion [bool] - If EnableVersioning is set, this will indicate if the document is a version
* DateVersioned [DateTime] - If EnableVersioning is set, this indicates the date the version was created
* VersionOfId [ObjectId] - If EnableVersioning is set, this is the source document's Id

### Methods
* Save() [void] - Persists the document and creates versions if EnableVersioning is set
* Delete() [void] - Deletes the document or "soft deletes" it if EnableSoftDelete is set
* GetVersions() [List<T>] - Returns a list of versions for the document if EnableVersioning is set

## Soft Deletes
To enable soft delete on a type, use the static method EnableSoftDelete() on any NoRMaticModel<T> object.  For example, the following will enable soft delete for all instances of Customer or Product.
	
<pre><code>
	Customer.EnableSoftDelete();
	Product.EnableSoftDelete();
</code></pre>

## Versioning


## Validation


## Behaviors
Much of what is available


## Configuration/Initialization