namespace SortedStorage.Application
{
    public static class StorageConfiguration
    {
        public static string Tombstone => "DELETED_REGISTER";

        public static int MaxMemtableSize => 3;
    }
}
