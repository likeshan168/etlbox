using ALE.ETLBox;
using ALE.ETLBox.ConnectionManager;
using ALE.ETLBox.ControlFlow;
using ALE.ETLBox.DataFlow;
using ALE.ETLBox.Helper;
using ALE.ETLBox.Logging;
using ALE.ETLBoxTests.Fixtures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Xunit;

namespace ALE.ETLBoxTests.DataFlowTests
{
    [Collection("DataFlow")]
    public class RowMultiplicationDynamicObjectTests
    {
        public SqlConnectionManager SqlConnection => Config.SqlConnection.ConnectionManager("DataFlow");
        public RowMultiplicationDynamicObjectTests(DataFlowDatabaseFixture dbFixture)
        {
        }

        [Fact]
        public void ReturningNewDynamicObject()
        {
            //Arrange
            TwoColumnsTableFixture source2Columns = new TwoColumnsTableFixture("RowMultiplicationSource");
            source2Columns.InsertTestData();

            DbSource source = new DbSource(SqlConnection, "RowMultiplicationSource");
            RowMultiplication  multiplication = new RowMultiplication(
                row =>
                {
                    List<ExpandoObject> result = new List<ExpandoObject>();
                    dynamic r = row as ExpandoObject;
                    for (int i = 0; i <= r.Col1; i++)
                    {
                        dynamic newdynamic = new ExpandoObject();
                        newdynamic.Col3 = i * r.Col1;
                        result.Add(newdynamic);
                    }
                    return result;
                });
            MemoryDestination dest = new MemoryDestination();

            //Act
            source.LinkTo(multiplication);
            multiplication.LinkTo(dest);
            source.Execute();
            dest.Wait();

            //Assert
            Assert.Collection(dest.Data,
                d => { dynamic r = d as dynamic; Assert.True(r.Col3 == 0); },
                d => { dynamic r = d as dynamic; Assert.True(r.Col3 == 1); },
                d => { dynamic r = d as dynamic; Assert.True(r.Col3 == 0); },
                d => { dynamic r = d as dynamic; Assert.True(r.Col3 == 2); },
                d => { dynamic r = d as dynamic; Assert.True(r.Col3 == 4); },
                d => { dynamic r = d as dynamic; Assert.True(r.Col3 == 0); },
                d => { dynamic r = d as dynamic; Assert.True(r.Col3 == 3); },
                d => { dynamic r = d as dynamic; Assert.True(r.Col3 == 6); },
                d => { dynamic r = d as dynamic; Assert.True(r.Col3 == 9); }
            );
        }
    }
}
