using ALE.ETLBox;
using ALE.ETLBox.ConnectionManager;
using ALE.ETLBox.ControlFlow;
using ALE.ETLBox.DataFlow;
using ALE.ETLBox.Helper;
using ALE.ETLBox.Logging;
using ALE.ETLBoxTests.Fixtures;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ALE.ETLBoxTests.DataFlowTests
{
    [Collection("DataFlow")]
    public class DbSourceExceptionTests
    {
        public static SqlConnectionManager SqlConnection => Config.SqlConnection.ConnectionManager("DataFlow");

        public DbSourceExceptionTests(DataFlowDatabaseFixture dbFixture)
        {
        }

        [Fact]
        public void UnknownTable()
        {
            //Arrange
            DbSource<string[]> source = new DbSource<string[]>(SqlConnection, "UnknownTable");
            MemoryDestination<string[]> dest = new MemoryDestination<string[]>();

            //Act & Assert
            Assert.Throws<ETLBoxException>(() =>
            {
                source.LinkTo(dest);
                source.Execute();
                dest.Wait();
            });
        }

        [Fact]
        public void UnknownTableViaTableDefinition()
        {
            //Arrange
            TableDefinition def = new TableDefinition("UnknownTable",
                new List<TableColumn>()
                {
                    new TableColumn("id", "INT")
                });
            DbSource<string[]> source = new DbSource<string[]>()
            {
                ConnectionManager = SqlConnection,
                SourceTableDefinition = def
            };
            MemoryDestination<string[]> dest = new MemoryDestination<string[]>();

            //Act & Assert
            Assert.Throws<Microsoft.Data.SqlClient.SqlException>(() =>
            {
                source.LinkTo(dest);
                source.Execute();
                dest.Wait();
            });
        }


        [Fact]
        public void ErrorInSql()
        {
            //Arrange
            DbSource source = new DbSource(SqlConnection)
            {
                Sql = "SELECT XYZ FROM ABC"
            };
            MemoryDestination dest = new MemoryDestination();
            source.LinkTo(dest);
            //Act & Assert
            Assert.Throws<SqlException>(() =>
            {
                try
                {
                    Task s = source.ExecuteAsync();
                    Task c = dest.Completion;
                    Task.WaitAll(c, s);
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            });
        }
    }
}
