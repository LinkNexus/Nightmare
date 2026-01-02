using Nightmare.Parser;

namespace Nightmare.Tests.ParserTests;

public class JsonIdTests
{
    [Fact]
    public void Parse_RootValue_HasIdDollarSign()
    {
        var result = JsonParser.Parse("null");
        
        Assert.Equal("$", result.Id);
    }

    [Fact]
    public void Parse_RootBoolean_HasIdDollarSign()
    {
        var result = JsonParser.Parse("true");
        
        Assert.Equal("$", result.Id);
    }

    [Fact]
    public void Parse_RootNumber_HasIdDollarSign()
    {
        var result = JsonParser.Parse("42");
        
        Assert.Equal("$", result.Id);
    }

    [Fact]
    public void Parse_RootString_HasIdDollarSign()
    {
        var result = JsonParser.Parse("\"hello\"");
        
        Assert.Equal("$", result.Id);
    }

    [Fact]
    public void Parse_RootEmptyObject_HasIdDollarSign()
    {
        var result = JsonParser.Parse("{}");
        
        Assert.Equal("$", result.Id);
    }

    [Fact]
    public void Parse_RootEmptyArray_HasIdDollarSign()
    {
        var result = JsonParser.Parse("[]");
        
        Assert.Equal("$", result.Id);
    }

    [Fact]
    public void Parse_ObjectProperty_HasPropertyId()
    {
        var result = JsonParser.Parse("""{ "hello": "world" }""");
        var obj = Assert.IsType<JsonObject>(result);
        var property = obj.GetProperty("hello");
        
        Assert.NotNull(property);
        Assert.Equal("$.hello", property.Id);
    }

    [Fact]
    public void Parse_ObjectMultipleProperties_EachHasCorrectId()
    {
        var result = JsonParser.Parse("""{ "name": "John", "age": 30, "active": true }""");
        var obj = Assert.IsType<JsonObject>(result);
        
        Assert.Equal("$.name", obj.GetProperty("name").Id);
        Assert.Equal("$.age", obj.GetProperty("age").Id);
        Assert.Equal("$.active", obj.GetProperty("active").Id);
    }

    [Fact]
    public void Parse_NestedObject_HasNestedPropertyId()
    {
        var result = JsonParser.Parse("""{ "user": { "name": "John", "email": "john@example.com" } }""");
        var obj = Assert.IsType<JsonObject>(result);
        var user = Assert.IsType<JsonObject>(obj.GetProperty("user"));
        
        Assert.Equal("$.user", user.Id);
        Assert.Equal("$.user.name", user.GetProperty("name").Id);
        Assert.Equal("$.user.email", user.GetProperty("email").Id);
    }

    [Fact]
    public void Parse_DeeplyNestedObject_HasCorrectIds()
    {
        var result = JsonParser.Parse(
            """{ "level1": { "level2": { "level3": "value" } } }"""
        );
        var obj = Assert.IsType<JsonObject>(result);
        var level1 = Assert.IsType<JsonObject>(obj.GetProperty("level1"));
        var level2 = Assert.IsType<JsonObject>(level1.GetProperty("level2"));
        var value = level2.GetProperty("level3");
        
        Assert.Equal("$.level1", level1.Id);
        Assert.Equal("$.level1.level2", level2.Id);
        Assert.Equal("$.level1.level2.level3", value.Id);
    }

    [Fact]
    public void Parse_ArrayWithPrimitives_ItemsHaveIndexIds()
    {
        var result = JsonParser.Parse("""[ "a", "b", "c" ]""");
        var arr = Assert.IsType<JsonArray>(result);
        
        Assert.Equal("$", arr.Id);
        Assert.Equal("$[0]", arr.Items[0].Id);
        Assert.Equal("$[1]", arr.Items[1].Id);
        Assert.Equal("$[2]", arr.Items[2].Id);
    }

    [Fact]
    public void Parse_ArrayWithNumbers_ItemsHaveIndexIds()
    {
        var result = JsonParser.Parse("[10, 20, 30, 40]");
        var arr = Assert.IsType<JsonArray>(result);
        
        Assert.Equal("$[0]", arr.Items[0].Id);
        Assert.Equal("$[1]", arr.Items[1].Id);
        Assert.Equal("$[2]", arr.Items[2].Id);
        Assert.Equal("$[3]", arr.Items[3].Id);
    }

    [Fact]
    public void Parse_ArrayWithObjects_ItemsAndPropertiesHaveCorrectIds()
    {
        var result = JsonParser.Parse(
            """[ { "id": 1, "name": "Alice" }, { "id": 2, "name": "Bob" } ]"""
        );
        var arr = Assert.IsType<JsonArray>(result);
        var first = Assert.IsType<JsonObject>(arr.Items[0]);
        var second = Assert.IsType<JsonObject>(arr.Items[1]);
        
        Assert.Equal("$[0]", first.Id);
        Assert.Equal("$[0].id", first.GetProperty("id").Id);
        Assert.Equal("$[0].name", first.GetProperty("name").Id);
        
        Assert.Equal("$[1]", second.Id);
        Assert.Equal("$[1].id", second.GetProperty("id").Id);
        Assert.Equal("$[1].name", second.GetProperty("name").Id);
    }

    [Fact]
    public void Parse_NestedArrays_ItemsHaveCorrectIds()
    {
        var result = JsonParser.Parse("[[], [1, 2], [3, 4, 5]]");
        var arr = Assert.IsType<JsonArray>(result);
        
        var inner1 = Assert.IsType<JsonArray>(arr.Items[0]);
        var inner2 = Assert.IsType<JsonArray>(arr.Items[1]);
        var inner3 = Assert.IsType<JsonArray>(arr.Items[2]);
        
        Assert.Equal("$[0]", inner1.Id);
        Assert.Equal("$[1]", inner2.Id);
        Assert.Equal("$[1][0]", inner2.Items[0].Id);
        Assert.Equal("$[1][1]", inner2.Items[1].Id);
        
        Assert.Equal("$[2]", inner3.Id);
        Assert.Equal("$[2][0]", inner3.Items[0].Id);
        Assert.Equal("$[2][1]", inner3.Items[1].Id);
        Assert.Equal("$[2][2]", inner3.Items[2].Id);
    }

    [Fact]
    public void Parse_ObjectWithArrayProperty_ItemsHaveCorrectIds()
    {
        var result = JsonParser.Parse("""{ "items": [1, 2, 3] }""");
        var obj = Assert.IsType<JsonObject>(result);
        var arr = Assert.IsType<JsonArray>(obj.GetProperty("items"));
        
        Assert.Equal("$.items", arr.Id);
        Assert.Equal("$.items[0]", arr.Items[0].Id);
        Assert.Equal("$.items[1]", arr.Items[1].Id);
        Assert.Equal("$.items[2]", arr.Items[2].Id);
    }

    [Fact]
    public void Parse_ArrayOfObjectsWithNestedArrays_AllIdsCorrect()
    {
        var result = JsonParser.Parse(
            """[ { "tags": ["a", "b"] }, { "tags": ["x", "y", "z"] } ]"""
        );
        var arr = Assert.IsType<JsonArray>(result);
        var obj1 = Assert.IsType<JsonObject>(arr.Items[0]);
        var tags1 = Assert.IsType<JsonArray>(obj1.GetProperty("tags"));
        var obj2 = Assert.IsType<JsonObject>(arr.Items[1]);
        var tags2 = Assert.IsType<JsonArray>(obj2.GetProperty("tags"));
        
        Assert.Equal("$[0]", obj1.Id);
        Assert.Equal("$[0].tags", tags1.Id);
        Assert.Equal("$[0].tags[0]", tags1.Items[0].Id);
        Assert.Equal("$[0].tags[1]", tags1.Items[1].Id);
        
        Assert.Equal("$[1]", obj2.Id);
        Assert.Equal("$[1].tags", tags2.Id);
        Assert.Equal("$[1].tags[0]", tags2.Items[0].Id);
        Assert.Equal("$[1].tags[1]", tags2.Items[1].Id);
        Assert.Equal("$[1].tags[2]", tags2.Items[2].Id);
    }

    [Fact]
    public void Parse_ComplexNestedStructure_AllIdsCorrect()
    {
        var result = JsonParser.Parse(
            """
            {
              "users": [
                {
                  "id": 1,
                  "name": "Alice",
                  "profile": {
                    "age": 30,
                    "hobbies": ["reading", "gaming"]
                  }
                },
                {
                  "id": 2,
                  "name": "Bob",
                  "profile": {
                    "age": 25,
                    "hobbies": ["sports"]
                  }
                }
              ]
            }
            """
        );
        var obj = Assert.IsType<JsonObject>(result);
        var users = Assert.IsType<JsonArray>(obj.GetProperty("users"));
        
        var user1 = Assert.IsType<JsonObject>(users.Items[0]);
        var profile1 = Assert.IsType<JsonObject>(user1.GetProperty("profile"));
        var hobbies1 = Assert.IsType<JsonArray>(profile1.GetProperty("hobbies"));
        
        Assert.Equal("$.users", users.Id);
        Assert.Equal("$.users[0]", user1.Id);
        Assert.Equal("$.users[0].id", user1.GetProperty("id").Id);
        Assert.Equal("$.users[0].name", user1.GetProperty("name").Id);
        Assert.Equal("$.users[0].profile", profile1.Id);
        Assert.Equal("$.users[0].profile.age", profile1.GetProperty("age").Id);
        Assert.Equal("$.users[0].profile.hobbies", hobbies1.Id);
        Assert.Equal("$.users[0].profile.hobbies[0]", hobbies1.Items[0].Id);
        Assert.Equal("$.users[0].profile.hobbies[1]", hobbies1.Items[1].Id);
        
        var user2 = Assert.IsType<JsonObject>(users.Items[1]);
        var profile2 = Assert.IsType<JsonObject>(user2.GetProperty("profile"));
        var hobbies2 = Assert.IsType<JsonArray>(profile2.GetProperty("hobbies"));
        
        Assert.Equal("$.users[1]", user2.Id);
        Assert.Equal("$.users[1].profile", profile2.Id);
        Assert.Equal("$.users[1].profile.hobbies", hobbies2.Id);
        Assert.Equal("$.users[1].profile.hobbies[0]", hobbies2.Items[0].Id);
    }

    [Fact]
    public void Parse_ObjectPropertyWithSpecialCharacters_IdHandlesCorrectly()
    {
        var result = JsonParser.Parse("""{ "user_name": "John", "email@domain": "test@example.com" }""");
        var obj = Assert.IsType<JsonObject>(result);
        
        Assert.Equal("$.user_name", obj.GetProperty("user_name").Id);
        Assert.Equal("$.email@domain", obj.GetProperty("email@domain").Id);
    }

    [Fact]
    public void Parse_EmptyArrayProperty_HasCorrectId()
    {
        var result = JsonParser.Parse("""{ "empty_list": [] }""");
        var obj = Assert.IsType<JsonObject>(result);
        var arr = Assert.IsType<JsonArray>(obj.GetProperty("empty_list"));
        
        Assert.Equal("$.empty_list", arr.Id);
    }

    [Fact]
    public void Parse_SingleElementArray_HasCorrectId()
    {
        var result = JsonParser.Parse("[42]");
        var arr = Assert.IsType<JsonArray>(result);
        
        Assert.Equal("$", arr.Id);
        Assert.Equal("$[0]", arr.Items[0].Id);
    }

    [Fact]
    public void Parse_AllValueTypes_HaveCorrectIds()
    {
        var result = JsonParser.Parse(
            """
            {
              "null_value": null,
              "bool_true": true,
              "bool_false": false,
              "number": 42.5,
              "string": "text",
              "object": {},
              "array": []
            }
            """
        );
        var obj = Assert.IsType<JsonObject>(result);
        
        Assert.Equal("$.null_value", obj.GetProperty("null_value").Id);
        Assert.Equal("$.bool_true", obj.GetProperty("bool_true").Id);
        Assert.Equal("$.bool_false", obj.GetProperty("bool_false").Id);
        Assert.Equal("$.number", obj.GetProperty("number").Id);
        Assert.Equal("$.string", obj.GetProperty("string").Id);
        Assert.Equal("$.object", obj.GetProperty("object").Id);
        Assert.Equal("$.array", obj.GetProperty("array").Id);
    }
}
