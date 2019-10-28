# ModelMaker for SQL Server
Welcome to the world of power and simplicity. ModelMaker is a simple and lightweight tool built on ADO.NET foundation. ModelMaker is for those who loves SQL queries and optimization. Let's start!!

### What is ModelMaker?

Before we begin, we need to understand the basic structure of a **Data Model** in **Data Access Layer (DAL)** in a **3-Layer** or **N-Layer** Application. DAL is responsible for handling all sorts of database operations of an application. A model represents an **entity** or a database **table**. 

Here, we create a class for model which will have some properties that will represent that entity or a table and it's attributes or columns. We will also have some methods for operations (insert, update, select, delete) for that certain entity. We have various tools for creating these data table. One of them is ADO.NET which is a core level tool for database operations in .NET framework. It directly connects and make transactions with the database. That's why it's fast. But we need to write a bunch of codes and sometimes repeat code segments to do one operation. For an application which relies mostly on database it will be so hard to work faster. Here comes the **ModelMaker**. It's built on ADO.NET and does not put an extra layer over it. ModelMaker directly handles operation using ADO.NET but in a optimized way. It makes ADO.NET easier and finishes work faster.


## Installation
To install ModelMaker from NuGet Package Manager CLI, <br><br>
``PM> Install-Package ModelMaker -Version 1.2.0`` <br><br>
or, goto: https://www.nuget.org/packages/ModelMaker/


## Documentation
### ModelMaker.Model (string connectionString) Class
A data model class will be a derived class of base class ``ModelMaker.Model`` and will consist of some of it's properties and methods. It's a basic structure of a data model:

``` c#

class StudentModel : Model
    {
        private static string connStr = "connection string";   
        public StudentModel() : base(connStr) 
        { 
        
        }
    }

```

This is how we make a subclass of ``ModelMaker.Model`` class. Now let's dive deep into it.

There are two basic operations in ModelMaker:
1. ``Exec(string query) [void]`` --> for queries that return no results (insert, update, delete)
2. ``Read<T>(string query) [List<T>]`` --> for queries that return results (select)

#### ModelMaker.Model.Exec (string query, object parameterValues) Method

Although, you write **dynamic** and **parameterized** SQL queries both but we strongly recommend to write parameterized SQL queries because there is a risk of **SQL Injection** with dynamic SQL queries. Well, here is an example of a parameterized `Exec()` operation: 

``` c#
using ModelMaker;

namespace modelMakerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            StudentModel student = new StudentModel();
            student.student_name = "Mahfuz";
            student.enrolled_dept = "CSE";
            student.Insert();
        }
    }

    class StudentModel : Model
    {
        public string id { get; set; }
        public string student_name { get; set; }
        public string enrolled_dept { get; set; }

        private static string connStr = "connection string";   

        public StudentModel() : base(connStr) { }

        public void Insert()
        {
            string cmdText = "insert into students(student_name, enrolled_dept) values 								 (@studentName, @enrolledDept)";
            Exec(
                query: cmdText, 
                parameterValues: new { @studentName = student_name, @enrolledDept = enrolled_dept}
            );
        }
        
        public void Update()
        {
            string cmdText = "update students set student_name=@studentName, 										 enrolled_dept=@enrolledDept where id=@Id";
            Exec(
                query: cmdText, 
                parameterValues: new { @studentName = student_name, @enrolledDept = enrolled_dept, @Id = id}
            );
        }
        
        public void Delete()
        {
            string cmdText = "delete from students where id = @Id";
            Exec(
                query: cmdText, 
                parameterValues: new { @Id = id}
            );
        }
    }
}
```

We declare parameter variables (eg. **@studentName, @enrolledDept**) in our SQL command text and later we set values (eg. **@studentName = student_name**). Here is an another example for reading table rows:

``` c#
using ModelMaker;

namespace modelMakerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            StudentModel student = new StudentModel();
            Console.WriteLine(student.SelectAll().FirstOrDefault().student_name);
            
            StudentModel student2 = new StudentModel();
            student2.enrolled_dept = "CSE";
            Console.WriteLine(student2.SelectByDept().FirstOrDefault().student_name); 
        }
    }

    class StudentModel : Model
    {
        public string id { get; set; }
        public string student_name { get; set; }
        public string enrolled_dept { get; set; }

        private static string connStr = "connection string";   

        public StudentModel() : base(connStr) { }

        public List<StudentModel> SelectAll(){
            string query = "select * from student";                     
            return Read<StudentModel>(query);
        }
        
        public List<StudentModel> SelectByDept(){
            string query = "select * from student where enrolled_dept=@Dept";             
            return Read<StudentModel>(query, new { @Dept = enrolled_dept});
        }
    }
}
```

**ModelMaker** supports **Automapping**. If you keep class property name same as table column then `Read<T>(string query)` will automatically return list of objects of that model type.  



### ModelMaker.EntityMap (string[] connections) Class

There will be situations when Automapping is not a solution. For instance, when we want to joint two tables or want to change out class property name which doesn't match with table column names. That time, our automapper will not work. We need to map our objects manually. 

We need to create an EntityMap object to map our class properties to table columns. We will use a special syntax for this: <br>
```class-property-name : table-column-name``` <br>
[this is for one connection]

We need an array of these connections to map all our properties to columns. Let's create one:

``` c#

string[] connections = new string[] {
  "id : id",
  "stuName : student_name",
  "enDept : enrolled_dept"
}

```

That's it!! Now let's read some data with ModelMaker:

``` c#
using ModelMaker;

namespace modelMakerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            StudentModel student = new StudentModel();
            Console.WriteLine(student.Select().FirstOrDefault().student_name);           
        }
    }

    class StudentModel : Model
    {
        public string id { get; set; }
        public string stuName { get; set; }
        public string enDept { get; set; }

        private static string connStr = "connection string";   

        public StudentModel() : base(connStr) { }

        public void Save()
        {
            string query = "insert into.....";
            Exec(query);
        }
        
        public List<StudentModel> SelectByDept(){
            string query = "select * from student where enrolled_dept=@Dept";
            string[] connections = new string[] {
              "id : id",
              "stuName : student_name",
              "enDept : enrolled_dept"
            }
			EntityMap map = new EntityMap(connections);          
            return Read<StudentModel>(query, new { @Dept = enrolled_dept}, map);
        }
    }
}
```

### ModelMaker.Model.getStatus() [int] Method
It's really necessary to check whether we are successfully connected to our database or not. ModelMaker also provides a method for that. Let's make changes to our previous example: 

``` c#

class Program
    {
        static void Main(string[] args)
        {
            StudentModel student = new StudentModel();
            if(student.getStatus() == 200){
                Console.WriteLine(student.Select().FirstOrDefault().student_name);
            }
            else{
                Console.WriteLine("Failed to connect with database!");
            }
                       
        }
    }
    
```

This method returns two interger values:
1. ``200`` if a successful connection occurs
2. ``500`` if it fails to connect to databse

That's it! Simple, Lightweight and Pure ADO.NET :smile:

