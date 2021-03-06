# Stardust.Paradox
Entity framework'ish tool for developing .net applications using gremlin graph query language with CosmosDb

# Usage (asp.net core)

## Startup.cs
### ConfigureServices
Add the generated entity implementations to the IOC container (I will provide an extention method to make this easier)
```cs
 services.AddEntityBinding((entityType, entityImplementation) => services.AddTransient(entityType, entityImplementation))
        .AddScoped<MyEntityContext,MyEntityContext>()
        .AddScoped<IGremlinLanguageConnector>(s => new CosmosDbLanguageConnector(DbAccountName, AccessKey, "databaseName","collectionName"));
```

## Defining the model

```cs
[VertexLabel("person")]
public interface IPerson : IVertex
{
    string Id {get;}

    string FirstName { get; set; }

    string LastName { get; set; }
    
    string Email { get; set; }

    IEdgeCollection<IPerson> Parents { get; }

    IEdgeCollection<IPerson> Children { get; }

    IEdgeCollection<IPerson> Siblings { get; }

    [EdgeLabel("city")] //pointing to the Residents property in ICity
    IEdgeReference<ICity> HomeCity { get; }//use IEdgeReference to enable task-async operations

    IEdgeCollection<ICompany> Employers { get; }
}

[VertexLabel("city")]
public interface ICity : IVertex
{
    string Id { get; }

    string Name { get; set; }

    string ZipCode { get; set; }
    

   [ReverseEdgeLabel("city")] //pointing to the HomCity property in IPerson
    IEdgeCollection<Iperson> Residents { get; } //use IEdgeCollection to enable task-async operations on the collection

    IEdgeReference<ICountry> Country { get; }
}

[VertexLabel("company")]
public interface ICompany : IVertex
{
    string Id { get; }

    string Name { get; set; }

    IEdgeCollection<ICompany> Employees { get; }
}

[VertexLabel("country")]
public interface ICountry : IVertex
{
    string Id { get; }

    string Name { get; set; }

    string CountryCode { get; set; }

    string PhoneNoPrefix { get; set; }

    IEdgeCollection<ICity> Cities { get; }
}

 [EdgeLabel("employer")]
    public interface IEmployment : IEdge<IProfile, ICompany>
    {
        string Id { get; }

        DateTime HiredDate { get; set; }

        string Manager { get; set; }
    }

```

## Defining the entity context and generating the entity implementations

```cs
public class MyEntityContext : Stardust.Paradox.Data.GraphContextBase
{
    public IGraphSet<IPerson> Persons => GraphSet<IPerson>();

    public IGraphSet<ICity> Cities => GraphSet<ICity>();

    public IGraphSet<ICountry> Countries => GraphSet<ICountry>();

    public IGraphSet<ICompany> Companies => GraphSet<ICompany>();

    public IGraphSet<IEmployment> Employments => EdgeGraphSet<IEmployment>();

    public MyEntityContext(IGremlinLanguageConnector connector, IServiceProvider resolver) : base(connector, resolver)
    {
    }

    protected override bool InitializeModel(IGraphConfiguration configuration)
    {
        //Added some fluent configuration of the edges
        configuration.ConfigureCollection<IPerson>()
                .AddEdge(t => t.Children, "children").Reverse<IPerson>(t => t.Parents)
            .ConfigureCollection<ICity>()
            .ConfigureCollection<ICountry>()
                .AddEdge(t=>t.Cities).Reverse<ICountry>(t=>t.Country)
            .ConfigureCollection<ICompany>()
                .AddEdge(t => t.Employees, "employees").Reverse<IPerson>(t => t.Employers)
                .ConfigureCollection<IEmployment>();;
        return true;
    }
}

```
## Using the datacontext

```CS

public class DemoController:Controller
{
    private MyEntityContext _dataContext;
    public DemoController(MyEntityContext dataContext)
    {
        _dataContext=dataContext;
    }

    public Task<IActionResult> GetDataAsync(string personId)
    {
        var person=await _dataContext.Persons.GetAsync(persionId);
        return new User
        {
            Id=person.Id,
            FirstName=person.FistName,
            LastName=person.LastName,
            Email=person.Email
        }
    }

    public Task<IActionResult> AddEmploymentAsync(string userId, string companyId,DateTime hiredDate, string managerName) //new in V2
    {
        var user=await await _dataContext.Persons.GetAsync(persionId);
        var company=await _dataContext.Companies.GetAsync(companyId);
        var e= _dataContext.Employments.Create(user,company);
        e.HiredDate=hiredDate;
        e.ManagerName=managerName;
        await _dataContext.SaveChangesAsync();
        //alternative
        
    }

    public Task<IActionResult> AddEmploymentAlternativeAsync(string userId, string companyId,DateTime hiredDate, string managerName) //edge property handling in V1
    {
        var user=await await _dataContext.Persons.GetAsync(persionId);
        var company=await _dataContext.Companies.GetAsync(companyId);
        user.Employers.Add(company,new Dictionary<string,object>{
            {"hiredDate",hiredDate},
            {"managerName","managerName"}
        })
        await _dataContext.SaveChangesAsync();
        //alternative
        
    }
}

```


