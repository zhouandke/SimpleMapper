using System;

namespace SimpleMapper.Sample
{
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

    class Program
    {
        static void Main(string[] args)
        {
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
        }
    }
}
