#NoRMatic
NoRMatic is a wrapper/extender for the excellent NoRM library for interacting with MongoDB.  NoRM exposes a complete LINQ provider over MongoDB collections.  NoRMatic applies an easy-to-use API in an ActiveRecord style using NoRM as it's underlying data access layer.  NoRMatic also provides behavior hooks for before and after save, before and after, delete, soft deleting, versioning, and basic auditing.

##Basics
Most of what you'll need to use NoRMatic is provided in the NoRMaticModel<T> base class.  Any class inheriting from this will be able to take advantage of the following instance members:

### Properties
* Id
* DateUpdated
* Errors

### Methods
* Save()
* Delete()
* GetVersions()

Additionally, the following static methods will be available:

-