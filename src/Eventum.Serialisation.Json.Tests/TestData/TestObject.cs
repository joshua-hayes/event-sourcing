using Eventum.Serialisation.Attributes;

namespace Eventum.Serialisation.Json.TestData;

public class TestObject
{
    public string Property1 { get; set; }
    public string Property2 { get; set; }

    [IgnoreSerialization]
    public string Property3 { get; set; }
}