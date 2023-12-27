using PropertyValidator.Models;

namespace PropertyValidator.Test.Exceptions;

public class PropertyException
{
    [Test, Description("Must be able to get the empty ValidationResultArgs.")]
    public void MustGetEmptyResultArgs()
    {
        var exception =
            new PropertyValidator.Exceptions.PropertyException(
                new ValidationResultArgs(
                    new Dictionary<string, IEnumerable<string?>>()
                )
            );
        Assert.Multiple(() =>
        {
            Assert.That(exception.ValidationResultArgs.HasError, Is.EqualTo(false));
            Assert.That(exception.ValidationResultArgs.FirstError, Is.Null);
            Assert.That(exception.ValidationResultArgs.ErrorMessages, Is.Null);
            Assert.That(exception.ValidationResultArgs.ErrorDictionary, Is.Empty);
        });
    }
    
    [Test, Description("Must be able to get the non-empty ValidationResultArgs.")]
    [TestCase("A", new[] { "1st", "2nd" })]
    [TestCase("B", new[] { "1st", "2nd", "3rd" })]
    [TestCase("C", new[] { "1st", "2nd", "3rd", "4th" })]
    public void MustGetNonEmptyResultArgs(string propertyName, IEnumerable<string> errorMessages)
    {
        var exception =
            new PropertyValidator.Exceptions.PropertyException(
                new ValidationResultArgs(
                    new Dictionary<string, IEnumerable<string?>>
                    {
                        { propertyName, errorMessages }
                    }
                )
            );
        
        Assert.Multiple(() =>
        {
            Assert.That(exception.ValidationResultArgs.HasError, Is.EqualTo(true));
            Assert.That(exception.ValidationResultArgs.FirstError, Is.Not.Null);
            Assert.That(exception.ValidationResultArgs.ErrorMessages, Is.Not.Null);
            Assert.That(exception.ValidationResultArgs.ErrorDictionary, Is.Not.Empty);
            Assert.That(exception.ValidationResultArgs.FirstError, Is.EqualTo("1st"));
            Assert.That(exception.ValidationResultArgs.ErrorMessages, Contains.Item("2nd"));
            Assert.That(exception.ValidationResultArgs.ErrorDictionary.Values.SelectMany(x => x), Contains.Item("2nd"));
        });
    }
}