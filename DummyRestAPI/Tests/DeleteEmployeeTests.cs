using System.Text.Json;
using DummyRestAPI.Objects;
using RA;

namespace DummyRestAPI.Tests;

[TestFixture]
public class DeleteEmployeeTests : TestsBase
{
    public string DeleteEmployeeUri = "/v1/delete/";
    public string EmployeeId;

    [SetUp]
    public void Setup()
    {
        var employeePayloads = new EmployeePayload
        {
            name = "testingOriginalForDelete",
            salary = "2345",
            age = "60"
        };
        string requestBody = JsonSerializer.Serialize(employeePayloads);


        var builder = new RestAssured();
        var response = builder
            .Given()
                .Name("One Time setup to create an employee for delete test")
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
            Assert.Fail("Fail to create a new employee for delete test");
        }
        EmployeeId = response.ToString();
    }

    public void DeleteEmployeeTest()
    {
        var url = $"{BaseUrl}{DeleteEmployeeUri}{EmployeeId}";

        var builder = new RestAssured();
        builder.Given()
                .Name("Delete Employee - Happy Path")
                .Timeout(standardTimeout)
                .Header("Content-Type", "application/json")
            .When()
                .Get(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == 200)
                .TestBody("response status", y => y.status == "success")
                .TestBody("response message", z => z.message == "successfully! deleted Records")
                .AssertAll();

        //Get employee by id to verify record deleted
        builder
            .Given()
                .Name("Delete Employee - Happy Path - Validate")
                .Timeout(standardTimeout)
                .Header("Content-Type", "application/json")
            .When()
                .Get($"{BaseUrl}/v1/employee/{EmployeeId}")
            .Then()
                .Debug()
                .TestStatus("response code", x => x == 400)
                .AssertAll();
    }

    [TestCase("Delete Employee - Invalid Id", "0")]
    [TestCase("Delete Employee - Id not a number", "s")]
    public void EmployeeNegativeTest(string testName, string employeeId)
    {
        var url = $"{BaseUrl}{DeleteEmployeeUri}{employeeId}";

        var builder = new RestAssured();
        builder
            .Given()
                .Name(testName)
                .Timeout(standardTimeout)
                .Header("Content-Type", "application/json")
            .When()
                .Get(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == 400)
                .AssertAll();
    }

    [Test]
    public void DeleteEmployeeIdNullTest()
    {
        var url = $"{BaseUrl}{DeleteEmployeeUri}";

        var builder = new RestAssured();
        builder
            .Given()
                .Name("Delete Employee - id is null")
                .Timeout(standardTimeout)
                .Header("Content-Type", "application/json")
            .When()
                .Get(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == 404)
                .TestBody("response status", y => y.message = "Error Occured! Page Not found, contact rstapi2example@gmail.com")
                .AssertAll();
    }

    public void EmployeeTooManyRequestsTest()
    {
        var url = $"{BaseUrl}{DeleteEmployeeUri}{EmployeeId}";

        var builder = new RestAssured();
        for (int i = 0; i < 5; i++)
        {
            try
            {
                builder
                    .Given()
                        .Name("Delete Employee - Too many requests")
                        .Timeout(standardTimeout)
                        .Header("Content-Type", "application/json")
                    .When()
                        .Get(url)
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