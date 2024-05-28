namespace DummyRestAPI;

public class TestsBase
{
    public string BaseUrl = "https://dummy.restapiexample.com/api";
    public int standardTimeout = 30 * 1000;
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        TestContext.Out.WriteLine("Execution of Test suite DummyRestAPI starts");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        TestContext.Out.WriteLine("Execution of Test suite DummyRestAPI ends");
    }
}

