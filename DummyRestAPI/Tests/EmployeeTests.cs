using RA;

namespace DummyRestAPI.Tests;

[TestFixture]
public class EmployeeTests : TestsBase
{
    public string EmployeeUri = "/v1/employee/";

    [TestCase("Get Employee - Happy Path", "1")]
    public void EmployeeTest(string testName, string employeeId)
    {
        var url = $"{BaseUrl}{EmployeeUri}{employeeId}";

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
                .TestStatus("response code", x => x == 200)
                .TestBody("response status", y => y.status == "success")
                .TestBody("response data id", z => z.data[0].id != null)
                .TestBody("response data name", z => z.data[0].employee_name != null)
                .TestBody("response data salary", z => z.data[0].employee_salary != null)
                .TestBody("response data age", z => z.data[0].employee_age != null)
                .TestBody("response data profile image", z => z.data[0].profile_image != null)
                .AssertAll();
    }

    [TestCase("Get Employee - Invalid Id", "0")]
    [TestCase("Get Employee - Id not a number", "s")]
    public void EmployeeNegativeTest(string testName, string employeeId)
    {
        var url = $"{BaseUrl}{EmployeeUri}{employeeId}";

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
    public void EmployeeIdNullTest()
    {
        var url = $"{BaseUrl}{EmployeeUri}";

        var builder = new RestAssured();
        builder
            .Given()
                .Name("Get Employee - id is null")
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

    [TestCase("Get Employee - Too many requests", "1")]
    public void EmployeeTooManyRequestsTest(string testName, string employeeId)
    {
        var url = $"{BaseUrl}{EmployeeUri}{employeeId}";

        var builder = new RestAssured();
        for (int i = 0; i < 5; i++)
        {
            try
            {
                builder
                    .Given()
                        .Name(testName)
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