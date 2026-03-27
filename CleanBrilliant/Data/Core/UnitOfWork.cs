namespace CleanBrilliant.Data
{
    public class UnitOfWork
    {
        private string dbConnectionString;

        public void registerNew(object obj) { }
        public void registerDirty(object obj) { }
        public void registerClean(object obj) { }
        public void registerDeleted(object obj) { }
        public void commit() { }
    }
}