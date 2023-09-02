using ProtoHackers.Problem01;

namespace Tests;

public class Problem01Tests
{
    [TestCase(9, false)] 
    [TestCase(97,true)]    
    [TestCase(97.77,false)]
    [TestCase(97.00,true)] 
    public async Task IsPrimeTest(decimal number, bool expected)
    {
        var sut = new PrimeService();

        var res = sut.IsPrime(number);
        
        Assert.AreEqual(expected, res);
    }
}