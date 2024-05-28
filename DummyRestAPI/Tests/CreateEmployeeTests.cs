using RA;
using DummyRestAPI.Objects;
using System.Text.Json;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace DummyRestAPI.Tests;

[TestFixture]
public class CreateEmployeeTests : TestsBase
{
    public string CreateEmployeeUri = "/v1/create";
    public string MaxStrings = Utilities.GenerateRandomtrings(256);

    //Assume name can be duplicated, otherwise needs a random name and a dedicated duplicate name test
    [TestCase("Create employee - happy path", "testing", "100000", "20", 201)]
    [TestCase("Create employee - name empty", "", "100000", "20", 400)]
    [TestCase("Create employee - name utf-8", "测试", "100000", "20", 200)]
    [TestCase("Create employee - name with number", "123testing", "100000", "20", 400)]
    [TestCase("Create employee - name starts with specialcharacter", "_testing", "100000", "20", 201)]
    [TestCase("Create employee - salary empty", "testing", "", "20", 201)]
    [TestCase("Create employee - salary -1", "testing", "-1", "20", 400)]
    [TestCase("Create employee - salary with decimal", "testing", "100000.23", "20", 201)]
    [TestCase("Create employee - salary starts with special character", "testing", "$100000", "20", 400)]
    [TestCase("Create employee - salary invalid", "testing", "abcd_@#$2", "20", 400)]
    [TestCase("Create employee - age empty", "testing", "100000", "", 201)]
    [TestCase("Create employee - age -1", "testing", "100000", "-1", 400)]
    [TestCase("Create employee - age >150", "testing", "100000", "151", 400)]
    [TestCase("Create employee - age with decimal", "testing", "100000", "20.01", 201)]
    [TestCase("Create employee - age starts with special character", "testing", "100000", "_!@#$%20", 400)]
    [TestCase("Create employee - age invalid", "testing", "10000", "abcded", 400)]
    public void CreateEmployeeTest(string testName, string name, string salary, string age, int expectedCode)
    {
        var url = $"{BaseUrl}{CreateEmployeeUri}";

        var employeePayloads = new EmployeePayload
        {
            name = name,
            salary = salary,
            age= age
        };
        string requestBody = JsonSerializer.Serialize(employeePayloads);

        var builder = new RestAssured();
        var response = builder
            .Given()
                .Name(testName)
                .Timeout(standardTimeout)
                .Header("Content-Type", "application/json")
                .Body(requestBody)
            .When()
                .Post(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == expectedCode);
        if (expectedCode == 201)
        {
            response.TestBody("response status", y => y.status == "success")
                    .TestBody("response data id", z => z.data[0].id > 0)
                    .TestBody("response data name", z => z.data[0].name == name)
                    .TestBody("response data salary", z => z.data[0].salary == salary)
                    .TestBody("response data age", z => z.data[0].age == age)
                    .AssertAll();
        }
        else
        {
            response.AssertAll();
        }
    }

    [TestCase("Create employee - missing name", null, "100000", "20", 400)]
    [TestCase("Create employee - missing salary", "testing", null, "20", 400)]
    [TestCase("Create employee - missing age", "testing", "1000000", null, 400)]
    public void CreateEmployeeMissingFieldTest(string testName, string? name, string? salary, string? age, int expectedCode)
    {
        var url = $"{BaseUrl}{CreateEmployeeUri}";

        var payLoadDict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(name))
        {
            payLoadDict.Add("name", name);
        }
        if (!string.IsNullOrEmpty(salary))
        {
            payLoadDict.Add("salary", salary);
        }
        if (!string.IsNullOrEmpty(age))
        {
            payLoadDict.Add("age", age);
        }
        string requestBody = Newtonsoft.Json.JsonConvert.SerializeObject(payLoadDict);

        var builder = new RestAssured();
        builder
            .Given()
                .Name(testName)
                .Timeout(standardTimeout)
                .Header("Content-Type", "application/json")
                .Body(requestBody)
            .When()
                .Post(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == expectedCode)
                .AssertAll();
    }

    [TestCase("Create employee - null name", null, "100000", "20", 400)]
    [TestCase("Create employee - null salary", "testing", null, "20", 400)]
    [TestCase("Create employee - null age", "testing", "1000000", null, 400)]
    public void CreateEmployeeNullFieldTest(string testName, string? name, string? salary, string? age, int expectedCode)
    {
        var url = $"{BaseUrl}{CreateEmployeeUri}";

        var payLoadDict = new Dictionary<string, string>
        {
            {"name", name },
            {"salary", salary },
            {"age", age }
        };

        string requestBody = Newtonsoft.Json.JsonConvert.SerializeObject(payLoadDict);

        var builder = new RestAssured();
        builder
            .Given()
                .Name(testName)
                .Timeout(standardTimeout)
                .Header("Content-Type", "application/json")
                .Body(requestBody)
            .When()
                .Post(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == expectedCode)
                .AssertAll();
    }

    [TestCase("Create employee - name more than 255 characters", ">max", "100000", "20", 400)]
    [TestCase("Create employee - alary more than 255 characters", "testing", ">max", "20", 400)]
    [TestCase("Create employee - age more than 255 characters", "testing", ">max", "20", 400)]
    public void CreateEmployeeMoreThanMaxTest(string testName, string name, string salary, string age, int expectedCode)
    {
        var url = $"{BaseUrl}{CreateEmployeeUri}";

        var payLoadDict = new Dictionary<string, string>
        {
            {"name", name },
            {"salary", salary },
            {"age", age }
        };

        if (name.Equals(">max"))
        {
            payLoadDict[name] = Utilities.GenerateRandomtrings(256);
        }
        if (salary.Equals(">max"))
        {
            payLoadDict[salary] = Utilities.GenerateRandomtrings(256);
        }
        if (age.Equals(">max"))
        {
            payLoadDict[age] = Utilities.GenerateRandomtrings(256);
        }

        string requestBody = Newtonsoft.Json.JsonConvert.SerializeObject(payLoadDict);

        var builder = new RestAssured();
        builder
            .Given()
                .Name(testName)
                .Timeout(standardTimeout)
                .Header("Content-Type", "application/json")
                .Body(requestBody)
            .When()
                .Post(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == expectedCode)
                .AssertAll();
    }

    [TestCase("Create employee - invalid field", "testing", "1000000", null, 400)]
    public void CreateEmployeeInvalidPayloadFieldTest(string testName, string? name, string? salary, string? age, int expectedCode)
    {
        var url = $"{BaseUrl}{CreateEmployeeUri}";

        var payLoadDict = new Dictionary<string, string>
        {
            {"name", name },
            {"salary", salary },
            {"age", age },
            {"xyz", "xyz" }
        };

        string requestBody = Newtonsoft.Json.JsonConvert.SerializeObject(payLoadDict);

        var builder = new RestAssured();
        builder
            .Given()
                .Name(testName)
                .Timeout(standardTimeout)
                .Header("Content-Type", "application/json")
                .Body(requestBody)
            .When()
                .Post(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == expectedCode)
                .AssertAll();
    }


    [Test]
    public void UpdateEmployeeTooManyRequestTest()
    {
        var url = $"{BaseUrl}{CreateEmployeeUri}";

        var employeePayloads = new EmployeePayload
        {
            name = "testing429",
            salary = "10000",
            age = "20"
        };
        string requestBody = JsonSerializer.Serialize(employeePayloads);

        var builder = new RestAssured();
        for (int i = 0; i < 5; i++)
        {
            try
            {
                builder
                    .Given()
                        .Name("Create Employee - too many requests")
                        .Timeout(standardTimeout)
                        .Header("Content-Type", "application/json")
                        .Body(requestBody)
                    .When()
                        .Post(url)
                    .Then()
                        .Debug()
                        .TestStatus("response code", x => x == 429)
                        .AssertAll();
                Assert.Pass();
            }
            catch (AssertionException)
            {
                continue;
            }
        }
        Assert.Fail("Failed to get too many requests return inside request rate.");
    }
}