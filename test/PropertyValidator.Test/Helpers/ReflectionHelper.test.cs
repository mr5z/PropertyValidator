namespace PropertyValidator.Test.Helpers;

public class ReflectionHelper
{
    private class Dummy
    {
        private readonly int x = 1;
        private readonly string y = "2";

        public int GetX() => x;
        public string GetY() => y;
    }
    
    [Test, Description("Must retrieve the fields and set their values.")]
    public void GetFieldTest()
    {
        var propX = PropertyValidator.Helpers.ReflectionHelper.GetField(typeof(Dummy), "x");
        var propY = PropertyValidator.Helpers.ReflectionHelper.GetField(typeof(Dummy), "y");
        
        Assert.Multiple(() =>
        {
            Assert.That(propX, Is.Not.Null);
            Assert.That(propY, Is.Not.Null);
        });
        
        Dummy dummy = new();
        propX?.SetValue(dummy, 2);
        propY?.SetValue(dummy, "1");
        
        Assert.Multiple(() =>
        {
            Assert.That(dummy.GetX(), Is.EqualTo(2));
            Assert.That(dummy.GetY(), Is.EqualTo("1"));
        });
    }
}