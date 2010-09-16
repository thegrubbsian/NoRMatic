namespace NoRMatic {

    /// <summary>
    /// This interface identifies initializer classes which can be used during application startup
    /// to register behaviors and providers for the AppDomain.
    /// </summary>
    public interface INoRMaticInitializer {

        /// <summary>
        /// This method will be called by NoRMaticConfig.Initialize().
        /// </summary>
        void Setup();
    }
}
