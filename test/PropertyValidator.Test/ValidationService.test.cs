using PropertyValidator.Models;
using PropertyValidator.Services;
using PropertyValidator.ValidationPack;

namespace PropertyValidator.Test;

public class Tests
{
    public class DummyViewModel : INotifiableModel, System.ComponentModel.INotifyPropertyChanged
    {
        public void NotifyErrorPropertyChanged() => throw new NotImplementedException();
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        public string? Value { get; set; }
    }
    
    private IValidationService validationService = null!;
    
    [SetUp]
    public void Setup()
    {
        validationService = new ValidationService();
    }

    [Test, Description("ValidationService.Validate() must throw Exception if not setup properly.")]
    public void MustThrowWhenNotConfigured()
    {
        var ex1 = new Exception("This shouldn't be thrown.");
        try
        {
            validationService.Validate();
            throw ex1;
        }
        catch (Exception ex2)
        {
            Assert.That(ex2, Is.Not.EqualTo(ex1));
        }
    }

    [Test, Description("ValidationService.Validate() must not throw Exception if setup properly.")]
    public void MustNotThrowWhenConfigured()
    {
        var ex1 = new Exception("This shouldn't be thrown.");
        try
        {
            validationService.For(new DummyViewModel());
            validationService.Validate();
            throw ex1;
        }
        catch (Exception ex2)
        {
            Assert.That(ex2, Is.EqualTo(ex1));
        }
    }

    [Test, Description("ValidationService.Validate() must return false if property rule is violated.")]
    public void MustValidatePropertyRuleToFalse()
    {
        var vm = new DummyViewModel();
        
        validationService
            .For(vm)
            .AddRule(e => e.Value, new StringRequiredRule());

        var result = validationService.Validate();
        
        Assert.That(result, Is.EqualTo(false));
    }

    [Test, Description("ValidationService.Validate() must return true if property rule is not violated.")]
    public void MustValidatePropertyRuleToTrue()
    {
        var vm = new DummyViewModel();
        
        validationService
            .For(vm)
            .AddRule(e => e.Value, new StringRequiredRule());

        vm.Value = "Something";
        var result = validationService.Validate();
        
        Assert.That(result, Is.EqualTo(true));
    }

    [Test, Description("ValidationService.Validate() must validate for StringRequired() rule.")]
    [TestCaseSource(nameof(DummyViewModelFixturesStringRequiredRule))]
    public void MustValidateStringRequiredRule(DummyViewModel vm)
    {
        validationService
            .For(vm)
            .AddRule(e => e.Value, new StringRequiredRule());

        var result = validationService.Validate();
        
        Assert.That(result, Is.EqualTo(true));
    }

    [Test, Description("ValidationService.Validate() must validate for RangeLengthRule() rule.")]
    [TestCaseSource(nameof(DummyViewModelFixturesRangeLengthRule))]
    public void MustValidateRangeLengthRule(DummyViewModel vm, RangeLengthRule rule)
    {
        validationService
            .For(vm)
            .AddRule(e => e.Value, rule);

        var result = validationService.Validate();
        
        Assert.That(result, Is.EqualTo(true), $"Didn't succeed because vb.Value: '{vm.Value}', length: {vm.Value?.Length}");
    }

    private static IEnumerable<TestCaseData> DummyViewModelFixturesStringRequiredRule
    {
        get
        {
            yield return new TestCaseData(new DummyViewModel { Value = " " });
            yield return new TestCaseData(new DummyViewModel { Value = "." });
            yield return new TestCaseData(new DummyViewModel { Value = "Test" });
        }
    }

    private static IEnumerable<TestCaseData> DummyViewModelFixturesRangeLengthRule
    {
        get
        {
            yield return new TestCaseData(new DummyViewModel { Value = "a" }, new RangeLengthRule(1, 1));
            yield return new TestCaseData(new DummyViewModel { Value = "12345" }, new RangeLengthRule(5, 10));
            yield return new TestCaseData(new DummyViewModel { Value = "1234567890+2" }, new RangeLengthRule(10, 20));
        }
    }
}