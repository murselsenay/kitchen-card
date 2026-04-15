namespace Modules.AdressableSystem.Constants
{
    public struct AddressableLogMessages
    {
        public const string ADDRESSABLE_LOAD_FAILED = "Failed to load addressable asset with address: {0}";
        public const string ADDRESSABLE_LOAD_ALL_FAILED = "Failed to load addressable assets with label: {0}";
        public const string ADDRESSABLE_INSTANTIATE_FAILED = "Failed to instantiate addressable asset with address: {0}";
        public const string ADDRESSABLE_INSTANTIATE_RETURN_NULL = "Addressable asset with address: {0} was instantiated but returned null";
        public const string ADDRESSABLE_COMPONENT_NOT_FOUND = "Addressable asset with address: {0} does not contain a component of type: {1}";
        public const string POOL_FAILED = "Failed to get or return object with key: {0} from the pool";
    }
}