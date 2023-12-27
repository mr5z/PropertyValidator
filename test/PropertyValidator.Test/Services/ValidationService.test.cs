using CrossUtility.Extensions;
using PropertyValidator.Exceptions;
using PropertyValidator.Models;
using PropertyValidator.Services;
using PropertyValidator.ValidationPack;

namespace PropertyValidator.Test;

public class Tests
{
    public class DummyViewModel : INotifiableModel, System.ComponentModel.INotifyPropertyChanged
    {
        public void NotifyErrorPropertyChanged() { }
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        public string? Value { get; set; }
    }
    
    private IValidationService validationService = null!;
    
    [SetUp]
    public void Setup()
    {
        validationService = new ValidationService();
    }

    [Test, Description("Validate() must throw Exception if not setup properly.")]
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
            Assert.That(ex2, Is.TypeOf<InvalidOperationException>());
        }
    }

    [Test, Description("Calling For() multiple times should throw an Exception.")]
    public void MustThrowForMultipleCalls()
    {
        var ex1 = new Exception("This shouldn't be thrown.");
        try
        {
            validationService.For(new DummyViewModel());
            validationService.For(new DummyViewModel());
            validationService.For(new DummyViewModel());
            throw ex1;
        }
        catch (Exception ex2)
        {
            Assert.That(ex2, Is.Not.EqualTo(ex1));
            Assert.That(ex2, Is.TypeOf<InvalidOperationException>());
        }
    }

    [Test, Description("Validate() must not throw Exception if setup properly.")]
    public void MustNotThrowWhenConfigured()
    {
        var ex1 = new Exception("This should be thrown.");
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

    [Test, Description("Validate() must return false if property rule is violated.")]
    public void MustValidatePropertyRuleToFalse()
    {
        var vm = new DummyViewModel();
        
        validationService
            .For(vm)
            .AddRule(e => e.Value, new StringRequiredRule());

        var result = validationService.Validate();
        
        Assert.That(result, Is.EqualTo(false));
    }

    [Test, Description("Validate() must return true if property rule is not violated.")]
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

    [Test, Description("Validate() must validate for StringRequired() rule.")]
    [TestCaseSource(nameof(DummyViewModelFixturesStringRequiredRule))]
    public void MustValidateStringRequiredRule(DummyViewModel vm)
    {
        validationService
            .For(vm)
            .AddRule(e => e.Value, new StringRequiredRule());

        var result = validationService.Validate();
        
        Assert.That(result, Is.EqualTo(true));
    }

    [Test, Description("Validate() must return true with RangeLengthRule() rule.")]
    [TestCaseSource(nameof(DummyViewModelFixturesRangeLengthRule))]
    public void MustValidateRangeLengthRule(DummyViewModel vm, RangeLengthRule rule)
    {
        validationService
            .For(vm)
            .AddRule(e => e.Value, rule);

        var result = validationService.Validate();
        
        Assert.That(result, Is.EqualTo(true), $"Didn't succeed because vm.Value: '{vm.Value}', length: {vm.Value?.Length}");
    }

    [Test, Description("PropertyInvalid must be invoked upon violation of property rules.")]
    public async Task MustInvokePropertyInvalidEvent()
    {
        var vm = new DummyViewModel { Value = "something" };
        
        validationService
            .For(vm)
            .AddRule(e => e.Value, new StringRequiredRule());

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var tcs = new TaskCompletionSource<bool>();
        cts.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

        validationService.PropertyInvalid += PropertyInvalid;
        
        vm.Value = null;

        var result = await tcs.Task;

        void PropertyInvalid(object? sender, ValidationResultArgs e)
        {
            tcs.SetResult(e.HasError);
        }

        Assert.That(result, Is.EqualTo(true), $"Didn't succeed because result from PropertyInvalid is not expected.");
    }
    
    [Test, Description("EnsurePropertiesAreValid() must throw Exception.")]
    [TestCaseSource(nameof(DummyViewModelFixturesXRule))]
    public void MustEnsurePropertiesAreInvalid(DummyViewModel vm, IValidationRule rule)
    {
        validationService
            .For(vm)
            .AddRule(e => e.Value, rule);

        bool result;

        try
        {
            validationService.EnsurePropertiesAreValid();
            result = false;
        }
        catch (Exception e)
        {
            result = e is PropertyException;
        }
        
        Assert.That(result, Is.EqualTo(true), $"Didn't succeed because EnsurePropertiesAreValid() didn't throw.");
    }

    [Test, Description("GetErrors() must include error messages from registered property rule.")]
    public void MustFetchCorrectErrorMessages()
    {
        var vm = new DummyViewModel();
        var rule = new StringRequiredRule();

        validationService.For(vm)
            .AddRule(e => vm.Value, rule);

        _ = validationService.Validate();

        var errorMessages = validationService.GetErrors()
            .Where(e => !string.IsNullOrEmpty(e.Value))
            .Select(kvp => kvp.Value);

        Assert.That(errorMessages, Contains.Item(rule.ErrorMessage));
    }

    [Test, Description("EnsurePropertiesAreValid() must throw and contains expected ValidationResultArgs.")]
    public void MustContainExpectedValidationResultArgs()
    {
        var vm = new DummyViewModel();
        var rule = new StringRequiredRule();

        validationService.For(vm)
            .AddRule(e => vm.Value, rule);

        var notEx = new Exception("this shouldn't be thrown");
        try
        {
            validationService.EnsurePropertiesAreValid();
            throw notEx;
        }
        catch (Exception ex)
        {
            Assert.That(ex, Is.Not.EqualTo(notEx));
            Assert.That(ex, Is.TypeOf<PropertyException>());
            
            var pEx = (PropertyException)ex;
            var result = pEx.ValidationResultArgs;
            Assert.Multiple(() =>
            {
                Assert.That(result.HasError, Is.EqualTo(true));
                Assert.That(result.FirstError, Is.Not.Null);
                Assert.That(result.ErrorMessages, Contains.Item(rule.ErrorMessage));
                Assert.That(result.ErrorDictionary, Contains.Key(nameof(DummyViewModel.Value)));
                Assert.That(result.ErrorDictionary, Contains.Value(new [] { rule.ErrorMessage }));
            });
        }
    }
    
    [Test, Description("SetErrorFormatter() must modify the GetErrors() format.")]
    public void MustFollowErrorMessageFormat()
    {
        var vm = new DummyViewModel();
        var rule = new StringRequiredRule();
        validationService.For(vm).AddRule(e => e.Value, rule);
        validationService.SetErrorFormatter(errorMessages 
            => "<error>" + errorMessages.FirstOrDefault() + "</error>"
        );

        _ = validationService.Validate();
        var errorMessages = validationService.GetErrors();
        
        Assert.That(errorMessages, Contains.Value("<error>" + rule.ErrorMessage + "</error>"));
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

    private static IEnumerable<TestCaseData> DummyViewModelFixturesXRule
    {
        get
        {
            yield return new TestCaseData(new DummyViewModel { Value = null }, new StringRequiredRule());
            yield return new TestCaseData(new DummyViewModel { Value = "1234" }, new RangeLengthRule(5, 10));
            yield return new TestCaseData(new DummyViewModel { Value = "12345+1" }, new MaxLengthRule(5));
            yield return new TestCaseData(new DummyViewModel { Value = "123" }, new MinLengthRule(5));
        }
    }
}