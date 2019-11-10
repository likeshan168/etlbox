# <span><img src="https://github.com/roadrunnerlenny/etlbox/raw/master/docs/images/logo_orig_32x32.png" alt="ETLBox logo" height="32" /> Welcome to ETLBox</span>

It's all in the box! Run all your ETL or ELT jobs with this C# class library. Create your own dataflow, where data is send from a source to a target and transformed on its way. Data can be read from or written into various sources.

## ETLBox.net

There is a project homepage for ETLBox availabe: [etlbox.net](https://etlbox.net) contains all the information you need about ETLBox. There is a whole set of [introductional articles](https://etlbox.net/articles/getting_started.html), examples for [Control Flow Tasks](https://etlbox.net/articles/example_controlflow.html), [Data Flow Tasks](https://etlbox.net/articles/example_dataflow.html) and [Logging](https://etlbox.net/articles/example_logging.html), and finally a [complete API documentation](https://etlbox.net/api/index.html).

### See the video

Watch a short video that introduces you into the basic concepts of ETLBox and how to create your own ETL process. [See the video on Youtube!](https://www.youtube.com/watch?v=CsWZuRpl6PA)

## What is ETLBox

ETLBox is a comprehensive C# class library that is able to manage your whole ETL or ELT. You can use it to run some simple (or complex) sql against your database. You can easily manage your database using some easy-to-use and easy-to-understand objects. Or you can create your own dataflow pipeline, where data is send from a source to a target and transformed on its way. All that comes with extended logging capabilites, that allow you to monitor and anlayze your ETL job runs.

### Why ETLBox

Perhaps you are looking for an alternative to Sql Server Integrations Services (SSIS). Or you are searching for a framework to define and run ETL jobs with C# code. The goal of ETLBox is to provide an easy-to-use but still powerful library with which you can create complex ETL routines and sophisticated data flows.

### Advantages of using ETLBox

**Build ETL in C#**: Code your ETL with a language fitting your team’s skills and that is coming with a mature toolset

**Run locally**: Develop and test your ETL code locally on your desktop using your existing development & debugging tools.

**Process In-Memory**: ETLBox comes with dataflow components that allow in-memory processing which is much faster than storing data on disk and processing later.

**Know your errors**: When exceptions are raised you get the exact line of code where your ETL stopped, including a hands-on description of the error.

**Manage Change**: Track you changes with git (or other source controls), code review your etl logic, and use your existing CI/CD processes.

**Embedded or standalone**: With .NET core and .NET standard, etlbox is a self-deploying toolbox – usable whereever .NET runs. (.NET Standard 2.0 or higher required). Or just include the library as a reference to your existing .NET projects.

### Supported sources and destinations

The following table shows which types of sources and destination are supported out-of-the box with the current version of ETLBox

Source or Destination type|Supported by ETLBox|
---------------------------|-------------------
Sql Server|Full support
Postgres|Full support
SQLite|Full support
MySql|Full support
Files|Support for Json and CSV

Any other database, file or source can be supported by `CustomSource`/`CustomDestination`.

## ETLBox capabilities

ETLBox is split into two main components: Data Flow and Control Flow Tasks. Both components will provide customizable logging functionalities.

### Data Flow Tasks

#### Data Flow - overview

Dataflow tasks gives you the ability to create your own pipeline where you can send your data through. Dataflows consists of one or more source element (like CSV files or data derived from a table), some transformations and optionally one or more target. To create your own data flow , you need to accomplish three steps:

- First you define your dataflow components.
- Then, you link these components together (each source has an output, each transformation at least one input and one output and each destination has an input).
- After the linking you just tell your source to start reading the data.

The source will start reading and post its data into the components connected to its output. As soon as a connected component retrieves any data in its input, the component will start with processing the data and then send it further down the line to its connected components. The dataflow will finish when all data from the source(s) are read and received from the destinations.

Of course, all data is processed asynchronously by the components. Each compoment has its own set of buffers, so while the source is still reading data the transformations already can process it and the destinations can start writing the processed information into their target.

There are a lot of components in ETLBox already available. The `DBSource` can connect to a SqlServer, SQLite, MySql or Postgres table. There are transformation to modify either each record one-by-one or in whole. You can split or join different sources, and using the `CustomSource` and `CustomDestination` you can even connection to any target or source you want.

#### Data flow tasks - examples

It's easy to create your own data flow pipeline. This example pipe will transfer data from a MySql database into a Sql Server database and transform the records on the fly.

Just create a source, some transformation and a destination:

```C#
var sourceCon = new MySqlConnectionManager("Server=10.37.128.2;Database=ETLBox_ControlFlow;Uid=etlbox;Pwd=etlboxpassword;");
var destCon = new SqlConnectionManager("Data Source=.;Integrated Security=SSPI;Initial Catalog=ETLBox;");

DBSource<MySimpleRow> source = new DBSource<MySimpleRow>(sourceCon, "SourceTable");
RowTransformation<MySimpleRow, MySimpleRow> trans = new RowTransformation<MySimpleRow, MySimpleRow>(
    myRow => {  
        myRow.Value += 1;
        return myRow;
    });
DBDestination<MySimpleRow> dest = new DBDestination<MySimpleRow>(destCon, "DestinationTable");
```

Now link these pipeline elements together.

```C#
source.LinkTo(trans);
trans.LinkTo(dest);
```

Finally, start the dataflow at the source and wait for your destination to rececive all data (and the completion message from the source).

```C#
source.Execute();
dest.Wait();
```

### Control Flow Tasks

#### Control Flow - overview

Control Flow Tasks gives you control over your database: They allow you to create or delete databases, tables, procedures, schemas, ... or other objects in your database. With these tasks you also can truncate your tables, count rows or execute *any* sql you like. Anything you can script in sql can be done here - but mostly with only one line of easy-to-read C# code. This improves the readability of your code a lot, and gives you more time to focus on your business logic. But Control Flow tasks are not restricted to databases only: e.g. you can even run an XMLA on your Sql Server Analysis Service.

#### Control Flow - example

It is now very easy to execute some Sql on the Database, without writing the whole "boilerplate" code by ADO.NET.

```C#
var conn = new SqlConnectionManager("Server=10.37.128.2;Database=ETLBox_ControlFlow;Uid=etlbox;Pwd=etlboxpassword;");
//Execute some Sql
SqlTask.ExecuteNonQuery(conn, "Do some sql",$@"EXEC myProc");
//Count rows
int count = RowCountTask.Count(conn, "demo.table1").Value;
//Create a table
CreateTableTask.Create(conn, "Table1", new List<TableColumn>() {
    new TableColumn(name:"key",dataType:"INT",allowNulls:false,isPrimaryKey:true, isIdentity:true),
    new TableColumn(name:"value", dataType:"NVARCHAR(100)",allowNulls:true)
});
```

## Logging

### Default logging with NLog

By default, ETLBox uses and extends [NLog](https://nlog-project.org). ETLBox already comes with NLog as dependency. So the needed packages will be retrieved from nuget. In order to have the logging activating, you just have to set up a nlog configuration called nlog.config, and create a target and a logger rule. After adding this, you will already get logging output for all tasks and components in ETLBox.

Additionally, ETLBox offer some tasks and out-of-the-box functionalites to directly log into pre-defined database tables.

## Getting ETLBox

You can use ETLBox within any .NET or .NET core project that supports .NET Standard 2.0.

### Adding ETLBox to your project

#### Variant 1: Nuget

[ETLBox is available on nuget](https://www.nuget.org/packages/ETLBox). Just add the package to your project via your nuget package manager.

#### Variant 2: Download the sources

Clone the repository:

```bash
git clone https://github.com/roadrunnerlenny/etlbox.git
```

Then, open the downloaded solution file ETLBox.sln with Visual Studio 2015 or higher.
Now you can build the solution, and use it as a reference in your other projects.

## Going further

ETLBox is open source.
Feel free to make changes or to fix bugs. Every particiation in this open source project is appreciated.

To dig deeper into it, have a look at the ETLBox tests within the solution. There is a test for (almost) everything that you can do with ETLToolbox.

[See the ETLBox Project website](https://etlbox.net) for [introductional articles](https://etlbox.net/articles/getting_started.html) and examples for [Control Flow Tasks](https://etlbox.net/articles/example_controlflow.html), [Data Flow Tasks](https://etlbox.net/articles/example_dataflow.html) and [Logging](https://etlbox.net/articles/example_logging.html). There is also a [complete API documentation](https://etlbox.net/api/index.html). Enjoy!
