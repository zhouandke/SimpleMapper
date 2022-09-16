# SimpleMapper
1. 不需要提前注册映射关系
2. 同名不同类型的属性也会尽可能映射
3. PostAction 可以在完成基本映射后, 用代码实现特殊字段的映射

Sample class
```
public class Person
{
    public int Id { get; set; }

    public string Name { get; set; }
    // Same Name, different type
    public byte? Age { get; set; }
}

public class PersonDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int? Age { get; set; }

    public string Description { get; set; }
}
```

map
```
// register PostAction
ZK.Mapper.SimpleMapper.Default.SetPostAction<Person, PersonDto>((p, dto) =>
{
    if (dto == null)
    {
        return dto;
    }
    dto.Description = p.Age.HasValue ? $"{p.Name} age is {p.Age}" : $"{p.Name} age is unknown";
    return dto;
});

var p = new Person()
{
    Id = 1,
    Name = "john",
    Age = 32
};

var dto = ZK.Mapper.SimpleMapper.Default.Map<PersonDto>(p);
Console.WriteLine(dto.Description);
```
