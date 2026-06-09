using Microsoft.VisualStudio.TestTools.UnitTesting;

// Test classes own isolated databases (see Containers/AzureSqlDbContainer.cs), so they have
// no shared writable state and can run concurrently. Workers = 4 keeps us well under the SQL
// container's 3.5 GB memory limit even with multiple per-class DBs in flight.
[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]
