using Newtonsoft.Json;
using RA;
using DummyRestAPI.Objects;

namespace DummyRestAPI.Tests;

[TestFixture]
public class EmployeesTests : TestsBase
{
    public string EmployeesUri = "/v1/employees";

    [Test]
    public void EmployeesTest()
    {
        var url = $"{BaseUrl}{EmployeesUri}";

        var builder = new RestAssured();
        builder
            .Given()
                .Name("Get employees- Happy Path")
                .Host(BaseUrl)
                .Timeout(standardTimeout)
                .Header("Content-Type", "application/json")
            .When()
               .Get(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == 200)
                .TestBody("response status", y => y.status == "success")
                .TestBody("response data id", z => z.data[0].id >0)
                .TestBody("response data name", z => z.data[0].employee_name != null)
                .TestBody("response data salary", z => z.data[0].employee_salary >0)
                .TestBody("response data age", z => z.data[0].employee_age >0)
                .TestBody("response data profile image", z => z.data[0].profile_image != null)
                .AssertAll();
    }

    [Test]
    public void EmployeesBadRequestTest()
    {
        var url = $"{BaseUrl}{EmployeesUri}";

        var builder = new RestAssured();
        builder
            .Given()
                .Name("Get employees- Bad Request")
                .Timeout(standardTimeout)
                .Header("Content-Type", "application/x-www-form-urlencoded")
            .When()
                .Get(url)
            .Then()
                .Debug()
                .TestStatus("response code", x => x == 400)
                .AssertAll();
    }

    [Test]
    public void EmployeesTooManyRequestsTest()
    {
        var url = $"{BaseUrl}{EmployeesUri}";

        var builder = new RestAssured();
        for (int i = 0; i < 5; i++)
        {
            try
            {
                builder
                    .Given()
                        .Name("Get employees- Too Many Requests")
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