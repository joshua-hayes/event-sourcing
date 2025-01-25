using Moq;
using System.Reflection;

namespace Eventum.Persistence.DynamoDB.Tests;

public class TestHelper<T> where T : class
{
    private readonly List<object> _mocks;
    private readonly Dictionary<string, object> _params;
    private readonly ConstructorInfo _constructorInfo;

    public TestHelper()
    {
        _mocks = new List<object>();
        _params = new Dictionary<string, object>();
        _constructorInfo = typeof(T).GetConstructors().First();

        // Prepopulate dictionary with constructor parameter names
        foreach (var param in _constructorInfo.GetParameters())
            _params[param.Name] = null;
    }

    public Mock<TMock> Mock<TMock>() where TMock : class
    {
        var mockTypeName = typeof(TMock).Name;
        var paramList = _constructorInfo.GetParameters().ToList();
        var constructorArgName = paramList.FirstOrDefault(p => p.ParameterType.Name == mockTypeName)?.Name;

        if (string.IsNullOrEmpty(constructorArgName))
            throw new InvalidOperationException($"No constructor parameter of type {typeof(TMock).Name} found for {typeof(T).Name}");

        if (_params.ContainsKey(constructorArgName) && _params[constructorArgName] != null)
            return (Mock<TMock>)_params[constructorArgName];

        var mock = new Mock<TMock>();
        _mocks.Add(mock);
        _params[constructorArgName] = mock;
        return mock;
    }

    public void AddParam<TParam>(string name, TParam parameter)
    {
        if (!_params.ContainsKey(name))
            throw new InvalidOperationException($"No constructor parameter named {name} found for {typeof(T).Name}");

        _params[name] = parameter;
    }

    public T Build()
    {
        var parameters = _constructorInfo.GetParameters()
                                         .Select(param =>
                                             _params.ContainsKey(param.Name)
                                             ? (_params[param.Name] is Mock ? ((dynamic)_params[param.Name]).Object : _params[param.Name])
                                             : throw new InvalidOperationException($"No parameter registered for {param.Name}"))
                                         .ToArray();

        return (T)_constructorInfo.Invoke(parameters);
    }

    public void VerifyAll()
    {
        foreach (var mock in _mocks)
            ((Mock)mock).Verify();
    }
}
