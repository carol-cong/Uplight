using System;
using System.Text.Json;
using System.Xml.Linq;
using DummyRestAPI.Objects;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using RA;

namespace DummyRestAPI.Tests;

[TestFixture]
public class UpdateEmployeeTests : TestsBase
{
    public string UpdateUri = "/v1/update/";
    public string MaxStrings = Utilities.GenerateRandomtrings(256);
    public string EmployeeId;

    [OneTimeSetUp]
    public void Setup()
    {
        var employeePayloads = new EmployeePayload
        {
            name = "testingOriginal",
            salary = "98765",
            age = "70"
        };
        string requestBody = JsonSerializer.Serialize(employeePayloads);

        var builder = new RestAssured();
        var response = builder
            .Given()
                .Name("One Time setup to create an employee for update test")
                .Timeout(standardTimeout)
                .Header("Content-Type", "application/json")
                .Body(requestBody)
            .When()
                .Post($"{BaseUrl}/v1/create")
            .Then()
                .Debug()
                .TestStatus("response code", x => x == 200)
                .Retrieve(y => y.data.id);
        if (string.IsNullOrEmpty(response.ToString()))
        {
            Assert.Fail("Fail to create a new employee for update test");
        }
        EmployeeId = response.ToString();
    }

    //Assume name can be duplicated, otherwise needs a random name and a dedicated duplicate name test
    [TestCase("Update employee - happy path", "testing", "100000", "20", 200)]
    [TestCase("Update employee - name empty", "", "100000", "20", 400)]
    [TestCase("Update employee - name utf-8", "测试", "100000", "20", 200)]
    [TestCase("Update employee - name with number", "123testing", "100000", "20", 400)]
    [TestCase("Update employee - name starts with specialcharacter", "_testing", "100000", "20", 200)]
    [TestCase("Update employee - salary empty", "testing", "", "20", 200)]
    [TestCase("Update employee - salary -1", "testing", "-1", "20", 400)]
    [TestCase("Update employee - salary with decimal", "testing", "100000.23", "20", 200)]
    [TestCase("Update employee - salary starts with special character", "testing", "$100000", "20", 400)]
    [TestCase("Update employee - salary invalid", "testing", "abcd_@#$2", "20", 400)]
    [TestCase("Update employee - age empty", "testing", "100000", "", 200)]
    [TestCase("Update employee - age -1", "testing", "100000", "-1", 400)]
    [TestCase("Update employee - age >150", "testing", "100000", "151", 400)]
    [TestCase("Update employee - age with decimal", "testing", "100000", "20.01", 200)]
    [TestCase("Update employee - age starts with special character", "testing", "100000", "_!@#$%20", 400)]
    [TestCase("Update employee - age invalid", "testing", "10000", "abcded", 400)]
    public void UpdateEmployeeTest(string testName, string name, string salary, string age, int expectedCode)
    {
        var url = $"{BaseUrl}{UpdateUri}{EmployeeId}";

        var employeePayloads = new EmployeePayload
        {
            name = name,
            salary = salary,
            age = age
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
                .Put(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == expectedCode);
        if (expectedCode == 200)
        {
            response.TestBody("response status", y => y.status == "success")
                    .TestBody("response data id", z => z.data.id == Convert.ToUInt32(EmployeeId))
                    .TestBody("response data name", z => z.data.name == name)
                    .TestBody("response data salary", z => z.data.salary == salary)
                    .TestBody("response data age", z => z.data.age == age)
                    .AssertAll();

            //Get employee by id to verify record updated
            builder
                .Given()
                    .Name($"{testName} - Validate")
                    .Timeout(standardTimeout)
                    .Header("Content-Type", "application/json")
                .When()
                    .Get($"{BaseUrl}/v1/employee/{EmployeeId}")
                .Then()
                    .Debug()
                    .TestStatus("response code", x => x == 200)
                    .TestBody("response data id", z => z.data[0].id == Convert.ToUInt32(EmployeeId))
                    .TestBody("response data name", z => z.data[0].employee_name == name)
                    .TestBody("response data salary", z => z.data[0].employee_salary == salary)
                    .TestBody("response data age", z => z.data[0].employee_age == age)
                    .AssertAll();
        }
        else
        {
            response.AssertAll();
        }
    }

    [TestCase("Update employee - missing name", null, "100000", "20", 400)]
    [TestCase("Update employee - missing salary", "testing", null, "20", 400)]
    [TestCase("Update employee - missing age", "testing", "1000000", null, 400)]
    public void UpdateEmployeeMissingFieldTest(string testName, string? name, string? salary, string? age, int expectedCode)
    {
        var url = $"{BaseUrl}{UpdateUri}{EmployeeId}";

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
                .Put(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == expectedCode)
                .AssertAll();
    }

    [TestCase("Update employee - null name", null, "100000", "20", 400)]
    [TestCase("Update employee - null salary", "testing", null, "20", 400)]
    [TestCase("Update employee - null age", "testing", "1000000", null, 400)]
    public void UpdateEmployeeNullFieldTest(string testName, string? name, string? salary, string? age, int expectedCode)
    {
        var url = $"{BaseUrl}{UpdateUri}{EmployeeId}";

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
                .Put(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == expectedCode)
                .AssertAll();
    }

    [TestCase("Update employee - name more than 255 characters", ">max", "100000", "20", 400)]
    [TestCase("Update employee - alary more than 255 characters", "testing", ">max", "20", 400)]
    [TestCase("Update employee - age more than 255 characters", "testing", ">max", "20", 400)]
    public void UpdateEmployeeMoreThanMaxTest(string testName, string name, string salary, string age, int expectedCode)
    {
        var url = $"{BaseUrl}{UpdateUri}{EmployeeId}";

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
                .Put(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == expectedCode)
                .AssertAll();
    }

    [TestCase("Update employee - invalid field", "testing", "1000000", null, 400)]
    public void UpdateEmployeeInvalidPayloadFieldTest(string testName, string? name, string? salary, string? age, int expectedCode)
    {
        var url = $"{BaseUrl}{UpdateUri}{EmployeeId}";

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
                .Put(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == expectedCode)
                .AssertAll();
    }


    [Test]
    public void UpdateEmployeeTooManyRequestTest()
    {
        var url = $"{BaseUrl}{UpdateUri}{EmployeeId}";

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
                        .Name("Update Employee - too many requests")
                        .Timeout(standardTimeout)
                        .Header("Content-Type", "application/json")
                        .Body(requestBody)
                    .When()
                        .Put(url)
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
    [TestCase("Update employee - Missing Id", "testing", "1000000", "30", 400)]
    public void UpdateEmployeeMissingIdTest(string testName, string? name, string? salary, string? age, int expectedCode)
    {
        var url = $"{BaseUrl}{UpdateUri}";

        var employeePayloads = new EmployeePayload
        {
            name = name,
            salary = salary,
            age = age
        };
        string requestBody = JsonSerializer.Serialize(employeePayloads);

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

    [TestCase("Update employee - Invaid Id", "testing", "1000000", "30", 400)]
    public void UpdateEmployeeInvalidMissingIdTest(string testName, string? name, string? salary, string? age, int expectedCode)
    {
        var invalidId = 2001;
        var url = $"{BaseUrl}{UpdateUri}{invalidId}";

        var employeePayloads = new EmployeePayload
        {
            name = name,
            salary = salary,
            age = age
        };
        string requestBody = JsonSerializer.Serialize(employeePayloads);


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

    [TestCase("Update employee - Id with strings", "testing", "1000000", "30", 400)]
    public void UpdateEmployeeInvalidMissingIdTest(string testName, string? name, string? salary, string? age, int expectedCode)
    {
        var invalidId = "#2012";
        var url = $"{BaseUrl}{UpdateUri}{invalidId}";

        var employeePayloads = new EmployeePayload
        {
            name = name,
            salary = salary,
            age = age
        };
        string requestBody = JsonSerializer.Serialize(employeePayloads);


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
}