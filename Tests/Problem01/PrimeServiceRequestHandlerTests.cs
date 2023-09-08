using System.Text;
using ProtoHackers.Problem01;

namespace Tests.Problem01;

public class PrimeServiceRequestHandlerTests
{
    [TestCase(9, false)] 
    [TestCase(97,true)]    
    [TestCase(97.77,false)]
    [TestCase(97.00,true)] 
    public void IsPrimeTest(decimal number, bool expected)
    {
        var res = PrimeServiceRequestHandler.IsPrime(number);
        
       Assert.AreEqual(res, expected);
    }

    [Test]
    public void ConformedRequestHandlerTest()
    {
        var sut = new PrimeServiceRequestHandler();
        const string jsonReq = "{\"method\":\"isPrime\",\"number\":97}";
        var conformedRequest = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(jsonReq));
        
        var res = sut.HandleRequest(conformedRequest);

        Assert.True(res.Prime);
        Assert.AreEqual("isPrime", res.Method);
    }
    
    [TestCase("{method\":\"isPrime\",\"number\":97}")]
    [TestCase("{\"method\":\"isPrime\",\"number\":97")]
    [TestCase("{\"wrongmethod\":\"isPrime\",\"number\":97")]
    [TestCase("{\"method\":\"isPrime\",\"number\":\"97\"")]
    public void NotConformedJsonFormatRequestHandlerTest(string json)
    {
        var sut = new PrimeServiceRequestHandler();
        var notConformedRequest = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));

        var res = Assert.Throws<JsonFormatException>(() => sut.HandleRequest(notConformedRequest));
        Assert.AreEqual(res.Message, "Invalid json format.");
    }
    
    [Test]
    public void MalformedRequestHandlerTest()
    {
        const string malformedRequest = "{\"method\":\"notCorrectMethod\",\"number\":97}";
        var sut = new PrimeServiceRequestHandler();
        var notConformedRequest = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(malformedRequest));

        var res = Assert.Throws<MalformedRequestException>(() => sut.HandleRequest(notConformedRequest));
        Assert.AreEqual(res.Message, "Incorrect method.");
    }
}